using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Serialize Inbound Request Step.
    /// </summary>
    public class SerializeInboundRequestStep : IFilter<MakeCancelInboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SerializeInboundRequestStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelInboundStateObject state)
        {
            state.SerializedRequestMessage = Serialization.DataContractSerialize(state.RequestMessage);
            _logger.Info($"INFO: HealthShare Make Cancel Inbound: {state.SerializedRequestMessage}");
        }
    }
}