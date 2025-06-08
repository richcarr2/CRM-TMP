using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Processor;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Models
{
    public class FindAvailableTimesHandler
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private IOptions<ApplicationSettings> _settings;

        public FindAvailableTimesHandler(ILogger logger, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            _logger = logger;
            _settings = settings;
        }

        public TelehealthSpecialtyLocationsFindAvailableTimesResponse HandleRequestResponse(TelehealthSpecialtyLocationsFindAvailableTimesRequest message)
        {
            var processor = new FindAvailableTimesProcessor(_logger, _settings, _config);

            try
            {
                var resp = processor.Execute(message);

                return resp.Result;
            }
            catch (System.Exception e)
            {
                return new TelehealthSpecialtyLocationsFindAvailableTimesResponse { ErrorMessage = e.Message, ErrorOccurred = true, DebugInfo = e.ToString() };
            }
        }

        public async Task<TelehealthSpecialtyLocationsFindAvailableTimesResponse> HandleRequestResponseAsync(TelehealthSpecialtyLocationsFindAvailableTimesRequest message)
        {
            var processor = new FindAvailableTimesProcessor(_logger, _settings, _config);

            try
            {
                var resp = await processor.Execute(message);

                return resp;
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception e)
            {
                return new TelehealthSpecialtyLocationsFindAvailableTimesResponse { ErrorMessage = e.Message, ErrorOccurred = true, DebugInfo = e.ToString() };
            }
        }
    }
}
