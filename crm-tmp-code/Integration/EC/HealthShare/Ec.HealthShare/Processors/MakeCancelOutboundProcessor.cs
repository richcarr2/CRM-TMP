using Ec.HealthShare.Messages;
using Ec.HealthShare.Services.Rest;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.HealthShare.Processors
{
    /// <summary>
    ///  HealthShare Make Cancel Outbound EC Processor.
    /// </summary>
    public class MakeCancelOutboundProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public MakeCancelOutboundProcessor(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        /// <summary>
        /// Process the request.
        /// </summary>
        /// <param name="request">EcHealthShareMakeCancelOutboundRequestMessage.</param>
        /// <returns>EcHealthShareMakeCancelOutboundResponseMessage.</returns>
        public EcHealthShareMakeCancelOutboundResponseMessage Execute(EcHealthShareMakeCancelOutboundRequestMessage request)
        {
            _logger.Info("Processing EC Make Cancel Outbound request");

            return _serviceFactory.MakeCancelOutbound(request);
        }
    }
}