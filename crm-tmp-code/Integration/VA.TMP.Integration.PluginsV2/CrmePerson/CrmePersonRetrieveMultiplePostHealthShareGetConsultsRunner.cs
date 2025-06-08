using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.CrmePerson
{
    public class CrmePersonRetrieveMultiplePostHealthShareGetConsultsRunner : PluginRunner
    {
        public CrmePersonRetrieveMultiplePostHealthShareGetConsultsRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "crme_person";

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        private List<Entity> ConsultsAndRtcs { get; set; }

        public override void Execute()
        {
            try
            {
                if (!PluginExecutionContext.InputParameters.Contains("Query") || !(PluginExecutionContext.InputParameters["Query"] is QueryExpression || PluginExecutionContext.InputParameters["Query"] is FetchExpression)) return;
                QueryExpression qe;

                var query = PluginExecutionContext.InputParameters["Query"];

                if (query.GetType() == typeof(FetchExpression))
                    qe = IntegrationPluginHelpers.ConvertFetchExpressionToQueryExpression((FetchExpression)query, OrganizationService, Logger);
                else
                    qe = (QueryExpression)PluginExecutionContext.InputParameters["Query"];

                PersonSearchPluginUtilities.TryGetConditionOptionSet(qe, "cvt_IntegrationType", out var integrationTypeValue);
                if (integrationTypeValue != (int)crme_personcvt_IntegrationType.GetConsultsForPatient) return;

                PersonSearchPluginUtilities.TryGetFilterValue(qe, "cvt_patientsiteid", out var patientSiteId);
                PersonSearchPluginUtilities.TryGetFilterValue(qe, "crme_contactid", out var patientContactId);
                PersonSearchPluginUtilities.TryGetFilterValue(qe, "cvt_providerfacilityid", out var providerFacilityId);
                PersonSearchPluginUtilities.TryGetFilterValue(qe, "cvt_issft", out var sft);
                PersonSearchPluginUtilities.TryGetFilterValue(qe, "cvt_ishomemobile", out var homeMobile);
                PersonSearchPluginUtilities.TryGetFilterValue(qe, "cvt_providerstationcode", out var providerStationCode);

                Logger.WriteToFile($"Patient Site Id: {patientSiteId}");
                Logger.WriteToFile($"Provider Station Code: {providerStationCode}");
                Logger.WriteToFile($"Provider Facility Id: {providerFacilityId}");

                var patientStationNumber = GetStationNumberFromSiteId(patientSiteId, Logger);
                var providerStationNumber = string.IsNullOrEmpty(providerStationCode) ? 0 : Convert.ToInt32(providerStationCode);

                Logger.WriteToFile($"Patient Station Number: {patientStationNumber}");
                
                var isSft = Convert.ToBoolean(sft);
                var isHomeMobile = Convert.ToBoolean(homeMobile);

                using (var context = new Xrm(OrganizationService))
                {
                    ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "HsGetConsultsUri");
                }

                var patientIds = new List<Guid> { new Guid(patientContactId) };

                var patientLoginStationNumber = ExtractStationNumber(patientStationNumber);
                
                Logger.WriteToFile($"Patient Login Station Number: {patientLoginStationNumber}");
                Logger.WriteToFile("Populating HealthShareGetConsultsRequest");

                var healthshareRequest = new TmpHealthShareGetConsultsRequest
                {
                    PatientIds = patientIds,
                    PatientLoginStationNumber = patientLoginStationNumber,
                    ProviderLoginStationNumber = providerStationNumber,
                    LogRequest = true,
                    UserId = PluginExecutionContext.UserId,
                    OrganizationName = PluginExecutionContext.OrganizationName,
                    IsHomeMobile = isHomeMobile,
                    IsStoreForward = isSft,
                };

                Logger.WriteToFile($"Get Consults Request: {Serialization.DataContractSerialize<TmpHealthShareGetConsultsRequest>(healthshareRequest)}");

                Logger.WriteToFile("About to send/receive HealthShareGetConsults message.");

                var baseUrl = ApiIntegrationSettings.BaseUrl;
                var uri = ApiIntegrationSettings.Uri;
                var resource = ApiIntegrationSettings.Resource;
                var appId = ApiIntegrationSettings.AppId;
                var secret = ApiIntegrationSettings.Secret;
                var authority = ApiIntegrationSettings.Authority;
                var tenantId = ApiIntegrationSettings.TenantId;
                var subscriptionId = ApiIntegrationSettings.SubscriptionId;
                var isProdApi = ApiIntegrationSettings.IsProdApi;
                var subscriptionIdEast = ApiIntegrationSettings.SubscriptionIdEast;
                var subscriptionIdSouth = ApiIntegrationSettings.SubscriptionIdSouth;

                var response = RestPoster.Post<TmpHealthShareGetConsultsRequest, TmpHealthShareGetConsultsResponse>("HealthShare Get Consults", baseUrl, uri, healthshareRequest, resource, appId, secret,
                    authority, tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);

                Logger.WriteToFile($"Get Consults Response: {Serialization.DataContractSerialize<TmpHealthShareGetConsultsResponse>(response)}");
                Logger.WriteToFile($"****Provider Consult Count {response.ProviderConsults.Count}");
                Logger.WriteToFile($"****Patient Consult Count {response.PatientConsults.Count}");
                Logger.WriteToFile($"****Provider RTC Count {response.ProviderReturnToClinicOrders.Count}");
                Logger.WriteToFile($"****Patient RTC Count {response.PatientReturnToClinicOrders.Count}");

                Logger.WriteToFile("Finished send/receive HealthShareGetConsults message.");
                response.VimtLagMs = lag;
                Logger.WriteToFile("Mapping the HealthShareGetConsults response");
                MapHealthShareResponseToCrm(response, patientStationNumber, providerStationCode, patientSiteId, isSft);
                Logger.WriteToFile("Response received: " + response.VimtResponse);

                var distinct = ConsultsAndRtcs.Distinct(new ConsultsComparer()).Distinct(new RtcComparer()).ToList();
                Logger.WriteToFile($"****Entity Count after Distinct is {distinct.Count}");
                                
                var ec = new EntityCollection(distinct)
                {
                    EntityName = crme_person.EntityLogicalName
                };
                PluginExecutionContext.OutputParameters["BusinessEntityCollection"] = ec;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        /// <summary>
        /// Extract the first 3 digits of the station from the station number string provided
        /// </summary>
        /// <param name="stationString">The Station Number String</param>
        /// <returns>The Station number in the form of Integer</returns>
        private static int ExtractStationNumber(string stationString)
        {
            var stationNumber = 0;

            if (string.IsNullOrWhiteSpace(stationString)) return stationNumber;

            stationString = stationString.Trim();
            var threeChars = stationString.Length < 3 ? stationString : stationString.Substring(0, 3);
            int.TryParse(threeChars, out stationNumber);

            return stationNumber;
        }

        /// <summary>
        /// Map the Health Share Get Consults Response To Crm
        /// </summary>
        /// <param name="response">The Health Share Get Consults Response</param>
        /// <param name="patientLoginStationNumber">The Patient Login Station Number</param>
        /// <param name="providerLoginStationNumber">The Provider Login StationNumber</param>
        /// <param name="patSiteId">The Patient Site Guid</param>
        /// <param name="isSft">is Store Forward</param>
        /// <returns>The list of person records</returns>
        private void MapHealthShareResponseToCrm(TmpHealthShareGetConsultsResponse response, string patientLoginStationNumber, string providerLoginStationNumber, string patSiteId, bool isSft)
        {
            Logger.WriteToFile($"MapHealthShareResponseToCrm Started. patientLoginStationNumber: {patientLoginStationNumber}, providerLoginStationNumber:{providerLoginStationNumber}, isSft: {isSft}");

            if (response == null) throw new InvalidPluginExecutionException("Get Consults Response is null");

            var person = new Entity { Id = Guid.NewGuid(), LogicalName = crme_person.EntityLogicalName };
            person["cvt_consultien"] = VistaPluginHelpers.GetDuzFromStationCode(PluginExecutionContext.UserId, Logger, OrganizationService, patientLoginStationNumber);
            person["cvt_consulttext"] = VistaPluginHelpers.GetDuzFromStationCode(PluginExecutionContext.UserId, Logger, OrganizationService, providerLoginStationNumber);
            person["crme_url"] = "neither";
            person["cvt_consulttimestamp"] = string.Empty;
            person["cvt_clinicallyindicateddate"] = string.Empty;
            person["cvt_consulttitle"] = string.Empty;
            person["cvt_consulttype"] = false;
            person["cvt_rtcid"] = string.Empty;

            ConsultsAndRtcs = new List<Entity> { person };

            var patStationCode = string.Empty;
            var proStationCode = providerLoginStationNumber;

            using (var srv = new Xrm(OrganizationService))
            {
                var patSite = !string.IsNullOrEmpty(patSiteId) ? srv.mcs_siteSet.FirstOrDefault(s => s.Id == new Guid(patSiteId)) : null;

                if (patSite?.mcs_FacilityId != null)
                {
                    var facility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == patSite.mcs_FacilityId.Id);
                    patStationCode = facility?.mcs_StationNumber ?? string.Empty;
                }
                else Logger.WriteToFile("Unable to get site for patient side.");
            }

            var sameFacilityCode = patStationCode == proStationCode;
            try
            {
                if ((response.PatientConsults != null && response.PatientConsults.Any()) || (response.PatientReturnToClinicOrders != null && response.PatientReturnToClinicOrders.Any()))
                {
                    var label = "patient";
                    if (sameFacilityCode && !isSft) label = "provider";

                    var patPersons = MapHealthShareConsultsResponse(response, response.PatientConsults.Distinct(new TmpConsultComparer()).ToList(), label);
                    AddResponseToDistinctPersonList(patPersons);
                    var patPersonsRtc = MapReturnToClinicOrdersResponse(response, response.PatientReturnToClinicOrders.Distinct(new TmpReturnToClinicOrderComparer()).ToList(), label);
                    AddResponseToDistinctPersonList(patPersonsRtc);
                }

                if ((response.ProviderConsults != null && response.ProviderConsults.Any()) || (response.ProviderReturnToClinicOrders != null && response.ProviderReturnToClinicOrders.Any()))
                {
                    var proPersons = MapHealthShareConsultsResponse(response, response.ProviderConsults.Distinct(new TmpConsultComparer()).ToList(), "provider");
                    AddResponseToDistinctPersonList(proPersons);
                    var proPersonsRtc = MapReturnToClinicOrdersResponse(response, response.ProviderReturnToClinicOrders.Distinct(new TmpReturnToClinicOrderComparer()).ToList(), "provider");
                    AddResponseToDistinctPersonList(proPersonsRtc);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"{ex} {ex.Message} {ex.StackTrace} {ex.InnerException}");
            }

            Logger.WriteToFile("MapHealthShareResponseToCrm Ended");
        }

        private void AddResponseToDistinctPersonList(IEnumerable<Entity> patPersons)
        {
            foreach (var person in patPersons)
            {
                ConsultsAndRtcs.Add(person);
            }
        }

        /// <summary>
        /// Map the HealthShare Consults Response
        /// </summary>
        /// <param name="response">The Health Share Get Consults response</param>
        /// <param name="consults">The consults from the response</param>
        /// <param name="side">Patient/Provider Side</param>
        /// <returns>List of People.</returns>
        private IEnumerable<Entity> MapHealthShareConsultsResponse(TmpHealthShareGetConsultsResponse response, List<TmpConsult> consults, string side)
        {
            Logger.WriteToFile($"MapHealthShareConsultsResponse Started. Side {side}");

            if (consults != null && consults.Any())
            {
                if (response.ExceptionOccured)
                {
                    Logger.WriteToFile($"Consults response.ExceptionMessage - {response.ExceptionMessage}");
                    yield return CreateFailedCrmePersonObject(response.ExceptionMessage, true);
                }
                else
                {
                    foreach (var consult in consults)
                    {
                        if (consult == null) continue;

                        var person = new Entity
                        {
                            Id = new Guid(),
                            LogicalName = crme_person.EntityLogicalName
                        };

                        person["cvt_consultien"] = consult.ConsultId;
                        person["cvt_consultstatus"] = GetStatusName(consult.ConsultStatus);
                        person["cvt_consulttext"] = string.Empty;
                        person["cvt_consulttimestamp"] = consult.ConsultRequestDateTime;
                        person["cvt_clinicallyindicateddate"] = consult.ClinicallyIndicatedDate;
                        person["cvt_consulttitle"] = consult.ConsultTitle;
                        person["cvt_receivingsite"] = consult.ReceivingSiteConsultId; //either blank or "10000,100000"                            
                        person["crme_url"] = side; //arbitrarily using this variable to temporarily send the pat/pro string across to the js
                        person["cvt_consulttype"] = true;
                        person["cvt_rtcid"] = string.Empty;

                        yield return person;
                    }
                }
            }

            Logger.WriteToFile("MapHealthShareConsultsResponse Ended");
        }

        /// <summary>
        /// Map Return to Clinic Orders.
        /// </summary>
        /// <param name="response">VIMT response.</param>
        /// <param name="returnToClinicOrders">Return to Clinic Orders.</param>
        /// <param name="side">Patient or Provider.</param>
        /// <returns>List of People.</returns>
        private IEnumerable<Entity> MapReturnToClinicOrdersResponse(TmpHealthShareGetConsultsResponse response, List<TmpReturnToClinicOrder> returnToClinicOrders, string side)
        {
            Logger.WriteToFile($"MapReturnToClinicOrdersResponse Started. Side {side}");

            if (returnToClinicOrders != null && returnToClinicOrders.Any())
            {
                if (response.ExceptionOccured)
                {
                    Logger.WriteToFile($"Return to Clinic Orders response.ExceptionMessage - {response.ExceptionMessage}");
                    yield return CreateFailedCrmePersonObject(response.ExceptionMessage, false);
                }
                else
                {
                    foreach (var returnToClinicOrder in returnToClinicOrders)
                    {
                        if (returnToClinicOrder == null) continue;

                        var person = new Entity
                        {
                            Id = new Guid(),
                            LogicalName = crme_person.EntityLogicalName
                        };

                        person["cvt_rtcid"] = returnToClinicOrder.RtcId;
                        person["cvt_rtcrequestdatetime"] = returnToClinicOrder.RtcRequestDateTime;
                        person["cvt_clinicien"] = returnToClinicOrder.ToClinicIen;
                        person["cvt_clinicname"] = returnToClinicOrder.ClinicName;
                        person["cvt_clinicallyindicateddate"] = returnToClinicOrder.ClinicallyIndicatedDate;
                        person["cvt_stopcodes"] = returnToClinicOrder.StopCodes;
                        person["cvt_provider"] = returnToClinicOrder.Provider;
                        person["cvt_comments"] = returnToClinicOrder.Comments;
                        person["crme_url"] = side;
                        person["cvt_consulttype"] = false;
                        person["cvt_consulttitle"] = string.Empty;
                        person["cvt_consultien"] = string.Empty;
                        person["cvt_consulttimestamp"] = string.Empty;

                        var multipleRtc = returnToClinicOrder.MultiRtc;

                        if (!string.IsNullOrEmpty(multipleRtc))
                        {
                            var parts = multipleRtc.Split(',');
                            if (parts != null && parts.Length > 0)
                            {
                                person["cvt_numberofappointments"] = parts[0];
                                person["cvt_interval"] = parts.Length >= 2 ? parts[1] : string.Empty;
                                person["cvt_rtcparent"] = parts.Length == 3 ? parts[2] : string.Empty;
                            }
                        }

                        yield return person;
                    }
                }
            }

            Logger.WriteToFile("MapReturnToClinicOrdersResponse Ended");
        }

        /// <summary>
        /// Replace/Substitute the Status Acronym with the Status Display Name
        /// </summary>
        /// <param name="consultStatus"></param>
        /// <returns></returns>
        private static string GetStatusName(string consultStatus)
        {
            string statusString;
            switch (consultStatus.Trim().ToLower())
            {
                case "dc":
                    statusString = "Discontinued";
                    break;
                case "c":
                    statusString = "Complete";
                    break;
                case "h":
                    statusString = "Hold";
                    break;
                case "p":
                    statusString = "Pending";
                    break;
                case "a":
                    statusString = "Active";
                    break;
                case "e":
                    statusString = "Expired";
                    break;
                case "s":
                    statusString = "Scheduled";
                    break;
                case "pr":
                    statusString = "Partial Results";
                    break;
                case "dly":
                    statusString = "Delayed";
                    break;
                case "u":
                    statusString = "Unreleased";
                    break;
                case "dce":
                    statusString = "Discontinued/Edit";
                    break;
                case "x":
                    statusString = "Cancelled";
                    break;
                case "l":
                    statusString = "Lapsed";
                    break;
                case "rn":
                    statusString = "Renewed";
                    break;
                default:
                    statusString = consultStatus.Contains("?") ? "Flagged" : "No Status";
                    break;
            }
            return statusString;
        }

        private static Entity CreateFailedCrmePersonObject(string message, bool isConsult)
        {
            var person = new Entity
            {
                Id = new Guid(),
                LogicalName = crme_person.EntityLogicalName
            };

            person["crme_url"] = "fail"; //flag read by javascript in html page to look for a failure
            person["cvt_consulttitle"] = message;
            person["cvt_consulttype"] = isConsult;

            return person;
        }

        /// <summary>
        /// Retrieve the Station Number for the Site Id Provided
        /// </summary>
        /// <param name="siteId">The Site Guid</param>
        /// <param name="logger">The logger object for logging purposes</param>
        /// <returns>The Site's Station Number</returns>
        public string GetStationNumberFromSiteId(string siteId, MCSUtilities2011.MCSLogger logger)
        {
            logger.WriteToFile("Starting GetStationNumberFromSiteId");

            var output = string.Empty;
            Guid siteGuid;

            if (siteId != string.Empty) siteGuid = Guid.Parse(siteId);
            else return output;

            using (var context = new Xrm(OrganizationService))
            {
                var siteRecord = context.mcs_siteSet.FirstOrDefault(s => s.Id == siteGuid);
                if (siteRecord != null)
                {
                    logger.WriteToFile("Found TMP Site");
                    var obj = siteRecord;

                    if (obj.mcs_FacilityId != null)
                    {
                        var facilityRecord = context.mcs_facilitySet.FirstOrDefault(f => f.Id == obj.mcs_FacilityId.Id);
                        if (facilityRecord != null)
                        {
                            logger.WriteToFile("Found Facility");
                            output = facilityRecord.mcs_StationNumber;
                        }
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Get the Station Number from the FacilityId
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public string GetStationNumberFromFacilityId(string facilityId, MCSUtilities2011.MCSLogger logger)
        {
            logger.WriteToFile("Starting GetStationNumberFromFacilityId");

            var output = string.Empty;
            Guid facilityGuid;

            if (facilityId != string.Empty) facilityGuid = Guid.Parse(facilityId);
            else return output;

            using (var context = new Xrm(OrganizationService))
            {
                var facilityRecord = context.mcs_facilitySet.FirstOrDefault(f => f.Id == facilityGuid);
                if (facilityRecord != null)
                {
                    logger.WriteToFile("Found Facility");
                    output = facilityRecord.mcs_StationNumber;
                }
            }
            return output;
        }
    }

    internal class ConsultsComparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y)
        {
            return x["cvt_consultien"].Equals(y["cvt_consultien"]) && (string)x["cvt_consultien"] != string.Empty && (string)x["cvt_consultien"] != string.Empty;
        }

        public int GetHashCode(Entity obj)
        {
            return obj["cvt_consultien"].GetHashCode();
        }
    }

    internal class RtcComparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y)
        {
            return x["cvt_rtcid"].Equals(y["cvt_rtcid"]) && (string)x["cvt_rtcid"] != string.Empty && (string)x["cvt_rtcid"] != string.Empty;
        }

        public int GetHashCode(Entity obj)
        {
            return obj["cvt_rtcid"].GetHashCode();
        }
    }

    internal class TmpConsultComparer : IEqualityComparer<TmpConsult>
    {        
        public bool Equals(TmpConsult x, TmpConsult y)
        {
            return x.ConsultId.Equals(y.ConsultId);
        }

        public int GetHashCode(TmpConsult obj)
        {
            return obj.ConsultId.GetHashCode();
        }
    }

    internal class TmpReturnToClinicOrderComparer : IEqualityComparer<TmpReturnToClinicOrder>
    {
        public bool Equals(TmpReturnToClinicOrder x, TmpReturnToClinicOrder y)
        {
            return x.RtcId.Equals(y.RtcId);
        }

        public int GetHashCode(TmpReturnToClinicOrder obj)
        {
            return obj.RtcId.GetHashCode();
        }
    }
}