using System.Linq;
using VA.TMP.DataModel;
using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using MCSShared;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Create the SystemResource record and ResourceSpec and associate with the Patient Group Resource
    /// Needed for Service Scheduling Engine to work
    /// </summary>
    public class CvtPatGroupCreatePostStageRunner : PluginRunner
    {
        public CvtPatGroupCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Business Logic
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_patientresourcegroup.EntityLogicalName)
                throw new Exception("Target entity is not of type cvt_patientresourcegroup");

            Logger.WriteGranularTimingMessage("Starting CreateSystemRecords");
            //since all of the fields we need are marked required on create we only need to do this
            CreateSystemRecords();
            Logger.WriteGranularTimingMessage("Ending CreateSystemRecords");
            //Check Execution Depth
            if (PluginExecutionContext.Depth == 1)
            {
                CvtHelper.CreateUpdateService(McsHelper.getEntRefID("cvt_RelatedTSAid"), Logger, OrganizationService, McsSettings);
            }
        }
       
        internal void CreateSystemRecords()
        {
            Logger.setMethod = "CreateSystemRecords";
            Logger.WriteDebugMessage("starting CreateSystemRecords");
            var whereami = "top";
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {                  
                    //build the top half of the constraints xml
                    var builder = new System.Text.StringBuilder("<Constraints>");
                    builder.Append("<Constraint>");
                    builder.Append("<Expression>");
                    builder.Append("<Body>resource[\"Id\"] == ");
                    //what resource guid we provide is different depending on the TSAResourceType
                    switch (McsHelper.getOptionSetValue("cvt_TSAResourceType"))
                    {
                        case 0: //resource group
                            Logger.WriteDebugMessage("About to get resourceGroup");
                            var resourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                            if (resourceGroup == null) return;
                            //If the System Service Records were already created, no need to create them. 
                            if (McsHelper.getStringValue("cvt_resourcespecguid") != null) return;
                            Logger.WriteDebugMessage("got resourceGroup");
                            builder.Append(resourceGroup.mcs_RelatedResourceGroupId.Id.ToString("B"));
                            break;
                        case 1: //single resource
                            Logger.WriteDebugMessage("About to get resource");
                            var resource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                            if (resource == null) return;
                            //If the System Service Records were already created, no need to create them. 
                            if (McsHelper.getStringValue("cvt_resourcespecguid") != null) return;
                            Logger.WriteDebugMessage("got resource");
                            builder.Append(resource.mcs_relatedResourceId.Id.ToString("B"));
                            break;
                        case 2: //single provider
                            Logger.WriteDebugMessage("About to get Provider");
                            builder.Append(McsHelper.getEntRefID("cvt_relateduserid").ToString("B"));
                            break;
                        case 3: //single person
                            Logger.WriteDebugMessage("About to get Telepresenter");
                            builder.Append(McsHelper.getEntRefID("cvt_relateduserid").ToString("B"));
                            break;
                    }
                    builder.Append("</Body>");
                    builder.Append("<Parameters>");
                    builder.Append("<Parameter name=\"resource\" />");
                    builder.Append("</Parameters>");
                    builder.Append("</Expression>");
                    builder.Append("</Constraint>");
                    builder.Append("</Constraints>");
                    // Define an anonymous type to define the possible constraint based group type code values.
                    var constraintBasedGroupTypeCode = new
                    {
                        Static = 0,
                        Dynamic = 1,
                        Implicit = 2
                    };
                    //we need the user for the business unit
                    Logger.WriteDebugMessage("About to get User");
                    var systemUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == PluginExecutionContext.InitiatingUserId);
                    
                    if (systemUser == null) return;
                    Logger.WriteDebugMessage("got user");

                    var group = new ConstraintBasedGroup
                    {
                        BusinessUnitId = systemUser.BusinessUnitId,
                        Constraints = builder.ToString(),
                        Name = "Selection Rule:" + McsHelper.getStringValue("cvt_name"),
                        GroupTypeCode = new OptionSetValue(constraintBasedGroupTypeCode.Implicit)
                    };

                    var newSysResource = OrganizationService.Create(group);
                    whereami = "created";

                    //now create the resource spec record
                    var spec = new ResourceSpec
                    {
                        BusinessUnitId = systemUser.BusinessUnitId,
                        ObjectiveExpression = @"<Expression><Body>udf ""Random""(factory,resource,appointment,request,leftoffset,rightoffset)</Body><Parameters><Parameter name=""factory"" /><Parameter name=""resource"" /><Parameter name=""appointment"" /><Parameter name=""request"" /><Parameter name=""leftoffset"" /><Parameter name=""rightoffset"" /></Parameters><Properties EvaluationInterval=""P0D"" evaluationcost=""0"" /></Expression>",
                        RequiredCount = 1,
                        Name = "Selection Rule:" + McsHelper.getStringValue("cvt_name"),
                        GroupObjectId = newSysResource,
                        SameSite = true
                    };
                    var _specId = OrganizationService.Create(spec);

                    //update the pat group resource so we can use these values when we create the service.
                    Logger.WriteDebugMessage("About to update Patient Resource Group");
                    var updateResource = new Entity("cvt_patientresourcegroup") { Id = PluginExecutionContext.PrimaryEntityId };

                    Logger.WriteDebugMessage("Looking up Resoure Type");
                     var resourceType = McsHelper.getOptionSetValue("cvt_type");
                     var TSAResourceType = McsHelper.getOptionSetValue("cvt_tsaresourcetype");
                     var patSite = McsHelper.getEntRefName("cvt_relatedsiteid");
                     var patSiteId = McsHelper.getEntRefID("cvt_relatedsiteid");
                     
                    //Single Resource
                     if (TSAResourceType == 1)
                     {
                         if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                         {
                             var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                             updateResource["cvt_type"] = relatedResource.mcs_Type;
                             if (relatedResource.mcs_Type.GetHashCode() == 251920001)
                             {
                                 updateResource["cvt_roomcapacity"] = relatedResource.cvt_capacity;
                             }
                         }
                         if (patSite == null)
                         {                            
                                 Logger.WriteDebugMessage("Looking up the resource's Site");
                                 var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                                 patSiteId = relatedResource.mcs_RelatedSiteId.Id;
                                 Logger.WriteDebugMessage("site:" + patSiteId);
                                 updateResource["cvt_relatedsiteid"] = relatedResource.mcs_RelatedSiteId;
                         }
                     }
                    //Resource Group 
                    if (TSAResourceType == 0)
                     {
                         if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                         {
                             var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                             updateResource["cvt_type"] = relatedResourceGroup.mcs_Type;
                         }
                         if (patSite == null)
                         {
                             Logger.WriteDebugMessage("Looking up the Resource Groups's Site");
                             var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                             patSiteId = relatedResourceGroup.mcs_relatedSiteId.Id;
                             Logger.WriteDebugMessage("Resource Group site:" + patSiteId);
                             updateResource["cvt_relatedsiteid"] = relatedResourceGroup.mcs_relatedSiteId;
                         }
                     }
                     //Telepresenter
                     if (TSAResourceType == 3)
                     {
                         if (patSite == null)
                         {
                             Logger.WriteDebugMessage("Looking up the Telepresenters's Site");
                             var relatedTelepresenter = srv.SystemUserSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relateduserid"));
                             patSiteId = relatedTelepresenter.cvt_site.Id;
                             Logger.WriteDebugMessage("Telepresenter Site:" + patSiteId);
                             updateResource["cvt_relatedsiteid"] = relatedTelepresenter.cvt_site;
                         }
                     }

                    //can't create relationships to these two, so we have to just store the guids in text fields.
                    updateResource["cvt_resourcespecguid"] = _specId.ToString();
                    updateResource["cvt_constraintgroupguid"] = newSysResource.ToString();
                    OrganizationService.Update(updateResource);
                    Logger.WriteDebugMessage("Patient Resource Group Updated");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(whereami + ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(whereami + ex.Message);
                }
            }
        }        
        #endregion

        #region Additional Interface Methods
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