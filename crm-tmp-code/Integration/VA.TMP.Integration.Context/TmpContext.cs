using System;
using System.Linq;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.Context
{
    public class TmpContext : ITmpContext
    {
        private const string CacheKey = "TMP_CRM_TOKEN";

        private readonly Settings _settings;
        private readonly ITokenService _tokenService;

        private string CrmBaseOrgUri => _settings.Items.First(x => x.Key == "CrmBaseOrgUri").Value;

        private string CrmXrmUri => _settings.Items.First(x => x.Key == "CrmXrmUri").Value;

        public TmpContext(ITokenService tokenService, Settings settings)
        {
            _tokenService = tokenService;
            _settings = settings;
        }

        public OrganizationWebProxyClient GetOrganizationServiceProxy()
        {
            var token = _tokenService.GetCrmToken(CacheKey).ConfigureAwait(false).GetAwaiter().GetResult();

            var client = new OrganizationWebProxyClient(new Uri(string.Format(CrmXrmUri, CrmBaseOrgUri)), typeof(Account).Assembly) { HeaderToken = token };

            return client;
        }
    }
}