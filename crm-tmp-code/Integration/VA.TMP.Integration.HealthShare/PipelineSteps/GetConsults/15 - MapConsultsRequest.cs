using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.Mappers;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.GetConsults
{
    /// <summary>
    /// Map Consult LOB to EC step.
    /// </summary>
    public class MapConsultsRequestStep : IFilter<GetConsultsStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapConsultsRequestStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetConsultsStateObject state)
        {
            if (state.IsHomeMobile && state.RequestMessage.ProviderLoginStationNumber == 0) throw new MissingStationNumberException("ProviderLoginStationNumber cannot be 0 for a Home/Mobile appointment");
            if (state.IsStoreForward && state.RequestMessage.PatientLoginStationNumber == 0) throw new MissingStationNumberException("PatientLoginStationNumber cannot be 0 for a Store/Forward appointment");

            if (!state.IsStoreForward && !state.IsHomeMobile && (state.RequestMessage.ProviderLoginStationNumber == 0 || state.RequestMessage.PatientLoginStationNumber == 0)) throw new MissingStationNumberException("Clinic and Group appointments require both Patient/Provider Station Number");

            var mapper = new GetConsultsLobEcMapper();

            //if (!state.IsHomeMobile && state.RequestMessage.PatientLoginStationNumber != 0) state.PatEcRequestMessage = mapper.Map
            if (state.RequestMessage.PatientLoginStationNumber != 0) state.PatEcRequestMessage = mapper.Map(state.RequestMessage, state.PatientRequestUniqueId, true, state.OrganizationServiceProxy, _logger);
            else _logger.Info("Skipping Patient Side mapping for GetConsults");

            if (state.RequestMessage.ProviderLoginStationNumber != 0) state.ProEcRequestMessage = mapper.Map(state.RequestMessage, state.ProviderRequestUniqueId, false, state.OrganizationServiceProxy, _logger);
            else _logger.Info("Skipping Provider Side mapping for GetConsults");
        }
    }
}
