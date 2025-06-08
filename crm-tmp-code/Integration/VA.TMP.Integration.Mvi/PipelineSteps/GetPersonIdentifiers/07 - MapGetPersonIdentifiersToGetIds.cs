using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Mappers;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.GetPersonIdentifiers
{
    /// <summary>
    /// Map Get Person Identifiers to GetIds step.
    /// </summary>
    public class MapGetPersonIdentifiersToGetIdsStep : IFilter<GetPersonIdentifiersStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapGetPersonIdentifiersToGetIdsStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetPersonIdentifiersStateObject state)
        {
            state.SelectedPersonRequest = new SelectedPersonRequestMapper().Map(state);
        }
    }
}