using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.Mvi
{
    /// <summary>
    /// Message for PersonSearchRequestMessage.
    /// </summary>
    [DataContract]
    public class PersonSearchRequestMessage : TmpBaseRequestMessage
    {
        /// <summary>
        /// Gets or sets birth date.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string BirthDate { get; set; }

        /// <summary>
        /// Gets or sets last name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets first name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets Fetch Message Process Type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MessageProcessType FetchMessageProcessType { get; set; }

        /// <summary>
        /// Gets or sets whether attended search.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsAttended { get; set; }

        /// <summary>
        /// Gets or sets Patient Identifier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PatientIdentifier { get; set; }

        /// <summary>
        /// Gets or sets social security number.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Ss { get; set; }

        /// <summary>
        /// Gets or sets user first name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserFirstName { get; set; }

        /// <summary>
        /// Gets or sets user last name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserLastName { get; set; }

        /// <summary>
        /// Gets or sets middle name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets EDIPI.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Edipi { get; set; }

        /// <summary>
        /// Gets or sets phone number.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets Search Use.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SearchUse { get; set; }

        /// <summary>
        /// Gets or sets Query.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets Person Search Fake Response Type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PersonSearchFakeResponseType { get; set; }
    }
}