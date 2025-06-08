using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Get Appointment Type Step.
    /// </summary>
    public class GetAppointmentTypeStep : IFilter<MakeCancelOutboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetAppointmentTypeStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelOutboundStateObject state)
        {
            if (state.ServiceAppointment.mcs_groupappointment.HasValue && state.ServiceAppointment.mcs_groupappointment.Value)
            {
                state.GroupAppointment = true;
            }

            if (state.ServiceAppointment.cvt_Type.HasValue && state.ServiceAppointment.cvt_Type.Value)
            {
                //Note: VA Video Connect Appointments with "Patient Site Resources Required" set as True is considered as Clinic Based Appointment Type as it involves both Patient and Provider resources
                if (state.ServiceAppointment.cvt_patientsiteresourcesrequired.HasValue && state.ServiceAppointment.cvt_patientsiteresourcesrequired.Value)
                    state.AppointmentType = AppointmentType.CLINIC_BASED;
                else
                    state.AppointmentType = AppointmentType.HOME_MOBILE;
            }
            else if (state.ServiceAppointment.mcs_groupappointment.HasValue && state.ServiceAppointment.mcs_groupappointment.Value)
            {
                state.AppointmentType = AppointmentType.GROUP;
                state.IsGroupAppointment = true;
            }
            else if (state.ServiceAppointment.cvt_TelehealthModality.HasValue && state.ServiceAppointment.cvt_TelehealthModality.Value)
            {
                state.AppointmentType = AppointmentType.STORE_FORWARD;
            }
            else state.AppointmentType = AppointmentType.CLINIC_BASED;

            _logger.Info($"AppointmentType: {state.AppointmentType}, IsGroup: {state.IsGroupAppointment}, RelatedProviderSite Null? {state.ServiceAppointment.mcs_relatedprovidersite == null}, RelatedSite(Patient) Null? {state.ServiceAppointment.mcs_relatedsite == null}, Patient Site Resources Required? " +
                $"{state.ServiceAppointment.cvt_patientsiteresourcesrequired.Value}");
        }
    }
}