using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    ///Purpose:  The purpose of this plugin is to validate the patient group being created.  You cannot create more than 1 patgroup per site/tsa/resource type.
    /// - throw an error is the create shoudn't be allowed.
    ///This is also called from the createResource process if the user hits create single resource from the patgroup grid.
    /// </summary>
    public class CvtPatGroupCreatePreStageRunner : PluginRunner
    {
        public CvtPatGroupCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods/Properties
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_patientresourcegroup.EntityLogicalName)
                throw new Exception("Target entity is not of type cvt_patientresourcegroup");

            var TSAResourceType = McsHelper.getOptionSetValue("cvt_tsaresourcetype");
            var resourceType = McsHelper.getOptionSetValue("cvt_type");
            var patSite = McsHelper.getEntRefName("cvt_relatedsiteid");
            var patSiteId = McsHelper.getEntRefID("cvt_relatedsiteid");
                          
            using (var srv = new Xrm(OrganizationService))
            {                 
                //Single Resource
                if (TSAResourceType == 1)
                {                  
                    if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                    {                     
                        var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                        resourceType = relatedResource.mcs_Type.GetHashCode();                           
                    }
                    if (patSite == null)
                    {
                        Logger.WriteDebugMessage("Looking up the resource's Site");
                        var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                        patSiteId = relatedResource.mcs_RelatedSiteId.Id;
                        Logger.WriteDebugMessage("Resource site:" + patSiteId);
                    }                     
                }

                //Resource Group
                if (TSAResourceType == 0)
                {
                    if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                    {
                        var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                        resourceType = relatedResourceGroup.mcs_Type.GetHashCode();
                    }
                    if (patSite == null)
                    {
                        Logger.WriteDebugMessage("Looking up the Resource Groups's Site");
                        var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                        patSiteId = relatedResourceGroup.mcs_relatedSiteId.Id;
                        Logger.WriteDebugMessage("Resource Group site:" + patSiteId);
                    }  
                }
                //Telepresenter
                if (TSAResourceType == 3)
                {
                    if (patSiteId == null)
                    {
                        Logger.WriteDebugMessage("Looking up the Telepresenters's Site");
                        var relatedTelepresenter = srv.SystemUserSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relateduserid"));

                        if (relatedTelepresenter.cvt_site.Id != null)
                        {
                            patSiteId = relatedTelepresenter.cvt_site.Id;
                        }
                        Logger.WriteDebugMessage("Telepresenter Site:" + patSiteId);
                    }
                }              
            }

            if (resourceType == 251920002)
            {
                Logger.WriteDebugMessage("Technology Duplicate Validation");
                //start the timing for the submethod
                Logger.WriteGranularTimingMessage("Starting ValidateTechnologyCreate");
                ValidateTechnologyCreate(resourceType, patSiteId);
                Logger.WriteGranularTimingMessage("Ending ValidateTechnologyCreate");
                //end the timing for the submethod
            }
            else
            {
                //start the timing for the submethod
                Logger.WriteGranularTimingMessage("Starting ValidateCreate");

                //Validate that this pat group record is OK
                ValidateCreate(resourceType, patSiteId);

                Logger.WriteGranularTimingMessage("Ending ValidateCreate");
                //end the timing for the submethod
            }
        }

        internal void ValidateCreate(int resourceType, Guid patSiteId)
        {
            Logger.WriteDebugMessage("starting ValidateCreate");
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "ValidateCreate";
                //For any given patient site, we can only have 1 resource or resrouce group per resource type
                // For example, if the user has a single resource of type room - then no other types can be for room, resource group or single resource.
                //var siteId = McsHelper.getEntRefID("cvt_relatedsiteid");
                var siteId = patSiteId;
                var TSAId = McsHelper.getEntRefID("cvt_relatedtsaid");
                //var thisType = McsHelper.getOptionSetValue("cvt_type");
                var thisType = resourceType;
                //var Name = McsHelper.getStringValue("cvt_name");

                if (thisType == 917290000) { return; };
                Logger.WriteDebugMessage("siteId:" + siteId);
                Logger.WriteDebugMessage("TSAId:" + TSAId);
                Logger.WriteDebugMessage("thisType:" + thisType);
                //Logger.WriteDebugMessage("Name:" + Name);
                var typeQuery = from psg in srv.cvt_patientresourcegroupSet
                                where psg.cvt_type.Value == thisType
                                where psg.cvt_relatedsiteid.Id == siteId
                                where psg.cvt_RelatedTSAid.Id == TSAId
                                where psg.statecode == 0
                                //where psg.cvt_name == Name
                                select new
                                {
                                    psg.cvt_patientresourcegroupId
                                };
                foreach (var fts in typeQuery)
                {
                    Logger.WriteDebugMessage("Site and Type already exist on this TSA");
                    //whereami = "Checking for Associated Provider Site Resource";
                    throw new InvalidPluginExecutionException("customThis Type of Resource for this Site already exists on this TSA, if you want more than 1 of a certain type create a resource group and add that to the TSA");
                }
            }
        }

        internal void ValidateTechnologyCreate(int resourceType, Guid patSiteId)
        {
            Logger.WriteDebugMessage("starting ValidateTechnologyCreate");
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "ValidateTehnologyCreate";

                //For Resources of type Technology, you can only add it to TSA once. 
                //  var siteId = McsHelper.getEntRefID("cvt_relatedsiteid");
                var siteId = patSiteId;
                var TSAId = McsHelper.getEntRefID("cvt_relatedtsaid");
                 // var thisType = McsHelper.getOptionSetValue("cvt_type");
                var Name = McsHelper.getStringValue("cvt_name");

                Logger.WriteDebugMessage("siteId:" + siteId);
                Logger.WriteDebugMessage("TSAId:" + TSAId);
                //Logger.WriteDebugMessage("thisType:" + thisType);
                Logger.WriteDebugMessage("Name:" + Name);
                var typeQuery = from tsg in srv.cvt_patientresourcegroupSet
                                //where psg.cvt_type.Value == thisType
                                where tsg.cvt_relatedsiteid.Id == siteId
                                where tsg.cvt_RelatedTSAid.Id == TSAId
                                where tsg.cvt_name == Name
                                where tsg.statecode == 0
                                select new
                                {
                                    tsg.cvt_patientresourcegroupId
                                };
                foreach (var tts in typeQuery)
                {
                    Logger.WriteDebugMessage("This Technology already exisits on this TSA");
                    //whereami = "Checking for Associated Provider Site Resource";
                    throw new InvalidPluginExecutionException("customThis Resource of type Technology already exisits on this TSA");
                }
            }
        }

        internal void UpdatePatResource(Guid ThisId)
        {
         using (var srv = new Xrm(OrganizationService))
            {
             var updateResource = new Entity("cvt_patientresourcegroup") { Id = ThisId };
             var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
             updateResource["cvt_type"] = relatedResource.mcs_Type;
             OrganizationService.Update(updateResource);
             Logger.WriteDebugMessage("Patient Resource Group Updated");
            }
        }
        #endregion

        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_patresourcegroupplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        #endregion
    }
}