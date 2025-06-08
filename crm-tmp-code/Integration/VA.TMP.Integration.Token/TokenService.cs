using System;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.Token
{
    public class TokenService : ITokenService
    {
        private readonly Settings _settings;
        private readonly MemoryCache _cache;

        private string AppId => _settings.Items.First(x => x.Key == "AppId").Value;

        private string Secret => _settings.Items.First(x => x.Key == "Secret").Value;

        private string Authority => _settings.Items.First(x => x.Key == "Authority").Value;

        private string Resource => _settings.Items.First(x => x.Key == "Resource").Value;

        private string CrmAppId => _settings.Items.First(x => x.Key == "CrmAppId").Value;

        private string CrmSecret => _settings.Items.First(x => x.Key == "CrmSecret").Value;

        private string CrmBaseOrgUri => _settings.Items.First(x => x.Key == "CrmBaseOrgUri").Value;

        public TokenService(Settings settings)
        {
            _settings = settings;
            _cache = MemoryCache.Default;
        }

        public async Task<string> GetToken(string cacheKey)
        {
            if (_cache.Contains(cacheKey))
            {
                return (string)_cache[cacheKey];
            }

            var authContext = new AuthenticationContext(Authority);
            var clientCred = new ClientCredential(AppId, Secret);
            var result = await authContext.AcquireTokenAsync(Resource, clientCred).ConfigureAwait(false);

            if (result == null) throw new InvalidOperationException("Failed to obtain the Azure token");

            var cacheEntry = result.AccessToken;

            var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = result.ExpiresOn.Subtract(new TimeSpan(0, 1, 0)) };
            _cache.Set(cacheKey, cacheEntry, cacheItemPolicy);

            return cacheEntry;
        }

        public async Task<string> GetCrmToken(string cacheKey)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (_cache.Contains(cacheKey))
            {
                return (string)_cache[cacheKey];
            }

            var authorityUrl = "https://login.microsoftonline.com/e95f1b23-abaf-45ee-821d-b7ab251ab3bf";
            var resourceUrl = $"https://{CrmBaseOrgUri}/";

            var authContext = new AuthenticationContext(authorityUrl, true, TokenCache.DefaultShared);
            var clientCredential = new ClientCredential(CrmAppId, CrmSecret);
            var result = await authContext.AcquireTokenAsync(resourceUrl, clientCredential).ConfigureAwait(false);

            if (result == null) throw new InvalidOperationException("Failed to obtain the Azure token for CRM");

            var cacheEntry = result.AccessToken;

            var cacheItemPolicy = new CacheItemPolicy { AbsoluteExpiration = result.ExpiresOn.Subtract(new TimeSpan(0, 1, 0)) };
            _cache.Set(cacheKey, cacheEntry, cacheItemPolicy);

            return cacheEntry;
        }
    }
}