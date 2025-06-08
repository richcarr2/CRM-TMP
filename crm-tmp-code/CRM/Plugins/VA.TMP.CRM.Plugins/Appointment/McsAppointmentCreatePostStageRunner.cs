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
    public class McsAppointmentCreatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsAppointmentCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods
        public override void Execute()
        {
            //CreateServiceActivity(PluginExecutionContext.PrimaryEntityId);  
            CvtHelper.ShareReserveResource(PrimaryEntity.Id, Logger, OrganizationService);
        }

        //internal void CreateServiceActivity(Guid thisApptId)
        //{
        //    Logger.setMethod = "CreateServiceActivity";
        //    Logger.WriteDebugMessage("starting CreateServiceActivity");

        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        //Get this Appt
        //        var thisAppt = srv.AppointmentSet.FirstOrDefault(i => i.Id == thisApptId);

        //        if ((thisAppt.cvt_serviceactivityid == null) || (thisAppt.SeriesId == null))
        //        {
        //            Logger.WriteDebugMessage("Exiting SA Recurrence Generation, no Service Activity present or not part of a Recurring Appointment");
        //            return;
        //        }

        //        //Get Service Activity
        //        var retrieveServiceActivity = srv.ServiceAppointmentSet.FirstOrDefault(i => i.Id == thisAppt.cvt_serviceactivityid.Id);

        //        //Check if SA actually exists
        //        if (retrieveServiceActivity == null)
        //            return;
        //        Logger.WriteDebugMessage("starting Service Activity Recurrence, since recurring SA present");
        //        //Remove All Resources from this Appointment
        //        var updateAppt = new Appointment()
        //        {
        //            Id = thisAppt.Id,
        //            RequiredAttendees = null
        //        };
        //        OrganizationService.Update(updateAppt);
        //        Logger.WriteDebugMessage("Removed Resources from Appointment");

        //        //Here we are defining the attributes to be carried over to the new Service Activities created. Each is created for every Appointment created. 
        //        ServiceAppointment addServiceActivity = new ServiceAppointment();
        //        Logger.WriteDebugMessage("Defining Service Activity Attributes");
        //        addServiceActivity.Subject = retrieveServiceActivity.Subject;
        //        addServiceActivity.cvt_openappointment = retrieveServiceActivity.cvt_openappointment;
        //        addServiceActivity.mcs_groupappointment = retrieveServiceActivity.mcs_groupappointment;
        //        addServiceActivity.mcs_relatedtsa = retrieveServiceActivity.mcs_relatedtsa;
        //        addServiceActivity.mcs_relatedsite = retrieveServiceActivity.mcs_relatedsite;
        //        addServiceActivity.mcs_relatedprovidersite = retrieveServiceActivity.mcs_relatedprovidersite;
        //        addServiceActivity.mcs_servicetype = retrieveServiceActivity.mcs_servicetype;
        //        addServiceActivity.mcs_servicesubtype = retrieveServiceActivity.mcs_servicesubtype;
        //        addServiceActivity.ServiceId = retrieveServiceActivity.ServiceId;
        //        addServiceActivity.cvt_recurrence = true;
        //        addServiceActivity.mcs_appointmentid = new EntityReference(thisAppt.LogicalName, thisAppt.Id);
        //        addServiceActivity.cvt_recurringappointmentsmaster = new EntityReference(RecurringAppointmentMaster.EntityLogicalName, thisAppt.SeriesId.Value);

        //        //Set the properties to the Appointment's Start/End
        //        addServiceActivity.ScheduledStart = thisAppt.ScheduledStart;
        //        addServiceActivity.ScheduledEnd = thisAppt.ScheduledEnd;

        //        //Creating a new Activity Party List and pulling all the resources from base Service Appointment to be carried over to recurring Service Activities. 
        //        List<ActivityParty> res = new List<ActivityParty>();
        //        var resources = retrieveServiceActivity.Resources.ToList();

        //        if (resources.Count > 0)
        //        {
        //            for (int i = 0; i < resources.Count; i++)
        //            {
        //                ActivityParty party = new ActivityParty();
        //                EntityReference entRef = resources[i].PartyId;
        //                party.PartyId = entRef;
        //                res.Add(party);
        //            }

        //            if (res.Count > 0)
        //                addServiceActivity.Resources = res;
        //        }

        //        Logger.WriteDebugMessage("Creating the new Service Activity");

        //        // Use the Book request message.
        //        BookRequest book = new BookRequest
        //        {
        //            Target = addServiceActivity
        //        };
        //        BookResponse booked = (BookResponse)OrganizationService.Execute(book);
        //        var ServiceApptId = booked.ValidationResult.ActivityId;

        //        // Verify that the Service Activity has been scheduled.
        //        if (ServiceApptId != Guid.Empty)
        //        {
        //            Logger.WriteDebugMessage(String.Format("Succesfully booked {0}.", addServiceActivity.Subject));
        //            SetStateRequest req = new SetStateRequest();
        //            req.EntityMoniker = new EntityReference(ServiceAppointment.EntityLogicalName, ServiceApptId);
        //            req.State = new OptionSetValue(3);//Scheduled
        //            req.Status = new OptionSetValue(4); //Reserved
        //            SetStateResponse reqResponse = (SetStateResponse)OrganizationService.Execute(req);
        //            Logger.WriteDebugMessage("Service Activity Status updated to Scheduled");
        //        }
        //        else
        //        {
        //            //Because a booking conflict was detected, this Service Activity will be set to an Open State with the Status as "Conflict".
        //            addServiceActivity.cvt_showtimeas = new OptionSetValue(917290000); //Open: Requested (Should it be Conflict?)
        //            //addServiceActivity.StateCode = new OptionSetValue(
        //            addServiceActivity.StatusCode = new OptionSetValue(1); //Was Tentative, now Requested?
        //            var newServiceActivityConflict = OrganizationService.Create(addServiceActivity);
        //        }
        //    }
        //}
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