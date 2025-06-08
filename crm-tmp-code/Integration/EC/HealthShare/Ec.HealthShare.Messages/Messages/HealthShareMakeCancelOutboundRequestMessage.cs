using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ec.HealthShare.Messages
{
    /// <summary>
    /// EC message for EcHealthShareMakeCancelOutboundRequestMessage.
    /// </summary>
    [DataContract]
    public class EcHealthShareMakeCancelOutboundRequestMessage
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EcHealthShareMakeCancelOutboundRequestMessage()
        {
            MessageId = Guid.NewGuid().ToString();
            Patient = new List<Patient>();
            Provider = new List<Provider>();
        }

        /// <summary>
        /// Gets or sets the Message Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the Organization Name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets ControlId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets SignOnFacility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SignOnFacility { get; set; }

        /// <summary>
        /// Gets or sets SecondFacility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SecondFacility { get; set; }

        /// <summary>
        /// Gets or Sets PatientIcn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIcn { get; set; }

        /// <summary>
        /// Gets or Sets the Patient Name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientName { get; set; }

        /// <summary>
        /// Gets or Sets Duration.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Duration { get; set; }

        /// <summary>
        /// Gets or Sets StartTime.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StartTime { get; set; }

        /// <summary>
        /// Gets or Sets VisitStatus.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VisitStatus { get; set; }

        /// <summary>
        /// Gets or Sets CancelReason.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CancelReason { get; set; }

        /// <summary>
        /// Gets or Sets CancelCode.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CancelCode { get; set; }

        /// <summary>
        /// Gets or Sets CancelRemarks.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CancelRemarks { get; set; }

        /// <summary>
        /// Gets or Sets Scheduler SecId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SchedulerSecId { get; set; }

        /// <summary>
        /// Gets or Sets SchedulerEmail.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SchedulerEmail { get; set; }

        /// <summary>
        /// Gets or Sets Comments.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Comments { get; set; }

        /// <summary>
        /// Gets or Sets VvdUrl.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VvdUrl { get; set; }

        /// <summary>
        /// Gets or Sets Cid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicallyIndicatedDate { get; set; }

        /// <summary>
        /// Gets or Sets Patient.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Patient> Patient { get; set; }

        /// <summary>
        /// Gets or Sets Providers.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Provider> Provider { get; set; }
    }

    /// <summary>
    /// Patient.
    /// </summary>
    [DataContract]
    public class Patient
    {
        /// <summary>
        /// Gets or Sets Facility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Facility { get; set; }

        /// <summary>
        /// Gets or Sets ClinicIen.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicIen { get; set; }

        /// <summary>
        /// Gets or Sets ClinicDefaultProviderDuz.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicDefaultProviderDuz { get; set; }

        /// <summary>
        /// Gets or Sets ProviderEmail.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProviderEmail { get; set; }

        /// <summary>
        /// Gets or Sets ConsultId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultId { get; set; }

        /// <summary>
        /// Gets or Sets ConsultTitle.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultTitle { get; set; }

        /// <summary>
        /// Gets or Sets RtcId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RtcId { get; set; }

        /// <summary>
        /// Gets or Sets Rtc parent.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RtcParent { get; set; }
    }

    /// <summary>
    /// Provider.
    /// </summary>
    [DataContract]
    public class Provider
    {
        /// <summary>
        /// Gets or Sets Facility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Facility { get; set; }

        /// <summary>
        /// Gets or Sets ClinicIen.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicIen { get; set; }

        /// <summary>
        /// Gets or Sets ClinicDefaultProviderDuz.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicDefaultProviderDuz { get; set; }

        /// <summary>
        /// Gets or Sets ProviderEmail.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProviderEmail { get; set; }

        /// <summary>
        /// Gets or Sets ConsultId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultId { get; set; }

        /// <summary>
        /// Gets or Sets ConsultTitle.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultTitle { get; set; }

        /// <summary>
        /// Gets or Sets RtcId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RtcId { get; set; }

        /// <summary>
        /// Gets or Sets Rtc parent.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RtcParent { get; set; }
    }
}