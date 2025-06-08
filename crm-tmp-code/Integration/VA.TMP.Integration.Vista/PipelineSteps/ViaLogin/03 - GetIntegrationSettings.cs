using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Vista.StateObject;

namespace VA.TMP.Integration.Vista.PipelineSteps.ViaLogin
{
    public class GetIntegrationSettingsStep : IFilter<ViaLoginStateObject>
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

        public void Execute(ViaLoginStateObject state)
        {
            var fakeResponseString = Strings.ViaLoginFakeResponseType;

            using (var _service = new Xrm(state.OrganizationServiceProxy))
            {
                var via_fakeresponse = _service.mcs_integrationsettingSet.FirstOrDefault(i => i.mcs_name == fakeResponseString);
                if (via_fakeresponse == null)
                {
                    state.FakeResponseType = string.Empty;
                }
                else
                {
                    state.FakeResponseType = via_fakeresponse.mcs_value;
                }
            }
        }
    }
}
