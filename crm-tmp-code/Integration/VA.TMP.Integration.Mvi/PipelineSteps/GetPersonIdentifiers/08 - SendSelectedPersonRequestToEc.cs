using System.Diagnostics;
using System.Linq;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Helpers;
using VA.TMP.Integration.Mvi.Mappers;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.Integration.Rest.Interface;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.PipelineSteps.GetPersonIdentifiers
{
    /// <summary>
    /// Send Selected Person Request to EC step.
    /// </summary>
    public class SendSelectedPersonRequestToEcStep : IFilter<GetPersonIdentifiersStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcGetPersonIdentifiersUri => _settings.Items.First(x => x.Key == "EcGetPersonIdentifiersUri").Value;

        private string MviOrgName => _settings.Items.First(x => x.Key == "MviOrgName").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SendSelectedPersonRequestToEcStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetPersonIdentifiersStateObject state)
        {
            state.SelectedPersonRequest.OrganizationName = MviOrgName;

            if (state.IsSearchNeeded)
            {

                CorrespondingIdsResponse response;
                switch (state.SelectedPersonFakeResponseType)
                {
                    case "0":
                        response = PersonSearchUtilities.FakeSelectedPersonResponse(state, _logger);
                        break;

                    default:
                        var timer = new Stopwatch();
                        timer.Start();

                        response = _servicePost.PostToEc<SelectedPersonRequest, CorrespondingIdsResponse>(
                            "MVI Selected Person Request", EcGetPersonIdentifiersUri, _settings, state.SelectedPersonRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                        timer.Stop();
                        state.EcProcessingTimeMs += (int)timer.ElapsedMilliseconds;
                        break;
                }

                if (string.IsNullOrEmpty(state.SelectedPersonRequest.SocialSecurityNumber) && string.IsNullOrEmpty(state.Ss)) state.Ss = response.SocialSecurityNumber;
                state.CorrespondingIdsResponse = response;

                foreach (var id in response.CorrespondingIdList)
                {
                    state.CorrespondingIds.Add(MapGetPersonIdentifiersRequestToContact.Map(id));
                }
            }
            else
            {
                _logger.Debug("Already Have Identifiers from EDIPI search, skipping send to EC");
            }
        }
    }
}
