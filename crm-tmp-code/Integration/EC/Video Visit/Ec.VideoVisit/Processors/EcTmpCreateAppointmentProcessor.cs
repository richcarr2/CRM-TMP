using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ec.VideoVisit.Messages;
using Ec.VideoVisit.Services.Rest;
using Ec.VideoVisit.Services.XSD;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;

namespace Ec.VideoVisit.Processors
{
    public class EcTmpCreateAppointmentProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IServiceFactory _serviceFactory;

        public EcTmpCreateAppointmentProcessor(ILog logger, Settings settings, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _serviceFactory = serviceFactory;
        }

        public EcTmpCreateAppointmentResponse Execute(EcTmpCreateAppointmentRequest request)
        {
            _logger.Info("Starting VVS Create");

            var thisTimer = Stopwatch.StartNew();

            var newPatList = new List<patient>();

            foreach (var patient in request.EcTmpCreateAppointmentRequestDataInfo.Patients)
            {
                var thisPatient = new patient
                {
                    id = new personIdentifier
                    {
                        assigningAuthority = patient.Id.AssigningAuthority,
                        uniqueId = patient.Id.UniqueId
                    },
                    name = new personName
                    {
                        firstName = patient.Name.FirstName,
                        lastName = patient.Name.LastName
                    },
                    contactInformation = new contactInformation
                    {
                        preferredEmail = patient.ContactInformation.PreferredEmail,
                        mobile = patient.ContactInformation.Mobile
                    },
                    location = new location
                    {
                        type = (locationType)patient.Location.Type
                    },
                    hasMobileGFE = patient.HasMobileGFE
                };

                if (patient.VistaDateTimeSpecified)
                {
                    thisPatient.vistaDateTime = patient.VistaDateTime;
                }

                if (patient.ContactInformation.TimeZoneSpecified)
                {
                    thisPatient.contactInformation.timeZone = patient.ContactInformation.TimeZone;
                    thisPatient.contactInformation.timeZoneSpecified = true;
                }

                if (patient.Location.Clinic != null)
                {
                    thisPatient.location.clinic = new clinic
                    {
                        ien = patient.Location.Clinic.Ien,
                        name = patient.Location.Clinic.Name
                    };
                }

                if (patient.Location.Facility != null)
                {
                    thisPatient.location.facility = new facility
                    {
                        name = patient.Location.Facility.Name,
                        timeZone = patient.Location.Facility.TimeZone,
                        siteCode = patient.Location.Facility.SiteCode
                    };
                }

                if (patient.VirtualMeetingRoom != null)
                {
                    thisPatient.virtualMeetingRoom = new virtualMeetingRoom
                    {
                        conference = patient.VirtualMeetingRoom.Conference,
                        pin = patient.VirtualMeetingRoom.Pin,
                        url = patient.VirtualMeetingRoom.Url
                    };
                }

                newPatList.Add(thisPatient);
            }

            var newProvList = new List<provider>();

            foreach (var item in request.EcTmpCreateAppointmentRequestDataInfo.Providers)
            {
                var thisProvider = new provider
                {
                    name = new personName
                    {
                        firstName = item.Name.FirstName,
                        lastName = item.Name.LastName
                    },
                    contactInformation = new contactInformation
                    {
                        preferredEmail = item.ContactInformation.PreferredEmail,
                    },
                    location = new location
                    {
                        type = (locationType)item.Location.Type,
                    }
                };

                if (item.VistaDateTimeSpecified)
                {
                    thisProvider.vistaDateTime = item.VistaDateTime.AddMinutes(5.0);
                }

                if (item.ContactInformation.TimeZoneSpecified)
                {
                    thisProvider.contactInformation.timeZone = item.ContactInformation.TimeZone;
                    thisProvider.contactInformation.timeZoneSpecified = true;
                }

                if (item.Location.Clinic != null)
                {
                    thisProvider.location.clinic = new clinic
                    {
                        ien = item.Location.Clinic.Ien,
                        name = item.Location.Clinic.Name
                    };
                }

                if (item.Location.Facility != null)
                {
                    thisProvider.location.facility = new facility
                    {
                        name = item.Location.Facility.Name,
                        timeZone = item.Location.Facility.TimeZone,
                        siteCode = item.Location.Facility.SiteCode
                    };
                }

                if (item.VirtualMeetingRoom != null)
                {
                    thisProvider.virtualMeetingRoom = new virtualMeetingRoom
                    {
                        conference = item.VirtualMeetingRoom.Conference,
                        pin = item.VirtualMeetingRoom.Pin,
                        url = item.VirtualMeetingRoom.Url
                    };
                }

                newProvList.Add(thisProvider);
            }

            var appointmentRequest = new appointment
            {
                version = request.EcTmpCreateAppointmentRequestDataInfo.Version,
                id = request.EcTmpCreateAppointmentRequestDataInfo.Id,
                sourceSystem = request.EcTmpCreateAppointmentRequestDataInfo.SourceSystem,
                patients = new patients
                {
                    patient = newPatList
                },
                duration = request.EcTmpCreateAppointmentRequestDataInfo.Duration,
                dateTime = DateTime.Parse(request.EcTmpCreateAppointmentRequestDataInfo.DateTime),
                appointmentKind = (appointmentKind)Enum.Parse(typeof(appointmentKind), request.EcTmpCreateAppointmentRequestDataInfo.AppointmentKind, true),
                bookingNotes = request.EcTmpCreateAppointmentRequestDataInfo.BookingNotes,
                providers = new providers
                {
                    provider = newProvList
                }
            };

            if (request.EcTmpCreateAppointmentRequestDataInfo.SchedulingRequestTypeSpecified)
            {
                appointmentRequest.schedulingRequestType = (schedulingRequestType)request.EcTmpCreateAppointmentRequestDataInfo.SchedulingRequestType;
                appointmentRequest.schedulingRequestTypeSpecified = true;
            }

            if (request.EcTmpCreateAppointmentRequestDataInfo.TypeSpecified)
            {
                appointmentRequest.type = (appointmentType)request.EcTmpCreateAppointmentRequestDataInfo.Type;
                appointmentRequest.typeSpecified = true;
            }

            if (request.EcTmpCreateAppointmentRequestDataInfo.DesiredDateSpecified)
            {
                appointmentRequest.desiredDate = DateTime.Parse(request.EcTmpCreateAppointmentRequestDataInfo.DesiredDate);
                appointmentRequest.desiredDateSpecified = true;
            }
            _logger.Debug($"Create Appointment Request: {Serialization.DataContractSerialize<appointment>(appointmentRequest)}");

            var createAppointmentResponse = _serviceFactory.CreateAppointment(appointmentRequest, request.EcTmpCreateAppointmentRequestDataInfo.SamlToken);

            thisTimer.Stop();
            _logger.Info($"Calling VVS Create took {thisTimer.ElapsedMilliseconds} ms");

            var response = new EcTmpCreateAppointmentResponse { HttpStatusCode = "OK", EcTmpWriteResults = new EcTmpWriteResults() };
            response.EcTmpWriteResults.EcTmpWriteResult = new List<EcTmpWriteResult>();

            foreach (var result in createAppointmentResponse.writeResult)
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
