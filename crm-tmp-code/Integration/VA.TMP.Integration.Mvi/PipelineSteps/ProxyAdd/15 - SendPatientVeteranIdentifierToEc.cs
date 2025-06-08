using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Services;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Send Patient Veteran to Proxy Add Request Enterprise Component step.
    /// </summary>
    public class SendPatientVeteranIdentifierToEcStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;
        private readonly IProxyAddService _proxyAddService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SendPatientVeteranIdentifierToEcStep(ILog logger, IProxyAddService proxyAddService)
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
            if (string.IsNullOrEmpty(state.PatientSite) || state.PatientSideIdentifierToAdd == null || state.Veteran == null) return;

            _proxyAddService.SendIdentifierToProxyAddEc(ref state, true);
        }
    }
}
