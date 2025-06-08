using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MCS.ApplicationInsights;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentUpdateVVSPostStageRunner : AILogicBase
    {

        public AppointmentUpdateVVSPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public AppointmentUpdateVVSPostStageRunner(IServiceProvider serviceProvider, Dictionary<Guid, bool> veteransDelta) : base(serviceProvider)
        {
            VeteransDelta = veteransDelta;
        }

        public override string McsSettingsDebugField => "mcs_appointmentplugin";

        private ApiIntegrationSettings ApiIntegrationSettingsCreate { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsUpdate { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsDelete { get; set; }

        public bool Success { get; internal set; }
        bool VistaTypeFacility = true;

        public Dictionary<Guid, bool> VeteransDelta;

        public override void ExecuteLogic()
        {

            LogEntry();
            //Logger.setDebug = true;
            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            bool callVvs;
            var appt = OrganizationService.Retrieve(DataModel.Appointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.Appointment>();

            using (var srv = new Xrm(OrganizationService))
            {
                //Logger.WriteDebugMessage("Checking Facility Type!");
                Trace("Checking Facility Type!", LogLevel.Debug);

                if (!string.IsNullOrEmpty(appt?.Category) && appt?.Category == "Cerner")
                {
                    //Logger.WriteDebugMessage($"{classAndMethod}: DEBUG: This reserve resource was sourced from Cerner's system (category = Cerner). There will be no service appointment attached to this reserve resource. Ending processing.");
                    Trace($"{classAndMethod}: DEBUG: This reserve resource was sourced from Cerner's system (category = Cerner). There will be no service appointment attached to this reserve resource. Ending processing.", LogLevel.Debug);
                    Success = true;
                    LogExit(1);
                    return;
                }
                if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                {
                    VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                    //Logger.WriteDebugMessage($"{classAndMethod}: DEBUG:isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                    Trace($"{classAndMethod}: DEBUG:isVistaFacility exists in shared variables; set to: {VistaTypeFacility}.", LogLevel.Debug);
                }
                else
                {
                    VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(appt, srv, pluginLogger);
                }

                //TMP/Cerner changes
                //check appointment modality
                var correctModalityForVVS = false;
                var sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == appt.cvt_serviceactivityid.Id);
                if (sa != null)
                {
                    var apptmodality = sa.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
                    switch (apptmodality.Value)
                    {
                        case 178970006:
                            correctModalityForVVS = true;
                            break;
                        default:
                            break;
                    }
                }
                if (VistaTypeFacility || correctModalityForVVS)
                {
                    //Logger.WriteDebugMessage("The Current Facility IS a Vista Facility, or correct Modality.  Continue Processing.");
                    Trace("The Current Facility IS a Vista Facility, or correct Modality.  Continue Processing.", LogLevel.Debug);

                    ApiIntegrationSettingsCreate = IntegrationPluginHelpers.GetApiSettings(srv, "VvsCreateUri");
                    ApiIntegrationSettingsUpdate = IntegrationPluginHelpers.GetApiSettings(srv, "VvsUpdateUri");
                    ApiIntegrationSettingsDelete = IntegrationPluginHelpers.GetApiSettings(srv, "VvsCancelUri");

                    sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == appt.cvt_serviceactivityid.Id);
                    if (sa == null)
                    {
                        LogExit(1);
                        throw new InvalidPluginExecutionException("No Service Activity found for this appointment");
                    }

                    callVvs = VistaPluginHelpers.RunVvs(sa, srv, pluginLogger, PluginExecutionContext);

                    if (callVvs)
                    {
                        SendVistaMessage(appt, VistaTypeFacility);
                    }
                    else
                    {
                        Success = true;
                        //Logger.WriteDebugMessage("VVS Integration Bypassed, Updating Service Activity Status");
                        Trace("VVS Integration Bypassed, Updating Service Activity Status.", LogLevel.Debug);
                    }
                }
                else
                {
                    Success = true;
                    //Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as VVS is not a concept applicable to Cerner integration.");
                    Trace("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as VVS is not a concept applicable to Cerner integration.", LogLevel.Debug);
                }
            }

            LogExit(2);
        }

        #region Vista Booking and Cancelation
        private void SendVistaMessage(DataModel.Appointment appointment, bool VistaTypeFacility)
        {
            LogEntry();

            // Default the success flag to false so that it is false until it successfully reaches a completed Vista Booking
            Success = false;
            List<Guid> changedPatients;

            //Logger.WriteDebugMessage("Beginning VVS Book/Cancel");
            Trace("Beginning VVS Book/Cancel.", LogLevel.Debug);
            bool isBookRequest;

            if (VeteransDelta != null)
            {
                //Logger.WriteDebugMessage("Using Patient passed in from Proxy Add Plugin");
                Trace("Using Patient passed in from Proxy Add Plugin.", LogLevel.Debug);

                // If there are any veterans that were added, this is considered a book, otherwise view it as a cancel
                isBookRequest = VeteransDelta.Any(kvp => kvp.Value);
                changedPatients = VeteransDelta.Where(kvp => kvp.Value == isBookRequest).Select(kvp => kvp.Key).ToList();
            }
            else
            {
                //Logger.WriteDebugMessage("Using Vista Integration Results to determine Patient Delta");
                Trace("Using Vista Integration Results to determine Patient Delta.", LogLevel.Debug);
                changedPatients = VistaPluginHelpers.GetChangedPatients(appointment, OrganizationService, pluginLogger, out isBookRequest);
            }

            //Logger.WriteDebugMessage("changedPatients:" + changedPatients.Count);
            Trace($"changedPatients: {changedPatients.Count}", LogLevel.Debug);
            //Logger.WriteDebugMessage("Going to check for update");
            Trace("Going to check for update", LogLevel.Debug);
            bool isUpdate;
            using (var srv = new Xrm(OrganizationService))
            {
                isUpdate = srv.mcs_integrationresultSet.Where(
                    i => i.mcs_appointmentid.Id == appointment.Id
                    && i.mcs_status.Value != (int)mcs_integrationresultmcs_status.Error && i.mcs_VimtMessageRegistryName != null).ToList()
                    .Any(i => i.mcs_VimtMessageRegistryName.ToLower().Contains("accenture"));
            }
            //Logger.WriteDebugMessage("Going to check for CancelPatients");
            Trace("Going to check for CancelPatients", LogLevel.Debug);

            var CancelPatients = GetcancelPatients(appointment, OrganizationService);

            //Logger.WriteDebugMessage("CancelPatients:" + CancelPatients);
            Trace($"CancelPatients: {CancelPatients}", LogLevel.Debug);

            if (isBookRequest)
            {
                //bool isUpdate;

                //using (var srv = new Xrm(OrganizationService))
                //{
                //    isUpdate = srv.mcs_integrationresultSet.Where(
                //        i => i.mcs_appointmentid.Id == appointment.Id
                //        && i.mcs_status.Value != (int)mcs_integrationresultmcs_status.Error).ToList()
                //        .Any(i => i.mcs_VimtMessageRegistryName.ToLower().Contains("accenture"));
                //}

                if (!isUpdate)
                {
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

                    var createRequest = new VideoVisitCreateRequestMessage
                    {
                        AppointmentId = appointment.Id,
                        LogRequest = true,
                        OrganizationName = PluginExecutionContext.OrganizationName,
                        UserId = PluginExecutionContext.UserId,
                        AddedPatients = changedPatients
                    };

                    //Logger.WriteDebugMessage("Sending Create VVS");
                    Trace("Sending Create VVS.", LogLevel.Debug);

                    var response = RestPoster.Post<VideoVisitCreateRequestMessage, VideoVisitCreateResponseMessage>("VVS Create", baseUrl, uri, createRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                    ProcessVistaCreateResponse(response, typeof(VideoVisitCreateRequestMessage), typeof(VideoVisitCreateResponseMessage), appointment);


                }
                else if (!CancelPatients && isUpdate)
                {
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

                    var updateRequest = new VideoVisitUpdateRequestMessage
                    {
                        AppointmentId = appointment.Id,
                        LogRequest = true,
                        OrganizationName = PluginExecutionContext.OrganizationName,
                        UserId = PluginExecutionContext.UserId,
                        Contacts = changedPatients
                    };

                    //Logger.WriteDebugMessage("Sending Update VVS");
                    Trace("Sending Update VVS.", LogLevel.Debug);

                    var response = RestPoster.Post<VideoVisitUpdateRequestMessage, VideoVisitUpdateResponseMessage>("VVS Update", baseUrl, uri, updateRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                    ProcessVistaUpdateResponse(response, typeof(VideoVisitUpdateRequestMessage), typeof(VideoVisitUpdateResponseMessage), appointment);

                }
            }
            else
            {
                ////The logic says if it isn't a bookrequest, then it has to be a cancel.  Then down in the building of the
                ////message to vista, it updated the cancelPatients list to whatever is in the changedPatients list.
                ////so I update the cancelpatients flag based on changedpatients.  In the event it is a book
                /////the cancelpatients function not returning correct values for a cancel
                //Logger.WriteDebugMessage("Not isBookRequest, updating cancel");
                Trace("Not isBookRequest, updating cancel.", LogLevel.Debug);

                if (changedPatients.Count > 0)
                {
                    CancelPatients = true;
                }
            }

            //Logger.WriteDebugMessage("CancelPatients " + CancelPatients);
            Trace($"CancelPatients {CancelPatients}", LogLevel.Debug);

            //Logger.WriteDebugMessage("isUpdateisUpdate " + isUpdate);
            Trace($"isUpdateisUpdate {isUpdate}", LogLevel.Debug);

            if (CancelPatients && isUpdate)
            {
                var isWholeApptCanceled = VistaPluginHelpers.FullAppointmentCanceled(PrimaryEntity.ToEntity<DataModel.Appointment>());
                if (isWholeApptCanceled)
                {
                    //Logger.WriteDebugMessage("isWholeApptCanceledisWholeApptCanceled " + isWholeApptCanceled);
                    Trace($"isWholeApptCanceledisWholeApptCanceled {isWholeApptCanceled}.", LogLevel.Debug);

                    var baseUrl = ApiIntegrationSettingsDelete.BaseUrl;
                    var uri = ApiIntegrationSettingsDelete.Uri;
                    var resource = ApiIntegrationSettingsDelete.Resource;
                    var appId = ApiIntegrationSettingsDelete.AppId;
                    var secret = ApiIntegrationSettingsDelete.Secret;
                    var authority = ApiIntegrationSettingsDelete.Authority;
                    var tenantId = ApiIntegrationSettingsDelete.TenantId;
                    var subscriptionId = ApiIntegrationSettingsDelete.SubscriptionId;
                    var isProdApi = ApiIntegrationSettingsDelete.IsProdApi;
                    var subscriptionIdEast = ApiIntegrationSettingsDelete.SubscriptionIdEast;
                    var subscriptionIdSouth = ApiIntegrationSettingsDelete.SubscriptionIdSouth;

                    var cancelRequest = new VideoVisitDeleteRequestMessage
                    {
                        AppointmentId = appointment.Id,
                        OrganizationName = PluginExecutionContext.OrganizationName,
                        UserId = PluginExecutionContext.UserId,
                        LogRequest = true,
                        CanceledPatients = changedPatients,
                        WholeAppointmentCanceled = true
                    };
                    //Logger.WriteDebugMessage("Sending Cancel VVS");
                    Trace("Sending Cancel VVS.", LogLevel.Debug);

                    var response = RestPoster.Post<VideoVisitDeleteRequestMessage, VideoVisitDeleteResponseMessage>("VVS Delete", baseUrl, uri, cancelRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;
                    //  ProcessVistaUpdateResponse(response, typeof(VideoVisitUpdateRequestMessage), typeof(VideoVisitUpdateResponseMessage), appointment);

                    ProcessVistaCancelResponse(response, typeof(VideoVisitDeleteRequestMessage), typeof(VideoVisitDeleteResponseMessage), appointment, isWholeApptCanceled);
                }
                else
                {
                    //Logger.WriteDebugMessage("Individual patient was canceled through Cancel Dialog and has already been sent to VVS in previous plugin instance.  No action needed here, ending thread.");
                    Trace("Individual patient was canceled through Cancel Dialog and has already been sent to VVS in previous plugin instance.  No action needed here, ending thread.", LogLevel.Debug);
                }
            }

            LogExit(1);
        }

        public bool GetcancelPatients(Entity currentRecord, IOrganizationService organizationService)//Naveen
        {

            List<Guid> currentPatients;

            switch (currentRecord.LogicalName)
            {
                case DataModel.Appointment.EntityLogicalName:
                    //Logger.WriteDebugMessage("Looking at appointment");
                    Trace("Looking at appointment", LogLevel.Debug);
                    currentPatients = currentRecord.ToEntity<DataModel.Appointment>().OptionalAttendees?.Select(ap => ap.PartyId.Id).ToList();
                    break;
                case DataModel.ServiceAppointment.EntityLogicalName:
                    //Logger.WriteDebugMessage("Looking at ServiceAppointment");
                    Trace("Looking at ServiceAppointment", LogLevel.Debug);
                    currentPatients = currentRecord.ToEntity<DataModel.ServiceAppointment>().Customers?.Select(ap => ap.PartyId.Id).ToList();
                    break;
                default:
                    throw new InvalidPluginExecutionException("Invalid Entity: Cannot retrieve Patient changes");
            }

            var previousPatients = GetPreviouslyBookedPatientCount(organizationService, currentRecord.ToEntityReference());
            var newPatients = currentPatients.Except(previousPatients).ToList();

            //Logger.WriteDebugMessage("previousPatients.Count   " + previousPatients.Count);
            Trace($"PreviousPatients.Count {previousPatients.Count}.", LogLevel.Debug);
            //Logger.WriteDebugMessage("newPatients.Count   " + newPatients.Count);
            Trace($"NewPatients.Count {newPatients.Count}.", LogLevel.Debug);


            if (previousPatients.Count == 0)
            {
                return true;
            }
            else { return false; }


        }

        public static List<Guid> GetPreviouslyBookedPatientCount(IOrganizationService OrganizationService, EntityReference bookingRecord)
        {
            List<Guid> previouslyBookedPatients = new List<Guid>();
            List<cvt_vistaintegrationresult> pastBookings;
            using (var srv = new Xrm(OrganizationService))
            {
                if (bookingRecord.LogicalName == DataModel.Appointment.EntityLogicalName)
                    pastBookings = srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_Appointment.Id == bookingRecord.Id && vir.cvt_VistAStatus != Common.VistaStatus.CANCELED.ToString() && vir.cvt_VistAStatus != Common.VistaStatus.FAILED_TO_SCHEDULE.ToString()).ToList();
                else if (bookingRecord.LogicalName == DataModel.ServiceAppointment.EntityLogicalName)
                    pastBookings = srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_ServiceActivity.Id == bookingRecord.Id && vir.cvt_VistAStatus != Common.VistaStatus.CANCELED.ToString() && vir.cvt_VistAStatus != Common.VistaStatus.FAILED_TO_SCHEDULE.ToString()).ToList();
                else
                    throw new InvalidPluginExecutionException("Invalid Entity Type: Unable to retrieve patient changes.");
               
                foreach (var book in pastBookings)
                {
                    var contactId = book.cvt_Veteran?.Id ?? IntegrationPluginHelpers.GetPatIdFromIcn(book.cvt_PersonId, OrganizationService);
                    //Only want distinct patients since WriteResults (aka vista integration results) will return 2 copies for a clinic based (1 for pat side and 1 for pro side with the same patient)
                    if (!previouslyBookedPatients.Contains(contactId))
                        previouslyBookedPatients.Add(contactId);
                }
            }

            return previouslyBookedPatients;
        }

        private void ProcessVistaBookResponse(bool isCreate, DataModel.Appointment appointment, string errorMessage, bool exceptionOccured, string vimtRequest, string serializedInstance, string vimtResponse, int vimtLag, int ecProcessingTime, int vimtProcessingTime, WriteResults writeResults)
        {
            LogEntry();

            var name = isCreate ? "Group VVS" : "Group Update to VVS";
            var reqType = isCreate ? typeof(VideoVisitCreateRequestMessage).FullName : typeof(VideoVisitUpdateRequestMessage).FullName;
            var respType = isCreate ? typeof(VideoVisitCreateResponseMessage).FullName : typeof(VideoVisitUpdateResponseMessage).FullName;
            var regName = isCreate ? MessageRegistry.VideoVisitCreateRequestMessage : MessageRegistry.VideoVisitUpdateRequestMessage;
            IntegrationPluginHelpers.CreateAppointmentIntegrationResult(name, exceptionOccured, errorMessage, vimtRequest, serializedInstance, vimtResponse, reqType, respType, regName, appointment.Id, OrganizationService, vimtLag, ecProcessingTime, vimtProcessingTime);

            if (exceptionOccured)
            {
                //Logger.WriteToFile("Exception Occurred in Group Booking: " + errorMessage);
                Trace($"Exception Occurred in Group Booking: {errorMessage}.", LogLevel.Debug);
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, PrimaryEntity.Id, Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure);
            }

            Success = !exceptionOccured;

            LogExit(1);
        }

        private void ProcessVistaCreateResponse(VideoVisitCreateResponseMessage response, Type requestType, Type responseType, DataModel.Appointment appointment)
        {
            LogEntry();

            if (response == null) return;
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            ProcessVistaBookResponse(true, appointment, errorMessage, response.ExceptionOccured, response.VimtRequest, response.SerializedInstance, response.VimtResponse, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, response.WriteResults);

            LogExit(1);

        }

        private void ProcessVistaUpdateResponse(VideoVisitUpdateResponseMessage response, Type requestType, Type responseType, VA.TMP.DataModel.Appointment appointment)
        {
            LogEntry();

            if (response == null) return;
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            ProcessVistaBookResponse(false, appointment, errorMessage, response.ExceptionOccured, response.VimtRequest, response.SerializedInstance, response.VimtResponse, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, response.WriteResults);

            LogExit(1);
        }

        private void ProcessVistaCancelResponse(VideoVisitDeleteResponseMessage response, Type requestType, Type responseType, VA.TMP.DataModel.Appointment appointment, bool wholeAppointmentCanceled)
        {
            LogEntry();

            if (response == null)
            {
                LogExit(1);
                return;
            }
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;

            var integrationResultId = IntegrationPluginHelpers.CreateAppointmentIntegrationResult("Group Cancel to VVS", response.ExceptionOccured, errorMessage, response.VimtRequest,
                response.SerializedInstance, response.VimtResponse, requestType.FullName, responseType.FullName,
                MessageRegistry.VideoVisitDeleteRequestMessage, PrimaryEntity.Id, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, false);

            var status = wholeAppointmentCanceled ? appointment.cvt_IntegrationBookingStatus.Value : (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled;

            if (response.ExceptionOccured) status = (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure;

            if (!wholeAppointmentCanceled)
            {
                //Logger.WriteDebugMessage("Individual Cancellation, not updating entire appointment status to " + ((Appointmentcvt_IntegrationBookingStatus)status));
                Trace($"Individual Cancellation, not updating entire appointment status to {((Appointmentcvt_IntegrationBookingStatus)status)}.", LogLevel.Debug);
                LogExit(2);
                return;
            }
            if (appointment.cvt_IntegrationBookingStatus.Value != status)
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, appointment.Id, (Appointmentcvt_IntegrationBookingStatus)status);
            else
            {
                //Logger.WriteDebugMessage("Appointment Booking Status has not changed, no need to update appointment on cancel");
                Trace("Appointment Booking Status has not changed, no need to update appointment on cancel.", LogLevel.Debug);
            }

            Success = status != (int)serviceappointment_statuscode.CancelFailure;

            LogExit(3);
        }


        private void LogEntry([CallerMemberName] string memberName = "",
                       [CallerFilePath] string sourceFilePath = "",
                       [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = this.GetType().Name;
                //Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");
                Trace($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}.", LogLevel.Debug);
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
                string className = this.GetType().Name;
                //Logger.WriteDebugMessage($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}");
                Trace($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}.", LogLevel.Debug);
            }
            catch (Exception)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }

        #endregion
    }
}
