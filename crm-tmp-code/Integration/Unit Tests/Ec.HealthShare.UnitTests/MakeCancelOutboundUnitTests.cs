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
    public class MakeCancelOutboundUnitTests
    {
        private StubILog _mockLogger;
        private Settings _settings;
        private StubIServiceFactory _mockServiceFactory;
        private ShimTelemetryClient _mockTelemetryClient;

        [TestInitialize()]
        public void Initialize()
        {
            _mockLogger = new StubILog();

            _mockServiceFactory = new StubIServiceFactory()
            {
                MakeCancelOutboundEcHealthShareMakeCancelOutboundRequestMessage = (req) => { return new EcHealthShareMakeCancelOutboundResponseMessage { ExceptionOccured = false }; }
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
            // Arrange
            using (ShimsContext.Create())
            {
                // Arrange
                _mockTelemetryClient = new ShimTelemetryClient(new TelemetryClient());
                var controller = new StubMakeCancelOutboundController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                var request = new StubEcHealthShareMakeCancelOutboundRequestMessage();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcHealthShareMakeCancelOutboundResponseMessage r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcHealthShareMakeCancelOutboundResponseMessage()
                    {
                        ExceptionOccured = false
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(request);
                var responseContent = httpResponse.TryGetContentValue(out EcHealthShareMakeCancelOutboundResponseMessage response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(false, response.ExceptionOccured);
            }
        }

        [TestMethod]
        public void FailWhenRequestIsNull()
        {
            // Arrange
            using (ShimsContext.Create())
            {
                // Arrange
                _mockTelemetryClient = new ShimTelemetryClient(new TelemetryClient());
                var controller = new StubMakeCancelOutboundController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                var request = new StubEcHealthShareMakeCancelOutboundRequestMessage();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcHealthShareMakeCancelOutboundResponseMessage r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcHealthShareMakeCancelOutboundResponseMessage()
                    {
                        ExceptionMessage = "Error calling HealthShare Make Cancel Outbound. The request cannot be null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(request);
                var responseContent = httpResponse.TryGetContentValue(out EcHealthShareMakeCancelOutboundResponseMessage response);

                // Assert
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling HealthShare Make Cancel Outbound. The request cannot be null", response.ExceptionMessage);
            }
        }
    }
}