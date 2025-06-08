using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Api.TelehealthSpecialtyLocation.MessageHandler;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderAvailabilityController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ProviderAvailabilityController> _logger;
        private readonly IMemoryCache _memoryCache;
        private IOptions<ApplicationSettings> _settings;
        private Stopwatch _stopwatch;

        public ProviderAvailabilityController(ILoggerFactory logger, IMemoryCache memoryCache, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            _logger = logger.CreateLogger<ProviderAvailabilityController>();
            _loggerFactory = logger;
            _memoryCache = memoryCache;
            _settings = settings;
        }

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(TelehealthSpecialtyLocationsFindAvailableTimesResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(TelehealthSpecialtyLocationsResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(TelehealthSpecialtyLocationsResponse), (int)HttpStatusCode.RequestTimeout)]
        public async Task<ActionResult<TelehealthSpecialtyLocationsResponse>> Post([FromBody] TelehealthSpecialtyLocationsFindAvailableTimesRequest requestMessage)
        {
            _stopwatch = Stopwatch.StartNew();
            try
            {
                if (requestMessage.DateRange > 30)
                {
                    throw new WebException("DateRange must be 30 days or less", new ArgumentOutOfRangeException("DateRange", requestMessage.DateRange, "DateRange must be 30 days or less"));
                }

                if (string.IsNullOrEmpty(requestMessage.PatientFacility))
                {
                    throw new WebException("PatientFacility is required.", new ArgumentException("PatientFacility is required.", "PatientFacility"));
                }

                if (string.IsNullOrEmpty(requestMessage.StopCode))
                {
                    throw new WebException("StopCode is required.", new ArgumentException("StopCode is required.", "StopCode"));
                }

                var response = await new FindAvailableTimesHandler(_loggerFactory, _memoryCache, _settings, _config).HandleRequestResponseAsync(requestMessage, _stopwatch);
                response.ValidateResponse();

                return response;
            }
            catch (WebException we)
            {
                if (we.InnerException != null && we.InnerException is TimeoutException)
                {
                    return StatusCode(StatusCodes.Status408RequestTimeout, 
                        BadRequest(new TelehealthSpecialtyLocationsResponse { 
                            DebugInfo = we.ToString(),
                            ErrorMessage = "Reqest failed due to time out limit reached. No Results Available.", 
                            ErrorOccurred = true, 
                            Status = "FAILED"
                        }));
                }

                return StatusCode(StatusCodes.Status400BadRequest, BadRequest(new TelehealthSpecialtyLocationsResponse { ErrorMessage = we.Message, ErrorOccurred = true, DebugInfo = we.ToString() }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new TelehealthSpecialtyLocationsResponse { ErrorMessage = ex.Message, ErrorOccurred = true, DebugInfo = ex.ToString() });
            }
            finally
            {
                _stopwatch.Stop();
                _logger.LogDebug($"ProviderAvailabilityController Execution Time: {_stopwatch.Elapsed}");
            }
        }
    }
}
