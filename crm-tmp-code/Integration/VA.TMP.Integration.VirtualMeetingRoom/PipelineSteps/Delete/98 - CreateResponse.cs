using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Delete
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<VirtualMeetingRoomDeleteStateObject>
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
        public void Execute(VirtualMeetingRoomDeleteStateObject state)
        {
            if (state.UseFakeResponse)
            {
                state.VirtualMeetingRoomDeleteResponseMessage = new VirtualMeetingRoomDeleteResponseMessage
                {
                    ExceptionMessage = "*****THIS IS FAKE DATA. VMR NOT CANCELED*****",
                    ExceptionOccured = false,
                    MiscData = string.Empty,
                    SerializedInstance = Serialization.DataContractSerialize(state.VirtualMeetingRoomDelete)
                };
            }
            else
            {
                state.VirtualMeetingRoomDeleteResponseMessage = state.ExceptionOccured
                ? new VirtualMeetingRoomDeleteResponseMessage { ExceptionOccured = true, ExceptionMessage = state.ExceptionMessage, EcProcessingMs = state.EcProcessingTimeMs }
                : new VirtualMeetingRoomDeleteResponseMessage
                {
                    ExceptionMessage = string.Empty,
                    ExceptionOccured = false,
                    MiscData = state.MiscDataForResponse,
                    SerializedInstance = Serialization.DataContractSerialize(state.VirtualMeetingRoomDelete),
                    EcProcessingMs = state.EcProcessingTimeMs
                };
            }
        }
    }
}