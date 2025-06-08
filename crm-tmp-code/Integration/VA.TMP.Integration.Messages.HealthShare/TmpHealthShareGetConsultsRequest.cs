using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    /// <summary>
    /// TMP HealthShare Get Consults Request.
    /// </summary>
    [DataContract]
    public class TmpHealthShareGetConsultsRequest : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets Patient DFN for the Patient Facility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientStationDfn { get; set; }

        /// <summary>
        /// Gets or sets Patient DFN for the Provider Facility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProviderStationDfn { get; set; }

        /// <summary>
        /// Gets or sets Patient ICN.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIcn { get; set; }

        /// <summary>
        /// Gets or sets Patient Station Number.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int PatientLoginStationNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> PatientIds { get; set; }

        /// <summary>
        /// Gets or sets Provider Site Station Number.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int ProviderLoginStationNumber { get; set; }

        /// <summary>
        /// Gets or sets whether is Home Mobile.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsHomeMobile { get; set; }

        /// <summary>
        /// Gets or sets whether is Store Forward.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsStoreForward { get; set; }
    }
}