using System.Net.Http;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Services.XSD;

namespace Ec.VideoVisit.Services.Rest
{
    public interface IServiceFactory
    {
        writeResults CancelAppointment(cancelAppointmentRequest payload, string samlToken);

        writeResults CreateAppointment(appointment payload, string samlToken);

        EcTmpGetLoanedDevicesResponse GetLoanedDevices(EcTmpGetLoanedDevicesRequest request);

        writeResults HandleHttpResponse(HttpResponseMessage response, string reqType);

        writeResults UpdateAppointment(appointment payload, string samlToken);
    }
}