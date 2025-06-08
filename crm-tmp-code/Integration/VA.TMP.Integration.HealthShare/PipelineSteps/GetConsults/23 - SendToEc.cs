using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ec.HealthShare.Messages;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Rest.Interface;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.GetConsults
{
    public class SendToEcStep : IFilter<GetConsultsStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcConsultsUri => _settings.Items.First(x => x.Key == "EcConsultsUri").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SendToEcStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        public void Execute(GetConsultsStateObject state)
        {
            if (!string.IsNullOrEmpty(state.ConsultsFakeResponseType))
            {
                _logger.Info("Currently Fakes is turned ON. Refer to 'Vista - Get Consult Fake Response Type' under integration settings. " + "Returning with the fake response instead of HealthShare REST call");
                var fakeEcResponse = HealthShareFakeResponses.FakeGetConsultsForPatientSuccess();
                if (state.PatEcRequestMessage != null) state.PatEcResponseMessage = fakeEcResponse;
                if (state.ProEcRequestMessage != null) state.ProEcResponseMessage = fakeEcResponse;
            }
            else
            {
                _logger.Info("Currently Fakes is turned OFF. Initiating the HealthShare Get Consults for Patients REST call");
                const string providerKey = "Pro";
                const string patientKey = "Pat";

                var requestKvps = new List<KeyValuePair<string, EcHealthShareGetConsultsRequest>>
                    {
                        new KeyValuePair<string, EcHealthShareGetConsultsRequest>(patientKey, state.PatEcRequestMessage),
                        new KeyValuePair<string, EcHealthShareGetConsultsRequest>(providerKey, state.ProEcRequestMessage)
                    };

                var timer = new Stopwatch();
                timer.Start();

                Parallel.ForEach(requestKvps, request =>
                {
                    if (request.Key == patientKey && state.PatEcRequestMessage != null) state.PatEcResponseMessage =
                        _servicePost.PostToEc<EcHealthShareGetConsultsRequest, EcHealthShareGetConsultsResponse>(
                            "HealthShare Get Consults", EcConsultsUri, _settings, state.PatEcRequestMessage).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (request.Key == providerKey && state.ProEcRequestMessage != null) state.ProEcResponseMessage =
                        _servicePost.PostToEc<EcHealthShareGetConsultsRequest, EcHealthShareGetConsultsResponse>(
                            "HealthShare Get Consults", EcConsultsUri, _settings, state.ProEcRequestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
                });

                timer.Stop();
                state.EcProcessingTimeMs += (int)timer.ElapsedMilliseconds;
            }
        }
    }
}
