using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Schema.VirtualMeetingRoom;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Delete
{
    /// <summary>
    /// Serialize VMR step.
    /// </summary>
    public class SerializeDeleteVmrStep : IFilter<VirtualMeetingRoomDeleteStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SerializeDeleteVmrStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomDeleteStateObject state)
        {
            state.VirtualMeetingRoomDelete = new VirtualMeetingRoomDeleteType
            {
                Version = "1.0",
                AppointmentId = state.AppointmentId.ToString(),
                MiscData = state.MiscDataForRequest
            };

            state.SerializedVirtualMeetingRoomDelete = Serialization.DataContractSerialize(state.VirtualMeetingRoomDelete);
        }
    }
}
