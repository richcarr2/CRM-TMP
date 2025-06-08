using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Update the Group Resource, the Constraint Based Group and the TSS Resource Group
    /// </summary>
    public class McsGroupResourceCreatePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsGroupResourceCreatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        public override void Execute()
        {
            PreventCreateResourceGroup(PluginExecutionContext.PrimaryEntityId);
        }

        #region Logic
        private void PreventCreateResourceGroup(Guid primaryEntityId)
        {
            Logger.setMethod = "PreventCreateResourceGroup";
            Logger.WriteDebugMessage("starting PreventCreateResourceGroup");

            try
            {
                var thisGroupResource = PrimaryEntity.ToEntity<mcs_groupresource>();

                using (var srv = new Xrm(OrganizationService))
                {
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

                    if (canBeScheduled.HasValue && canBeScheduled.Value == true)
                    {
                        throw new InvalidPluginExecutionException("Scheduling Resource cannot be added to a 'Can Be Scheduled' Participating Site. Change this Participating Site to NO, save it, and try again.");
                    }
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.WriteDebugMessage($"Failed to create Group Resource due to the following error: {e}");
            }
        }
        #endregion
        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_groupresourceplugin"; }
        }
        #endregion
    }
}