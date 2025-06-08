using System;
using System.Linq;
using System.Text;
using Ec.VideoVisit.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.Helpers
{
    internal class BusinessRuleValidator
    {
        enum AppointmentModality
        {
            VVC = 178970002,
            VVC_TestCall = 178970008
        }

        internal string CheckPatientCount(EcTmpPatients[] patients)
        {
            return patients.Count() >= 0 && patients.Count() <= 50 ? string.Empty : "The maximum number of patients is 50";
        }

        internal string CheckPatientAssigningAuthorityAndName(EcTmpPatients[] patients)
        {
            var errors = new StringBuilder();
            foreach (var pat in patients)
            {
                if (pat.Id == null) errors.Append(string.Format("Patient {0} should have an identifier|", GetPatientName(pat)));
                else if (pat.Id.AssigningAuthority != "ICN") errors.Append(string.Format("Patient {0}'s identifier should be ICN|", GetPatientName(pat)));
                errors.Append(CheckPatientName(pat));
            }
            return errors.ToString();
        }

        internal string CheckFacilityAndClinic(EcTmpLocation location)
        {
            var errors = new StringBuilder();
            if (location.Type != EcTmpLocationType.VA) return errors.ToString();
            if (string.IsNullOrEmpty(location.Facility?.SiteCode) || string.IsNullOrEmpty(location.Facility.Name))
                errors.Append("For VA Location Type, Facility name and number are required|");
            if (string.IsNullOrEmpty(location.Clinic?.Ien) || string.IsNullOrEmpty(location.Clinic.Name))
                errors.Append("For VA Location Type, Clinic Name and IEN are required|");
            return errors.ToString();
        }

        internal string CheckPatientEmail(EcTmpPatients[] patients, EcTmpAppointmentKind appointmentKind)
        {
            if (appointmentKind == EcTmpAppointmentKind.MOBILE_ANY)
            {
                var errors = new StringBuilder();
                foreach (var pat in patients)
                {
                    if (string.IsNullOrEmpty(pat.ContactInformation?.PreferredEmail))
                        errors.Append(string.Format("Patient Email is Required for Mobile BYOD appointments for {0}|", GetPatientName(pat)));
                }
                return errors.ToString();
            }

            return string.Empty;
        }

        internal string CheckPatientEmail(EcTmpPatients[] patients, EcTmpAppointmentKind appointmentKind, int appointmentModality)
        {
            if (appointmentModality.Equals((int)AppointmentModality.VVC) || appointmentModality.Equals((int)AppointmentModality.VVC_TestCall))
                return string.Empty;
            else
            {
                return CheckPatientEmail(patients, appointmentKind);
            }
        }

        internal string CheckProviderEmail(EcTmpProviders[] providers)
        {
            var errors = new StringBuilder();
            foreach (var pro in providers)
            {
                if (string.IsNullOrEmpty(pro.ContactInformation?.PreferredEmail))
                    errors.Append(string.Format("Provider {0} must have an email address|", Convert(pro.Name)));
            }
            return errors.ToString();
        }

        internal string CheckPatientName(EcTmpPatients patient)
        {
            return patient.Name == null ? "Patient name cannot be null" : string.IsNullOrEmpty(GetPatientName(patient).Trim()) ? "Patient name cannot be empty" : string.Empty;
        }

        internal string CheckAppointmentKind(EcTmpCreateAppointmentRequestData createRequest)
        {
            return string.IsNullOrEmpty(createRequest.AppointmentKind.ToString()) ? "Appointment Kind cannot be null|" : string.Empty;
        }

        internal string CheckAppointmentKind(EcTmpUpdateAppointmentRequest updateRequest)
        {
            return string.IsNullOrEmpty(updateRequest.AppointmentKind.ToString()) ? "Appointment Kind cannot be null|" : string.Empty;
        }

        internal string CheckPatLocation(EcTmpPatients patient, EcTmpAppointmentKind appointmentKind)
        {
            var errors = new StringBuilder();
            if (patient.Location == null) errors.Append("Patient Location information must be specified for all appointments|");
            else
            {
                switch (appointmentKind)
                {
                    case EcTmpAppointmentKind.CLINIC_BASED:
                    case EcTmpAppointmentKind.STORE_FORWARD:
                        if (patient.Location.Type != EcTmpLocationType.VA)
                            errors.Append(string.Format("Patient Location Type must be VA for {0} appointments|", appointmentKind));
                        errors.Append(CheckFacilityAndClinic(patient.Location));
                        break;
                    case EcTmpAppointmentKind.MOBILE_ANY:
                    case EcTmpAppointmentKind.MOBILE_GFE:
                        if (patient.Location.Type == EcTmpLocationType.VA)
                            errors.Append(string.Format("Patient Location Type must be NON-VA for {0} appointments|", appointmentKind));
                        break;
                }
            }
            return errors.ToString();
        }

        internal string CheckProvLocation(EcTmpProviders provider, EcTmpAppointmentKind appointmentKind)
        {
            var errors = new StringBuilder();
            switch (appointmentKind)
            {
                case EcTmpAppointmentKind.CLINIC_BASED:
                case EcTmpAppointmentKind.MOBILE_ANY:
                case EcTmpAppointmentKind.MOBILE_GFE:
                    if (provider.Location == null || provider.Location.Type != EcTmpLocationType.VA)
                        errors.Append("Provider Location type must be VA for all appointments except for Store Forward|");
                    errors.Append(CheckFacilityAndClinic(provider.Location));
                    break;
                case EcTmpAppointmentKind.STORE_FORWARD:
                    errors.Append("Providers should not be booked on Store Forward appointments|");
                    break;
            }

            return errors.ToString();
        }

        internal string CheckLocationTypes(EcTmpUpdateAppointmentRequest updateRequest)
        {
            var errors = new StringBuilder();
            foreach (var pat in updateRequest.Patients) errors.Append(CheckPatLocation(pat, (EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), updateRequest.AppointmentKind, true)));
            foreach (var prov in updateRequest.Providers) errors.Append(CheckProvLocation(prov, (EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), updateRequest.AppointmentKind, true)));
            return errors.ToString();
        }

        internal string CheckLocationTypes(EcTmpCreateAppointmentRequestData createRequest)
        {
            var errors = new StringBuilder();
            foreach (var pat in createRequest.Patients) errors.Append(CheckPatLocation(pat, (EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), createRequest.AppointmentKind, true)));
            foreach (var prov in createRequest.Providers) errors.Append(CheckProvLocation(prov, (EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), createRequest.AppointmentKind, true)));
            return errors.ToString();
        }

        internal string CheckVmrs(EcTmpCreateAppointmentRequestData createRequest, ServiceAppointment serviceAppointment)
        {
            const int SFT = 178970001;
            const int CVT = 178970000;
            const int CVTGRP = 178970006;
            const int SIP_DEVICE = 100000000;

            var apptmodality = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
            var technologyType = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_technologytype");
            var errors = new StringBuilder();
            if (apptmodality != null && (apptmodality.Value == CVT || apptmodality.Value == CVTGRP || apptmodality.Value == SFT)) return errors.ToString();
            else if (technologyType != null && technologyType.Value != SIP_DEVICE)
            {
                foreach (var pat in createRequest.Patients)
                {
                    errors.Append(CheckForVmr(pat));
                }
            }
            return errors.ToString();
        }

        internal string CheckVmrs(EcTmpUpdateAppointmentRequest updateRequest, ServiceAppointment serviceAppointment)
        {
            const int SFT = 178970001;
            const int CVT = 178970000;
            const int CVTGRP = 178970006;
            const int SIP_DEVICE = 100000000;

            var apptmodality = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
            var technologyType = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_technologytype");
            var errors = new StringBuilder();
            if (apptmodality != null && (apptmodality.Value == CVT || apptmodality.Value == CVTGRP || apptmodality.Value == SFT)) return errors.ToString();
            else if (technologyType != null && technologyType.Value != SIP_DEVICE)
            {
                foreach (var pat in updateRequest.Patients)
                {
                    errors.Append(CheckForVmr(pat));
                }
            }
            return errors.ToString();
        }

        /* TO DO - Old for reference. Remove in next release
         * 
        internal string CheckVmrs(EcTmpUpdateAppointmentRequest updateRequest, ServiceAppointment serviceAppointment)
        {
            var errors = new StringBuilder();
            if ((EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), updateRequest.AppointmentKind, true) != EcTmpAppointmentKind.MOBILE_ANY) return errors.ToString();
            foreach (var pat in updateRequest.Patients)
            {
                errors.Append(CheckForVmr(pat));
            }
            return errors.ToString();
        }
        */

        private static string CheckForVmr(EcTmpPatients patient)
        {
            if (patient.VirtualMeetingRoom == null) return "Patient VMR cannot be null|";
            if (string.IsNullOrEmpty(patient.VirtualMeetingRoom.Conference) || string.IsNullOrEmpty(patient.VirtualMeetingRoom.Pin) || string.IsNullOrEmpty(patient.VirtualMeetingRoom.Url))
                return "Patient VMR details must all be populated|";
            return string.Empty;
        }

        internal string ValidateCancelRequest(EcTmpCancelAppointmentRequest request)
        {
            var errors = new StringBuilder();
            if (string.IsNullOrEmpty(request.Id))
                errors.Append("Appointment Id must be populated for a Cancel Request|");
            if (request.PatientBookingStatuses == null || request.PatientBookingStatuses.PersonBookingStatus.Count() == 0)
            {
                errors.Append("At least one patient cancellation must be sent in all Cancel Requests|");
                return errors.ToString();
            }

            foreach (var booking in request.PatientBookingStatuses.PersonBookingStatus)
            {
                if (string.IsNullOrEmpty(booking.Id?.AssigningAuthority) || booking.Id.AssigningAuthority != "ICN" || string.IsNullOrEmpty(booking.Id.UniqueId))
                    errors.Append("All Patients being canceled must have an ID and an ICN|");
                if (booking.Status == null) errors.Append("All cancellations must have a cancel status and reason|");

                if (booking.Status == null) errors.Append("Cancel Booking Status cannot be null");
                if (booking.Status != null && booking.Status.Reason != EcTmpReasonCode.OTHER) errors.Append("Cancel reason should = 'Other'|");
            }
            return errors.ToString();
        }

        internal string CheckAppointmentDateUnchanged(EcTmpUpdateAppointmentRequest updateRequest, VideoVisitUpdateStateObject state)
        {
            var errors = new StringBuilder();
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var results = !state.IsGroup
                    ? srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_ServiceActivity.Id == state.ServiceAppointment.Id && vir.cvt_VistAStatus == EcTmpVistaStatus.RECEIVED.ToString())
                    : srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_Appointment.Id == state.CrmAppointment.Id && vir.cvt_VistAStatus == EcTmpVistaStatus.RECEIVED.ToString());
                foreach (var result in results)
                {

                    if (result == null)
                        return "";
                    if (result.cvt_DateTime.Replace(" GMT", "") != state.Appointment.DateTime)
                        errors.Append("Cannot change time of appointment once it has been booked in vista without canceling the Vista Appointment First|");
                    if (state.Appointment?.Providers?.FirstOrDefault() == null || state.Appointment.Providers.FirstOrDefault().Location == null || state.Appointment.Providers.FirstOrDefault().Location.Facility.SiteCode == null)
                    {
                        if ((EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), updateRequest.AppointmentKind, true) != EcTmpAppointmentKind.STORE_FORWARD)
                            errors.Append("Cannot have a null provider|");
                    }
                    //if (state)
                    if ((EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), updateRequest.AppointmentKind, true) != EcTmpAppointmentKind.STORE_FORWARD && result.cvt_FacilityCode != state.Appointment.Providers.FirstOrDefault().Location.Facility.SiteCode)
                        errors.Append("Cannot change the facility for the provider or patient|");

                }
            }

            state.OrganizationServiceProxy.Retrieve(state.ServiceAppointment.LogicalName, state.ServiceAppointment.Id, new ColumnSet(true)).ToEntity<ServiceAppointment>();
            return errors.ToString();
        }

        internal string GetPatientName(EcTmpPatients patient)
        {
            return patient.Name == null ? "unknown name" : Convert(patient.Name);
        }

        internal string Convert(EcTmpPersonName name)
        {
            return string.Format("{0}, {1} {2}", name.LastName, name.FirstName, name.MiddleInitial);
        }
    }
}
