using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsResourceGroupDeletePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceGroupDeletePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            DeleteResourceGroup(PluginExecutionContext.PrimaryEntityId);
        }

        /// <summary>
        /// Starting Validation Checks before deleting the MCS Resource Group & System Resource Group. 
        /// </summary>
        /// <param name="thisId"></param>
        internal void DeleteResourceGroup(Guid thisId)
        {
            Logger.setMethod = "Delete System Resource Group";
            Logger.WriteDebugMessage("Starting Delete System Resource Group");
           
            using (var srv = new Xrm(OrganizationService))
            {
                
                //Checking to see if a System Resource Group Exists for the target MCS Resource Group being deleted. 
                var systemRG = srv.ResourceGroupSet.FirstOrDefault(i => i.Id == McsHelper.getEntRefID("mcs_relatedresourcegroupid"));
                //If a System Resource Group does not exist, we will return because the Plugin will have nothing to delete. 
                if (systemRG == null)
                    return;
                Logger.WriteDebugMessage("Got System Resource(facility/equip):" + systemRG.Name);

                //Checking to see if any Patient Site Resource Group have this target MCS Resource Group associated with it.
                var patResource = srv.cvt_patientresourcegroupSet.FirstOrDefault(i => i.cvt_RelatedResourceGroupid.Id == thisId);                                  
                //If an associated Patient Site Resource Group does exist, we will throw an exception with a message and stop the delete of the MCS Resource Group, to prevent orphan data. 
                if (patResource != null)
                {
                    Logger.WriteDebugMessage("Patient Resource Exists:" + patResource.cvt_name);                                  
                    throw new InvalidPluginExecutionException("customPlease check Left Nav on Resource Group form for Patient Site Resources that this Resource Group is associated with. Resource Group cannot be deleted until associations are removed.");                      
                }
                //Checking to see if any Provider Site Resource Groups have this target MCS Resource associated with it. 
                var proResource = srv.cvt_providerresourcegroupSet.FirstOrDefault(i => i.cvt_RelatedResourceGroupid.Id == thisId);
                //If an associated Provider Site Resource Group does exist, we will throw an exception with a message and stop the delete of the MCS Resource Group, to prevent orphan data.
                if (proResource != null)
                {
                    Logger.WriteDebugMessage("Provider Resource Exists:" + proResource.cvt_name);                                     
                    throw new InvalidPluginExecutionException("customPlease check Left Nav on Resource Group form for Provider Site Resources that this Resource Group is associated with. Resource Group cannot be deleted until associations are removed.");
                }
                //Checking to see if any Group Resources have this target MCS Resource Group associated with it.
                var groupResource = srv.mcs_groupresourceSet.FirstOrDefault(i => i.mcs_relatedResourceGroupId.Id == thisId);
                //If an associated Group Resource does exist, we will throw an exception with a message and stop the delete of the MCS Resource Group, to prevent orphan data.
                if (groupResource != null)
                {
                    Logger.WriteDebugMessage("Group Resource Exists:" + groupResource.mcs_name);                                    
                    throw new InvalidPluginExecutionException("customPlease check Left Nav on Resource Group form for Group Resources that this Resource Group is associated with. Resource Group cannot be deleted until associations are removed.");
                }
                
                //If no exceptions are thrown from validation checks, we will actually delete the System Resource associated with the MCS Resource.               
                //CRM delete message does not support delete of system resource group. 
                // OrganizationService.Delete(systemRG.LogicalName, systemRG.Id);
                // Logger.WriteDebugMessage("System Resource Deleted");
            }
        }

        public override string McsSettingsDebugField
        {
            get { return "mcs_resourcegroupplugin"; }
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
