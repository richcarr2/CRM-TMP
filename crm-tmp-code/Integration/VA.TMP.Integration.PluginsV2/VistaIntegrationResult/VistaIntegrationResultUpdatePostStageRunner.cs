using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.Cerner;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;

namespace VA.TMP.Integration.Plugins.VistaIntegrationResult
{
    public class VistaIntegrationResultUpdatePostStageRunner : PluginRunner
    {
        public VistaIntegrationResultUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private ApiIntegrationSettings ApiIntegrationSettingsHs { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsVvs { get; set; }

        public override string McsSettingsDebugField => "cvt_vistaintegrationresult";

        bool VistaTypeFacility = true;

        public override void Execute()
        {

            LogEntry();

            var vir = PrimaryEntity.ToEntity<cvt_vistaintegrationresult>();
            var reason = vir.cvt_CancelReason?.Value ?? -1;
            var primaryRecord = OrganizationService.Retrieve(cvt_vistaintegrationresult.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<cvt_vistaintegrationresult>();

            var VistaTypeFacility = true;

            if (reason != -1)
            {
                var apptId = primaryRecord.cvt_Appointment?.Id ?? Guid.Empty;
                var serviceApptId = primaryRecord.cvt_ServiceActivity?.Id ?? Guid.Empty;
                Logger.WriteDebugMessage("Step1 aaptID is :" + apptId);
                bool callVvs;
                bool callVista;
                var contactId = Guid.Empty;
                DataModel.ServiceAppointment sa;

                using (var srv = new Xrm(OrganizationService))
                {
                    if (primaryRecord.cvt_Veteran == null) throw new InvalidPluginExecutionException("No Veteran Listed on Vista Integration Result, stopping integration");

                    contactId = primaryRecord.cvt_Veteran.Id;

                    Logger.WriteDebugMessage($"Patient Id: {contactId}");

                    ApiIntegrationSettingsHs = IntegrationPluginHelpers.GetApiSettings(srv, "HsMakeCancelOutboundUri");
                    ApiIntegrationSettingsVvs = IntegrationPluginHelpers.GetApiSettings(srv, "VvsCancelUri");

                    var appt = srv.AppointmentSet.FirstOrDefault(a => a.Id == apptId);
                    //Logger.WriteDebugMessage("Step2 aaptID is :" + appt.Id);
                    var id = appt != null ? appt.cvt_serviceactivityid?.Id : serviceApptId;
                    //Logger.WriteDebugMessage("Step3 aaptID is :" + appt.Id);
                    Logger.WriteDebugMessage("Step4 id is :" + id);

                    sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == id);

                    Logger.WriteDebugMessage("Checking Facility Type!");
                    if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                    {
                        VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                        Logger.WriteDebugMessage($"INFO: isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                    }
                    else
                    {
                        VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(sa, srv, Logger);
                        PluginExecutionContext.SharedVariables.Add("isVistaFacility", VistaTypeFacility);
                    }

                    callVista = VistaPluginHelpers.RunVistaIntegration(sa, srv, Logger, PluginExecutionContext);
                    callVvs = VistaPluginHelpers.RunVvs(sa, srv, Logger, PluginExecutionContext);
                }

                bool success = false;

                if (callVista)
                {
                    if (VistaTypeFacility)
                    {
                        Logger.WriteDebugMessage("Step5 aaptID is :" + apptId);
                        var vistaResponse = SendCancelToVista(apptId, reason, sa, contactId);
                        success = ProcessCancelVistaResponse(vistaResponse, apptId, serviceApptId, contactId);
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Step6 aaptID is :" + apptId);
                        var cernerResponse = SendCancelToCerner(apptId, reason, sa, contactId);

                        if (!cernerResponse.ExceptionOccured)
                            success = true;

                        ProcessCancelCernerResponse(cernerResponse, apptId);
                    }

                }
                else
                {
                    Logger.WriteDebugMessage("Vista Bypassed, checking for running VVS");
                    success = true;
                }

                if (VistaTypeFacility)
                {
                    if (callVvs && success)
                    {
                        Logger.WriteDebugMessage("Step7 aaptID is :" + apptId);
                        var response = SendCancelToVvs(apptId, serviceApptId, contactId);
                        ProcessCancelVvsResponse(response, apptId, serviceApptId, contactId);
                    }
                    else
                    {
                        Logger.WriteDebugMessage($"VVS bypassed ({!callVvs}) or Vista failed ({!success}), so skipping VVS call");
                    }

                }

                //Logger.WriteDebugMessage(FinalizeCrmAppointment(apptId, serviceApptId, contactId)
                //    ? "Finished Cancel Individual Patient"
                //    : "Failed to Remove Patient from appointment");

            }
            else Logger.WriteDebugMessage("No Cancel Reason was set.  Can't cancel to Vista/VVS");

            LogExit(1);
        }

        private TmpHealthShareMakeCancelOutboundResponseMessage SendCancelToVista(Guid apptId, int cancelReason, DataModel.ServiceAppointment sa, Guid contactId)
        {

            LogEntry();

            Logger.WriteDebugMessage("Step8 aaptID is :" + apptId + " id is :" + sa.Id);

            //WholeAppointmentCanceled = false
            var cancelRequest = new TmpHealthShareMakeCancelOutboundRequestMessage
            {
                ServiceAppointmentId = sa.Id,
                AppointmentId = apptId,
                Patients = new List<Guid> { contactId },
                LogRequest = true,
                OrganizationName = PluginExecutionContext.OrganizationName,
                UserId = PluginExecutionContext.UserId,
                VistaIntegrationResultId = PrimaryEntity.Id,
                VisitStatus = Common.VistaStatus.CANCELED.ToString()
            };

            var vimtRequest = Serialization.DataContractSerialize(cancelRequest);
            TmpHealthShareMakeCancelOutboundResponseMessage response = null;
            try
            {
                Logger.WriteDebugMessage("Sending Cancel To VIMT");

                var baseUrl = ApiIntegrationSettingsHs.BaseUrl;
                var uri = ApiIntegrationSettingsHs.Uri;
                var resource = ApiIntegrationSettingsHs.Resource;
                var appId = ApiIntegrationSettingsHs.AppId;
                var secret = ApiIntegrationSettingsHs.Secret;
                var authority = ApiIntegrationSettingsHs.Authority;
                var tenantId = ApiIntegrationSettingsHs.TenantId;
                var subscriptionId = ApiIntegrationSettingsHs.SubscriptionId;
                var isProdApi = ApiIntegrationSettingsHs.IsProdApi;
                var subscriptionIdEast = ApiIntegrationSettingsHs.SubscriptionIdEast;
                var subscriptionIdSouth = ApiIntegrationSettingsHs.SubscriptionIdSouth;

                response = RestPoster.Post<TmpHealthShareMakeCancelOutboundRequestMessage, TmpHealthShareMakeCancelOutboundResponseMessage>(
                    "HealthShare MakeCancel Outbound Cancel", baseUrl, uri, cancelRequest, resource, appId, secret, authority, tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth,
                    out int lag, Logger);
                Logger.WriteDebugMessage($"Send Cancel To Vista Response: {Serialization.DataContractSerialize<TmpHealthShareMakeCancelOutboundResponseMessage>(response)}");
                response.VimtLagMs = lag;

                Logger.WriteDebugMessage("Completed Cancel Vista Pipeline");

                LogExit(1);
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                IntegrationPluginHelpers.CreateAppointmentIntegrationResultOnVimtFailure("Cancel Vista Appointment", errorMessage, vimtRequest, typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName, MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, apptId, OrganizationService, response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);

                Logger.WriteToFile(errorMessage);

                LogExit(2);
                return null;
            }


        }

        private TmpCernerOutboundResponseMessage SendCancelToCerner(Guid apptId, int cancelReason, DataModel.ServiceAppointment sa, Guid cancelContact)
        {
            LogEntry();

            bool VistaTypeFacility = true;

            using (var srv = new Xrm(OrganizationService))
            {
                LogExit(1);
                return CernerHelper.FireLogicApp(sa.ToEntityReference(), srv, OrganizationService, Logger, new List<Guid> { cancelContact }, "Cancel|ServiceAppointment|IndividualPatient");
            }

        }

        private VideoVisitDeleteResponseMessage SendCancelToVvs(Guid apptId, Guid serviceApptId, Guid contactId)
        {
            LogEntry();

            var cancelRequest = new VideoVisitDeleteRequestMessage
            {
                AppointmentId = apptId == Guid.Empty ? serviceApptId : apptId,
                OrganizationName = PluginExecutionContext.OrganizationName,
                UserId = PluginExecutionContext.UserId,
                LogRequest = true,
                CanceledPatients = new List<Guid> { contactId },
                WholeAppointmentCanceled = false
            };

            var vimtRequest = Serialization.DataContractSerialize(cancelRequest);
            VideoVisitDeleteResponseMessage response = null;
            try
            {
                Logger.WriteDebugMessage("Sending Delete VVS to VIMT");

                var baseUrl = ApiIntegrationSettingsVvs.BaseUrl;
                var uri = ApiIntegrationSettingsVvs.Uri;
                var resource = ApiIntegrationSettingsVvs.Resource;
                var appId = ApiIntegrationSettingsVvs.AppId;
                var secret = ApiIntegrationSettingsVvs.Secret;
                var authority = ApiIntegrationSettingsVvs.Authority;
                var tenantId = ApiIntegrationSettingsVvs.TenantId;
                var subscriptionId = ApiIntegrationSettingsVvs.SubscriptionId;
                var isProdApi = ApiIntegrationSettingsVvs.IsProdApi;
                var subscriptionIdEast = ApiIntegrationSettingsVvs.SubscriptionIdEast;
                var subscriptionIdSouth = ApiIntegrationSettingsVvs.SubscriptionIdSouth;

                response = RestPoster.Post<VideoVisitDeleteRequestMessage, VideoVisitDeleteResponseMessage>("VVS Cancel Cancel", baseUrl, uri, cancelRequest, resource, appId, secret, authority,
                    tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag, Logger);
                response.VimtLagMs = lag;

                Logger.WriteDebugMessage("VVS Delete Successfully sent to VIMT");

                LogExit(1);
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                IntegrationPluginHelpers.CreateAppointmentIntegrationResultOnVimtFailure("Delete Video Visit", errorMessage, vimtRequest, typeof(VideoVisitDeleteRequestMessage).FullName, typeof(VideoVisitDeleteResponseMessage).FullName, MessageRegistry.VideoVisitDeleteRequestMessage, apptId, OrganizationService, response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);

                Logger.WriteToFile(errorMessage);

                LogExit(2);
                return null;
            }
        }

        private bool ProcessCancelVistaResponse(TmpHealthShareMakeCancelOutboundResponseMessage response, Guid apptId, Guid serviceApptId, Guid contactId)
        {
            LogEntry();

            if (response == null) return false;

            var errMessage = string.Empty;
            var exceptionOccured = false;

            Logger.WriteDebugMessage($"Patience Integration Result Information Count: {response.PatientIntegrationResultInformation.Count}");

            foreach (var patientIntegrationResultInformation in response.PatientIntegrationResultInformation)
            {
                Logger.WriteDebugMessage($"Control Id: {patientIntegrationResultInformation?.ControlId}");
                var errorMessage = patientIntegrationResultInformation.ExceptionOccured ? patientIntegrationResultInformation.ExceptionMessage : string.Empty;
                if (apptId != Guid.Empty)
                {
                    IntegrationPluginHelpers.CreateAppointmentIntegrationResult(
                        "Vista Cancel Individual Patient", patientIntegrationResultInformation.ExceptionOccured, errorMessage,
                        patientIntegrationResultInformation.VimtRequest, response.SerializedInstance, patientIntegrationResultInformation.VimtResponse,
                        typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                        MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, apptId, OrganizationService, response.VimtLagMs, patientIntegrationResultInformation.EcProcessingMs,
                        response.VimtProcessingMs, true, patientIntegrationResultInformation);

                }

                if (serviceApptId != Guid.Empty)
                {
                    IntegrationPluginHelpers.CreateServiceAppointmentIntegrationResult("Vista Cancel Individual Patient", patientIntegrationResultInformation.ExceptionOccured, errorMessage,
                        patientIntegrationResultInformation.VimtRequest, response.SerializedInstance, patientIntegrationResultInformation.VimtResponse,
                        typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                        MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, serviceApptId, contactId, OrganizationService, response.VimtLagMs, patientIntegrationResultInformation.EcProcessingMs,
                        response.VimtProcessingMs, Logger, true, patientIntegrationResultInformation);

                }


                errMessage += errorMessage;
                exceptionOccured = !string.IsNullOrEmpty(errMessage);
            }

            if (!exceptionOccured && !response.ExceptionOccured)
            {
                LogExit(1);
                return true;
            }

            Logger.WriteToFile("Cancel Individual Patient Failed: " + errMessage);

            LogExit(2);
            return false;
        }

        private bool ProcessCancelCernerResponse(TmpCernerOutboundResponseMessage response, Guid apptId)
        {
            LogEntry();

            if (response == null)
            {
                LogExit(1);
                return false;
            }

            var errMessage = string.Empty;
            var exceptionOccured = false;


            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            IntegrationPluginHelpers.CreateCernerAppointmentIntegrationResult(
                "Cerner Cancel Individual Patient", response.ExceptionOccured, errorMessage,
                response.RequestMessage, "", response.ResponseMessage,
                typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, apptId, OrganizationService, 0, 0,
                response.MessageProcessingTime, false);

            errMessage += errorMessage;
            exceptionOccured = !string.IsNullOrEmpty(errMessage);


            if (!exceptionOccured && !response.ExceptionOccured)
            {
                LogExit(2);
                return true;
            }

            Logger.WriteToFile("Cerner Cancel Individual Patient Failed: " + errMessage);

            LogExit(3);
            return false;
        }


        private void ProcessCancelVvsResponse(VideoVisitDeleteResponseMessage response, Guid apptId, Guid serviceApptId, Guid contactId)
        {
            LogEntry();

            if (response == null)
            {
                LogExit(1);
                return;
            }

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;

            if (apptId != Guid.Empty)
            {
                IntegrationPluginHelpers.CreateAppointmentIntegrationResult("VVS Cancel Individual Patient", response.ExceptionOccured, errorMessage, response.VimtRequest,
                response.SerializedInstance, response.VimtResponse, typeof(VideoVisitDeleteRequestMessage).FullName, typeof(VideoVisitDeleteResponseMessage).FullName,
                MessageRegistry.VideoVisitDeleteRequestMessage, apptId, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs);

            }

            if (serviceApptId != Guid.Empty)
            {
                IntegrationPluginHelpers.CreateServiceAppointmentIntegrationResult("VVS Cancel Individual Patient",
                response.ExceptionOccured, errorMessage, response.VimtRequest, response.SerializedInstance, response.VimtResponse,
                typeof(VideoVisitDeleteRequestMessage).FullName, typeof(VideoVisitDeleteResponseMessage).FullName, MessageRegistry.VideoVisitDeleteRequestMessage,
                serviceApptId, contactId, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs
                , Logger, false);
            }

            if (!response.ExceptionOccured) Logger.WriteDebugMessage("VVS Results Updated Successfully");
            else Logger.WriteToFile("VVS Cancel Individual Patient Failed: " + errorMessage);

            LogExit(2);
        }

        private bool FinalizeCrmAppointment(Guid apptId, Guid serviceApptId, Guid contactId)
        {
            LogEntry();

            Logger.WriteDebugMessage($"Patient Id: {contactId}");

            DataModel.ServiceAppointment sa;
            DataModel.Appointment appt;
            using (var srv = new Xrm(OrganizationService))
            {
                sa = srv.ServiceAppointmentSet.FirstOrDefault(i => i.Id == serviceApptId);
                appt = sa == null ? srv.AppointmentSet.FirstOrDefault(a => a.Id == apptId) : null;
            }
            if (appt != null)
            {
                var updateAppt = new DataModel.Appointment
                {
                    Id = apptId,
                    OptionalAttendees = GetNewApList(appt, contactId)
                };
                try
                {
                    OrganizationService.Update(updateAppt);
                    Logger.WriteDebugMessage("Removed Patient From Appointment: " + contactId);

                    LogExit(1);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteDebugMessage("Unable to Remove Activity Party from appointment following successful VistA Cancel.  Error: " + CvtHelper.BuildExceptionMessage(ex));
                    LogExit(2);
                    return false;
                }
            }

            Logger.WriteDebugMessage($"Current Customer count: {sa.Customers.Count()}");

            var updateSa = new DataModel.ServiceAppointment
            {
                Id = serviceApptId,
                Customers = GetNewApList(sa, contactId)
            };

            Logger.WriteDebugMessage($"Updated Customer count: {updateSa.Customers.Count()}");

            try
            {
                OrganizationService.Update(updateSa);
                Logger.WriteDebugMessage("Removed Patient From SA: " + contactId);
                LogExit(3);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteDebugMessage("Unable to Remove Activity Party from sa following successful VistA Cancel.  Error: " + CvtHelper.BuildExceptionMessage(ex));
                LogExit(4);
                return false;
            }
        }

        private List<ActivityParty> GetNewApList(Entity appointment, Guid contactId)
        {
            LogEntry();

            Logger.WriteDebugMessage($"Patient Id: {contactId}");

            var appt = appointment.LogicalName == DataModel.Appointment.EntityLogicalName ? appointment.ToEntity<DataModel.Appointment>() : null;
            var sa = appt == null ? appointment.ToEntity<DataModel.ServiceAppointment>() : null;
            var apList = new List<ActivityParty>();

            if (sa == null && appt == null) throw new InvalidPluginExecutionException($"Appointment with id: {appointment.Id} could not be found");

            LogExit(1);

            if (sa != null)
            {
                var customers = sa.Customers.ToList();
                var customerToRemove = customers.Where(ap => ap.PartyId.Id == contactId);
                var updatedCustomers = customers.Except(customerToRemove);
                Logger.WriteDebugMessage($"Customer Count: {customers.Count}");
                Logger.WriteDebugMessage($"Customer Count: {updatedCustomers.Count()}");
                Logger.WriteDebugMessage($"Customer found: {customerToRemove != null}");
                if (updatedCustomers.Count() != customers.Count)
                {
                    Logger.WriteDebugMessage($"Updated customer count: {customers.Count}");
                    apList = updatedCustomers.ToList();
                }
                else
                {
                    apList = customers;
                    Logger.WriteDebugMessage("Removal of customer failed");
                }
            }
            else
            {
                var attendeees = appt.OptionalAttendees.ToList();
                var attendeeToRemove = attendeees.Find(a => a.PartyId.Id == contactId);
                Logger.WriteDebugMessage($"Attendee count: {attendeees.Count}");
                Logger.WriteDebugMessage($"Attendee found: {attendeeToRemove != null}");
                if (attendeees.Remove(attendeeToRemove))
                {
                    Logger.WriteDebugMessage($"Updated attendee count: {attendeees.Count}");
                    apList = attendeees;
                }
                else
                {
                    apList = attendeees;
                    Logger.WriteDebugMessage("Removal of attendee failed");
                }
            }

            return apList;
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

    }
}
