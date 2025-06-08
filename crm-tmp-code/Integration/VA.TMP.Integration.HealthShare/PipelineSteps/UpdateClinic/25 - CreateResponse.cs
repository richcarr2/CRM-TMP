using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.UpdateClinic
{
    /// <summary>
    /// Create response step.
    /// </summary>
    public class CreateResponseStep : IFilter<UpdateClinicStateObject>
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
        public void Execute(UpdateClinicStateObject state)
        {
            state.ResponseMessage = new TmpHealthShareUpdateClinicResponseMessage { ExceptionOccured = false };
        }
    }
}