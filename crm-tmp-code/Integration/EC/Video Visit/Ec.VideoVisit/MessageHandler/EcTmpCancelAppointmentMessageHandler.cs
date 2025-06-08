using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Processors;
using Ec.VideoVisit.Services.Rest;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.VideoVisit.MessageHandler
{
    public class EcTmpCancelAppointmentMeetingMessageHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public EcTmpCancelAppointmentMeetingMessageHandler(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        public EcTmpCancelAppointmentResponse HandleRequestResponse(EcTmpCancelAppointmentRequest message)
        {
            var processor = new EcTmpCancelAppointmentProcessor(_logger, _settings, _serviceFactory);
            return processor.Execute(message);
        }
    }
}