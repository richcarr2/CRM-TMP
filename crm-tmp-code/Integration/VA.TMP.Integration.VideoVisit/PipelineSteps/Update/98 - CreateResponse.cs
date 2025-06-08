using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.VideoVisit.Helpers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    public class CreateResponseStep : IFilter<VideoVisitUpdateStateObject>
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

        public void Execute(VideoVisitUpdateStateObject state)
        {
            if (!string.IsNullOrEmpty(state.FakeResponseType))
            {
                var fakeEcResponse = VistaSchedulingUtilities.CreateFakeBookResult(state.FakeResponseType, state);
                state.VideoVisitUpdateResponseMessage = new VideoVisitUpdateResponseMessage
                {
                    ExceptionMessage = "*****THIS IS FAKE DATA. DATA NOT SENT TO VVS*****",
                    ExceptionOccured = false,
                    SerializedInstance = state.SerializedAppointment,
                    WriteResults = VistaSchedulingUtilities.MapEcToWriteResult(fakeEcResponse)
                };
            }
            else
            {
                state.VideoVisitUpdateResponseMessage = state.ExceptionOccured
                    ? new VideoVisitUpdateResponseMessage
                    {
                        ExceptionOccured = true,
                        ExceptionMessage = state.ExceptionMessage,
                        SerializedInstance = state.SerializedAppointment,
                        EcProcessingMs = state.EcProcessingTimeMs
                    }
                    : new VideoVisitUpdateResponseMessage
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