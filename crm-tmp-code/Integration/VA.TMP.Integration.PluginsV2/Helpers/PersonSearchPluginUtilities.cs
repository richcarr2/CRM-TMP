using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.Mvi;

namespace VA.TMP.Integration.Plugins.Helpers
{
    /// <summary>
    /// Static helper methods for Person Search.
    /// </summary>
    public static class PersonSearchPluginUtilities
    {
        static MCSLogger _logger;

        /// <summary>
        /// Retreives the search type from the input query expression.
        /// </summary>
        /// <param name="qe">The query expression generated from the input parameters.</param>
        /// <param name="filterKey">The condition key in the query expression to search for.</param>
        /// <param name="filterValue">The value of the condition.</param>
        /// <returns>True if the value was found; False otherwise.</returns>
        public static bool TryGetFilterValue(QueryExpression qe, string filterKey, out string filterValue)
        {
            filterValue = string.Empty;
            try
            {
                var filters = qe.Criteria;
                FilterExpression result = null;

                result = SearchFilter(filters, filterKey);

                if (result == null || result.Conditions == null) return false;

                if (!result.Conditions.Any(x => x.AttributeName.Equals(filterKey, StringComparison.CurrentCultureIgnoreCase))) return false;

                var condition = result.Conditions.First(x => x.AttributeName.Equals(filterKey, StringComparison.CurrentCultureIgnoreCase)).Values.FirstOrDefault();
                if (condition != null) filterValue = condition.ToString();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ConditionExpression CheckConditionsForConditionExpression(DataCollection<ConditionExpression> ces, string filter)
        {
            ConditionExpression expression = null;
            foreach (var ce in ces)
            {
                if (ce.AttributeName.ToLower() == filter.ToLower())
                {
                    expression = ce;
                    break;
                }
            }
            return expression;
        }

        public static bool TryGetConditionOptionSet(QueryExpression qe, string filterKey, out int value)
        {
            value = 0;
            try
            {
                ConditionExpression condition = null;
                var filterFound = SearchFilter(qe.Criteria, filterKey);
                condition = CheckConditionsForConditionExpression(filterFound.Conditions, filterKey);

                condition = condition ?? CheckConditionsForConditionExpression(qe.Criteria.Conditions, filterKey);
                if (condition == null)
                {
                    foreach (var filter in qe.Criteria.Filters)
                    {
                        condition = CheckConditionsForConditionExpression(filter.Conditions, filterKey);
                        if (condition != null) break;
                    }
                }
                value = (int)condition.Values.FirstOrDefault();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method used find the crme_searchtype condition expression.
        /// </summary>
        /// <param name="expression">The filter expression to search.</param>
        /// <param name="attributeName">The name of the attribute to locate in a condition expression.</param>
        /// <returns>The filter expression containing the attribute name.</returns>
        private static FilterExpression SearchFilter(FilterExpression expression, string attributeName)
        {
            if (expression.Conditions.Any(x => x.AttributeName.Equals(attributeName, StringComparison.CurrentCultureIgnoreCase))) return expression;

            return expression.Filters.Count <= 0
                ? null
                : expression.Filters.Select(filter => SearchFilter(filter, attributeName)).FirstOrDefault(result => result != null);
        }

        /// <summary>
        /// Generates the person search request for VIMT from a query expression object.
        /// </summary>
        /// <param name="queryExpression">The CRM query expression containing the query parameters.</param>
        /// <param name="pluginExecutionContext">The Plugin Execution Context.</param>
        /// <param name="orgService">The Organization Service.</param>
        /// <returns>The PersonSearchRequest object.</returns>
        internal static PersonSearchRequestMessage GetPersonSearchRequest(QueryExpression queryExpression, IPluginExecutionContext pluginExecutionContext, IOrganizationService orgService, MCSLogger Logger)
        {
            _logger = Logger;
               var request = new PersonSearchRequestMessage();

            if (queryExpression.Criteria == null) return request;

            if (queryExpression.Criteria.Filters.Any())
            {
                request.Edipi = GetStringOrDefaultValue(queryExpression.Criteria, "crme_EDIPI");
                if (string.IsNullOrEmpty(request.Edipi))
                {
                    Logger.WriteDebugMessage("No EDIPI");
                    request.Ss = GetStringOrDefaultValue(queryExpression.Criteria, "crme_SSN");
                    request.FirstName = GetStringOrDefaultValue(queryExpression.Criteria, "crme_FirstName");
                    request.MiddleName = GetStringOrDefaultValue(queryExpression.Criteria, "crme_MiddleName");
                    request.FamilyName = GetStringOrDefaultValue(queryExpression.Criteria, "crme_LastName");
                    request.BirthDate = GetStringOrDefaultValue(queryExpression.Criteria, "crme_DOBString");
                    request.PhoneNumber = GetStringOrDefaultValue(queryExpression.Criteria, "crme_PrimaryPhone");
                }
                else
                {
                    Logger.WriteDebugMessage("got EDIPI, goiung for identifier:"  + request.Edipi);
                    request.PatientIdentifier = GetStringOrDefaultValue(queryExpression.Criteria, "crme_PatientMviIdentifier"); 
                }
                request.Query = GetStringOrDefaultValue(queryExpression.Criteria, "crme_searchtype");

                var isAttended = GetStringOrDefaultValue(queryExpression.Criteria, "crme_isattended");
                request.IsAttended = Convert.ToBoolean(string.IsNullOrEmpty(isAttended) ? "false" : isAttended);
                request.OrganizationName = pluginExecutionContext.OrganizationName;
                request.FetchMessageProcessType = MessageProcessType.Local;

                var user = orgService.Retrieve(SystemUser.EntityLogicalName, pluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();
                request.UserId = user.Id;
                request.UserFirstName = user.FirstName;
                request.UserLastName = user.LastName;
            }
            else
            {
                throw new InvalidPluginExecutionException("No search parameters specified");
            }

            return request;
        }

        /// <summary>
        /// Generates the selected person request for VIMT from a query expression object.
        /// </summary>
        /// <param name="queryExpression">Query Expression.</param>
        /// <param name="pluginExecutionContext">Plugin Execution Context.</param>
        /// <param name="organizationService">Organization Service.</param>
        /// <returns>GetPersonIdentifiersRequestMessage.</returns>
        internal static GetPersonIdentifiersRequestMessage GetSelectedPersonRequest(QueryExpression queryExpression, IPluginExecutionContext pluginExecutionContext, IOrganizationService organizationService, MCSLogger Logger)
        {
            _logger = Logger;
            var request = new GetPersonIdentifiersRequestMessage();

            if (queryExpression.Criteria == null) return request;
            if (!queryExpression.Criteria.Filters.Any()) return request;

            request.DateofBirth = GetStringOrDefaultValue(queryExpression.Criteria, "crme_DOBString");
            request.DateofDeath = GetStringOrDefaultValue(queryExpression.Criteria, "crme_DeceasedDate");
            request.Edipi = GetStringOrDefaultValue(queryExpression.Criteria, "crme_EDIPI");
            request.FetchMessageProcessType = MessageProcessType.Remote;
            request.FullAddress = GetStringOrDefaultValue(queryExpression.Criteria, "crme_FullAddress");
            request.FullName = GetStringOrDefaultValue(queryExpression.Criteria, "crme_FullName");
            request.IdentifierClassCode = GetStringOrDefaultValue(queryExpression.Criteria, "crme_ClassCode");
            request.OrganizationName = pluginExecutionContext.OrganizationName;
            request.RawValueFromMvi = GetStringOrDefaultValue(queryExpression.Criteria, "crme_PatientMviIdentifier");
            request.RecordSource = GetStringOrDefaultValue(queryExpression.Criteria, "crme_RecordSource");
            request.SocialSecurityNumber = GetStringOrDefaultValue(queryExpression.Criteria, "crme_SSN");
            request.IdTheftIndicator = GetStringOrDefaultValue(queryExpression.Criteria, "crme_IdentityTheft");
            request.Gender = GetStringOrDefaultValue(queryExpression.Criteria, "crme_Gender");
            request.PhoneNumber = GetStringOrDefaultValue(queryExpression.Criteria, "crme_PrimaryPhone");

            var idString = GetStringOrDefaultValue(queryExpression.Criteria, "crme_ReturnMessage");
            if (!string.IsNullOrEmpty(idString)) request.CorrespondingIds = DeserializeIdentifiersString(idString);

            var aliasString = GetStringOrDefaultValue(queryExpression.Criteria, "crme_Alias");
            request.Alias = !string.IsNullOrEmpty(aliasString) ? ReformatAliasString(aliasString) : string.Empty;

            var user = organizationService.Retrieve(SystemUser.EntityLogicalName, pluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();
            request.UserId = user.Id;
            request.UserFirstName = user.FirstName;
            request.UserLastName = user.LastName;

            return request;
        }

        /// <summary>
        /// Takes a pipe delimited string of caret dimilited name parts and formats it into a list of names with pattern LastName Suffix, FirstName MiddleName.
        /// </summary>
        /// <param name="aliasString">The delimited list of names.</param>
        /// <returns>The reformatted list.</returns>
        private static string ReformatAliasString(string aliasString)
        {
            var reformattedString = new StringBuilder();
            var aliases = aliasString.Split('|');

            if (aliases.Length <= 0) return reformattedString.ToString().Trim().TrimEnd(';');
            var nameString = new StringBuilder();

            foreach (var alias in aliases)
            {
                nameString.Clear();
                var parts = alias.Split('^');

                if (parts.Length >= 0)
                {
                    if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0])) nameString.Append(parts[0]);

                    if (parts.Length > 3 && !string.IsNullOrEmpty(parts[3])) nameString.Append(" " + parts[3] + ",");
                    else nameString.Append(",");

                    if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1])) nameString.Append(" " + parts[1]);

                    if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2])) nameString.Append(" " + parts[2]);
                }

                var name = nameString.ToString().Trim();

                if (!string.IsNullOrEmpty(name)) reformattedString.Append(name + "; ");
            }

            return reformattedString.ToString().Trim().TrimEnd(';');
        }

        /// <summary>
        /// Retreives the value of the specified field from the input query expression.
        /// </summary>
        /// <param name="filterExpression">The filterExpression tree that could contain teh referenced field name.</param>
        /// <param name="fieldName">The name of the field to search for..</param>
        /// <returns>The value of the field or null if not found.</returns>
        private static string GetStringOrDefaultValue(FilterExpression filterExpression, string fieldName)
        {
            try
            {
                if (filterExpression.Conditions == null) return string.Empty;

                // If the value is in the parent expression, get the value and return.
                var condition = filterExpression.Conditions.FirstOrDefault(x => x.AttributeName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
                if (condition != null)
                {
                    var value = condition.Values.FirstOrDefault();

                    if (value != null) return value.ToString();
                }

                FilterExpression result = null;
                foreach (var filter in filterExpression.Filters)
                {
                    result = SearchFilter(filter, fieldName);
                    if (result != null) break;
                }

                if (result == null) return string.Empty;

                var conditionExpression = result.Conditions.FirstOrDefault(x => x.AttributeName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));

                if (conditionExpression == null) return string.Empty;
                var conditionExpressionValue = conditionExpression.Values.FirstOrDefault();

                return conditionExpressionValue != null ? conditionExpressionValue.ToString().Replace("%APOS", "'") : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get Person Search Response.
        /// </summary>
        /// <param name="request">PersonSearchRequestMessage.</param>
        /// <param name="response">RetrieveOrSearchPersonResponse.</param>
        /// <param name="orgService">IOrganizationService.</param>
        /// <returns>Collection of Veterans.</returns>
        internal static List<Entity> GetPersonSearchResponse(PersonSearchRequestMessage request, RetrieveOrSearchPersonResponse response, IOrganizationService orgService)
        {

            var output = new List<Entity>();

            // If candidates are returned, format the output object and add person specific data
            if (response.Person != null && response.Person.Length > 0)
            {
                foreach (var person in response.Person)
                {
                    var crmePerson = new Entity { Id = Guid.NewGuid(), LogicalName = crme_person.EntityLogicalName };
                    var legalName = GetLegalName(person.NameList);
                    var identifiers = GetIdentifiers(person);

                    EntityReference country;
                    EntityReference state;
                    EntityReference postCode;

                    using (var crm = new Xrm(orgService))
                    {
                        if (!string.IsNullOrEmpty(person.Address.Country) && person.Address.Country.Length >= 2)
                            country = (from c in crm.CreateQuery<crme_countrylookup>()
                                       where c.crme_country == person.Address.Country.Substring(0, 2)
                                       select new EntityReference
                                       {
                                           LogicalName = crme_countrylookup.EntityLogicalName,
                                           Id = c.crme_countrylookupId.Value,
                                           Name = c.crme_country
                                       }).FirstOrDefault();
                        else
                            country = null;

                        if (!string.IsNullOrEmpty(person.Address.State))
                            state = (from c in crm.CreateQuery<crme_stateorprovincelookup>()
                                     where c.crme_stateorprovince == person.Address.State
                                     select new EntityReference
                                     {
                                         LogicalName = crme_stateorprovincelookup.EntityLogicalName,
                                         Id = c.crme_stateorprovincelookupId.Value,
                                         Name = c.crme_stateorprovince
                                     }).FirstOrDefault();
                        else
                            state = null;

                        if (!string.IsNullOrEmpty(person.Address.PostalCode) && person.Address.PostalCode.Length >= 5)
                            postCode = (from c in crm.CreateQuery<crme_postalcodelookup>()
                                        where c.crme_postalcode == person.Address.PostalCode.Substring(0, 5)
                                        select new EntityReference
                                        {
                                            LogicalName = crme_postalcodelookup.EntityLogicalName,
                                            Id = c.crme_postalcodelookupId.Value,
                                            Name = c.crme_postalcode
                                        }).FirstOrDefault();
                        else
                            postCode = null;
                    }

                    crmePerson["crme_address1"] = person.Address.StreetAddressLine;
                    crmePerson["crme_address2"] = null;
                    crmePerson["crme_address3"] = null;
                    crmePerson["crme_alias"] = GetAliasNames(person.NameList);
                    crmePerson["crme_branchofservice"] = person.BranchOfService;
                    crmePerson["crme_city"] = person.Address.City;
                    crmePerson["crme_countryid"] = country;
                    crmePerson["crme_deceaseddate"] = person.DeceasedDate;

                    // TODO Determine why attributes aren't coming back through to the plugin
                    crmePerson["crme_dob"] = GetBirthDate(person.BirthDate);

                    crmePerson["crme_dobstring"] = person.BirthDate;
                    crmePerson["crme_edipi"] = identifiers["edipi"];
                    crmePerson["crme_email"] = null;
                    crmePerson["crme_filenumber"] = null;
                    crmePerson["crme_firstname"] = legalName["firstName"];
                    crmePerson["crme_fulladdress"] = GetFullAddress(person.Address);
                    crmePerson["crme_fullname"] = legalName["fullName"];
                    crmePerson["crme_gender"] = person.GenderCode;
                    crmePerson["crme_identitytheft"] = person.IdentifyTheft;
                    crmePerson["crme_isattended"] = request.IsAttended;
                    crmePerson["crme_lastname"] = legalName["lastName"];
                    crmePerson["crme_middlename"] = legalName["middleName"];
                    crmePerson["crme_namesuffix"] = legalName["suffix"];
                    crmePerson["crme_mvimessage"] = EvaluateMviResponse(response);
                    crmePerson["crme_parentid"] = null;
                    crmePerson["crme_participantid"] = identifiers["particpantId"];
                    crmePerson["crme_patientid"] = identifiers["patientId"];
                    crmePerson["crme_patientmviidentifier"] = identifiers["patientMviId"];
                    crmePerson["crme_patientno"] = identifiers["patientNo"];
                    crmePerson["crme_personid"] = Guid.Empty;
                    crmePerson["crme_personrecordtype"] = null;
                    crmePerson["crme_primaryphone"] = person.PhoneNumber;
                    crmePerson["crme_recordsource"] = person.RecordSource;
                    crmePerson["crme_relationshiptype"] = null;
                    crmePerson["crme_searchtype"] = request.Query;
                    crmePerson["crme_secondaryphone"] = null;
                    crmePerson["crme_ssn"] = identifiers["ss"];
                    crmePerson["crme_stateprovinceid"] = state;
                    crmePerson["crme_storedssn"] = null;
                    crmePerson["crme_url"] = null;
                    crmePerson["crme_veteransensitivitylevel"] = null;
                    crmePerson["crme_zippostalcodeid"] = postCode;

                    // If the EDIPI number was used as the sole query parameter, all the Id's needed will be returned on the first call to MVI.
                    // Serialize the ids and return to the calling page so that the list can be returned after id proofing.
                    if (request.Query == "SearchByIdentifier") crmePerson["crme_returnmessage"] = SerializeIdentifierList(person.CorrespondingIdList);

                    output.Add(crmePerson);
                }
            }
            else
            {
                // Create an error response
                var crmePerson = new Entity
                {
                    Id = Guid.NewGuid(),
                    LogicalName = crme_person.EntityLogicalName
                };

                crmePerson["crme_mvimessage"] = EvaluateMviResponse(response);

                if (!string.IsNullOrEmpty(response.RawMviExceptionMessage))
                {
                    crmePerson["crme_returnmessage"] = response.RawMviExceptionMessage;
                }

                output.Add(crmePerson);
            }

            return output;
        }

        /// <summary>
        /// Get Selected Person Ids.
        /// </summary>
        /// <param name="response">Response.</param>
        /// <param name="serverName">Server Name.</param>
        /// <returns>List of People.</returns>
        internal static List<Entity> GetSelectedPersonIds(GetPersonIdentifiersResponseMessage response, string serverName)
        {
            var persons = new List<Entity>();
            var person = new Entity
            {
                // This is needed to return correctly to the caller
                Id = Guid.NewGuid(),
                LogicalName = crme_person.EntityLogicalName
            };
            person["crme_mvimessage"] = response.ExceptionMessage;

            if (response.ExceptionOccured)
            {
                person["crme_returnmessage"] = response.ExceptionMessage;
            }
            else if (string.IsNullOrEmpty(response.Url) || response.ContactId == Guid.Empty)
            {
                person["crme_returnmessage"] = "Did not receive a response from MVI that allows the display of the Contact record.";
            }
            else
            {
                if (!string.IsNullOrEmpty(serverName))
                {
                    const string urlFormat = @"{0}/main.aspx?etn=contact&pagetype=entityrecord&id=%7B{1}%7D";
                    person["crme_url"] = string.Format(urlFormat, serverName, response.ContactId);
                }
                else
                    person["crme_url"] = response.Url;

                person["crme_contactid"] = response.ContactId.ToString();
                person["crme_fullname"] = response.FullName;
                person["crme_firstname"] = response.FirstName;
                person["crme_lastname"] = response.FamilyName;
                person["crme_middlename"] = response.MiddleName;
                person["crme_namesuffix"] = response.Suffix;
            }

            persons.Add(person);
            return persons;
        }

        #region PersonSearchResponse Helpers
        /// <summary>
        /// Finds and returns the Legal name from an array of names returned from an MVI search.
        /// </summary>
        /// <param name="names">The array to search.</param>
        /// <returns>A dictionary containing the parts of the name plust the full name.</returns>
        private static Dictionary<string, string> GetLegalName(Name[] names)
        {
            var output = new Dictionary<string, string>();
            var name = names.First(n => n.NameType == "Legal");
            output.Add("lastName", name.FamilyName);
            output.Add("firstName", name.GivenName);
            output.Add("middleName", name.MiddleName);
            output.Add("suffix", name.NameSuffix);

            var sb = new StringBuilder();
            sb.Append((!string.IsNullOrEmpty(name.FamilyName)) ? name.FamilyName : "{last name missing}");
            sb.Append((!string.IsNullOrEmpty(name.NameSuffix)) ? " " + name.NameSuffix : string.Empty);
            sb.Append(", ");
            sb.Append((!string.IsNullOrEmpty(name.GivenName)) ? name.GivenName : "{first name missing}");
            sb.Append((!string.IsNullOrEmpty(name.MiddleName)) ? " " + name.MiddleName : string.Empty);

            output.Add("fullName", sb.ToString().Trim());

            return output;
        }

        /// <summary>
        /// Returns a dictionary containing all the identifiers for a person returned from an MVI search.
        /// </summary>
        /// <param name="person">A PatientPerson object returned from MVI.</param>
        /// <returns>A dictionary containing the indentifiers.</returns>
        private static Dictionary<string, string> GetIdentifiers(PatientPerson person)
        {
            var output = new Dictionary<string, string>();

            var nationalId = person.CorrespondingIdList.FirstOrDefault(i => i.IdentifierType == "NI");
            var ss = person.CorrespondingIdList.FirstOrDefault(i => i.IdentifierType == "Ss");
            var participantId = person.CorrespondingIdList.FirstOrDefault(i => i.IdentifierType == "PI");
            var patientNo = person.CorrespondingIdList.FirstOrDefault(i => i.IdentifierType == "PN");

            output.Add("patientMviId", nationalId != null ? nationalId.RawValueFromMvi : null);

            output.Add("ss", ss != null ? ss.PatientIdentifier : person.Ss);

            output.Add("particpantId", participantId != null ? participantId.PatientIdentifier : person.ParticipantId);

            output.Add("patientNo", patientNo != null ? patientNo.PatientIdentifier : null);

            output.Add("edipi", person.EdiPi);
            output.Add("patientId", person.Identifier);

            return output;
        }

        /// <summary>
        /// Creates a formatted name from a name object with type of Alias.
        /// </summary>
        /// <param name="names">The name object.</param>
        /// <returns>The formatted name string.</returns>
        private static string GetAliasNames(Name[] names)
        {
            var aliases = names.Where(n => n.NameType == "Alias").ToList();

            if (!aliases.Any()) return string.Empty;

            const string aliasFormat = "{0}^";
            const string nameFormat = "{0}|";
            var nameList = new StringBuilder();
            var name = new StringBuilder();

            foreach (var alias in aliases)
            {
                name.Clear();
                name.AppendFormat(aliasFormat, string.IsNullOrEmpty(alias.FamilyName) ? string.Empty : alias.FamilyName);
                name.AppendFormat(aliasFormat, string.IsNullOrEmpty(alias.GivenName) ? string.Empty : alias.GivenName);
                name.AppendFormat(aliasFormat, string.IsNullOrEmpty(alias.MiddleName) ? string.Empty : alias.MiddleName);
                name.AppendFormat(aliasFormat, string.IsNullOrEmpty(alias.NameSuffix) ? string.Empty : alias.NameSuffix);

                if (name.ToString() != "^^^^") nameList.AppendFormat(nameFormat, name);
            }

            return nameList.ToString().TrimEnd('|');
        }

        /// <summary>
        /// Gets a DateTime object representing a person's birthdate from a string formatted as {yyyyMMdd}.
        /// </summary>
        /// <param name="dateOfBirth">The string containing the birthdate.</param>
        /// <returns>A DateTime object with the birthdate or a null value.</returns>
        /// <remarks>This is not a culture safe function. Expects date string in the format indicated.</remarks>
        private static DateTime? GetBirthDate(string dateOfBirth)
        {
            if (dateOfBirth.Length < 8) return null;

            var sYear = dateOfBirth.Substring(0, 4);
            var sMonth = dateOfBirth.Substring(4, 2);
            var sDay = dateOfBirth.Substring(6, 2);
            var sDate = string.Format("{0}/{1}/{2}", sMonth, sDay, sYear);

            DateTime date;
            if (DateTime.TryParse(sDate, out date)) return date;

            return null;
        }

        /// <summary>
        /// Returns a formated address from a patient address object.
        /// </summary>
        /// <param name="address">The object containing the address information.</param>
        /// <returns>The formatted address string.</returns>
        private static string GetFullAddress(PatientAddress address)
        {
            var sb = new StringBuilder();
            const string format = "{0}|";
            sb.AppendFormat(format, string.IsNullOrEmpty(address.StreetAddressLine) ? string.Empty : address.StreetAddressLine);
            sb.AppendFormat(format, string.IsNullOrEmpty(address.City) ? string.Empty : address.City);
            sb.AppendFormat(format, string.IsNullOrEmpty(address.State) ? string.Empty : address.State);
            sb.AppendFormat(format, string.IsNullOrEmpty(address.PostalCode) ? string.Empty : address.PostalCode);
            sb.AppendFormat(format, string.IsNullOrEmpty(address.Country) ? string.Empty : address.Country);

            return sb.ToString();
        }

        /// <summary>
        /// Determines the correct response message to return to the UI based on the codes received from MVI.
        /// </summary>
        /// <param name="response">The response returned from VIMT.</param>
        /// <returns>The response code the UI should interpret to display the correct message to the user.</returns>
        private static string EvaluateMviResponse(RetrieveOrSearchPersonResponse response)
        {
            var output = "[OK]";
            const string format = "[{0}]";
            const string errorFormat = "[ER] {0}";
            const string unknown = "An unknown error was returned from MVI. If the problem persists, please contact your system administrator.";

            switch (response.Acknowledgement.TypeCode)
            {
                case "AA":
                    if (response.QueryAcknowledgement.QueryResponseCode == "NF" || response.QueryAcknowledgement.QueryResponseCode == "QE")
                        output = string.Format(format, response.QueryAcknowledgement.QueryResponseCode);
                    break;
                case "AE":
                case "AR":
                    if (response.Acknowledgement.AcknowledgementDetails != null && response.Acknowledgement.AcknowledgementDetails.Length > 0)
                    {
                        // The two messages below are specific to EDIPI (unattended) searches and should be treated as a "not found" error.
                        output = response.Acknowledgement.AcknowledgementDetails.Any(a => a.Text.Contains("Correlation Does Not Exist") || a.Text.Contains("No ACTIVE Correlation found"))
                            ? string.Format(format, "NF")
                            : string.Format(errorFormat, response.Acknowledgement.AcknowledgementDetails[0].Text);
                    }
                    else
                        output = string.Format(errorFormat, unknown);
                    break;
                default:
                    output = string.Format(errorFormat, unknown);
                    break;
            }

            return output;
        }

        /// <summary>
        /// Serializes the corresponding id array.
        /// </summary>
        /// <param name="identifiers">The array containing the identifiers.</param>
        /// <returns>A pipe delimited string of the identifiers.</returns>
        private static string SerializeIdentifierList(UnattendedSearchRequest[] identifiers)
        {
            if (identifiers == null || identifiers.Length == 0) return string.Empty;

            var sb = new StringBuilder();

            foreach (var identifier in identifiers) sb.AppendFormat("{0}|", identifier);

            return sb.ToString().TrimEnd('|');
        }

        /// <summary>
        /// Deserializes a string containing a pipe delimited list of identifiers to a CorrespondingIDs array.
        /// </summary>
        /// <param name="identifiers">The string containing the identifier list.</param>
        /// <returns>The array of CorrespondingIDs.</returns>
        private static UnattendedSearchRequest[] DeserializeIdentifiersString(string identifiers)
        {
            if (string.IsNullOrEmpty(identifiers)) return null;

            var ids = identifiers.Split('|');

            return ids.Length == 0 ? null : ids.Select(ConvertString).ToArray();
        }

        /// <summary>
        /// Converts the string representation of the object to it's object.
        /// </summary>
        /// <param name="item">The string to convert.</param>
        /// <returns>The CorrespondingIDs object.</returns>
        private static UnattendedSearchRequest ConvertString(string item)
        {
            if (string.IsNullOrEmpty(item)) return null;

            var parts = item.Split('^');

            if (parts.Length == 0) return null;

            if (parts.Length < 4) return new UnattendedSearchRequest { RawValueFromMvi = item };

            return new UnattendedSearchRequest
            {
                PatientIdentifier = parts[0],
                IdentifierType = parts[1],
                AssigningFacility = parts[2],
                AssigningAuthority = parts[3]
            };
        }
        #endregion
    }
}
