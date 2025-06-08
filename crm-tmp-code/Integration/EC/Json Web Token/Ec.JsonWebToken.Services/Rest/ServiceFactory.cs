using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.EcExceptions;

namespace Ec.JsonWebToken.Services.Rest
{
    /// <summary>
    /// Service Factory.
    /// </summary>
    public class ServiceFactory : IServiceFactory
    {
        private const string CacheKey = "_JWT";

        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;

        private string BaseUri => _settings.Items.First(x => x.Key == "BaseUri").Value;

        private string EncryptUri => _settings.Items.First(x => x.Key == "EncryptUri").Value;

        private string RefererUri => _settings.Items.First(x => x.Key == "RefererUri").Value;

        private string MobileAuthUrl => _settings.Items.First(x => x.Key == "MobileAuthUrl").Value;

        private string MobileAuthIssuer => _settings.Items.First(x => x.Key == "MobileAuthIssuer").Value;

        private string Client_Private_Key => _settings.Items.First(x => x.Key == "Client_Private_Key").Value;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ServiceFactory(ILog logger, Settings settings, IKeyVaultCert keyVaultCert)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
        }

        /// <summary>
        /// Encrypt Json Web Token.
        /// </summary>
        /// <param name="payload">Json Web Token Payload.</param>
        /// <returns>Encrypted Json Web Token Token.</returns>
        public async Task<string> EncryptToken(JwtPayload payload)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var jwtToken = new JwtSecurityToken(new JwtHeader(GetSigningCredentials()), payload);
            var headerRemoved = jwtToken.Header.Remove("kid");
            _logger.Info($"KID Header removed = {headerRemoved}");

            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Referrer = new Uri(RefererUri);

                _logger.Info($"Posting to {BaseUri}{EncryptUri}");
                _logger.Info($"Payload: {token}");

                var response = await client.PostAsync(EncryptUri, new StringContent(token, Encoding.UTF8, "text/plain")).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error($"Failed to POST to JWT Service. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    throw new JwtPostException($"Failed to POST JWT Service. Reason : {response}, Reason: {response.ReasonPhrase}");
                }

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(result))
                {
                    _logger.Error("Failed to get Response Content from JWT Service because it was null or emapty");
                    throw new JwtPostException("Failed to get Response Content from JWT Service because it was null or emapty");
                }

                _logger.Info(string.Format("JWT POST Result: {0}", result));

                return result;
            }
        }

        /// <summary>
        /// Encrypt Json Web Token.
        /// </summary>
        /// <param name="notBefore">Effective date/time of the token.</param>
        /// <param name="expiration">Expiration date/time of the token</param>
        /// <returns>Encrypted Json Web Token Token.</returns>
        public async Task<string> EncryptToken(string notBefore, string expiration, string secId)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string token = "";
            var jwtHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Iss, MobileAuthIssuer),
                            new Claim(JwtRegisteredClaimNames.Sub, MobileAuthIssuer),
                            new Claim(JwtRegisteredClaimNames.Aud, $"{BaseUri}{MobileAuthUrl}"),
                            new Claim("role", "staff"),
                            new Claim("staff_id", secId),
                            new Claim("staff_id_type", "secid"),
                            new Claim(JwtRegisteredClaimNames.Iat, notBefore)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    SigningCredentials = new SigningCredentials(PrivateKeyFromPem(Client_Private_Key), SecurityAlgorithms.RsaSha512)
                };

                _logger.Debug("Generating the JWT Assertion");
                token = jwtHandler.WriteToken(jwtHandler.CreateJwtSecurityToken(tokenDescriptor));

                var formParameters = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                    { "client_id", MobileAuthIssuer },
                    { "client_assertion", token }
                };

                _logger.Debug("Formatting the request body");
                var requestBody = string.Join("&", formParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                _logger.Debug($"Request Body: {requestBody}");

                /*
                 *  Token Request Process Step 3: Construct the request object from the token URL and the request body
                 */
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseUri);
                    client.DefaultRequestHeaders.Referrer = new Uri(RefererUri);
                    client.DefaultRequestHeaders.Add("User-Agent", "Firefox");

                    _logger.Info($"Posting to {BaseUri}{MobileAuthUrl}");
                    _logger.Info($"Payload: {token}");

                    var response = await client.PostAsync(MobileAuthUrl, new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded")).ConfigureAwait(false);
                    //var response = client.SendAsync(mobileAuthRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        var resp = response.Content.ReadAsStringAsync().Result;
                        _logger.Error($"Failed to POST to JWT Service. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                        throw new JwtPostException($"Failed to POST JWT Service. Reason : {response}, Reason: {response.ReasonPhrase}");
                    }

                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (string.IsNullOrEmpty(result))
                    {
                        _logger.Error("Failed to get Response Content from JWT Service because it was null or emapty");
                        throw new JwtPostException("Failed to get Response Content from JWT Service because it was null or emapty");
                    }

                    _logger.Info(string.Format("JWT POST Result: {0}", result));

                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Post to Mobile Auth failed with error: {e}");
                throw;
            }
        }

        /// <summary>
        /// Get Signing Credentials.
        /// </summary>
        /// <returns>Signing Credentials.</returns>
        private SigningCredentials GetSigningCredentials()
        {
            var certificate = _keyVaultCert.GetKeyVaultCertificate(CacheKey).ConfigureAwait(false).GetAwaiter().GetResult();

            _logger.Info($"Using Cert - Friendly Name: {certificate.FriendlyName}, Subject: {certificate.Subject}, Alternatives: {certificate.SubjectName.Name}");

            var signingCredentials = new SigningCredentials(new X509SecurityKey(certificate), SecurityAlgorithms.RsaSha512);

            return signingCredentials;
        }

        private RsaSecurityKey PrivateKeyFromPem(string rsaPrivateKey)
        {
            var byteArray = Encoding.ASCII.GetBytes(rsaPrivateKey);
            using (var ms = new MemoryStream(byteArray))
            {
                using (var sr = new StreamReader(ms))
                {
                    var pemReader = new PemReader(sr);
                    var pem = pemReader.ReadPemObject();
                    var privateKey = PrivateKeyFactory.CreateKey(pem.Content);

                    return new RsaSecurityKey(DotNetUtilities.ToRSAParameters(privateKey as RsaPrivateCrtKeyParameters));
                }
            }
        }
    }
}
