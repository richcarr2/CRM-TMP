using System.Collections.Generic;
using Ec.VideoVisit.Messages;
using log4net;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.Mappers;
using VA.TMP.Integration.VideoVisit.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    /// <summary>
    /// Delete Appointment step.
    /// </summary>
    public class MapAppointmentStep : IFilter<VideoVisitDeleteStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitDeleteStateObject state)
        {
            state.CancelAppointmentRequest = new EcTmpCancelAppointmentRequest
            {
                OrganizationName = state.OrganizationName,
                UserId = state.UserId,
                Id = state.AppointmentId.ToString(),
                SourceSystem = "TMP",
                PatientBookingStatuses = MapCancellations(state),
                SamlToken = ""
            };
        }

        private static EcTmpPersonBookingStatuses MapCancellations(VideoVisitDeleteStateObject state)
        {
            var statusList = new List<EcTmpPersonBookingStatus>();
            var statuses = new EcTmpPersonBookingStatuses { };
            var description = string.Empty;
            EcTmpStatusCode ecStatus;
            if (state.WholeApptCanceled)
            {
                if (state.IsGroup)
                {
                    var crmStatus = state.CrmAppointment.cvt_IntegrationBookingStatus.Value;
                    ecStatus = GetAppointmentCancelStatusCode(crmStatus);
                    description = ((Appointmentcvt_IntegrationBookingStatus)crmStatus).ToString();
                }
                else
                {
                    ecStatus = GetServiceAppointmentCancelStatusCode(state);
                    description = ((Appointmentcvt_IntegrationBookingStatus)state.ServiceAppointment.StatusCode.Value).ToString();
                }
            }
            else
            {
                // TODO - Temporary
                ecStatus = EcTmpStatusCode.CANCELLED_BY_PATIENT;
                description = Appointmentcvt_IntegrationBookingStatus.PatientCanceled.ToString();

                //var crmstatus = GetIndividualPatientCancellationStatus(state);
                //description = ((Appointmentcvt_IntegrationBookingStatus)crmstatus).ToString();
                //ecStatus = GetAppointmentCancelStatusCode(crmstatus);
            }

            foreach (var patId in state.CanceledPatients)
            {
                var contact = (Contact)state.OrganizationServiceProxy.Retrieve(Contact.EntityLogicalName, patId, new ColumnSet(true));

                var status = new EcTmpStatus
                {
                    Code = ecStatus,
                    CodeSpecified = true,
                    Reason = EcTmpReasonCode.OTHER,
                    ReasonSpecified = true,
                    Description = description
                };
                var patStatus = new EcTmpPersonBookingStatus
                {
                    Id = MappingResolvers.PersonIdentifierResolver(state.OrganizationServiceProxy, contact),
                    Status = status
                };
                statusList.Add(patStatus);

            }
            statuses.PersonBookingStatus = statusList.ToArray();

            return statuses;
        }

        /// <summary>
        /// Map the TMP status code to the schema status code.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static EcTmpStatusCode GetServiceAppointmentCancelStatusCode(VideoVisitDeleteStateObject state)
        {
            if (state.ServiceAppointment.StatusCode == null) return EcTmpStatusCode.CANCELLED_BY_CLINIC;

            switch (state.ServiceAppointment.StatusCode.Value)
            {
                case (int)serviceappointment_statuscode.PatientCanceled: return EcTmpStatusCode.CANCELLED_BY_PATIENT;
                case (int)serviceappointment_statuscode.SchedulingError:
                case (int)serviceappointment_statuscode.ClinicCanceled:
                case (int)serviceappointment_statuscode.TechnologyFailure: return EcTmpStatusCode.CANCELLED_BY_CLINIC;
                default: return EcTmpStatusCode.CANCELLED_BY_CLINIC;
            }
        }

        private static EcTmpStatusCode GetAppointmentCancelStatusCode(int crmStatus)
        {
            switch (crmStatus)
            {
                case (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled: return EcTmpStatusCode.CANCELLED_BY_PATIENT;
                case (int)Appointmentcvt_IntegrationBookingStatus.SchedulingError:
                case (int)Appointmentcvt_IntegrationBookingStatus.ClinicCancelled:
                case (int)Appointmentcvt_IntegrationBookingStatus.TechnologyFailure: return EcTmpStatusCode.CANCELLED_BY_CLINIC;
                case (int)Appointmentcvt_IntegrationBookingStatus.PatientNoShow: return EcTmpStatusCode.NO_SHOW;
                default: return EcTmpStatusCode.CANCELLED_BY_PATIENT;
            }
        }
    }
}
