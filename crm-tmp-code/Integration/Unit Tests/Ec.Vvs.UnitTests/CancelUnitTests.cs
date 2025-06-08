using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Fakes;
using System.Web.Http;
using Ec.VideoVisit.Api.Controllers.Fakes;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Messages.Fakes;
using Ec.VideoVisit.Services.Rest.Fakes;
using Ec.VideoVisit.Services.XSD;
using log4net.Fakes;
using Microsoft.ApplicationInsights.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VA.TMP.Integration.Core;

namespace Ec.Vvs.UnitTests
{
    [TestClass]
    public class CancelUnitTests
    {
        private StubILog _mockLogger;
        private Settings _settings;
        private StubIServiceFactory _mockServiceFactory;
        private ShimTelemetryClient _mockTelemetryClient;

        [TestInitialize()]
        public void Initialize()
        {
            _mockLogger = new StubILog();
            _mockServiceFactory = new StubIServiceFactory
            {
                CancelAppointmentcancelAppointmentRequestString = (ar, s) => { return GetWriteResults(); }
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
                _mockTelemetryClient = new ShimTelemetryClient();
                var controller = new StubCancelController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };

                var request = new StubEcTmpCancelAppointmentRequest();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcTmpCancelAppointmentResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcTmpCancelAppointmentResponse()
                    {
                        ExceptionOccured = false,
                        EcTmpWriteResults = new EcTmpWriteResults
                        {
                            EcTmpWriteResult = new List<EcTmpWriteResult>
                            {
                                new EcTmpWriteResult
                                {
                                    Name = new EcTmpPersonName{ FirstName = "Billy", LastName = "Bob"},
                                    Reason = "Cancel",
                                    VistaStatus = EcTmpVistaStatus.CANCELLED
                                }
                            }
                        },
                        HttpStatusCode = "OK"
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(request);
                var responseContent = httpResponse.TryGetContentValue(out EcTmpCancelAppointmentResponse response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(false, response.ExceptionOccured);
                Assert.AreEqual(vistaStatus.CANCELLED.ToString(), response.EcTmpWriteResults.EcTmpWriteResult.First().VistaStatus.ToString());
                Assert.AreEqual("Cancel", response.EcTmpWriteResults.EcTmpWriteResult.First().Reason);
                Assert.AreEqual("OK", response.HttpStatusCode);
                Assert.AreEqual("Billy", response.EcTmpWriteResults.EcTmpWriteResult.First().Name.FirstName);
                Assert.AreEqual("Bob", response.EcTmpWriteResults.EcTmpWriteResult.First().Name.LastName); 
            }
        }

        [TestMethod]
        public void FailWhenRequestIsNull()
        {
            // Arrange
            using (ShimsContext.Create())
            {
                _mockTelemetryClient = new ShimTelemetryClient();
                var controller = new StubCancelController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcTmpCancelAppointmentResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcTmpCancelAppointmentResponse()
                    {
                        ExceptionMessage = "Error calling VVS Cancel. The request cannot be null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(null);
                var responseContent = httpResponse.TryGetContentValue(out EcTmpCancelAppointmentResponse response);

                // Assert
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling VVS Cancel. The request cannot be null", response.ExceptionMessage);
            }
        }

        private static writeResults GetWriteResults()
        {
            return new writeResults
            {
                writeResult = new List<writeResult>
                {
                    new writeResult
                    {
                        name = new personName { firstName = "Billy", lastName = "Bob" },
                        reason = "Cancel",
                        vistaStatus = vistaStatus.CANCELLED
                    }
                }
            };
        }
    }
}