using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentSetStatePostStageRunner : PluginRunner
    {
        #region Constructor
        public ServiceAppointmentSetStatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {           
            CheckStatusDate(PluginExecutionContext.PrimaryEntityId);
            CloseChildBlockResources();
        }

        /// <summary>
        /// Checking Service Appointment Status & DateTime.
        /// </summary>
        /// <param name="serviceApptId"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        internal void CheckStatusDate(Guid serviceApptId)
        {
            Logger.setMethod = "CheckStatusDate";
            Logger.WriteDebugMessage("Starting CheckStatusDate");
            
                //Retrieve the Service Appointment
                ServiceAppointment serviceAppt = OrganizationService.Retrieve("serviceappointment", serviceApptId,
                    new ColumnSet(true)).ToEntity<ServiceAppointment>();
            //Check for Complete or Patient No Show status. 
            if (serviceAppt.StatusCode.Value == (int) serviceappointment_statuscode.Completed)
            //|| serviceAppt.StatusCode.Value == (int) serviceappointment_statuscode.PatientNoShow)
            {
                var scheduledStart = serviceAppt.ScheduledStart?.ToLocalTime();
                int? dateComparison = scheduledStart?.CompareTo(DateTime.Now.ToLocalTime());
                if (dateComparison > 0)
                    throw new InvalidPluginExecutionException(
                        "customA Service Appointment scheduled in the future cannot be closed in a 'Complete' or 'Patient No Show' status. The closure of this Service Appointment has been aborted.");
            }
            else
            {
                return;
            }

            Logger.WriteDebugMessage("Ending CheckStatusDate");
        }

        internal void CloseChildBlockResources()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var appointment = srv.ServiceAppointmentSet.FirstOrDefault(sa => sa.Id == PrimaryEntity.Id);
                if (appointment.mcs_groupappointment.Value && !appointment.cvt_Type.Value)
                {
                    var childAppts = srv.AppointmentSet.Where(a => a.cvt_serviceactivityid.Id == PrimaryEntity.Id && (a.StateCode.Value == AppointmentState.Scheduled || a.StateCode.Value == AppointmentState.Open)).ToList();
                    Logger.WriteDebugMessage("Attempting to close out {0} (child) Block Resource records");
                    var successes = 0;
                    foreach (var appt in childAppts)
                    {
                        var updateAppt = new Appointment
                        {
                            ActivityId = appt.Id,
                            cvt_IntegrationBookingStatus = new OptionSetValue(appointment.StatusCode.Value)
                        };
                        try
                        {
                            OrganizationService.Update(updateAppt);
                            successes++;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile(string.Format("Failed to close out child Block Resource {0}.  Error Details: {1}", appt.Subject, CvtHelper.BuildExceptionMessage(ex)));
                        }
                    }
                    Logger.WriteDebugMessage(string.Format("Finished closing out {0}/{1} Block Resource records.", successes, childAppts.Count));
                }
                else
                    Logger.WriteDebugMessage(string.Format("Not closing out child Block Resource Records because it is a clinic based ({0}) group ({1}) appointment.", appointment.cvt_Type.Value, appointment.mcs_groupappointment.Value));
            }
        }
        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }
        #endregion
    }
}