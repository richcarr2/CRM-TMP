using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Schema.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create
{
    /// <summary>
    /// Serialize VMR step.
    /// </summary>
    public class SerializeVmrStep : IFilter<VirtualMeetingRoomCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SerializeVmrStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomCreateStateObject state)
        {
            state.VirtualMeetingRoom = new VirtualMeetingRoomType
            {
                Version = "1.0",
                AppointmentId = state.AppointmentId.ToString(),
                PatientName = state.PatientId.ToString(),
                ProviderName = state.ProviderId.ToString(),
                MeetingRoomName = state.MeetingRoomName,
                PatientPin = state.PatientPin,
                ProviderPin = state.ProviderPin,
                StartDate = state.AppointmentStartDate,
                EndDate = state.AppointmentEndDate,
                MiscData = state.MiscDataForRequest
            };

            state.SerializedVirtualMeetingRoom = Serialization.DataContractSerialize(state.VirtualMeetingRoom);
        }
    }
}
