using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.Mvi
{
    /// <summary>
    /// Message for ProxyAddResponseMessage.
    /// </summary>
    [DataContract]
    public class GetPersonIdentifiersResponseMessage : TmpBaseResponseMessage
    {
        /// <summary>
        /// The URL for the contact record associated with these identifiers.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Url { get; set; }

        /// <summary>
        /// The type of message processing.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MessageProcessType FetchMessageProcessType { get; set; }

        /// <summary>
        /// The social security number of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Ss { get; set; }

        /// <summary>
        /// The EDIPI number of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Edipi { get; set; }

        /// <summary>
        /// The participant identifier of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ParticipantId { get; set; }

        /// <summary>
        /// The first name of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FirstName { get; set; }

        /// <summary>
        /// The middle name of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MiddleName { get; set; }

        /// <summary>
        /// The last name of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FamilyName { get; set; }

        /// <summary>
        /// suffix of the veteran
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Suffix { get; set; }

        /// <summary>
        /// The CRM user id of the requestor.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid UserId { get; set; }

        /// <summary>
        /// The full address for the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FullAddress { get; set; }

        /// <summary>
        /// The data of birth of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DateofBirth { get; set; }

        /// <summary>
        /// The full name of the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the Contact Id.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid ContactId { get; set; }

        /// <summary>
        /// An array of identifiers associated with the veteran.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<UnattendedSearchRequest> CorrespondingIdList { get; set; }
    }
}