using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Ec.JsonWebToken.Messages;
using Ec.JsonWebToken.Messages.Messages;
using Ec.JsonWebToken.Services.Rest;
using log4net;
using Microsoft.IdentityModel.Tokens;
//using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.EcExceptions;

namespace Ec.JsonWebToken.Processors
{
    /// <summary>
    /// JWT Encrypt Processor.
    /// </summary>
    public class EcJwtEncryptProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public EcJwtEncryptProcessor(ILog logger, Settings settings, IKeyVaultCert keyVaultCert, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
            _serviceFactory = serviceFactory;
        }

        /// <summary>
        /// Execute Processor.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public EcJwtEncryptTokenResponse Execute(EcJwtEncryptTokenRequest request)
        {
            if (request == null) throw new JwtSamlTokenException("EcJwtEncryptTokenRequest message is null");
            if (string.IsNullOrEmpty(request.SamlToken)) throw new JwtSamlTokenException("SAML token is null or empty");

            var xdoc = XDocument.Parse(request.SamlToken);

            var secIdAttribute = xdoc.Descendants().FirstOrDefault(x => (string)x.Attribute("Name") == "urn:va:vrm:iam:secid");
            if (secIdAttribute == null) throw new JwtSamlTokenException("SecID attribute in SAML token cannot be missing or null");
            var secId = secIdAttribute.Value;

            var nbf = DateTime.UtcNow.AddSeconds(-1);
            var exp = DateTime.UtcNow.AddMinutes(5);

            var resp = _serviceFactory.EncryptToken(nbf.ToString(), exp.ToString(), secId).ConfigureAwait(false).GetAwaiter().GetResult();
            var mobileAuthResp = JsonSerializer.Deserialize<EcJwtMobileAuthResponse>(resp);
            return new EcJwtEncryptTokenResponse { EncryptedJwtToken = mobileAuthResp.AccessToken };
        }
    }
}
