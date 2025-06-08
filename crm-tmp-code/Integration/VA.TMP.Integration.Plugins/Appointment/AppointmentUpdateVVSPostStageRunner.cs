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

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentUpdateVVSPostStageRunner : PluginRunner
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

        public override void Execute()
        {

            LogEntry();

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            bool callVvs;
            var appt = OrganizationService.Retrieve(DataModel.Appointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.Appointment>();

            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("Checking Facility Type!");

                if (!string.IsNullOrEmpty(appt?.Category) && appt?.Category == "Cerner")
                {
                    Logger.WriteDebugMessage($"{classAndMethod}: DEBUG: This reserve resource was sourced from Cerner's system (category = Cerner). There will be no service appointment attached to this reserve resource. Ending processing.");
                    Success = true;
                    LogExit(1);
                    return;
                }

                VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(appt, srv, Logger);

                if (VistaTypeFacility) //Vista facility. Proceed
                {
                    Logger.WriteDebugMessage("The Current Facility IS  a Vista Facility.  Continue Processing.");
                   
                    ApiIntegrationSettingsCreate = IntegrationPluginHelpers.GetApiSettings(srv, "VvsCreateUri");
                    ApiIntegrationSettingsUpdate = IntegrationPluginHelpers.GetApiSettings(srv, "VvsUpdateUri");
                    ApiIntegrationSettingsDelete = IntegrationPluginHelpers.GetApiSettings(srv, "VvsCancelUri");

                    var sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == appt.cvt_serviceactivityid.Id);
                    if (sa == null)
                    {
                        LogExit(1);
                        throw new InvalidPluginExecutionException("No Service Activity found for this appointment");
                    }
               
                    callVvs = VistaPluginHelpers.RunVvs(sa, srv, Logger);
                   
                    if (callVvs)
                    {
                        SendVistaMessage(appt, VistaTypeFacility);
                    }
                    else
                    {
                        Success = true;
                        Logger.WriteDebugMessage("VVS Integration Bypassed, Updating Service Activity Status");
                    }
                }
                else
                {
                    Success = true;
                    Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as VVS is not a concept applicable to Cerner integration.");
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

            Logger.WriteDebugMessage("Beginning VVS Book/Cancel");
            bool isBookRequest;

            if (VeteransDelta != null)
            {
                Logger.WriteDebugMessage("Using Patient passed in from Proxy Add Plugin");

                // If there are any veterans that were added, this is considered a book, otherwise view it as a cancel
                isBookRequest = VeteransDelta.Any(kvp => kvp.Value);
                changedPatients = VeteransDelta.Where(kvp => kvp.Value == isBookRequest).Select(kvp => kvp.Key).ToList();
            }
            else
            {
                Logger.WriteDebugMessage("Using Vista Integration Results to determine Patient Delta");
                changedPatients = VistaPluginHelpers.GetChangedPatients(appointment, OrganizationService, Logger, out isBookRequest);
            }

            if (isBookRequest)
            {
                bool isUpdate;

                using (var srv = new Xrm(OrganizationService))
                {
                    isUpdate = srv.mcs_integrationresultSet.Where(
                        i => i.mcs_appointmentid.Id == appointment.Id
                        && i.mcs_status.Value != (int)mcs_integrationresultmcs_status.Error).ToList()
                        .Any(i => i.mcs_VimtMessageRegistryName.ToLower().Contains("accenture"));
                }

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

                    Logger.WriteDebugMessage("Sending Create VVS");
                   
                    var response = RestPoster.Post<VideoVisitCreateRequestMessage, VideoVisitCreateResponseMessage>("VVS Create", baseUrl, uri, createRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                    ProcessVistaCreateResponse(response, typeof(VideoVisitCreateRequestMessage), typeof(VideoVisitCreateResponseMessage), appointment);
                   
                 
                }
                else
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

                    Logger.WriteDebugMessage("Sending Update VVS");

                    var response = RestPoster.Post<VideoVisitUpdateRequestMessage, VideoVisitUpdateResponseMessage>("VVS Update", baseUrl, uri, updateRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                    ProcessVistaUpdateResponse(response, typeof(VideoVisitUpdateRequestMessage), typeof(VideoVisitUpdateResponseMessage), appointment);

                }
            }
            else
            {
                var isWholeApptCanceled = VistaPluginHelpers.FullAppointmentCanceled(PrimaryEntity.ToEntity<DataModel.Appointment>());
                if (isWholeApptCanceled)
                {
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
                    Logger.WriteDebugMessage("Sending Cancel VVS");
                  
                    var response = RestPoster.Post<VideoVisitDeleteRequestMessage, VideoVisitDeleteResponseMessage>("VVS Delete", baseUrl, uri, cancelRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                }
                else
                {
                    Logger.WriteDebugMessage("Individual patient was canceled through Cancel Dialog and has already been sent to VVS in previous plugin instance.  No action needed here, ending thread.");
                }
            }

            LogExit(1);
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
                Logger.WriteToFile("Exception Occurred in Group Booking: " + errorMessage);
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
                Logger.WriteDebugMessage("Individual Cancellation, not updating entire appointment status to " + ((Appointmentcvt_IntegrationBookingStatus)status));
                LogExit(2);
                return;
            }
            if (appointment.cvt_IntegrationBookingStatus.Value != status)
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, appointment.Id, (Appointmentcvt_IntegrationBookingStatus)status);
            else
                Logger.WriteDebugMessage("Appointment Booking Status has not changed, no need to update appointment on cancel");

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
                Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");
            }
            catch (Exception e)
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
                Logger.WriteDebugMessage($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}");
            }
            catch (Exception e)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }

        #endregion
    }
}
