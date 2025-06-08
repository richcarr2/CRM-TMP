using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.OptionSets;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.Mappers
{
    /// <summary>
    /// Maps responses from a person search to a Contact entity record.
    /// </summary>
    public static class MapGetPersonIdentifiersRequestToContact
    {
        /// <summary>
        /// Map responses from a person search to a Contact entity record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Messages.Mvi.UnattendedSearchRequest Map(UnattendedSearchRequest id)
        {
            return new Messages.Mvi.UnattendedSearchRequest
            {
                AssigningAuthority = id.AssigningAuthority,
                AssigningFacility = id.AssigningFacility,
                AuthorityOid = id.AssigningAuthority,
                IdentifierType = id.IdentifierType,
                OrganizationName = id.OrganizationName,
                PatientIdentifier = id.PatientIdentifier,
                RawValueFromMvi = id.RawValueFromMvi,
                UseRawMviValue = id.UseRawMviValue,
                UserFirstName = id.UserFirstName,
                UserId = id.UserId,
                UserLastName = id.UserLastName
            };
        }

        /// <summary>
        /// Creates a new Contact entity record from data received from a person search.
        /// </summary>
        /// <param name="source">The SelectedPersonRequest object containing the person data.</param>
        /// <param name="state"></param>
        /// <returns>The new Contact entity record.</returns>
        public static Contact Create(SelectedPersonRequest source, GetPersonIdentifiersStateObject state)
        {
            var dob = GetDate(source.DateofBirth);
            var dod = GetDate(state.DateOfDeath);
            var name = GetNameObject(source.FullName);
            var address = GetAddressObject(source.FullAddress);
            var idTheft = GetIdTheftIndicator(state.IdTheftIndicator);

            var contact = new Contact
            {
                mcs_othernames = state.Alias,
                Address1_Line1 = address.StreetAddressLine,
                Address1_City = address.City,
                Address1_Country = address.Country,
                Address1_PostalCode = address.PostalCode,
                Address1_Name = Enum.GetName(typeof(AddressUse), address.Use),
                Address1_StateOrProvince = address.State,
                FirstName = name.GivenName,
                LastName = name.FamilyName,
                MiddleName = name.MiddleName,
                Salutation = name.NamePrefix,
                Suffix = name.NameSuffix,
                Telephone2 = state.PhoneNumber,
                BirthDate = dob,
                mcs_identitytheft = idTheft
            };

            if (dod.HasValue)
            {
                contact.mcs_deceased = true;
                contact.mcs_deceaseddate = dod;
            }

            if (!string.IsNullOrEmpty(state.Gender))
            {
                int genderCode;
                switch (state.Gender.ToLower())
                {
                    case "m":
                        genderCode = 1;
                        break;
                    case "f":
                        genderCode = 2;
                        break;
                    default:
                        genderCode = -1;
                        break;
                }

                if (genderCode > 0) contact.GenderCode = new OptionSetValue(genderCode);
            }

            string lastFour;
            contact.mcs_contact_mcs_personidentifiers_patient = GetPersonIdentifiers(state.CorrespondingIds, state.SelectedPersonFakeResponseType, source.Edipi, source.SocialSecurityNumber, out lastFour);

            if (!string.IsNullOrEmpty(lastFour)) contact.mcs_Last4 = lastFour;

            contact.EntityState = EntityState.Created;
            return contact;
        }

        /// <summary>
        /// Updates an existing Contact entity record with information received from a MVI person search.  
        /// </summary>
        /// <param name="source">The SelectedPersonRequest object containing the updated person data.</param>
        /// <param name="state"></param>
        /// <param name="beforeImage">The Contact entity record to update.</param>
        /// <param name="proxy">The CRM organization service proxy. Used to update any entity records related to the Contact.</param>
        /// <returns>The updated Contact entity record.</returns>
        public static Contact Update(SelectedPersonRequest source, GetPersonIdentifiersStateObject state, Contact beforeImage, OrganizationWebProxyClient proxy)
        {
            string lastFour;
            var identifierStatus = state?.RawMviValue?.Split('^').LastOrDefault();
            var dob = GetDate(source.DateofBirth);
            var dod = GetDate(state.DateOfDeath);
            var name = GetNameObject(source.FullName);
            var address = GetAddressObject(source.FullAddress);
            var personIds = GetPersonIdentifiers(state.CorrespondingIds, state.SelectedPersonFakeResponseType, source.Edipi, source.SocialSecurityNumber, out lastFour);
            var isIdTheft = GetIdTheftIndicator(state.IdTheftIndicator);
            var updateContact = new Contact
            {
                Id = beforeImage.Id,
                mcs_othernames = GetUpdatedValue(beforeImage.mcs_othernames, state.Alias),
                Address1_Line1 = GetUpdatedValue(beforeImage.Address1_Line1, address.StreetAddressLine),
                Address1_City = GetUpdatedValue(beforeImage.Address1_City, address.City),
                Address1_Country = GetUpdatedValue(beforeImage.Address1_Country, address.Country),
                Address1_PostalCode = GetUpdatedValue(beforeImage.Address1_PostalCode, address.PostalCode),
                Address1_Name = GetUpdatedValue(beforeImage.Address1_Name, Enum.GetName(typeof(AddressUse), address.Use)),
                Address1_StateOrProvince = GetUpdatedValue(beforeImage.Address1_StateOrProvince, address.State),
                FirstName = GetUpdatedValue(beforeImage.FirstName, name.GivenName),
                LastName = GetUpdatedValue(beforeImage.LastName, name.FamilyName),
                MiddleName = GetUpdatedValue(beforeImage.MiddleName, name.MiddleName),
                Salutation = GetUpdatedValue(beforeImage.Salutation, name.NamePrefix),
                Suffix = GetUpdatedValue(beforeImage.Suffix, name.NameSuffix),
                BirthDate = GetUpdatedDate(beforeImage.BirthDate, dob),
                mcs_deceaseddate = GetUpdatedDate(beforeImage.mcs_deceaseddate, dod),
                mcs_deceased = beforeImage.mcs_deceaseddate.HasValue,
                Telephone2 = GetUpdatedValue(beforeImage.Telephone2, state.PhoneNumber),
                mcs_Last4 = GetUpdatedValue(beforeImage.mcs_Last4, lastFour),
                GenderCode = GetUpdatedGenderCode(beforeImage.GenderCode, state.Gender),
                mcs_identitytheft = GetUpdatedIdTheftValue(beforeImage.mcs_identitytheft, isIdTheft)
            };

            RefreshIdList(proxy, updateContact.Id, personIds);

            return updateContact;
        }

        /// <summary>
        /// Get Name.
        /// </summary>
        /// <param name="fullName">Full Name.</param>
        /// <returns>Name.</returns>
        private static Name GetNameObject(string fullName)
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
        /// Get Address.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <returns>Patient Address.</returns>
        private static PatientAddress GetAddressObject(string address)
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
        /// Get Person Identifiers.
        /// </summary>
        /// <param name="ids">Ids.</param>
        /// <param name="edipi">Edipi.</param>
        /// <param name="ss">Social Security Number.</param>
        /// <param name="lastFour">Last Four.</param>
        /// <returns>List of Person Identifiers.</returns>
        public static List<mcs_personidentifiers> GetPersonIdentifiers(List<Messages.Mvi.UnattendedSearchRequest> ids, string fakeResponseType, string edipi, string ss, out string lastFour)
        {
            var identifierStatus = string.Empty;
            var personIds = new List<mcs_personidentifiers>();
            lastFour = string.Empty;

            foreach (var id in ids)
            {
                //Skip Person Identifiers that do not contain at least 1 Alpha Character
                if (!id.AssigningAuthority.ToArray().Any(c => char.IsLetter(c))) continue;

                identifierStatus = string.IsNullOrEmpty(id?.RawValueFromMvi) ? "" : id.RawValueFromMvi.Split('^').LastOrDefault().Trim();

                var personId = new mcs_personidentifiers
                {
                    mcs_name = fakeResponseType != null && fakeResponseType.Equals("0") ? "FAKE Identifier - DO NOT REMOVE" : "",
                    mcs_identifier = id.PatientIdentifier,
                    mcs_assigningfacility = id.AssigningFacility == "200M" ? "" : id.AssigningFacility.Replace("200", ""),
                    mcs_assigningauthority = id.AssigningAuthority.Replace("200", ""),
                    mcs_authorityorganizationid = id.AuthorityOid
                };

                //Check to make sure the Status retrieved from the Raw Value is only 1 character in length
                personId.Attributes["tmp_identifierstatus"] = identifierStatus.Length.Equals(1) ? identifierStatus : null;

                int idType;
                switch (id.IdentifierType)
                {
                    case "NI":
                        idType = 125150000;
                        break;
                    case "PI":
                        idType = id.AssigningAuthority.Equals("USSSA") ? 125150004 : 125150001;
                        break;
                    case "EI":
                        idType = 125150002;
                        break;
                    case "PN":
                        idType = 125150003;
                        break;
                    case "SS":
                        idType = 125150004;
                        break;
                    default:
                        idType = 0;
                        break;
                }
                personId.mcs_identifiertype = new OptionSetValue(idType);

                personIds.Add(personId);

                if (personIds.Any(i => i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS))
                {
                    ss = personIds.First(i => i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS).mcs_identifier;

                    if (!string.IsNullOrEmpty(ss) && ss.Length >= 4) lastFour = ss.Substring(ss.Length - 4);
                }
                else
                {
                    if (id.AssigningAuthority.Equals("USSSA") && !string.IsNullOrEmpty(ss))
                    {
                        personIds.Add(new mcs_personidentifiers
                        {
                            mcs_name = fakeResponseType != null && fakeResponseType.Equals("0") ? "FAKE Identifier - DO NOT REMOVE" : "",
                            mcs_identifier = ss,
                            mcs_assigningauthority = "USSSA",
                            mcs_identifiertype = new OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS),
                            //Check to make sure the Status retrieved from the Raw Value is only 1 character in length
                            tmp_identifierstatus = identifierStatus.Length.Equals(1) ? identifierStatus : null
                        });

                        if (ss.Length >= 4) lastFour = ss.Substring(ss.Length - 4);
                    }
                }

                if (!string.IsNullOrEmpty(edipi) && personIds.All(i => i.mcs_identifier != edipi))
                    personIds.Add(new mcs_personidentifiers
                    {
                        mcs_identifier = edipi,
                        mcs_assigningauthority = "USDOD",
                        mcs_identifiertype = new OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI)
                    });
            }

            //Account for no SSN Person Identifiers were returned by MVI
            if (!personIds.Any(i => i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS) &&
                !string.IsNullOrEmpty(ss))
            {
                //Create the PI using the SSN submitted with the Request
                personIds.Add(new mcs_personidentifiers
                {
                    mcs_name = fakeResponseType != null && fakeResponseType.Equals("0") ? "FAKE Identifier - DO NOT REMOVE" : "",
                    mcs_identifier = ss,
                    mcs_assigningauthority = "USSSA",
                    mcs_identifiertype = new OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS),
                    //Check to make sure the Status retrieved from the Raw Value is only 1 character in length
                    tmp_identifierstatus = identifierStatus.Length.Equals(1) ? identifierStatus : null
                });

                if (ss.Length >= 4) lastFour = ss.Substring(ss.Length - 4);
            }

            return personIds;
        }

        /// <summary>
        /// Get Date. Adjust it by 17 hours upward so that regardless of user time zone, the date is correct - assumes CRM Server is in East Coast (subtract 5 hours for UTC offset + 12 hours to put the time to noon UTC)
        /// </summary>
        /// <param name="dateString">Date string.</param>
        /// <returns>Date.</returns>
        private static DateTime? GetDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return null;
            if (dateString.Length < 8) return null;

            var sYear = dateString.Substring(0, 4);
            var sMonth = dateString.Substring(4, 2);
            var sDay = dateString.Substring(6, 2);
            var sDate = string.Format("{0}/{1}/{2}", sMonth, sDay, sYear);

            DateTime date;
            if (DateTime.TryParse(sDate, out date)) return date.AddHours(7);

            return null;
        }

        /// <summary>
        /// Get Theft Indicator.
        /// </summary>
        /// <param name="idTheftIndicator">Theft Indicator Id</param>
        /// <returns>Theft Indicator.</returns>
        private static bool? GetIdTheftIndicator(string idTheftIndicator)
        {
            return !string.IsNullOrEmpty(idTheftIndicator) && idTheftIndicator.ToLower() == "yes";
        }

        /// <summary>
        /// Get Update Value for string.
        /// </summary>
        /// <param name="oldValue">Old Value.</param>
        /// <param name="newValue">New Value.</param>
        /// <returns>Old or New value.</returns>
        private static string GetUpdatedValue(string oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue) && string.IsNullOrEmpty(newValue)) return oldValue;

            if (string.IsNullOrEmpty(oldValue) && !string.IsNullOrEmpty(newValue)) return newValue;

            return oldValue == newValue ? oldValue : newValue;
        }

        /// <summary>
        /// Get Update Date.
        /// </summary>
        /// <param name="oldValue">Old Value.</param>
        /// <param name="newValue">New Value.</param>
        /// <returns>Old or New value.</returns>
        private static DateTime? GetUpdatedDate(DateTime? oldValue, DateTime? newValue)
        {
            if (oldValue.HasValue && !newValue.HasValue) return oldValue;

            if (!oldValue.HasValue && newValue.HasValue) return newValue;

            return oldValue == newValue ? oldValue : newValue;
        }

        /// <summary>
        /// Get Gender Code.
        /// </summary>
        /// <param name="oldValue">Old Value.</param>
        /// <param name="newValue">New Value.</param>
        /// <returns>Old or New value.</returns>
        private static OptionSetValue GetUpdatedGenderCode(OptionSetValue oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue)) return oldValue;

            var newCode = -1;
            switch (newValue.ToLower())
            {
                case "m":
                    newCode = 1;
                    break;
                case "f":
                    newCode = 2;
                    break;
            }

            if (newCode <= 0) return oldValue;

            if (oldValue == null) return new OptionSetValue(newCode);

            return oldValue.Value == newCode ? oldValue : new OptionSetValue(newCode);
        }

        /// <summary>
        /// Get Updated Theft value.
        /// </summary>
        /// <param name="oldValue">Old Value.</param>
        /// <param name="newValue">New Value.</param>
        /// <returns>Old or New Value.</returns>
        private static bool? GetUpdatedIdTheftValue(bool? oldValue, bool? newValue)
        {
            if (oldValue.HasValue && !newValue.HasValue) return oldValue;

            if (!oldValue.HasValue && newValue.HasValue) return newValue;

            return oldValue == newValue ? oldValue : newValue;
        }

        ///// <summary>
        ///// Get Updated Id List.
        ///// </summary>
        ///// <param name="proxy">Organization Service Proxy.</param>
        ///// <param name="contactId">Contact Id.</param>
        ///// <param name="newList">New List.</param>
        //public static void GetUpdatedIdList(OrganizationWebProxyClient proxy, Guid contactId, List<mcs_personidentifiers> newList)
        //{
        //    var stopWatch = Stopwatch.StartNew();

        //    //Logger.Instance.Debug("Calling TMP.GetUpdatedIdList.");

        //    using (var crm = new Xrm(proxy))
        //    {
        //        // Get the current list of identifiers
        //        var currentList = (from i in crm.CreateQuery<mcs_personidentifiers>()
        //                           where i.mcs_patient.Id == contactId
        //                           select i).ToList();

        //        //Logger.Instance.Debug(string.Format("***************Progress.{0}. Took {1} ms************", "Get CRM Identifiers for this patient", stopWatch.ElapsedMilliseconds));

        //        // Get all the identifiers in the new list that don't match a value in the old list.
        //        var createIdList = newList.Where(n => !currentList.Any(c => c.mcs_identifier == n.mcs_identifier &&
        //                                                                    c.mcs_identifiertype.Value == n.mcs_identifiertype.Value &&
        //                                                                    c.mcs_assigningauthority == n.mcs_assigningauthority &&
        //                                                                    c.mcs_assigningfacility == n.mcs_assigningfacility)).ToList();
        //        //Logger.Instance.Debug(string.Format("***************Progress.{0}. Took {1} ms************", "Got list of NEW IDs to create", stopWatch.ElapsedMilliseconds));

        //        var deleteIdList = currentList.Where(c => !newList.Any(n => n.mcs_identifier == c.mcs_identifier &&
        //                                                                    n.mcs_identifiertype.Value == c.mcs_identifiertype.Value &&
        //                                                                    n.mcs_assigningauthority == c.mcs_assigningauthority &&
        //                                                                    n.mcs_assigningfacility == c.mcs_assigningfacility)).ToList();
        //        //Logger.Instance.Debug(string.Format("***************Progress.{0}. Took {1} ms************", "Got List of Old IDs to be deleted", stopWatch.ElapsedMilliseconds));

        //        // Create the new identifiers linked to the contact record.
        //        foreach (var createId in createIdList)
        //        {
        //            createId.mcs_patient = new EntityReference(Contact.EntityLogicalName, contactId);
        //            proxy.Create(createId);
        //        }
        //        //Logger.Instance.Debug(string.Format("***************Progress.{0}. Took {1} ms************", "Created All New IDs", stopWatch.ElapsedMilliseconds));

        //        DeleteIds(deleteIdList, proxy);
        //        // Delete the old identifiers not in the current identifier list
        //        //foreach (var deleteId in deleteIdList)
        //        //{
        //        //    if (deleteId.mcs_personidentifiersId != null) proxy.Delete(mcs_personidentifiers.EntityLogicalName, deleteId.mcs_personidentifiersId.Value);
        //        //}
        //        //Logger.Instance.Debug(string.Format("***************Progress.{0}. Took {1} ms************", "Deleted OBE IDs", stopWatch.ElapsedMilliseconds));

        //    }
        //}

        public static void DeleteIds(List<mcs_personidentifiers> deleteIdList, IOrganizationService orgService)
        {
            if (deleteIdList.Count == 0) return;

            var execMultiple = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },

                Requests = new OrganizationRequestCollection()
            };

            foreach (var id in deleteIdList)
            {
                var deleteRequest = new DeleteRequest
                {
                    Target = id.ToEntityReference()
                };
                execMultiple.Requests.Add(deleteRequest);
            }

            try
            {
                var response = (ExecuteMultipleResponse)orgService.Execute(execMultiple);
                var errors = response.Responses.Where(r => r.Fault != null).ToList();
                //if (errors.Count > 0)
                //Logger.Instance.Debug("Failure to remove Identifiers no longer associated with this veteran: " + errors.Count);
            }
            catch (Exception)
            {
                //Logger.Instance.Debug("Failed to delete Person Identifiers: " + ex.Message + "||||" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Refresh all Person Identifiers.
        /// </summary>
        /// <param name="proxy">Organization Service Proxy.</param>
        /// <param name="contactId">Contact Id.</param>
        /// <param name="newList">New List.</param>
        public static void RefreshIdList(OrganizationWebProxyClient proxy, Guid contactId, List<mcs_personidentifiers> newList)
        {
            var stopWatch = Stopwatch.StartNew();

            using (var crm = new Xrm(proxy))
            {
                // Get the current list of identifiers
                var deleteIdList = (from i in crm.CreateQuery<mcs_personidentifiers>()
                                    where i.mcs_patient.Id == contactId
                                    select i).ToList();

                // Create the new identifiers linked to the contact record.
                foreach (var createId in newList)
                {
                    createId.mcs_patient = new EntityReference(Contact.EntityLogicalName, contactId);
                    proxy.Create(createId);
                }

                DeleteIds(deleteIdList, proxy);
            }
        }
    }
}
