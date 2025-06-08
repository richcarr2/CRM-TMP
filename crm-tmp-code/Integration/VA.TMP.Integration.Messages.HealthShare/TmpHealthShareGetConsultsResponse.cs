using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    /// <summary>
    /// Tmp HealthShare Get Consults Response.
    /// </summary>
    [DataContract]
    public class TmpHealthShareGetConsultsResponse : TmpBaseResponseMessage
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TmpHealthShareGetConsultsResponse()
        {
            PatientConsults = new List<TmpConsult>();
            ProviderConsults = new List<TmpConsult>();
            PatientReturnToClinicOrders = new List<TmpReturnToClinicOrder>();
            ProviderReturnToClinicOrders = new List<TmpReturnToClinicOrder>();
        }

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
        /// Gets or sets Patient Consults.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<TmpConsult> PatientConsults { get; set; }

        /// <summary>
        /// Gets or sets Provider Consults.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<TmpConsult> ProviderConsults { get; set; }

        /// <summary>
        /// Gets or sets Patient Return to Clinic Orders.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<TmpReturnToClinicOrder> PatientReturnToClinicOrders { get; set; }

        /// <summary>
        /// Gets or sets Provider Return to Clinic Orders.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<TmpReturnToClinicOrder> ProviderReturnToClinicOrders { get; set; }
    }

    /// <summary>
    /// Pending Consults.
    /// </summary>
    [DataContract]
    public class TmpConsult
    {
        /// <summary>
        /// Gets or sets ConsultId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConsultId { get; set; }

        /// <summary>
        /// Gets or sets ControlId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UniqueRequestId { get; set; }

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
    public class TmpReturnToClinicOrder
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