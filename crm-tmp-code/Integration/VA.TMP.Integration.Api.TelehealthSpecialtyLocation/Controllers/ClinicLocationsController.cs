using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicLocationsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ClinicLocationsController> _logger;
        private IOptions<ApplicationSettings> _settings;

        public ClinicLocationsController(ILogger<ClinicLocationsController> logger, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            _logger = logger;
            _settings = settings;
        }

        [HttpPost]
        [NonAction]
        public TelehealthSpecialtyLocationsGetClinicLocationsResponse Post([FromBody] TelehealthSpecialtyLocationsGetClinicLocationsRequest requestMessage)
        {
            //_logger.LogDebug($"Request payload: {Serialization.JsonSerialize<TelehealthSpecialtyLocationsGetClinicLocationsRequest>(requestMessage)}");

            //if (string.IsNullOrEmpty(requestMessage.FacilityStationNumber))
            //{
            //    return new TelehealthSpecialtyLocationsGetClinicLocationsResponse { ErrorMessage = "Missing value for Required parameter: facilityStationNumber" };
            //}

            //if (string.IsNullOrEmpty(requestMessage.SiteStationNumber))
            //{
            //    return new TelehealthSpecialtyLocationsGetClinicLocationsResponse { ErrorMessage = "Missing value for Required parameter: siteStationNumber" };
            //}

            //if (string.IsNullOrEmpty(requestMessage.SpecialtyName))
            //{
            //    return new TelehealthSpecialtyLocationsGetClinicLocationsResponse { ErrorMessage = "Missing value for Required parameter: specialityName" };
            //}

            //var response = new GetClinicLocationsHandler(_logger, _settings, _config).HandleRequestResponse(requestMessage);
            //return response;
            return null;
        }
    }
}
