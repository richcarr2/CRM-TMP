using System.Net;
using System.Net.Http;
using System.Net.Http.Fakes;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http.Fakes;
using Ec.JsonWebToken.Messages;
using Ec.JsonWebToken.Messages.Fakes;
using Ec.JsonWebToken.Services.Rest.Fakes;
using Ec.Jwt.Api.Controllers.Fakes;
using log4net.Fakes;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VA.TMP.Integration.Certificate.Interface.Fakes;
using VA.TMP.Integration.Core;

namespace Ec.Jwt.UnitTests
{
    [TestClass]
    public class EncryptTokenTest
    {
        private StubILog _mockLogger;
        private StubIKeyVaultCert _mockKeyVaultCert;
        private StubIServiceFactory _mockServiceFactory;
        private Settings _settings;
        private ShimTelemetryClient _mockTelemetryClient;
        private string _base64Token;

        [TestInitialize()]
        public void Initialize()
        {
            _base64Token = "Encrypted Token";

            _mockLogger = new StubILog();

            _mockKeyVaultCert = new StubIKeyVaultCert()
            {
                GetKeyVaultCertificateString = (key) => { return Task.FromResult(new X509Certificate2()); }
            };

            _mockServiceFactory = new StubIServiceFactory()
            {
                EncryptTokenJwtPayload = (payload) => { return Task.FromResult(string.Empty); }
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
                var req = new StubEcJwtEncryptTokenRequest();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcJwtEncryptTokenResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcJwtEncryptTokenResponse
                    {
                        EncryptedJwtToken = _base64Token,
                        ExceptionOccured = false
                    };
                    return true;
                });

                _mockTelemetryClient = new ShimTelemetryClient(new TelemetryClient());

                var controller = new StubEncryptTokenController(_mockLogger, _settings, _mockKeyVaultCert, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                //Act
                var httpResponse = controller.Post(req);
                httpResponse.TryGetContentValue(out EcJwtEncryptTokenResponse response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(false, response.ExceptionOccured);
                Assert.AreEqual(null, response.ExceptionMessage);
                Assert.AreEqual(_base64Token, response.EncryptedJwtToken);
            }
        }

        [TestMethod]
        public void FailWhenRequestIsNull()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcJwtEncryptTokenResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcJwtEncryptTokenResponse
                    {
                        ExceptionMessage = "Error calling JWT. The request cannot be null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                _mockTelemetryClient = new ShimTelemetryClient(new TelemetryClient());

                var controller = new StubEncryptTokenController(_mockLogger, _settings, _mockKeyVaultCert, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                //Act
                var httpResponse = controller.Post(null);
                httpResponse.TryGetContentValue(out EcJwtEncryptTokenResponse response);
                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling JWT. The request cannot be null", response.ExceptionMessage);
            }
        }

        [TestMethod]
        public void FailWhenInvalidToken()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                var req = new StubEcJwtEncryptTokenRequest();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcJwtEncryptTokenResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new StubEcJwtEncryptTokenResponse
                    {
                        ExceptionMessage = "Error calling JWT: First Name attribute in SAML token cannot be missing or null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                _mockTelemetryClient = new ShimTelemetryClient(new TelemetryClient());

                var controller = new StubEncryptTokenController(_mockLogger, _settings, _mockKeyVaultCert, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                //Act
                var httpResponse = controller.Post(req);
                httpResponse.TryGetContentValue(out EcJwtEncryptTokenResponse response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling JWT: First Name attribute in SAML token cannot be missing or null", response.ExceptionMessage);
            }
        }
    }
}