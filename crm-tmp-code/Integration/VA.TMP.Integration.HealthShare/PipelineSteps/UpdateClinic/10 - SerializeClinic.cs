using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.UpdateClinic
{
    /// <summary>
    /// Serialize Clinic step.
    /// </summary>
    public class SerializeClinicStep : IFilter<UpdateClinicStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SerializeClinicStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(UpdateClinicStateObject state)
        {
            state.SerializedRequestMessage = Serialization.DataContractSerialize(state.RequestMessage);
            _logger.Info($"INFO: HealthShare Update Clinic Data: {state.SerializedRequestMessage}");
        }
    }
}