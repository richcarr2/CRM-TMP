using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.GetLoanedDevices
{
    /// <summary>
    /// Get Integration Settings step.
    /// </summary>
    public class GetIntegrationSettingsStep : IFilter<VideoVisitGetLoanedDevicesStateObject>
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
        public void Execute(VideoVisitGetLoanedDevicesStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                var fakeType = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "Vista Fake Response Type");
                state.VistaFakeResponseType = fakeType != null ? fakeType.mcs_value : string.Empty;
            }
        }
    }
}
