using log4net;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    /// <summary>
    /// Connect to CRM step.
    /// </summary>
    public class ConnectToCrmStep : IFilter<VideoVisitUpdateStateObject>
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
        public void Execute(VideoVisitUpdateStateObject state)
        {
            state.OrganizationServiceProxy = _tmpContext.GetOrganizationServiceProxy();
        }
    }
}