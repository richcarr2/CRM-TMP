using System;
using System.Linq;
using System.ServiceModel;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.CrmePerson
{
    /// <summary>
    ///  CRM Plugin Runner class to handle Person Search and Get Identifiers.
    /// </summary>
    public class CrmePersonRetrieveMultiplePostStageRunner : PluginRunner
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public CrmePersonRetrieveMultiplePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "crme_person";

        private ApiIntegrationSettings ApiIntegrationSettingsPersonSearch { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsSelectedPerson { get; set; }

        /// <summary>
        /// Executes the plugin runner.
        /// </summary>
        public override void Execute()
        {
            try
            {
                Logger.setDebug = true;
                // Make sure that a query has query parameters
                if (!PluginExecutionContext.InputParameters.Contains("Query") || !(PluginExecutionContext.InputParameters["Query"] is QueryExpression || PluginExecutionContext.InputParameters["Query"] is FetchExpression)) return;
                QueryExpression qe;

                var query = PluginExecutionContext.InputParameters["Query"];
                if (query.GetType() == typeof(FetchExpression))
                {
                    var fetch = (FetchExpression)query;
                    qe = IntegrationPluginHelpers.ConvertFetchExpressionToQueryExpression((FetchExpression)query, OrganizationService, Logger);
                    Logger.WriteDebugMessage("query was fetch expression");
                }
                else
                {
                    qe = (QueryExpression)PluginExecutionContext.InputParameters["Query"];
                    Logger.WriteDebugMessage("query is query expression");
                }


                int integrationType;
                PersonSearchPluginUtilities.TryGetConditionOptionSet(qe, "cvt_IntegrationType", out integrationType);
                Logger.WriteDebugMessage("integrationType:" + integrationType);

                if (integrationType != (int)crme_personcvt_IntegrationType.PatientSearch)
                    return;

                string mviSearchType;

                if (!PersonSearchPluginUtilities.TryGetFilterValue(qe, "crme_SearchType", out mviSearchType)) return;
                Logger.WriteDebugMessage("mviSearchType:" + mviSearchType);
                ((EntityCollection)PluginExecutionContext.OutputParameters["BusinessEntityCollection"]).Entities.Clear();

                Logger.WriteGranularTimingMessage("Starting retrieval of Person(s)");

                string serverName;
                PersonSearchPluginUtilities.TryGetFilterValue(qe, "crme_url", out serverName);
                Logger.WriteGranularTimingMessage("serverName:" + serverName);

                mcs_integrationsetting fakeType;
                using (var context = new Xrm(OrganizationService))
                {
                    ApiIntegrationSettingsPersonSearch = IntegrationPluginHelpers.GetApiSettings(context, "MviPersonSearchUri");
                    ApiIntegrationSettingsSelectedPerson = IntegrationPluginHelpers.GetApiSettings(context, "MviSelectedPersonUri");
                }

                var fakeTypeValue = string.Empty;


                switch (mviSearchType)
                {
                    case "SearchByIdentifier":
                    case "SearchByFilter":
                        using (var context = new Xrm(OrganizationService))
                        {
                            Logger.WriteDebugMessage("about to get PersonSearchFakeResponseType");

                            fakeType = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "PersonSearchFakeResponseType");
                        }
                        fakeTypeValue = fakeType == null ? "" : fakeType.mcs_value;
                        Logger.WriteDebugMessage("fakeTypeValue:" + fakeTypeValue);
                        var request = PersonSearchPluginUtilities.GetPersonSearchRequest(qe, PluginExecutionContext, OrganizationService, Logger);
                      

                        request.PersonSearchFakeResponseType = fakeTypeValue;

                        var baseUrl = ApiIntegrationSettingsPersonSearch.BaseUrl;
                        var uri = ApiIntegrationSettingsPersonSearch.Uri;
                        var resource = ApiIntegrationSettingsPersonSearch.Resource;
                        var appId = ApiIntegrationSettingsPersonSearch.AppId;
                        var secret = ApiIntegrationSettingsPersonSearch.Secret;
                        var authority = ApiIntegrationSettingsPersonSearch.Authority;
                        var tenantId = ApiIntegrationSettingsPersonSearch.TenantId;
                        var subscriptionId = ApiIntegrationSettingsPersonSearch.SubscriptionId;
                        var isProdApi = ApiIntegrationSettingsPersonSearch.IsProdApi;
                        var subscriptionIdEast = ApiIntegrationSettingsPersonSearch.SubscriptionIdEast;
                        var subscriptionIdSouth = ApiIntegrationSettingsPersonSearch.SubscriptionIdSouth;

                        var response = RestPoster.Post<PersonSearchRequestMessage, PersonSearchResponseMessage>("Mvi Person Search", baseUrl, uri, request, resource, appId, secret, authority, tenantId,
                            subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                        response.VimtLagMs = lag;

                        Logger.WriteDebugMessage("VIMT PersonSearch Response Received, processing response.");
                        CreateIntegrationResult(response, Logger);
                        var psResponse = PersonSearchPluginUtilities.GetPersonSearchResponse(request, response.RetrieveOrSearchPersonResponse, OrganizationService);
                        var ec = new EntityCollection(psResponse)
                        {
                            EntityName = crme_person.EntityLogicalName
                        };
                        PluginExecutionContext.OutputParameters["BusinessEntityCollection"] = ec;
                        break;
                    case "SelectedPersonSearch":
                        using (var context = new Xrm(OrganizationService))
                        {
                            Logger.WriteDebugMessage("about to get PersonSearchFakeResponseType");

                            fakeType = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "SelectedPersonFakeResponseType");
                        }
                        fakeTypeValue = fakeType != null ? fakeType.mcs_value : string.Empty;

                        Logger.WriteDebugMessage("fakeTypeValue:" + fakeTypeValue);

                        var request2 = PersonSearchPluginUtilities.GetSelectedPersonRequest(qe, PluginExecutionContext, OrganizationService, Logger);
                        request2.ServerName = serverName;
                        request2.SelectedPersonFakeResponseType = fakeTypeValue;

                        var baseUrl2 = ApiIntegrationSettingsSelectedPerson.BaseUrl;
                        var uri2 = ApiIntegrationSettingsSelectedPerson.Uri;
                        var resource2 = ApiIntegrationSettingsSelectedPerson.Resource;
                        var appId2 = ApiIntegrationSettingsSelectedPerson.AppId;
                        var secret2 = ApiIntegrationSettingsSelectedPerson.Secret;
                        var authority2 = ApiIntegrationSettingsSelectedPerson.Authority;
                        var tenantId2 = ApiIntegrationSettingsSelectedPerson.TenantId;
                        var subscriptionId2 = ApiIntegrationSettingsSelectedPerson.SubscriptionId;
                        var isProdApi2 = ApiIntegrationSettingsSelectedPerson.IsProdApi;
                        var subscriptionIdEast2 = ApiIntegrationSettingsSelectedPerson.SubscriptionIdEast;
                        var subscriptionIdSouth2 = ApiIntegrationSettingsSelectedPerson.SubscriptionIdSouth;

                        var vimtResponse = RestPoster.Post<GetPersonIdentifiersRequestMessage, GetPersonIdentifiersResponseMessage>("VMR VOD", baseUrl2, uri2, request2, resource2, appId2, secret2,
                            authority2, tenantId2, subscriptionId2, isProdApi2, subscriptionIdEast2, subscriptionIdSouth2, out int lag2);
                        vimtResponse.VimtLagMs = lag2;

                        Logger.WriteDebugMessage("VIMT GetIds Response Received, processing Response");
                        CreateIntegrationResultOnGetPersonIdentifiers(vimtResponse, Logger);
                        var selectedPersons = PersonSearchPluginUtilities.GetSelectedPersonIds(vimtResponse, serverName);
                        var ec2 = new EntityCollection(selectedPersons)
                        {
                            EntityName = crme_person.EntityLogicalName
                        };
                        PluginExecutionContext.OutputParameters["BusinessEntityCollection"] = ec2;
                        break;
                    default:
                        throw new InvalidPluginExecutionException("Unable to Determine MVI Search Type");
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(ex.Message);
                throw new InvalidPluginExecutionException(string.Format("ERROR in CrmePersonRetrieveMultiplePostStageRunner: {0}", IntegrationPluginHelpers.BuildErrorMessage(ex)));
            }
            catch (InvalidPluginExecutionException ex)
            {
                Logger.WriteToFile(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
                throw;
            }
        }

        private void CreateIntegrationResult(PersonSearchResponseMessage response, MCSLogger logger)
        {
            logger.WriteDebugMessage("CreateIntegrationResultOnPersonSearch Initiated");
            IntegrationPluginHelpers.CreateIntegrationResultOnPersonSearch("Person Search - Traits", response.ExceptionOccured, response.ExceptionMessage, response.VimtRequest, response.SerializedInstance, response.VimtResponse, typeof(PersonSearchRequestMessage).FullName, typeof(PersonSearchResponseMessage).FullName, MessageRegistry.PersonSearchRequestMessage, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, logger);
            logger.WriteDebugMessage("CreateIntegrationResultOnPersonSearch Call Complete");
        }

        private void CreateIntegrationResultOnGetPersonIdentifiers(GetPersonIdentifiersResponseMessage response, MCSLogger logger)
        {
            logger.WriteDebugMessage("CreateIntegrationResultOnPersonSearchGetPersonIdentifiers Initiated");
            IntegrationPluginHelpers.CreateIntegrationResultOnGetPersonIdentifiers("Person Search - GetPersonIdentifiers", response.ExceptionOccured, response.ExceptionMessage, response.VimtRequest, response.SerializedInstance, response.VimtResponse, typeof(GetPersonIdentifiersResponseMessage).FullName, typeof(GetPersonIdentifiersResponseMessage).FullName, MessageRegistry.GetPersonIdentifiersResponseMessage, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, logger);
            logger.WriteDebugMessage("CreateIntegrationResultOnPersonSearchGetPersonIdentifiers Call Complete");
        }
    }
}
