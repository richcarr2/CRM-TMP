using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Get Appointment step.
    /// </summary>
    public class GetAppointmentStep : IFilter<MakeCancelInboundStateObject>
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
        public void Execute(MakeCancelInboundStateObject state)
        {
            if (!state.IsGroupAppointment) return;

            if (!state.OutboundRequestMessage.AppointmentId.HasValue) throw new MissingAppointmentException("$Group Appointment must have an AppointmentId");

            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                state.Appointment = srv.AppointmentSet.FirstOrDefault(x => x.Id == state.OutboundRequestMessage.AppointmentId.Value);
                if (state.Appointment == null) throw new MissingAppointmentException($"Cannot find Appointment for Service Appointment: {state.OutboundRequestMessage.AppointmentId.Value} required for a Group Appointment");
            }
        }
    }
}
