using MCSShared;
using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Deprecated: Update the TSA string fields since the Pat Group is being deleted
    /// </summary>
    public class SchedulingResourceDeletePreStageRunner : PluginRunner
    {
        public SchedulingResourceDeletePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods
        public override void Execute()
        {
            CvtHelper.CheckPStatus(McsHelper.getEntRefID("cvt_participatingsite"), Logger, OrganizationService);
        }
        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_patresourcegroupplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        #endregion
    }
}