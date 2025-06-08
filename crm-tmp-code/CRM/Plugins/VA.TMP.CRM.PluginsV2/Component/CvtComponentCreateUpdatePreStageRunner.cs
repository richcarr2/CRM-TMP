using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class CvtComponentCreateUpdatePreStageRunner : PluginRunner
    {
        public CvtComponentCreateUpdatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for Component Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Checks the TSS Resource for Technology, sets parent resource ID
        /// </summary>
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_component.EntityLogicalName)
                throw new InvalidPluginExecutionException("Entity is not of Type Component");

            CheckTSSResource();

            //Pre-Stage create, pass values into the actual object will result in the update being passed into the create event (no service call is needed)
            //Copies the Guid from the related TSS Resource to the Parent Resource Identifier field
            PrimaryEntity.Attributes["cvt_parentresourceidentifier"] = ((EntityReference)(PrimaryEntity.Attributes["cvt_relatedresourceid"])).Id.ToString();
            Logger.WriteDebugMessage("Updated ResourceID Guid to " + PrimaryEntity.Attributes["cvt_parentresourceidentifier"]);
        }

        /// <summary>
        /// Validates that Component Type aligns properly with parent resource. 
        /// </summary>
        internal void CheckTSSResource()
        {
            Logger.WriteDebugMessage("Check for parent TSS Resource");
           

            //Field Requirement should prevent logic from entering this branch
            if (!PrimaryEntity.Contains("cvt_relatedresourceid"))
                throw new InvalidPluginExecutionException("customTSS Resource is missing.");

            Guid parentResourceId = ((EntityReference)(PrimaryEntity.Attributes["cvt_relatedresourceid"])).Id;
            mcs_resource parentResource = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName, parentResourceId, new ColumnSet(true));
            if (parentResource.mcs_Type != null && parentResource.mcs_Type.Value != 251920002)
            {
                Logger.WriteDebugMessage("Parent TSS Resource is not Technology, prevented create.");
                throw new InvalidPluginExecutionException("customTSS Resource is not a Technology.");
            }
           
        }
        
        #region Additional Interface Methods/Properties
        /// <summary>
        /// Used for Debugging - turns on or off creation of log records for this particular entity
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_componentplugin"; }
        }
        #endregion
    }
}
