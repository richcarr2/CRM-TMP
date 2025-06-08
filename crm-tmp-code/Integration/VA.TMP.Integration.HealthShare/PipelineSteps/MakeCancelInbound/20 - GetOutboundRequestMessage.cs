using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Get Outbound Request Message step.
    /// </summary>
    public class GetOutboundRequestMessageStep : IFilter<MakeCancelInboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetOutboundRequestMessageStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelInboundStateObject state)
        {
            var integrationRequest = state.IntegrationResult.mcs_integrationrequest;
            if (string.IsNullOrEmpty(integrationRequest)) throw new MissingIntegrationResultException($"The Integration Request cannot be null or empty for Integration Result {state.IntegrationResult.Id}");

            state.OutboundRequestMessage = Serialization.DataContractDeserialize<TmpHealthShareMakeCancelOutboundRequestMessage>(integrationRequest);
        }
    }
}