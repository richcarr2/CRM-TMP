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
    public class CvtProvGroupUpdatePostStageRunner : PluginRunner
    {
        public CvtProvGroupUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods/Properties
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_providerresourcegroup.EntityLogicalName)
                throw new Exception("Target entity is not of type cvt_providerresourcegroup");

            Logger.WriteDebugMessage("Plugin Depth: " + PluginExecutionContext.Depth);
            if (PluginExecutionContext.Depth == 2)
            {
                Logger.WriteDebugMessage("PRG Update: About to check if Provider was updated on a TSA.");
                McsGroupResourceCreatePostStageRunner.sendEmailIfProvider(Guid.Empty, PrimaryEntity.Id, OrganizationService, Logger);
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