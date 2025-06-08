using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<MakeCancelOutboundStateObject>
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
        public void Execute(MakeCancelOutboundStateObject state)
        {
            state.ResponseMessage.SerializedInstance = state.SerializedRequestMessage;
        }
    }
}