using System;
using System.Activities;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.Vista;
using VA.TMP.Integration.Plugins.Helpers;

namespace VA.TMP.Integration.CustomActivities
{
    public class ViaLoginActionRunner : CustomActionRunner
    {
        public ViaLoginActionRunner(CodeActivityContext context) : base(context) { }

        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        public string StationNumber { get; set; }

        public string SamlToken { get; set; }

        private string AccessCode { get; set; }

        private string VerifyCode { get; set; }

        public bool SuccessfulLogin { get; private set; }

        public string ErrorMessage { get; private set; }

        public string UserDuz { get; private set; }

        public override void Execute(AttributeCollection attributes)
        {
            GetIntegrationSettings();
            GetPassedInVariables(attributes);
            var response = CallVimt();
            ProcessResponse(response);
        }

        private void GetPassedInVariables(AttributeCollection inputs)
        {
            StationNumber = inputs["StationNumber"].ToString();
            SamlToken = inputs["SamlToken"].ToString();
            AccessCode = inputs["AccessCode"]?.ToString() ?? "";
            VerifyCode = inputs["VerifyCode"]?.ToString() ?? "";
        }

        private void GetIntegrationSettings()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var settings = srv.mcs_integrationsettingSet.Select(x => new ApiIntegrationSettingsNameValuePair { Name = x.mcs_name, Value = x.mcs_value }).ToList();

                var integrationSettings = new ApiIntegrationSettings
                {
                    Resource = settings.FirstOrDefault(x => x.Name == "Resource").Value,
                    AppId = settings.FirstOrDefault(x => x.Name == "AppId").Value,
                    Secret = settings.FirstOrDefault(x => x.Name == "Secret").Value,
                    Authority = settings.FirstOrDefault(x => x.Name == "Authority").Value,
                    TenantId = settings.FirstOrDefault(x => x.Name == "TenantId").Value,
                    SubscriptionId = settings.FirstOrDefault(x => x.Name == "SubscriptionId").Value,
                    BaseUrl = settings.FirstOrDefault(x => x.Name == "BaseUrl").Value,
                    Uri = settings.FirstOrDefault(x => x.Name == "ViaLoginUri").Value,
                    IsProdApi = Convert.ToBoolean(settings.FirstOrDefault(x => x.Name == "IsProdApi").Value),
                    SubscriptionIdEast = settings.FirstOrDefault(x => x.Name == "SubscriptionIdEast").Value,
                    SubscriptionIdSouth = settings.FirstOrDefault(x => x.Name == "SubscriptionIdSouth").Value
                };

                ApiIntegrationSettings = integrationSettings;
            }
        }

        private ViaLoginResponseMessage CallVimt()
        {
            var request = new ViaLoginRequestMessage
            {
                SamlToken = SamlToken,
                StationNumber = StationNumber,
                AccessCode = AccessCode,
                VerifyCode = VerifyCode,
                UserId = WorkflowExecutionContext.UserId,
                OrganizationName = WorkflowExecutionContext.OrganizationName
            };
            try
            {
                Logger.WriteDebugMessage("Sending Login request to VIMT");

                var baseUrl = ApiIntegrationSettings.BaseUrl;
                var uri = ApiIntegrationSettings.Uri;
                var resource = ApiIntegrationSettings.Resource;
                var appId = ApiIntegrationSettings.AppId;
                var secret = ApiIntegrationSettings.Secret;
                var authority = ApiIntegrationSettings.Authority;
                var tenantId = ApiIntegrationSettings.TenantId;
                var subscriptionId = ApiIntegrationSettings.SubscriptionId;
                var isProdApi = ApiIntegrationSettings.IsProdApi;
                var subscriptionIdEast = ApiIntegrationSettings.SubscriptionIdEast;
                var subscriptionIdSouth = ApiIntegrationSettings.SubscriptionIdSouth;

                var response = RestPoster.Post<ViaLoginRequestMessage, ViaLoginResponseMessage>("VIA Login", baseUrl, uri, request, resource, appId, secret, authority, tenantId, subscriptionId,
                    isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                response.VimtLagMs = lag - response.VimtProcessingMs;

                Logger.WriteDebugMessage("Finished sending Login request to VIMT");

                var serializeRequest = VA.TMP.Integration.Common.Serialization.DataContractSerialize(response);
                Logger.WriteDebugMessage($"Login request: {serializeRequest}");

                if (response.ExceptionOccured)
                    response.ExceptionMessage = "Designated User Account (DUZ) was not found in your SAML token. Please check your access with your local ADPAC.";

                return response;
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Failed to Call VIMT");
                Logger.WriteDebugMessage("Login Exception: " + ex.Message);

                return new ViaLoginResponseMessage
                {
                    ExceptionOccured = true,
                    ExceptionMessage = ex.Message
                };
            }
        }

        private void ProcessResponse(ViaLoginResponseMessage response)
        {
            if (response == null) throw new Exception("No Response was returned from VIMT");
            var requestString = string.Format("VIMT Request: {0}\n|EC Request: {1}\n|VIMT Response {2}", response.VimtRequest, response.SerializedInstance, response.VimtResponse);
            Logger.WriteDebugMessage(requestString.Length > 20000 ? requestString.Substring(0, 20000) : requestString);
            SuccessfulLogin = !response.ExceptionOccured;
            ErrorMessage = response.ExceptionMessage;
            UserDuz = response.UserDuz;
            Logger.WriteDebugMessage("User Duz: " + UserDuz + "|Error Message: " + ErrorMessage + "|Success: " + SuccessfulLogin);
        }
    }
}
