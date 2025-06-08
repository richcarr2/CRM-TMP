using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Vista.StateObject;

namespace VA.TMP.Integration.Vista.PipelineSteps.ViaLogin
{
    public class SerializeInstanceStep : IFilter<ViaLoginStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SerializeInstanceStep(ILog logger)
        {
            _logger = logger;
        }

        public void Execute(ViaLoginStateObject state)
        {
            state.SerializedInstance = state.EcRequest != null ? Serialization.DataContractSerialize(state.EcRequest) : string.Empty;
        }
    }
}