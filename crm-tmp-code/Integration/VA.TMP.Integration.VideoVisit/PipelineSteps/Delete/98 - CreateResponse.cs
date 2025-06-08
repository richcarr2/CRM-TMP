using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.VideoVisit.Helpers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<VideoVisitDeleteStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CreateResponseStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitDeleteStateObject state)
        {
            if (!string.IsNullOrEmpty(state.VistaFakeResponseType))
            {
                var fakeEcResponse = VistaSchedulingUtilities.CreateFakeCancelResult(state.VistaFakeResponseType, state);
                state.VideoVisitDeleteResponseMessage = new VideoVisitDeleteResponseMessage
                {
                    ExceptionMessage = "*****THIS IS FAKE DATA. DATA NOT SENT TO VVS*****",
                    ExceptionOccured = false,
                    SerializedInstance = state.SerializedAppointment,
                    WriteResults = VistaSchedulingUtilities.MapEcToWriteResult(fakeEcResponse)
                };
            }
            else
            {
                state.VideoVisitDeleteResponseMessage = state.ExceptionOccured
                    ? new VideoVisitDeleteResponseMessage { ExceptionOccured = true, ExceptionMessage = state.ExceptionMessage, EcProcessingMs = state.EcProcessingTimeMs }
                    : new VideoVisitDeleteResponseMessage
                    {
                        ExceptionMessage = string.Empty,
                        ExceptionOccured = false,
                        SerializedInstance = state.SerializedAppointment,
                        WriteResults = VistaSchedulingUtilities.MapEcToWriteResult(state.EcResponse.EcTmpWriteResults),
                        EcProcessingMs = state.EcProcessingTimeMs
                    };
            }
        }
    }
}