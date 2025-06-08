using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Delete SAs if status is Close or Cancel on the Appt series.
    /// </summary>
    public class McsAppointmentMasterCloseCancelPostStageRunner : PluginRunner
    {
        #region Constructor
        public McsAppointmentMasterCloseCancelPostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods
        public override void Execute()
        {
            ////checks to make sure the registration is correct
            //if (PluginExecutionContext.PrimaryEntityId != null)
            //    CloseCancelAppointmentSeries(PluginExecutionContext.PrimaryEntityId);
        }

        //internal void CloseCancelAppointmentSeries(Guid thisApptMasterId)
        //{
        //    Logger.setMethod = "CloseCancelAppointmentSeries";
        //    Logger.WriteDebugMessage("starting CloseCancelAppointmentSeries");
        //    try
        //    {
        //        using (var srv = new Xrm(OrganizationService))
        //        {
        //            var RecurringMaster = srv.RecurringAppointmentMasterSet.FirstOrDefault(i => i.Id == thisApptMasterId);
        //            // DateTime RecurringSeriesEndDate = RecurringMaster.date.Value;
        //            var RecurringMasterStatusCode = RecurringMaster.StatusCode.Value;

        //            //RecurringMaster Closed(3) or Cancelled(4)
        //            if ((RecurringMasterStatusCode == 3) || (RecurringMasterStatusCode == 4))
        //            {
        //                //Check if the SA is Reserved(4) or Requested(1).
        //                //Also check if the Service Activity is part of series that was ended early. We will delete the SA's that were schedueld outside the end date.
        //                var ServiceActivities = from sa in srv.ServiceAppointmentSet
        //                                        where (sa.cvt_recurringappointmentsmaster.Id == thisApptMasterId)
        //                                           && ((sa.StatusCode.Value == 1) || (sa.StatusCode.Value == 4))
        //                                           && (sa.mcs_appointmentid == null)
        //                                        // && (sa.ScheduledStart.Value.CompareTo(RecurringSeriesEndDate) >= 0)
        //                                        && sa.StateCode == 0
        //                                        select sa;

        //                var SACount = 0;
        //                foreach (var sa in ServiceActivities)
        //                {
        //                    SACount += 1;
        //                }
        //                if (SACount > 0)
        //                {
        //                    Logger.WriteDebugMessage("Found " + SACount + " Service Activities. Starting Delete.");
        //                    foreach (var sa in ServiceActivities)
        //                    {
        //                        Logger.WriteDebugMessage("Got ServiceActivity:" + sa.Subject);
        //                        OrganizationService.Delete(sa.LogicalName, sa.Id);
        //                        Logger.WriteDebugMessage("Service Activity Deleted");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (FaultException<OrganizationServiceFault> ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //        throw new InvalidPluginExecutionException(McsSettings.getUnexpectedErrorMessage + ":" + ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.StartsWith("custom"))
        //        {
        //            Logger.WriteDebugMessage(ex.Message.Substring(6));
        //            throw new InvalidPluginExecutionException(ex.Message.Substring(6));
        //        }
        //        else
        //        {
        //            Logger.setMethod = "Execute";
        //            Logger.WriteToFile(ex.Message);
        //            throw new InvalidPluginExecutionException(McsSettings.getUnexpectedErrorMessage + ":" + ex.Message);
        //        }
        //    }
        //}       
        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_appointmentplugin"; }
        }
        #endregion
    }
}