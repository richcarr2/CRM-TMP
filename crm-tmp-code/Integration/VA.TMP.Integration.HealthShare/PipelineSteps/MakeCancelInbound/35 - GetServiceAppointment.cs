using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Get Service Appointment Step.
    /// </summary>
    public class GetServiceAppointmentStep : IFilter<MakeCancelInboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetServiceAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelInboundStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                state.ServiceAppointment = srv.ServiceAppointmentSet.FirstOrDefault(x => x.Id == state.OutboundRequestMessage.ServiceAppointmentId);
                if (state.ServiceAppointment == null)
                {
                    throw new MissingAppointmentException($"Unable to find Service Appointment for: {state.OutboundRequestMessage.ServiceAppointmentId}");
                }
            }
        }
    }
}
