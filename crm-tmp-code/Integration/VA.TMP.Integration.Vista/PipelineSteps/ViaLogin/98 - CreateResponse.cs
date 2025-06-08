using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Messages.Vista;
using VA.TMP.Integration.Vista.StateObject;

namespace VA.TMP.Integration.Vista.PipelineSteps.ViaLogin
{
    public class CreateResponseStep : IFilter<ViaLoginStateObject>
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

        public void Execute(ViaLoginStateObject state)
        {
            if (state.EcResponse == null) throw new MissingViaResponseException("No EC Response was returned");
            var failure = state.EcResponse.ExceptionOccurred || state.EcResponse.VEISVIAScheLIuserTOInfo?.VEISVIAScheLIfault2Info != null;

            _logger.Info($"Request: {state.EcResponse.SerializedSOAPRequest}");
            _logger.Info($"Response: {state.EcResponse.SerializedSOAPResponse}");

            state.LoginResponse = new ViaLoginResponseMessage
            {
                ExceptionMessage = failure ? state.EcResponse?.ExceptionMessage ?? state.EcResponse.VEISVIAScheLIuserTOInfo.VEISVIAScheLIfault2Info.mcs_message : string.IsNullOrEmpty(state.FakeResponseType) ? "" : "*****THIS IS FAKE DATA. DATA NOT SENT TO Vista*****",
                ExceptionOccured = failure,
                SerializedInstance = state.SerializedInstance,
                UserDuz = failure ? null : state.EcResponse.VEISVIAScheLIuserTOInfo.mcs_DUZ,
                EcProcessingMs = state.EcProcessingTimeMs
            };
        }
    }
}