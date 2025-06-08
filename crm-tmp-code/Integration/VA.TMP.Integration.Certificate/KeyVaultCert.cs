using System;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using VA.TMP.Integration.Certificate.Interface;

namespace VA.TMP.Integration.Certificate
{
    public class KeyVaultCert : IKeyVaultCert
    {
        private readonly MemoryCache _cache;
        private readonly string _certificateUrl;
        private readonly string _appId;
        private readonly string _secret;
        private DateTimeOffset _tokenExpiration;

        public KeyVaultCert(string certificateUrl, string appId, string secret)
        {
            _cache = MemoryCache.Default;
            _certificateUrl = certificateUrl;
            _appId = appId;
            _secret = secret;
        }

        public async Task<X509Certificate2> GetKeyVaultCertificate(string cacheKey)
        {
            if (_cache.Contains(cacheKey))
            {
                return (X509Certificate2)_cache[cacheKey];
            }

            var keyVaultClient = new KeyVaultClient(GetToken);

            if (string.IsNullOrEmpty(_certificateUrl)) throw new ArgumentNullException("_certificateUrl");

            var result = await keyVaultClient.GetSecretAsync(_certificateUrl).ConfigureAwait(false);

            if (result == null) throw new InvalidOperationException("Failed to obtain the certificate from Key Vault");

            var secret = Convert.FromBase64String(result.Value);
            var cacheEntry = new X509Certificate2(secret, (string)null);

            var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = _tokenExpiration.Subtract(new TimeSpan(0, 1, 0)) };
            MemoryCache.Default.Set(cacheKey, cacheEntry, cacheItemPolicy);

            return cacheEntry;
        }

        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(_appId, _secret);
            var result = await authContext.AcquireTokenAsync(resource, clientCred).ConfigureAwait(false);

            if (result == null) throw new InvalidOperationException("Failed to obtain the token to retrieve certificate");

            _tokenExpiration = result.ExpiresOn;

            return result.AccessToken;
        }
    }
}