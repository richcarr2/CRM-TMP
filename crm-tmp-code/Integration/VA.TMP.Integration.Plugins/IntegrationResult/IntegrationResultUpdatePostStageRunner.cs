using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.IntegrationResult
{
    public class IntegrationResultUpdatePostStageRunner : PluginRunner
    {
        public IntegrationResultUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override string McsSettingsDebugField => "mcs_integrationresultplugin";

        private ApiIntegrationSettings ApiIntegrationSettingsCreate { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsUpdate { get; set; }

        public Dictionary<Guid, bool> Veterans { get; set; }

        public bool Success { get; set; }

        public override void Execute()
        {
            LogEntry();

            var priImage = PrimaryEntity.ToEntity<DataModel.mcs_integrationresult>();
            var preImage = (GetSecondaryEntity()?.ToEntity<DataModel.mcs_integrationresult>() ?? null);

            if (priImage.mcs_status.Value == (int)mcs_integrationresultmcs_status.Complete)
            {
                var apptId = priImage.mcs_serviceappointmentid ?? preImage.mcs_serviceappointmentid;
                var name = string.IsNullOrEmpty(priImage.mcs_name) ? preImage.mcs_name : priImage.mcs_name;
                var reqMsgType = string.IsNullOrEmpty(priImage.mcs_VimtRequestMessageType) ? preImage.mcs_VimtRequestMessageType : priImage.mcs_VimtRequestMessageType;

                //Check for Appointment and make sure the Request Message Type ends with "TmpHealthShareMakeCancelOutboundRequestMessage"
                if (apptId != null) Logger.WriteDebugMessage($"Appointment Id: {apptId.Id}");
                Logger.WriteDebugMessage($"Request Message Type: {reqMsgType}");
                if (apptId == null || !reqMsgType.EndsWith("TmpHealthShareMakeCancelOutboundRequestMessage")) return;
                Logger.WriteDebugMessage($"Request Name: {name}");

                if (!string.IsNullOrEmpty(name))
                {
                    using (var svc = new Xrm(OrganizationService))
                    {
                        var svcAppt = svc.ServiceAppointmentSet.Where(sa => sa.Id == apptId.Id).FirstOrDefault().ToEntity<DataModel.ServiceAppointment>();
                        if (name.StartsWith("Make") || name.StartsWith("Update"))
                        {
                            if (RunVvs(svcAppt)) Logger.WriteDebugMessage("Successfully completed booking integration pipeline.");
                        }
                        else if (name.StartsWith("Cancel"))
                        {
                            if (RunCancelVvs(svcAppt)) Logger.WriteDebugMessage("Successfully completed cancel integration pipeline.");
                        }
                    }
                }
                else Logger.WriteDebugMessage("Could not run VVS due to missing Name");
            }

            LogExit(1);
        }

        private bool RunCancelVvs(DataModel.ServiceAppointment serviceAppointment)
        {
            LogEntry();

            bool isVistaTypeFacility;

            try
            {
                using (var context = new Xrm(OrganizationService))
                {
                    var settings = context.mcs_settingSet.FirstOrDefault(x => x.mcs_name == "Active Settings");

                    if (settings == null)
                    {
                        LogExit(1);
                        throw new InvalidPluginExecutionException("Active Settings Cannot be Null");
                    }

                    if (serviceAppointment == null) throw new InvalidPluginExecutionException("Service Appointment cannot be null.");

                    isVistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(serviceAppointment, context, Logger);

                    if (serviceAppointment.StateCode.Value != ServiceAppointmentState.Canceled)
                    {
                        Logger.WriteDebugMessage("Service Activity Not in Canceled status, exiting Cancel Integrations");
                        LogExit(2);
                        return Success;
                    }

                    // Ensure this is NOT a Telephone Call VVC Service Appointment.
                    if (serviceAppointment.cvt_TelephoneCall.HasValue && serviceAppointment.cvt_TelephoneCall.Value)
                    {
                        Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VVS cancel");
                        Success = true;
                        LogExit(3);
                        return Success;
                    }

                    // Call Service to cancel the Video Visit.
                    if (!(!serviceAppointment.cvt_Type.Value && serviceAppointment.mcs_groupappointment.Value) && VistaPluginHelpers.RunVvs(serviceAppointment, context, Logger))
                    {
                        if (isVistaTypeFacility)
                        {
                            var videoVisitDeleteResponseMessage = VistaPluginHelpers.CancelAndSendVideoVisitServiceSa(serviceAppointment, serviceAppointment.CreatedBy.Id, PluginExecutionContext.OrganizationName, OrganizationService, Logger);
                            if (videoVisitDeleteResponseMessage == null)
                            {
                                LogExit(4);
                                return Success;
                            }

                            ProcessVideoVisitDeleteResponseMessage(videoVisitDeleteResponseMessage, serviceAppointment);
                        }
                        else
                        {
                            Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as Canceling VVS is not applicable to Cerner integration.");
                        }

                    }
                    else Logger.WriteDebugMessage("Bypassed VVS Cancel. Either the VVS Switch is OFF or the SA is a CVT Group appointment (Cancellation are triggered from individual RRs in case of group)");

                    Success = true;
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(ex.Message);
                LogExit(5);
                throw new InvalidPluginExecutionException($"ERROR in ServiceAppointmentCancelPostStageRunner: {IntegrationPluginHelpers.BuildErrorMessage(ex)}");
            }
            catch (InvalidPluginExecutionException ex)
            {
                Logger.WriteDebugMessage(ex.Message);
                LogExit(6);
                throw;
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
                LogExit(7);
                throw;
            }

            LogExit(1);

            return Success;
        }

        private bool RunVvs(DataModel.ServiceAppointment sa)
        {
            LogEntry();

            bool isisVistaTypeFacility;

            if (sa == null) throw new InvalidPluginExecutionException($"Unable to Find Service Appointment with id {PrimaryEntity.Id}");

            // Ensure this is NOT a Telephone Call VVC Service Appointment.
            if (sa.cvt_TelephoneCall.HasValue && sa.cvt_TelephoneCall.Value)
            {
                Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VVS integration");
                Success = true;
                return Success;
            }

            using (var context = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("Checking Facility Type!");
                isisVistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(sa, context, Logger);

                if (isisVistaTypeFacility)
                {
                    Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");

                    if (VistaPluginHelpers.RunVvs(sa, context, Logger))
                    {
                        ApiIntegrationSettingsCreate = IntegrationPluginHelpers.GetApiSettings(context, "VvsCreateUri");
                        ApiIntegrationSettingsUpdate = IntegrationPluginHelpers.GetApiSettings(context, "VvsUpdateUri");

                        if (sa.StateCode == null || sa.StateCode != ServiceAppointmentState.Scheduled) throw new InvalidPluginExecutionException("Service Appointment is not in Scheduled State.");

                        if (sa.StatusCode == null
                            || sa.StatusCode.Value == (int)serviceappointment_statuscode.Pending
                            || sa.StatusCode.Value == (int)serviceappointment_statuscode.InterfaceVIMTFailure
                            || sa.StatusCode.Value == (int)serviceappointment_statuscode.ReservedScheduled)
                        {
                            SendVistaMessage(sa);
                            Success = true;
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

            LogExit(1);

            return Success;
        }

        private void SendVistaMessage(DataModel.ServiceAppointment serviceAppointment)
        {
            // Default the success flag to false so that it is false until it successfully reaches a completed Vista Booking
            List<Guid> changedPatients;

            Logger.WriteDebugMessage("Beginning VistA Book/Cancel");

            Logger.WriteDebugMessage("Using Vista Integration Results to determine Patient Delta");
            changedPatients = VistaPluginHelpers.GetChangedPatients(serviceAppointment, OrganizationService, Logger, out var isBookRequest);

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
                    UserId = serviceAppointment.CreatedBy.Id,
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
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag, Logger);
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
                    UserId = serviceAppointment.CreatedBy.Id,
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

                var request = VA.TMP.Integration.Common.Serialization.DataContractSerialize(updateRequest);
                Logger.WriteDebugMessage($"Update request {request}");

                var response = RestPoster.Post<VideoVisitUpdateRequestMessage, VideoVisitUpdateResponseMessage>("VVS Update", baseUrl, uri, updateRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag, Logger);
                response.VimtLagMs = lag;

                ProcessVistaUpdateResponse(response, typeof(VideoVisitUpdateRequestMessage), typeof(VideoVisitUpdateResponseMessage), serviceAppointment);
            }
        }

        private void ProcessVistaBookResponse(bool isCreate, DataModel.ServiceAppointment serviceAppointment, string errorMessage, bool exceptionOccured, string vimtRequest, string serializedInstance, string vimtResponse, int EcProcessingTime, int vimtProcessingTime, int lagTime, WriteResults writeResults)
        {
            var name = isCreate ? "Create VVS" : "Update VVS";
            var reqType = isCreate ? typeof(VideoVisitCreateRequestMessage).FullName : typeof(VideoVisitUpdateRequestMessage).FullName;
            var respType = isCreate ? typeof(VideoVisitCreateResponseMessage).FullName : typeof(VideoVisitUpdateResponseMessage).FullName;
            var regName = isCreate ? MessageRegistry.VideoVisitCreateRequestMessage : MessageRegistry.VideoVisitUpdateRequestMessage;
            IntegrationPluginHelpers.CreateIntegrationResult(name, exceptionOccured, errorMessage, vimtRequest, serializedInstance, vimtResponse, reqType, respType, regName,
                serviceAppointment.Id, OrganizationService, lagTime, EcProcessingTime, vimtProcessingTime, null, false);

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

        private void ProcessVideoVisitDeleteResponseMessage(VideoVisitDeleteResponseMessage videoVisitDeleteResponseMessage, DataModel.ServiceAppointment sa)
        {
            LogEntry();

            Logger.WriteDebugMessage("Processing VVS Cancel Response");
            var errorMessage = videoVisitDeleteResponseMessage.ExceptionOccured ? videoVisitDeleteResponseMessage.ExceptionMessage : string.Empty;
            IntegrationPluginHelpers.CreateIntegrationResult("Cancel VVS", videoVisitDeleteResponseMessage.ExceptionOccured, errorMessage,
                videoVisitDeleteResponseMessage.VimtRequest, videoVisitDeleteResponseMessage.SerializedInstance, videoVisitDeleteResponseMessage.VimtResponse,
                typeof(VideoVisitDeleteRequestMessage).FullName, typeof(VideoVisitDeleteResponseMessage).FullName, MessageRegistry.VideoVisitDeleteRequestMessage,
                sa.Id, OrganizationService, videoVisitDeleteResponseMessage.VimtLagMs, videoVisitDeleteResponseMessage.EcProcessingMs, videoVisitDeleteResponseMessage.VimtProcessingMs, null, false);

            Logger.WriteDebugMessage("Finished Processing VVS Cancel Response");

            LogExit(1);
        }

        private void ProcessVistaUpdateResponse(VideoVisitUpdateResponseMessage response, Type requestType, Type responseType, VA.TMP.DataModel.ServiceAppointment appointment)
        {
            if (response == null) return;
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            ProcessVistaBookResponse(false, appointment, errorMessage, response.ExceptionOccured, response.VimtRequest, response.SerializedInstance, response.VimtResponse, response.EcProcessingMs, response.VimtProcessingMs, response.VimtLagMs, response.WriteResults);
        }

        private void LogEntry([CallerMemberName] string memberName = "",
                            [CallerFilePath] string sourceFilePath = "",
                            [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                var className = GetType().Name;
                Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");

            }
            catch (Exception)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }

        private void LogExit(int exitPoint,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                var className = GetType().Name;
                Logger.WriteDebugMessage($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}");
            }
            catch (Exception)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }


    }
}
