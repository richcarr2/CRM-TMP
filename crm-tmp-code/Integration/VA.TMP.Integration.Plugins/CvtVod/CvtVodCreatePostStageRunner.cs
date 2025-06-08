using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;

namespace VA.TMP.Integration.Plugins.CvtVod
{
    /// <summary>
    ///  CRM Plugin Runner class to handle updating a VOD.
    /// </summary>
    public class CvtVodCreatePostStageRunner : PluginRunner
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public CvtVodCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        /// <summary>
        /// Gets or sets the VIMT URL.
        /// </summary>
        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        /// <summary>
        /// Executes the plugin runner.
        /// </summary>
        public override void Execute()
        {
            try
            {
                var vodEntity = PrimaryEntity.ToEntity<cvt_vod>();
                using (var context = new Xrm(OrganizationService))
                {
                    // Retrieve the VOD.
                    var vod = context.cvt_vodSet.FirstOrDefault(x => x.Id == vodEntity.Id);
                    if (vod == null) throw new InvalidPluginExecutionException("VOD cannot be null.");

                    // Check the settings record to make sure we want to Call the Accenture & Vyopta Services
                    var settings = context.mcs_settingSet.FirstOrDefault(x => x.mcs_name == "Active Settings");
                    if (settings == null) throw new InvalidPluginExecutionException("Active Settings cannot be null.");

                    if (settings.cvt_accenturevyopta != null && settings.cvt_accenturevyopta.Value)
                    {
                        ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "VmrVodUri");

                        // Call Service to create the Virtual Meeting Room
                        Logger.WriteDebugMessage("Beginning CreateAndSendVirtualMeetingRoom");
                        var vodCreateResponseMessage = CreateAndSendVod(vod, PluginExecutionContext.InitiatingUserId, PluginExecutionContext.OrganizationName, context);
                        Logger.WriteDebugMessage("Finished CreateAndSendVirtualMeetingRoom");
                        if (vodCreateResponseMessage == null) return;

                        Logger.WriteDebugMessage("Beginning ProcessVodCreateResponseMessage");
                        ProcessVodCreateResponseMessage(vodCreateResponseMessage);
                        Logger.WriteDebugMessage("Finished ProcessVodCreateResponseMessage");
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(IntegrationPluginHelpers.BuildErrorMessage(ex));
                throw new InvalidPluginExecutionException($"ERROR in CvtVodCreatePostStageRunner: {IntegrationPluginHelpers.BuildErrorMessage(ex)}");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Logger.WriteToFile($"Full Error: {ex.ToString()}");
                Logger.WriteDebugMessage(IntegrationPluginHelpers.BuildErrorMessage(ex));
                throw;
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"Full Error: {ex.ToString()}");
                Logger.WriteToFile(IntegrationPluginHelpers.BuildErrorMessage(ex));
                throw;
            }
        }

        /// <summary>
        /// Create an instance of the Virtual Meeting Room request and send to VIMT.
        /// </summary>
        /// <returns>VmrOnDemandCreateResponseMessage.</returns>
        private VmrOnDemandCreateResponseMessage CreateAndSendVod(cvt_vod vod, Guid userId, string organizationName, Xrm context)
        {
            // Get the PatientId and ProviderId. Guard verbosely here against null references/objects.
            var patientEmail = vod.cvt_patientemail;
            if (patientEmail == null) throw new InvalidPluginExecutionException("VOD: Patient Email cannot be null.");

            var provider = vod.cvt_provider;
            if (provider == null) throw new InvalidPluginExecutionException("VOD: Provider cannot be null.");

            var providerId = provider.Id;

            var request = new VmrOnDemandCreateRequestMessage
            {
                LogRequest = true,
                UserId = userId,
                OrganizationName = organizationName,
                VideoOnDemandId = vod.Id,
                PatientId = vod.Id,
                ProviderId = providerId,
            };

            var vimtRequest = Serialization.DataContractSerialize(request);

            VmrOnDemandCreateResponseMessage response = null;
            try
            {
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

                Logger.WriteToFile("1-Before calling VOD pin generation method");

                // Removed integration code related to Vyopta server
                response = GetOnDemandVMRResponse(context, vod, userId);
                Logger.WriteToFile("2-After calling VOD pin generation method");

                //response.VimtLagMs = lag;

                Logger.WriteToFile("13-Set Lag time");

                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                IntegrationPluginHelpers.CreateVodIntegrationResultOnVimtFailure("Create On-Demand Virtual Meeting Room", errorMessage, vimtRequest,
                    typeof(VmrOnDemandCreateRequestMessage).FullName, typeof(VmrOnDemandCreateResponseMessage).FullName, MessageRegistry.VmrOnDemandCreateRequestMessage,
                    PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs);
                Logger.WriteToFile($" Error generating VMR pins in TMP: {IntegrationPluginHelpers.BuildErrorMessage(ex)}");

                return null;
            }
        }


        private VmrOnDemandCreateResponseMessage GetOnDemandVMRResponse(Xrm context, cvt_vod vod, Guid userId)
        {
            // var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, patientAP.PartyId.Id, new ColumnSet(true));
            var integrationSettings = context.mcs_integrationsettingSet.Select(x => new IntegrationSetting { Name = x.mcs_name, Value = x.mcs_value }).ToList();

            var VirtualMeetingRoomDigitLength = Convert.ToInt32(integrationSettings.First(x => x.Name == "Virtual Meeting Room Digit Length").Value);
            var VirtualMeetingRoomPrefix = integrationSettings.First(x => x.Name == "Virtual Meeting Room On Demand Prefix").Value;
            var VirtualMeetingRoomSuffix = integrationSettings.First(x => x.Name == "Virtual Meeting Room Suffix").Value;

            var meetingroom = VA.TMP.Integration.Common.RandomDigits.GetRandomDigitString(VirtualMeetingRoomDigitLength);
            var meetingroomWithPrefix = VirtualMeetingRoomPrefix + meetingroom;

            #region host & guest key

            var sharedKey = integrationSettings.First(x => x.Name == "VMRSharedKey").Value;

            string host_key = string.Format("{0}{1}", sharedKey, meetingroom);
            string guest_key = string.Format("{0}{1}", meetingroom, sharedKey);

            // it should retun integer/string pin numbers. we can get first 7 or 10 based on link type
            var host_pin = VA.TMP.Integration.Common.RandomDigits.SHA256HexHashString(host_key).Substring(0, 7) + "#"; // Provider
            var guest_pin = VA.TMP.Integration.Common.RandomDigits.SHA256HexHashString(guest_key).Substring(0, 10) + "#"; // Patient

            #endregion

            // Provider
            var ProviderVmrFormatUrl = integrationSettings.First(x => x.Name == "Provider VMR Format URL").Value;
            var PatientVmrFormatUrl = integrationSettings.First(x => x.Name == "Patient VMR Format URL").Value;
            var VmrBaseUrl = integrationSettings.First(x => x.Name == "VMR Base URL").Value;
            var VmrBaseUrlExtension = integrationSettings.First(x => x.Name == "VMR Base URL Extension").Value;

            var patientUrl = string.Format(PatientVmrFormatUrl, VmrBaseUrl, VmrBaseUrlExtension, meetingroomWithPrefix, VirtualMeetingRoomSuffix, guest_pin);




            var providerUrl = string.Format(ProviderVmrFormatUrl, VmrBaseUrl, VmrBaseUrlExtension, vod.cvt_provider.Id,
                    meetingroomWithPrefix, VirtualMeetingRoomSuffix, host_pin);

            //concatenate for miscData
            var miscData = "hostDialUrl=" + providerUrl + ";guestDialUrl=" + patientUrl;
            var dialingAlias = string.Format("{0}@{1}", meetingroom, VirtualMeetingRoomSuffix);

            VmrOnDemandCreateResponseMessage response = new VmrOnDemandCreateResponseMessage()
            {
                ProviderUrl = providerUrl,
                PatientUrl = patientUrl,
                MeetingRoomName = meetingroom,
                PatientPin = guest_pin,
                ProviderPin = host_pin,
                VimtLagMs = 0,
                MiscData = miscData,
                DialingAlias = dialingAlias
            };

            return response;

        }

        /// <summary>
        /// Update the VOD and create an Integration Result.
        /// </summary>
        /// <param name="vmrOnDemandCreateResponseMessage">Virtual Meeting Room Create Response Message.</param>
        private void ProcessVodCreateResponseMessage(VmrOnDemandCreateResponseMessage vmrOnDemandCreateResponseMessage)
        {
            var errorMessage = vmrOnDemandCreateResponseMessage.ExceptionOccured ? vmrOnDemandCreateResponseMessage.ExceptionMessage : null;
            IntegrationPluginHelpers.CreateVodIntegrationResult("Create On-Demand Virtual Meeting Room", vmrOnDemandCreateResponseMessage.ExceptionOccured, errorMessage,
                vmrOnDemandCreateResponseMessage.VimtRequest, vmrOnDemandCreateResponseMessage.SerializedInstance, vmrOnDemandCreateResponseMessage.VimtResponse,
                typeof(VmrOnDemandCreateRequestMessage).FullName, typeof(VmrOnDemandCreateResponseMessage).FullName, MessageRegistry.VmrOnDemandCreateRequestMessage,
                PrimaryEntity.Id, OrganizationService, vmrOnDemandCreateResponseMessage.VimtLagMs, vmrOnDemandCreateResponseMessage.EcProcessingMs, vmrOnDemandCreateResponseMessage.VimtProcessingMs, Logger);

            if (vmrOnDemandCreateResponseMessage.ExceptionOccured) return;

            var vod = new cvt_vod
            {
                Id = PrimaryEntity.Id,
                cvt_meetingroomname = vmrOnDemandCreateResponseMessage.MeetingRoomName,
                cvt_patienturl = vmrOnDemandCreateResponseMessage.PatientUrl,
                cvt_providerurl = vmrOnDemandCreateResponseMessage.ProviderUrl,
                cvt_patientpin = vmrOnDemandCreateResponseMessage.PatientPin,
                cvt_providerpin = vmrOnDemandCreateResponseMessage.ProviderPin,
                cvt_dialingalias = vmrOnDemandCreateResponseMessage.DialingAlias,
                cvt_miscdata = vmrOnDemandCreateResponseMessage.MiscData,
                statuscode = new OptionSetValue((int)OptionSets.cvt_vod_statuscode.Success)
            };
            OrganizationService.Update(vod);
            Logger.WriteDebugMessage("Updated VOD record.  Included statuscode change to Success.");
        }
    }
}
