using System;
using System.Diagnostics;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Services.Rest;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;

namespace Ec.VideoVisit.Processors
{
    public class EcTmpGetLoanedDevicesProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        public EcTmpGetLoanedDevicesProcessor(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        public EcTmpGetLoanedDevicesResponse Execute(EcTmpGetLoanedDevicesRequest request)
        {
            _logger.Info("Starting VVS GetLoanedDevices");

            _logger.Debug($"Request payload: { Serialization.DataContractSerialize(request)}");

            var thisTimer = Stopwatch.StartNew();

            if (string.IsNullOrEmpty(request.ICN))
            {
                throw new ArgumentNullException("ICN", "VVS GetLoanedDevice request is missing ICN");
            }

            if (string.IsNullOrEmpty(request.SamlToken))
            {
                throw new ArgumentNullException("SamlToken", "VVS GetLoanedDevice request is missing SamlToken");
            }

            var response = _serviceFactory.GetLoanedDevices(request);

            thisTimer.Stop();
            _logger.Info($"Calling VVS GetLoanedDevices took {thisTimer.ElapsedMilliseconds} ms");

            return response;
        }
    }
}
