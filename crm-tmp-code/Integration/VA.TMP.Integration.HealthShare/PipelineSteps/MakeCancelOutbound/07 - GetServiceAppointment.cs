using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Get Service Appointment step.
    /// </summary>
    public class GetServiceAppointmentStep : IFilter<MakeCancelOutboundStateObject>
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
        public void Execute(MakeCancelOutboundStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                state.ServiceAppointment = srv.ServiceAppointmentSet.FirstOrDefault(x => x.Id == state.ServiceAppointmentId);
            }

            if (state.ServiceAppointment == null) throw new MissingAppointmentException("Service Appointment cannot be null");
        }
    }
}
