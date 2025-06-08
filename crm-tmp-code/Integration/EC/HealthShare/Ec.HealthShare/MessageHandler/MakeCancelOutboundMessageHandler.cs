using Ec.HealthShare.Messages;
using Ec.HealthShare.Processors;
using Ec.HealthShare.Services.Rest;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.HealthShare.MessageHandler
{
    /// <summary>
    /// HealthShare Make Cancel Outbound EC Message Handler.
    /// </summary>
    public class MakeCancelOutboundMessageHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public MakeCancelOutboundMessageHandler(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="message">EcHealthShareMakeCancelOutboundRequestMessage.</param>
        /// <returns>EcHealthShareMakeCancelOutboundResponseMessage.</returns>
        public EcHealthShareMakeCancelOutboundResponseMessage HandleRequestResponse(EcHealthShareMakeCancelOutboundRequestMessage message)
        {
            var processor = new MakeCancelOutboundProcessor(_logger, _settings, _serviceFactory);
            return processor.Execute(message);
        }
    }
}