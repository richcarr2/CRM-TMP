using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;

namespace VA.TMP.CRM
{
    public class McsResourceMergeAction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService OrganizationService = serviceFactory.CreateOrganizationService(PluginExecutionContext.UserId);

            try
            {
                MergeRecords(PluginExecutionContext, OrganizationService, tracingService);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in MergeActionPlugin.", ex);
            }

            catch (Exception ex)
            {
                tracingService.Trace("An error occurred in MergeActionPlugin: {0}", ex.ToString());
                throw;
            }
        }

        private void MergeRecords(IPluginExecutionContext PluginExecutionContext, IOrganizationService OrganizationService, ITracingService tracingService)
        {
            try
            {
                #region main logic

                string entityName = Convert.ToString(PluginExecutionContext.InputParameters["entityName"]);
                Guid winnerGUID = Guid.Parse(Convert.ToString(PluginExecutionContext.InputParameters["winner"]));
                Guid looserGUID = Guid.Parse(Convert.ToString(PluginExecutionContext.InputParameters["looser"]));
                string fieldsToUpdate = Convert.ToString(PluginExecutionContext.InputParameters["fieldsToUpdate"]);

                tracingService.Trace($"entityName: {entityName}");
                tracingService.Trace($"winnerGUID: {winnerGUID}");
                tracingService.Trace($"looserGUID: {looserGUID}");
                tracingService.Trace($"fieldsToUpdate: {fieldsToUpdate}");

                //update winner fields with looser fields
                if (fieldsToUpdate.Length > 0)
                {
                    tracingService.Trace($"fieldsToUpdate length: {fieldsToUpdate.Length}");

                    string[] fieldsToUpdateArray = fieldsToUpdate.Split(',');

                    //if fields are selected from looser
                    if (fieldsToUpdateArray.Count() > 0)
                    {
                        tracingService.Trace($"fieldsToUpdateArray length: {fieldsToUpdateArray.Count()}");

                        Entity looser = OrganizationService.Retrieve(entityName, looserGUID, new ColumnSet(fieldsToUpdateArray));

                        //if we get fields other than the mcs_resourceid
                        if (looser.Attributes.Count > 1)
                        {
                            Entity winnerUpdate = new Entity(entityName);
                            winnerUpdate.Id = winnerGUID;

                            foreach (string att in fieldsToUpdateArray)
                            {
                                if (looser.Attributes.Contains(att))
                                {
                                    winnerUpdate[att] = looser[att];
                                }
                            }

                            tracingService.Trace("Before winner update");
                            OrganizationService.Update(winnerUpdate);
                            tracingService.Trace("After winner update");
                        }
                    }
                }

                tracingService.Trace("Before Equipment Records update");
                UpdateEquipmentRecords(looserGUID, winnerGUID, entityName, OrganizationService, tracingService);
                tracingService.Trace("After Equipment Records update");
                tracingService.Trace("Before Scheduling Resources update");
                UpdateParticipatingSiteResources(looserGUID, winnerGUID, entityName, OrganizationService, tracingService);
                tracingService.Trace("After Scheduling Resources update");
                tracingService.Trace("Before Activity Parties update");
                UpdateActivityPartyRelatedRecords(looserGUID, winnerGUID, entityName, OrganizationService, tracingService);
                tracingService.Trace("After Activity Parties update");
                tracingService.Trace("Before Email Records update");
                UpdateEmailRecords(looserGUID, winnerGUID, entityName, OrganizationService, tracingService);
                tracingService.Trace("After Email Records update");

                //update looser with masterid 
                Entity looserUpdate = new Entity(entityName);
                looserUpdate.Id = looserGUID;

                looserUpdate["tmp_masterid"] = new EntityReference(entityName, winnerGUID);
                looserUpdate["statecode"] = new OptionSetValue(1);
                looserUpdate["statuscode"] = new OptionSetValue(2);

                tracingService.Trace("Before looser update");
                OrganizationService.Update(looserUpdate);
                tracingService.Trace("After looser update");

                PluginExecutionContext.OutputParameters["Success"] = true;
                #endregion
            }

            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateEquipmentRecords(Guid losingResourceId, Guid winningResourceId, string entityName, IOrganizationService organizationService, ITracingService tracingService)
        {
            try
            {
                #region main logic
                var fetchXml = "<fetch>" +
                    "<entity name=\"equipment\">" +
                    "<attribute name=\"mcs_relatedresource\" />" +
                    "<link-entity name=\"mcs_resource\" from=\"mcs_resourceid\" to=\"mcs_relatedresource\">" +
                    "<filter>" +
                    "<condition attribute=\"mcs_resourceid\" operator=\"eq\" value=\"" + losingResourceId + "\" />" +
                    "</filter>" +
                    "</link-entity >" +
                    "</entity>" +
                    "</fetch>";

                tracingService.Trace($"FetchXml: {fetchXml}");

                var resources = organizationService.RetrieveMultiple(new FetchExpression(fetchXml));

                if (resources != null && !resources.Entities.Count.Equals(0))
                {
                    tracingService.Trace($"Resources found: {resources.Entities.Count}");
                    resources.Entities.ToList().ForEach((resource) =>
                    {
                        resource["mcs_relatedresource"] = new EntityReference(entityName, winningResourceId);
                        organizationService.Update(resource);
                    });
                }
                #endregion
            }
            catch (Exception e)
            {
                tracingService.Trace($"Error occurred while trying to update Equipment for losing {entityName} with Id {losingResourceId}:\n{e}");
                throw;
            }
        }

        private void UpdateParticipatingSiteResources(Guid losingResourceId, Guid winningResourceId, string entityName, IOrganizationService organizationService, ITracingService tracingService)
        {
            try
            {
                #region main logic
                var fetchXml = "<fetch>" +
                    "<entity name=\"cvt_schedulingresource\">" +
                    "<attribute name=\"cvt_tmpresource\" />" +
                    "<link-entity name=\"mcs_resource\" from=\"mcs_resourceid\" to=\"cvt_tmpresource\">" +
                    "<filter>" +
                    "<condition attribute=\"mcs_resourceid\" operator=\"eq\" value=\"" + losingResourceId + "\" />" +
                    "</filter>" +
                    "</link-entity >" +
                    "</entity>" +
                    "</fetch>";

                tracingService.Trace($"FetchXml: {fetchXml}");

                var resources = organizationService.RetrieveMultiple(new FetchExpression(fetchXml));

                if (resources != null && !resources.Entities.Count.Equals(0))
                {
                    tracingService.Trace($"Resources found: {resources.Entities.Count}");
                    resources.Entities.ToList().ForEach((resource) =>
                    {
                        resource["cvt_tmpresource"] = new EntityReference(entityName, winningResourceId);
                        organizationService.Update(resource);
                    });
                }
                #endregion
            }
            catch (Exception e)
            {
                tracingService.Trace($"Error occurred while trying to update Scheduling Resource for losing {entityName} with Id {losingResourceId}:\n{e}");
                throw;
            }
        }

        private void UpdateActivityPartyRelatedRecords(Guid losingResourceId, Guid winningResourceId, string entityName, IOrganizationService organizationService, ITracingService tracingService)
        {
            try
            {
                #region main logic  
                var fetchXml = "<fetch>" +
                    "<entity name='activityparty'>" +
                    "<attribute name='partyid' />" +
                    "<filter>" +
                    "<condition attribute='partyid' operator='eq' value='" + losingResourceId + "' />" +
                    "</filter>" +
                    "</entity>" +
                    "</fetch>";

                tracingService.Trace($"FetchXml: {fetchXml}");

                var activityParty = organizationService.RetrieveMultiple(new FetchExpression(fetchXml));

                if (activityParty != null && !activityParty.Entities.Count.Equals(0))
                {
                    tracingService.Trace($"Resources found: {activityParty.Entities.Count}");
                    activityParty.Entities.ToList().ForEach((activityPartyEntity) =>
                    {
                        activityPartyEntity["partyid"] = new EntityReference(entityName, winningResourceId);
                        organizationService.Update(activityPartyEntity);
                    });
                }
                #endregion
            }
            catch (Exception e)
            {
                tracingService.Trace($"Error occurred while trying to update Activity Party for losing {entityName} with Id {losingResourceId}:\n{e}");
                throw;
            }
        }

        private void UpdateEmailRecords(Guid losingResourceId, Guid winningResourceId, string entityName, IOrganizationService organizationService, ITracingService tracingService)
        {
            try
            {
                #region main logic                  

                var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"+
                    "<entity name='email'>" +
                    //"<attribute name='subject' />" +
                    "<attribute name='regardingobjectid' />" +
                    "<order attribute='subject' descending='false' />" +
                    "<filter type='and'>" +
                    "<condition attribute='regardingobjectid' operator='eq' value= '" + losingResourceId + "' />" +
                    "</filter>" +
                    "</entity>" +
                    "</fetch>";

                tracingService.Trace($"FetchXml: {fetchXml}");

                var email = organizationService.RetrieveMultiple(new FetchExpression(fetchXml));

                if (email != null && !email.Entities.Count.Equals(0))
                {
                    tracingService.Trace($"Resources found: {email.Entities.Count}");
                    email.Entities.ToList().ForEach((emailEntity) =>
                    {
                        emailEntity["regardingobjectid"] = new EntityReference(entityName, winningResourceId);
                        organizationService.Update(emailEntity);
                    });
                }
                #endregion
            }
            catch (Exception e)
            {
                tracingService.Trace($"Error occurred while trying to update email for losing {entityName} with Id {losingResourceId}:\n{e}");
                throw;
            }
        }
    }
}