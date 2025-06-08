using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Processor;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.MessageHandler
{
    public class FindAvailableTimesHandler
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMemoryCache _memoryCache;
        private IOptions<ApplicationSettings> _settings;

        public FindAvailableTimesHandler(ILoggerFactory logger, IMemoryCache memoryCache, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            //_logger = logger;
            _logger = logger.CreateLogger<FindAvailableTimesHandler>();
            _loggerFactory = logger;
            _memoryCache = memoryCache;
            _settings = settings;
        }


        public async Task<TelehealthSpecialtyLocationsFindAvailableTimesResponse> HandleRequestResponseAsync(TelehealthSpecialtyLocationsFindAvailableTimesRequest message, Stopwatch stopwatch)
        {
            _logger.LogDebug("Enter HandleRequestResponseAsync");
            var processor = new FindAvailableTimesProcessor(_loggerFactory, _memoryCache, _settings, _config);

            try
            {
                var resp = await processor.ExecuteAsync(message, stopwatch);

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
