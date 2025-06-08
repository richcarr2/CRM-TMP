using Ec.HealthShare.Messages;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Get the Make/Cancel Outbound EC Request Message.
    /// </summary>
    public class GetOutboundEcRequestMessageStep : IFilter<MakeCancelInboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetOutboundEcRequestMessageStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelInboundStateObject state)
        {
            var ecRequestMessageString = state.IntegrationResult.mcs_vimtrequest;
            if (string.IsNullOrEmpty(ecRequestMessageString)) throw new MissingLobRequestException($"The Lob Request cannot be null or empty for Integration Result {state.IntegrationResult.Id}");

            state.OutboundEcRequestMessage = Serialization.DataContractDeserialize<EcHealthShareMakeCancelOutboundRequestMessage>(ecRequestMessageString);
        }
    }
}