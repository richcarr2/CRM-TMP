using System.Net;
using System.Net.Http;
using System.Net.Http.Fakes;
using System.Web.Http.Fakes;
using Ec.VirtualMeetingRoom.Messages;
using Ec.VirtualMeetingRoom.Services.Fakes;
using Ec.Vmr.Api.Controllers.Fakes;
using log4net.Fakes;
using Microsoft.ApplicationInsights.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VA.TMP.Integration.Certificate.Interface.Fakes;
using VA.TMP.Integration.Core;

namespace Ec.Vmr.UnitTests
{
    [TestClass]
    public class DeleteUnitTests
    {
        private StubILog _mockLogger;
        private StubIKeyVaultCert _mockKeyVaultCert;
        private StubIServiceFactory _mockServiceFactory;
        private ShimTelemetryClient _mockTelemetryClient;
        private Settings _settings;

        [TestInitialize()]
        public void Initialize()
        {
            _mockLogger = new StubILog();
            _mockKeyVaultCert = new StubIKeyVaultCert();
            _mockServiceFactory = new StubIServiceFactory();
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
                _mockTelemetryClient = new ShimTelemetryClient();
                var controller = new StubVmrDeleteController(_mockLogger, _settings, _mockKeyVaultCert, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                var request = new EcVirtualDeleteMeetingRequest();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcVirtualDeleteMeetingResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new EcVirtualDeleteMeetingResponse()
                    {
                        ExceptionOccured = false,
                        mcs_MiscData = "Misc"
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(request);
                var responseContent = httpResponse.TryGetContentValue(out EcVirtualDeleteMeetingResponse response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(false, response.ExceptionOccured);
                Assert.AreEqual("Misc", response.mcs_MiscData);
            }
        }

        [TestMethod]
        public void FailWhenRequestIsNull()
        {
            // Arrange
            using (ShimsContext.Create())
            {
                _mockTelemetryClient = new ShimTelemetryClient();
                var controller = new StubVmrDeleteController(_mockLogger, _settings, _mockKeyVaultCert, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcVirtualDeleteMeetingResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new EcVirtualDeleteMeetingResponse()
                    {
                        ExceptionMessage = "Error calling VMR Delete Meeting. The request cannot be null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(null);
                var responseContent = httpResponse.TryGetContentValue(out EcVirtualDeleteMeetingResponse response);

                // Assert
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling VMR Delete Meeting. The request cannot be null", response.ExceptionMessage);
            }
        }

        private static EcVirtualDeleteMeetingRequest GetRequest()
        {
            return new EcVirtualDeleteMeetingRequest
            {
                mcs_EncounterId = "123",
                mcs_MiscData = "Misc"
            };
        }
    }
}