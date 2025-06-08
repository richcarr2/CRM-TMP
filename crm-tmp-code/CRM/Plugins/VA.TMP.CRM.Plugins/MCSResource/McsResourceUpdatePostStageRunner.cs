using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class McsResourceUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }       
        #endregion
        
        #region Internal Methods
        /// <summary>
        /// This plugin runs after the mcs_Resource has been updated.
        ///It accomplishes the following:
        ///1.Checks to see if the mcs_Resource has the Resource Group GUID text field, and will run the UpdateResourceGroup method (In MCS Resource Group Plugin Update PS Runner), if it has a GUID. If the Resource Group GUID text field is null, the method does not run. 
        /// 2.Updates all attributes for the System Resource that is associated with the mcs_Resource. 
        /// </summary>
        public override void Execute()
        {
            if (PluginExecutionContext.Depth > 2) { return; }
            var thisResource = PrimaryEntity.ToEntity<mcs_resource>();

            var thisResourceRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, mcs_resource.EntityLogicalName, Logger, OrganizationService);
            CvtHelper.AssignOwner(thisResourceRecord, Logger, OrganizationService);
            
            //We will check these Boolean fields first to see if they have been flagged for Update / Replace / Delete. 
            bool updateResourceConnections = thisResource.cvt_updateresourceconnections != null ? thisResource.cvt_updateresourceconnections.Value : false;
            bool replaceResourceConnections = thisResource.cvt_replaceresourceconnections != null ? thisResource.cvt_replaceresourceconnections.Value : false;
            bool deleteResourceConnections = thisResource.cvt_deleteresourceconnections != null ? thisResource.cvt_deleteresourceconnections.Value : false;

            var ReplacementResource = thisResource.cvt_replacementresource;
            //Update self with new Facility and VISN.
            CvtHelper.AlignLocations(thisResource, OrganizationService, Logger);

            if (updateResourceConnections)
                UpdateConnections(thisResource, thisResource.Id);
            if (ReplacementResource != null && replaceResourceConnections)
                UpdateConnections(thisResource, McsHelper.getEntRefID("cvt_replacementresource"));
            if (deleteResourceConnections)
                DeleteResourceConnections(PluginExecutionContext.PrimaryEntityId);

            if (thisResource.cvt_CartTypeId != null)
                UpdateComponentCartType(thisResource.Id, thisResource.cvt_CartTypeId);

            if (thisResource.mcs_relatedResourceId != null)
                UpdateSystemResource(PluginExecutionContext.PrimaryEntityId, thisResource);                    
            else
            {
                var resource = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName, PluginExecutionContext.PrimaryEntityId, new ColumnSet(true));
                if (resource.mcs_relatedResourceId != null)
                    UpdateSystemResource(resource.Id, resource);
                else
                    Logger.WriteDebugMessage("Not Doing Update of Sys Resource");
            }

            if (PrimaryEntity.Attributes.Contains("mcs_relatedsiteid") && PluginExecutionContext.Depth == 1)
            {
                var preEntity = GetPreImage();
                if (preEntity?.Attributes != null && preEntity.Attributes.Contains("mcs_relatedsiteid") &&
                    preEntity.GetAttributeValue<EntityReference>("mcs_relatedsiteid").Id != McsHelper.getEntRefID("mcs_relatedsiteid"))
                {
                    var error = ValidateMoveResource(PrimaryEntity.Id);
                    if (error != String.Empty)
                        throw new InvalidPluginExecutionException(error);
                }
            }

            if (PrimaryEntity.Attributes.Contains("mcs_name"))
                UpdateGRRecords(PrimaryEntity.Id, PrimaryEntity.Attributes["mcs_name"].ToString());
        }

        private void UpdateComponentCartType(Guid resourceId, EntityReference cartType)
        {
            Logger.setMethod = "UpdateComponentCartType";
            Logger.WriteGranularTimingMessage("Starting UpdateComponentCartType");
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    var componentList = from c in srv.cvt_componentSet where c.statecode.Value == 0 && c.cvt_relatedresourceid.Id == resourceId select c;
                    foreach (cvt_component component in componentList)
                    {
                        try
                        {
                            var updateComponent = new cvt_component()
                            {
                                Id = component.Id,
                                cvt_CartTypeId = cartType
                            };
                            OrganizationService.Update(updateComponent);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile(string.Format("Failed to update Component {0} for the resource: {1}. Exception: {2}", component.Id, resourceId, ex.Message));
                        }
                    }
                    Logger.WriteGranularTimingMessage("Ending UpdateComponentCartType");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        /// <summary>
        /// Update Group Resource Records
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="newName"></param>
        internal void UpdateGRRecords(Guid resourceId, string newName)
        {
            Logger.WriteDebugMessage("starting UpdateTSSResourceGroups");
            using (var srv = new Xrm(OrganizationService)) {

                //Search for all Group Resources that this TSS Resource is respresented by.
                var groups = srv.mcs_groupresourceSet.Where(gr => gr.mcs_RelatedResourceId.Id == resourceId);

                foreach (mcs_groupresource gr in groups) {
                    if (gr.mcs_relatedResourceGroupId == null)
                    {
                        Logger.WriteDebugMessage($"Resource Group (mcs_relatedResourceGroupId) field value is not available for the Group Resources (mcs_groupresource) record {gr.mcs_name} with Id: {gr.Id}");
                    }
                    else
                    {
                        //Find the Sibling Group Resources for this parent TSS Resource Group
                        var siblingGroupResources = srv.mcs_groupresourceSet.Where(rg => rg.mcs_relatedResourceGroupId.Id == gr.mcs_relatedResourceGroupId.Id);

                        Logger.WriteDebugMessage("Looking for child TSS Resources for TSS Resource Group: " + gr.mcs_relatedResourceGroupId.Name);
                        foreach (mcs_groupresource sGR in siblingGroupResources)
                        {
                            if (sGR.mcs_RelatedResourceId != null && sGR.mcs_RelatedResourceId.Id == resourceId)
                            {
                                mcs_groupresource updateGR = new mcs_groupresource()
                                {
                                    Id = sGR.Id,
                                    mcs_name = newName
                                };
                                OrganizationService.Update(updateGR);
                                Logger.WriteDebugMessage("Updating the Group Resource's Name");
                            }
                        }
                    }
                }

                //Update Prov and Pat Site Resources
                var patRes = srv.cvt_patientresourcegroupSet.Where(pgr => pgr.cvt_RelatedResourceId.Id == resourceId);
                var provRes = srv.cvt_providerresourcegroupSet.Where(pgr => pgr.cvt_RelatedResourceId.Id == resourceId);

                foreach (cvt_patientresourcegroup prg in patRes) {
                    cvt_patientresourcegroup updatePRG = new cvt_patientresourcegroup() {
                        Id = prg.Id,
                        cvt_name = newName
                    };
                    try {
                        OrganizationService.Update(updatePRG);
                        Logger.WriteDebugMessage("Updated Patient Resource Group");
                    } catch (Exception ex) {
                        Logger.WriteToFile(String.Format("Failed to update Patient Resource Group: {0}. Error: {1}", prg.cvt_name, ex.Message));
                    }
                }

                foreach (cvt_providerresourcegroup prg in provRes) {
                    cvt_providerresourcegroup updatePRG = new cvt_providerresourcegroup() {
                        Id = prg.Id,
                        cvt_name = newName
                    };
                    try {
                        OrganizationService.Update(updatePRG);
                        Logger.WriteDebugMessage("Updated Provider Resource Group");
                    } catch (Exception ex) {
                        Logger.WriteToFile(String.Format("Failed to update Provider Resource Group: {0}. Error: {1}", prg.cvt_name, ex.Message));
                    }
                }
            }

        }
        /// <summary>
        /// Check the TSS Resource for Related records and return an error string if any exist - look for active TSAs, Appts, SAs, and Resource Groups
        /// </summary>
        /// <param name="resourceId">id of resource record being moved</param>
        /// <returns>a string of error messages or an empty string if validation passes</returns>
        internal string ValidateMoveResource(Guid resourceId)
        {
            //instantiate return variable
            var errorMessage = String.Empty;
            var equipmentId = McsHelper.getEntRefID("mcs_relatedresourceid");
            using (var srv = new Xrm(OrganizationService))
            {
                //Get All Relevant Relationships for this resource (patGroup is active, tsa is active; same for ProGroup; Resource Group is active, activity scheduled is future and is in scheduled status)
                var relevantPats = (from patGroup in srv.cvt_patientresourcegroupSet
                                    join tsa in srv.cvt_facilityapprovalSet on patGroup.cvt_RelatedTSAid.Id equals tsa.cvt_facilityapprovalId
                                    where patGroup.cvt_RelatedResourceId.Id == resourceId
                                    where patGroup.statecode.Value == cvt_patientresourcegroupState.Active
                                    where tsa.statecode.Value == cvt_facilityapprovalState.Active   //mcs_servicesState.Active
                                    select new
                                    {
                                        patGroup.Id //Only care about counts, so just select id column
                                    }).ToList();

                var relevantPros = (from provGroup in srv.cvt_providerresourcegroupSet
                                    join tsa in srv.cvt_facilityapprovalSet on provGroup.cvt_RelatedTSAid.Id equals tsa.cvt_facilityapprovalId
                                    where provGroup.cvt_RelatedResourceId.Id == resourceId
                                    where provGroup.statecode.Value == cvt_providerresourcegroupState.Active
                                    where tsa.statecode.Value == cvt_facilityapprovalState.Active   //mcs_servicesState.Active 
                                    select new
                                    {
                                        provGroup.Id //Only care about counts, so just select id column
                                    }).ToList();

                var relevantGroups = (from groupRes in srv.mcs_groupresourceSet
                                      join resGroup in srv.mcs_resourcegroupSet on groupRes.mcs_relatedResourceGroupId.Id equals resGroup.Id
                                      where groupRes.mcs_RelatedResourceId.Id == resourceId
                                      where resGroup.statecode.Value == mcs_resourcegroupState.Active
                                      where groupRes.statecode.Value == mcs_groupresourceState.Active
                                      select new
                                      {
                                          id = groupRes.mcs_relatedResourceGroupId.Id //need resource group ID for later query
                                      }).ToList();

                var tsas = relevantPros.Count() + relevantPats.Count();
                if (relevantGroups != null && relevantGroups.Count() > 0)
                {
                    var proGroups = srv.cvt_providerresourcegroupSet.Where(p => p.cvt_RelatedTSAid != null && p.cvt_RelatedResourceGroupid != null);
                    var groupProTSAs = (from groups in relevantGroups
                                        join provGroups in proGroups on groups.id equals provGroups.cvt_RelatedResourceGroupid.Id
                                        join tsa in srv.cvt_facilityapprovalSet on provGroups.cvt_RelatedTSAid.Id equals tsa.Id
                                        where provGroups.statecode.Value == cvt_providerresourcegroupState.Active
                                        where tsa.statecode.Value == cvt_facilityapprovalState.Active   //mcs_servicesState.Active 
                                        select new
                                        {
                                            tsa.Id
                                        }).ToList();

                    var patGroupSet = srv.cvt_patientresourcegroupSet.Where(p => p.cvt_RelatedTSAid != null && p.cvt_RelatedResourceGroupid != null);
                    var groupPatTSAs = (from groups in relevantGroups
                                        join patGroups in patGroupSet on groups.id equals patGroups.cvt_RelatedResourceGroupid.Id
                                        join tsa in srv.cvt_facilityapprovalSet on patGroups.cvt_RelatedTSAid.Id equals tsa.Id
                                        where patGroups.statecode.Value == cvt_patientresourcegroupState.Active
                                        where tsa.statecode.Value == cvt_facilityapprovalState.Active   //mcs_servicesState.Active 
                                        select new
                                        {
                                            tsa.Id
                                        }).ToList();
                    tsas += groupProTSAs.Count() + groupPatTSAs.Count();
                }

                var activities = (from ap in srv.ActivityPartySet
                                  join activity in srv.ActivityPointerSet on ap.ActivityId.Id equals activity.Id
                                  where ap.PartyId.Id == equipmentId
                                  where (activity.StateCode == ActivityPointerState.Scheduled || activity.StateCode == ActivityPointerState.Open)
                                  where (activity.ScheduledStart.Value > DateTime.Now.ToUniversalTime()) //activity is in future
                                  select new
                                  {
                                      activity.Id //Only care about counts, so just select id column
                                  }).ToList();

                if (activities.Count() + relevantGroups.Count() + tsas > 0)
                {
                    errorMessage = String.Format("You have {0} Activities, {1} TSAs, and {2} Resource Groups related to this TMP Resource.  ", activities.Count(), tsas, relevantGroups.Count());
                    if (activities.Count() > 0)
                        errorMessage += "\nYou cannot change the site for this resource until you have cancelled or rescheduled all of its appointments and service activities";
                    if (tsas > 0)
                        errorMessage += "\nYou cannot change the site for this resource until you have removed or replaced it on all TSAs (with the \"Replace Resource\" or \"Remove All Connections\" button)";
                    if (relevantGroups.Count() > 0)
                        errorMessage += "\nYou cannot change the site for this resource until you have removed it from all existing resource groups";
                }
            }
            return errorMessage;
        }

        internal void UpdateSystemResource(Guid thisId, mcs_resource resource)
        {
            Logger.setMethod = "UpdateSystemResource";
            Logger.WriteGranularTimingMessage("Starting UpdateSystemResource");
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {                  
                    //Grabs all the attributes needed to update the associated System Resource of the mcs_Resource that was just updated. 
                    var systemEquip = srv.EquipmentSet.FirstOrDefault(i => i.Id == resource.mcs_relatedResourceId.Id);
                    if (systemEquip == null) { return; }

                    var thisResource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == thisId);
                    var updateSysResource = new Entity("equipment") {Id = systemEquip.Id};

                    updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, thisResource, "name", "mcs_name");
                    updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, thisResource, "cvt_type", "mcs_type");
                    updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, thisResource, "mcs_relatedresource", "mcs_resourceid");
                    updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, thisResource, "businessunitid", "owningbusinessunit");
                    
                    if (thisResource.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic)
                    {
                        updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, thisResource, "cvt_capacity", "cvt_vistacapacity");
                        if (updateSysResource.Attributes.Contains("cvt_capacity"))  //If the capacity changed, update the calendar accordingly
                            ChangeNewlyCreatedCalendarUpdate(systemEquip.Id);
                    }
                    else
                        updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, thisResource, "cvt_capacity", "cvt_capacity");
                    
                    var equipSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("mcs_relatedsiteid"));
                    Logger.WriteDebugMessage("got mcssite: " + equipSite.mcs_RelatedActualSiteId.Name);

                    if (equipSite == null)
                        return;
                    updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, equipSite, "siteid", "mcs_relatedactualsiteid");
                    updateSysResource = CvtHelper.UpdateField(updateSysResource, systemEquip, equipSite, "timezonecode", "mcs_timezone");

                    //Updates System Resource if anything has changed
                    //TODO: Does the ID count as 1 attribute?
                    if (updateSysResource.Attributes.Count() > 0)
                    {
                        OrganizationService.Update(updateSysResource);
                        Logger.WriteDebugMessage("System Resource Updated");
                    }
                    Logger.WriteGranularTimingMessage("Ending UpdateSytemResource");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }                
            }
        }

        /// <summary>
        /// Updates a resource's child records (patientresourcegroup, providerresourcegroup, groupresources, and resourcegroups) to match the updated info on the resource
        /// </summary>
        /// <param name="thisResource">the existing resource</param>
        /// <param name="updateResourceId">the new resource information</param>
        internal void UpdateConnections(mcs_resource thisResource, Guid updateResourceId)
        {
            Logger.WriteDebugMessage("Updating Resource's Connections");
            var UpdateResource = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName, updateResourceId, new ColumnSet(true));

            var schResources = new List<cvt_schedulingresource>();
            var counter = 0;
            using (var srv = new Xrm(OrganizationService))
                schResources = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_tmpresource.Id == thisResource.Id && sr.statecode.Value == 0).ToList();
            foreach (var sResource in schResources)
            {
                var update = new Entity();
                update.Id = sResource.Id;
                update = CvtHelper.UpdateField(update, sResource, UpdateResource, "cvt_name", "mcs_name");
                update = CvtHelper.UpdateField(update, sResource, UpdateResource, "cvt_type", "mcs_type");
                update = CvtHelper.UpdateField(update, sResource, UpdateResource, "cvt_capacityrequired", "cvt_capacity");
                update = CvtHelper.UpdateField(update, sResource, UpdateResource, "cvt_roomcapacity", "cvt_capacity");
                update = CvtHelper.UpdateField(update, sResource, UpdateResource, "cvt_resourcespecguid", "mcs_resourcespecguid");
                update = CvtHelper.UpdateField(update, sResource, UpdateResource, "cvt_constraintgroupguid", "mcs_constraintgroupguid");
                update = CvtHelper.UpdateField(update, sResource, UpdateResource, "cvt_relatedsiteid", "mcs_relatedsiteid");
                if (thisResource.cvt_replaceresourceconnections != null && thisResource.cvt_replaceresourceconnections.Value && thisResource.cvt_replacementresource != null)
                    update = CvtHelper.UpdateField(update, sResource, thisResource, "cvt_relatedresourceid", "cvt_replacementresource");
                if (update.Attributes.Count() > 0)
                {
                    counter++;
                    update.LogicalName = cvt_schedulingresource.EntityLogicalName;
                    var updateEntity = update.ToEntity<cvt_schedulingresource>();
                    OrganizationService.Update(updateEntity);
                    //if (sResource.cvt_RelatedTSAid != null)
                    //    CvtHelper.CreateUpdateService(sResource.cvt_RelatedTSAid.Id, Logger, OrganizationService, McsSettings);
                }
                else
                    Logger.WriteDebugMessage("No Change was detected in the scheduling resource.");
            }
            Logger.WriteDebugMessage(string.Format("{0}/{1} Scheduling Resources Updated", counter, schResources.Count()));

            var groupResources = new List<mcs_groupresource>();
            counter = 0;
            using (var srv = new Xrm(OrganizationService))
                groupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_RelatedResourceId.Id == thisResource.Id).ToList();
            //Going through Group Resources that need to be updated
            foreach (var groupResource in groupResources)
            {
                var update = new Entity();
                update.Id = groupResource.Id;
                update = CvtHelper.UpdateField(update, groupResource, UpdateResource, "mcs_name", "mcs_name");
                update = CvtHelper.UpdateField(update, groupResource, UpdateResource, "mcs_type", "mcs_type");
                update = CvtHelper.UpdateField(update, groupResource, UpdateResource, "mcs_relatedsiteid", "mcs_relatedsiteid");
                update = CvtHelper.UpdateField(update, groupResource, UpdateResource, "cvt_capacity", "cvt_capacity");
                if (thisResource.cvt_replaceresourceconnections != null && thisResource.cvt_replaceresourceconnections.Value && thisResource.cvt_replacementresource != null)
                    update = CvtHelper.UpdateField(update, groupResource, thisResource, "mcs_relatedresourceid", "cvt_replacementresource");
                if (update.Attributes.Count() > 0)
                {
                    counter++;
                    update.LogicalName = mcs_groupresource.EntityLogicalName;
                    var updateEntity = update.ToEntity<mcs_groupresource>();
                    OrganizationService.Update(updateEntity);
                    Logger.WriteDebugMessage("Group Resource Updated");
                    if (groupResource.mcs_relatedResourceGroupId != null)
                    {
                        UpdateRG(groupResource.mcs_relatedResourceGroupId.Id);
                        //using (var srv = new Xrm(OrganizationService))
                        //{
                        //    var proResourceGroups = srv.cvt_providerresourcegroupSet.Where(prg => prg.cvt_RelatedResourceGroupid.Id == groupResource.mcs_relatedResourceGroupId.Id).ToList();
                        //    var patResourceGroups = srv.cvt_patientresourcegroupSet.Where(prg => prg.cvt_RelatedResourceGroupid.Id == groupResource.mcs_relatedResourceGroupId.Id).ToList();
                        //    foreach (var group in proResourceGroups)
                        //    {
                        //        if (group.cvt_RelatedTSAid != null)
                        //            CvtHelper.CreateUpdateService(group.cvt_RelatedTSAid.Id, Logger, OrganizationService, McsSettings);
                        //        if (group.cvt_RelatedMasterTSAId != null)
                        //            CvtHelper.UpdateMTSA(group.cvt_RelatedMasterTSAId.Id, Guid.Empty, Logger, OrganizationService);
                        //    }
                        //    foreach (var group in patResourceGroups)
                        //    {
                        //        if (group.cvt_RelatedTSAid != null)
                        //            CvtHelper.CreateUpdateService(group.cvt_RelatedTSAid.Id, Logger, OrganizationService, McsSettings);
                        //    }
                        //}
                    }
                }
                else
                    Logger.WriteDebugMessage("No Change was detected in the Resource Group");
            }
            Logger.WriteDebugMessage(String.Format("{0}/{1} Group Resources Updated.", counter, groupResources.Count()));

            if (thisResource.cvt_replaceresourceconnections != null && thisResource.cvt_replaceresourceconnections.Value)
            {
                var updateResource = new mcs_resource() { 
                    Id = thisResource.Id,
                    cvt_replaceresourceconnections = false
                };
                OrganizationService.Update(updateResource);
                Logger.WriteDebugMessage("Replace Resources field reset");
            }
            else if (thisResource.cvt_updateresourceconnections != null && thisResource.cvt_updateresourceconnections.Value)
            {
                var updateResource = new mcs_resource()
                { 
                    Id = thisResource.Id,
                    cvt_updateresourceconnections = false
                };               
                OrganizationService.Update(updateResource);
                Logger.WriteDebugMessage("cvt_updateresourceconnections field reset");
            }
        }

        internal void DeleteResourceConnections(Guid thisId)
        {
            Logger.setMethod = "DeleteResourceConnections";
            Logger.WriteGranularTimingMessage("Starting DeleteResourceConnections");
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    //Going through Pat Site Resources that need to be deleted. 
                    var getPatResources = from patGroup in srv.cvt_patientresourcegroupSet
                                          where patGroup.cvt_RelatedResourceId.Id == thisId
                                          where patGroup.statecode == 0
                                          select new
                                          {
                                              patGroup.Id,
                                              patGroup.cvt_RelatedTSAid
                                          };
                    foreach (var patGroup in getPatResources)
                    {
                        var deletePatGroup = new Entity("cvt_patientresourcegroup") { Id = patGroup.Id };
                        OrganizationService.Delete(deletePatGroup.LogicalName, patGroup.Id);
                        Logger.WriteDebugMessage("Pat Group Deleted");

                        if (patGroup.cvt_RelatedTSAid != null)
                            CvtHelper.CreateUpdateService(patGroup.cvt_RelatedTSAid.Id, Logger, OrganizationService, McsSettings);
             
                    }

                    //Going through Prov Site Resources that need to be deleted
                    var getProvResources = from provGroup in srv.cvt_providerresourcegroupSet
                                           where provGroup.cvt_RelatedResourceId.Id == thisId
                                           where provGroup.statecode == 0
                                           select new
                                           {
                                               provGroup.Id,
                                               provGroup.cvt_RelatedTSAid,
                                               provGroup.cvt_RelatedMasterTSAId
                                           };
                    foreach (var provGroup in getProvResources)
                    {
                        var deleteProvGroup = new Entity("cvt_providerresourcegroup") { Id = provGroup.Id };
                        OrganizationService.Delete(deleteProvGroup.LogicalName, provGroup.Id);
                        Logger.WriteDebugMessage("Prov Group Deleted");

                        if (provGroup.cvt_RelatedTSAid != null)
                            CvtHelper.CreateUpdateService(provGroup.cvt_RelatedTSAid.Id, Logger, OrganizationService, McsSettings);
                        if (provGroup.cvt_RelatedMasterTSAId != null)
                            CvtHelper.UpdateMTSA(provGroup.cvt_RelatedMasterTSAId.Id, Guid.Empty, Logger, OrganizationService);
                    }

                    //Going through Group Resources that need to be deleted
                    var getGroupResources = from groupResource in srv.mcs_groupresourceSet
                                            where groupResource.mcs_RelatedResourceId.Id == thisId
                                            where groupResource.statecode == 0
                                            select new
                                            {
                                                groupResource.Id,
                                                groupResource.mcs_relatedResourceGroupId
                                            };
                    foreach (var groupResource in getGroupResources)
                    {
                        var deleteGroupResource = new Entity("mcs_groupresource") { Id = groupResource.Id };
                        var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == groupResource.mcs_relatedResourceGroupId.Id);
                        OrganizationService.Delete(deleteGroupResource.LogicalName, groupResource.Id);
                        Logger.WriteDebugMessage("Group Resource Deleted");

                        //After Deleting the Group Resource, we need to update the Resource Group.
                        UpdateRG(relatedResourceGroup.Id);
                        Logger.WriteDebugMessage("Resource Group Updated");

                        //now we need to look for Pat / Pro Site Resources related to this Resource Group. And update the TSA's related to them. 
                        //Going through Pat Site Resources that need to be deleted. 
                        var getRGPatResources = from RGpatGroup in srv.cvt_patientresourcegroupSet
                                              where RGpatGroup.cvt_RelatedResourceGroupid.Id == relatedResourceGroup.Id
                                              where RGpatGroup.statecode == 0
                                              select new
                                              {
                                                  RGpatGroup.Id,
                                                  RGpatGroup.cvt_RelatedTSAid
                                              };
                        foreach (var RGpatGroup in getRGPatResources)
                        {
                            if (RGpatGroup.cvt_RelatedTSAid != null)
                                CvtHelper.CreateUpdateService(RGpatGroup.cvt_RelatedTSAid.Id, Logger, OrganizationService, McsSettings);
                        }

                        var getRGProvResources = from RGprovGroup in srv.cvt_providerresourcegroupSet
                                                where RGprovGroup.cvt_RelatedResourceGroupid.Id == relatedResourceGroup.Id
                                                where RGprovGroup.statecode == 0
                                                select new
                                                {
                                                    RGprovGroup.Id,
                                                    RGprovGroup.cvt_RelatedTSAid,
                                                    RGprovGroup.cvt_RelatedMasterTSAId
                                                };
                        foreach (var RGprovGroup in getRGProvResources)
                        {
                            if (RGprovGroup.cvt_RelatedTSAid != null)
                                CvtHelper.CreateUpdateService(RGprovGroup.cvt_RelatedTSAid.Id, Logger, OrganizationService, McsSettings);
                            if (RGprovGroup.cvt_RelatedMasterTSAId != null)
                                CvtHelper.UpdateMTSA(RGprovGroup.cvt_RelatedMasterTSAId.Id, Guid.Empty, Logger, OrganizationService);
                        }
                    }
                    var updateResource = new mcs_resource() 
                    { 
                        Id = thisId,
                        cvt_deleteresourceconnections = false
                    };
                    OrganizationService.Update(updateResource);
                    Logger.WriteDebugMessage("cvt_deleteresourceconnections field updated");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
            Logger.WriteGranularTimingMessage("Ending DeleteResourceConnections");
        }

        internal void UpdateRG(Guid thisId)
        {
            Logger.setMethod = "UpdateRG";
            Logger.WriteDebugMessage("starting UpdateResourceGroup");

            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    var RG = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == thisId);
                    var resourceNames = String.Empty;
                    var count = 0;
                    var resources = CvtHelper.GetResources(thisId, srv, out resourceNames, out count);

                    var builder = CvtHelper.BuildConstraintsXML(resources);
                    var group = new ConstraintBasedGroup
                    {
                        Id = RG.mcs_RelatedResourceGroupId.Id,
                        Constraints = builder.ToString(),
                        Name = RG.mcs_name
                    };
                    Logger.WriteDebugMessage(String.Format("About to Update Constraint Group with {0} resources", count));
                    OrganizationService.Update(group);
                    var thisGroup = new mcs_resourcegroup()
                    {
                        Id = thisId,
                        cvt_resources = resourceNames
                    };
                    //updating resources string field for the resource group with new correct string
                    Logger.WriteDebugMessage("Updated Constraint Group, Updating Resource Group with new resources string");
                    OrganizationService.Update(thisGroup);
                    Logger.WriteDebugMessage("Update Resource Group String Successful");
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
        #endregion

        internal void recurNavigateRules(CalendarRule calRule, Equipment thisEquipment, double effort, int depth)
        {
            depth++;
            if (calRule.Name == "Holiday Closure Link" || calRule.Description == "Holiday Rule")
                return;
            if (effort == 0 || calRule.InnerCalendarId == null)
            {
                calRule.IsSimple = true;
                return;
            }
            calRule.IsSimple = null;

            Entity thisInnerCalendarEntity = OrganizationService.Retrieve("calendar", calRule.InnerCalendarId.Id, new ColumnSet(true));
            EntityCollection thisInnerCalendarRules = (EntityCollection)thisInnerCalendarEntity.Attributes["calendarrules"];
            if (thisInnerCalendarRules.Entities.Count == 0)
            {
                Logger.WriteDebugMessage("No Inner Calendar Rules Found");
                return;
            }
            foreach (CalendarRule thisInnerCalRule in thisInnerCalendarRules.Entities)
            {
                if (thisEquipment.cvt_type.Value == 251920000 && thisEquipment.cvt_capacity != null)
                    thisInnerCalRule.Effort = effort;
                else
                    thisInnerCalRule.Effort = 1;

                if (thisInnerCalRule.TimeZoneCode == null)
                    thisInnerCalRule.TimeZoneCode = thisEquipment.TimeZoneCode;
                if (thisInnerCalRule.Rank == null)
                    thisInnerCalRule.Rank = 0;
                if (thisInnerCalRule.Duration == null)
                    thisInnerCalRule.Duration = 540;
                Logger.WriteDebugMessage(String.Format("Rank: {0}, TimeZone: {1}, Duration: {2}, Name: {3}", thisInnerCalRule.Rank, thisInnerCalRule.TimeZoneCode, thisInnerCalRule.Duration, thisInnerCalRule.Name));
                
                recurNavigateRules(thisInnerCalRule, thisEquipment, effort, depth);
            }
            Logger.WriteDebugMessage("About to Update Inner Calendar " + (depth).ToString());
            OrganizationService.Update(thisInnerCalendarEntity);
            Logger.WriteDebugMessage("updated Inner Calendar");
        }

        internal void ChangeNewlyCreatedCalendarUpdate(Guid sysResourceId)
        {
            Logger.setMethod = "ChangeNewlyCreatedCalendarUpdate";
            Logger.WriteDebugMessage("starting ChangeNewlyCreatedCalendarUpdate");

            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    //var defaultDuration = 540;
                    var thisEquipment = srv.EquipmentSet.FirstOrDefault(e => e.Id == sysResourceId);
                    if (thisEquipment == null)
                        return;

                    var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == thisEquipment.mcs_relatedresource.Id);
                    var thisCalendar = OrganizationService.Retrieve("calendar", thisEquipment.CalendarId.Id, new ColumnSet(true));
                    // Retrieve the calendar rules defined in the calendar
                    EntityCollection thiscalendarRules = (EntityCollection)thisCalendar.Attributes["calendarrules"];
                    foreach (CalendarRule calRule in thiscalendarRules.Entities)
                    {
                        var vistaCapacity = (resource.cvt_vistacapacity == null) ? 1.0 : resource.cvt_vistacapacity.Value;
                        if (thisEquipment.cvt_type.Value == 251920000 && thisEquipment.cvt_capacity != null)
                            calRule.Effort = vistaCapacity;

                        if (calRule.InnerCalendarId == null)
                        {
                            calRule.IsSimple = true;
                            break;
                        }
                        else
                        {
                            calRule.IsSimple = null;

                            recurNavigateRules(calRule, thisEquipment, vistaCapacity, 0);
                        }
                    }
                    Logger.WriteDebugMessage("Updating Equipment Top-Level Calendar");
                    OrganizationService.Update(thisCalendar);
                    Logger.WriteDebugMessage("Calendar Update Complete");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
        }

        private Entity GetPreImage()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion

    }
}