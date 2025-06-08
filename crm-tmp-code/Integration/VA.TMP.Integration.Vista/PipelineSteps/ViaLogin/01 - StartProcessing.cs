using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Vista.StateObject;

namespace VA.TMP.Integration.Vista.PipelineSteps.ViaLogin
{
    public class StartProcessingStep : IFilter<ViaLoginStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public StartProcessingStep(ILog logger)
        {
            _logger = logger;
        }

        public void Execute(ViaLoginStateObject state)
        {
        }
    }
}