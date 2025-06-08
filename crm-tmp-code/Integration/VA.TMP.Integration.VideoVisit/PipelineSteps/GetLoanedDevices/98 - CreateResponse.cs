using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.VideoVisit.Helpers;
using VA.TMP.Integration.VideoVisit.StateObject;
using static VA.TMP.Integration.Messages.VideoVisit.VideoVisitGetLoanedDevicesResponseMessage;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.GetLoanedDevices
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<VideoVisitGetLoanedDevicesStateObject>
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
        public void Execute(VideoVisitGetLoanedDevicesStateObject state)
        {
            if (!string.IsNullOrEmpty(state.VistaFakeResponseType))
            {
                state.VideoVisitGetLoanedDevicesResponseMessage = new VideoVisitGetLoanedDevicesResponseMessage
                {
                    ExceptionMessage = "*****THIS IS FAKE DATA. DATA NOT SENT TO VVS*****",
                    ExceptionOccured = false,
                    SerializedInstance = state.SerializedAppointment
                };
            }
            else
            {
                state.VideoVisitGetLoanedDevicesResponseMessage = state.ExceptionOccured
                    ? new VideoVisitGetLoanedDevicesResponseMessage
                    {
                        ExceptionOccured = true,
                        ExceptionMessage = state.ExceptionMessage,
                        SerializedInstance = state.SerializedAppointment,
                        EcProcessingMs = state.EcProcessingTimeMs
                    }
                    : new VideoVisitGetLoanedDevicesResponseMessage
                    {
                        ExceptionMessage = string.Empty,
                        ExceptionOccured = false,
                        SerializedInstance = state.SerializedAppointment,
                        EcProcessingMs = state.EcProcessingTimeMs,
                        Devices = state.VideoVisitGetLoanedDevicesResponseMessage.Devices,
                        Links = state.VideoVisitGetLoanedDevicesResponseMessage.Links
                    };
            }
        }
    }
}