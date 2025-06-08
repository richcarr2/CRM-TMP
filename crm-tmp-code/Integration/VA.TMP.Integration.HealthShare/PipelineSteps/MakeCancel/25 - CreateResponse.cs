using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancel
{
    /// <summary>
    /// Create response step.
    /// </summary>
    public class CreateResponseStep : IFilter<MakeCancelStateObject>
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
        public void Execute(MakeCancelStateObject state)
        {
            state.ResponseMessage = new TmpHealthShareMakeAndCancelAppointmentResponseMessage { ExceptionOccured = false };
        }
    }
}