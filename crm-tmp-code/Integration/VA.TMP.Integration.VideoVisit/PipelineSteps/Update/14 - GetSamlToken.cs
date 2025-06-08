using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    /// <summary>
    /// Get SAML Token.
    /// </summary>
    public class GetSamlTokenStep : IFilter<VideoVisitUpdateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetSamlTokenStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitUpdateStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var currentUser = srv.SystemUserSet.FirstOrDefault(x => x.Id == state.UserId);

                if (currentUser == null) throw new SamlTokenException(string.Format("Unable to retrieve user {0} for updating a Video Visit.", state.UserId));
                if (string.IsNullOrEmpty(currentUser.cvt_SAMLToken)) throw new SamlTokenException(string.Format("Unable to retrieve SAML token for user {0} for updating a Video Visit.", state.UserId));

                state.SamlToken = currentUser.cvt_SAMLToken;
            }
        }
    }
}
