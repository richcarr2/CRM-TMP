using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Delete a related Service Activities when this Appointment is being deleted.
    /// </summary>
    public class McsAppointmentDeletePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsAppointmentDeletePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion
        
        #region Internal Methods
        public override void Execute()
        {
            //DeleteServiceActivity(PluginExecutionContext.PrimaryEntityId);
        }
       
        //internal void DeleteServiceActivity(Guid thisApptId)
        //{
        //    Logger.setMethod = "DeleteServiceActivity";
        //    Logger.WriteDebugMessage("starting DeleteServiceActivity");                     

        //    using (var srv = new Xrm(OrganizationService))
        //    {            
        //        //Look for related Service Activities
        //        var ServiceActivity = srv.ServiceAppointmentSet.FirstOrDefault(i => i.mcs_appointmentid.Id == thisApptId);
        //        if (ServiceActivity == null)
        //            return;
        //        Logger.WriteDebugMessage("Retrieved Service Activity: " + ServiceActivity.Subject);

        //        //Delete the related Service Activity
        //        OrganizationService.Delete(ServiceActivity.LogicalName, ServiceActivity.Id);
        //        Logger.WriteDebugMessage("Service Activity Deleted");
        //    }              
        //}
        #endregion
        
        #region Additional Interface Methods      
        public override string McsSettingsDebugField
        {
            get { return "mcs_appointmentplugin"; }
        }
        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        #endregion
    }
}