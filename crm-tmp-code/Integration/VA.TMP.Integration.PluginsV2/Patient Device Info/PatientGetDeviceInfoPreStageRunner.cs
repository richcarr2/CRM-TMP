using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;
using VA.TMP.Integration.Common;

namespace VA.TMP.Integration.Plugins.Patient_DeviceInfo
{
    public class PatientGetDeviceInfoPreStageRunner : PluginRunner
    {
        public override string McsSettingsDebugField => "mcs_appointmentplugin";
        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }
        //private string PatientIcn = "1012901147";


        public PatientGetDeviceInfoPreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
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

        public override void Execute()
        {
            try
            {

                using (var srv = new Xrm(OrganizationService))
                {
                    Logger.WriteDebugMessage("Started Integration Settings");

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
                        Uri = settings.FirstOrDefault(x => x.Name == "VvsGetLoanedDevices").Value
                    };

                    Logger.WriteDebugMessage("Ending Integration Settings");

                    var identifier = srv.mcs_personidentifiersSet.FirstOrDefault(s => s.mcs_identifiertype == new Microsoft.Xrm.Sdk.OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI)
                  && s.mcs_patient != null && s.mcs_patient.Id == PrimaryEntity.Id && s.mcs_assigningauthority != null && s.mcs_assigningauthority == "USVHA");

                    if (identifier != null)
                    {
                        String patientIcn = identifier.mcs_identifier;
                        Logger.WriteDebugMessage(string.Format("Started Sending Device Info Request FOR  {0}", patientIcn));

                        var createRequest = new VideoVisitGetLoanedDevicesRequestMessage
                        {
                            ICN = patientIcn,
                            LogRequest = true,
                            OrganizationName = PluginExecutionContext.OrganizationName,
                            UserId = PluginExecutionContext.UserId
                        };
                        Logger.WriteDebugMessage("Sending Get Loaned Devices from VVS");

                        var response = RestPoster.Post<VideoVisitGetLoanedDevicesRequestMessage, VideoVisitGetLoanedDevicesResponseMessage>("Get VVS Loaned Devices", integrationSettings.BaseUrl, integrationSettings.Uri, createRequest, integrationSettings.Resource, integrationSettings.AppId, integrationSettings.Secret, integrationSettings.Authority,
                    integrationSettings.TenantId, integrationSettings.SubscriptionId, integrationSettings.IsProdApi, integrationSettings.SubscriptionIdEast, integrationSettings.SubscriptionIdSouth, out int lag, Logger);

                        Logger.WriteDebugMessage($"Response: {Serialization.DataContractSerialize(response)}");

                        //Filter Devices with the Name COMPUTER HARDWARE,TABLET TELEHEALTH then order by OrderedDateTime in descending order so the newest device is first in the list
                        response.Devices = response.Devices.Where(d => d.Attributes.DeviceName.ToUpper().Contains("COMPUTER HARDWARE") && d.Attributes.DeviceName.ToUpper().Contains("TABLET TELEHEALTH")).OrderByDescending(r => r.Attributes.OrderedDateTime).ToArray();

                        Logger.WriteDebugMessage($"Filtered Response: {Serialization.DataContractSerialize(response)}");

                        PluginExecutionContext.OutputParameters["outputJSON"] = Serialization.DataContractSerialize(response);
                    }

                    Logger.WriteDebugMessage("Finished Sending Device Info");

                }
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);

                Logger.WriteDebugMessage(errorMessage);

            }

        }

    }
}
