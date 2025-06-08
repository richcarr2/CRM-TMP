using System;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Mappers;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Map Patient Veteran to Proxy Add Request Enterprise Component.
    /// </summary>
    public class MapPatientVeteranIdentifierToProxyAddRequestEcStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapPatientVeteranIdentifierToProxyAddRequestEcStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            if (string.IsNullOrEmpty(state.PatientSite) || state.PatientSideIdentifierToAdd == null || state.Veteran == null) return;

            state.ProxyAddToVistaRequest = new ProxyAddMapper(_logger, state, true).Map();

            state.SerializedInstance = $"Patient {Environment.NewLine},{Serialization.DataContractSerialize(state.ProxyAddToVistaRequest)}";
        }
    }
}
