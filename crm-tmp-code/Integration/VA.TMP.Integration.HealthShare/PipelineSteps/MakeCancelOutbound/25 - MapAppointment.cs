using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.Mappers;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Map Appointment step.
    /// </summary>
    public class MapAppointmentStep : IFilter<MakeCancelOutboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelOutboundStateObject state)
        {
            foreach (var patient in state.RequestMessage.Patients)
            {
                var ecRequestMessage = new MakeCancelOutboundEcRequestMapper(state, patient, _logger).Map();

                if (ecRequestMessage == null) throw new MissingMakeCancelRequest("HealthShare Make/Cancel Enterprise Component Request Message cannot be null");

                state.EcRequestMessages.Add(ecRequestMessage);
                state.ResponseMessage.PatientIntegrationResultInformation.Add(new PatientIntegrationResultInformation { ControlId = ecRequestMessage.ControlId, PatientId = patient });
            }
        }
    }
}