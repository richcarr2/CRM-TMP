using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class GetPersonIdentifiersStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="getPersonIdentifiersRequestMessage">GetPersonIdentifiersRequestMessage instance.</param>
        public GetPersonIdentifiersStateObject(GetPersonIdentifiersRequestMessage getPersonIdentifiersRequestMessage)
        {
            OrganizationName = getPersonIdentifiersRequestMessage.OrganizationName;
            UserId = getPersonIdentifiersRequestMessage.UserId;
            UserFirstName = getPersonIdentifiersRequestMessage.UserFirstName;
            UserLastName = getPersonIdentifiersRequestMessage.UserLastName;
            LogRequest = getPersonIdentifiersRequestMessage.LogRequest;
            CorrespondingIds = new List<Messages.Mvi.UnattendedSearchRequest>();
            CorrespondingIds.AddRange(getPersonIdentifiersRequestMessage.CorrespondingIds);
            Edipi = getPersonIdentifiersRequestMessage.Edipi;
            DateOfBirth = getPersonIdentifiersRequestMessage.DateofBirth;
            FullName = getPersonIdentifiersRequestMessage.FullName;
            FullAddress = getPersonIdentifiersRequestMessage.FullAddress;
            RawMviValue = getPersonIdentifiersRequestMessage.RawValueFromMvi;
            RecordSource = getPersonIdentifiersRequestMessage.RecordSource;
            IdentifierClassCode = getPersonIdentifiersRequestMessage.IdentifierClassCode;
            IdentifierType = getPersonIdentifiersRequestMessage.IdentifierType;
            Ss = getPersonIdentifiersRequestMessage.SocialSecurityNumber;
            PatientSearchIdentifier = getPersonIdentifiersRequestMessage.PatientSearchIdentifier;
            SelectedPersonFakeResponseType = getPersonIdentifiersRequestMessage.SelectedPersonFakeResponseType;
            DateOfDeath = getPersonIdentifiersRequestMessage.DateofDeath;
            IdTheftIndicator = getPersonIdentifiersRequestMessage.IdTheftIndicator;
            Alias = getPersonIdentifiersRequestMessage.Alias;
            PhoneNumber = getPersonIdentifiersRequestMessage.PhoneNumber;
            Gender = getPersonIdentifiersRequestMessage.Gender;
            ServerName = getPersonIdentifiersRequestMessage.ServerName;
            FirstName = getPersonIdentifiersRequestMessage.FirstName;
            FamilyName = getPersonIdentifiersRequestMessage.FamilyName;
        }

        /// <summary>
        /// Gets or sets the CRM organization name.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the UserId.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets User's first name.
        /// </summary>
        public string UserFirstName { get; set; }

        /// <summary>
        /// Gets or sets User's last name.
        /// </summary>
        public string UserLastName { get; set; }

        /// <summary>
        /// Gets or sets whether to log the request.
        /// </summary>
        public bool LogRequest { get; set; }

        /// <summary>
        /// Gets or sets the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public bool UseFakeResponse { get; set; }

        /// <summary>
        /// Gets or sets whether an exception occurred.
        /// </summary>
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets or sets the ExceptionMessage.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// List of Person Identifierss corresponding to this Veteran
        /// </summary>
        public List<Messages.Mvi.UnattendedSearchRequest> CorrespondingIds { get; set; }

        /// <summary>
        /// Determines if GetIds needs to call MVI or not
        /// </summary>
        public bool IsSearchNeeded { get; set; }

        /// <summary>
        /// Gets or sets the Request
        /// </summary>
        public SelectedPersonRequest SelectedPersonRequest { get; set; }

        /// <summary>
        /// Edipi of Veteran
        /// </summary>
        public string Edipi { get; set; }

        /// <summary>
        /// Assigning Authority of Identifier of Veteran
        /// </summary>
        public string AssigningAuthority { get; set; }

        /// <summary>
        /// Assigning Facility of Identifier of Veteran
        /// </summary>
        public string AssigningFacility { get; set; }

        /// <summary>
        /// Veteran's First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Veteran's Last Name
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Veteran's Middle Name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Veteran's Full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Veteran's Date of Birth (in string format)
        /// </summary>
        public string DateOfBirth { get; set; }

        /// <summary>
        /// Veteran's Full address, concatenated by |
        /// </summary>
        public string FullAddress { get; set; }

        /// <summary>
        /// String representing source of Veteran Record
        /// </summary>
        public string RecordSource { get; set; }

        /// <summary>
        /// indicator to use or don't use raw MVI identifier value
        /// </summary>
        public bool UseRawMviValue { get; set; }

        /// <summary>
        /// Raw MVI identifier value concatenated by ^
        /// </summary>
        public string RawMviValue { get; set; }

        /// <summary>
        /// Get or sets the Identifier Class Code.
        /// </summary>
        public string IdentifierClassCode { get; set; }

        /// <summary>
        /// Gets or sets the Identifier Type.
        /// </summary>
        public string IdentifierType { get; set; }

        /// <summary>
        /// Gets or sets the Patient Search Identifier.
        /// </summary>
        public string PatientSearchIdentifier { get; set; }

        /// <summary>
        /// Veteran's Social
        /// </summary>
        public string Ss { get; set; }

        /// <summary>
        /// Type of fake response to use, empty string indicates call to MVI
        /// </summary>
        public string SelectedPersonFakeResponseType { get; set; }

        /// <summary>
        /// string representation of Veteran's Date of Death (if applicable)
        /// </summary>
        public string DateOfDeath { get; set; }

        /// <summary>
        /// Flag indicating if Veteran is a possible victim of Identity Theft
        /// </summary>
        public string IdTheftIndicator { get; set; }

        /// <summary>
        /// Veteran's Alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Veteran's Phone Number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Veteran's Gender (m or f or unknown)
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Server name from crme settings
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// CRM Contact Record
        /// </summary>
        public DataModel.Contact Contact { get; set; }

        /// <summary>
        /// Gets or sets the Corresponding Ids Response (Enterprise Component)
        /// </summary>
        public CorrespondingIdsResponse CorrespondingIdsResponse { get; set; }

        /// <summary>
        /// Gets or sets the GetPersonIdentifiers Response (message from plugin)
        /// </summary>
        public GetPersonIdentifiersResponseMessage GetPersonIdentifiersResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets EC Processing Time
        /// </summary>
        public int EcProcessingTimeMs { get; set; }
    }
}
