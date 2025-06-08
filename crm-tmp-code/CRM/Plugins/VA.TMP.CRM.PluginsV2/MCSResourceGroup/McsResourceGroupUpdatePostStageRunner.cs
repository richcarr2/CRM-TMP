using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsResourceGroupUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceGroupUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            var thisRecord = PrimaryEntity.ToEntity<mcs_resourcegroup>();
            //If changing fields contains mcs_name, then change related records
            if (thisRecord.Attributes.Contains("mcs_name"))
            {
                UpdateCBG(thisRecord);
                UpdatePatProvResources(thisRecord);
            }
        }
        #endregion

        /// <summary>
        /// Update the Name of the Out of the Box Constraint Based Group
        /// </summary>
        /// <param name="thisResourceGroup"></param>
        public void UpdateCBG(mcs_resourcegroup thisResourceGroup)
        {
            Logger.WriteDebugMessage("Retrieving TSS Resource Group.");
            var thisGroup = OrganizationService.Retrieve(mcs_resourcegroup.EntityLogicalName, thisResourceGroup.Id, new ColumnSet(true)).ToEntity<mcs_resourcegroup>();

            //Verify that the Resource Group has a cbg
            if (thisGroup != null && thisGroup.mcs_RelatedResourceGroupId != null)
            {
                var cbg = new ConstraintBasedGroup()
                {
                    Id = thisGroup.mcs_RelatedResourceGroupId.Id,
                    Name = thisResourceGroup.mcs_name
                };
                OrganizationService.Update(cbg);
                Logger.WriteDebugMessage("Updated CBG with new name: " + thisResourceGroup.mcs_name);
            }
        }

        /// <summary>
        /// Update the Patient and Provider Side Resource(s)' Names
        /// </summary>
        /// <param name="thisResourceGroup"></param>
        public void UpdatePatProvResources(mcs_resourcegroup thisResourceGroup)
        {
            Logger.WriteDebugMessage("starting UpdatePatProvResources");
            using (var srv = new Xrm(OrganizationService)) {
                //Update Prov and Pat Site Resources
                var patRes = srv.cvt_patientresourcegroupSet.Where(pgr => pgr.cvt_RelatedResourceGroupid.Id == thisResourceGroup.Id);
                var provRes = srv.cvt_providerresourcegroupSet.Where(pgr => pgr.cvt_RelatedResourceGroupid.Id == thisResourceGroup.Id);

                foreach (cvt_patientresourcegroup prg in patRes) {
                    cvt_patientresourcegroup updatePRG = new cvt_patientresourcegroup() {
                        Id = prg.Id,
                        cvt_name = thisResourceGroup.mcs_name
                    };
                    try
                    {
                        OrganizationService.Update(updatePRG);
                        Logger.WriteDebugMessage("Updated Patient Resource Group");
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(String.Format("Failed to update Patient Resource Group: {0}. Error: {1}", prg.cvt_name, ex.Message));
                    }
                }

                foreach (cvt_providerresourcegroup prg in provRes) {
                    cvt_providerresourcegroup updatePRG = new cvt_providerresourcegroup() {
                        Id = prg.Id,
                        cvt_name = thisResourceGroup.mcs_name
                    };
                    try
                    {
                        OrganizationService.Update(updatePRG);
                        Logger.WriteDebugMessage("Updated Provider Resource Group");
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(String.Format("Failed to update Provider Resource Group: {0}. Error: {1}", prg.cvt_name, ex.Message));
                    }
                }
            }
        }
        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourcegroupplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}