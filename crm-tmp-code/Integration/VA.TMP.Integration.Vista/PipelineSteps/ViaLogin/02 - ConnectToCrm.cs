using log4net;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Vista.StateObject;

namespace VA.TMP.Integration.Vista.PipelineSteps.ViaLogin
{
    public class ConnectToCrmStep : IFilter<ViaLoginStateObject>
    {
        private readonly ILog _logger;
        private readonly ITmpContext _tmpContext;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ConnectToCrmStep(ILog logger, ITmpContext tmpContext)
        {
            _logger = logger;
            _tmpContext = tmpContext;
        }

        public void Execute(ViaLoginStateObject state)
        {
            state.OrganizationServiceProxy = _tmpContext.GetOrganizationServiceProxy();
        }
    }
}