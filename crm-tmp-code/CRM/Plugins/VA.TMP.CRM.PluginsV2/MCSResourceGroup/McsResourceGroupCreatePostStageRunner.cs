using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using MCSUtilities2011;

namespace VA.TMP.CRM
{
    public class McsResourceGroupCreatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceGroupCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        
        public override void Execute()
        {

            Logger.WriteDebugMessage("ExecutionDepth = " + PluginExecutionContext.Depth);

            var thisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, mcs_resourcegroup.EntityLogicalName, Logger, OrganizationService);
            var thisResGrRecord = (mcs_resourcegroup)thisRecord;

            CreateSysGroupResource(thisResGrRecord);

            //TSA or MTSA guid exists then it is because they are doing a quick create
            var doQuickCreate = McsHelper.getStringValue("mcs_tsaguid") != null ? true : false;
            doQuickCreate = (doQuickCreate != true && McsHelper.getStringValue("cvt_mastertsaguid") != null) ? true : false;

            //Determine if Pat or Prov (fields are populated from jscript on a ribbon button)
            var quickCreatePat = McsHelper.getBoolValue("mcs_createpatientrg");
            var quickCreateProv = McsHelper.getBoolValue("mcs_createproviderrg");

            if (doQuickCreate)
            {
                Logger.WriteDebugMessage("About to do a quick create.");
                if (quickCreatePat)
                    QuickCreatePatientGroupResource(PluginExecutionContext.PrimaryEntityId);
                else if (quickCreateProv)
                    QuickCreateProviderGroupResource(PluginExecutionContext.PrimaryEntityId);
            }
            else
                Logger.WriteDebugMessage("Do not need to do a quick create for Resource Group.");
            updateRecordName(thisResGrRecord, Logger, OrganizationService);
        }

        #region Internal Methods/Properties
        internal static void updateRecordName(mcs_resourcegroup thisRecord, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            var derivedName = CvtHelper.ReturnRecordNameIfChanged((mcs_resourcegroup)thisRecord, true, Logger, OrganizationService);
            if (derivedName != thisRecord.mcs_name && !String.IsNullOrEmpty(derivedName))
            {
                mcs_resourcegroup updateRG = new mcs_resourcegroup()
                {
                    Id = thisRecord.Id,
                    mcs_name = derivedName
                };
                Logger.WriteDebugMessage("The TSS Resource Group name should be different, updating it now: " + derivedName + ".");
                OrganizationService.Update(updateRG);
            }
        }
        /// <summary>
        /// Create a Patient Group Resource record related to the TSA
        /// </summary>
        /// <param name="primaryEntityId"></param>
        internal void QuickCreatePatientGroupResource(Guid primaryEntityId)
        {
            try
            {
                Logger.setMethod = "QuickCreatePatientGroupResource";
                Logger.WriteDebugMessage("Starting Method");

                cvt_patientresourcegroup newGroupResource = new cvt_patientresourcegroup()
                {
                    cvt_TSAResourceType = new OptionSetValue(0),
                    cvt_type = new OptionSetValue(McsHelper.getOptionSetValue("mcs_type")),
                    cvt_name = McsHelper.getStringValue("mcs_name"),
                    cvt_relatedsiteid = new EntityReference(mcs_site.EntityLogicalName, McsHelper.getEntRefID("mcs_relatedsiteid")),
                    cvt_RelatedResourceGroupid = new EntityReference(mcs_resourcegroup.EntityLogicalName, primaryEntityId)
                };

                var tsapresent = McsHelper.getStringValue("mcs_tsaguid");
                if (tsapresent != null)
                    newGroupResource.cvt_RelatedTSAid = new EntityReference(cvt_facilityapproval.EntityLogicalName, new Guid(tsapresent));

                OrganizationService.Create(newGroupResource);
                Logger.WriteDebugMessage("Created patient resource group record");
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

        /// <summary>
        /// Create a Provider Group Resource record related to the TSA
        /// </summary>
        /// <param name="primaryEntityId"></param>
        internal void QuickCreateProviderGroupResource(Guid primaryEntityId)
        {
            try
            {
                Logger.setMethod = "QuickCreateProviderGroupResource";
                Logger.WriteDebugMessage("Starting Method");

                cvt_providerresourcegroup newGroupResource = new cvt_providerresourcegroup()
                {
                    cvt_TSAResourceType = new OptionSetValue(0),
                    cvt_Type = new OptionSetValue(McsHelper.getOptionSetValue("mcs_type")),
                    cvt_name = McsHelper.getStringValue("mcs_name"),
                    cvt_relatedsiteid = new EntityReference(mcs_site.EntityLogicalName, McsHelper.getEntRefID("mcs_relatedsiteid")),
                    cvt_RelatedResourceGroupid = new EntityReference(mcs_resourcegroup.EntityLogicalName, primaryEntityId)
                };
                
                var mastertsapresent = McsHelper.getStringValue("cvt_mastertsaguid");
                if (mastertsapresent != null)
                    newGroupResource.cvt_RelatedMasterTSAId = new EntityReference(cvt_mastertsa.EntityLogicalName, new Guid(mastertsapresent));

                var tsapresent = McsHelper.getStringValue("mcs_tsaguid");
                if (tsapresent != null)
                    newGroupResource.cvt_RelatedTSAid = new EntityReference(cvt_facilityapproval.EntityLogicalName, new Guid(tsapresent));
                
                OrganizationService.Create(newGroupResource);
                Logger.WriteDebugMessage("Created provider resource group record");
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

        /// <summary>
        /// Create System Group Resource record and Update this record
        /// </summary>
        /// <param name="thisResourceGroup"></param>
        internal void CreateSysGroupResource(mcs_resourcegroup thisResourceGroup)
        {
            Logger.setMethod = "CreateSysGroupResource";
            Logger.WriteDebugMessage("starting CreateSysGroupResource");

         
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {

                    var thisRG = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == thisResourceGroup.Id);

                    System.Text.StringBuilder builder = new System.Text.StringBuilder("<Constraints><Constraint><Expression>");
                    builder.Append("<Body>resource[\"Id\"] == ");
                    builder.Append(thisResourceGroup.Id.ToString("B"));
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

                    Logger.WriteDebugMessage("About to get Site Team");

                    var siteTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisResourceGroup.mcs_relatedSiteId.Name);

                    Logger.WriteDebugMessage("Got Site Team");

                    if (siteTeam == null)
                    {
                        Logger.WriteDebugMessage("No Team found for TSS Resource Group's site. Check for site team. SITE: " + thisResourceGroup.mcs_relatedSiteId.Name);
                        return;
                    }
                    int GroupTypeCode;
                    var RGType = thisRG.mcs_Type.Value;
                    if (RGType == 917290000)
                        GroupTypeCode = constraintBasedGroupTypeCode.Static;
                    else
                        GroupTypeCode = constraintBasedGroupTypeCode.Dynamic;

                  
                    Logger.WriteDebugMessage("Getting mcs_name 1");

                    var mcs_name = McsHelper.getStringValue("mcs_name");

                    Logger.WriteDebugMessage("Creating Constraint Based Group");

                    var group = new ConstraintBasedGroup
                    {
                        BusinessUnitId = siteTeam.BusinessUnitId,
                        Name = mcs_name,
                        Constraints = builder.ToString(),
                        GroupTypeCode = new OptionSetValue(GroupTypeCode)
                    };

                    Logger.WriteDebugMessage("Created Constraint Based Group");

                    Logger.WriteDebugMessage("Adding System Resource");

                    var newSysResource = OrganizationService.Create(group);

                    Logger.WriteDebugMessage("System Resource Added");

                    var selectiongroup = new ConstraintBasedGroup
                    {
                        BusinessUnitId = siteTeam.BusinessUnitId,
                        Constraints = builder.ToString(),
                        Name = "Selection Rule:" + McsHelper.getStringValue("cvt_name"),
                        GroupTypeCode = new OptionSetValue(constraintBasedGroupTypeCode.Implicit)
                    };
                    var newSelectionGroup = OrganizationService.Create(selectiongroup);
                    Logger.WriteDebugMessage("Selection Group Added");

                    int reqCount;
                    if (RGType == 917290000)
                        reqCount = -1;
                    else
                        reqCount = 1;

                    Logger.WriteDebugMessage("Getting mcs_name 2");

                    mcs_name = McsHelper.getStringValue("mcs_name");

                    var spec = new ResourceSpec
                    {
                        BusinessUnitId = siteTeam.BusinessUnitId,
                        ObjectiveExpression = @"<Expression><Body>udf ""Random""(factory,resource,appointment,request,leftoffset,rightoffset)</Body><Parameters><Parameter name=""factory"" /><Parameter name=""resource"" /><Parameter name=""appointment"" /><Parameter name=""request"" /><Parameter name=""leftoffset"" /><Parameter name=""rightoffset"" /></Parameters><Properties EvaluationInterval=""P0D"" evaluationcost=""0"" /></Expression>",
                        RequiredCount = reqCount,
                        Name = "Selection Rule:" + mcs_name,
                        GroupObjectId = newSysResource,
                        SameSite = true
                    };

                    Logger.WriteDebugMessage("Creating ResourceSpec");

                    var _specId = OrganizationService.Create(spec);

                    Logger.setMethod = "Update MCS Resource Group";
                    Logger.WriteDebugMessage("About to update MCS Resource Group");
                    var updateResourceGroup = new mcs_resourcegroup()
                    {
                        Id = thisResourceGroup.Id,
                        mcs_RelatedResourceGroupId = new EntityReference(ConstraintBasedGroup.EntityLogicalName, newSysResource),
                        mcs_constraintgroupguid = newSelectionGroup.ToString(),
                        mcs_resourcespecguid = _specId.ToString()
                    };

                    OrganizationService.Update(updateResourceGroup);
                    Logger.WriteDebugMessage("MCS Resource Group Updated");

                    AssignRequest assignRequest = new AssignRequest()
                    {
                        Assignee = new EntityReference(Team.EntityLogicalName, siteTeam.Id),
                        Target = new EntityReference(mcs_resourcegroup.EntityLogicalName, thisResourceGroup.Id)
                    };

                    OrganizationService.Execute(assignRequest);
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

        #region Additional Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourcegroupplugin"; }
        }
        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}