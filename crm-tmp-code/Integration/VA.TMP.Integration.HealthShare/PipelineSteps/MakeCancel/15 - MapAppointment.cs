using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Mappers;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancel
{
    /// <summary>
    /// Map Appointment to CRM etities step.
    /// </summary>
    public class MapAppointmentStep : IFilter<MakeCancelStateObject>
    {
        private readonly ILog _logger;
        private readonly Rest.Interface.IServicePost _servicePost;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapAppointmentStep(ILog logger, Rest.Interface.IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelStateObject state)
        {
            state.Appointment = new MakeCancelMapper(state.RequestMessage, state.OrganizationServiceProxy, _logger, _servicePost, _settings).Map();
        }
    }
}