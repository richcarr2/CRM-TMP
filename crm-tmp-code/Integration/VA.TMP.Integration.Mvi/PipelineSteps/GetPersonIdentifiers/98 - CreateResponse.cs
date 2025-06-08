using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.GetPersonIdentifiers
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<GetPersonIdentifiersStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CreateResponseStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetPersonIdentifiersStateObject state)
        {
            if (!string.IsNullOrEmpty(state.SelectedPersonFakeResponseType))
            {
                state.GetPersonIdentifiersResponseMessage.ExceptionMessage = "*****THIS IS FAKE DATA. DATA NOT SENT TO MVI*****";
                state.GetPersonIdentifiersResponseMessage.SerializedInstance = string.Empty;
            }
            else
            {
                state.GetPersonIdentifiersResponseMessage = state.ExceptionOccured
                    ? new GetPersonIdentifiersResponseMessage { ExceptionOccured = true, ExceptionMessage = state.ExceptionMessage, EcProcessingMs = state.EcProcessingTimeMs }
                    : state.GetPersonIdentifiersResponseMessage;
            }
        }
    }
}