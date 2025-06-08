using MCSShared;
using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Assigns the ownership of various records
    /// </summary>
    public class AssignRecordCreatePostStageRunner : PluginRunner
    {
        public AssignRecordCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        /// <summary>
        /// Validates the record and then assigns the owner
        /// </summary>
        public override void Execute()
        {
            var ThisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, PrimaryEntity.LogicalName, Logger, OrganizationService);
            CvtHelper.AssignOwner(ThisRecord, Logger, OrganizationService);
        }
       
        #region Additional Interface Methods
        /// <summary>
        /// Debug field
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_assignment"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        #endregion
    }
}