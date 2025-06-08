using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using VA.TMP.Integration.Mvi.StateObject;
using VEIS.Mvi.Messages;
using VA.TMP.Integration.Mvi.Mappers;

namespace VA.TMP.Integration.Mvi.Helpers
{
    /// <summary>
    /// Class for Pearson Search Utilities.
    /// </summary>
    public static class PersonSearchUtilities
    {
        /// <summary>
        /// Create fake EDIPI result.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns>RetrieveOrSearchPersonResponse.</returns>
        public static RetrieveOrSearchPersonResponse CreateFakeEdipiResult(PersonSearchStateObject state)
        {
            return new RetrieveOrSearchPersonResponse
            {
                MessageId = "88047127-5f3d-4090-8e93-1104a0384fdf",
                Person = new[]
                {
                    new PatientPerson()
                    {
                        SocialSecurityNumber = "898765678",
                        PhoneNumber = "(603)555-1111",
                        GenderCode = "M",
                        BirthDate = "19450820",
                        EdiPi = state.Edipi ?? "1606683207",
                        Address = new PatientAddress()
                        {
                            Use = AddressUse.Unspecified,
                            Country = "USA"
                        },
                        NameList = new[]
                        {
                            new Name
                            {
                                GivenName = "Jimmy",
                                FamilyName = "Chesney",
                                MiddleName = "David",
                                NameSuffix = "III",
                                Use = NameUse.Unspecified,
                                NameType = "Legal"
                            },
                            new Name
                            {
                                GivenName = "NAME",
                                FamilyName = "ALIAS",
                                MiddleName = "ONE",
                                Use = NameUse.Unspecified,
                                NameType = "Alias"
                            },
                            new Name
                            {
                                GivenName = "NAME",
                                FamilyName = "ALIAS",
                                MiddleName = "TWO",
                                Use = NameUse.Unspecified,
                                NameType = "Alias"
                            },
                            new Name
                            {
                                FamilyName = "NONAME",
                                Use = NameUse.Unspecified,
                                NameType = "Maiden"
                            }
                        },
                        CorrespondingIdList = new[]
                        {
                            new UnattendedSearchRequest
                            {
                                MessageId = "0f1a0ebc-ccaa-4d5b-b9a2-4ffd04fb439f",
                                UseRawMviValue = false,
                                PatientIdentifier = "1008520256V373999",
                                IdentifierType = "NI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200M",
                                RawValueFromMvi = "1008520256V373999^NI^200M^USVHA^P",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "57d1aab1-122c-48e3-9562-05ad063b0c75",
                                UseRawMviValue = false,
                                PatientIdentifier = "898765678",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVBA",
                                AssigningFacility = "200BRLS",
                                RawValueFromMvi = "898765678^PI^200BRLS^USVBA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "0f2cd19a-462b-47c2-a2b6-bc2f7ed47bf0",
                                UseRawMviValue = false,
                                PatientIdentifier = "9100787222",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200M",
                                RawValueFromMvi = "9100787222^PI^200CORP^USVBA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "9592c1a5-2757-4cb3-b4ff-3e839173d905",
                                UseRawMviValue = false,
                                PatientIdentifier = "1606683207",
                                IdentifierType = "NI",
                                AssigningAuthority = "USDOD",
                                AssigningFacility = "200DOD",
                                RawValueFromMvi = "1606683207^NI^200DOD^USDOD^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "d9b95863-3fbb-4d26-9d4b-1f84432ce82f",
                                UseRawMviValue = false,
                                PatientIdentifier = "100000025",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "500",
                                RawValueFromMvi = "100000025^PI^500^USVHA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "eeea6d83-ee64-4baa-bacd-2e7f20da3d6e",
                                UseRawMviValue = false,
                                PatientIdentifier = "0000001008520256V373999000000",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200ESR",
                                RawValueFromMvi = "0000001008520256V373999000000^PI^200ESR^USVHA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "b1f93f22-8683-48e4-8914-835549d59d50",
                                UseRawMviValue = false,
                                PatientIdentifier = "600088657",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "500",
                                RawValueFromMvi = "600088657^PI^500^USVHA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "88ac5525-2949-45e8-8517-bfb517be4bbe",
                                UseRawMviValue = false,
                                PatientIdentifier = "600088657",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200CORP",
                                RawValueFromMvi = "600088657^PI^200CORP^USVHA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                UseRawMviValue = false,
                                PatientIdentifier = "7172146",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "608",
                                RawValueFromMvi = "7172146^PI^608^USVHA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "ff3a0c8d-e2db-4de3-81ac-486a8f4195df",
                                UseRawMviValue = false,
                                PatientIdentifier = "100002049",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "553",
                                RawValueFromMvi = "100002049^PI^553^USVHA^A",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            },
                            new UnattendedSearchRequest
                            {
                                MessageId = "28867a22-c9d6-49c7-860e-5488a8dd8cb5",
                                UseRawMviValue = false,
                                PatientIdentifier = "0000001008591266V400774000000",
                                IdentifierType = "PI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200ESR",
                                RawValueFromMvi = "0000001008591266V400774000000^PI^200ESR^USVHA^PCE",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            }
                        },
                        RecordSource = "MVI"
                    }
                },
                ExceptionOccured = false,
                Message = "Your search in MVI found 1 matching record(s).",
                Acknowledgement = new Acknowledgement()
                {
                    TypeCode = "AA",
                    TargetMessage = "200CCVT-VRM.Integration.Servicebus.MVI.Services.TS-e600ef0e-0661-462f-b0ea-0a0f86874e26",
                },
                QueryAcknowledgement = new QueryAcknowledgement()
                {
                    QueryResponseCode = "OK",
                    ResultCurrentQuantity = "1"
                }
            };
        }

        /// <summary>
        /// Create Fake Success with one result.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns>RetrieveOrSearchPersonResponse.</returns>
        public static RetrieveOrSearchPersonResponse CreateFakeSuccess1Result(PersonSearchStateObject state, log4net.ILog _logger)
        {
            #region Search In TMP

            string personIcnIdentifier = null;

            //Search for a patient/contact based on first name, last name social, DOB
            //If found, read the ICN and use the existing for fakes creation
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                var contact = (from c in context.ContactSet
                               join i in context.mcs_personidentifiersSet
                               on c.Id equals i.mcs_patient.Id
                               where
                               i.mcs_identifier == state.Ss &&
                               i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS &&
                               i.mcs_assigningauthority == "USSSA" && i.statecode == (int)mcs_personidentifiersState.Active &&
                               c.FirstName == state.FirstName && c.LastName == state.FamilyName && c.StateCode == (int)ContactState.Active
                               select c).FirstOrDefault();

                if (contact != null)
                {
                    personIcnIdentifier = (from i in context.mcs_personidentifiersSet
                                           where
                                           i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI &&
                                           i.mcs_assigningauthority == "USVHA" &&
                                           //i.mcs_assigningauthority == "USVHA" && i.statecode == (int)mcs_personidentifiersState.Active &&
                                           i.mcs_patient.Id == contact.Id
                                           select i.mcs_identifier).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(personIcnIdentifier))
                        _logger.Info($"Could not find the ICN for the patient with {contact.Id} && FirstName == {state.FirstName} && LastName == {state.FamilyName} && SS == {state.Ss}");
                }
                else
                {
                    _logger.Info($"Could not find a patient available with the FirstName == {state.FirstName} && LastName == {state.FamilyName} && SS == {state.Ss}");
                }
            }
            #endregion
            return new RetrieveOrSearchPersonResponse
            {
                MessageId = "88047127-5f3d-4090-8e93-1104a0384fdf",
                Person = new[]
                {
                    new PatientPerson
                    {
                        SocialSecurityNumber = state.Ss,
                        PhoneNumber = "(603)555-1111",
                        GenderCode = "M",
                        BirthDate = state.BirthDate,
                        Address = new PatientAddress
                        {
                            Use = AddressUse.Unspecified,
                            Country = "USA"
                        },
                        NameList = new[]
                        {
                            new Name
                            {
                                GivenName = state.FirstName,
                                FamilyName = state.FamilyName,
                                MiddleName = "David",
                                NameSuffix = "III",
                                Use = NameUse.Unspecified,
                                NameType = "Legal"
                            },
                            new Name
                            {
                                GivenName = "NAME",
                                FamilyName = "ALIAS",
                                MiddleName = "ONE",
                                Use = NameUse.Unspecified,
                                NameType = "Alias"
                            },
                            new Name
                            {
                                GivenName = "NAME",
                                FamilyName = "ALIAS",
                                MiddleName = "TWO",
                                Use = NameUse.Unspecified,
                                NameType = "Alias"
                            },
                            new Name
                            {
                                FamilyName = "NONAME",
                                Use = NameUse.Unspecified,
                                NameType = "Maiden"
                            },
                        },
                        CorrespondingIdList = new[]
                        {
                            new UnattendedSearchRequest
                            {
                                MessageId = "bc748648-e7a9-49c4-97f6-5ac8cc433792",
                                UseRawMviValue = false,
                                PatientIdentifier = personIcnIdentifier ?? "1008520256V373999",
                                IdentifierType = "NI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200M",
                                RawValueFromMvi = $"{personIcnIdentifier?? "1008520256V373999"}^NI^200M^USVHA^P",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            }
                        },
                        RecordSource = "MVI"
                    }
                },
                ExceptionOccured = false,
                Message = "Your search in MVI found 1 matching record(s).",
                Acknowledgement = new Acknowledgement
                {
                    TypeCode = "AA",
                    TargetMessage = "200CCVT-VRM.Integration.Servicebus.MVI.Services.TS-ab0d95ef-4166-4286-828c-c595071a256f",
                    AcknowledgementDetails = new[]
                    {
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                Code = "132",
                                CodeSystemName = "MVI",
                                DisplayName = "IMT"
                            },
                            Text = "Identity Match Threshold"
                        },
                        new AcknowledgementDetail(){
                            Code = new AcknowledgementDetailCode
                            {
                                Code = "120",
                                CodeSystemName = "MVI",
                                DisplayName = "PDT"
                            },
                            Text = "Potential Duplicate Threshold"
                        }
                    }
                },
                QueryAcknowledgement = new QueryAcknowledgement
                {
                    QueryResponseCode = "OK",
                    ResultCurrentQuantity = "1"
                }
            };
        }

        /// <summary>
        /// Create Fake success with many results.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns>RetrieveOrSearchPersonResponse</returns>
        public static RetrieveOrSearchPersonResponse CreateFakeSuccessManyResults(PersonSearchStateObject state)
        {
            return new RetrieveOrSearchPersonResponse
            {
                MessageId = "88047127-5f3d-4090-8e93-1104a0384fdf",
                Person = new[]
                {
                    new PatientPerson
                    {
                        SocialSecurityNumber = state.Ss ?? "239948835",
                        PhoneNumber = "(603)555-1111",
                        GenderCode = "M",
                        BirthDate = state.BirthDate ?? "19491109",
                        Address = new PatientAddress
                        {
                            Use = AddressUse.Unspecified,
                            Country = "USA"
                        },
                        NameList = new[]
                        {
                            new Name
                            {
                                GivenName = state.FirstName ?? "TYLER",
                                FamilyName = state.FamilyName ?? "NOHEC",
                                MiddleName = state.MiddleName ?? "David",
                                NameSuffix = "",
                                Use = NameUse.Unspecified,
                                NameType = "Legal"
                            },
                            new Name
                            {
                                FamilyName = "White",
                                Use = NameUse.Unspecified,
                                NameType = "Maiden"
                            },
                        },
                        CorrespondingIdList = new[]
                        {
                            new UnattendedSearchRequest
                            {
                                MessageId = "adfef043-1caf-4bcc-beb8-6fe8655f767d",
                                UseRawMviValue = false,
                                PatientIdentifier = "1008593449V179009",
                                IdentifierType = "NI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200M",
                                RawValueFromMvi = "1008593449V179009^NI^200M^USVHA^P",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            }
                        },
                        RecordSource = "MVI"
                    },
                    new PatientPerson
                    {
                        SocialSecurityNumber = state.Ss ?? "239948835",
                        GenderCode = "M",
                        BirthDate = state.BirthDate ?? "19491109",
                        Address = new PatientAddress
                        {
                            Use = AddressUse.Unspecified,
                            Country = "USA"
                        },
                        NameList = new[]
                        {
                            new Name
                            {
                                GivenName = state.FirstName ?? "TYLER",
                                FamilyName = state.FamilyName ?? "NOHEC",
                                MiddleName = state.MiddleName ?? "David",
                                NameSuffix = "",
                                Use = NameUse.Unspecified,
                                NameType = "Legal"
                            },
                            new Name
                            {
                                FamilyName = "White",
                                Use = NameUse.Unspecified,
                                NameType = "Maiden"
                            },
                        },
                        CorrespondingIdList = new[]
                        {
                            new UnattendedSearchRequest
                            {
                                MessageId = "adfef043-1caf-4bcc-beb8-6fe8655f767d",
                                UseRawMviValue = false,
                                PatientIdentifier = "1008696589V053728",
                                IdentifierType = "NI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200M",
                                RawValueFromMvi = "1008696589V053728^NI^200M^USVHA^P",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            }
                        },
                        RecordSource = "MVI"
                    },
                    new PatientPerson
                    {
                        SocialSecurityNumber = state.Ss ?? "239948835",
                        GenderCode = "M",
                        BirthDate = state.BirthDate ?? "19491109",
                        Address = new PatientAddress
                        {
                            Use = AddressUse.Unspecified,
                            Country = "USA"
                        },
                        NameList = new[]
                        {
                            new Name
                            {
                                GivenName = "TYLER",
                                FamilyName = "NOHEC",
                                MiddleName = "David",
                                NameSuffix = "",
                                Use = NameUse.Unspecified,
                                NameType = "Legal"
                            },
                            new Name
                            {
                                FamilyName = "White",
                                Use = NameUse.Unspecified,
                                NameType = "Maiden"
                            },
                        },
                        CorrespondingIdList = new[]
                        {
                            new UnattendedSearchRequest
                            {
                                MessageId = "27ff9a53-3ea1-4661-b5a6-ea7f6e415702",
                                UseRawMviValue = false,
                                PatientIdentifier = "1008593447V264311",
                                IdentifierType = "NI",
                                AssigningAuthority = "USVHA",
                                AssigningFacility = "200M",
                                RawValueFromMvi = "1008593447V264311^NI^200M^USVHA^P",
                                UserId = new Guid("00000000-0000-0000-0000-000000000000")
                            }
                        },
                        RecordSource = "MVI"
                    }
                },
                ExceptionOccured = false,
                Message = "Your search in MVI found 3 matching record(s).",
                Acknowledgement = new Acknowledgement
                {
                    TypeCode = "AA",
                    TargetMessage = "200CCVT-VRM.Integration.Servicebus.MVI.Services.TS-ab0d95ef-4166-4286-828c-c595071a256f",
                    AcknowledgementDetails = new[]
                    {
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                Code = "132",
                                CodeSystemName = "MVI",
                                DisplayName = "IMT"
                            },
                            Text = "Identity Match Threshold"
                        },
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                Code = "120",
                                CodeSystemName = "MVI",
                                DisplayName = "PDT"
                            },
                            Text = "Potential Duplicate Threshold"
                        }
                    }
                },
                QueryAcknowledgement = new QueryAcknowledgement
                {
                    QueryResponseCode = "OK",
                    ResultCurrentQuantity = "1"
                }
            };
        }

        /// <summary>
        /// Create Fake Success with no results.
        /// </summary>
        /// <returns>RetrieveOrSearchPersonResponse</returns>
        public static RetrieveOrSearchPersonResponse CreateFakeSuccess0Result()
        {
            return new RetrieveOrSearchPersonResponse
            {
                Acknowledgement = new Acknowledgement
                {
                    TypeCode = "AA",
                    TargetMessage = "200CCVT-VRM.Integration.Servicebus.MVI.Services.TS-814704af-1646-4130-b403-39f6a95d5fc4",
                    AcknowledgementDetails = new[]
                    {
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                Code = "132",
                                CodeSystemName = "MVI",
                                DisplayName = "IMT"
                            },
                            Text = "Identity Match Threshold"
                        },
                        new AcknowledgementDetail
                        {
                            Code = new AcknowledgementDetailCode
                            {
                                Code = "120",
                                CodeSystemName = "MVI",
                                DisplayName = "PDT"
                            },
                            Text = "Potential Duplicate Threshold"
                        }
                    }

                },
                QueryAcknowledgement = new QueryAcknowledgement
                {
                    QueryResponseCode = "NF",
                    ResultCurrentQuantity = "0"
                }
            };
        }

        /// <summary>
        /// Create Fake Selected Person Response.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns>CorrespondingIdsResponse.</returns>
        public static CorrespondingIdsResponse FakeSelectedPersonResponse(GetPersonIdentifiersStateObject state, log4net.ILog _logger)
        {
            var rawIdentifier = state.RawMviValue?.Split('^').FirstOrDefault();
            string personIcnIdentifier = null;

            #region Search In TMP
            //Search for a patient/contact based on first name, last name social, DOB
            //If found, read the ICN and use the existing for fakes creation
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                var contact = (from c in context.ContactSet
                               join i in context.mcs_personidentifiersSet
                               on c.Id equals i.mcs_patient.Id
                               where
                               i.mcs_identifier == state.Ss &&
                               i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS &&
                               i.mcs_assigningauthority == "USSSA" && i.statecode == (int)mcs_personidentifiersState.Active &&
                               c.StateCode == (int)ContactState.Active
                               select c).FirstOrDefault();

                if (contact != null)
                {
                    personIcnIdentifier = (from i in context.mcs_personidentifiersSet
                                           where
                                           i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI &&
                                           i.mcs_assigningauthority == "USVHA" && i.statecode == (int)mcs_personidentifiersState.Active &&
                                           i.mcs_patient.Id == contact.Id && (i.tmp_identifierstatus.Equals("A") || i.tmp_identifierstatus.Equals("C"))
                                           select i.mcs_identifier).FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(personIcnIdentifier))
                    {
                        _logger.Info($"Could not find the ICN for the patient with {contact.Id}  && SS == {state.Ss}");
                        personIcnIdentifier = rawIdentifier;
                    }
                }
                else
                {
                    _logger.Info($"Could not find a patient available with the SS == {state.Ss}");
                }
                #endregion

                return new CorrespondingIdsResponse
                {
                    CorrespondingIdList = new[]
                    {
                        //ICN
                        new UnattendedSearchRequest
                        {
                            MessageId = "e23f6a10-0c33-41d3-901e-071926d049ef",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "200M",
                            AuthorityOid = null,
                            IdentifierType = "NI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = $"{personIcnIdentifier ?? "1001179424V492193"}",
                            RawValueFromMvi = $"{personIcnIdentifier ?? "1001179424V492193"}^NI^200M^USVHA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "6c43bc8e-4589-4092-b22d-37f7153fb5d2",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "910",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = $"000000{personIcnIdentifier ?? "1001179424V492193"}000000",
                            RawValueFromMvi = $"000000{personIcnIdentifier ?? "1001179424V492193"}000000^PI^910^USVHA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "068f3760-4470-471d-ae98-e468c41473ae",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USDVA",
                            AssigningFacility = "200PROV",
                            AuthorityOid = null,
                            IdentifierType = "PN",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "0001234567",
                            RawValueFromMvi = "0000001234567^PN^200PROV^USDVA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "26133d2b-7473-474f-9c79-267fcb826423",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "914",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ1234567",
                            RawValueFromMvi = "ZZ1234567^PI^914^USVHA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "5758c79d-7932-4e99-90b4-81dbd09bf73b",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "200M",
                            AuthorityOid = null,
                            IdentifierType = "NI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = $"{personIcnIdentifier ?? "1001179424V492193"}",
                            RawValueFromMvi = $"{personIcnIdentifier ?? "1001179424V492193"}^NI^200M^USVHA^P",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "21afcaea-298d-43f3-9b72-661b409f434f",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USDOD",
                            AssigningFacility = "200DOD",
                            AuthorityOid = null,
                            IdentifierType = "NI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ123456789",
                            RawValueFromMvi = "ZZ123456789^NI^200DOD^USDOD^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "2eaa8ee8-4393-45f6-8f7a-2710c6f4fd5c",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "915",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "VICJOZZZ15",
                            RawValueFromMvi = "VICJOZZZ15^PI^915^USVHA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "be82bfdc-3239-4db3-b020-bc5c95469197",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USDVA",
                            AssigningFacility = "912",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ12345678",
                            RawValueFromMvi = "ZZ12345678^PI^912^USDVA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "fede0e99-40b8-438d-bc1b-5d8c8b4cd6a1",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVBA",
                            AssigningFacility = "913",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ123456789",
                            RawValueFromMvi = "ZZ123456789^PI^913^USVBA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "b0f66122-edff-44b6-9d07-41d7ce0a8b51",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVBA",
                            AssigningFacility = "918",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ12345678",
                            RawValueFromMvi = "ZZ12345678^PI^918^USVBA^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "4dcfc402-8f3b-4a67-846b-131886be9895",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "1.2.840.999999.1.13.296.2.7.3.688884.100",
                            AssigningFacility = "200NPS",
                            AuthorityOid = null,
                            IdentifierType = "NI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "PWM123456U",
                            RawValueFromMvi = "PWM123456U^NI^200NPS^1.2.840.999999.1.13.296.2.7.3.688884.100^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "5635d248-6b53-4949-9206-d357dfb56e6e",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USDVA",
                            AssigningFacility = "200DSLF",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "999t000000Zd9HwAAJ_vasfdc",
                            RawValueFromMvi = "999t000000Zd9HwAAJ_vasfdc^PI^200DSLF^USDVA^P",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "1478bfee-e942-42ca-aa7e-623194b7652e",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "200CH",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ1234567890",
                            RawValueFromMvi = "ZZ1234567890^PI^200CH^USVHA^P",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "6669773c-f782-40d1-8fd1-02eb484ccdf8",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "742V1",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ12345678",
                            RawValueFromMvi = "ZZ12345678^PI^742V1^USVHA^P",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "d76f11f1-f86c-4e54-95de-5dd5542a3f0b",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USVHA",
                            AssigningFacility = "200MH",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ12345678901",
                            RawValueFromMvi = "ZZ12345678901^PI^200MH^USVHA^P",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "ace29da3-ba7f-4724-86b1-29464dea06b7",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USDVA",
                            AssigningFacility = "200DSLF",
                            AuthorityOid = null,
                            IdentifierType = "PI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "a1c3d000999XL9PAAW_vavsfdc",
                            RawValueFromMvi = "a1c3d000999XL9PAAW_vavsfdc^PI^200DSLF^USDVA^P",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "7440fa9a-94f6-414d-b5f9-1730975a26fa",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "2.16.999.1.113883.3.2018",
                            AssigningFacility = "200NWG",
                            AuthorityOid = null,
                            IdentifierType = "NI",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "ZZ1234567890",
                            RawValueFromMvi = "ZZ1234567890^NI^200NWG^2.16.999.1.113883.3.2018^A",
                            UserFirstName = null,
                            UserLastName = null
                        },
                        new UnattendedSearchRequest
                        {
                            MessageId = "c4b10648-c825-428d-8020-be0e527c10ab",
                            CorrelationId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            OrganizationName = null,
                            UserId  = new Guid("00000000-0000-0000-0000-000000000000"),
                            AssigningAuthority = "USDVA",
                            AssigningFacility = "200VIDM",
                            AuthorityOid = null,
                            IdentifierType = "PN",
                            LogSoap = false,
                            LogTiming = false,
                            PatientIdentifier = "9c72e123456c48d7bfc62ba488e65ed9",
                            RawValueFromMvi = "9c72e123456c48d7bfc62ba488e65ed9^PN^200VIDM^USDVA^A",
                            UserFirstName = null,
                            UserLastName = null
                        }
                    },
                    ExceptionOccured = false,
                    OrganizationName = state.OrganizationName,
                    UserId = new Guid("01c13ce7-8fa8-e411-acdb-00155d5575e0"),
                    Acknowledgement = new Acknowledgement
                    {
                        TypeCode = "AA",
                        TargetMessage = "200CCVT-VRM.Integration.Servicebus.MVI.Services.TS-c8909264-e536-45c4-bb81-60eb013f1e65"
                    },
                    QueryAcknowledgement = new QueryAcknowledgement
                    {
                        QueryResponseCode = "OK"
                    }
                };
            }
        }

        /// <summary>
        /// Get Address Object.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <returns>PatientAddress.</returns>
        internal static PatientAddress GetAddressObject(string address)
        {
            if (string.IsNullOrEmpty(address)) return null;

            // Address is of format (AL|CY|ST|ZP|CN)
            var names = address.Split('|');
            return new PatientAddress
            {
                StreetAddressLine = (names.Length >= 1) ? names[0] : string.Empty,
                City = (names.Length >= 2) ? names[1] : string.Empty,
                State = (names.Length >= 3) ? names[2] : string.Empty,
                PostalCode = (names.Length >= 4) ? names[3] : string.Empty,
                Country = (names.Length >= 5) ? names[4] : string.Empty
            };
        }

        /// <summary>
        /// Get Name Object.
        /// </summary>
        /// <param name="fullName">Full Name.</param>
        /// <returns>Name.</returns>
        internal static Name GetNameObject(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;

            // Full name is of format (LN|FN|MN|SF|PF)
            var names = fullName.Split('|');
            return new Name
            {
                FamilyName = (names.Length >= 1) ? names[0] : string.Empty,
                GivenName = (names.Length >= 2) ? names[1] : string.Empty,
                MiddleName = (names.Length >= 3) ? names[2] : string.Empty,
                NameSuffix = (names.Length >= 4) ? names[3] : string.Empty,
                NamePrefix = (names.Length >= 5) ? names[4] : string.Empty
            };
        }

        /// <summary>
        /// Get Date.
        /// </summary>
        /// <param name="dateString">Date string.</param>
        /// <returns>DateTime.</returns>
        internal static DateTime? GetDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return null;

            if (dateString.Length < 8) return null;

            var sYear = dateString.Substring(0, 4);
            var sMonth = dateString.Substring(4, 2);
            var sDay = dateString.Substring(6, 2);
            var sDate = string.Format("{0}/{1}/{2}", sMonth, sDay, sYear);

            DateTime date;
            if (DateTime.TryParse(sDate, out date)) return date;

            return null;
        }
    }
}