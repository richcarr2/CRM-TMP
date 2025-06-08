using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.Mvi
{
    /// <summary>
    /// Message for GetPersonIdentifiersRequestMessage.
    /// </summary>
    [DataContract]
    public class GetPersonIdentifiersRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public GetPersonIdentifiersRequestMessage()
        {
            CorrespondingIds = new UnattendedSearchRequest[] { };
        }

        /// <summary>
        /// A string containing all the other names the person is known by.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the search identifier to use when the user clicks a record from search results grid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientSearchIdentifier { get; set; }

        /// <summary>
        /// The class code of the identifier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IdentifierClassCode { get; set; }

        /// <summary>
        /// NI - National Identifier 
        /// PI - Patient Identifier
        /// EI - Employee Identifier
        /// PN - Patient Number 
        /// SS – Social Security
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IdentifierType { get; set; }

        /// <summary>
        /// This is the organizationn identifier -- similar to the identifier for CRMe, which is "200CMRE"
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AssigningFacility { get; set; }

        /// <summary>
        /// If the search is with SS, the authority is SSA, if it's with the VA then the value is VHA, etc.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AssigningAuthority { get; set; }

        /// <summary>
        /// Returns the Source ID for the MVI search. Source Id is based on the combination of the
        /// "PatientSearchIdentifier^IdentifierType^AssigningFacility^AssigningAuthority". Not setting
        /// the values for IdentifierType, AssigningFacility and AssigningAuthority returns the DOD Source Id as default.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}^{1}^{2}^{3}", PatientSearchIdentifier,
                string.IsNullOrEmpty(IdentifierType) ? "NI" : IdentifierType,
                string.IsNullOrEmpty(AssigningFacility) ? "200DOD" : AssigningFacility,
                string.IsNullOrEmpty(AssigningAuthority) ? "USDOD" : AssigningAuthority);
        }

        /// <summary>
        /// The authorized user's first name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserFirstName { get; set; }

        /// <summary>
        /// The authorized user's last name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserLastName { get; set; }

        /// <summary>
        /// The given or first name of the person.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FirstName { get; set; }

        /// <summary>
        /// Any additional given names of the person.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiddleName { get; set; }

        /// <summary>
        /// The family or last name of the person.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FamilyName { get; set; }

        /// <summary>
        /// The full address of the person associated with the identifier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FullAddress { get; set; }

        /// <summary>
        /// The full name of the person associated with the identifier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FullName { get; set; }

        /// <summary>
        /// The date of birth of the person associated with the identifier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DateofBirth { get; set; }

        /// <summary>
        /// The date of death of the person associated with the identifier.
        /// </summary> 
        [DataMember(EmitDefaultValue = false)]
        public string DateofDeath { get; set; }

        /// <summary>
        /// This is the raw value retrieved from MVI.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RawValueFromMvi { get; set; }

        /// <summary>
        /// use MVI Raw value
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool UseRawMviValue { get; set; }

        /// <summary>
        /// The processing mode for the message.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MessageProcessType FetchMessageProcessType { get; set; }

        /// <summary>
        /// The system that provided this information.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RecordSource { get; set; }

        /// <summary>
        /// The social security number of the person associated with the identifier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SocialSecurityNumber { get; set; }

        /// <summary>
        /// The VA participant identifier of the person associated with the identifier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Edipi { get; set; }

        /// <summary>
        /// A phone number associated with this person.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The gender of the person
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Gender { get; set; }

        /// <summary>
        /// Yes if this identity is suspect; No otherwise.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IdTheftIndicator { get; set; }

        /// <summary>
        /// Type of Fake Response
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SelectedPersonFakeResponseType { get; set; }

        /// <summary>
        /// Gets or sets the Server Name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ServerName { get; set; }

        /// <summary>
        /// Used to hold corresponding identifiers returned from MVI search.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UnattendedSearchRequest[] CorrespondingIds { get; set; }
    }
}