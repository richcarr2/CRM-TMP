using Ec.VideoVisit.Api.Controllers;
using Ec.VideoVisit.Api.Controllers.Fakes;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Messages.Fakes;
using Ec.VideoVisit.Services.Rest;
using Ec.VideoVisit.Services.Rest.Fakes;
using Ec.VideoVisit.Services.XSD;
using log4net;
using log4net.Fakes;
using Microsoft.ApplicationInsights.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Fakes;
using System.Web.Http;
using System.Web.Http.Fakes;
using VA.TMP.Integration.Core;

namespace Ec.Vvs.UnitTests
{
    [TestClass]
    public class UpdateUnitTests
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
                UpdateAppointmentappointmentString = (ar, s) => { return GetWriteResults(); }
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
                var controller = new StubUpdateController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };

                var request = new StubEcTmpUpdateAppointmentRequest();// GetRequest();

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcTmpUpdateAppointmentResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new EcTmpUpdateAppointmentResponse()
                    {
                        ExceptionOccured = false,
                        EcTmpWriteResults = new EcTmpWriteResults
                        {
                            EcTmpWriteResult = new List<EcTmpWriteResult>
                            {
                                new EcTmpWriteResult
                                {
                                    Name = new EcTmpPersonName{ FirstName = "Billy", LastName = "Bob"},
                                    Reason = "Update",
                                    VistaStatus = EcTmpVistaStatus.BOOKED
                                }
                            }
                        },
                        HttpStatusCode = "OK"
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(request);
                var responseContent = httpResponse.TryGetContentValue(out EcTmpUpdateAppointmentResponse response);

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
                Assert.AreEqual(false, response.ExceptionOccured);
                Assert.AreEqual(vistaStatus.BOOKED.ToString(), response.EcTmpWriteResults.EcTmpWriteResult.First().VistaStatus.ToString());
                Assert.AreEqual("Update", response.EcTmpWriteResults.EcTmpWriteResult.First().Reason);
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
                var controller = new StubUpdateController(_mockLogger, _settings, _mockServiceFactory, _mockTelemetryClient)
                {
                    Request = new StubHttpRequestMessage(),
                    Configuration = new StubHttpConfiguration()
                };

                ShimHttpResponseMessageExtensions.TryGetContentValueOf1HttpResponseMessageM0Out((HttpResponseMessage rm, out EcTmpUpdateAppointmentResponse r) =>
                {
                    rm = new HttpResponseMessage();
                    r = new EcTmpUpdateAppointmentResponse()
                    {
                        ExceptionMessage = "Error calling VVS Update. The request cannot be null",
                        ExceptionOccured = true
                    };
                    return true;
                });

                // Act
                var httpResponse = controller.Post(null);
                var responseContent = httpResponse.TryGetContentValue(out EcTmpUpdateAppointmentResponse response);

                // Assert
                Assert.AreEqual(true, response.ExceptionOccured);
                Assert.AreEqual("Error calling VVS Update. The request cannot be null", response.ExceptionMessage);
            }
        }

        private static EcTmpUpdateAppointmentRequest GetRequest()
        {
            var request = new EcTmpUpdateAppointmentRequest
            {
                AppointmentKind = EcTmpAppointmentKind.CLINIC_BASED.ToString(),
                DateTime = DateTime.Now.ToString(),
                DesiredDate = DateTime.Now.ToString(),
                Duration = 30,
                SchedulingRequestType = EcTmpSchedulingRequestType.NEXT_AVAILABLE_APPT,
                SourceSystem = "TMP",
                Status = new EcTmpStatus { Code = EcTmpStatusCode.CHECKED_IN, Description = "TODO", Reason = EcTmpReasonCode.OTHER },
                Type = EcTmpAppointmentType.EMPLOYEE
            };

            var patient = new EcTmpPatients
            {
                ContactInformation = new EcTmpContactInformation
                {
                    AlternativeEmail = "joe@va.gov",
                    Mobile = "999-999-9999",
                    PreferredEmail = "joe2@va.gov",
                    TimeZone = "1"
                },
                Id = new EcTmpPersonIdentifier { AssigningAuthority = "VHA", UniqueId = "1234" },
                Location = new EcTmpLocation
                {
                    Clinic = new EcTmpClinic { Ien = "123", Name = "Clinic1" },
                    Facility = new EcTmpFacility { Name = "Facility1", SiteCode = "111", TimeZone = "1" },
                    Type = EcTmpLocationType.VA
                },
                Name = new EcTmpPersonName { FirstName = "joe", LastName = "bob", MiddleInitial = "b" },
                VirtualMeetingRoom = new EcTmpVirtualMeetingRoom { Conference = "conf1", Pin = "1234", Url = "https://va.gov" },
                VistaDateTime = DateTime.Now
            };

            var patients = new List<EcTmpPatients> { patient }.ToArray();

            var provider = new EcTmpProviders
            {
                ContactInformation = new EcTmpContactInformation
                {
                    AlternativeEmail = "bob@va.gov",
                    Mobile = "999-999-9999",
                    PreferredEmail = "bob2@va.gov",
                    TimeZone = "1"
                },
                Id = new EcTmpPersonIdentifier { AssigningAuthority = "VHA", UniqueId = "9876" },
                Location = new EcTmpLocation
                {
                    Clinic = new EcTmpClinic { Ien = "987", Name = "Clinic2" },
                    Facility = new EcTmpFacility { Name = "Facility2", SiteCode = "222", TimeZone = "1" },
                    Type = EcTmpLocationType.VA
                },
                Name = new EcTmpPersonName { FirstName = "bob", LastName = "bob", MiddleInitial = "b" },
                VirtualMeetingRoom = new EcTmpVirtualMeetingRoom { Conference = "conf2", Pin = "9876", Url = "https://va2.gov" },
                VistaDateTime = DateTime.Now
            };

            var providers = new List<EcTmpProviders> { provider }.ToArray();

            request.Patients = patients;
            request.Providers = providers;

            return request;
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
                        reason = "Create",
                        vistaStatus = vistaStatus.BOOKED
                    }
                }
            };
        }
    }
}