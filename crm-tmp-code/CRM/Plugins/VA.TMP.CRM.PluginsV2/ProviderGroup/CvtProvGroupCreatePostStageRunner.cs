using System.Linq;
using VA.TMP.DataModel;
using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using MCSShared;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Create the SystemResource record and ResourceSpec and associate with the Provider Group Resource
    /// Needed for Service Scheduling Engine to work
    /// </summary>
    public class CvtProvGroupCreatePostStageRunner : PluginRunner
    {
        public CvtProvGroupCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods/Properties
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_providerresourcegroup.EntityLogicalName)
                throw new Exception("Target entity is not of type cvt_providerresourcegroup");

            Logger.WriteGranularTimingMessage("Starting CreateSystemRecords");
            //since all of the fields we need are marked required on create we only need to do this
            CreateSystemRecords();
            Logger.WriteGranularTimingMessage("Ending CreateSystemRecords");
            //Check Execution Depth
            if (PluginExecutionContext.Depth == 1)
            {
                CvtHelper.UpdateMTSA(McsHelper.getEntRefID("cvt_RelatedMasterTSAId"), Guid.Empty, Logger, OrganizationService);
                CvtHelper.CreateUpdateService(McsHelper.getEntRefID("cvt_RelatedTSAid"), Logger, OrganizationService, McsSettings);
            }
            Logger.WriteDebugMessage("PRG Create: About to check if Provider was added to a TSA.");
            McsGroupResourceCreatePostStageRunner.sendEmailIfProvider(Guid.Empty, PrimaryEntity.Id, OrganizationService, Logger);
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
                            builder.Append(McsHelper.getEntRefID("cvt_RelatedUserId").ToString("B"));
                            break;
                        case 3: //single person
                            builder.Append(McsHelper.getEntRefID("cvt_RelatedUserId").ToString("B"));
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

                    if (systemUser == null) 
                        return;

                    Logger.WriteDebugMessage("Got user");
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

                    //update the prov group resource so we can use these values when we create the service.
                    Logger.WriteDebugMessage("About to update Provider Resource Group");
                    var updateResource = new cvt_providerresourcegroup() { Id = PluginExecutionContext.PrimaryEntityId };

                    var resourceType = McsHelper.getOptionSetValue("cvt_type");
                    var provSiteId = McsHelper.getEntRefID("cvt_relatedsiteid");                   
                    var TSAResourceType = McsHelper.getOptionSetValue("cvt_tsaresourcetype");
                  
                    switch (TSAResourceType)
                    {
                        case 1: //Single Resource
                            if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                            {
                                var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                                if (relatedResource != null && relatedResource.mcs_Type != null)
                                {
                                    updateResource.cvt_Type = relatedResource.mcs_Type;
                                    if (relatedResource.mcs_Type.GetHashCode() == 251920001)
                                        updateResource.cvt_roomcapacity = relatedResource.cvt_capacity;
                                }
                            }
                            if (provSiteId == null)
                            {
                                var relatedResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourceid"));
                                if (relatedResource != null && relatedResource.mcs_RelatedSiteId != null)
                                    updateResource.cvt_relatedsiteid = relatedResource.mcs_RelatedSiteId;
                            }
                            break;
                        case 0: //Resource Group
                            if ((resourceType != 251920002) && (resourceType != 251920001) && (resourceType != 251920000))
                            {
                                var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                                if (relatedResourceGroup != null && relatedResourceGroup.mcs_Type != null)
                                    updateResource.cvt_Type = relatedResourceGroup.mcs_Type;
                            }
                            if (provSiteId == null)
                            {
                                var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relatedresourcegroupid"));
                                if (relatedResourceGroup != null && relatedResourceGroup.mcs_relatedSiteId != null)
                                    updateResource.cvt_relatedsiteid = relatedResourceGroup.mcs_relatedSiteId;
                            }
                            break;
                        case 2: //Single Provider
                            if (provSiteId == null)
                            {
                                var relatedTelepresenter = srv.SystemUserSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("cvt_relateduserid"));
                                if (relatedTelepresenter != null && relatedTelepresenter.cvt_site != null)
                                    updateResource.cvt_relatedsiteid = relatedTelepresenter.cvt_site;
                            }
                            break;
                    }

                    updateResource.cvt_resourcespecguid = _specId.ToString();
                    updateResource.cvt_constraintgroupguid = newSysResource.ToString();
                    OrganizationService.Update(updateResource);
                    Logger.WriteDebugMessage("Provider Resource Group Updated");
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

        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_provresourcegroupplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        #endregion
    }
}