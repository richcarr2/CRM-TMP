using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Mappers;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.GetConsults
{
    /// <summary>
    /// Map Consult EC to LOB step.
    /// </summary>
    public class MapConsultsResponseStep : IFilter<GetConsultsStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapConsultsResponseStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetConsultsStateObject state)
        {
            var mapper = new GetConsultsEcLobMapper();

            if (!string.IsNullOrEmpty(state.ConsultsFakeResponseType))
            {
                const string message = "*****THIS IS FAKE DATA. DATA NOT SENT TO HEALTHSHARE*****";

                if (state.PatEcResponseMessage != null) state.ResponseMessage = mapper.Map(state.PatEcResponseMessage, true, state.ResponseMessage);
                if (state.ProEcResponseMessage != null) state.ResponseMessage = mapper.Map(state.ProEcResponseMessage, false, state.ResponseMessage);

                state.ResponseMessage.ExceptionMessage = message;
                state.ResponseMessage.ExceptionOccured = false;
            }
            else
            {
                if (state.ExceptionOccured)
                {
                    state.ResponseMessage = new TmpHealthShareGetConsultsResponse
                    {
                        ExceptionOccured = true,
                        ExceptionMessage = state.ExceptionMessage,
                        EcProcessingMs = state.EcProcessingTimeMs
                    };
                }
                else
                {
                    if (state.PatEcResponseMessage != null)
                    {
                        state.ResponseMessage = mapper.Map(state.PatEcResponseMessage, true, state.ResponseMessage);
                        _logger.Info("Mapping Patient Side Response");
                        _logger.Info($"Patient Side has {state.ResponseMessage.PatientReturnToClinicOrders.Count} Patient RTCs");
                        _logger.Info($"Patient Side has {state.ResponseMessage.ProviderReturnToClinicOrders.Count} Provider RTCs");
                    }

                    if (state.ProEcResponseMessage != null)
                    {
                        state.ResponseMessage = mapper.Map(state.ProEcResponseMessage, false, state.ResponseMessage);
                        _logger.Info("Mapping Provider Side Response");
                        _logger.Info($"Provider Side has {state.ResponseMessage.ProviderReturnToClinicOrders.Count} Provider RTCs");
                        _logger.Info($"Provider Side has {state.ResponseMessage.PatientReturnToClinicOrders.Count} Patient RTCs");
                    }

                    state.ResponseMessage.ExceptionMessage = string.Empty;
                    state.ResponseMessage.ExceptionOccured = false;
                    state.ResponseMessage.EcProcessingMs = state.EcProcessingTimeMs;

                    _logger.Info($"Get Consults Response: {Serialization.DataContractSerialize(state.ResponseMessage)}");
                }
            }
        }
    }
}