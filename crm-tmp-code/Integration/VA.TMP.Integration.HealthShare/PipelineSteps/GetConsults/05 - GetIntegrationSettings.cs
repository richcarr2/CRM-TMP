using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.GetConsults
{
    /// <summary>
    /// Get integration settings step.
    /// </summary>
    public class GetIntegrationSettingsStep : IFilter<GetConsultsStateObject>
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
        public void Execute(GetConsultsStateObject state)
        {
            const string fakeResponseString = "Vista - Get Consult Fake Response Type";

            using (var service = new Xrm(state.OrganizationServiceProxy))
            {
                var healthShareFakeResponse = service.mcs_integrationsettingSet.FirstOrDefault(i => i.mcs_name == fakeResponseString);
                state.ConsultsFakeResponseType = healthShareFakeResponse == null ? string.Empty : healthShareFakeResponse.mcs_value;
            }
        }
    }
}
