using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Processors;
using Ec.VideoVisit.Services.Rest;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.VideoVisit.MessageHandler
{
    public class EcTmpCreateAppointmentMessageHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public EcTmpCreateAppointmentMessageHandler(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        public EcTmpCreateAppointmentResponse HandleRequestResponse(EcTmpCreateAppointmentRequest message)
        {
            var processor = new EcTmpCreateAppointmentProcessor(_logger, _settings, _serviceFactory);
            return processor.Execute(message);
        }
    }
}