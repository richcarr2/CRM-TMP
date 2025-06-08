using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.HealthShare
{
    /// <summary>
    /// Message for TmpHealthShareUpdateClinicRequestMessage.
    /// </summary>
    [DataContract]
    public class TmpHealthShareUpdateClinicRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets the Institution.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Institution { get; set; }

        /// <summary>
        /// Gets or sets the Visn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Visn { get; set; }

        /// <summary>
        /// Gets or sets the StationNumber.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StationNumber { get; set; }

        /// <summary>
        /// Gets or sets the PrimaryStopCode.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int PrimaryStopCode { get; set; }

        /// <summary>
        /// Gets or sets the SecondaryStopCode.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int SecondaryStopCode { get; set; }

        /// <summary>
        /// Gets or sets the ClinicIen.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int ClinicIen { get; set; }

        /// <summary>
        /// Gets or sets the ClinicName.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicName { get; set; }

        /// <summary>
        /// Gets or sets the TreatingSpecialty.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string TreatingSpecialty { get; set; }

        /// <summary>
        /// Gets or sets the Service.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the DefaultProviderId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DefaultProviderId { get; set; }

        /// <summary>
        /// Gets or sets the DefaultProviderName.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DefaultProviderName { get; set; }

        /// <summary>
        /// Gets or sets the DefaultProviderEmail.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DefaultProviderEmail { get; set; }

        /// <summary>
        /// Gets or sets the OverBookAllowed.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int OverBookAllowed { get; set; }

        /// <summary>
        /// Gets or sets the ClinicStatus.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ClinicStatus { get; set; }

        /// <summary>
        /// Gets or sets the ActionDateTime.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ActionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PrivilegedUsers.
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [IgnoreDataMember]
        public List<PrivilegedUser> PrivilegedUsers { get; set; }
    }

    /// <summary>
    /// PrivilegedUser class.
    /// </summary>
    [DataContract]
    public class PrivilegedUser
    {
        /// <summary>
        /// Gets or sets the User.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the UserDuz.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserDuz { get; set; }

        /// <summary>
        /// Gets or sets the UserEmail.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserEmail { get; set; }
    }
}