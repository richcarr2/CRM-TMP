using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MCS.ApplicationInsights;
using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.Cerner;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentUpdateVistaHealthSharePostStageRunner : AILogicBase
    {
        public AppointmentUpdateVistaHealthSharePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "mcs_appointmentplugin";

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        public bool Success;

        public override void ExecuteLogic()
        {

            LogEntry();

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            //Logger.WriteDebugMessage($"{classAndMethod}: BEGIN Get reserve resource with id = {PrimaryEntity.Id}");
            Trace($"{classAndMethod}: BEGIN Get reserve resource with id = {PrimaryEntity.Id}", LogLevel.Debug);

            var appointment = OrganizationService.Retrieve(DataModel.Appointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.Appointment>();

            //Logger.WriteDebugMessage($"{classAndMethod}: END Got reserve resource with id = {appointment.Id}");
            Trace($"{classAndMethod}: END Got reserve resource with id = {appointment.Id}", LogLevel.Debug);

            if (!string.IsNullOrEmpty(appointment?.Category) && appointment?.Category == "Cerner")
            {
                //Logger.WriteDebugMessage($"{classAndMethod}: DEBUG: This reserve resource was sourced from Cerner's system (category = Cerner). There will be no service appointment attached to this reserve resource. Ending processing.");
                Trace("{classAndMethod}: DEBUG: This reserve resource was sourced from Cerner's system (category = Cerner). There will be no service appointment attached to this reserve resource. Ending processing.", LogLevel.Debug);
                Success = true;
                LogExit(1);
                return;
            }

            //Logger.WriteDebugMessage($"{classAndMethod}: BEGIN Get service appointment with id = {appointment.cvt_serviceactivityid.Id}");
            Trace($"{classAndMethod}: BEGIN Get service appointment with id = {appointment.cvt_serviceactivityid.Id}", LogLevel.Debug);

            var sa = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, appointment.cvt_serviceactivityid.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();

            //Logger.WriteDebugMessage($"{classAndMethod}: END Get service appointment with id = {appointment.cvt_serviceactivityid.Id}");
            Trace($"{classAndMethod}: END Get service appointment with id = {appointment.cvt_serviceactivityid.Id}", LogLevel.Debug);

            bool runIntegration;

            using (var srv = new Xrm(OrganizationService))
            {
                //Logger.WriteDebugMessage($"{classAndMethod}: Checking facility type!");
                Trace($"{classAndMethod}: Checking facility type!", LogLevel.Debug);

                bool VistaTypeFacility = false;

                if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                {
                    VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                    //Logger.WriteDebugMessage($"{classAndMethod}: DEBUG:isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                    Trace($"{classAndMethod}: DEBUG:isVistaFacility exists in shared variables; set to: {VistaTypeFacility}", LogLevel.Debug);
                }
                else
                {
                    VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(sa, srv, Logger, appointment);
                    PluginExecutionContext.SharedVariables.Add("isVistaFacility", VistaTypeFacility);
                }
                runIntegration = VistaPluginHelpers.RunVistaIntegration(sa, srv, Logger, PluginExecutionContext);

                if (runIntegration) SendVistaIntegration(appointment, sa, VistaTypeFacility);
                else
                {
                    Success = true;
                    //Logger.WriteDebugMessage($"{classAndMethod}: Vista switch turned off");
                    Trace($"{classAndMethod}: Vista switch turned off.", LogLevel.Debug);
                }
            }

            LogExit(2);

        }

        public void SendVistaIntegration(DataModel.Appointment appointment, DataModel.ServiceAppointment serviceAppointment, bool VistaTypeFacility)
        {

            LogEntry();

            using (var context = new Xrm(OrganizationService))
            {
                ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "HsMakeCancelOutboundUri");
            }

            //Logger.WriteDebugMessage("Beginning VistA Book/Cancel");
            Trace("Beginning VistA Book/Cancel.", LogLevel.Debug);

            var changedPatientIds = VistaPluginHelpers.GetChangedPatients(appointment, OrganizationService, Logger, out var isBookRequest);

            // Use the version of the record that got passed in (so that the appointment is only considered canceled if the user actually hit the "Cancel Dialog" - aka the Integration Booking Status was changed to a "canceled state")
            var isWholeAppointmentCanceled = VistaPluginHelpers.FullAppointmentCanceled(PrimaryEntity.ToEntity<DataModel.Appointment>());

            
            if (isWholeAppointmentCanceled)
            {
                if (changedPatientIds != null && changedPatientIds.Any())
                {
                    using (var context = new Xrm(OrganizationService))
                    {
                        var vistaIntegrationResult = context.cvt_vistaintegrationresultSet.FirstOrDefault(x =>
                            x.cvt_Appointment.Id == appointment.Id &&
                            x.cvt_Veteran.Id == changedPatientIds.First());

                        if (vistaIntegrationResult == null) throw new InvalidPluginExecutionException("Cannot find the VistA Integration Result for Group Cancel");

                        SendReceiveVistaCancel(appointment, changedPatientIds, vistaIntegrationResult, VistaTypeFacility);

                    }
                }
            }
            else if (isBookRequest) SendReceiveVistaBook(appointment, changedPatientIds, VistaTypeFacility);
            else
            {
                //Logger.WriteDebugMessage("Patient removed from Block Resource as result of Individual Cancellation, no further action needed.  Ending plugin now.");
                Trace("Patient removed from Block Resource as result of Individual Cancellation, no further action needed.  Ending plugin now.", LogLevel.Debug);
                //throw new InvalidPluginExecutionException("Invalid Plugin Registration for Vista Integration");
                Success = true;
            }

            LogExit(1);

        }

        private void SendReceiveVistaBook(DataModel.Appointment appointment, List<Guid> addedPatients, bool VistaTypeFacility)
        {
            LogEntry();

            var failures = 0;
            var vistaStatus = -1;

            if (addedPatients != null && addedPatients.Any() && appointment.cvt_serviceactivityid != null)
            {
                var request = new TmpHealthShareMakeCancelOutboundRequestMessage
                {
                    Patients = addedPatients,
                    ServiceAppointmentId = appointment.cvt_serviceactivityid.Id,
                    AppointmentId = appointment.Id,
                    OrganizationName = PluginExecutionContext.OrganizationName,
                    UserId = PluginExecutionContext.UserId,
                    LogRequest = true,
                    VisitStatus = VistaStatus.SCHEDULED.ToString()
                };

                //Logger.WriteDebugMessage("Set up HealthShareMakeCancelOutboundRequestMessage request object.");
                Trace("Set up HealthShareMakeCancelOutboundRequestMessage request object.", LogLevel.Debug);

                var vimtRequest = Serialization.DataContractSerialize(request);
                TmpHealthShareMakeCancelOutboundResponseMessage response = null;
                TmpCernerOutboundResponseMessage responseCerner = null;

                try
                {

                    if (VistaTypeFacility)
                    {
                        //Logger.WriteDebugMessage($"Sending HealthShare Make Group Appointment Request Message to VIMT: {vimtRequest}.");
                        Trace("Sending HealthShare Make Group Appointment Request Message to VIMT: {vimtRequest}.", LogLevel.Debug);

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

                        response = RestPoster.Post<TmpHealthShareMakeCancelOutboundRequestMessage, TmpHealthShareMakeCancelOutboundResponseMessage>(
                            "HealthShare Make Group Appointment", baseUrl, uri, request, resource, appId, secret, authority, tenantId, subscriptionId,
                            isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                        response.VimtLagMs = lag;

                        //Logger.WriteDebugMessage($"Finished Sending HealthShare Make Appointment Request Message to VIMT for Appointment with Id: {PrimaryEntity.Id}");
                        Trace($"Finished Sending HealthShare Make Appointment Request Message to VIMT for Appointment with Id: {PrimaryEntity.Id}.", LogLevel.Debug);

                        foreach (var patientIntegrationResultInformation in response.PatientIntegrationResultInformation)
                        {
                            var status = ProcessVistaMakeApptResponse(response, typeof(TmpHealthShareMakeCancelOutboundRequestMessage), typeof(TmpHealthShareMakeCancelOutboundResponseMessage),
                                patientIntegrationResultInformation);

                            if (status == (int)serviceappointment_statuscode.ReservedScheduled) continue;

                            failures++;
                            vistaStatus = status;
                        }
                    }
                    else
                    {
                        //Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control Passed to Logic App for Cerner Processing.");
                        Trace("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control Passed to Logic App for Cerner Processing.", LogLevel.Debug);

                        using (var context = new Xrm(OrganizationService))
                        {
                            responseCerner = CernerHelper.FireLogicApp(PrimaryEntity.ToEntityReference(), context, OrganizationService, Logger, addedPatients, "Book|ReserveResource|GroupAppointment", "ReserveResource");
                            vistaStatus = ProcessCernerMakeApptResponse(responseCerner, typeof(TmpCernerOutboundResponseMessage), typeof(TmpCernerOutboundResponseMessage));
                            if (responseCerner.ExceptionOccured) failures++;
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (VistaTypeFacility)
                    {
                        var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                        IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Make Vista Appointment", errorMessage, vimtRequest,
                            typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                            MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse,
                            response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                        //Logger.WriteToFile(errorMessage);
                        Trace(errorMessage, LogLevel.Error);
                        failures++;
                    }
                    else
                    {
                        var errorMessage = string.Format("Make Cerner appointment - error", ex);
                        IntegrationPluginHelpers.CreateIntegrationResultOnCernerFailure("Make Cerner Appointment", errorMessage, responseCerner?.RequestMessage,
                            typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                            MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, responseCerner?.RequestMessage, responseCerner?.ResponseMessage,
                            0, 0, responseCerner?.MessageProcessingTime);
                        //Logger.WriteToFile(errorMessage);
                        Trace(errorMessage, LogLevel.Error);
                        failures++;
                    }

                }
            }
            else
            {
                //Logger.WriteToFile("Either Added Patient is null/empty or NO Service Activity is associated");
                Trace("Either Added Patient is null/empty or NO Service Activity is associated.", LogLevel.Debug);
            }

            if (failures > 0)
            {
                Success = false;
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, PrimaryEntity.Id, (Appointmentcvt_IntegrationBookingStatus)vistaStatus);
            }
            else Success = true;

            LogExit(1);
        }

        private int ProcessVistaMakeApptResponse(TmpHealthShareMakeCancelOutboundResponseMessage response, Type requestType, Type responseType, PatientIntegrationResultInformation patientIntegrationResultInformation)
        {
            LogEntry();

            if (response == null)
            {
                LogExit(1);
                return (int)Appointmentcvt_IntegrationBookingStatus.VistaFailure;

            }

            var errorMessage = patientIntegrationResultInformation.ExceptionOccured ? patientIntegrationResultInformation.ExceptionMessage : string.Empty;

            IntegrationPluginHelpers.CreateAppointmentIntegrationResult("Group Book to Vista", patientIntegrationResultInformation.ExceptionOccured, errorMessage,
                patientIntegrationResultInformation.VimtRequest, response.SerializedInstance, patientIntegrationResultInformation.VimtResponse, requestType.FullName,
                responseType.FullName, MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, response.VimtLagMs,
                patientIntegrationResultInformation.EcProcessingMs, response.VimtProcessingMs, false, patientIntegrationResultInformation);

            var status = Appointmentcvt_IntegrationBookingStatus.ReservedScheduled;

            if (response.ExceptionOccured || patientIntegrationResultInformation.ExceptionOccured) status = Appointmentcvt_IntegrationBookingStatus.VistaFailure;

            LogExit(2);
            return (int)status;
        }

        private int ProcessCernerMakeApptResponse(TmpCernerOutboundResponseMessage response, Type requestType, Type responseType)
        {
            LogEntry();

            if (response == null) return (int)Appointmentcvt_IntegrationBookingStatus.VistaFailure;

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;

            IntegrationPluginHelpers.CreateCernerAppointmentIntegrationResult("Group Book to Cerner", response.ExceptionOccured, errorMessage,
                response.RequestMessage, response.RequestMessage, response.ResponseMessage, requestType.FullName,
                responseType.FullName, MessageRegistry.CernerOutboundResponseMessage, PrimaryEntity.Id, OrganizationService, 0, response.MessageProcessingTime, response.MessageProcessingTime, false, response.ControlId);

            var status = Appointmentcvt_IntegrationBookingStatus.ReservedScheduled;

            if (response.ExceptionOccured || response.ExceptionOccured) status = Appointmentcvt_IntegrationBookingStatus.VistaFailure;

            LogExit(1);
            return (int)status;
        }

        private void SendReceiveVistaCancel(DataModel.Appointment appointment, List<Guid> removedPatients, cvt_vistaintegrationresult vistaIntegrationResult, bool VistaTypeFacility)
        {
            LogEntry();

            var failures = 0;

            try
            {
                var appointmentRequestMessage = new TmpHealthShareMakeCancelOutboundRequestMessage
                {
                    AppointmentId = appointment.Id,
                    ServiceAppointmentId = appointment.cvt_serviceactivityid.Id,
                    Patients = removedPatients,
                    LogRequest = true,
                    OrganizationName = PluginExecutionContext.OrganizationName,
                    UserId = PluginExecutionContext.UserId,
                    VisitStatus = VistaStatus.CANCELED.ToString(),
                    VistaIntegrationResultId = vistaIntegrationResult.Id
                };


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

                if (VistaTypeFacility)
                {

                    //Logger.WriteDebugMessage("Sending to CancelGroup Request to VIMT");
                    Trace("Sending to CancelGroup Request to VIMT.", LogLevel.Debug);

                    var response = RestPoster.Post<TmpHealthShareMakeCancelOutboundRequestMessage, TmpHealthShareMakeCancelOutboundResponseMessage>(
                    "HealthShare Cancel Group Appointment", baseUrl, uri, appointmentRequestMessage, resource, appId, secret, authority, tenantId, subscriptionId,
                    isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);

                    if (response != null)
                    {
                        response.VimtLagMs = lag;
                        foreach (var patientIntegrationResultInformation in response.PatientIntegrationResultInformation)
                        {
                            var failed = ProcessVistaCancelResponse(response, typeof(TmpHealthShareMakeCancelOutboundRequestMessage), typeof(TmpHealthShareMakeCancelOutboundResponseMessage), appointment, true, patientIntegrationResultInformation);
                            if (failed) failures++;
                        }
                    }

                }
                else
                {
                    //Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control Passed to Logic App for Cerner Processing.");
                    Trace("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control Passed to Logic App for Cerner Processing.", LogLevel.Debug);

                    using (var context = new Xrm(OrganizationService))
                    {
                        var responseCerner = CernerHelper.FireLogicApp(PrimaryEntity.ToEntityReference(), context, OrganizationService, Logger, removedPatients, "Cancel|ReserveResource|GroupAppointment|WholeGroupCanceled", "ReserveResource");
                        var failed = ProcessCernerCancelResponse(responseCerner, typeof(TmpCernerOutboundResponseMessage), typeof(TmpCernerOutboundResponseMessage), appointment, true, responseCerner.ControlId);
                        if (failed) failures++;
                    }
                }

            }
            catch (Exception ex)
            {
                //Logger.WriteToFile($"Cancel Vista Appointment Failed on appt: {appointment.Subject}. Error Message: {CvtHelper.BuildExceptionMessage(ex)}");
                Trace($"Cancel Vista Appointment Failed on appt: {appointment.Subject}. Error Message: {CvtHelper.BuildExceptionMessage(ex)}.", LogLevel.Error);
                failures++;
            }

            if (failures > 0)
            {
                Success = false;
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, PrimaryEntity.Id, Appointmentcvt_IntegrationBookingStatus.CancelFailure);
            }
            else Success = true;

            LogExit(1);
        }

        private bool ProcessVistaCancelResponse(TmpHealthShareMakeCancelOutboundResponseMessage response, Type requestType, Type responseType, DataModel.Appointment appointment, bool wholeAppointmentCanceled, PatientIntegrationResultInformation patientIntegrationResultInformation)
        {
            LogEntry();

            if (response == null)
            {
                LogExit(1);
                return true;
            }

            var errorMessage = patientIntegrationResultInformation.ExceptionOccured ? patientIntegrationResultInformation.ExceptionMessage : string.Empty;

            IntegrationPluginHelpers.CreateAppointmentIntegrationResult("Group Cancel to Vista", patientIntegrationResultInformation.ExceptionOccured, errorMessage,
                patientIntegrationResultInformation.VimtRequest, response.SerializedInstance, patientIntegrationResultInformation.VimtResponse, requestType.FullName,
                responseType.FullName, MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, response.VimtLagMs,
                patientIntegrationResultInformation.EcProcessingMs, response.VimtProcessingMs, false, patientIntegrationResultInformation);

            var status = wholeAppointmentCanceled ? appointment.cvt_IntegrationBookingStatus.Value : (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled;

            if (response.ExceptionOccured || patientIntegrationResultInformation.ExceptionOccured) status = (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure;

            if (!wholeAppointmentCanceled)
            {
                //Logger.WriteDebugMessage($"Individual Cancellation, not updating entire appointment status to {(Appointmentcvt_IntegrationBookingStatus)status}");
                Trace($"Individual Cancellation, not updating entire appointment status to {(Appointmentcvt_IntegrationBookingStatus)status}.", LogLevel.Debug);
                LogExit(2);
                return false;
            }

            if (appointment.cvt_IntegrationBookingStatus.Value != status) IntegrationPluginHelpers.UpdateAppointment(OrganizationService, appointment.Id, (Appointmentcvt_IntegrationBookingStatus)status);
            else
            {
                //Logger.WriteDebugMessage("Appointment Booking Status has not changed, no need to update appointment on cancel");
                Trace("Appointment Booking Status has not changed, no need to update appointment on cancel.", LogLevel.Debug);
            }

            LogExit(3);
            return status == (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure;
        }

        private bool ProcessCernerCancelResponse(TmpCernerOutboundResponseMessage response, Type requestType, Type responseType, DataModel.Appointment appointment, bool wholeAppointmentCanceled, string controlId)
        {
            LogEntry();

            if (response == null)
            {
                LogExit(1);
                return true;
            }


            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;

            IntegrationPluginHelpers.CreateCernerAppointmentIntegrationResult("Group Cancel to Cerner", response.ExceptionOccured, errorMessage,
                response.RequestMessage, response.RequestMessage, response.ResponseMessage, requestType.FullName,
                responseType.FullName, MessageRegistry.CernerOutboundResponseMessage, PrimaryEntity.Id, OrganizationService, 0,
                0, response.MessageProcessingTime, false, controlId);

            var status = wholeAppointmentCanceled ? appointment.cvt_IntegrationBookingStatus.Value : (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled;

            if (response.ExceptionOccured || response.ExceptionOccured) status = (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure;

            if (!wholeAppointmentCanceled)
            {
                //Logger.WriteDebugMessage($"Individual Cancellation, not updating entire appointment status to {(Appointmentcvt_IntegrationBookingStatus)status}");
                Trace($"Individual Cancellation, not updating entire appointment status to {(Appointmentcvt_IntegrationBookingStatus)status}.", LogLevel.Debug);
                LogExit(2);
                return false;
            }

            if (appointment.cvt_IntegrationBookingStatus.Value != status) IntegrationPluginHelpers.UpdateAppointment(OrganizationService, appointment.Id, (Appointmentcvt_IntegrationBookingStatus)status);
            else
            {
                //Logger.WriteDebugMessage("Appointment Booking Status has not changed, no need to update appointment on cancel");
                Trace("Appointment Booking Status has not changed, no need to update appointment on cancel.", LogLevel.Debug);
            }

            LogExit(3);

            return status == (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure;
        }

        private void LogEntry([CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = this.GetType().Name;
                //Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");
                Trace("Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}.", LogLevel.Debug);
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
                Trace("Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}.", LogLevel.Debug);
            }
            catch (Exception)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }
    }
}
