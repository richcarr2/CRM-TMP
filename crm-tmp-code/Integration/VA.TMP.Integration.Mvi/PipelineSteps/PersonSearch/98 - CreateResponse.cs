using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.Mappers;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.PersonSearch
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<PersonSearchStateObject>
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
        public void Execute(PersonSearchStateObject state)
        {
            var mapper = new PersonSearchMapper(state);
            if (!string.IsNullOrEmpty(state.PersonSearchFakeResponseType))
            {
                state.PersonSearchResponseMessage = new PersonSearchResponseMessage
                {
                    ExceptionMessage = "*****THIS IS FAKE DATA. DATA NOT SENT TO MVI*****",
                    ExceptionOccured = false,
                    SerializedInstance = string.Empty,
                    RetrieveOrSearchPersonResponse = mapper.MapEcToLob(state.RetrieveOrSearchPersonResponse)
                };
            }
            else
            {
                state.PersonSearchResponseMessage = state.ExceptionOccured
                    ? new PersonSearchResponseMessage { ExceptionOccured = true, ExceptionMessage = state.ExceptionMessage, EcProcessingMs = state.EcProcessingTimeMs }
                    : new PersonSearchResponseMessage
                    {
                        ExceptionMessage = string.Empty,
                        ExceptionOccured = false,
                        SerializedInstance = string.Empty,
                        RetrieveOrSearchPersonResponse = mapper.MapEcToLob(state.RetrieveOrSearchPersonResponse),
                        EcProcessingMs = state.EcProcessingTimeMs
                    };
            }
        }
    }
}