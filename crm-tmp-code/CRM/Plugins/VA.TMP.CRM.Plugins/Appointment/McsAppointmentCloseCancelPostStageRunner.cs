using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Update a related Service Activity to Close or Canceled when this Appointment is Closed or Canceled.
    /// </summary>
    public class McsAppointmentCloseCancelPostStageRunner : PluginRunner
    {
        #region Constructor
        public McsAppointmentCloseCancelPostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods
        public override void Execute()
        {
            //checks to make sure the registration is correct
            //if (PluginExecutionContext.PrimaryEntityId != null)
            //    CloseCancelServiceActivity(PluginExecutionContext.PrimaryEntityId);
        }

        //internal void CloseCancelServiceActivity(Guid thisApptId)
        //{
        //    Logger.setMethod = "CloseCancelServiceActivity";
        //    Logger.WriteDebugMessage("starting Delete Appointment");

        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        Logger.WriteDebugMessage("Looking for related Service Activity.");
        //        var ServiceActivity = srv.ServiceAppointmentSet.FirstOrDefault(i => i.mcs_appointmentid.Id == thisApptId);
        //        var Appointment = srv.AppointmentSet.FirstOrDefault(i => i.Id == thisApptId);

                

        //        if (ServiceActivity == null)
        //            return;
        //        Logger.WriteDebugMessage("Retrieved Service Activity: " + ServiceActivity.Subject);

        //        //Get the StatusCode of this Appt
        //        var StatusCode = Appointment.StatusCode.Value;

        //        //set req to the Service Activity
        //        SetStateRequest req = new SetStateRequest();
        //        req.EntityMoniker = new EntityReference("serviceappointment", ServiceActivity.Id);

        //        if (StatusCode == (int)appointment_statuscode.Completed)
        //        {
        //            req.State = new OptionSetValue((int)ServiceAppointmentState.Closed); 
        //            req.Status = new OptionSetValue((int)serviceappointment_statuscode.Completed);
        //            SetStateResponse reqResponse = (SetStateResponse)OrganizationService.Execute(req);
        //            Logger.WriteDebugMessage("Service Activity Status updated to Closed");
        //        }
        //        if (StatusCode == (int)appointment_statuscode.Canceled)
        //        {
        //            req.State = new OptionSetValue((int)ServiceAppointmentState.Canceled);
        //            req.Status = new OptionSetValue((int)serviceappointment_statuscode.PatientCanceled);
        //            SetStateResponse reqResponse = (SetStateResponse)OrganizationService.Execute(req);
        //            Logger.WriteDebugMessage("Service Activity Status updated to Canceled");
        //        }
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
            if (PluginExecutionContext.InputParameters.Contains("Target"))
                return (Entity)PluginExecutionContext.InputParameters["Target"];
            else
            {
                var PrimaryReference = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];
                return new Entity(PrimaryReference.LogicalName) { Id = PrimaryReference.Id };
            }
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}