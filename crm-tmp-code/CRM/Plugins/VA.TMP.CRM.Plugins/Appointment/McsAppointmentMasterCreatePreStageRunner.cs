using Microsoft.Xrm.Sdk;
using System;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// If related to SA, then throw error to user preventing
    /// </summary>
    public class McsAppointmentMasterCreatePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsAppointmentMasterCreatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion
        #region Internal Methods
        public override void Execute()
        {
            var thisAppt = PrimaryEntity.ToEntity<RecurringAppointmentMaster>();

            if (thisAppt.cvt_serviceactivityid != null)
                throw new InvalidPluginExecutionException("customPreventing save of record.  Recurring Appointments feature is blocked and no longer allowed. Contact NTTHD for questions.");
        }
        #endregion
        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_appointmentplugin"; }
        }
        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        #endregion
    }
}