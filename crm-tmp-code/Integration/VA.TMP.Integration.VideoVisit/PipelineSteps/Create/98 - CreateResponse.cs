using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.VideoVisit.Helpers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Create
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<VideoVisitCreateStateObject>
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
        public void Execute(VideoVisitCreateStateObject state)
        {
            if (!string.IsNullOrEmpty(state.VistaFakeResponseType))
            {
                var fakeEcResponse = VistaSchedulingUtilities.CreateFakeBookResult(state.VistaFakeResponseType, state);
                state.VideoVisitCreateResponseMessage = new VideoVisitCreateResponseMessage
                {
                    ExceptionMessage = "*****THIS IS FAKE DATA. DATA NOT SENT TO VVS*****",
                    ExceptionOccured = false,
                    SerializedInstance = state.SerializedAppointment,
                    WriteResults = VistaSchedulingUtilities.MapEcToWriteResult(fakeEcResponse)
                };
            }
            else
            {
                state.VideoVisitCreateResponseMessage = state.ExceptionOccured
                    ? new VideoVisitCreateResponseMessage
                    {
                        ExceptionOccured = true,
                        ExceptionMessage = state.ExceptionMessage,
                        SerializedInstance = state.SerializedAppointment,
                        EcProcessingMs = state.EcProcessingTimeMs
                    }
                    : new VideoVisitCreateResponseMessage
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