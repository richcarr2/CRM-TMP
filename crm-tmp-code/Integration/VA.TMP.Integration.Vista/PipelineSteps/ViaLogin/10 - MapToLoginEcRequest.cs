using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Vista.Mappers;
using VA.TMP.Integration.Vista.StateObject;
using VEIS.Messages.VIAScheduling;

namespace VA.TMP.Integration.Vista.PipelineSteps.ViaLogin
{
    public class MapToLoginEcRequestStep : IFilter<ViaLoginStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapToLoginEcRequestStep(ILog logger)
        {
            _logger = logger;
        }

        public void Execute(ViaLoginStateObject state)
        {
            var queryBeanSettings = VistaMapperHelper.GetSecuritySettings(state.OrganizationServiceProxy);
            var request = new VEISVIAScheLIloginVIARequest
            {
                mcs_sitecode = state.LoginRequest.StationNumber,
                VEISVIAScheLIReqqueryBeanInfo = new VEISVIAScheLIReqqueryBean
                {
                    mcs_recordSiteCode = state.LoginRequest.StationNumber,
                    mcs_consumingAppToken = queryBeanSettings.ConsumingAppToken,
                    mcs_consumingAppPassword = queryBeanSettings.ConsumingAppPassword,
                    mcs_requestingApp = queryBeanSettings.RequestingApp
                },
                OrganizationName = state.OrganizationName,
                UserId = state.UserId,
                Debug = true,
                LogSoap = true

            };
            if (!string.IsNullOrEmpty(state.LoginRequest.AccessCode) && !string.IsNullOrEmpty(state.LoginRequest.VerifyCode))
            {
                request.mcs_accesscode = state.LoginRequest.AccessCode;
                request.mcs_verifycode = state.LoginRequest.VerifyCode;
            }
            else
            {
                request.VEISVIAScheLIReqqueryBeanInfo.mcs_criteria = state.LoginRequest.SamlToken;
            }
            state.EcRequest = request;
        }
    }
}
