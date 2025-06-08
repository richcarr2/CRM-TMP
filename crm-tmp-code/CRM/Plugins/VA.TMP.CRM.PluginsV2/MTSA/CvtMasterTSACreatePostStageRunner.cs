using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class CvtMasterTSACreatePostStageRunner : PluginRunner
    {
        public CvtMasterTSACreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override void Execute()
        {           
            var ThisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, cvt_mastertsa.EntityLogicalName, Logger, OrganizationService);
            CvtHelper.AssignOwner(ThisRecord, Logger, OrganizationService);
        }
       
        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_mastertsaplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        #endregion
    }
}