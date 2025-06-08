using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    /// <summary>
    ///  CRM Plugin Runner class to handle updating a ServiceAppointment.
    /// </summary>
    public class ServiceAppointmentVvsUpdatePostStageRunner : PluginRunner
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public ServiceAppointmentVvsUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public ServiceAppointmentVvsUpdatePostStageRunner(IServiceProvider serviceProvider, Dictionary<Guid, bool> veterans) : base(serviceProvider)
        {
            Veterans = veterans;
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        public bool Success { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsCreate { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsUpdate { get; set; }

        public Dictionary<Guid, bool> Veterans { get; set; }

        /// <summary>
        /// Executes the plugin runner.
        /// </summary>
        public override void Execute()
        {
            Success = false;
            bool VistaTypeFacility = true;

            var sa = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();
            if (sa == null) throw new InvalidPluginExecutionException($"Unable to Find Service Appointment with id {PrimaryEntity.Id}");

            var isVVCAppointment = sa.cvt_Type ?? false;

            // Ensure this is NOT a Telephone Call VVC Service Appointment.
            if (sa.cvt_TelephoneCall.HasValue && sa.cvt_TelephoneCall.Value)
            {
                Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VVS integration");
                Success = true;
                return;
            }

            using (var context = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("Checking Facility Type!");
                VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(sa, context, Logger);

                if (VistaTypeFacility)
                {
                    Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");

                    if (VistaPluginHelpers.RunVvs(sa, context, Logger))
                    {
                        ApiIntegrationSettingsCreate = IntegrationPluginHelpers.GetApiSettings(context, "VvsCreateUri");
                        ApiIntegrationSettingsUpdate = IntegrationPluginHelpers.GetApiSettings(context, "VvsUpdateUri");

                        //if (sa.cvt_Type == null || !sa.cvt_Type.Value) return;
                        if (sa.StateCode == null || sa.StateCode != ServiceAppointmentState.Scheduled) throw new InvalidPluginExecutionException("Service Appointment is not in Scheduled State.");

                        if (sa.StatusCode == null
                            || sa.StatusCode.Value == (int)serviceappointment_statuscode.Pending
                            || sa.StatusCode.Value == (int)serviceappointment_statuscode.InterfaceVIMTFailure
                            || sa.StatusCode.Value == (int)serviceappointment_statuscode.ReservedScheduled)
                        {
                            SendVistaMessage(sa);
                        }
                        else
                            Logger.WriteDebugMessage("Service Activity not in Proper 'Pending' or 'Interface VIMT Failure' status for writing Vista Results back into TMP, skipping VVS");
                    }
                    else
                    {
                        Logger.WriteDebugMessage("VVS Integration Bypassed, not updating Service Activity Status");
                        Success = true;
                    }
                }
                else
                {
                    Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App becuse VVS is not applicable to Cerner.");
                    Success = true;
                }
            }
        }

        private void SendVistaMessage(DataModel.ServiceAppointment serviceAppointment)
        {
            // Default the success flag to false so that it is false until it successfully reaches a completed Vista Booking
            Success = false;
            List<Guid> changedPatients;

            Logger.WriteDebugMessage("Beginning VistA Book/Cancel");
            bool isBookRequest;

            if (Veterans != null)
            {
                Logger.WriteDebugMessage("Using Patients passed in from Proxy Add Plugin");
                isBookRequest = Veterans.Any(kvp => kvp.Value);
                changedPatients = Veterans.Where(kvp => kvp.Value == isBookRequest).Select(kvp => kvp.Key).ToList();
            }
            else
            {
                Logger.WriteDebugMessage("Using Vista Integration Results to determine Patient Delta");
                changedPatients = VistaPluginHelpers.GetChangedPatients(serviceAppointment, OrganizationService, Logger, out isBookRequest);
            }

            if (isBookRequest)
            {
                bool isUpdate;
                using (var srv = new Xrm(OrganizationService))
                {
                    isUpdate = srv.mcs_integrationresultSet.Where(
                        i => i.mcs_serviceappointmentid.Id == serviceAppointment.Id
                        && i.mcs_status.Value != (int)mcs_integrationresultmcs_status.Error).ToList()
                        .Any(i => i.mcs_VimtMessageRegistryName.ToLower().Contains("accenture"));
                }

                if (!isUpdate)
                {
                    var createRequest = new VideoVisitCreateRequestMessage
                    {
                        AppointmentId = serviceAppointment.Id,
                        LogRequest = true,
                        OrganizationName = PluginExecutionContext.OrganizationName,
                        UserId = PluginExecutionContext.UserId,
                        AddedPatients = changedPatients
                    };

                    Logger.WriteDebugMessage("Sending Create Booking to Vista");

                    var baseUrl = ApiIntegrationSettingsCreate.BaseUrl;
                    var uri = ApiIntegrationSettingsCreate.Uri;
                    var resource = ApiIntegrationSettingsCreate.Resource;
                    var appId = ApiIntegrationSettingsCreate.AppId;
                    var secret = ApiIntegrationSettingsCreate.Secret;
                    var authority = ApiIntegrationSettingsCreate.Authority;
                    var tenantId = ApiIntegrationSettingsCreate.TenantId;
                    var subscriptionId = ApiIntegrationSettingsCreate.SubscriptionId;
                    var isProdApi = ApiIntegrationSettingsCreate.IsProdApi;
                    var subscriptionIdEast = ApiIntegrationSettingsCreate.SubscriptionIdEast;
                    var subscriptionIdSouth = ApiIntegrationSettingsCreate.SubscriptionIdSouth;

                    var request = VA.TMP.Integration.Common.Serialization.DataContractSerialize(createRequest);
                    Logger.WriteDebugMessage($"Create request {request}");


                    var response = RestPoster.Post<VideoVisitCreateRequestMessage, VideoVisitCreateResponseMessage>("VVS Create", baseUrl, uri, createRequest, resource, appId, secret, authority,
                        tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag,Logger);
                    response.VimtLagMs = lag;

                    ProcessVistaCreateResponse(response, typeof(VideoVisitCreateRequestMessage), typeof(VideoVisitCreateResponseMessage), serviceAppointment);
                }
                else
                {
                    var updateRequest = new VideoVisitUpdateRequestMessage
                    {
                        AppointmentId = serviceAppointment.Id,
                        LogRequest = true,
                        OrganizationName = PluginExecutionContext.OrganizationName,
                        UserId = PluginExecutionContext.UserId,
                        Contacts = changedPatients
                    };

                    Logger.WriteDebugMessage("Sending Update Booking to Vista");

                    var baseUrl = ApiIntegrationSettingsUpdate.BaseUrl;
                    var uri = ApiIntegrationSettingsUpdate.Uri;
                    var resource = ApiIntegrationSettingsUpdate.Resource;
                    var appId = ApiIntegrationSettingsUpdate.AppId;
                    var secret = ApiIntegrationSettingsUpdate.Secret;
                    var authority = ApiIntegrationSettingsUpdate.Authority;
                    var tenantId = ApiIntegrationSettingsUpdate.TenantId;
                    var subscriptionId = ApiIntegrationSettingsUpdate.SubscriptionId;
                    var isProdApi = ApiIntegrationSettingsUpdate.IsProdApi;
                    var subscriptionIdEast = ApiIntegrationSettingsUpdate.SubscriptionIdEast;
                    var subscriptionIdSouth = ApiIntegrationSettingsUpdate.SubscriptionIdSouth;

                    var request= VA.TMP.Integration.Common.Serialization.DataContractSerialize(updateRequest);
                    Logger.WriteDebugMessage($"Update request {request}");

                    var response = RestPoster.Post<VideoVisitUpdateRequestMessage, VideoVisitUpdateResponseMessage>("VVS Update", baseUrl, uri, updateRequest, resource, appId, secret, authority,
                        tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag, Logger);
                    response.VimtLagMs = lag;

                    ProcessVistaUpdateResponse(response, typeof(VideoVisitUpdateRequestMessage), typeof(VideoVisitUpdateResponseMessage), serviceAppointment);
                }
            }
            else
            {
                var patIds = "";
                changedPatients.ForEach(p => patIds += p.ToString() + "; ");
                patIds.Trim().TrimEnd(';');
                Logger.WriteToFile("No Booked Patients.  Individual patient(s) was canceled through Cancel Dialog and has already been sent to VVS in previous plugin instance.  No action needed here, ending thread.  Canceled Patients: " + patIds);
            }
        }

        private void ProcessVistaBookResponse(bool isCreate, DataModel.ServiceAppointment serviceAppointment, string errorMessage, bool exceptionOccured, string vimtRequest, string serializedInstance, string vimtResponse, int EcProcessingTime, int vimtProcessingTime, int lagTime, WriteResults writeResults)
        {
            var name = isCreate ? "Create VVS" : "Update VVS";
            var reqType = isCreate ? typeof(VideoVisitCreateRequestMessage).FullName : typeof(VideoVisitUpdateRequestMessage).FullName;
            var respType = isCreate ? typeof(VideoVisitCreateResponseMessage).FullName : typeof(VideoVisitUpdateResponseMessage).FullName;
            var regName = isCreate ? MessageRegistry.VideoVisitCreateRequestMessage : MessageRegistry.VideoVisitUpdateRequestMessage;
            IntegrationPluginHelpers.CreateIntegrationResult(name, exceptionOccured, errorMessage, vimtRequest, serializedInstance, vimtResponse, reqType, respType, regName,
                serviceAppointment.Id, OrganizationService, lagTime, EcProcessingTime, vimtProcessingTime,null, false);

            if (exceptionOccured)
            {
                Logger.WriteDebugMessage("VVS Booking Failed, not updating Service Activity Status");
                return;
            }

            Success = !exceptionOccured;
        }

        private void ProcessVistaCreateResponse(VideoVisitCreateResponseMessage response, Type requestType, Type responseType, DataModel.ServiceAppointment appointment)
        {
            if (response == null) return;
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            ProcessVistaBookResponse(true, appointment, errorMessage, response.ExceptionOccured, response.VimtRequest, response.SerializedInstance, response.VimtResponse, response.EcProcessingMs, response.VimtProcessingMs, response.VimtLagMs, response.WriteResults);
        }

        private void ProcessVistaUpdateResponse(VideoVisitUpdateResponseMessage response, Type requestType, Type responseType, VA.TMP.DataModel.ServiceAppointment appointment)
        {
            if (response == null) return;
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            ProcessVistaBookResponse(false, appointment, errorMessage, response.ExceptionOccured, response.VimtRequest, response.SerializedInstance, response.VimtResponse, response.EcProcessingMs, response.VimtProcessingMs, response.VimtLagMs, response.WriteResults);
        }
    }
}
