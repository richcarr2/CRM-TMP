using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    public class SerializeAppointmentStep : IFilter<VideoVisitUpdateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SerializeAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        public void Execute(VideoVisitUpdateStateObject state)
        {
            state.SerializedAppointment = Serialization.DataContractSerialize(state.Appointment);
        }
    }
}