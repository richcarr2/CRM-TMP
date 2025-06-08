using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Xrm.Sdk;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Patient_flag
{
    public class PatientGetPatientFlagPreStageRunner : PluginRunner
    {
        public override string McsSettingsDebugField => "mcs_appointmentplugin";
        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }
        //private string PatientIcn = "1012901147";


        public PatientGetPatientFlagPreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public override Entity GetPrimaryEntity()
        {
            if (PluginExecutionContext.InputParameters.Contains("Target"))
            {
                TracingService.Trace("Traget");

                if (PluginExecutionContext.InputParameters["Target"] is Entity)
                {
                    return (Entity)PluginExecutionContext.InputParameters["Target"];
                }
                else
                {
                    var PrimaryReference = (EntityReference)PluginExecutionContext.InputParameters["Target"];
                    return new Entity(PrimaryReference.LogicalName) { Id = PrimaryReference.Id };
                }
            }
            else if (PluginExecutionContext.InputParameters.Contains("EntityMoniker"))
            {
                var PrimaryReference = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];
                return new Entity(PrimaryReference.LogicalName) { Id = PrimaryReference.Id };
            }
            else
            {
                return new Entity(PluginExecutionContext.PrimaryEntityName);
            }

        }


        /*
        internal static HttpWebRequest CreateGetRequest(string url, string subscriptionKeys)
        {
            HttpWebRequest apiRequest = (HttpWebRequest)WebRequest.Create(url);
            if (subscriptionKeys.Length > 0)
            {
                string[] headers = subscriptionKeys.Split('|');
                for (int i = 0; i < headers.Length; i = i + 2)
                {
                    apiRequest.Headers.Add(headers[i], headers[i + 1]);
                }
            }
            apiRequest.Method = "GET";
            return apiRequest;
        }
        */
       
        private void GetIntegrationSettings()
        {

            using (var srv = new Xrm(OrganizationService))
            {
                TracingService.Trace("Started Integration Settings");


                var settings = srv.mcs_integrationsettingSet.Select(x => new ApiIntegrationSettingsNameValuePair { Name = x.mcs_name, Value = x.mcs_value }).ToList();

                var integrationSettings = new ApiIntegrationSettings
                {
                    Resource = settings.FirstOrDefault(x => x.Name == "Resource").Value,
                    AppId = settings.FirstOrDefault(x => x.Name == "AppId").Value,
                    Secret = settings.FirstOrDefault(x => x.Name == "Secret").Value,
                    Authority = settings.FirstOrDefault(x => x.Name == "Authority").Value,
                    TenantId = settings.FirstOrDefault(x => x.Name == "TenantId").Value,
                    SubscriptionId = settings.FirstOrDefault(x => x.Name == "SubscriptionId").Value,
                    IsProdApi = Convert.ToBoolean(settings.FirstOrDefault(x => x.Name == "IsProdApi").Value),
                    SubscriptionIdEast = settings.FirstOrDefault(x => x.Name == "SubscriptionIdEast").Value,
                    SubscriptionIdSouth = settings.FirstOrDefault(x => x.Name == "SubscriptionIdSouth").Value,
                    BaseUrl = settings.FirstOrDefault(x => x.Name == "BaseUrl").Value,
                    Uri = settings.FirstOrDefault(x => x.Name == "PatientFlagsUrl").Value
                };

                ApiIntegrationSettings = integrationSettings;

                TracingService.Trace("Ending Integration Settings");
            }
        }

        public override void Execute()
        {
            try
            {
                GetIntegrationSettings();

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

                using (var srv = new Xrm(OrganizationService))
                {
                    var identifier = srv.mcs_personidentifiersSet.FirstOrDefault(s => s.mcs_identifiertype == new Microsoft.Xrm.Sdk.OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI)
                  && s.mcs_patient != null && s.mcs_patient.Id == PrimaryEntity.Id && s.mcs_assigningauthority != null && s.mcs_assigningauthority == "USVHA");

                    if (identifier != null)
                    {
                        String patientIcn = identifier.mcs_identifier.Substring(0,10);
                        TracingService.Trace(string.Format("Started Sending Patient Flag Request FOR  {0}", patientIcn));
                        using (var client = new HttpClient())
                        {
                            TracingService.Trace($" Identifier {patientIcn}");

                            var appuri = String.Format("{0}{1}{2}", baseUrl, uri, patientIcn);

                            client.BaseAddress = new Uri(baseUrl);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionId);
                            client.DefaultRequestHeaders.ConnectionClose = true;

                            var response = client.GetAsync(appuri).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (response.IsSuccessStatusCode)
                            {
                                var stringResponse = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                TracingService.Trace($" REsponse {stringResponse}");
                                PluginExecutionContext.OutputParameters["outputJSON"] = stringResponse;
                            }
                        }
                    }

                    TracingService.Trace(string.Format("Finished Sending Patient Flsg "));

                }
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
               
                TracingService.Trace(errorMessage);
               
            }

        
        }

       
    }
}
