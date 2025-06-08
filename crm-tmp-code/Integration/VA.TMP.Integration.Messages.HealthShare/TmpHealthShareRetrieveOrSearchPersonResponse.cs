using System;
using System.Runtime.Serialization;

namespace VA.TMP.Integration.Messages.HealthShare
{
    [DataContract]
    public class TmpHealthShareRetrieveOrSearchPersonResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Acknowledgement Acknowledgement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MessageProcessType FetchMessageProcessType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string OrganizationName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public PatientPerson[] Person { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public QueryAcknowledgement QueryAcknowledgement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RawMviExceptionMessage { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Acknowledgement
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AcknowledgementDetail[] AcknowledgementDetails { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string TargetMessage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string TypeCode { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class AcknowledgementDetail
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AcknowledgementDetailCode Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Text { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class AcknowledgementDetailCode
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CodeSystemName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public enum MessageProcessType
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Local = 0,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Remote = 1,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        RemoteQueued = 2,
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PatientPerson
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public PatientAddress Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string BirthDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string BranchOfService { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UnattendedSearchRequest[] CorrespondingIdList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DeceasedDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string EdiPi { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GenderCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Identifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IdentifierType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IdentifyTheft { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IsDeceased { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Name[] NameList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ParticipantId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RecordSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Ss { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StatusCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Url { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PatientAddress
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string City { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Country { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PostalCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string State { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StreetAddressLine { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AddressUse Use { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}\n{1}, {2} {3} {4}", StreetAddressLine, City, State, PostalCode, Country).TrimEnd(new[] { '\n', ',' }).Trim();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public enum AddressUse
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Unspecified = 0,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Bad = 1,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Confidential = 2,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Home = 3,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        PrimaryHome = 4,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        OtherHome = 5,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Temporary = 6,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Workplace = 7,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Other = 8,
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class UnattendedSearchRequest
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AssigningAuthority { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AssigningFacility { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AuthorityOid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MessageProcessType FetchMessageProcessType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IdentifierType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string OrganizationName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIdentifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RawValueFromMvi { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool UseRawMviValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserFirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserLastName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}^{1}^{2}^{3}", PatientIdentifier, IdentifierType, AssigningFacility, AssigningAuthority);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Name
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FamilyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GivenName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiddleName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NamePrefix { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NameSuffix { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NameType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public NameUse Use { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var nameString = string.Empty;

            if (!string.IsNullOrEmpty(FamilyName))
            {
                nameString += FamilyName;

                if (!string.IsNullOrEmpty(NameSuffix)) nameString += " " + NameSuffix;

                nameString += ",";
            }

            if (!string.IsNullOrEmpty(GivenName)) nameString += " " + GivenName;

            if (!string.IsNullOrEmpty(MiddleName)) nameString += " " + MiddleName;

            return nameString.Trim().TrimEnd(',');
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public enum NameUse
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Unspecified = 0,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Assigned = 1,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Certificate = 2,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        OfficialRegistry = 3,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Indigenous = 4,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Legal = 5,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Pseudoymn = 6,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Religous = 7,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Maiden = 8,

        /// <summary>
        /// 
        /// </summary>
        [EnumMember]
        Alias = 9,
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class QueryAcknowledgement
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string QueryResponseCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ResultCurrentQuantity { get; set; }
    }
}
