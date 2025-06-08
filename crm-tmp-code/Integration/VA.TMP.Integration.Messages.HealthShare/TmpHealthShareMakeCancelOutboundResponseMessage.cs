using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    /// <summary>
    /// Tmp HealthShare Make Cancel Outbound Response.
    /// </summary>
    [DataContract]
    public class TmpHealthShareMakeCancelOutboundResponseMessage : TmpBaseResponseMessage
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TmpHealthShareMakeCancelOutboundResponseMessage()
        {
            PatientIntegrationResultInformation = new List<PatientIntegrationResultInformation>();
        }

        /// <summary>
        /// Gets or sets ControlId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<PatientIntegrationResultInformation> PatientIntegrationResultInformation { get; set; }
    }

    /// <summary>
    /// Contains information needed to build an Integration Result.
    /// </summary>
    [DataContract]
    public class PatientIntegrationResultInformation
    {
        /// <summary>
        /// Gets or sets the Control Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the PatientId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Gets or sets the Request.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VimtRequest { get; set; }

        /// <summary>
        /// Gets or sets the Response.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string VimtResponse { get; set; }

        /// <summary>
        /// Gets or sets EcProcessing time.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int EcProcessingMs { get; set; }

        /// <summary>
        /// Gets or sets Exception Message.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets whether an Exception occurred.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }
    }
}