using System;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class PersonSearchStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="personSearchRequestMessage">PersonSearchRequestMessage instance.</param>
        public PersonSearchStateObject(PersonSearchRequestMessage personSearchRequestMessage)
        {
            OrganizationName = personSearchRequestMessage.OrganizationName;
            UserId = personSearchRequestMessage.UserId;
            LogRequest = personSearchRequestMessage.LogRequest;
            IsAttended = personSearchRequestMessage.IsAttended;
            FirstName = personSearchRequestMessage.FirstName;
            FamilyName = personSearchRequestMessage.FamilyName;
            Ss = personSearchRequestMessage.Ss;
            UserFirstName = personSearchRequestMessage.UserFirstName;
            UserLastName = personSearchRequestMessage.UserLastName;
            BirthDate = personSearchRequestMessage.BirthDate;
            MiddleName = personSearchRequestMessage.MiddleName;
            Edipi = personSearchRequestMessage.Edipi;
            PatientIdentifier = personSearchRequestMessage.PatientIdentifier;
            PhoneNumber = personSearchRequestMessage.PhoneNumber;
            PersonSearchFakeResponseType = personSearchRequestMessage.PersonSearchFakeResponseType;
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
        /// Gets or sets whether to log the request.
        /// </summary>
        public bool LogRequest { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public string PersonSearchFakeResponseType { get; set; }

        /// <summary>
        /// Gets or Sets whether an exception occured.
        /// </summary>
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets or Sets the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Get or sets whether attended search.
        /// </summary>
        public bool IsAttended { get; set; }

        /// <summary>
        /// Get or sets first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Get or sets last name.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Get or sets patient identifier
        /// </summary>
        public string PatientIdentifier { get; set; }

        /// <summary>
        /// Get or sets social
        /// </summary>
        public string Ss { get; set; }

        /// <summary>
        /// Get or sets user first name.
        /// </summary>
        public string UserFirstName { get; set; }

        /// <summary>
        /// Get or sets user last name.
        /// </summary>
        public string UserLastName { get; set; }

        /// <summary>
        /// Get or sets birth date.
        /// </summary>
        public string BirthDate { get; set; }

        /// <summary>
        /// Get or sets middle name.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Get or sets EDIPI.
        /// </summary>
        public string Edipi { get; set; }

        /// <summary>
        /// Get or sets phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Get or sets Search Use.
        /// </summary>
        public string SearchUse { get; set; }

        /// <summary>
        /// Get or sets Attended Search Request.
        /// </summary>
        public AttendedSearchRequest AttendedSearchRequest { get; set; }

        /// <summary>
        /// Get or sets Serialized instance.
        /// </summary>
        public string SerializedInstance { get; set; }

        /// <summary>
        /// Get or sets Retrieve or Search Person response.
        /// </summary>
        public VEIS.Mvi.Messages.RetrieveOrSearchPersonResponse RetrieveOrSearchPersonResponse { get; set; }

        /// <summary>
        /// Get or sets Person Search Response Message.
        /// </summary>
        public PersonSearchResponseMessage PersonSearchResponseMessage { get; set; }

        /// <summary>
        /// Get or sets Attended Search Request.
        /// </summary>
        public VEIS.Mvi.Messages.UnattendedSearchRequest UnattendedSearchRequest { get; set; }

        /// <summary>
        /// Gets or sets EC Processing Time
        /// </summary>
        public int EcProcessingTimeMs { get; set; }
    }
}
