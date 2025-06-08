using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    /// <summary>
    /// Serialize Appointment step.
    /// </summary>
    public class SerializeAppointmentStep : IFilter<VideoVisitDeleteStateObject>
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

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitDeleteStateObject state)
        {
            state.SerializedAppointment = Serialization.DataContractSerialize(state.CancelAppointmentRequest);
        }
    }
}