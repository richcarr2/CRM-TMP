using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create
{
    /// <summary>
    /// Sets MiscData step.
    /// </summary>
    public class SetMiscDataStep : IFilter<VirtualMeetingRoomCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SetMiscDataStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomCreateStateObject state)
        {
            state.MiscDataForRequest = string.Format("Domain={0};BaseDialUrl={1}", state.VirtualMeetingRoomSuffix, string.Format("https://{0}/{1}", state.VirtualMeetingRoomSuffix, state.VmrBaseUrlExtension));
        }
    }
}