using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.OnDemand
{
    /// <summary>
    /// Create Patient/Provider URL step.
    /// </summary>
    public class CreateUrlsStep : IFilter<VmrOnDemandCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CreateUrlsStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VmrOnDemandCreateStateObject state)
        {
            if (state.ExceptionOccured) return;

            state.ProviderUrl = string.Format(state.ProviderVmrFormatUrl, state.VmrBaseUrl, state.VmrBaseUrlExtension, state.ProviderId, state.MeetingRoomName, state.VirtualMeetingRoomSuffix, state.ProviderPin);
            state.PatientUrl = string.Format(state.PatientVmrFormatUrl, state.VmrBaseUrl, state.VmrBaseUrlExtension, state.MeetingRoomName, state.VirtualMeetingRoomSuffix, state.PatientPin);

            if (state.UseFakeResponse) state.MiscDataForResponse = string.Format("hostDialUrl={0};guestDialUrl={1};", state.ProviderUrl, state.PatientUrl);
        }
    }
}