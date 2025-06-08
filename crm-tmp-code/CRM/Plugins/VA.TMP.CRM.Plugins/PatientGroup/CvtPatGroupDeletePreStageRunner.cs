using MCSShared;
using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Deprecated: Update the TSA string fields since the Pat Group is being deleted
    /// </summary>
    public class CvtPatGroupDeletePreStageRunner : PluginRunner
    {
        public CvtPatGroupDeletePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods
        public override void Execute()
        {
            //CvtHelper.CheckTSAStatus(McsHelper.getEntRefID("cvt_RelatedTSAid"), Logger, OrganizationService);
            CvtHelper.CreateUpdateService(McsHelper.getEntRefID("cvt_RelatedTSAid"), Logger, OrganizationService, McsSettings);
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