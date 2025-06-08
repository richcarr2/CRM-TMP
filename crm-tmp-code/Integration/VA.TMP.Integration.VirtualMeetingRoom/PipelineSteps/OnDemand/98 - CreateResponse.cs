using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.OnDemand
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<VmrOnDemandCreateStateObject>
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
        public void Execute(VmrOnDemandCreateStateObject state)
        {
            if (state.UseFakeResponse)
            {
                state.VmrOnDemandCreateResponseMessage = new VmrOnDemandCreateResponseMessage
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
                _logger.Debug("Used Fake Response");
            }
            else
            {
                state.VmrOnDemandCreateResponseMessage = state.ExceptionOccured
                ? new VmrOnDemandCreateResponseMessage { ExceptionOccured = true, ExceptionMessage = state.ExceptionMessage, EcProcessingMs = state.EcProcessingTimeMs }
                : new VmrOnDemandCreateResponseMessage
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