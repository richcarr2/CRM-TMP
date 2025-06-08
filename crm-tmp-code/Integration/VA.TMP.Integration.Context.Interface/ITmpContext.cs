using Microsoft.Xrm.Sdk.WebServiceClient;

namespace VA.TMP.Integration.Context.Interface
{
    public interface ITmpContext
    {
        OrganizationWebProxyClient GetOrganizationServiceProxy();
    }
}