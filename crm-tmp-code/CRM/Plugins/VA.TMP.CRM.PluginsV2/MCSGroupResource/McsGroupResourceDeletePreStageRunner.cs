using VA.TMP.DataModel;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Collections.Generic;

namespace VA.TMP.CRM
{
    public class McsGroupResourceDeletePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsGroupResourceDeletePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        public override void Execute()
        {
            if (PluginExecutionContext.Depth > 1)
                return;
            PreventDeleteResourceGroup(PluginExecutionContext.PrimaryEntityId);
            UpdateResourceGroup(PluginExecutionContext.PrimaryEntityId);
        }

        private void PreventDeleteResourceGroup(Guid primaryEntityId)
        {
            Logger.setMethod = "PreventDeleteResourceGroup";
            Logger.WriteDebugMessage("starting PreventDeleteResourceGroup");

            using (var srv = new Xrm(OrganizationService))
            {
                var thisGroupResource = srv.mcs_groupresourceSet.FirstOrDefault(i => i.Id == primaryEntityId);
                //First check to see if Related Resource Group is related to Pat/Prov Site Resources. 
                var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == thisGroupResource.mcs_relatedResourceGroupId.Id);
                var schedulingResource = relatedResourceGroup != null && !relatedResourceGroup.Id.Equals(Guid.Empty)
                    ? srv.cvt_schedulingresourceSet.FirstOrDefault(sr => sr.cvt_tmpresourcegroup.Id == relatedResourceGroup.Id)
                    : null;
                var participatingSite = schedulingResource != null && !schedulingResource.Id.Equals(Guid.Empty)
                    ? srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == schedulingResource.cvt_participatingsite.Id)
                    : null;

                var canBeScheduled = participatingSite != null && !participatingSite.Id.Equals(Guid.Empty)
                    ? participatingSite.cvt_scheduleable
                    : new Nullable<bool>();

                if(canBeScheduled.HasValue && canBeScheduled.Value == true)
                {
                    throw new InvalidPluginExecutionException("The Participating Site cannot be in To Be Scheduled status for the Resource to be deleted. Change the Participating Site, Save and then delete the Resource.");
                }
            }
        }

        #region Logic
        internal void UpdateResourceGroup(Guid thisId)
        {
            Logger.setMethod = "UpdateResourceGroup";
            Logger.WriteDebugMessage("starting UpdateResourceGroup");

            using (var srv = new Xrm(OrganizationService))
            {
                var thisGroupResource = srv.mcs_groupresourceSet.FirstOrDefault(i => i.Id == thisId);
                //First check to see if Related Resource Group is related to Pat/Prov Site Resources. 
                var RelatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == thisGroupResource.mcs_relatedResourceGroupId.Id);

                //Checking to see if any Patient Site Resources have this target MCS Group Resource / Resource Group associated with it. 
                var patResource = srv.cvt_patientresourcegroupSet.FirstOrDefault(i => i.cvt_RelatedResourceGroupid.Id == RelatedResourceGroup.Id);
                //If an associated Patient Site Resource does exist, we will throw an exception with a message and stop the delete of the MCS Resource, to prevent orphan data. 
                if (patResource != null)
                {
                    Logger.WriteDebugMessage("Patient Resource Exists:" + patResource.cvt_name);
                    throw new InvalidPluginExecutionException("customPlease check the related Resource Group view or the Related TSA subgrid below for Patient Site Resources that the related Resource Group is associated with. Resource cannot be deleted until associations are removed.");
                }
                //Checking to see if any Provider Site Resources have this target MCS Group Resource / Resource Group associated with it. 
                var proResource = srv.cvt_providerresourcegroupSet.FirstOrDefault(i => i.cvt_RelatedResourceGroupid.Id == RelatedResourceGroup.Id);
                //If an associated Provider Site Resource does exist, we will throw an exception with a message and stop the delete of the MCS Resource, to prevent orphan data.
                if (proResource != null)
                {
                    Logger.WriteDebugMessage("Provider Resource Exists:" + proResource.cvt_name);
                    throw new InvalidPluginExecutionException("customPlease check the related Resource Group view or the Related TSA subgrid below for Provider Site Resources that the related Resource Group is associated with. Resource cannot be deleted until associations are removed.");
                }

                var builder = new System.Text.StringBuilder("<Constraints><Constraint><Expression>");
                var getResources = from resGroups in srv.mcs_groupresourceSet
                                   join mcsResourcs in srv.mcs_resourceSet on resGroups.mcs_RelatedResourceId.Id equals mcsResourcs.mcs_resourceId.Value
                                   where resGroups.mcs_relatedResourceGroupId.Id == RelatedResourceGroup.Id && resGroups.statecode == 0
                                   select new
                                   {
                                       mcsResourcs.mcs_name,
                                       mcsResourcs.mcs_relatedResourceId,
                                       resGroups.mcs_groupresourceId
                                   };

                string resourceString = null;
                var count = 0;
                foreach (var mcsResource in getResources)
                {
                    if (mcsResource.mcs_groupresourceId.Value != thisId)
                    {
                        builder = McsGroupResourceCreatePostStageRunner.buildFunction(mcsResource.mcs_relatedResourceId, resourceString, builder, count, out count, out resourceString);
                    }
                }

                var getUsers = from resGroups in srv.mcs_groupresourceSet
                               join Users in srv.SystemUserSet on resGroups.mcs_RelatedUserId.Id equals Users.Id
                               where resGroups.mcs_relatedResourceGroupId.Id == RelatedResourceGroup.Id && resGroups.statecode == 0
                               select new
                               {
                                   resGroups.mcs_RelatedUserId,
                                   resGroups.mcs_name,
                                   resGroups.mcs_groupresourceId
                               };

                foreach (var User in getUsers)
                {
                    if (User.mcs_groupresourceId.Value != thisId)
                    {
                        builder = McsGroupResourceCreatePostStageRunner.buildFunction(User.mcs_RelatedUserId, resourceString, builder, count, out count, out resourceString);
                    }
                }

                if (count == 0)
                    builder.Append("<Body>false");

                builder.Append("</Body><Parameters><Parameter name=\"resource\" /></Parameters></Expression></Constraint></Constraints>");

                Logger.WriteDebugMessage("About to get mcs_resourcegroup");
                var tssResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == RelatedResourceGroup.Id);
                if (tssResourceGroup == null)
                    return;
                Logger.WriteDebugMessage("got mcs_resourcegroup");
                if (tssResourceGroup.mcs_RelatedResourceGroupId != null)
                {
                    var group = new ConstraintBasedGroup
                    {
                        Id = tssResourceGroup.mcs_RelatedResourceGroupId.Id,
                        Constraints = builder.ToString()
                    };
                    var updateRG = new mcs_resourcegroup
                    {
                        Id = tssResourceGroup.Id,
                        cvt_resources = resourceString
                    };

                    OrganizationService.Update(updateRG);
                    OrganizationService.Update(group);
                    Logger.WriteDebugMessage("ConstraintBasedGroup and TSS Resource Group updated with " + count + " resources.");
                }
            }
        }

        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_groupresourceplugin"; }
        }
        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        #endregion
    }
}