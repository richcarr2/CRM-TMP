using System.Collections.Generic;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Mvi.StateObject;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.Mappers
{
    /// <summary>
    /// Class to map data from Person Search to Attended Search request.
    /// </summary>
    public class PersonSearchMapper
    {
        private readonly PersonSearchStateObject _state;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="state">State.</param>
        public PersonSearchMapper(PersonSearchStateObject state)
        {
            _state = state;
        }

        /// <summary>
        /// Map data from Person Search to Attended Search request.
        /// </summary>
        /// <returns>AttendedSearchRequest</returns>
        public AttendedSearchRequest Map()
        {
            return new AttendedSearchRequest
            {
                BirthDate = _state.BirthDate,
                FamilyName = _state.FamilyName,
                FirstName = _state.FirstName,
                IsAttended = _state.IsAttended,
                MiddleName = _state.MiddleName,
                Edipi = _state.Edipi,
                OrganizationName = _state.OrganizationName,
                PhoneNumber = _state.PhoneNumber,
                SocialSecurityNumber = _state.Ss,
                UserFirstName = _state.UserFirstName,
                UserLastName = _state.UserLastName,
                UserId = _state.UserId,
                SearchUse = _state.SearchUse
            };
        }

        /// <summary>
        /// Map data from Person Search to Attended Search request.
        /// </summary>
        /// <returns>AttendedSearchRequest</returns>
        public UnattendedSearchRequest MapUnattended()
        {
            var assigningAuth = string.Empty;
            var assigningFacility = string.Empty;
            var identifierType = string.Empty;
            var rawValueFromMvi = string.Empty;

            var parts = _state.PatientIdentifier.Split('^');

            if (parts.Length.Equals(4))
            {
                rawValueFromMvi = parts[0];
                identifierType = parts[1];
                assigningFacility = parts[2];
                assigningAuth = parts[3];
            }

            return new UnattendedSearchRequest
            {
                AssigningAuthority = assigningAuth,
                AssigningFacility = assigningFacility,
                IdentifierType = identifierType,
                OrganizationName = _state.OrganizationName,
                PatientIdentifier = _state.PatientIdentifier,
                RawValueFromMvi = rawValueFromMvi,
                //UseRawMviValue = false,
                UserFirstName = _state.UserFirstName,
                UserLastName = _state.UserLastName,
                UserId = _state.UserId
            };
        }

        public Messages.Mvi.RetrieveOrSearchPersonResponse MapEcToLob(RetrieveOrSearchPersonResponse EcResponse)
        {
            var ackDetails = new List<Messages.Mvi.AcknowledgementDetail>();
            if (EcResponse.Acknowledgement == null) throw new MissingMviAckException("No Acknowledgement was returned");
            foreach (var ackDetail in EcResponse.Acknowledgement.AcknowledgementDetails)
            {
                if (ackDetail != null)
                {
                    ackDetails.Add(new Messages.Mvi.AcknowledgementDetail
                    {
                        Code = new Messages.Mvi.AcknowledgementDetailCode
                        {
                            Code = ackDetail.Code?.Code,
                            CodeSystemName = ackDetail.Code?.CodeSystemName,
                            DisplayName = ackDetail.Code?.DisplayName
                        },
                        Text = ackDetail.Text
                    });
                }
            }
            return new Messages.Mvi.RetrieveOrSearchPersonResponse
            {
                Acknowledgement = new Messages.Mvi.Acknowledgement
                {
                    TargetMessage = EcResponse.Acknowledgement.TargetMessage,
                    TypeCode = EcResponse.Acknowledgement.TypeCode,
                    AcknowledgementDetails = ackDetails.ToArray()
                },
                QueryAcknowledgement = new Messages.Mvi.QueryAcknowledgement
                {
                    QueryResponseCode = EcResponse.QueryAcknowledgement.QueryResponseCode,
                    ResultCurrentQuantity = EcResponse.QueryAcknowledgement.ResultCurrentQuantity
                },
                ExceptionOccured = EcResponse.ExceptionOccured,
                OrganizationName = EcResponse.OrganizationName,
                Person = MapPersons(EcResponse.Person),
                RawMviExceptionMessage = EcResponse.RawMviExceptionMessage,
                MessageId = EcResponse.MessageId,
                Message = EcResponse.Message
            };
        }

        public Messages.Mvi.PatientPerson[] MapPersons(PatientPerson[] ecPerson)
        {
            var persons = new List<Messages.Mvi.PatientPerson>();
            if (ecPerson == null)
                return persons.ToArray();

            foreach (var person in ecPerson)
            {
                var ids = new List<Messages.Mvi.UnattendedSearchRequest>();
                foreach (var id in person.CorrespondingIdList)
                {
                    ids.Add(new Messages.Mvi.UnattendedSearchRequest
                    {
                        AssigningAuthority = id.AssigningAuthority,
                        AssigningFacility = id.AssigningFacility,
                        AuthorityOid = id.AuthorityOid,
                        IdentifierType = id.IdentifierType,
                        OrganizationName = id.OrganizationName,
                        PatientIdentifier = id.PatientIdentifier,
                        RawValueFromMvi = id.RawValueFromMvi,
                        UseRawMviValue = id.UseRawMviValue,
                        UserFirstName = id.UserFirstName,
                        UserId = id.UserId,
                        UserLastName = id.UserLastName
                    }
                        );
                }
                var names = new List<Messages.Mvi.Name>();
                foreach (var name in person.NameList)
                {
                    names.Add(new Messages.Mvi.Name
                    {
                        FamilyName = name.FamilyName,
                        GivenName = name.GivenName,
                        MiddleName = name.MiddleName,
                        NamePrefix = name.NamePrefix,
                        NameSuffix = name.NameSuffix,
                        NameType = name.NameType,
                        Use = (Messages.Mvi.NameUse)(int)name.Use
                    });
                }
                persons.Add(new Messages.Mvi.PatientPerson
                {
                    BirthDate = person.BirthDate,
                    //BranchOfService = No such field exists on EC
                    DeceasedDate = person.DeceasedDate,
                    EdiPi = person.EdiPi,
                    GenderCode = person.GenderCode,
                    Identifier = person.Identifier,
                    IdentifierType = person.IdentifierType,
                    IdentifyTheft = person.IdentifierType,
                    IsDeceased = person.IsDeceased,
                    ParticipantId = person.ParticipantId,
                    PhoneNumber = person.PhoneNumber,
                    RecordSource = person.RecordSource,
                    Ss = person.SocialSecurityNumber,
                    StatusCode = person.StatusCode,
                    Url = person.Url,
                    Address = new Messages.Mvi.PatientAddress
                    {
                        City = person.Address.City,
                        Country = person.Address.Country,
                        PostalCode = person.Address.PostalCode,
                        State = person.Address.State,
                        StreetAddressLine = person.Address.StreetAddressLine,
                        Use = (Messages.Mvi.AddressUse)(int)person.Address.Use
                    },
                    CorrespondingIdList = ids.ToArray(),
                    NameList = names.ToArray()
                });


            }
            return persons.ToArray();
        }
    }
}