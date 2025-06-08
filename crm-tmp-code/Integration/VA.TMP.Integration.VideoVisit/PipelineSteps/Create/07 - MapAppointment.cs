using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.Mappers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Create
{
    /// <summary>
    /// Create Appointment step.
    /// </summary>
    public class MapAppointmentStep : IFilter<VideoVisitCreateStateObject>
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
        public void Execute(VideoVisitCreateStateObject state)
        {
            var isGroup = state.ServiceAppointment.mcs_groupappointment ?? false;
            var videoVisitMapper = new VideoVisitMapper(state.OrganizationServiceProxy, state.ServiceAppointment, state.ContactIds, state.SystemUsers, isGroup, state.CrmAppointment);
            state.Appointment = videoVisitMapper.Map(_logger);
        }
    }
}