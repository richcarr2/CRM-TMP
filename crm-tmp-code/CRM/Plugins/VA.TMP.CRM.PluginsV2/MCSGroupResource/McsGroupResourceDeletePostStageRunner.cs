using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsGroupResourceDeletePostStageRunner : PluginRunner
    {
        public override string McsSettingsDebugField
        {
            get { return ""; }
        }

        public McsGroupResourceDeletePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override void Execute()
        {
            if (PluginExecutionContext.Depth > 1)
                return;
            UpdatePSWarning(PluginExecutionContext.PrimaryEntityId);
        }

        internal void UpdatePSWarning(Guid groupResourceId)
        {
            var vistaClinics = new List<cvt_schedulingresource>();

            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var groupResource = GetSecondaryEntity().ToEntity<mcs_groupresource>();
                    var resourceGroup = groupResource != null && groupResource.mcs_relatedResourceGroupId != null
                        ? srv.mcs_resourcegroupSet.FirstOrDefault(rg => rg.Id == groupResource.mcs_relatedResourceGroupId.Id)
                        : null;
                    var schedulingResource = resourceGroup != null
                        ? srv.cvt_schedulingresourceSet.FirstOrDefault(sr => sr.cvt_tmpresourcegroup.Id == resourceGroup.Id)
                        : null;
                    var participatingSite = schedulingResource != null && schedulingResource.cvt_participatingsite != null
                        ? srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == schedulingResource.cvt_participatingsite.Id)
                        : null;

                    if (schedulingResource == null || schedulingResource.Id.Equals(Guid.Empty))
                    {
                        Logger.WriteDebugMessage("Missing Scheduling Resource");
                    }
                    if (participatingSite == null || participatingSite.Id.Equals(Guid.Empty))
                    {
                        Logger.WriteDebugMessage("Missing Participating Site");
                        return;
                    }

                    var canBeScheduled = participatingSite.Contains("cvt_scheduleable") ? participatingSite.cvt_scheduleable : null;

                    Logger.WriteDebugMessage($"Can be scheduled: {canBeScheduled}");
                    Logger.WriteDebugMessage($"Group Resource Type: {groupResource.mcs_Type.Value}");

                    if (schedulingResource.cvt_resourcetype.Value.Equals(917290000))
                    {
                        if (resourceGroup != null)
                        {
                            //Get other group resources associated with the Resource Group
                            var otherGroupResources = srv.mcs_groupresourceSet.Where((gr) => gr.mcs_relatedResourceGroupId.Id == resourceGroup.Id && gr.Id != groupResourceId).ToList();
                            Logger.WriteDebugMessage($"Group Resources found: {otherGroupResources.Count}");

                            if (otherGroupResources != null && !otherGroupResources.Count.Equals(0))
                            {
                                otherGroupResources.ForEach((otherGroupResource) =>
                                {
                                    Logger.WriteDebugMessage($"Group Resource Type: {otherGroupResource.mcs_Type.Value}");

                                    var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == otherGroupResource.mcs_RelatedResourceId.Id);
                                    Logger.WriteDebugMessage($"Resource Type: {resource?.mcs_Type.Value}");

                                    if (resource != null && resource.mcs_Type.Value.Equals(251920000))
                                    {
                                        Logger.WriteDebugMessage("Adding Vista Clinic from Pair Resource Group");
                                        vistaClinics.Add(schedulingResource);
                                    }
                                });
                            }
                        }
                    }

                    Logger.WriteDebugMessage($"Vista Clinics found: {vistaClinics.Count}");
                    if (vistaClinics.Count.Equals(0) && canBeScheduled != null && canBeScheduled.Value == true)
                    {
                        Logger.WriteDebugMessage("Setting Warning Type to 'Missing Vista Clinic'");
                        participatingSite.Attributes["tmp_warningtype"] = new OptionSetValue(917290000);
                        srv.UpdateObject(participatingSite);
                        srv.SaveChanges();
                    }
                    else
                    {
                        var warningType = participatingSite.GetAttributeValue<OptionSetValue>("tmp_warningtype");
                        Logger.WriteDebugMessage($"Warning Type: {warningType?.Value}");
                        //Clear the Warning if Vista Clinics have been added OR Can be Scheduled is No
                        if (warningType != null && warningType.Value > 0 && (vistaClinics.Count > 0 || (canBeScheduled != null && canBeScheduled.Value == false)))
                        {
                            Logger.WriteDebugMessage("Clearing Warning Type");
                            participatingSite["tmp_warningtype"] = null;
                            srv.UpdateObject(participatingSite);
                            srv.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteDebugMessage($"Update Warnings failed due to the following: {e}");
            }
        }

        public override Entity GetPrimaryEntity()
        {
            Logger.WriteDebugMessage("starting GetPrimaryEntity");
            if (PluginExecutionContext.InputParameters.Contains("Target") && PluginExecutionContext.InputParameters["Target"] is Entity)
                return (Entity)PluginExecutionContext.InputParameters["Target"];
            if (PluginExecutionContext.InputParameters.Contains("Target") && PluginExecutionContext.InputParameters["Target"] is EntityReference)
            {
                var entityReference = (EntityReference)PluginExecutionContext.InputParameters["Target"];
                return new Entity(entityReference.LogicalName) { Id = entityReference.Id };
            }
            else if (PluginExecutionContext.InputParameters.Contains("EntityMoniker"))
            {
                var PrimaryReference = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];
                return new Entity(PrimaryReference.LogicalName) { Id = PrimaryReference.Id };
            }
            else
                return new Entity(PluginExecutionContext.PrimaryEntityName);
        }
    }
}
