using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ec.VideoVisit.Messages
{
    [DataContract]
    public class EcTmpPatients
    {
        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonIdentifier Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonName Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpContactInformation ContactInformation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpLocation Location { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpVirtualMeetingRoom VirtualMeetingRoom { get; set; }

        [DataMember(EmitDefaultValue = true, Name = "hasMobileGFE")]
        public bool HasMobileGFE { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool VistaDateTimeSpecified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime VistaDateTime { get; set; }
    }

    [DataContract]
    public class EcTmpPersonIdentifier
    {
        [DataMember(EmitDefaultValue = false)]
        public string AssigningAuthority { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UniqueId { get; set; }
    }

    [DataContract]
    public class EcTmpProviders
    {
        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonName Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonIdentifier Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpContactInformation ContactInformation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpVirtualMeetingRoom VirtualMeetingRoom { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpLocation Location { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool VistaDateTimeSpecified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime VistaDateTime { get; set; }
    }

    [DataContract]
    public class EcTmpPersonName
    {
        [DataMember(EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MiddleInitial { get; set; }
    }

    [DataContract]
    public class EcTmpContactInformation
    {
        [DataMember(EmitDefaultValue = false)]
        public string Mobile { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PreferredEmail { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AlternativeEmail { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TimeZone { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool TimeZoneSpecified { get; set; }
    }

    [DataContract]
    public class EcTmpVirtualMeetingRoom
    {
        [DataMember(EmitDefaultValue = false)]
        public string Conference { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Pin { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Url { get; set; }
    }

    [DataContract]
    public class EcTmpLocation
    {
        [DataMember(EmitDefaultValue = false)]
        public EcTmpLocationType Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpFacility Facility { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpClinic Clinic { get; set; }
    }

    [DataContract]
    public class EcTmpFacility
    {
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SiteCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TimeZone { get; set; }
    }

    [DataContract]
    public class EcTmpClinic
    {
        [DataMember(EmitDefaultValue = false)]
        public string Ien { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }
    }

    [DataContract]
    public class EcTmpStatus
    {
        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpStatusCode Code { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool CodeSpecified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpReasonCode Reason { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ReasonSpecified { get; set; }
    }

    public enum EcTmpStatusCode
    {
        CHECKED_IN,
        CHECKED_OUT,
        NO_ACTION_TAKEN,
        NO_SHOW,
        CANCELLED_BY_CLINIC,
        NO_SHOW_AND_AUTO_RE_BOOK,
        CANCELLED_BY_CLINIC_AND_AUTO_RE_BOOK,
        INPATIENT_APPOINTMENT,
        CANCELLED_BY_PATIENT,
        CANCELLED_BY_PATIENT_AND_AUTO_REBOOK,
        FUTURE,
        NON_COUNT,
        DELETED,
        ACTION_REQUIRED
    }

    public enum EcTmpReasonCode
    {
        APPOINTMENT_NO_LONGER_REQUIRED,
        CLINIC_CANCELLED,
        CLINIC_STAFFING,
        DEATH_IN_FAMILY,
        INPATIENT_STATUS,
        OTHER,
        PATIENT_DEATH,
        PATIENT_NOT_ELIGIBLE,
        SCHEDULING_CONFLICT_OR_ERROR,
        TRANSFER_OPT_CARE_TO_OTHER_VA,
        TRAVEL_DIFFICULTY,
        UNABLE_TO_KEEP_APPOINTMENT,
        WEATHER
    }

    [DataContract]
    public class EcTmpWriteResults
    {
        [DataMember(EmitDefaultValue = false)]
        public List<EcTmpWriteResult> EcTmpWriteResult { get; set; }
    }

    [DataContract]
    public class EcTmpWriteResult
    {
        [DataMember(EmitDefaultValue = false)]
        public string PersonId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpPersonName Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FacilityCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FacilityName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ClinicIen { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ClinicName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime DateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EcTmpVistaStatus VistaStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Reason { get; set; }
    }

    public enum EcTmpVistaStatus
    {
        BOOKED,
        FAILED_TO_BOOK,
        RECEIVED,
        FAILED_TO_RECEIVE,
        CANCELLED,
        FAILED_TO_CANCEL
    }

    public enum EcTmpSchedulingRequestType
    {
        NEXT_AVAILABLE_APPT,
        OTHER_THAN_NEXT_AVAILABLE_CLINICIAN_REQUESTED,
        OTHER_THAN_NEXT_AVAILABLE_PATIENT_REQUESTED,
        WALKIN_APPTOINTMENT,
        MULTIPLE_APPTOINTMENT_BOOKING,
        AUTO_REBOOK,
        OTHER_THAN_NEXT_AVAILABLE_APPOINTMENT
    }

    public enum EcTmpAppointmentType
    {
        COMPENSATION_AND_PENSION,
        CLASS_II_DENTAL,
        ORGAN_DONORS,
        EMPLOYEE,
        PRIMA_FACIA,
        RESEARCH,
        COLLATERAL_OF_VET,
        SHARING_AGREEMENT,
        REGULAR,
        COMPUTER_GENERATED,
        SERVICE_CONNECTED
    }

    public enum EcTmpAppointmentKind
    {
        CLINIC_BASED,
        STORE_FORWARD,
        MOBILE_ANY,
        MOBILE_GFE,
        ADHOC
    }

    public enum EcTmpLocationType
    {
        VA,
        NonVA
    }
}