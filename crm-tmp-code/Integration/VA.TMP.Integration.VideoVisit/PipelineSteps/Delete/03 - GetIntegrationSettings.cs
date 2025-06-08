using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    /// <summary>
    /// Get Integration Settings Step
    /// </summary>
    public class GetIntegrationSettingsStep : IFilter<VideoVisitDeleteStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetIntegrationSettingsStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitDeleteStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                var useFakeResponse = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "Vista Fake Response Type");
                state.VistaFakeResponseType = useFakeResponse.mcs_value;
            }
        }
    }
}
