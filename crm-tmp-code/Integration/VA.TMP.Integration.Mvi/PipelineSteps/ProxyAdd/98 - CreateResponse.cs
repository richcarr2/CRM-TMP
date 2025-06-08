using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Create Response step.
    /// </summary>
    public class CreateResponseStep : IFilter<ProxyAddStateObject>
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
        public void Execute(ProxyAddStateObject state)
        {
            if (!string.IsNullOrEmpty(state.FakeResponseType))
            {
                state.ProxyAddResponseMessage = new ProxyAddResponseMessage
                {
                    ExceptionMessage = string.Format("MVI Fake Response Type {0}, Exception: {1}", state.FakeResponseType, state.ExceptionMessage),
                    ExceptionOccured = state.ExceptionOccured,
                    SerializedInstance = state.SerializedInstance
                };
            }
            else
            {
                state.ProxyAddResponseMessage = state.ExceptionOccured
                    ? new ProxyAddResponseMessage { ExceptionOccured = true, ExceptionMessage = state.ExceptionMessage, EcProcessingMs = state.EcProcessingTimeMs }
                    : new ProxyAddResponseMessage
                    {
                        ExceptionMessage = string.Empty,
                        ExceptionOccured = false,
                        SerializedInstance = state.SerializedInstance,
                        EcProcessingMs = state.EcProcessingTimeMs
                    };
            }
        }
    }
}