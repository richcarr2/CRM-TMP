using log4net;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    /// <summary>
    /// Connect to CRM step.
    /// </summary>
    public class ConnectToCrmStep : IFilter<VideoVisitDeleteStateObject>
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

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitDeleteStateObject state)
        {
            state.OrganizationServiceProxy = _tmpContext.GetOrganizationServiceProxy();
        }
    }
}