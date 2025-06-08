using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<VirtualMeetingRoomCreateStateObject>
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
        public void Execute(VirtualMeetingRoomCreateStateObject state)
        {
            if (state.UseFakeResponse)
            {
                state.VirtualMeetingRoomCreateResponseMessage = new VirtualMeetingRoomCreateResponseMessage
                {
                    ExceptionMessage = "*****THIS IS FAKE DATA. VMR NOT CREATED*****",
                    ExceptionOccured = false,
                    MeetingRoomName = state.MeetingRoomName,
                    PatientUrl = state.PatientUrl,
                    ProviderUrl = state.ProviderUrl,
                    PatientPin = state.PatientPin,
                    ProviderPin = state.ProviderPin,
                    AppointmentId = state.CorrelationId,
                    DialingAlias = string.Format("{0}{1}", state.VirtualMeetingRoom, state.VirtualMeetingRoomSuffix),
                    MiscData = string.Format("hostDialUrl={0};guestDialUrl={1};", state.ProviderUrl, state.PatientUrl),
                    SerializedInstance = Serialization.DataContractSerialize(state.VirtualMeetingRoom)
                };
            }
            else
            {
                state.VirtualMeetingRoomCreateResponseMessage = state.ExceptionOccured
                ? new VirtualMeetingRoomCreateResponseMessage { ExceptionOccured = true, ExceptionMessage = state.ExceptionMessage, EcProcessingMs = state.EcProcessingTimeMs }
                : new VirtualMeetingRoomCreateResponseMessage
                {
                    ExceptionMessage = string.Empty,
                    ExceptionOccured = false,
                    MeetingRoomName = state.MeetingRoomName,
                    PatientUrl = state.PatientUrl,
                    ProviderUrl = state.ProviderUrl,
                    PatientPin = state.PatientPin,
                    ProviderPin = state.ProviderPin,
                    AppointmentId = state.CorrelationId,
                    DialingAlias = state.DialingAlias,
                    MiscData = state.MiscDataForResponse,
                    SerializedInstance = Serialization.DataContractSerialize(state.VirtualMeetingRoom),
                    EcProcessingMs = state.EcProcessingTimeMs
                };
            }
        }
    }
}