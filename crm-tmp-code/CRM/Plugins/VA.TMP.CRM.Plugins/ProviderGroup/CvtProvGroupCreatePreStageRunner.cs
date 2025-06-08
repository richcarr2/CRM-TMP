using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Purpose:  The purpose of this plugin is to validate the patient group being created.  You cannot create more than 1 patgroup per site/tsa/resource type.
    /// - throw an error is the create shoudn't be allowed.
    ///This is also called from the createResource process if the user hits create single resource from the patgroup grid.
    /// </summary>
    public class CvtProvGroupCreatePreStageRunner : PluginRunner
    {
        public CvtProvGroupCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods/Properties
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_providerresourcegroup.EntityLogicalName)
                throw new Exception("Target entity is not of type cvt_providerresourcegroup");

            var resourceType = McsHelper.getOptionSetValue("cvt_type");
            var provSite = McsHelper.getEntRefName("cvt_relatedsiteid");
            var provSiteId = McsHelper.getEntRefID("cvt_relatedsiteid");                                        
            var TSAResourceType = McsHelper.getOptionSetValue("cvt_tsaresourcetype");

            using (var srv = new Xrm(OrganizationService))
            {
                switch (TSAResourceType)
                {
                    case 2:
                        //Single Provider
                        Logger.WriteDebugMessage("Single Provider- no checks");
                        if (provSiteId == null)
                        {
                            var relatedTelepresenter = srv.SystemUserSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relateduserid"));
                            if (relatedTelepresenter.cvt_site.Id != null)
                            {
                                provSiteId = relatedTelepresenter.cvt_site.Id;
                            }
                        }                           
                        break;
                    case 3:
                        //Single Telepresenter
                        Logger.WriteDebugMessage("Single Telepresenter - no checks");
                        break;
                    case 0:
                        //Resource Group
                        Logger.WriteDebugMessage("Resource Group Checks");
                        if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                        {
                            var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                            resourceType = relatedResourceGroup.mcs_Type.GetHashCode();
                        }
                        if (provSite == null)
                        {
                            var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                            provSiteId = relatedResourceGroup.mcs_relatedSiteId.Id;
                        }
                        switch (resourceType)
                        {
                            case 251920002:
                           
                                Logger.WriteDebugMessage("Technology validation check");
                                if (PrimaryEntity.Attributes.Contains("cvt_relatedtsaid"))
                                {
                                    //Validate that this pat group record is OK
                                    ValidateTechnologyCreate(true, resourceType, provSiteId);
                                }
                                else
                                {
                                    //Validate that this pat group record is OK
                                    ValidateTechnologyCreate(false, resourceType, provSiteId);
                                }
                                break;
                            default:
                                //start the timing for the submethod
                                Logger.WriteGranularTimingMessage("Starting ValidateCreate");

                                if (PrimaryEntity.Attributes.Contains("cvt_relatedtsaid"))
                                {
                                    //Validate that this pat group record is OK
                                    ValidateCreate(true, resourceType, provSiteId);
                                }
                                else
                                {
                                    //Validate that this pat group record is OK
                                    ValidateCreate(false, resourceType, provSiteId);
                                }
                                Logger.WriteGranularTimingMessage("Ending ValidateCreate");
                                //end the timing for the submethod
                                break;
                        }                           
                        break;
                    case 1:
                        //Single Resource
                        Logger.WriteDebugMessage("Single Resource Checks");
                        if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                        {
                            var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                            resourceType = relatedResource.mcs_Type.GetHashCode();
                        }
                        if (provSite == null)
                        {
                            var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                            provSiteId = relatedResource.mcs_RelatedSiteId.Id;
                        }                                                     
                        switch (resourceType)
                        {
                            case 251920002:
                           
                                Logger.WriteDebugMessage("Technology validation check");
                                if (PrimaryEntity.Attributes.Contains("cvt_relatedtsaid"))
                                {
                                    //Validate that this pat group record is OK
                                    ValidateTechnologyCreate(true, resourceType, provSiteId);
                                }
                                else
                                {
                                    //Validate that this pat group record is OK
                                    ValidateTechnologyCreate(false, resourceType, provSiteId);
                                }
                                break;
                            default:
                                //start the timing for the submethod
                                Logger.WriteGranularTimingMessage("Starting ValidateCreate");

                                if (PrimaryEntity.Attributes.Contains("cvt_relatedtsaid"))
                                {
                                    //Validate that this pat group record is OK
                                    ValidateCreate(true, resourceType, provSiteId);
                                }
                                else
                                {
                                    //Validate that this pat group record is OK
                                    ValidateCreate(false, resourceType, provSiteId);
                                }
                                Logger.WriteGranularTimingMessage("Ending ValidateCreate");
                                //end the timing for the submethod
                                break;
                        }                           
                        break;
                }
            }
        }

        internal void ValidateCreate(bool forTSA, int resourceType, Guid provSiteId)
        {
            Logger.WriteDebugMessage("starting ValidateCreate");

            
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "ValidateCreate";

                //For any given patient site, we can only have 1 resource or resrouce group per resource type
                // For example, if the user has a single resource of type room - then no other types can be for room, resource group or single resource.

                //var siteId = McsHelper.getEntRefID("cvt_relatedsiteid");
                var siteId = provSiteId;
                //var thisType = McsHelper.getOptionSetValue("cvt_type");
                var thisType = resourceType;
                
                Logger.WriteDebugMessage("siteId:" + siteId);
                Logger.WriteDebugMessage("thisType:" + thisType);
                if (forTSA)
                {

                    var TSAId = McsHelper.getEntRefID("cvt_relatedtsaid");
                    Logger.WriteDebugMessage("TSAId:" + TSAId);
                    var typeQuery = from psg in srv.cvt_providerresourcegroupSet
                                    where psg.cvt_Type.Value == thisType
                                    where psg.cvt_Type.Value != 917290000
                                    where psg.cvt_relatedsiteid.Id == siteId
                                    where psg.cvt_RelatedTSAid.Id == TSAId 
                                    where psg.statecode == 0
                                    select new
                                    {
                                        psg.cvt_providerresourcegroupId

                                    };
                    foreach (var fts in typeQuery)
                    {
                        Logger.WriteDebugMessage("Site and Type already exist on this TSA");
                        //whereami = "Checking for Associated Provider Site Resource";
                        throw new InvalidPluginExecutionException("customThis Type of Resource for this Site already exists on this TSA, if you want more than 1 of a certain type create a resource group and add that to the TSA");

                    }
                }
                else {
                    var TSAId = McsHelper.getEntRefID("cvt_relatedmastertsaid");                    
                    Logger.WriteDebugMessage("MasterTSAId:" + TSAId);
                    var typeQuery = from psg in srv.cvt_providerresourcegroupSet
                                    where psg.cvt_Type.Value == thisType
                                    where psg.cvt_Type.Value != 917290000
                                    where psg.cvt_relatedsiteid.Id == siteId
                                    where psg.cvt_RelatedMasterTSAId.Id == TSAId 
                                    where psg.statecode == 0
                                    select new
                                    {
                                        psg.cvt_providerresourcegroupId
                                    };
                    foreach (var fts in typeQuery)
                    {
                        Logger.WriteDebugMessage("Site and Type already exist on this Master TSA");
                        //whereami = "Checking for Associated Provider Site Resource";
                        throw new InvalidPluginExecutionException("customThis Type of Resource for this Site already exists on this TSA, if you want more than 1 of a certain type create a resource group and add that to the TSA");

                    }
                }
            }
        }

        internal void ValidateTechnologyCreate(bool forTSA, int resourceType, Guid provSiteId)
        {

            Logger.WriteDebugMessage("starting ValidateTechnologyCreate");
           
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "ValidateCreate";

                //For any given patient site, we can only have 1 resource or resrouce group per resource type
                // For example, if the user has a single resource of type room - then no other types can be for room, resource group or single resource.

                //var siteId = McsHelper.getEntRefID("cvt_relatedsiteid");
                var siteId = provSiteId;
                //var thisType = McsHelper.getOptionSetValue("cvt_type");
                var Name = McsHelper.getStringValue("cvt_name");

                Logger.WriteDebugMessage("siteId:" + siteId);
                //Logger.WriteDebugMessage("thisType:" + thisType);
                Logger.WriteDebugMessage("Name:" + Name);
                if (forTSA)
                {

                    var TSAId = McsHelper.getEntRefID("cvt_relatedtsaid");
                    Logger.WriteDebugMessage("TSAId:" + TSAId);
                    var typeQuery = from tsg in srv.cvt_providerresourcegroupSet
                                    //where psg.cvt_Type.Value == thisType
                                    where tsg.cvt_relatedsiteid.Id == siteId
                                    where tsg.cvt_RelatedTSAid.Id == TSAId
                                    where tsg.cvt_name == Name
                                    where tsg.statecode == 0
                                    select new
                                    {
                                        tsg.cvt_providerresourcegroupId

                                    };
                    foreach (var tts in typeQuery)
                    {
                        Logger.WriteDebugMessage("This Technology Resource already exists on this TSA");
                        //whereami = "Checking for Associated Provider Site Resource";
                        throw new InvalidPluginExecutionException("customThis Technology Resource already exists on this TSA");

                    }
                }
                else
                {
                    var TSAId = McsHelper.getEntRefID("cvt_relatedmastertsaid");
                    Logger.WriteDebugMessage("MasterTSAId:" + TSAId);
                    var typeQuery = from tsg in srv.cvt_providerresourcegroupSet
                                   // where psg.cvt_Type.Value == thisType
                                    where tsg.cvt_relatedsiteid.Id == siteId
                                    where tsg.cvt_RelatedMasterTSAId.Id == TSAId
                                    where tsg.cvt_name == Name
                                    where tsg.statecode == 0
                                    select new
                                    {
                                        tsg.cvt_providerresourcegroupId

                                    };
                    foreach (var tts in typeQuery)
                    {
                        Logger.WriteDebugMessage("This Technology Resource already exists on this Master TSA");
                        //whereami = "Checking for Associated Provider Site Resource";
                        throw new InvalidPluginExecutionException("customThis Technology Resource already exists on this Master TSA");

                    }
                }
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