using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Processors;
using Ec.VideoVisit.Services.Rest;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.VideoVisit.MessageHandler
{
    public class EcTmpGetLoanedDevicesMessageHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public EcTmpGetLoanedDevicesMessageHandler(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        public EcTmpGetLoanedDevicesResponse HandleRequestResponse(EcTmpGetLoanedDevicesRequest message)
        {
            var processor = new EcTmpGetLoanedDevicesProcessor(_logger, _settings, _serviceFactory);
            return processor.Execute(message);
        }
    }
}