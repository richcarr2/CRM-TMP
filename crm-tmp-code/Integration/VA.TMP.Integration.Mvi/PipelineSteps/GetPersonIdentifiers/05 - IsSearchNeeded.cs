using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.GetPersonIdentifiers
{
    /// <summary>
    /// Is Search needed step.
    /// </summary>
    public class IsSearchNeededStep : IFilter<GetPersonIdentifiersStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public IsSearchNeededStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetPersonIdentifiersStateObject state)
        {
            if (state.CorrespondingIds == null || state.CorrespondingIds.Count == 0) state.IsSearchNeeded = true;
            else state.IsSearchNeeded = false;
        }
    }
}