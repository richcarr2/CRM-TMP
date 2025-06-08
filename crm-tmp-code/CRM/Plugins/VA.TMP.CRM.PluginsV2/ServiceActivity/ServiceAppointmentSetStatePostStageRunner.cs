using MCS.ApplicationInsights;
using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentSetStatePostStageRunner : AILogicBase
    {
        #region Constructor
        public ServiceAppointmentSetStatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void ExecuteLogic()
        {           
            CheckStatusDate(PluginExecutionContext.PrimaryEntityId);
            CloseChildBlockResources();
        }

        /// <summary>
        /// Checking Service Appointment Status & DateTime.
        /// </summary>
        /// <param name="serviceApptId"></param>
        internal void CheckStatusDate(Guid serviceApptId)
        {
            //Logger.setMethod = "CheckStatusDate";
            //Logger.WriteDebugMessage("Starting CheckStatusDate");
            Trace("Starting CheckStatusDate", LogLevel.Debug);

            try
            {
                //Retrieve the Service Appointment
                ServiceAppointment serviceAppt = OrganizationService.Retrieve("serviceappointment", serviceApptId,
                        new ColumnSet(true)).ToEntity<ServiceAppointment>();
                //Check for Complete or Patient No Show status. 
                if (serviceAppt.StatusCode.Value == (int)serviceappointment_statuscode.Completed)
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
            }
            catch (Exception ex)
            {
                Trace($"Failed to Check Status Date: Error: {ex}.");
            }

            //Logger.WriteDebugMessage("Ending CheckStatusDate");
            Trace("Ending CheckStatusDate", LogLevel.Debug);
        }

        internal void CloseChildBlockResources()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    Trace($"Appointment ID: {PrimaryEntity?.Id}");

                    var appointment = srv.ServiceAppointmentSet.FirstOrDefault(sa => sa.Id == PrimaryEntity.Id);
                    if (appointment.mcs_groupappointment.Value && !appointment.cvt_Type.Value)
                    {
                        var childAppts = srv.AppointmentSet.Where(a => a.cvt_serviceactivityid.Id == PrimaryEntity.Id && (a.StateCode.Value == AppointmentState.Scheduled || a.StateCode.Value == AppointmentState.Open)).ToList();
                        //Logger.WriteDebugMessage("Attempting to close out {0} (child) Block Resource records");
                        Trace("Attempting to close out {0} (child) Block Resource records", LogLevel.Debug);
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
                                //Logger.WriteToFile(string.Format("Failed to close out child Block Resource {0}.  Error Details: {1}", appt.Subject, CvtHelper.BuildExceptionMessage(ex)));
                                Trace($"Failed to close out child Block Resource {appt.Subject}.  Error Details: {CvtHelper.BuildExceptionMessage(ex)}", LogLevel.Debug);
                            }
                        }
                        //Logger.WriteDebugMessage(string.Format("Finished closing out {0}/{1} Block Resource records.", successes, childAppts.Count));
                        Trace($"Finished closing out {successes}/{childAppts.Count} Block Resource records.", LogLevel.Debug);
                    }
                    else
                    {
                        //Logger.WriteDebugMessage(string.Format("Not closing out child Block Resource Records because it is a clinic based ({0}) group ({1}) appointment.", appointment.cvt_Type.Value, appointment.mcs_groupappointment.Value));
                        Trace($"Not closing out child Block Resource Records because it is a clinic based ({appointment.cvt_Type.Value}) group ({appointment.mcs_groupappointment.Value}) appointment.", LogLevel.Debug);
                    }
                }
                catch (Exception ex)
                {
                    Trace($"Failed to complete Close Block Resources with the following error: {ex}.");
                }
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