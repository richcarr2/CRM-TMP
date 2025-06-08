using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancel
{
    /// <summary>
    /// Deserialize Appointment step.
    /// </summary>
    public class SerializeAppointmentStep : IFilter<MakeCancelStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SerializeAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelStateObject state)
        {
            state.SerializedRequestMessage = Serialization.DataContractSerialize(state.RequestMessage);
            _logger.Info($"INFO: HealthShare Make and Cancel Appointment Data: {state.SerializedRequestMessage}");
        }
    }
}