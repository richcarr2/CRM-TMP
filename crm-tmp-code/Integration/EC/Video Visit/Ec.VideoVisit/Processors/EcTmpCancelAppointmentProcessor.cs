using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Services.Rest;
using Ec.VideoVisit.Services.XSD;
using log4net;
using VA.TMP.Integration.Core;

namespace Ec.VideoVisit.Processors
{
    public class EcTmpCancelAppointmentProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        public EcTmpCancelAppointmentProcessor(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        public EcTmpCancelAppointmentResponse Execute(EcTmpCancelAppointmentRequest request)
        {
            _logger.Info("Starting VVS Cancel");

            var thisTimer = Stopwatch.StartNew();

            var cancelMeetingRequest = new cancelAppointmentRequest
            {
                id = request.Id,
                patientBookingStatuses = new personBookingStatuses(),
                sourceSystem = request.SourceSystem
            };

            if (request.PatientBookingStatuses?.PersonBookingStatus != null && request.PatientBookingStatuses.PersonBookingStatus.Length > 0)
            {
                cancelMeetingRequest.patientBookingStatuses.personBookingStatus = new List<personBookingStatus>();

                foreach (var personBooking in request.PatientBookingStatuses.PersonBookingStatus)
                {
                    var bookingStatus = new personBookingStatus
                    {
                        id = new personIdentifier
                        {
                            uniqueId = personBooking.Id.UniqueId,
                            assigningAuthority = personBooking.Id.AssigningAuthority
                        },
                        status = new status { description = personBooking.Status.Description }
                    };

                    if (personBooking.Status.CodeSpecified)
                    {
                        bookingStatus.status.code = (statusCode)personBooking.Status.Code;
                        bookingStatus.status.codeSpecified = true;
                    }

                    if (personBooking.Status.ReasonSpecified)
                    {
                        bookingStatus.status.reason = (reasonCode)personBooking.Status.Reason;
                        bookingStatus.status.reasonSpecified = true;
                    }

                    cancelMeetingRequest.patientBookingStatuses.personBookingStatus.Add(bookingStatus);
                }
            }

            var cancelResponse = _serviceFactory.CancelAppointment(cancelMeetingRequest, request.SamlToken);

            thisTimer.Stop();
            _logger.Info($"Calling VVS Cancel took {thisTimer.ElapsedMilliseconds} ms");

            var response = new EcTmpCancelAppointmentResponse { HttpStatusCode = "OK", EcTmpWriteResults = new EcTmpWriteResults() };
            response.EcTmpWriteResults.EcTmpWriteResult = new List<EcTmpWriteResult>();

            foreach (var result in cancelResponse.writeResult)
            {
                var writeResult = new EcTmpWriteResult
                {
                    PersonId = result.personId,
                    FacilityCode = result.facilityCode,
                    FacilityName = result.facilityName,
                    ClinicIen = result.clinicIEN,
                    ClinicName = result.clinicName,
                    DateTime = result.dateTime,
                    VistaStatus = (EcTmpVistaStatus)Enum.Parse(typeof(EcTmpVistaStatus), result.vistaStatus.ToString(), true),
                    Reason = result.reason
                };

                if (result.name != null) writeResult.Name = new EcTmpPersonName { FirstName = result.name.firstName, LastName = result.name.lastName };
                response.EcTmpWriteResults.EcTmpWriteResult.Add(writeResult);
            }

            return response;
        }
    }
}
