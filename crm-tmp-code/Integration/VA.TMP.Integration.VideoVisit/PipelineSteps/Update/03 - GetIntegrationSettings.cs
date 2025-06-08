using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    /// <summary>
    /// Get Integration Settings step.
    /// </summary>
    public class GetIntegrationSettingsStep : IFilter<VideoVisitUpdateStateObject>
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
        public void Execute(VideoVisitUpdateStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                var useFakeResponse = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "Vista Fake Response Type");
                state.FakeResponseType = useFakeResponse == null ? string.Empty : useFakeResponse.mcs_value;
            }
        }
    }
}
