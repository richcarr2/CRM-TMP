using System;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Vista;
using VEIS.Messages.VIAScheduling;

namespace VA.TMP.Integration.Vista.StateObject
{
    public class ViaLoginStateObject : PipeState
    {
        public ViaLoginStateObject(ViaLoginRequestMessage request)
        {
            UserId = request.UserId;
            OrganizationName = request.OrganizationName;
            LoginRequest = request;
            EcProcessingTimeMs = 0;
        }

        public string UserDuz { get; set; }

        public string OrganizationName { get; set; }

        public string FakeResponseType { get; set; }

        public string ConsumingAppToken { get; set; }

        public string ConsumingAppPassword { get; set; }

        public Guid UserId { get; set; }

        public VEISVIAScheLIloginVIARequest EcRequest { get; set; }

        public VEISVIAScheLIloginVIAResponse EcResponse { get; set; }

        public ViaLoginRequestMessage LoginRequest { get; set; }

        public ViaLoginResponseMessage LoginResponse { get; set; }

        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        public string SerializedInstance { get; set; }

        public int EcProcessingTimeMs { get; set; }
    }
}
