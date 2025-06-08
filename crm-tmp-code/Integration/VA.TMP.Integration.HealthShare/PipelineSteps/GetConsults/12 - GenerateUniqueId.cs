using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.GetConsults
{
    /// <summary>
    /// Generate Unique Id step.
    /// </summary>
    public class GenerateUniqueIdStep : IFilter<GetConsultsStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GenerateUniqueIdStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetConsultsStateObject state)
        {
            state.PatientRequestUniqueId = RandomDigits.GetRandomDigitString(20);
            state.ProviderRequestUniqueId = RandomDigits.GetRandomDigitString(20);
        }
    }
}