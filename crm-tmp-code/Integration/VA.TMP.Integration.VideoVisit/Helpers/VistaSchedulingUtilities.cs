using System;
using System.Collections.Generic;
using System.Linq;
using Ec.VideoVisit.Messages;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.Helpers
{
    public static class VistaSchedulingUtilities
    {
        public static WriteResults MapEcToWriteResult(EcTmpWriteResults source)
        {
            var results = new List<WriteResult>();
            foreach (var res in source.EcTmpWriteResult)
            {
                var mappedresult = new WriteResult
                {
                    ClinicIen = res.ClinicIen,
                    ClinicName = res.ClinicName,
                    DateTime = res.DateTime.ToString("MM/dd/yyyy HH:mm 'GMT'"),
                    FacilityCode = res.FacilityCode,
                    FacilityName = res.FacilityName,
                    Name = res.Name == null ? new PersonName() : new PersonName
                    {
                        FirstName = res.Name.FirstName,
                        LastName = res.Name.LastName,
                        MiddleInitial = res.Name.MiddleInitial
                    },
                    PersonId = res.PersonId,
                    Reason = res.Reason,
                    VistaStatus = (VistaStatus)(int)res.VistaStatus
                };
                results.Add(mappedresult);
            }

            return new WriteResults
            {
                WriteResult = results.ToArray()
            };
        }

        public static EcTmpWriteResults CreateFakeBookResult(string fakeType, VideoVisitUpdateStateObject state)
        {
            return CreateFakeBookResult(fakeType, (EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), state.Appointment.AppointmentKind, true), state.Appointment.Patients, state.Appointment.Providers);
        }

        public static EcTmpWriteResults CreateFakeBookResult(string fakeType, VideoVisitCreateStateObject state)
        {
            return CreateFakeBookResult(fakeType, (EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), state.Appointment.AppointmentKind), state.Appointment.Patients, state.Appointment.Providers);
        }

        public static EcTmpWriteResults CreateFakeBookResult(string fakeType, EcTmpAppointmentKind appointmentKind, EcTmpPatients[] patients, EcTmpProviders[] providers)
        {
            var writeResults = new EcTmpWriteResults { EcTmpWriteResult = new List<EcTmpWriteResult>() };

            for (var i = 0; i < patients.Length; i++)
            {
                if (appointmentKind != EcTmpAppointmentKind.STORE_FORWARD)
                {
                    var lastProv = providers[providers.Length - 1];
                    var res = new EcTmpWriteResult
                    {
                        ClinicIen = lastProv.Location?.Clinic?.Ien,
                        ClinicName = lastProv.Location?.Clinic?.Name,
                        DateTime = DateTime.UtcNow,
                        FacilityCode = lastProv.Location?.Facility?.SiteCode,
                        FacilityName = lastProv.Location?.Facility?.Name,
                        Name = new EcTmpPersonName
                        {
                            FirstName = patients[i].Name.FirstName,
                            LastName = patients[i].Name.LastName,
                            MiddleInitial = patients[i].Name.MiddleInitial
                        },
                        PersonId = patients[i].Id.UniqueId,
                        Reason = (fakeType == "0" || fakeType == "1") ? string.Empty : "Failure Reason 2",
                        VistaStatus = (fakeType == "0" || fakeType == "1") ? EcTmpVistaStatus.RECEIVED : EcTmpVistaStatus.FAILED_TO_RECEIVE
                    };
                    writeResults.EcTmpWriteResult[i] = res;
                }
                if (appointmentKind == EcTmpAppointmentKind.CLINIC_BASED || appointmentKind == EcTmpAppointmentKind.STORE_FORWARD)
                {
                    var res = new EcTmpWriteResult
                    {
                        ClinicIen = patients[i].Location?.Clinic?.Ien,
                        ClinicName = patients[i].Location?.Clinic?.Name,
                        DateTime = DateTime.Now,
                        FacilityCode = patients[i].Location?.Facility?.SiteCode,
                        FacilityName = patients[i].Location?.Facility?.Name,
                        Name = new EcTmpPersonName
                        {
                            FirstName = patients[i].Name.FirstName,
                            LastName = patients[i].Name.LastName,
                            MiddleInitial = patients[i].Name.MiddleInitial
                        },
                        PersonId = patients[i].Id.UniqueId,
                        Reason = fakeType == "0" ? "" : "Failure Reason 1",
                        VistaStatus = fakeType == "0" ? EcTmpVistaStatus.RECEIVED : EcTmpVistaStatus.FAILED_TO_RECEIVE
                    };
                    if (appointmentKind == EcTmpAppointmentKind.CLINIC_BASED)
                        writeResults.EcTmpWriteResult[patients.Length - i] = res;
                    else
                        writeResults.EcTmpWriteResult[i] = res;
                }
            }
            return writeResults;
        }

        public static EcTmpWriteResults CreateFakeCancelResult(string fakeType, VideoVisitDeleteStateObject state)
        {
            var virsForAppt = new List<cvt_vistaintegrationresult>();
            var matchingVirs = new List<cvt_vistaintegrationresult>();
            var writeResults = new List<EcTmpWriteResult>();
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                virsForAppt.AddRange(state.CrmAppointment != null
                    ? srv.cvt_vistaintegrationresultSet.Where(x => x.cvt_Appointment.Id == state.CrmAppointment.Id)
                    : srv.cvt_vistaintegrationresultSet.Where(x => x.cvt_ServiceActivity.Id == state.ServiceAppointment.Id));

                foreach (var status in state.CancelAppointmentRequest.PatientBookingStatuses.PersonBookingStatus)
                {
                    var vistaIntegrationResult = virsForAppt.Where(x => x.cvt_PersonId == status.Id.UniqueId).ToList();
                    matchingVirs.AddRange(vistaIntegrationResult);
                }
            }

            foreach (var match in matchingVirs)
            {
                var res = new EcTmpWriteResult
                {
                    ClinicIen = match.cvt_ClinicIEN,
                    ClinicName = match.cvt_ClinicName,
                    DateTime = DateTime.Now,
                    FacilityCode = match.cvt_FacilityCode,
                    FacilityName = match.cvt_FacilityName,
                    Name = new EcTmpPersonName
                    {
                        FirstName = match.cvt_FirstName,
                        LastName = match.cvt_LastName
                    },
                    PersonId = match.cvt_PersonId,
                    Reason = fakeType == "0" ? string.Empty : "Failed to Cancel, Reason 1",
                    VistaStatus = fakeType == "0" ? EcTmpVistaStatus.CANCELLED : EcTmpVistaStatus.FAILED_TO_CANCEL
                };
                writeResults.Add(res);
            }
            return new EcTmpWriteResults
            {
                EcTmpWriteResult = writeResults
            };
        }
    }
}
