using System;
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
    public class CreateUnitTests
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
                var controller = new StubVmrCreateController(_mockLogger, _settings, _mockKeyVaultCert, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                var request = new EcVyoptaSMScheduleMeetingRequest();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcVyoptaSMScheduleMeetingResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new EcVyoptaSMScheduleMeetingResponse()
                    {
                        ExceptionOccured = false,
                        mcs_DialingAlias = "123",
                        mcs_EncounterId = "456",
                        mcs_MiscData = "Misc"
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(request);
                var responseContent = httpResponse.TryGetContentValue(out EcVyoptaSMScheduleMeetingResponse response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(false, response.ExceptionOccured);
                Assert.AreEqual("123", response.mcs_DialingAlias);
                Assert.AreEqual("456", response.mcs_EncounterId);
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
                var controller = new StubVmrCreateController(_mockLogger, _settings, _mockKeyVaultCert, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };


                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcVyoptaSMScheduleMeetingResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new EcVyoptaSMScheduleMeetingResponse()
                    {
                        ExceptionMessage = "Error calling VMR Schedule Meeting. The request cannot be null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(null);
                var responseContent = httpResponse.TryGetContentValue(out EcVyoptaSMScheduleMeetingResponse response);

                // Assert
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling VMR Schedule Meeting. The request cannot be null", response.ExceptionMessage);
            }
        }

        private static EcVyoptaSMScheduleMeetingRequest GetRequest()
        {
            return new EcVyoptaSMScheduleMeetingRequest
            {
                mcs_EncounterId = "456",
                mcs_EndTime = DateTime.Now,
                mcs_GuestName = "Guest",
                mcs_GuestPin = "55555",
                mcs_HostName = "host",
                mcs_HostPin = "77777",
                mcs_MeetingRoomName = "name",
                mcs_MiscData = "Misc",
                mcs_StartTime = DateTime.Now,
                MessageId = Guid.NewGuid().ToString(),
                OrganizationName = "org",
                UserId = Guid.NewGuid()
            };
        }
    }
}