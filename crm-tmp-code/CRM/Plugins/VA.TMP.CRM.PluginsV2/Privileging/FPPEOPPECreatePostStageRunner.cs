using MCSShared;
using System;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class FPPEOPPECreatePostStageRunner : PluginRunner
    {
        public FPPEOPPECreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for FPPEOPPE Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Assigns the Owner
        /// </summary>
        public override void Execute()
        {
            var ThisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, cvt_qualitycheck.EntityLogicalName, Logger, OrganizationService);
            CvtHelper.AssignOwner(ThisRecord, Logger, OrganizationService);
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