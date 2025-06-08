using System.Net;
using System.Net.Http;
using System.Net.Http.Fakes;
using System.Web.Http.Fakes;
using Ec.HealthShare.Api.Controllers.Fakes;
using Ec.HealthShare.Messages;
using Ec.HealthShare.Messages.Fakes;
using Ec.HealthShare.Services.Rest.Fakes;
using log4net.Fakes;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VA.TMP.Integration.Core;

namespace Ec.HealthShare.UnitTests
{
    [TestClass]
    public class GetConsultsUnitTests
    {
        private StubILog _mockLogger;
        private Settings _settings;
        private StubIServiceFactory _mockServiceFactory;
        private ShimTelemetryClient _mockTelemetryClient;
        private string _controlId;

        [TestInitialize()]
        public void Initialize()
        {
            _controlId = "987654321";

            _mockLogger = new StubILog();

            _mockServiceFactory = new StubIServiceFactory()
            {
                GetConsultsEcHealthShareGetConsultsRequest = (req) =>
                {
                    return new StubEcHealthShareGetConsultsResponse()
                    {
                        ExceptionOccured = false,
                        ControlId = _controlId
                    };
                }
            };

            _settings = new Settings();
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
        }

        [TestMethod]
        public void HappyPath()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                _mockTelemetryClient = new ShimTelemetryClient(new TelemetryClient());
                var controller = new StubConsultsController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                var request = new StubEcHealthShareGetConsultsRequest();

                //StubHttpResponseMessage
                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcHealthShareGetConsultsResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcHealthShareGetConsultsResponse()
                    {
                        ExceptionOccured = false,
                        ControlId = _controlId
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(request);
                var responseContent = httpResponse.TryGetContentValue(out EcHealthShareGetConsultsResponse response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(false, response.ExceptionOccured);
                Assert.AreEqual(_controlId, response.ControlId);
            }
        }

        [TestMethod]
        public void FailWhenRequestIsNull()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                _mockTelemetryClient = new ShimTelemetryClient(new TelemetryClient());
                var controller = new StubConsultsController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                var request = new StubEcHealthShareGetConsultsRequest();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcHealthShareGetConsultsResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcHealthShareGetConsultsResponse()
                    {
                        ExceptionMessage = "Error calling HealthShare Get Consults. The request cannot be null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(null);
                var responseContent = httpResponse.TryGetContentValue(out EcHealthShareGetConsultsResponse response);
                // Assert
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling HealthShare Get Consults. The request cannot be null", response.ExceptionMessage);
            }
        }
    }
}