using System;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Mappers;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Map Provider Veteran Identifier to Proxy Add Request EC step.
    /// </summary>
    public class MapProviderVeteranIdentifierToProxyAddRequestEcStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapProviderVeteranIdentifierToProxyAddRequestEcStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            if (string.IsNullOrEmpty(state.ProviderSite) || state.PatientAndProviderSitesAreEqual || state.ProviderSideIdentifierToAdd == null || state.ExceptionOccured || state.Veteran == null) return;

            state.ProxyAddToVistaRequest = new ProxyAddMapper(_logger, state, false).Map();

            state.SerializedInstance = state.SerializedInstance + $"{Environment.NewLine}Provider {Environment.NewLine},{Serialization.DataContractSerialize(state.ProxyAddToVistaRequest)}";
        }
    }
}
