using MCS.ApplicationInsights;
using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// To Create a Service Activity when an Appointment is created.
    /// </summary>
    public class McsAppointmentCreatePostStageRunner : AILogicBase
    {
        #region Constructor
        public McsAppointmentCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods
        public override void ExecuteLogic()
        {
            //CreateServiceActivity(PluginExecutionContext.PrimaryEntityId);  
            CvtHelper.ShareReserveResource(PrimaryEntity.Id, pluginLogger, OrganizationService);
        }
        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_appointmentplugin"; }
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}