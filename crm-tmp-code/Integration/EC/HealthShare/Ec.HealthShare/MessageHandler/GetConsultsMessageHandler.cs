using Ec.HealthShare.Messages;
using Ec.HealthShare.Processors;
using Ec.HealthShare.Services.Rest;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.HealthShare.MessageHandler
{
    /// <summary>
    /// HealthShare Get Consults EC Message Handler.
    /// </summary>
    public class GetConsultsMessageHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public GetConsultsMessageHandler(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        /// <summary>
        /// Handle the request.
        /// </summary>
        /// <param name="message">EcHealthShareGetConsultsRequest.</param>
        /// <returns>EcHealthShareGetConsultsResponse.</returns>
        public EcHealthShareGetConsultsResponse HandleRequestResponse(EcHealthShareGetConsultsRequest message)
        {
            var processor = new GetConsultsProcessor(_logger, _settings, _serviceFactory);
            return processor.Execute(message);
        }
    }
}