using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    /// <summary>
    /// Message for TmpHealthShareMakeAndCancelAppointmentRequestMessage.
    /// </summary>
    [DataContract]
    public class TmpHealthShareMakeAndCancelAppointmentRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the PatientIcn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIcn { get; set; }

        /// <summary>
        /// Gets or sets the Duration.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the StartTime.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StartTime { get; set; }

        /// <summary>
        /// Gets or sets the VisitStatus.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VisitStatus { get; set; }

        /// <summary>
        /// Gets or sets the Facility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Facility { get; set; }

        /// <summary>
        /// Gets or sets the Facility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StationNumber { get; set; }

        /// <summary>
        /// Gets or sets the ClinicIen.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicIen { get; set; }

        /// <summary>
        /// Gets or sets the ClinicName.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicName { get; set; }

        /// <summary>
        /// Gets or sets the ProviderEmail.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProviderEmail { get; set; }

        /// <summary>
        /// Gets or sets the ConsultId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultId { get; set; }

        /// <summary>
        /// Gets or sets the ConsultName.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultName { get; set; }

        /// <summary>
        /// Gets or sets the CancelReason.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CancelReason { get; set; }

        /// <summary>
        /// Gets or sets the CancelCode.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CancelCode { get; set; }

        /// <summary>
        /// Gets or sets the CancelRemarks.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CancelRemarks { get; set; }

        /// <summary>
        /// Gets or sets the SchedulerName.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SchedulerName { get; set; }

        /// <summary>
        /// Gets or sets the SchedulerEmail.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SchedulerEmail { get; set; }
    }
}