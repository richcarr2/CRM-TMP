using System;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Get Integration Settings step.
    /// </summary>
    public class GetIntegrationSettingsStep : IFilter<MakeCancelOutboundStateObject>
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
        public void Execute(MakeCancelOutboundStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var useFakeResponse = srv.mcs_integrationsettingSet.First(x => x.mcs_name == "HealthShare MakeCancel Outbound Use Fake").mcs_value;
                state.UseFakeResponse = Convert.ToBoolean(useFakeResponse);
            }
        }
    }
}
