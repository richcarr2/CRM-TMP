using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCS.ApplicationInsights;
using Microsoft.Crm.Sdk.Messages;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Provider_Availability
{
    public class UserGetProviderAvailabilityRunner : PluginRunner
    {
        public override string McsSettingsDebugField => "";

        public UserGetProviderAvailabilityRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override void Execute()
        {
            var requestData = PluginExecutionContext.InputParameters.Contains("SerializedQueryParameters") ? (string)PluginExecutionContext.InputParameters["SerializedQueryParameters"] : "";

            Logger.WriteDebugMessage($"Request Data: {requestData}");

            var queryParms = JsonSerializer.Deserialize<ProviderAvailabilityParameters>(requestData);

            Logger.WriteDebugMessage("Check for missing parameters");

            if (queryParms.Parameters.Count.Equals(0) || !queryParms.EndDate.HasValue || !queryParms.AvailabilityStartDate.HasValue)
            {
                Logger.WriteDebugMessage("Missing Parameters");
                var result = new FindAvailabilityResults
                {
                    ErrorMessage = queryParms.Parameters.Count.Equals(0)
                        ? "Missing Parameters"
                        : !queryParms.EndDate.HasValue
                            ? "Missing End Date"
                            : !queryParms.AvailabilityStartDate.HasValue
                                ? "Missing Start Date"
                                : string.Empty
                };

                PluginExecutionContext.OutputParameters.Add(new KeyValuePair<string, object>("SerializedQueryResults", JsonSerializer.Serialize(result)));
            }
            else
            {
                var providerAvailabilityList = new List<ProviderAvailability>();

                queryParms.Parameters.AsParallel().ToList().ForEach(param =>
                {
                    Logger.WriteDebugMessage($"Creating Request using Parameter Values: {JsonSerializer.Serialize(param)}");
                    Logger.WriteDebugMessage("Create Required Resource");

                    var providerReq = new RequiredResource
                    {
                        ResourceId = param.ProviderId
                    };

                    var clinicReq = new RequiredResource
                    {
                        ResourceId = param.ClinicId
                    };

                    var apptReq = new AppointmentRequest
                    {
                        Direction = SearchDirection.Forward,
                        NumberOfResults = 10000,
                        RequiredResources = new RequiredResource[] { clinicReq },
                        //RequiredResources = new RequiredResource[] { providerReq, clinicReq },
                        //// The search window describes the time when the resouce can be scheduled.
                        //// It must be set.
                        SearchWindowEnd = queryParms.EndDate.Value.ToUniversalTime(),
                        SearchWindowStart = queryParms.AvailabilityStartDate.Value.ToUniversalTime(),
                        ServiceId = param.ServiceId
                    };

                    var searchReq = new SearchRequest
                    {
                        AppointmentRequest = apptReq
                    };

                    try
                    {
                        var searchResp = (SearchResponse)OrganizationService.Execute(searchReq);

                        var results = JsonSerializer.Serialize(searchResp);

                        if (results.Length > 20000)
                        {
                            var contId = new Random().Next(19000);
                            var count = 19000;
                            var resultsArray = results.ToArray();
                            var message = resultsArray.Take(count);

                            do
                            {
                                if (count.Equals(19000))
                                {
                                    Logger.WriteDebugMessage($"{contId} - Find Availability Search Results: {new string(message.ToArray())}");
                                }
                                else
                                {
                                    contId++;
                                    Logger.WriteDebugMessage($"{contId} - Find Availability Search Results Cont: {new string(message.ToArray())}");
                                }
                                message = resultsArray.Skip(count).Take(19000);
                                count += 19000;
                                if (count > resultsArray.Count()) count = resultsArray.Count();
                            } while (message.Count() > 0);
                        }
                        else
                        {
                            Logger.WriteDebugMessage($"Find Availability Search Results: {results}");
                        }

                        if (searchResp != null && searchResp.SearchResults != null)
                        {
                            Logger.WriteDebugMessage($"Proposals Count: {searchResp.SearchResults.Proposals.Length}");

                            var providerName = GetProviderName(param.ProviderId);

                            var proposalsWithProviders = searchResp.SearchResults.Proposals.Where(p => p.ProposalParties.Any(su => su.EntityName.Equals("equipment") && p.ProposalParties.Any(c => c.ResourceId.Equals(param.ClinicId)))).ToList();

                            Logger.WriteDebugMessage($"Proposals With Providers Count: {proposalsWithProviders.Count}");

                            proposalsWithProviders.AsParallel().ToList().ForEach(proposal =>
                            {
                                if (proposal.ProposalParties.Any(pp => proposal.Start.HasValue && proposal.End.HasValue))
                                {
                                    Logger.WriteDebugMessage($"Adding Provider to Availbility List: {JsonSerializer.Serialize(proposal)}");

                                    providerAvailabilityList.Add(new ProviderAvailability
                                    {
                                        AvailabilityStartDate = proposal.Start.Value.ToString(),
                                        ClinicId = proposal.ProposalParties.Any(p => p.EntityName.Equals("equipment") && p.ResourceId.Equals(param.ClinicId)) ?
                                            proposal.ProposalParties.First(p => p.EntityName.Equals("equipment") && p.ResourceId.Equals(param.ClinicId)).ResourceId
                                            : Guid.Empty,
                                        ClinicName = proposal.ProposalParties.Any(p => p.EntityName.Equals("equipment") && p.ResourceId.Equals(param.ClinicId))
                                            ? proposal.ProposalParties.First(p => p.EntityName.Equals("equipment") && p.ResourceId.Equals(param.ClinicId)).DisplayName
                                            : string.Empty,
                                        Duration = proposal.End.HasValue && proposal.Start.HasValue ? (int)proposal.End.Value.Subtract(proposal.Start.Value).TotalMinutes : 0,
                                        EndDate = proposal.End.Value.ToString(),
                                        Facility = param.FacilityName,
                                        GroupAppointment = param.GroupAppointment,
                                        PatientLocationType = GetPatientLocationTypeName(param.PatientLocationType),
                                        PatientFacilityTimeZone = GetTimeZoneByCode(param.FacilityTimeZoneCode),
                                        ProviderFacilityStationName = param.ProviderFacilityStationName,
                                        ProviderFacilityStationNumber = param.ProviderFacilityStationNumber,
                                        ProviderFacilityTimeZone = GetTimeZoneByCode(param.ProviderFacilityTimeZoneCode),
                                        ProviderId = proposal.ProposalParties.Any(p => p.EntityName.Equals("systemuser"))
                                            ? proposal.ProposalParties.First(p => p.EntityName.Equals("systemuser")).ResourceId
                                            : param.ProviderId,
                                        ProviderName = proposal.ProposalParties.Any(p => p.EntityName.Equals("systemuser"))
                                            ? proposal.ProposalParties.First(p => p.EntityName.Equals("systemuser")).DisplayName
                                            : providerName,
                                        SchedulingPackageModality = param.SchedulingPackageModality,
                                        SchedulingPackageName = param.SchedulingPackageName,
                                        SiteId = param.SiteId,
                                        SiteName = param.SiteName,
                                        VISN = param.VISN
                                    });
                                }
                            });
                        }
                        else
                        {
                            Logger.WriteDebugMessage("No Results Found");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.WriteDebugMessage($"Find Availability Search Failed: {e}");
                        throw;
                    }
                });

                var searchResults = new FindAvailabilityResults
                {
                    ProviderAvailabilities = providerAvailabilityList
                };

                PluginExecutionContext.OutputParameters.Add(new KeyValuePair<string, object>("SerializedQueryResults", SerializationHelper.Serialize(searchResults)));
            }
        }

        private string GetPatientLocationTypeName(int patientLocationType)
        {
            switch (patientLocationType)
            {
                case 917290000:
                    return "Clinic Based";
                case 917290001:
                    return "VA Video Connect/Telephone";
                default:
                    return "NA";
            }

        }

        private string GetProviderName(Guid providerId)
        {
            if (providerId.Equals(Guid.Empty)) return "";

            using (var srv = new Xrm(OrganizationService))
            {
                var provider = srv.SystemUserSet.FirstOrDefault(su => su.SystemUserId.Equals(providerId));
                var providerName = provider.FullName;
                return providerName;
            }
        }

        private string GetTimeZoneByCode(int timeZoneCode)
        {
            if (timeZoneCode.Equals(0)) return "";

            using (var srv = new Xrm(OrganizationService))
            {
                var timeZone = srv.TimeZoneDefinitionSet.FirstOrDefault(tz => tz.TimeZoneCode.Equals(timeZoneCode));
                var timeZoneName = timeZone.Id.Equals(Guid.Empty) ? "" : timeZone.UserInterfaceName;
                return timeZoneName;
            }
        }
    }

    [DataContract]
    class ProviderAvailability
    {
        [DataMember(EmitDefaultValue = false, Name = "availabilityStartDate")]
        public string AvailabilityStartDate { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "clinicId")]
        public Guid ClinicId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "clinicName")]
        public string ClinicName { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "duration")]
        public int Duration { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "endDate")]
        public string EndDate { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "facility")]
        public string Facility { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "groupAppointment")]
        public bool GroupAppointment { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "patientLocationType")]
        public string PatientLocationType { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "patientFacilityTimeZone")]
        public string PatientFacilityTimeZone { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "providerFacilityStationName")]
        public string ProviderFacilityStationName { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "providerFacilityStationNumber")]
        public string ProviderFacilityStationNumber { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "providerFacilityTimeZone")]
        public string ProviderFacilityTimeZone { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "providerId")]
        public Guid ProviderId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "providerName")]
        public string ProviderName { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "schedulingPackageModality")]
        public string SchedulingPackageModality { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "schedulingPackageName")]
        public string SchedulingPackageName { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "siteId")]
        public Guid SiteId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "siteName")]
        public string SiteName { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "visn")]
        public string VISN { get; set; }
    }

    [Serializable]
    class ProviderAvailabilityParameters
    {
        [JsonPropertyName("parameters")]
        public List<ProviderAvailabilityParameter> Parameters { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("availabilityStartDate")]
        public DateTime? AvailabilityStartDate { get; set; }
    }

    class ProviderAvailabilityParameter
    {
        [JsonPropertyName("clinicId")]
        public Guid ClinicId { get; set; }

        [JsonPropertyName("clinicName")]
        public string ClinicName { get; set; }

        [JsonPropertyName("facilityName")]
        public string FacilityName { get; set; }

        [JsonPropertyName("facilityTimeZoneCode")]
        public int FacilityTimeZoneCode { get; set; }

        [JsonPropertyName("cvt_groupappointment")]
        public bool GroupAppointment { get; set; }

        [JsonPropertyName("cvt_patientlocationtype")]
        public int PatientLocationType { get; set; }

        [JsonPropertyName("providerFacilityStationName")]
        public string ProviderFacilityStationName { get; set; }

        [JsonPropertyName("providerFacilityStationNumber")]
        public string ProviderFacilityStationNumber { get; set; }

        [JsonPropertyName("providerFacilityTimeZoneCode")]
        public int ProviderFacilityTimeZoneCode { get; set; }

        [JsonPropertyName("providerId")]
        public Guid ProviderId { get; set; }

        [JsonPropertyName("schedulingPackageModality")]
        public string SchedulingPackageModality { get; set; }

        [JsonPropertyName("schedulingPackageName")]
        public string SchedulingPackageName { get; set; }

        [JsonPropertyName("serviceId")]
        public Guid ServiceId { get; set; }

        [JsonPropertyName("siteName")]
        public string SiteName { get; set; }

        [JsonPropertyName("siteId")]
        public Guid SiteId { get; set; }

        [JsonPropertyName("visn")]
        public string VISN { get; set; }
    }

    [DataContract]
    class FindAvailabilityResults
    {
        [DataMember(Name = "providerAvailability")]
        public List<ProviderAvailability> ProviderAvailabilities { get; set; }

        [DataMember(Name = "errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
