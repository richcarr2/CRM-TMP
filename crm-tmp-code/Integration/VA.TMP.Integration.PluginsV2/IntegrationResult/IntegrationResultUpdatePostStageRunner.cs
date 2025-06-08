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

            using (var svc = new Xrm(OrganizationService))
            {
                var priImage = svc.mcs_integrationresultSet.First(ir => ir.Id == PrimaryEntity.Id);
                var preImage = (GetSecondaryEntity()?.ToEntity<DataModel.mcs_integrationresult>() ?? null);

                Logger.WriteDebugMessage($"Integration Result Name: {priImage.mcs_name}");

                if (priImage.mcs_status.Value == (int)mcs_integrationresultmcs_status.Complete && !string.IsNullOrEmpty(priImage.mcs_name) && !priImage.mcs_name.Contains("Cancel Individual"))
                {
                    var apptId = priImage.mcs_serviceappointmentid ?? preImage.mcs_serviceappointmentid;
                    var name = string.IsNullOrEmpty(priImage.mcs_name) ? preImage.mcs_name : priImage.mcs_name;
                    var reqMsgType = string.IsNullOrEmpty(priImage.mcs_VimtRequestMessageType) ? preImage.mcs_VimtRequestMessageType : priImage.mcs_VimtRequestMessageType;
                    OptionSetValue apptmodality = null;

                    //Check for Appointment and make sure the Request Message Type ends with "TmpHealthShareMakeCancelOutboundRequestMessage"
                    if (apptId != null) Logger.WriteDebugMessage($"Appointment Id: {apptId.Id}");
                    Logger.WriteDebugMessage($"Request Message Type: {reqMsgType}");

                    //tmp/CERNER - changing suppression logic to include some modalities with cerner integration.
                    //runVvs and CancelVvs will determine if they should fire
                    //if (apptId == null || !reqMsgType.EndsWith("TmpHealthShareMakeCancelOutboundRequestMessage")) return;
                    if (apptId == null || !reqMsgType.EndsWith("MakeCancelOutboundRequestMessage"))
                    {
                        Logger.WriteDebugMessage("wrong msgType, exiting");
                        return;
                    }


                    if (!string.IsNullOrEmpty(name))
                    {
                        Logger.WriteDebugMessage("Last Integration Result: "+ name);
                        
                        var svcAppt = svc.ServiceAppointmentSet.Where(sa => sa.Id == apptId.Id).FirstOrDefault().ToEntity<DataModel.ServiceAppointment>();
                        if (svcAppt.Contains("tmp_appointmentmodality"))
                        {
                            Logger.WriteDebugMessage("Getting Appointment Modality from SA");
                            apptmodality = svcAppt.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
                        }

                        Logger.WriteDebugMessage(apptmodality.Value.ToString());

                        //Check if Appointment Modality is not set to VVC Test Call
                        if (name.StartsWith("Make") || name.StartsWith("Update") || (name.StartsWith("TMP/Cerner Outbound - Book") && apptmodality.Value != 178970008))
                        {
                            Logger.WriteDebugMessage("Need to run RunVVS");
                            if (RunVvs(svcAppt)) Logger.WriteDebugMessage("Successfully completed booking integration pipeline.");
                        }
                        else if (!name.StartsWith("Cancel VVS") || (name.StartsWith("TMP/Cerner Outbound - Cancel") && apptmodality.Value != 178970008))
                        {
                            Logger.WriteDebugMessage("Need to run RunCancelVvs");
                            if (RunCancelVvs(svcAppt, priImage)) Logger.WriteDebugMessage("Successfully completed cancel integration pipeline.");
                        }
                        else
                        {
                            Logger.WriteDebugMessage("Skipping to run RunCancelVvs");
                        }


                    }
                    else Logger.WriteDebugMessage("Could not run VVS due to missing Name");
                }
            }

            LogExit(1);
        }

        private bool RunCancelVvs(DataModel.ServiceAppointment serviceAppointment, mcs_integrationresult priImage)
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

                    if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                    {
                        isVistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                        Logger.WriteDebugMessage($"INFO: isVistaFacility exists in shared variables; set to: " + isVistaTypeFacility.ToString());
                    }
                    else
                    {
                        isVistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(serviceAppointment, context, Logger);
                        PluginExecutionContext.SharedVariables.Add("isVistaFacility", isVistaTypeFacility);
                    }
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
                    if (!(!serviceAppointment.cvt_Type.Value && serviceAppointment.mcs_groupappointment.Value) && VistaPluginHelpers.RunVvs(serviceAppointment, context, Logger, PluginExecutionContext))
                    {

                        //TMP/Cerner changes
                        //check appointment modality
                        var correctModalityForVVS = false;
                        var apptmodality = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
                        switch (apptmodality.Value)
                        {
                            case 178970000:
                                correctModalityForVVS = true;
                                break;
                            case 178970001:
                                correctModalityForVVS = true;
                                break;
                            case 178970006:
                                correctModalityForVVS = true;
                                break;
                            default:
                                break;
                        }
                        if (isVistaTypeFacility || correctModalityForVVS)
                        {
                            var virs = IntegrationPluginHelpers.GetVistaIntegrationResultsForSA(OrganizationService, serviceAppointment.Id, Logger);
                            //Added OR clauses for VistA Integration Results created from Cerner Integration. When an SA is Cerner/VistA and is CANCELLED, the VIRs are CANCELLED before the Patient IR is COMPLETE.
                            var activeVirs = virs.Where(v => v.cvt_VistAStatus != "CANCELED" || v.cvt_name.StartsWith("Cerner Integration Result") || v.cvt_name.StartsWith("Vista Integration Result"));
                            Logger.WriteDebugMessage($"Active Vista Integration Results: {activeVirs.Count()}");
                            var results = virs.Count.Equals(1) ||
                                (virs.Count() - activeVirs.Count()).Equals(1) ||
                                ((serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality").Value == 178970000
                                || serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality").Value == 178970001)
                                    && virs.Count().Equals(activeVirs.Count()));


                            if (!results && priImage.mcs_name.StartsWith("Cancel Vista"))
                                results = true;

                            Logger.WriteDebugMessage($"IntegrationPluginHelpers: Fire Cancel VVS for Cerner/VistA SA: {results}");

                            //If there are fewer active VIRS than VIRS for the service appointment then Exit because the Cancel VVS was already performed
                            if (!results)
                            {
                                LogExit(4);
                                return Success;
                            }

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
                            Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility or correct modality. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as Canceling VVS is not applicable to Cerner integration.");
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

                if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                {
                    isisVistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                    Logger.WriteDebugMessage($"INFO: isVistaFacility exists in shared variables; set to: " + isisVistaTypeFacility.ToString());
                }
                else
                {
                    isisVistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(sa, context, Logger);
                    PluginExecutionContext.SharedVariables.Add("isVistaFacility", isisVistaTypeFacility);
                }

                //TMP/Cerner changes
                //check appointment modality
                var correctModalityForVVS = false;
                var apptmodality = sa.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
                switch (apptmodality.Value)
                {
                    case 178970000:
                        correctModalityForVVS = true;
                        break;
                    case 178970001:
                        correctModalityForVVS = true;
                        break;
                    default:
                        break;
                }
                if (isisVistaTypeFacility || correctModalityForVVS)
                {
                    Logger.WriteDebugMessage("The Current Facility IS a Vista Facility, or correct Modality.  Continue Processing.");

                    if (VistaPluginHelpers.RunVvs(sa, context, Logger, PluginExecutionContext))
                    {
                        ApiIntegrationSettingsCreate = IntegrationPluginHelpers.GetApiSettings(context, "VvsCreateUri");
                        ApiIntegrationSettingsUpdate = IntegrationPluginHelpers.GetApiSettings(context, "VvsUpdateUri");

                        //if (sa.StateCode == null || sa.StateCode != ServiceAppointmentState.Scheduled) throw new InvalidPluginExecutionException("Service Appointment is not in Scheduled State.");

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
                    Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility, or the correct Modality. Vista Processing halted; Control NOT Passed to Logic App becuse VVS is not applicable to Cerner.");
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

            bool isUpdate = false;
            Logger.WriteDebugMessage("Check for update");
            using (var srv = new Xrm(OrganizationService))
            {
                isUpdate = srv.mcs_integrationresultSet.Where(
                    i => i.mcs_serviceappointmentid.Id == serviceAppointment.Id
                    && i.mcs_status.Value != (int)mcs_integrationresultmcs_status.Error && i.mcs_VimtMessageRegistryName != null).ToList()
                    .Any(i => i.mcs_VimtMessageRegistryName.ToLower().Contains("accenture"));
            }
            Logger.WriteDebugMessage("Idupdate:" + isUpdate);

            if (!isUpdate)
            {
                Logger.WriteDebugMessage("Starting not update");
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
                Logger.WriteDebugMessage("Starting  update");
                changedPatients = serviceAppointment.Customers.Select(a => a.PartyId.Id).ToList();
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
