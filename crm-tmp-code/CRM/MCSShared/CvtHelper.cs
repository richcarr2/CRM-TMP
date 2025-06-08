using MCS.ApplicationInsights;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace MCSShared
{
    public static partial class CvtHelper
    {
        #region Helper Functions

        /// <summary>
        /// This accepts 1 or 2 strings and outputs them as a clickable link
        /// </summary>
        /// <param name="input">the string to be converted to a URL</param>
        /// <param name="clickDisplay">the display text that is clickable - defaults to be the same as the actual URL unless otherwise specified</param>
        /// <returns>formatted url</returns>
        public static string buildHTMLUrl(string input, string clickDisplay = "")
        {
            if (input != null && input != "")
            {
                var http = (input.Length < 4 || input.Substring(0, 4).ToLower() != "http") ? "https://" + input : input;
                clickDisplay = (clickDisplay == string.Empty) ? http : clickDisplay;
                return "<a href=" + http + "><font size='5' color='#0070c0' face='Tahoma'>" + clickDisplay + "</font></a>";
            }
            return "No Link Available.";
        }

        public static string buildHTMLUrlAlt(string input, string clickDisplay = "")
        {
            var http = (input.Length < 4 || input.Substring(0, 4).ToLower() != "http") ? "https://" + input : input;
            clickDisplay = (clickDisplay == string.Empty) ? http : clickDisplay;
            return "<a href=" + http + "><font size='4' color='#0070c0' face='Tahoma'>" + clickDisplay + "</font></a>";
        }
        /// <summary>
        /// Create a note related to this object
        /// </summary>
        /// <param name="Regarding"></param>
        /// <param name="Message"></param>
        /// <param name="Title"></param>
        /// <param name="OrgService"></param>
        public static void CreateNote(EntityReference Regarding, string Message, string Title, IOrganizationService OrgService)
        {
            var newNote = new Entity("annotation");
            newNote["subject"] = Title;
            newNote["notetext"] = Message;
            newNote["objectid"] = Regarding;
            OrgService.Create(newNote);
        }

        /// <summary>
        /// gets the URL for the environment from the settings table's cvt_URL field
        /// </summary>
        /// <param name="OrganizationService">OrganizationService for local environment</param>
        /// <returns>returns url</returns>
        public static string getServerURL(IOrganizationService OrganizationService)
        {
            var url = "";
            using (var srv = new Xrm(OrganizationService))
            {
                var setting = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings");
                if (setting != null && !string.IsNullOrEmpty(setting.cvt_URL))
                {
                    url = setting.cvt_URL;
                    url = url.EndsWith("/") ? url : url + "/";
                    url = url.StartsWith("http") ? url : "https://" + url;
                }
                else
                    throw new InvalidPluginExecutionException("URL setting is not configured, please contact the Help Desk and request the URL be set up");
            }
            return url;
        }

        /// <summary>
        /// Retrieves the ETC
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static int GetEntityTypeCode(IOrganizationService service, Entity entity)
        {
            return GetEntityTypeCode(service, entity.LogicalName);
        }

        /// <summary>
        /// Get Entity Type Code because it can change per environment
        /// </summary>
        /// <param name="service"></param>
        /// <param name="entityname"></param>
        /// <returns></returns>
        public static int GetEntityTypeCode(IOrganizationService service, string entityname)
        {
            RetrieveEntityRequest request = new RetrieveEntityRequest
            {
                LogicalName = entityname
            };
            RetrieveEntityResponse response = (RetrieveEntityResponse)service.Execute(request);
            int objectTypecode = response.EntityMetadata.ObjectTypeCode.Value;
            return objectTypecode;
        }

        /// <summary>
        /// This method accepts the resource (or user) id from the patient or provider site resource, and then returns the user or equipment record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Entity getPartyParticipantFromResource(Guid id, Xrm context)
        {
            var participant = new Entity();
            participant = context.EquipmentSet.FirstOrDefault(e => e.mcs_relatedresource.Id == id);
            if (participant == null)
                participant = context.SystemUserSet.FirstOrDefault(u => u.Id == id);
            return participant;
        }

        /// <summary>
        /// Get owner of workflow
        /// </summary>
        /// <param name="WorkflowName"></param>
        /// <param name="OrganizationService"></param>
        /// <returns></returns>
        public static List<ActivityParty> GetWorkflowOwner(string WorkflowName, IOrganizationService OrganizationService)
        {
            //Get the workflow definition's owner
            using (var context = new Xrm(OrganizationService))
            {
                var workflow = ((from w in context.WorkflowSet
                                 where (w.Type.Value == 1 &&
                                 w.Name == WorkflowName)
                                 select new
                                 {
                                     w.OwnerId
                                 }).FirstOrDefault());

                //var workflow = context.WorkflowSet.FirstOrDefault(c =>
                //    c.Type.Value == 1 && c.Name == WorkflowName);

                if (workflow != null && workflow.OwnerId != null)
                {
                    var owner = new ActivityParty()
                    {
                        PartyId = workflow.OwnerId
                    };
                    return CvtHelper.SetPartyList(owner);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Builds an error message from an exception recursively, excluding the first level message
        /// </summary>
        /// <param name="ex">Exception.</param>
        /// <returns>Exception message.</returns>
        public static string BuildErrorMessage(Exception ex)
        {
            var errorMessage = string.Empty;

            if (ex.InnerException == null) return errorMessage;

            errorMessage += string.Format("\n\n{0}\n", ex.InnerException.Message);
            errorMessage += BuildErrorMessage(ex.InnerException);

            return errorMessage;
        }

        /// <summary>
        /// Concatenates the error message including the first level message
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static string BuildExceptionMessage(Exception ex, string errorMessage = "")
        {
            var message = string.Format("{0} \n{1}\n", errorMessage, ex.Message);
            if (ex.InnerException != null)
                BuildExceptionMessage(ex.InnerException, message);
            return message;
        }

        /// <summary>
        /// Conditionally sets the field on the return Entity object if it has changed, otherwise returns the Entity object unchanged
        /// </summary>
        /// <param name="update">the object that is the result of the operation</param>
        /// <param name="existing">The source object to check if something has changed</param>
        /// <param name="newer">the proper target to match against source, and to be used if they aren't equal</param>
        /// <param name="patGroupFieldName">the name of the field on the patient resource group record</param>
        /// <param name="resourceFieldName">the name of the field on the tss resource record</param>
        /// <returns>the cvt_patientresourcegroup record either updated if it is out of date or the same as before if no update is needed</returns>
        /// <remarks>use this for updating entities so that only changes are recorded and persisted instead of brute forcing all changes</remarks>
        public static Entity UpdateField(Entity update, Entity existing, Entity newer, string targetFieldName, string sourceFieldName, bool overrideNulls = true)
        {
            if (!newer.Attributes.Contains(sourceFieldName))
            {
                if (overrideNulls && existing.Attributes.Contains(targetFieldName) && existing[targetFieldName] != null)
                    update[targetFieldName] = null;
                return update;
            }
            if (!existing.Attributes.Contains(targetFieldName))
            {
                update[targetFieldName] = newer[sourceFieldName];
                return update;
            }
            if ((existing[targetFieldName]).GetType().ToString() == "Microsoft.Xrm.Sdk.EntityReference")
            {
                if (newer[sourceFieldName].GetType().ToString() == "System.Guid")
                {
                    if (((EntityReference)(existing[targetFieldName])).Id != ((Guid)(newer[sourceFieldName])))
                        update[targetFieldName] = (new EntityReference(newer.LogicalName, (Guid)newer[sourceFieldName]));
                }
                else if (newer[sourceFieldName].GetType().ToString() == "Microsoft.Xrm.Sdk.EntityReference")
                {
                    if (((EntityReference)(existing[targetFieldName])).Id != ((EntityReference)(newer[sourceFieldName])).Id)
                        update[targetFieldName] = ((EntityReference)(newer[sourceFieldName]));
                }
            }
            else if ((existing[targetFieldName]).GetType().ToString() == "Microsoft.Xrm.Sdk.OptionSetValue")
            {
                if (((OptionSetValue)(existing[targetFieldName])).Value != ((OptionSetValue)(newer[sourceFieldName])).Value)
                    update[targetFieldName] = ((OptionSetValue)(newer[sourceFieldName]));
            }
            else
            {
                if (existing[targetFieldName].ToString().Trim() != newer[sourceFieldName].ToString().Trim())
                    update[targetFieldName] = newer[sourceFieldName];
            }
            return update;
        }

        /// <summary>
        /// retrieves the record and verifies that it is of the correct type
        /// </summary>
        /// <param name="PrimaryEntity"></param>
        /// <param name="EntityLogicalName"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <returns></returns>
        public static Entity ValidateReturnRecord(Entity PrimaryEntity, string EntityLogicalName, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.setMethod = "ValidateReturnRecord";
            if (PrimaryEntity.Id == null)
                throw new InvalidPluginExecutionException("Entity has no ID.");

            if (PrimaryEntity.LogicalName != EntityLogicalName)
                throw new InvalidPluginExecutionException("Entity is not Type: " + EntityLogicalName);

            var ThisRecord = OrganizationService.Retrieve(EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true));
            return ThisRecord;
        }
        #endregion

        #region MTSA/TSA/Service Related Functions
        //TODO - finish out commenting Parameters
        //TODO - check all ifs
        /// <summary>
        /// Deprecated: Update TSAs provider and vista clinic string fields and build the service if it is in status of Production.
        /// </summary>
        /// <remarks>3-25-2015: Major Refactoring to improve readability and consolidate code.  Errors currently exist, so this should fix them as well</remarks>
        public static void CreateUpdateService(Guid TSAId, MCSLogger Logger, IOrganizationService OrganizationService, MCSSettings McsSettings)
        {
            Logger.setMethod = "CvtHelper.CreateUpdateService";
            Logger.WriteDebugMessage("Not using TSAs for scheduling. Switched to Scheduling Package for scheduling.  Exiting out of this function: CreateUpdateService.");
            return;
            //using (var srv = new Xrm(OrganizationService))
            //{
            //    try
            //    {
            //        if (TSAId == Guid.Empty)
            //        {
            //            Logger.WriteDebugMessage("No TSA value, exiting.");
            //            return;
            //        }

            //        //Get current TSA
            //        var thisTSA = srv.mcs_servicesSet.FirstOrDefault(i => i.Id == TSAId);
            //        if (thisTSA == null)
            //            throw new InvalidPluginExecutionException("TSA to be updated could not be found.");

            //        Logger.WriteDebugMessage("Starting to build the Service components");

            //        //Declaration of Variables
            //        var builder = new System.Text.StringBuilder("");
            //        var builderProv = new System.Text.StringBuilder("");
            //        var builderProvAllRequired = new System.Text.StringBuilder("");
            //        var builderPat = new System.Text.StringBuilder("");
            //        var builderPatAllRequired = new System.Text.StringBuilder("");
            //        var builderGroupProvOnlybranch = new System.Text.StringBuilder("");
            //        var builderGroupProvOnlyAllRequiredBranch = new System.Text.StringBuilder("");
            //        string provSiteVCs = string.Empty;
            //        string providers = string.Empty;
            //        string patsiteUsers = string.Empty;
            //        string patSiteVCs = string.Empty;
            //        string patSites = string.Empty;
            //        var tsaType = "Clinic";
            //        Guid resourceSpecId;
            //        String patActivityParty = String.Empty;  //For auto setting the Find Available Times to the patient and provider branch

            //        #region PROVIDER SITE RESOURCES
            //        //Retrieve all the provider site resources
            //        Logger.WriteDebugMessage("Start sorting through Provider Site Resources");
            //        var getProvResources = from provGroups in srv.cvt_providerresourcegroupSet
            //                               where provGroups.cvt_RelatedTSAid.Id == TSAId
            //                               where provGroups.statecode == 0
            //                               select new
            //                               {
            //                                   provGroups.Id,
            //                                   provGroups.cvt_RelatedResourceId,
            //                                   provGroups.cvt_resourcespecguid,
            //                                   provGroups.cvt_TSAResourceType,
            //                                   provGroups.cvt_Type,
            //                                   provGroups.cvt_RelatedUserId,
            //                                   provGroups.cvt_RelatedResourceGroupid,
            //                                   provGroups.cvt_name
            //                               };

            //        Logger.WriteDebugMessage("# of Provider Site Resources associated: " + getProvResources.ToList().Count);
            //        //Loop through all of the Provider Site resources
            //        foreach (var provSiteResource in getProvResources)
            //        {
            //            Logger.WriteDebugMessage("Starting loop for " + provSiteResource.cvt_name);

            //            //Verify that the Resource is typed, not required, but should be filled in
            //            if (provSiteResource.cvt_Type != null)
            //            {
            //                //StrBuilder
            //                if (provSiteResource.cvt_resourcespecguid != null)
            //                {
            //                    Guid sysResId = Guid.Empty;
            //                    switch (provSiteResource.cvt_Type.Value)
            //                    {
            //                        case 917290000: //Paired
            //                            builderProvAllRequired.Append(AddResourceToConstraintGroup(provSiteResource.cvt_resourcespecguid));
            //                            var grResAR = srv.mcs_resourcegroupSet.FirstOrDefault(rg => rg.Id == provSiteResource.cvt_RelatedResourceGroupid.Id);
            //                            if (grResAR == null || grResAR.mcs_RelatedResourceGroupId == null)
            //                                throw new InvalidPluginExecutionException(String.Format("Provider Site Resource Group is invalid, please remove and re-add the Paired Resource group to this TSA: {0}.", provSiteResource.cvt_name));
            //                            sysResId = grResAR.mcs_RelatedResourceGroupId.Id; //What is this?
            //                            var nestedARcbg = AddResourceToConstraintGroup(sysResId.ToString());
            //                            var nestedARrs = BuildOut(thisTSA, new StringBuilder(nestedARcbg), OrganizationService, -1, 0, false);
            //                            builderGroupProvOnlyAllRequiredBranch.Append(AddResourceToConstraintGroup(nestedARrs.ToString()));
            //                            break;
            //                        default: //Others
            //                            builderProv.Append(AddResourceToConstraintGroup(provSiteResource.cvt_resourcespecguid));

            //                            //for Group Prov Only Branch
            //                            if (provSiteResource.cvt_TSAResourceType == null)
            //                            {
            //                                Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the Resource, Provider, or Resource Group to this TSA: {0}.", provSiteResource.cvt_name));
            //                                break;
            //                            }
            //                            switch (provSiteResource.cvt_TSAResourceType.Value)
            //                            {
            //                                case 1: //Single Resource = 1
            //                                    var res = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == provSiteResource.cvt_RelatedResourceId.Id);
            //                                    if (res == null || res.mcs_relatedResourceId == null)
            //                                        throw new InvalidPluginExecutionException(String.Format("Provider Site Single Resource is invalid, please remove and re-add the Resource to this TSA: {0}.", provSiteResource.cvt_name));
            //                                    sysResId = res.mcs_relatedResourceId.Id;
            //                                    break;
            //                                case 2://Single Provider = 2
            //                                    if (provSiteResource.cvt_RelatedUserId == null)
            //                                        throw new InvalidPluginExecutionException(String.Format("Provider Site Single Provider is invalid, please remove and re-add the Provider to this TSA: {0}.", provSiteResource.cvt_name));
            //                                    sysResId = provSiteResource.cvt_RelatedUserId.Id;
            //                                    break;
            //                                case 0: //Resource Group = 0
            //                                    var grRes = srv.mcs_resourcegroupSet.FirstOrDefault(rg => rg.Id == provSiteResource.cvt_RelatedResourceGroupid.Id);
            //                                    if (grRes == null || grRes.mcs_RelatedResourceGroupId == null)
            //                                        throw new InvalidPluginExecutionException(String.Format("Provider Site Group Resource is invalid, please remove and re-add the Group Resource to this TSA {0}.", provSiteResource.cvt_name));
            //                                    sysResId = grRes.mcs_RelatedResourceGroupId.Id;
            //                                    var GroupResourceSpec = AddResourceToConstraintGroup(sysResId.ToString());
            //                                    sysResId = BuildOut(thisTSA, new StringBuilder(GroupResourceSpec), OrganizationService, 1, 0, false);
            //                                    break;
            //                                default: //Unknown scenario
            //                                    Logger.WriteDebugMessage(String.Format("Provider Site Resource Record Resource Type is invalid for Resource {0}.", provSiteResource.cvt_name));
            //                                    break;
            //                            }

            //                            //Nest the Individual Group into a Choose 1
            //                            //Logger.WriteDebugMessage("About to Nest Individual Group into a Choose 1.");
            //                            builderGroupProvOnlybranch.Append(AddResourceToConstraintGroup(sysResId.ToString()));
            //                            break;
            //                    }
            //                }
            //                else
            //                {
            //                    Logger.WriteDebugMessage(String.Format("Resource Spec for resource - {0} - unable to be found, please recreate provider site resource.", provSiteResource.cvt_name));
            //                    //TODO - Consider throwing Exception
            //                }
            //                //Naming

            //                //Logger.WriteDebugMessage("cvt_Type SWITCH");
            //                switch (provSiteResource.cvt_Type.Value)
            //                {
            //                    case 251920000: //Vista Clinic

            //                        if (provSiteResource.cvt_TSAResourceType == null)
            //                        {
            //                            Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the VistA Clinic(s) to this TSA: {0}.", provSiteResource.cvt_name));
            //                            break;
            //                        }

            //                        if (provSiteResource.cvt_TSAResourceType.Value == 0) //Group of Vista Clinics
            //                        {
            //                            if (provSiteResource.cvt_RelatedResourceGroupid == null)
            //                            {
            //                                Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the VistA Clinic Group to this TSA: {0}.", provSiteResource.cvt_name));
            //                                break;
            //                            }
            //                            //Query for child names
            //                            var groupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == provSiteResource.cvt_RelatedResourceGroupid.Id && g.statecode == 0);
            //                            foreach (var child in groupResourceRecords)
            //                            {
            //                                if (child.mcs_RelatedResourceId != null)
            //                                    provSiteVCs += child.mcs_RelatedResourceId.Name.ToString() + " ; ";
            //                            }
            //                        }
            //                        else
            //                        {
            //                            if (provSiteResource.cvt_RelatedResourceId == null)
            //                            {
            //                                Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the VistA Clinic to this TSA: {0}.", provSiteResource.cvt_name));
            //                                break;
            //                            }
            //                            if (provSiteResource.cvt_RelatedResourceId != null)
            //                                provSiteVCs += provSiteResource.cvt_RelatedResourceId.Name.ToString() + " ; ";
            //                        }
            //                        break;
            //                    case 100000000: //Telepresenter/Imager
            //                    case 99999999: //Provider

            //                        if (provSiteResource.cvt_TSAResourceType == null)
            //                        {
            //                            Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the Provider(s) to this TSA: {0}.", provSiteResource.cvt_name));
            //                            break;
            //                        }

            //                        if (provSiteResource.cvt_TSAResourceType.Value == 0) //Group of Providers
            //                        {
            //                            if (provSiteResource.cvt_RelatedResourceGroupid == null)
            //                            {
            //                                Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the Provider Group to this TSA: {0}.", provSiteResource.cvt_name));
            //                                break;
            //                            }
            //                            //Query for child names
            //                            var groupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == provSiteResource.cvt_RelatedResourceGroupid.Id && g.statecode == 0);
            //                            foreach (var child in groupResourceRecords)
            //                            {
            //                                if ((child.mcs_RelatedUserId != null) && (providers == null || !providers.Contains(child.mcs_RelatedUserId.Name + " ; ")))
            //                                    providers += child.mcs_RelatedUserId.Name + " ; ";
            //                            }
            //                        }
            //                        else
            //                        {
            //                            if (provSiteResource.cvt_RelatedUserId == null)
            //                            {
            //                                Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the Provider to this TSA: {0}.", provSiteResource.cvt_name));
            //                                break;
            //                            }
            //                            if ((provSiteResource.cvt_RelatedUserId != null) && (providers == null || !providers.Contains(provSiteResource.cvt_RelatedUserId.Name + " ; ")))
            //                                providers += provSiteResource.cvt_RelatedUserId.Name + " ; ";
            //                        }
            //                        break;
            //                    case 917290000: //Paired
            //                        if (provSiteResource.cvt_RelatedResourceGroupid == null)
            //                        {
            //                            Logger.WriteDebugMessage(String.Format("Provider Site Paired Resources record is invalid, please remove and re-add the Paired Resources record to this TSA: {0}.", provSiteResource.cvt_name));
            //                            break;
            //                        }
            //                        //Query for child names
            //                        Logger.WriteDebugMessage("About to query for child names");
            //                        var childgroupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == provSiteResource.cvt_RelatedResourceGroupid.Id && g.statecode == 0);

            //                        Logger.WriteDebugMessage("Child record count: " + childgroupResourceRecords.ToList().Count);
            //                        foreach (var child in childgroupResourceRecords)
            //                        {
            //                            Logger.WriteDebugMessage("Child name: " + child.mcs_name + ". Guid: " + child.Id);
            //                            if (child.mcs_RelatedUserId != null)
            //                            {
            //                                var username = child.mcs_RelatedUserId.Name + " ; ";
            //                                if (providers.Contains(username))
            //                                {
            //                                    Logger.WriteDebugMessage("Not updating a duplicate name into the Provider string."); //Why is there a duplicate?
            //                                }
            //                                else
            //                                {
            //                                    Logger.WriteDebugMessage("Unique Provider Name, adding it to the Provider string.");
            //                                    providers += username;
            //                                }
            //                            }
            //                            else
            //                            {
            //                                Logger.WriteDebugMessage("Child record is not a User, query for a resource record to see if it is a VistA Clinic.");
            //                                //Check the related Resource if it is a Vista Clinic
            //                                try
            //                                {
            //                                    var childR = srv.mcs_resourceSet.First(r => r.Id == child.mcs_RelatedResourceId.Id);
            //                                    if (childR.mcs_Type.Value == 251920000)
            //                                    {
            //                                        Logger.WriteDebugMessage("Resource is a VistA Clinic, adding it to the Provider Site VCs String");
            //                                        provSiteVCs += childR.mcs_name + " ; ";
            //                                    }
            //                                    else
            //                                    {
            //                                        Logger.WriteDebugMessage("Resource is not a vista clinic.");
            //                                    }
            //                                }
            //                                catch (Exception ex)
            //                                {
            //                                    var error = "";
            //                                    if (ex.Message != null)
            //                                        error = "Exception: " + ex.Message;
            //                                    if (ex.InnerException != null)
            //                                        error += " Inner Exeption: " + ex.InnerException;
            //                                    Logger.WriteDebugMessage(error);
            //                                }
            //                            }
            //                        }
            //                        Logger.WriteDebugMessage("Finished Paired case of Type switch.");
            //                        break;
            //                        //default: No Default Required - Room or Technology (or unknown type), do nothing
            //                        //    break;
            //                }
            //            }
            //            else //Probably Single Provider, but check.
            //            {
            //                Logger.WriteDebugMessage("//Probably Single Provider, but check");
            //                if (provSiteResource.cvt_TSAResourceType == null)
            //                {
            //                    Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the single provider to this TSA: {0}.", provSiteResource.cvt_name));
            //                    break;
            //                }
            //                //Provider or Telepresenter
            //                if ((provSiteResource.cvt_TSAResourceType.Value == 2) || (provSiteResource.cvt_TSAResourceType.Value == 3))
            //                {
            //                    Logger.WriteDebugMessage("//Provider or Telepresenter [cvt_TSAResourceType is 2 or 3]");
            //                    if (provSiteResource.cvt_RelatedUserId != null)
            //                    {
            //                        if (provSiteResource.cvt_resourcespecguid != null)
            //                            builderProv.Append(AddResourceToConstraintGroup(provSiteResource.cvt_resourcespecguid));
            //                        else
            //                            Logger.WriteDebugMessage("No provSiteResource.cvt_resourcespecguid detected");

            //                        Logger.WriteDebugMessage("About to add Provider Name to Provider String");
            //                        if (providers == null || !providers.Contains(provSiteResource.cvt_RelatedUserId.Name + " ; "))
            //                            providers += provSiteResource.cvt_RelatedUserId.Name + " ; ";

            //                        //Add to Group Prov Only Branch
            //                        Logger.WriteDebugMessage("//Add to Group Prov Only Branch");
            //                        builderGroupProvOnlybranch.Append(AddResourceToConstraintGroup(provSiteResource.cvt_RelatedUserId.Id.ToString()));
            //                    }
            //                    else
            //                        Logger.WriteDebugMessage(String.Format("User not listed in Provider Group {0}.", provSiteResource.cvt_name));
            //                }
            //                else
            //                    Logger.WriteDebugMessage(String.Format("Type is not null and provider site resource is not a user {0}", provSiteResource.cvt_name));
            //            }
            //        }
            //        //Buildout Group Prov Only Paired here
            //        if (builderGroupProvOnlyAllRequiredBranch.Length > 0)
            //        {
            //            Logger.WriteDebugMessage("Building Group Prov Only Paired.");
            //            builderGroupProvOnlybranch.Append(AddResourceToConstraintGroup(BuildOut(thisTSA, builderGroupProvOnlyAllRequiredBranch, OrganizationService, 1, 0, false).ToString()));
            //        }
            //        Logger.WriteDebugMessage("Finished Sorting through Provider Site Resources");
            //        #endregion

            //        #region PATIENT SITE RESOURCES
            //        //Retrieve all the patient site resources
            //        Logger.WriteDebugMessage("Start Sorting through Patient Site Resources");
            //        var getPatResources = from patGroups in srv.cvt_patientresourcegroupSet
            //                              where patGroups.cvt_RelatedTSAid.Id == TSAId
            //                              where patGroups.statecode == 0
            //                              select new
            //                              {
            //                                  patGroups.Id,
            //                                  patGroups.cvt_RelatedResourceId,
            //                                  patGroups.cvt_resourcespecguid,
            //                                  patGroups.cvt_TSAResourceType,
            //                                  patGroups.cvt_type,
            //                                  patGroups.cvt_RelatedResourceGroupid,
            //                                  patGroups.cvt_RelatedUserId,
            //                                  patGroups.cvt_name
            //                              };

            //        //Loop through all of the Patient Site resources
            //        foreach (var patSiteResource in getPatResources)
            //        {
            //            if (patSiteResource.cvt_resourcespecguid != null)
            //            {
            //                //Verify that the Resource is typed
            //                if (patSiteResource.cvt_type != null)
            //                {
            //                    switch (patSiteResource.cvt_type.Value)
            //                    {
            //                        case 917290000: //Paired Resource Group
            //                            builderPatAllRequired.Append(AddResourceToConstraintGroup(patSiteResource.cvt_resourcespecguid));

            //                            if (patSiteResource.cvt_RelatedResourceGroupid == null)
            //                            {
            //                                Logger.WriteDebugMessage(String.Format("Patient Site Resource is invalid, please remove and re-add the Paired Resource to this TSA: {0}.", patSiteResource.cvt_name));
            //                                break;
            //                            }

            //                            //Query for child names, if Vista Clinics then write into the string
            //                            var childgroupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == patSiteResource.cvt_RelatedResourceGroupid.Id && g.statecode == 0);
            //                            foreach (var child in childgroupResourceRecords)
            //                            {
            //                                if (child.mcs_RelatedResourceId != null)
            //                                {
            //                                    //Check the related Resource if it is a Vista Clinic
            //                                    var childR = srv.mcs_resourceSet.First(r => r.Id == child.mcs_RelatedResourceId.Id);
            //                                    if (childR.mcs_Type.Value == 251920000)
            //                                        patSiteVCs += childR.mcs_name + " ; ";

            //                                    if (String.IsNullOrEmpty(patActivityParty) && childR.mcs_relatedResourceId != null)
            //                                        patActivityParty = childR.mcs_relatedResourceId.Id + "|" + Equipment.EntityLogicalName + "|" + patSiteResource.cvt_resourcespecguid;
            //                                }
            //                                else if (child.mcs_RelatedUserId != null) //User for Group
            //                                {
            //                                    if (patsiteUsers == null || !patsiteUsers.Contains(child.mcs_RelatedUserId?.Name + " ; ")) //User for Group
            //                                    {
            //                                        patsiteUsers += child.mcs_RelatedUserId?.Name + " ; ";
            //                                    }

            //                                    if (String.IsNullOrEmpty(patActivityParty))
            //                                        patActivityParty = child.mcs_RelatedUserId.Id + "|" + SystemUser.EntityLogicalName + "|" + patSiteResource.cvt_resourcespecguid;
            //                                }
            //                                else
            //                                    Logger.WriteDebugMessage(String.Format("Patient Site Resource is invalid, please remove and re-add the resource to this Paired Resource: {0}.", patSiteResource.cvt_name));
            //                            }
            //                            break;
            //                        case 251920000: //Vista Clinic
            //                            builderPat.Append(AddResourceToConstraintGroup(patSiteResource.cvt_resourcespecguid));
            //                            if (patSiteResource.cvt_RelatedResourceId != null)
            //                                patSiteVCs += patSiteResource.cvt_RelatedResourceId.Name.ToString() + " ; ";
            //                            break;
            //                        case 99999999: //Provider
            //                        case 100000000: //Telepresenter/Imager
            //                            builderPat.Append(AddResourceToConstraintGroup(patSiteResource.cvt_resourcespecguid));
            //                            var childgrpResourceRecords = from gr in srv.mcs_groupresourceSet
            //                                                          join rg in srv.mcs_resourcegroupSet on gr.mcs_relatedResourceGroupId.Id equals rg.mcs_resourcegroupId.Value
            //                                                          join prg in srv.cvt_patientresourcegroupSet on rg.mcs_resourcegroupId.Value equals prg.cvt_RelatedResourceGroupid.Id
            //                                                          where gr.mcs_RelatedUserId != null && prg.cvt_patientresourcegroupId.Value == patSiteResource.Id
            //                                                          select gr.mcs_RelatedUserId;

            //                            foreach (var child in childgrpResourceRecords)
            //                            {
            //                                if ((child != null) && (patsiteUsers == null || !patsiteUsers.Contains(child.Name + " ; ")))
            //                                    patsiteUsers += child.Name + " ; ";
            //                            }
            //                            break;
            //                        default: //Others  
            //                            builderPat.Append(AddResourceToConstraintGroup(patSiteResource.cvt_resourcespecguid));
            //                            break;
            //                    }
            //                }
            //                //If not typed, could be single provider, shouldn't be on patient side, but if
            //                else
            //                {   //Provider or Telepresenter
            //                    if (patSiteResource.cvt_TSAResourceType != null && ((patSiteResource.cvt_TSAResourceType.Value == 2) || (patSiteResource.cvt_TSAResourceType.Value == 3)))
            //                        builderPat.Append(AddResourceToConstraintGroup(patSiteResource.cvt_resourcespecguid));
            //                }
            //            }

            //            if ((patSiteResource.cvt_RelatedUserId != null) && (patsiteUsers == null || !patsiteUsers.Contains(patSiteResource.cvt_RelatedUserId.Name + " ; ")))
            //            {
            //                patsiteUsers += patSiteResource.cvt_RelatedUserId.Name + " ; ";
            //            }
            //        }

            //        //Getting all the unique Patient Sites to add into a field for the view.                      
            //        var getPatientSites = (from patGroupSites in srv.cvt_patientresourcegroupSet
            //                               where patGroupSites.cvt_RelatedTSAid.Id == TSAId
            //                               where patGroupSites.statecode == 0
            //                               select new
            //                               {
            //                                   patGroupSites.cvt_relatedsiteid,
            //                               }).Distinct();

            //        foreach (var patGroupSites in getPatientSites)
            //        {
            //            if (patGroupSites.cvt_relatedsiteid != null)
            //                patSites += patGroupSites.cvt_relatedsiteid.Name.ToString() + " ; ";
            //        }

            //        Logger.WriteDebugMessage("Finished Sorting through Patient Site Resources");
            //        #endregion

            //        //Check for TSA status != Production
            //        if (thisTSA.statuscode.Value != 251920000)
            //        {
            //            Logger.WriteDebugMessage("TSA is not in status Production, just update the string fields");
            //            UpdateTSA(thisTSA, patSites, providers, patSiteVCs, provSiteVCs, patsiteUsers, Logger, OrganizationService, srv, Guid.Empty);
            //            return;
            //        }

            //        //Else continue building
            //        #region Logic - Constructing the builders etc
            //        //Validation: No Resources, throw error
            //        if (builderProv.Length == 0 && builderProvAllRequired.Length == 0 && builderPat.Length == 0 && builderPatAllRequired.Length == 0)
            //            throw new InvalidPluginExecutionException("A TSA must have a resource listed in order to be put into production");


            //        //Determine the Type of TSA, and check specifically
            //        //Defaulted to 'Clinic' initially
            //        if (thisTSA.cvt_AvailableTelehealthModalities != null && thisTSA.cvt_AvailableTelehealthModalities.Value == 917290001)
            //            tsaType = "SFT";
            //        else if (thisTSA.cvt_Type == true)
            //            tsaType = "Home";
            //        else if (thisTSA.cvt_groupappointment == true)
            //            tsaType = "Group";

            //        switch (tsaType)
            //        {
            //            case "Home"://VA Video Connect - only Provider Side
            //                if (builderProv.Length == 0 && builderProvAllRequired.Length == 0)
            //                    throw new InvalidPluginExecutionException("A VA Video Connect TSA must have provider resources listed in order to be put into production");
            //                else
            //                {
            //                    if (builderProv.Length > 0)
            //                        builder.Append(builderProv);

            //                    if (!string.IsNullOrEmpty(builderProvAllRequired.ToString()))
            //                    {
            //                        Guid ProvAllRequiredSpecId = BuildOut(thisTSA, builderProvAllRequired, OrganizationService, 1, 0, true);
            //                        builder.Append(AddResourceToConstraintGroup(ProvAllRequiredSpecId.ToString()));
            //                    }
            //                }
            //                break;
            //            case "SFT"://SFT - only Patient Side
            //                if (builderPat.Length == 0 && builderPatAllRequired.Length == 0)
            //                    throw new InvalidPluginExecutionException("A Store Forward TSA must have patient resources listed in order to be put into production");
            //                else
            //                {
            //                    if (builderPat.Length > 0)
            //                        builder.Append(builderPat);

            //                    if (builderPatAllRequired.Length > 0)
            //                    {
            //                        Guid PatAllRequiredSpecId = BuildOut(thisTSA, builderPatAllRequired, OrganizationService, 1, 0, true);
            //                        builder.Append(AddResourceToConstraintGroup(PatAllRequiredSpecId.ToString()));
            //                    }
            //                }
            //                break;
            //            default: // Clinic Based or Group (PatAllReq is both) - both Provider and Patient sides
            //                if (builderProv.Length == 0 && builderProvAllRequired.Length == 0)
            //                    throw new InvalidPluginExecutionException("A TSA must have a provider resource listed in order to be put into production");
            //                if (builderPat.Length == 0 && builderPatAllRequired.Length == 0)
            //                    throw new InvalidPluginExecutionException("A TSA must have a patient resource listed in order to be put into production");
            //                if (tsaType == "Group" && builderPatAllRequired.Length == 0) // && patActivityParty.Length > 0)
            //                    throw new InvalidPluginExecutionException("A Group TSA must have at least one patient paired group listed in order to be put into production");
            //                if (builderProv.Length > 0) //Prov Normal
            //                {
            //                    Logger.WriteDebugMessage("Detected Prov resource");
            //                    builder.Append(builderProv);
            //                    //builderGroupProvOnlybranch.Append(builderProv);
            //                }

            //                if (builderProvAllRequired.Length > 0) //Prov AR
            //                {
            //                    Logger.WriteDebugMessage("Detected Prov Paired groups");
            //                    Guid ProvAllRequiredSpecId = BuildOut(thisTSA, builderProvAllRequired, OrganizationService, 1, 0, true);
            //                    builder.Append(AddResourceToConstraintGroup(ProvAllRequiredSpecId.ToString()));

            //                    //Guid ProvGroupBranchOnlyId = BuildOut(thisTSA, builderProvAllRequired, OrganizationService, 1, 0, true);
            //                    //builderGroupProvOnlybranch.Append(AddResourceToConstraintGroup(ProvGroupBranchOnlyId.ToString()));  
            //                }

            //                if (builderPat.Length > 0 && tsaType != "Group")
            //                    builder.Append(builderPat);

            //                if (builderPatAllRequired.Length > 0)
            //                {
            //                    Guid PatAllRequiredSpecId;
            //                    if (tsaType == "Group")
            //                        PatAllRequiredSpecId = BuildOut(thisTSA, builderPatAllRequired, OrganizationService, -1, 0, true);
            //                    else
            //                        PatAllRequiredSpecId = BuildOut(thisTSA, builderPatAllRequired, OrganizationService, 1, 0, true);

            //                    builder.Append(AddResourceToConstraintGroup(PatAllRequiredSpecId.ToString()));
            //                }
            //                break;
            //        }
            //        #endregion

            //        #region Logic - Constructing the service
            //        Logger.WriteDebugMessage("Building out ResourceSpec; TSASpec " + tsaType);

            //        if (tsaType == "Group")
            //        {//Need to default a resource to second branch for the search.  Preplugin will remove all pat resources.
            //            Logger.WriteDebugMessage("Group - Building out both branches");
            //            StringBuilder builderGroup = new System.Text.StringBuilder("");
            //            //Stage branch 2 - prov only
            //            Guid groupProvId = BuildOut(thisTSA, builderGroupProvOnlybranch, OrganizationService, -1, 0, false);
            //            builderGroup.Append(AddResourceToConstraintGroup(groupProvId.ToString()));

            //            //Stage branch 1 - all
            //            Guid groupCombinedId = BuildOut(thisTSA, builder, OrganizationService, -1, 0, false);
            //            builderGroup.Append(AddResourceToConstraintGroup(groupCombinedId.ToString()));

            //            Logger.WriteDebugMessage(builderGroup.ToString());

            //            //build those out into resourceSpecId. Choose either prov or prov+pat
            //            resourceSpecId = BuildOut(thisTSA, builderGroup, OrganizationService, 1, 2, false);
            //        }
            //        else
            //        {
            //            Logger.WriteDebugMessage("Individual - Building out normal");
            //            resourceSpecId = BuildOut(thisTSA, builder, OrganizationService, -1, 2, false);
            //        }
            //        int initialstatus = (int)service_initialstatuscode.Pending;
            //        if (thisTSA.cvt_groupappointment.HasValue && thisTSA.cvt_groupappointment.Value && !thisTSA.cvt_Type.Value)
            //            initialstatus = (int)service_initialstatuscode.Reserved;

            //        //Release 3.8 - Starts Every hour, duration is 1 hr.
            //        //Release 4.0 - duration and granularity configurable
            //        var duration = 60;
            //        var startEvery = "60";
            //        if (thisTSA.cvt_duration != null)
            //            duration = thisTSA.cvt_duration.Value;
            //        if (thisTSA.cvt_startevery != null)
            //            startEvery = thisTSA.cvt_startevery.ToString();

            //        Service newService = new Service
            //        {
            //            Name = thisTSA.mcs_name.ToString(),
            //            AnchorOffset = 480,
            //            Duration = duration,
            //            InitialStatusCode = new OptionSetValue(initialstatus),
            //            Granularity = "FREQ=MINUTELY;INTERVAL=" + startEvery + ";",
            //            ResourceSpecId = new EntityReference(ResourceSpec.EntityLogicalName, resourceSpecId)
            //        };

            //        //create the service
            //        Logger.WriteDebugMessage("Creating new Service");
            //        var newServiceId = OrganizationService.Create(newService);
            //        UpdateTSA(thisTSA, patSites, providers, patSiteVCs, provSiteVCs, patsiteUsers, Logger, OrganizationService, srv, newServiceId, patActivityParty);
            //        #endregion
            //    }
            //    catch (FaultException<OrganizationServiceFault> ex)
            //    {
            //        Logger.WriteToFile(ex.Message);
            //        throw new InvalidPluginExecutionException(McsSettings.getUnexpectedErrorMessage + ":" + ex.Message);
            //    }
            //    catch (Exception ex)
            //    {
            //        if (ex.Message.StartsWith("custom"))
            //        {
            //            Logger.WriteDebugMessage(ex.Message.Substring(6));

            //            throw new InvalidPluginExecutionException(ex.Message.Substring(6));
            //        }
            //        else
            //        {
            //            Logger.setMethod = "Execute";
            //            Logger.WriteToFile(ex.Message);
            //            throw new InvalidPluginExecutionException(McsSettings.getUnexpectedErrorMessage + ":" + ex.Message);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Passing in the Resource Group GUID, return the StringBuilder of the associated child records.
        /// </summary>
        /// <param name="resourceGroupId"></param>
        /// <param name="srv"></param>
        /// <param name="resourceNames"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static StringBuilder GetResources(Guid resourceGroupId, Xrm srv, out string resourceNames, out int count)
        {
            resourceNames = string.Empty;
            var resources = new StringBuilder();
            var grs = from resGroup in srv.mcs_groupresourceSet
                      join r in srv.mcs_resourceSet on resGroup.mcs_RelatedResourceId.Id equals r.mcs_resourceId.Value into resource
                      from res in resource.DefaultIfEmpty()
                      where resGroup.mcs_relatedResourceGroupId.Id == resourceGroupId && resGroup.statecode == 0
                      select new
                      {
                          name = resGroup.mcs_name,
                          user = resGroup.mcs_RelatedUserId,
                          resource = res.mcs_relatedResourceId
                      };
            count = grs.ToList().Count;

            foreach (var res in grs)
            {
                resourceNames += " " + res.name + " ;";
                if (res.user != null)
                    resources.Append(CvtHelper.AddResourceToConstraintGroup(res.user.Id.ToString()));
                else if (res.resource != null)
                    resources.Append(CvtHelper.AddResourceToConstraintGroup(res.resource.Id.ToString()));
            }
            return resources;
        }

        /// <summary>
        /// Add the Resources to the Constraint Group
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public static string AddResourceToConstraintGroup(string resourceId)
        {
            var builder = "resource[\"Id\"] == ";
            builder += new Guid(resourceId).ToString("B");
            builder += " || ";
            return builder;
        }

        /// <summary>
        /// wraps the xml body string with the appropriate tags to make it a valid constraints field in a constraintbasedgroup record
        /// </summary>
        /// <param name="strBuilder">the string that becomes the body of the constraintbasedgroup</param>
        /// <returns>the full Constraints xml string</returns>
        public static StringBuilder BuildConstraintsXML(StringBuilder strBuilder)
        {
            var mainStrBuilder = new System.Text.StringBuilder("<Constraints><Constraint><Expression><Body>");
            if (strBuilder.Length == 0)
                strBuilder.Append("false || ");
            mainStrBuilder.Append(strBuilder);
            //Remove " || " from end and close the Constraint Group with the correct tags
            mainStrBuilder.Remove(mainStrBuilder.Length - 4, 4);
            mainStrBuilder.Append("</Body><Parameters><Parameter name=\"resource\" /></Parameters></Expression></Constraint></Constraints>");
            return mainStrBuilder;
        }

        /// <summary>
        /// Deprecated: Build Constraint Based Group
        /// </summary>
        //public static Guid BuildOut(mcs_services thisTSA, StringBuilder strBuilder, IOrganizationService OrgService, int ReqCount, int constraintBasedGroupTypeCode, Boolean Site)
        //{
        //    //constraintBasedGroupTypeCode
        //    //        Static = 0,
        //    //        Dynamic = 1,
        //    //        Implicit = 2

        //    //Correctly tag the XML
        //    var mainStrBuilder = BuildConstraintsXML(strBuilder);

        //    var constraintBasedGroupSetup = new ConstraintBasedGroup
        //    {
        //        BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisTSA.OwningBusinessUnit.Id),
        //        Constraints = mainStrBuilder.ToString(),
        //        Name = thisTSA.mcs_name,
        //        GroupTypeCode = new OptionSetValue(constraintBasedGroupTypeCode)
        //    };
        //    //Create the new Constraint Based Group
        //    var newConstraintGroup = OrgService.Create(constraintBasedGroupSetup);

        //    var newSpec = new ResourceSpec
        //    {
        //        BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisTSA.OwningBusinessUnit.Id),
        //        ObjectiveExpression = @"<Expression><Body>udf ""Random""(factory,resource,appointment,request,leftoffset,rightoffset)</Body><Parameters><Parameter name=""factory"" /><Parameter name=""resource"" /><Parameter name=""appointment"" /><Parameter name=""request"" /><Parameter name=""leftoffset"" /><Parameter name=""rightoffset"" /></Parameters><Properties EvaluationInterval=""P0D"" evaluationcost=""0"" /></Expression>",
        //        RequiredCount = ReqCount,
        //        Name = "Selection Rule",
        //        GroupObjectId = newConstraintGroup,
        //        SameSite = Site
        //    };
        //    var specId = OrgService.Create(newSpec);
        //    return specId;
        //}

        /// <summary>
        /// Deprecated: Update TSA fields. Only update if values are different (clean up audit history)
        /// </summary>
        //public static void UpdateTSA(mcs_services thisTSA, String patSites, String providers, String patSiteVCs, String provSiteVCs, String patientSiteUsers, MCSLogger Logger, IOrganizationService OrganizationService, Xrm srv, Guid newServiceId, String patActivityParty = "")
        //{
        //    Logger.setMethod = "Update TSA";
        //    Logger.WriteDebugMessage("About to update TSA");
        //    var updateTSA = new Entity("mcs_services") { Id = thisTSA.Id };
        //    var updateCount = 0;
        //    Guid formerService = Guid.Empty;
        //    char[] charsToTrim = { ' ', ';' };

        //    if (newServiceId == Guid.Empty) //Draft
        //    {
        //        if (thisTSA.mcs_RelatedServiceId != null)
        //        {
        //            //Set to null
        //            formerService = thisTSA.mcs_RelatedServiceId.Id;
        //            updateTSA["mcs_relatedserviceid"] = null;
        //            updateTSA["cvt_grouppatientbranch"] = null;
        //            updateCount += 1;
        //        }
        //    }
        //    else //Production - always use the new id if present
        //    {
        //        //Check if there is a former value in service
        //        if (thisTSA.mcs_RelatedServiceId != null)
        //            formerService = thisTSA.mcs_RelatedServiceId.Id;
        //        updateTSA["mcs_relatedserviceid"] = new EntityReference(Service.EntityLogicalName, newServiceId);
        //        Logger.WriteDebugMessage("Group: patActivityParty=" + patActivityParty);
        //        if (patActivityParty != "")
        //        {
        //            updateTSA["cvt_grouppatientbranch"] = patActivityParty;
        //            Logger.WriteDebugMessage("Attempting to update the Group Patient Branch on the TSA.");
        //        }
        //        updateCount += 1;
        //    }

        //    var patSiteString = ValidateLength(patSites, 2500);
        //    if (thisTSA.cvt_patientsites != patSiteString)
        //    {
        //        if ((thisTSA.cvt_providers != null || thisTSA.cvt_providers != "") && (patSiteString != null || patSiteString != ""))
        //        {
        //            updateTSA["cvt_patientsites"] = patSiteString;
        //            updateCount += 1;
        //        }
        //    }

        //    var prositeUsersToUpdate = CvtHelper.ValidateLength(providers, 2500).TrimEnd(charsToTrim);
        //    if (thisTSA.cvt_providers != prositeUsersToUpdate)
        //    {
        //        if ((thisTSA.cvt_providers != null || thisTSA.cvt_providers != "") && (prositeUsersToUpdate != null || prositeUsersToUpdate != ""))
        //        {
        //            updateTSA["cvt_providers"] = prositeUsersToUpdate;
        //            updateCount += 1;
        //        }
        //    }

        //    var patVC = ValidateLength(patSiteVCs, 2500);
        //    if (thisTSA.cvt_patsitevistaclinics != patVC)
        //    {
        //        if ((thisTSA.cvt_patsitevistaclinics != null || thisTSA.cvt_patsitevistaclinics != "") && (patVC != null || patVC != ""))
        //        {
        //            updateTSA["cvt_patsitevistaclinics"] = patVC;
        //            updateCount += 1;
        //        }
        //    }

        //    var proVCs = ValidateLength(provSiteVCs, 2500);
        //    if (thisTSA.cvt_provsitevistaclinics != proVCs)
        //    {
        //        if ((thisTSA.cvt_provsitevistaclinics != null || thisTSA.cvt_provsitevistaclinics != "") && (proVCs != null || proVCs != ""))
        //        {
        //            updateTSA["cvt_provsitevistaclinics"] = proVCs;
        //            updateCount += 1;
        //        }
        //    }

        //    var patsiteUsersToUpdate = CvtHelper.ValidateLength(patientSiteUsers, 2500).TrimEnd(charsToTrim);
        //    if (thisTSA.cvt_patsiteusers != patsiteUsersToUpdate)
        //    {
        //        if ((thisTSA.cvt_patsiteusers != null || thisTSA.cvt_patsiteusers != "") && (patsiteUsersToUpdate != null || patsiteUsersToUpdate != ""))
        //        {
        //            updateTSA["cvt_patsiteusers"] = patsiteUsersToUpdate;
        //            updateCount += 1;
        //        }
        //    }

        //    Logger.WriteDebugMessage("Clearing Bulk Edit fields");
        //    //Clear Bulk Edit Fields
        //    if (thisTSA.cvt_BulkRemoveResource != null)
        //    {
        //        updateTSA["cvt_bulkremoveresource"] = null;
        //        updateCount += 1;
        //    }
        //    if (thisTSA.cvt_BulkAddResource != null)
        //    {
        //        updateTSA["cvt_bulkaddresource"] = null;
        //        updateCount += 1;
        //    }
        //    if (thisTSA.cvt_BulkRemoveUser != null)
        //    {
        //        updateTSA["cvt_bulkremoveuser"] = null;
        //        updateCount += 1;
        //    }
        //    if (thisTSA.cvt_BulkAddUser != null)
        //    {
        //        updateTSA["cvt_bulkadduser"] = null;
        //        updateCount += 1;
        //    }
        //    if (thisTSA.cvt_BulkRemoveResourceGroup != null)
        //    {
        //        updateTSA["cvt_bulkremoveresourcegroup"] = null;
        //        updateCount += 1;
        //    }
        //    if (thisTSA.cvt_BulkAddResourceGroup != null)
        //    {
        //        updateTSA["cvt_bulkaddresourcegroup"] = null;
        //        updateCount += 1;
        //    }

        //    if (updateCount > 0)
        //    {
        //        OrganizationService.Update(updateTSA);
        //        Logger.WriteDebugMessage("TSA Updated");
        //        if (formerService != Guid.Empty)
        //        {
        //            //Search on the prior service and attempt to delete it.
        //            var ServiceActivity = srv.ServiceAppointmentSet.FirstOrDefault(i => i.ServiceId.Id == formerService);
        //            //If an associated SA doesn't exist, delete
        //            if (ServiceActivity == null)
        //            {
        //                OrganizationService.Delete(Service.EntityLogicalName, formerService);
        //                Logger.WriteDebugMessage("Prior Service successfully deleted.");
        //            }
        //            else
        //                Logger.WriteDebugMessage("Prior Service could not be deleted, associated Service Activities exist.");
        //        }
        //    }
        //    else
        //        Logger.WriteDebugMessage("No systematically built values to update on TSA");
        //}

        /// <summary>
        /// Deprecated: Update MTSA fields
        /// </summary>
        public static void UpdateMTSA(Guid mtsaId, Guid deleteId, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.setMethod = "UpdateMTSA";
            Logger.WriteDebugMessage("starting UpdateMTSA");
            using (var srv = new Xrm(OrganizationService))
            {
                if (mtsaId == Guid.Empty)
                {
                    Logger.WriteDebugMessage("No MTSA value, exiting.");
                    return;
                }

                var getProvResources = from provGroups in srv.cvt_providerresourcegroupSet
                                       where provGroups.cvt_RelatedMasterTSAId.Id == mtsaId
                                       where provGroups.statecode == 0
                                       select new
                                       {
                                           provGroups.Id,
                                           provGroups.cvt_RelatedResourceId,
                                           provGroups.cvt_resourcespecguid,
                                           provGroups.cvt_TSAResourceType,
                                           provGroups.cvt_Type,
                                           provGroups.cvt_RelatedUserId,
                                           provGroups.cvt_RelatedResourceGroupid
                                       };

                //grab all the provider groups
                string provSiteVCs = null;
                string providers = null;
                foreach (var provGroups in getProvResources)
                {
                    if (provGroups.Id != deleteId)
                    {
                        //Verify that the Resource is typed, not required, but should be filled in
                        if (provGroups.cvt_Type != null)
                        {
                            //Naming
                            switch (provGroups.cvt_Type.Value)
                            {
                                case 251920000: //Vista Clinic
                                    if (provGroups.cvt_TSAResourceType.Value == 0) //Group of Vista Clinics
                                    {
                                        //Query for child names
                                        var groupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == provGroups.cvt_RelatedResourceGroupid.Id);
                                        foreach (var child in groupResourceRecords)
                                        {
                                            if (child.mcs_RelatedResourceId != null)
                                                provSiteVCs += child.mcs_RelatedResourceId.Name.ToString() + " ; ";
                                        }
                                    }
                                    else
                                    {
                                        if (provGroups.cvt_RelatedResourceId != null)
                                            provSiteVCs += provGroups.cvt_RelatedResourceId.Name.ToString() + " ; ";
                                    }
                                    break;
                                case 100000000: //Telepresenter/Imager
                                case 99999999: //Provider
                                    if (provGroups.cvt_TSAResourceType.Value == 0) //Group of Providers
                                    {
                                        //Query for child names
                                        var groupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == provGroups.cvt_RelatedResourceGroupid.Id);
                                        foreach (var child in groupResourceRecords)
                                        {
                                            if (child.mcs_RelatedUserId != null)
                                                providers += child.mcs_RelatedUserId.Name.ToString() + " ; ";
                                        }
                                    }
                                    else
                                    {
                                        if (provGroups.cvt_RelatedUserId != null)
                                            providers += provGroups.cvt_RelatedUserId.Name.ToString() + " ; ";
                                    }
                                    break;
                                case 917290000: //Paired
                                    //Query for child names
                                    var childgroupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == provGroups.cvt_RelatedResourceGroupid.Id);
                                    foreach (var child in childgroupResourceRecords)
                                    {
                                        if (child.mcs_RelatedUserId != null)
                                            providers += child.mcs_RelatedUserId.Name.ToString() + " ; ";
                                        else
                                        { //Check the related Resource if it is a Vista Clinic
                                            var childR = srv.mcs_resourceSet.First(r => r.Id == child.mcs_RelatedResourceId.Id);
                                            if (childR.mcs_Type.Value == 251920000)
                                                provSiteVCs += childR.mcs_name + " ; ";
                                        }
                                    }
                                    break;
                            }
                        }
                        else //Probably Single Provider, but check.
                        {
                            //Provider or Telepresenter
                            if ((provGroups.cvt_TSAResourceType.Value == 2) || (provGroups.cvt_TSAResourceType.Value == 3))
                            {
                                if (provGroups.cvt_RelatedUserId != null)
                                {
                                    providers += provGroups.cvt_RelatedUserId.Name.ToString() + " ; ";
                                }
                            }
                        }
                    }
                }

                var mtsa = srv.cvt_mastertsaSet.FirstOrDefault(i => i.Id == mtsaId);
                var updateMTSA = new Entity("cvt_mastertsa") { Id = mtsaId };
                var updateCount = 0;

                if (mtsa.cvt_providers != ValidateLength(providers, 2500))
                {
                    updateMTSA["cvt_providers"] = ValidateLength(providers, 2500);
                    updateCount += 1;
                }
                if (mtsa.cvt_providersitevistaclinics != ValidateLength(provSiteVCs, 2500))
                {
                    updateMTSA["cvt_providersitevistaclinics"] = ValidateLength(provSiteVCs, 2500);
                    updateCount += 1;
                }
                if (updateCount > 0)
                {
                    OrganizationService.Update(updateMTSA);
                    Logger.WriteDebugMessage("MTSA Updated");
                }
                else
                    Logger.WriteDebugMessage("No systematically built values to update on MTSA");
            }
        }

        /// <summary>
        /// Check length of a string and either reduce or return the original value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ValidateLength(string value, int length)
        {
            if (value != null)
            {
                if (value.Length > length)
                    return value.Substring(0, length);
            }
            else
                return "";
            return value;
        }

        public static void CheckPStatus(Guid psId, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            if (psId == Guid.Empty)
            {
                Logger.WriteDebugMessage("No PS value, exiting.");
                return;
            }

            using (var srv = new Xrm(OrganizationService))
            {
                var ps = srv.cvt_participatingsiteSet.FirstOrDefault(t => t.Id == psId);
                if (ps != null)
                {
                    Logger.WriteDebugMessage("Scheduleable Value = " + ps.cvt_scheduleable.Value);

                    if (ps.cvt_scheduleable.Value == false)
                        Logger.WriteDebugMessage("The Participating Site is not in to be scheduled status. Allowing the Resource to be deleted.");
                    else
                    {
                        string message = "The Participating Site cannot be in To Be Scheduled status for the Resource to be deleted.  Change the Participating Site, Save and then delete the Resource.";
                        Logger.WriteDebugMessage(message);
                        throw new InvalidPluginExecutionException(message);
                    }
                }
                else
                    Logger.WriteDebugMessage("Attempted to retrieve PS with ID: " + psId + ". Participating Site was not found.");
            }
        }
        #endregion

        #region Service Activity Filter for GFE vs BYOD
        public static bool isGfeServiceActivity(ServiceAppointment sa, IOrganizationService organizationService, MCSLogger logger)
        {
            int TECHNOLOGY_TYPE_SIP_DEVICE = 100000000, TECHNOLOGY_TYPE_VA_ISSUED_DEVICE = 917290002;
            if (sa.Customers == null || sa.Customers.FirstOrDefault() == null)
                throw new InvalidPluginExecutionException(string.Format("No Patients are listed on the service activity {0} named {1}", sa.Id, sa.Subject));
            if (sa.Customers.ToList().Count > 1) return false; //Run VMR integration if group
            var patientAP = sa.Customers.FirstOrDefault();
            Contact patient = null;
            using (var srv = new Xrm(organizationService))
                patient = srv.ContactSet.FirstOrDefault(c => c.Id == patientAP.PartyId.Id);

            if (patient == null)
                throw new InvalidPluginExecutionException(string.Format("No patient was found with Id: {0}, AP.ActivityId: {1}", patientAP.PartyId.Id, patientAP.ActivityId.Id));
            //var hasTablet = (!string.IsNullOrEmpty(patient.cvt_bltablet) && patient.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == TECHNOLOGY_TYPE_SIP_DEVICE) ||
            //                (!patient.GetAttributeValue<bool>("donotemail") && !string.IsNullOrEmpty(patient.EMailAddress1) && patient.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == TECHNOLOGY_TYPE_VA_ISSUED_DEVICE);
            var hasTablet = !string.IsNullOrEmpty(patient.cvt_bltablet) && patient.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == TECHNOLOGY_TYPE_SIP_DEVICE;
            logger.WriteDebugMessage(string.Format("SaId: {0}, PatientName: {1} Patient hasTablet (Service Activity is GFE): {2}", sa.Id, patient.FullName, hasTablet));
            return hasTablet;
        }
        public static bool isGfeServiceActivity(ServiceAppointment sa, IOrganizationService organizationService, PluginLogger logger)
        {
            int TECHNOLOGY_TYPE_SIP_DEVICE = 100000000;
            if (sa.Customers == null || sa.Customers.FirstOrDefault() == null)
                throw new InvalidPluginExecutionException(string.Format("No Patients are listed on the service activity {0} named {1}", sa.Id, sa.Subject));
            if (sa.Customers.ToList().Count > 1) return false; //Run VMR integration if group
            var patientAP = sa.Customers.FirstOrDefault();
            Contact patient = null;
            using (var srv = new Xrm(organizationService))
                patient = srv.ContactSet.FirstOrDefault(c => c.Id == patientAP.PartyId.Id);

            if (patient == null)
                throw new InvalidPluginExecutionException(string.Format("No patient was found with Id: {0}, AP.ActivityId: {1}", patientAP.PartyId.Id, patientAP.ActivityId.Id));
            //var hasTablet = (!string.IsNullOrEmpty(patient.cvt_bltablet) && patient.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == TECHNOLOGY_TYPE_SIP_DEVICE) ||
            //                (!patient.GetAttributeValue<bool>("donotemail") && !string.IsNullOrEmpty(patient.EMailAddress1) && patient.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == TECHNOLOGY_TYPE_VA_ISSUED_DEVICE);
            var hasTablet = !string.IsNullOrEmpty(patient.cvt_bltablet) && patient.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == TECHNOLOGY_TYPE_SIP_DEVICE;
            logger.Trace($"SaId: {sa.Id}, PatientName: {patient.FullName} Patient hasTablet (Service Activity is GFE): {hasTablet}");
            return hasTablet;
        }
        #endregion

        #region Time Zones
        public static DateTime ConvertTimeZone(IOrganizationService OrganizationService, MCSLogger Logger, DateTime date, int CRMTimeZoneCode, out string timeZonesString, out bool success)
        {
            success = false;
            timeZonesString = string.Empty;
            var timeZoneStdName = string.Empty;
            try
            {
                Logger.WriteDebugMessage("Converting Time to Appropriate Time Zone");
                using (var srv = new Xrm(OrganizationService))
                {
                    var timeZonerecord = srv.TimeZoneDefinitionSet.FirstOrDefault(t => t.TimeZoneCode != null && t.TimeZoneCode.Value == CRMTimeZoneCode);
                    var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZonerecord.StandardName);
                    timeZoneStdName = timeZonerecord.StandardName;
                    timeZonesString = (zone.SupportsDaylightSavingTime && zone.IsDaylightSavingTime(date)) ? zone.DaylightName : zone.StandardName;
                    Logger.WriteDebugMessage("timezoneString: " + timeZonesString);
                }
                var timeZoneCode = TimeZoneInfo.FindSystemTimeZoneById(timeZoneStdName);
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(date, timeZoneCode);
                success = true;
                return localTime;
            }
            catch (TimeZoneNotFoundException ex)
            {
                Logger.WriteToFile("Could not find " + timeZonesString + " time zone with code " + CRMTimeZoneCode.ToString() + ": " + ex.Message + " ; using UTC instead");
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Time Zone conversion issue" + ex.Message + " ; using UTC instead");
            }
            return date;
        }

        public static string GetTimeZoneString(int? timeZone, DateTime unconvertedTime, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            var fullTimeString = string.Empty;
            int timeZoneCode;
            if (timeZone == null)
            {
                Logger.WriteDebugMessage("No time zone was listed for this patient, defaulting to Eastern Standard Time");
                timeZoneCode = 35;
            }
            else
                timeZoneCode = timeZone.Value;
            var timeZoneString = string.Empty;
            var conversionSuccess = false;
            var scheduledTime = ConvertTimeZone(OrganizationService, Logger, unconvertedTime, timeZoneCode, out timeZoneString, out conversionSuccess);

            fullTimeString = scheduledTime.ToString("dddd dd MMMM yyyy") + " @ " + scheduledTime.ToString("HH:mm");

            fullTimeString += conversionSuccess ? " " + timeZoneString : " GMT";
            return fullTimeString;
        }

        public static int? GetSiteTimeZoneCode(ServiceAppointment serviceAppointment, IOrganizationService organizationService, MCSLogger logger)
        {
            logger.WriteDebugMessage("Getting Time Zone to convert String");
            var proSiteId = serviceAppointment.mcs_relatedprovidersite;
            int proSiteTimeZone = 35;
            if (proSiteId != null)
            {
                using (var srv = new Xrm(organizationService))
                {
                    var site = srv.mcs_siteSet.FirstOrDefault(s => s.Id == proSiteId.Id);
                    if (site != null)
                        proSiteTimeZone = site.mcs_TimeZone != null ? site.mcs_TimeZone.Value : proSiteTimeZone;
                }
            }
            return proSiteTimeZone;
        }

        /// <summary>
        /// Gets user time zone from user record, then falls back to user's site's time zone, finally defaults to user setting time zone if no others exist
        /// </summary>
        /// <param name="systemUserId">Id of the user</param>
        /// <returns></returns>
        public static int? GetUserTimeZoneCode(Guid systemUserId, IOrganizationService OrganizationService)
        {
            var provider = OrganizationService.Retrieve(SystemUser.EntityLogicalName, systemUserId, new ColumnSet(true)).ToEntity<SystemUser>();
            var timeZone = provider.cvt_TimeZone;
            if (timeZone == null)
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    if (provider.cvt_site != null)
                    {
                        var userSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == provider.cvt_site.Id);
                        timeZone = userSite.mcs_TimeZone;
                    }
                    if (timeZone == null)
                    {
                        var providerSettings = srv.UserSettingsSet.FirstOrDefault(s => s.SystemUserId.Value == provider.Id);
                        timeZone = providerSettings.TimeZoneCode;
                    }
                }
            }
            return timeZone;
        }
        #endregion

        #region Inventory Management Helper methods

        /// <summary>
        /// Configure and fill mismatch record information
        /// </summary>
        /// <param name="mismatch"></param>
        /// <param name="field"></param>
        /// <param name="tmp"></param>
        /// <param name="staging"></param>
        private static void ConfigureMismatch(cvt_fieldmismatch mismatch, KeyValuePair<string, Tuple<string, string>> field, Entity tmp, Entity staging)
        {

            switch (field.Value.Item2)
            {
                case "Picklist":
                    mismatch.cvt_tmpdisplayvalue = tmp.FormattedValues[field.Key]?.ToString();
                    mismatch.cvt_tmpinternalname = tmp.GetAttributeValue<OptionSetValue>(field.Key).Value.ToString();
                    mismatch.cvt_importdisplayvalue = staging.FormattedValues[field.Key]?.ToString();
                    mismatch.cvt_importinternalvalue = staging.GetAttributeValue<OptionSetValue>(field.Key).Value.ToString();
                    break;

                case "Lookup":
                    //TODO: Get the name from CRM if needed
                    mismatch.cvt_tmpdisplayvalue = tmp.GetAttributeValue<EntityReference>(field.Key).Name;
                    mismatch.cvt_tmpinternalname = tmp.GetAttributeValue<EntityReference>(field.Key).Id.ToString();
                    mismatch.cvt_importdisplayvalue = staging.GetAttributeValue<EntityReference>(field.Key).Name;
                    mismatch.cvt_importinternalvalue = staging.GetAttributeValue<EntityReference>(field.Key).Id.ToString();
                    break;

                case "DateTime":
                    mismatch.cvt_tmpdisplayvalue = tmp.GetAttributeValue<DateTime>(field.Key).ToString();
                    mismatch.cvt_tmpinternalname = tmp.GetAttributeValue<DateTime>(field.Key).ToString();
                    mismatch.cvt_importdisplayvalue = staging.GetAttributeValue<DateTime>(field.Key).ToString();
                    mismatch.cvt_importinternalvalue = staging.GetAttributeValue<DateTime>(field.Key).ToString();
                    break;

                default:
                    mismatch.cvt_tmpdisplayvalue = tmp.GetAttributeValue<string>(field.Key);
                    mismatch.cvt_tmpinternalname = tmp.GetAttributeValue<string>(field.Key);
                    mismatch.cvt_importdisplayvalue = staging.GetAttributeValue<string>(field.Key);
                    mismatch.cvt_importinternalvalue = staging.GetAttributeValue<string>(field.Key);
                    break;
            }

        }
        /// <summary>
        /// Find mismatched fileds between tmp resource and staging resource
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="satgingResrouce"></param>
        /// <returns></returns>
        public static List<Entity> FindMismatch(mcs_resource resource, cvt_stagingresource satgingResrouce)
        {
            List<Entity> mismatched = new List<Entity>();
            foreach (var field in ResrouceMetadata)
            {
                cvt_fieldmismatch mismatch = new cvt_fieldmismatch();


                var resEnt = (resource.Contains(field.Key) ? resource[field.Key] : null);
                var stgEnt = (satgingResrouce.Contains(field.Key) ? satgingResrouce[field.Key] : null);

                if (stgEnt == null || resEnt == null)
                    continue;
                //if picklist we need to make sure that the values are equal becuase not all picklists glopal
                if (field.Value.Item2 == "Picklist" && resource.FormattedValues[field.Key] == satgingResrouce.FormattedValues[field.Key])
                    continue;

                if (!resEnt.Equals(stgEnt))
                {
                    ConfigureMismatch(mismatch, field, resource, satgingResrouce);
                    mismatch.cvt_entity = new OptionSetValue((int)cvt_fieldmismatchcvt_entity.Resource);
                    mismatch.cvt_resourceid = new EntityReference(resource.LogicalName, resource.Id);
                    if (satgingResrouce.Id != null && satgingResrouce.Id != Guid.Empty)
                        mismatch.cvt_stagingresource = satgingResrouce.ToEntityReference();
                    mismatch.cvt_fieldname = field.Value.Item1;
                    mismatch.cvt_fieldschemaname = field.Key;
                    mismatched.Add(mismatch);
                }

            }

            return mismatched;
        }

        /// <summary>
        /// Find mismatched fileds between tmp component and staging component
        /// </summary>
        /// <param name="component"></param>
        /// <param name="satgingComponent"></param>
        /// <returns></returns>
        public static List<Entity> FindMismatch(cvt_component component, cvt_stagingcomponent satgingComponent, cvt_stagingresource stagingresource = null)
        {
            List<Entity> mismatched = new List<Entity>();
            foreach (var field in ComponentsMetadata)
            {
                cvt_fieldmismatch mismatch = new cvt_fieldmismatch();

                var resEnt = (component.Contains(field.Key) ? component[field.Key] : null);
                var stgEnt = (satgingComponent.Contains(field.Key) ? satgingComponent[field.Key] : null);

                if (stgEnt == null || resEnt == null)
                    continue;

                //if picklist we need to make sure that the values are equal becuase not all picklists glopal
                if (field.Value.Item2 == "Picklist" && component.FormattedValues[field.Key] == satgingComponent.FormattedValues[field.Key])
                    continue;

                if (!resEnt.Equals(stgEnt))
                {
                    ConfigureMismatch(mismatch, field, component, satgingComponent);
                    mismatch.cvt_entity = new OptionSetValue((int)cvt_fieldmismatchcvt_entity.Component);
                    mismatch.cvt_componentid = new EntityReference(component.LogicalName, component.Id);
                    if (stagingresource != null && stagingresource.Id != Guid.Empty)
                        mismatch.cvt_stagingresource = stagingresource.ToEntityReference();
                    if (satgingComponent.Id != null && satgingComponent.Id != Guid.Empty)
                        mismatch.cvt_stagingcomponentId = satgingComponent.ToEntityReference();
                    mismatch.cvt_fieldname = field.Value.Item1;
                    mismatch.cvt_fieldschemaname = field.Key;
                    mismatched.Add(mismatch);
                }
            }

            return mismatched;
        }

        //Instead of using metadata services for each field, info hardcoded here
        private static readonly Dictionary<string, Tuple<string, string>> ResrouceMetadata
            = new Dictionary<string, Tuple<string, string>>
            {
                //{ "mcs_type", Tuple.Create("Type","Picklist")},
                //{ "cvt_masterserialnumber", Tuple.Create("Master Serial Number","String")},
                { "cvt_systemtype", Tuple.Create("System Type","Picklist")},
                //{ "mcs_relatedsiteid", Tuple.Create("TMP Site","Lookup")},//x
                { "cvt_carttypeid", Tuple.Create("Cart Type","Lookup")},
                //We might need to implement a logic to determine facility
                { "mcs_facility", Tuple.Create("Facility","Lookup")}, // queried based on station number
                //{ "mcs_businessunitid", Tuple.Create("VISN","Lookup")},
                { "cvt_locationuse", Tuple.Create("Equipment Location Type","Picklist")},
                { "cvt_relateduser", Tuple.Create("POC","Lookup")},
                { "cvt_uniqueid", Tuple.Create("Unique ID","String")},
                //{ "cvt_supportedmodality", Tuple.Create("Supported TH Modality","Picklist")},//x
                //{ "cvt_room", Tuple.Create("Room","String")},//x
                //{ "cvt_lastequipmentrefreshdate", Tuple.Create("Last Equipment Refresh Date","DateTime")},//x
                //{ "cvt_lastcalibrationdate", Tuple.Create("Last Calibration Date","DateTime")},//x
                //{ "cvt_systemindex", Tuple.Create("System Index","String")}//x
            };

        private static readonly Dictionary<string, Tuple<string, string>> ComponentsMetadata
            = new Dictionary<string, Tuple<string, string>>
            {
                { "cvt_componenttype", Tuple.Create("Component Type","Lookup")},
                { "cvt_status", Tuple.Create("Status","Picklist")},
                //{ "cvt_description", Tuple.Create("Description","Memo")},
                 { "cvt_manufacturerid", Tuple.Create("Manufacturer","Lookup")},
               // { "cvt_manufacturer", Tuple.Create("Manufacturer","String")},
                //{ "cvt_serialnumber", Tuple.Create("Serial Number","String")},
                { "cvt_modelnumber", Tuple.Create("Model Number","Lookup")},
                //{ "cvt_eenumber", Tuple.Create("EE Number","String")},
                { "cvt_partnumber", Tuple.Create("Part Number","String")},
                { "cvt_e164alias", Tuple.Create("E.164 alias","String")}
                //{ "cvt_ipaddress", Tuple.Create("IP Address","String")},//x
                //{ "cvt_interfacedperipherals", Tuple.Create("Interfaced Peripherals","String")},//x
                //{ "cvt_cevnalias", Tuple.Create("CEVN Alias","String")},//x
                //{ "cvt_evnaccountname", Tuple.Create("EVN Account Name","String")},//x
                //{ "cvt_tmssystemname", Tuple.Create("TMS System Name","String")},//x
                //{ "cvt_computername", Tuple.Create("Computer Name","String")},//x
                //{ "cvt_webinterfaceurl", Tuple.Create("Web Interface URL","String")},//x
                //{ "cvt_tmsid", Tuple.Create("TMS ID","String")}//x
            };

        #endregion
    }

    public enum ResourcesCSVColumns
    {
        MasterSerialNumber = 8,
        SystemTypes = 9, //include
        CartType = 10,
        //LastCalibrationDate = 13, //x
        //LastEquipmentRefreshDate = 14, //x
        UniqueID = 11,
        StationNumber = 12,
        MedicalCenter = 13,
        Ifcapponumber = 14,
        POC = 15, //include
                  //Room = 17,//x
                  //SupportedTHModality = 18,//x
                  //TMPSite = 19,//x
                  //Type = 10,

        // used to query facility

        //EquipmentLocationType =14, // default to clnick based, not included in the excel

        //The name needs to be fixed

    }

    public enum ComponentsCSVColumns
    {
        ComponentType = 0,
        //ComponentTypeName = 1, //x
        Manufacturer = 1,
        //Model = 3,//x
        //ModelName = 4, good to have, if model number does not exist then set model number to other and add the text to Model name (other model field)
        ModelNumber = 2,
        PartNumber = 3,
        //EENumber = 7, //x
        //SystemNumber = 8, //x
        //Resource = 4,
        SerialNumber = 4,
        TMSSystemTypeDescription = 5, //cvt_tmssystemtypedescription
        E164alias = 6,
        SystemName = 7,
        IpAddress = 16,

        // Status = 7// deployed
    }

    //need to be added 
    // componnet - E#164 need mismatch
    //compoenet - System name text : needs to import as text field, NoMismatch
    //Visin: needs to confirm
    //Resrouce Medical center text fields
    //Resource IFCxxxNumber text noMismach

    //Aditional logic
    //Notes TMS System name should be on both entities (need to get the one that has value)
    //Name of Resrouce should be master serial number
    //Name of the compoenent should be Componenet Type
}