using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.Mappers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    public class MapAppointmentStep : IFilter<VideoVisitUpdateStateObject>
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

        public void Execute(VideoVisitUpdateStateObject state)
        {
            var isGroup = state.ServiceAppointment.mcs_groupappointment ?? false;
            var videoVisitMapper = new VideoVisitMapper(state.OrganizationServiceProxy, state.ServiceAppointment, state.ContactIds, state.SystemUsers, isGroup, state.CrmAppointment);

            state.Appointment = videoVisitMapper.MapUpdate(_logger);
        }
    }
}