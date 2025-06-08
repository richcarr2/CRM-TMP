using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ec.JsonWebToken.MessageHandler;
using Ec.JsonWebToken.Messages;
using Ec.JsonWebToken.Services.Rest;
using log4net;
using Microsoft.ApplicationInsights;
using Swashbuckle.Swagger.Annotations;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;

namespace Ec.Jwt.Api.Controllers
{
    public class EncryptTokenController : ApiController
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;
        private readonly IServiceFactory _serviceFactory;
        private readonly TelemetryClient _telemetryClient;

        public EncryptTokenController(ILog logger, Settings settings, IKeyVaultCert keyVaultCert, IServiceFactory serviceFactory, TelemetryClient telemetryClient)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
            _serviceFactory = serviceFactory;
            _telemetryClient = telemetryClient;
        }

        [SwaggerOperation("EncryptToken")]
        public HttpResponseMessage Post([FromBody]EcJwtEncryptTokenRequest requestMessage)
        {
            try
            {
                if (requestMessage == null)
                {
                    _logger.Error("Error calling JWT. The request cannot be null");
                    var error = new EcJwtEncryptTokenResponse { ExceptionOccured = true, ExceptionMessage = "Error calling JWT. The request cannot be null" };
                    return Request.CreateResponse(HttpStatusCode.Created, error);
                }

                var response = new EcJwtEncryptMessageHandler(_logger, _settings, _keyVaultCert, _serviceFactory).HandleRequestResponse(requestMessage);

                return Request.CreateResponse(HttpStatusCode.Created, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Created, CreateResponse(ex));
            }
        }

        private EcJwtEncryptTokenResponse CreateResponse<T>(T ex) where T : Exception
        {
            _logger.Error($"Error JWT: {ex.Message}", ex);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>() { { "Method", "JWT EncryptToken" } });
            return new EcJwtEncryptTokenResponse { ExceptionOccured = true, ExceptionMessage = $"Error calling JWT: {Strings.BuildErrorMessage(ex)}" };
        }
    }
}
