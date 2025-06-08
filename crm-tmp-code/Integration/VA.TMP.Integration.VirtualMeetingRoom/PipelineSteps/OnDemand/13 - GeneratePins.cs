using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.OnDemand
{
    /// <summary>
    /// Generate Patient/Provider PIN Processing step.
    /// </summary>
    public class GeneratePinsStep : IFilter<VmrOnDemandCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GeneratePinsStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VmrOnDemandCreateStateObject state)
        {
            state.PatientPin = string.Format("{0}#", RandomDigits.GetRandomDigitString(state.PatientPinLength));
            state.ProviderPin = string.Format("{0}#", RandomDigits.GetRandomDigitString(state.ProviderPinLength));
        }
    }
}