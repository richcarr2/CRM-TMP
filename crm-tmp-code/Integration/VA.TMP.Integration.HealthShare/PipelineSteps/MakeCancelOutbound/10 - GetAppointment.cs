using System;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Get Appointments Step.
    /// </summary>
    public class GetAppointmentStep : IFilter<MakeCancelOutboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelOutboundStateObject state)
        {
            if (!state.IsGroupAppointment) return;

            if (!state.AppointmentId.HasValue || state.AppointmentId.Value == Guid.Empty) throw new MissingAppointmentException("Group Appointment must have a valid AppointmentId");

            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                state.Appointment = srv.AppointmentSet.FirstOrDefault(x => x.Id == state.AppointmentId.Value);
                if (state.Appointment == null) throw new MissingAppointmentException($"Cannot find Appointment: {state.AppointmentId.Value} required for a Group Appointment");
            }
        }
    }
}
