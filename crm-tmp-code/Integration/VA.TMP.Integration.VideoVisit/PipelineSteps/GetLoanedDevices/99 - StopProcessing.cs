using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.GetLoanedDevices
{
    /// <summary>
    /// Stop Processing step.
    /// </summary>
    public class StopProcessingStep : IFilter<VideoVisitGetLoanedDevicesStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public StopProcessingStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitGetLoanedDevicesStateObject state)
        {
        }
    }
}