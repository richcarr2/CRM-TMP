using MCSShared;
using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Deprecated: Update the MTSA or TSA string fields since the Prov Group is being deleted
    /// </summary>
    public class CvtProvGroupDeletePreStageRunner : PluginRunner
    {
        public CvtProvGroupDeletePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods
        public override void Execute()
        {
            Logger.WriteDebugMessage("Deprecated Methods in CvtProvGroupDeletePreStageRunner");
            //CvtHelper.CheckTSAStatus(McsHelper.getEntRefID("cvt_RelatedTSAid"), Logger, OrganizationService);
            //CvtHelper.UpdateMTSA(McsHelper.getEntRefID("cvt_RelatedMasterTSAId"), PluginExecutionContext.PrimaryEntityId, Logger, OrganizationService);
            //CvtHelper.CreateUpdateService(McsHelper.getEntRefID("cvt_RelatedTSAid"), Logger, OrganizationService, McsSettings);
        }
        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_provresourcegroupplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }

        #endregion
    }
}