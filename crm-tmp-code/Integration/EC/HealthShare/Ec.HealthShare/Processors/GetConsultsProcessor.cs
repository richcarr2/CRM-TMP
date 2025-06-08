using Ec.HealthShare.Messages;
using Ec.HealthShare.Services.Rest;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.HealthShare.Processors
{
    /// <summary>
    ///  HealthShare Get Consults EC Processor.
    /// </summary>
    public class GetConsultsProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public GetConsultsProcessor(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <param name="request">EcHealthShareGetConsultsRequest.</param>
        /// <returns>EcHealthShareGetConsultsResponse.</returns>
        public EcHealthShareGetConsultsResponse Execute(EcHealthShareGetConsultsRequest request)
        {
            _logger.Info("Processing EC Get Consults request");

            return _serviceFactory.GetConsults(request);
        }
    }
}