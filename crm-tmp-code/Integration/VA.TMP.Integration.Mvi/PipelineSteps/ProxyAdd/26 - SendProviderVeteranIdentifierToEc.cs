using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Services;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Send Provider Veteran Identifier to EC step.
    /// </summary>
    public class SendProviderVeteranIdentifierToEcStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;
        private readonly IProxyAddService _proxyAddService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SendProviderVeteranIdentifierToEcStep(ILog logger, IProxyAddService proxyAddService)
        {
            _logger = logger;
            _proxyAddService = proxyAddService;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            if (string.IsNullOrEmpty(state.ProviderSite) || state.ProviderSideIdentifierToAdd == null || state.PatientAndProviderSitesAreEqual || state.ExceptionOccured || state.Veteran == null) return;

            _proxyAddService.SendIdentifierToProxyAddEc(ref state, false);
        }
    }
}
