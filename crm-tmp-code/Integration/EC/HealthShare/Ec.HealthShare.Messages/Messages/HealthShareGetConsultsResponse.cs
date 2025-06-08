using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ec.HealthShare.Messages
{
    /// <summary>
    /// EC HealthShare Get Consults Response.
    /// </summary>
    [DataContract]
    public class EcHealthShareGetConsultsResponse
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public EcHealthShareGetConsultsResponse()
        {
            MessageId = Guid.NewGuid().ToString();
            Consults = new List<EcConsult>();
            ReturnToClinic = new List<EcReturnToClinicOrder>();
        }

        /// <summary>
        /// Gets or sets the Message Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets ControlId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets Patient DFN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientDfn { get; set; }

        /// <summary>
        /// Gets or sets Patient ICN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIcn { get; set; }

        /// <summary>
        /// Gets or sets Query Name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string QueryName { get; set; }

        /// <summary>
        /// Gets or sets Institution.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Institution { get; set; }

        /// <summary>
        /// Gets or sets Pending Consults.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<EcConsult> Consults { get; set; }

        /// <summary>
        /// Gets or sets Return to Clinic Orders.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<EcReturnToClinicOrder> ReturnToClinic { get; set; }

        /// <summary>
        /// Gets or sets whether Exception occurred.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets or sets Exception message.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }
    }

    /// <summary>
    /// Pending Consult Class.
    /// </summary>
    [DataContract]
    public class EcConsult
    {
        /// <summary>
        /// Gets or sets ConsultId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultId { get; set; }

        /// <summary>
        /// Gets or sets Consult Request Date.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultRequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets To Consult Service.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ToConsultService { get; set; }

        /// <summary>
        /// Gets or sets Consult Title.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultTitle { get; set; }

        /// <summary>
        /// Gets or sets Consult Status.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultStatus { get; set; }

        /// <summary>
        /// Gets or sets Clinically Indicated Date.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicallyIndicatedDate { get; set; }

        /// <summary>
        /// Gets or sets Stop Codes.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StopCodes { get; set; }

        /// <summary>
        /// Gets or sets Provider.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets Receiving Site ConsultId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ReceivingSiteConsultId { get; set; }
    }

    /// <summary>
    /// Return to Clinic Order.
    /// </summary>
    [DataContract]
    public class EcReturnToClinicOrder
    {
        /// <summary>
        /// Gets or sets RTC Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RtcId { get; set; }

        /// <summary>
        /// Gets or sets RTC Request DateTime.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RtcRequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets Clinic IEN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ToClinicIen { get; set; }

        /// <summary>
        /// Gets or sets Clinic Name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicName { get; set; }

        /// <summary>
        /// Gets or sets Clinically Indicated Date.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicallyIndicatedDate { get; set; }

        /// <summary>
        /// Gets or sets Stop Codes.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StopCodes { get; set; }

        /// <summary>
        /// Gets or sets Provider.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets Comments.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets Multiple Rtc.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MultiRtc { get; set; }
    }
}