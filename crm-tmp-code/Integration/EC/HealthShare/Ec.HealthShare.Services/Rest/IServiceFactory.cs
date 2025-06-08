using Ec.HealthShare.Messages;

namespace Ec.HealthShare.Services.Rest
{
    public interface IServiceFactory
    {
        EcHealthShareGetConsultsResponse GetConsults(EcHealthShareGetConsultsRequest request);

        EcHealthShareMakeCancelOutboundResponseMessage MakeCancelOutbound(EcHealthShareMakeCancelOutboundRequestMessage request);
    }
}