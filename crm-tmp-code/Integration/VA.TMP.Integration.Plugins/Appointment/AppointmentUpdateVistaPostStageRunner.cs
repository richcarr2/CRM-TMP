using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.Integration.Plugins.Shared;
using MCSShared;
using VA.TMP.Integration.Plugins.Messages;
using VRMRest;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Appointment
{
    class AppointmentUpdateVistaPostStageRunner : PluginRunner
    {
        public AppointmentUpdateVistaPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override string McsSettingsDebugField => "mcs_appointmentplugin";

        public string VimtUrl { get; set; }

        public int VimtTimeout { get; set; }

        public bool Success;

        public override void Execute()
        {
            var appt = OrganizationService.Retrieve(DataModel.Appointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.Appointment>();

            var sa = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, appt.cvt_serviceactivityid.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();
            var runIntegration = false;
            using (var srv = new Xrm(OrganizationService))
                runIntegration = VistaPluginHelpers.RunVistaIntegration(sa, srv, Logger);
            if (runIntegration)
                SendVistaIntegration(appt, sa);
            else
            {
                Success = true;
                Logger.WriteDebugMessage("Vista switch turned off");
            }
        }

        public void SendVistaIntegration(DataModel.Appointment appointment, DataModel.ServiceAppointment serviceAppointment)
        {
            using (var context = new Xrm(OrganizationService))
            {
                VimtTimeout = IntegrationPluginHelpers.GetVimtTimeout(context, Logger, GetType().Name);

                var vimtUrlSetting = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "VIMT URL");
                if (vimtUrlSetting != null) VimtUrl = vimtUrlSetting.mcs_value;
                else throw new InvalidPluginExecutionException("No VIMT Url listed in Integration Settings.  Please contact the Help Desk to add VIMT URL.  Proxy Add Canceled.");
            }
            Logger.WriteDebugMessage("Beginning VistA Book/Cancel");
            bool isBookRequest;
            var changedPatientIds = VistaPluginHelpers.GetChangedPatients(appointment, OrganizationService, Logger, out isBookRequest);
            
            //Use the version of the record that got passed in (so that the appt is only considered canceled if the user actually hit the "Cancel Dialog" - aka the Integration Booking Status was changed to a "canceled state")
            bool isWholeAppointmentCanceled = VistaPluginHelpers.FullAppointmentCanceled(PrimaryEntity.ToEntity<DataModel.Appointment>());

            if (isBookRequest)
                SendReceiveVistaBook(appointment, changedPatientIds);
            else if (!isBookRequest && isWholeAppointmentCanceled)
                SendReceiveVistaCancel(appointment, changedPatientIds);
            else
            {
                Logger.WriteDebugMessage("Patient removed from Block Resource as result of Individual Cancelation, no further action needed.  Ending plugin now.");
                //throw new InvalidPluginExecutionException("Invalid Plugin Registration for Vista Integration");
                Success = true;
            }
        }

        private int SendReceiveVistaBook(DataModel.Appointment appointment, List<Guid> addedPatients)
        {
            var failures = 0;
            var vistaStatus = -1;
            using (var srv = new Xrm(OrganizationService))
            {
                var appt = srv.AppointmentSet.FirstOrDefault(a => a.Id == appointment.Id);
                var id = appt != null ? appt.cvt_serviceactivityid?.Id : appointment.Id;
                var sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == id);

                string proUserDuz = string.Empty;

                foreach (var patientId in addedPatients)
                {
                    var apptReq = new MakeGroupAppointmentRequestMessage
                    {
                        AddedPatients = new List<Guid> { patientId },
                        AppointmentId = appointment.Id,
                        OrganizationName = PluginExecutionContext.OrganizationName,
                        UserId = PluginExecutionContext.UserId,
                        LogRequest = true,
                        PatUserDuz = VistaPluginHelpers.GetUserDuz(sa, OrganizationService, PluginExecutionContext.UserId, Logger, out proUserDuz),
                        ProUserDuz = proUserDuz,
                        SAMLToken = VistaPluginHelpers.GetUserSaml(OrganizationService, PluginExecutionContext.UserId, Logger)
                    };

                    var vimtRequest = IntegrationPluginHelpers.SerializeInstance(apptReq);
                    MakeGroupAppointmentResponseMessage response = null;
                    try
                    {
                        response = Utility.SendReceive<MakeGroupAppointmentResponseMessage>(new Uri(VimtUrl), MessageRegistry.MakeGroupAppointmentRequestMessage, apptReq, null, VimtTimeout, out int lag);
                        response.VimtLagMs = lag;
                        var status = ProcessVistaMakeApptResponse(response, typeof(MakeGroupAppointmentRequestMessage), typeof(MakeGroupAppointmentResponseMessage), appointment, new EntityReference(Contact.EntityLogicalName, patientId));
                        if (status != (int)serviceappointment_statuscode.ReservedScheduled)
                        {
                            failures++;
                            vistaStatus = status;
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO: Log the response object on failure
                        var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);

                        IntegrationPluginHelpers.CreateAppointmentIntegrationResultOnVimtFailure("Make Group Vista Appointment", errorMessage,
                            vimtRequest, typeof(MakeGroupAppointmentRequestMessage).FullName, typeof(MakeGroupAppointmentResponseMessage).FullName, MessageRegistry.MakeGroupAppointmentRequestMessage, appointment.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                        Logger.WriteToFile("Failed Make Group Appointment: " + CvtHelper.BuildExceptionMessage(ex));
                        failures++;
                    }
                }
            }
            if (failures > 0)
            {
                Success = false;
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, PrimaryEntity.Id, (Appointmentcvt_IntegrationBookingStatus)vistaStatus);
            }
            else
                Success = true;
            return failures;
        }

        private int SendReceiveVistaCancel(DataModel.Appointment appointment, List<Guid> removedPatients)
        {
            var failures = 0;
            using (var srv = new Xrm(OrganizationService))
            {
                var appt = srv.AppointmentSet.FirstOrDefault(a => a.Id == appointment.Id);
                var id = appt != null ? appt.cvt_serviceactivityid?.Id : appointment.Id;
                var sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == id);

                string proUserDuz = string.Empty;
                foreach (var patient in removedPatients)
                {
                    try
                    {
                        var appointmentRequestMessage = new CancelGroupAppointmentRequestMessage
                        {
                            AppointmentId = appointment.Id,
                            CanceledPatients = new List<Guid> { patient },
                            LogRequest = true,
                            OrganizationName = PluginExecutionContext.OrganizationName,
                            UserId = PluginExecutionContext.UserId,
                            WholeAppointmentCanceled = true,
                            PatUserDuz = VistaPluginHelpers.GetUserDuz(sa, OrganizationService, PluginExecutionContext.UserId, Logger, out proUserDuz),
                            ProUserDuz = proUserDuz,
                            SamlToken = VistaPluginHelpers.GetUserSaml(OrganizationService, PluginExecutionContext.UserId, Logger)
                        };
                        Logger.WriteDebugMessage("Sending to CancelGroup Request to VIMT");
                        var response = Utility.SendReceive<CancelGroupAppointmentResponseMessage>(new Uri(VimtUrl), MessageRegistry.CancelGroupAppointmentRequestMessage, appointmentRequestMessage, null, VimtTimeout, out int lag);
                        response.VimtLagMs = lag;
                        Logger.WriteDebugMessage("CancelGroup Response Received from VIMT");
                        var failed = ProcessVistaCancelResponse(response, typeof(CancelGroupAppointmentRequestMessage), typeof(CancelGroupAppointmentResponseMessage), appointment, true, new EntityReference(Contact.EntityLogicalName, patient));
                        if (failed)
                            failures++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(string.Format("Cancel Vista Appointment Failed for: {0} on appt: {1}. Error Message: {2}", patient, appointment.Subject, CvtHelper.BuildExceptionMessage(ex)));
                        failures++;
                    }
                }
            }
            if (failures > 0)
            {
                Success = false;
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, PrimaryEntity.Id, Appointmentcvt_IntegrationBookingStatus.CancelFailure);
            }
            else
                Success = true;
            return failures;
        }

        private int ProcessVistaMakeApptResponse(MakeGroupAppointmentResponseMessage response, Type requestType, Type responseType, VA.TMP.DataModel.Appointment appointment, EntityReference patient)
        {
            if (response == null) return (int)Appointmentcvt_IntegrationBookingStatus.VistaFailure;

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;

            var integrationResultId = IntegrationPluginHelpers.CreateAppointmentIntegrationResult("Group Book to Vista", response.ExceptionOccured, errorMessage, response.VimtRequest, response.SerializedInstance, response.VimtResponse, requestType.FullName, responseType.FullName, MessageRegistry.MakeGroupAppointmentRequestMessage, PrimaryEntity.Id, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, false);

            Appointmentcvt_IntegrationBookingStatus status = Appointmentcvt_IntegrationBookingStatus.ReservedScheduled;
            if (response.ExceptionOccured)
                status = Appointmentcvt_IntegrationBookingStatus.VistaFailure;
            else
            {
                var vistaAppointments = new List<VistaAppointment>
                {
                    response.PatVistaAppointment,
                    response.ProVistaAppointment
                };
                var vistaFailures = IntegrationPluginHelpers.WriteVistaResults(vistaAppointments, integrationResultId, "book", OrganizationService, Logger, patient);
                if (vistaFailures > 0)
                    status = Appointmentcvt_IntegrationBookingStatus.VistaFailure;
            }
            return (int)status;
        }

        private bool ProcessVistaCancelResponse(CancelGroupAppointmentResponseMessage response, Type requestType, Type responseType, VA.TMP.DataModel.Appointment appointment, bool wholeAppointmentCanceled, EntityReference patient)
        {
            if (response == null) return true;

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;

            var integrationResultId = IntegrationPluginHelpers.CreateAppointmentIntegrationResult("Group Cancel to Vista", response.ExceptionOccured, errorMessage, response.VimtRequest,
                response.SerializedInstance, response.VimtResponse, requestType.FullName, responseType.FullName,
                MessageRegistry.CancelGroupAppointmentRequestMessage, PrimaryEntity.Id, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, false);

            int status = wholeAppointmentCanceled ? appointment.cvt_IntegrationBookingStatus.Value : (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled;
            if (response.ExceptionOccured)
                status = (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure;
            else
            {
                var vistaAppointments = new List<VistaAppointment>
                {
                    response.PatVistaAppointment,
                    response.ProVistaAppointment
                };
                status = IntegrationPluginHelpers.WriteVistaResults(vistaAppointments, integrationResultId, "cancel", OrganizationService, Logger, patient, status);
            }
            if (!wholeAppointmentCanceled)
            {
                Logger.WriteDebugMessage("Individual Cancelation, not updating entire appointment status to " + ((Appointmentcvt_IntegrationBookingStatus)status).ToString());
                return false;
            }
            if (appointment.cvt_IntegrationBookingStatus.Value != status)
                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, appointment.Id, (Appointmentcvt_IntegrationBookingStatus)status);
            else
                Logger.WriteDebugMessage("Appointment Booking Status has not changed, no need to update appointment on cancel");
            return status == (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure;
        }

        //internal void TriggerVVS()
        //{
        //    Success = true;
        //    //var updateAppt = new DataModel.Appointment
        //    //{
        //    //    Id = PrimaryEntity.Id,
        //    //    cvt_TriggerVVS = true
        //    //};
        //    //OrganizationService.Update(updateAppt);
        //    //Logger.WriteDebugMessage("Initiated VVS");
        //}
    }
}
