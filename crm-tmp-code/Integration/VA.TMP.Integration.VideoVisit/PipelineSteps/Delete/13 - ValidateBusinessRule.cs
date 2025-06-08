using Ec.VideoVisit.Messages;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VideoVisit.Helpers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    public class ValidateBusinessRuleStep : IFilter<VideoVisitDeleteStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ValidateBusinessRuleStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates the business rules before sending the Ec Request to the EC so that no validation errors are expected from AFS side
        /// </summary>
        /// <param name="state"></param>
        public void Execute(VideoVisitDeleteStateObject state)
        {
            var errors = ValidateBusinessRules(state.CancelAppointmentRequest);
            if (!string.IsNullOrEmpty(errors))
            {
                throw new VvsBusinessRulesException(errors);
            }
        }

        private string ValidateBusinessRules(EcTmpCancelAppointmentRequest ecRequest)
        {
            var validator = new BusinessRuleValidator();
            return validator.ValidateCancelRequest(ecRequest);
        }
    }
}