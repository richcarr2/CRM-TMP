using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.TelehealthSpecialtyLocation;
using VA.TMP.Integration.Api.TelehealthSpecialtyLocation.Processor;

namespace VA.TMP.Integration.Api.TelehealthSpecialtyLocation.MessageHandler
{
    public class GetClinicLocationsHandler
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private IOptions<ApplicationSettings> _settings;

        public GetClinicLocationsHandler(ILogger logger, IOptions<ApplicationSettings> settings, IConfiguration config)
        {
            _config = config;
            _logger = logger;
            _settings = settings;
        }

        public TelehealthSpecialtyLocationsGetClinicLocationsResponse HandleRequestResponse(TelehealthSpecialtyLocationsGetClinicLocationsRequest message)
        {
            var processor = new GetClinicLocationsProcessor(_loggerFactory, _settings, _config);

            try
            {
                var resp = processor.Execute(message);

                return resp;
            }
            catch (System.Exception e)
            {
                return new TelehealthSpecialtyLocationsGetClinicLocationsResponse { ErrorMessage = e.Message };
            }
        }
    }
}
