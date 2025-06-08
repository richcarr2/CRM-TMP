using MCS.ApplicationInsights;
using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM.ServiceActivity
{
    public class ServiceAppointmentUpdatePostStageRunner : AILogicBase
    {
        public ServiceAppointmentUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }

        public override void ExecuteLogic()
        {
            //Write function to generate email for patient(s) that just got removed
            var sa = new ServiceAppointment();
            var currentPatients = GetCurrentPatients(out sa);
            var oldPatients = GetOldPatients();
            //Logger.WriteDebugMessage(string.Format("Retrieved previously scheduled {0} and currently scheduled {1} Patients", oldPatients.Count, currentPatients.Count));
            Trace($"Retrieved previously scheduled {oldPatients.Count} and currently scheduled {currentPatients.Count} Patients", LogLevel.Debug);
            var removedPatients = GetRemovedPatients(currentPatients, oldPatients);
            var addedPatients = GetAddedPatients(currentPatients, oldPatients);
            //Logger.WriteDebugMessage("Ran Compare");
            Trace("Ran Compare", LogLevel.Debug);
            if (removedPatients.Count > 0 || addedPatients.Count > 0)
            {
                //Logger.WriteDebugMessage(removedPatients.Count + " patients were removed and " + addedPatients.Count + " were added");
                Trace($"{removedPatients.Count} patients were removed and {addedPatients.Count} were added", LogLevel.Debug);
                GenerateProviderEmail(removedPatients, addedPatients, sa);
                GeneratePatientAttendanceChangeEmails(removedPatients, addedPatients, sa);
                //Logger.WriteDebugMessage("Provider and Patient emails sent for patient attendance change");
                Trace("Provider and Patient emails sent for patient attendance change", LogLevel.Debug);
            }
            if (removedPatients.Count == 0 && addedPatients.Count == 0)
            {
                //Logger.WriteDebugMessage("No emails sent from patient change");
                Trace("No emails sent from patient change", LogLevel.Debug);
            }
        }

        public void GenerateProviderEmail(List<Guid> removedPatients, List<Guid> addedPatients, ServiceAppointment sa)
        {
            var start = sa.ScheduledStart.Value;
            var now = DateTime.Now;
            var isAfter730EST = Convert.ToInt16(now.ToLocalTime().ToString("HHmm")) > 730;
            //Logger.WriteDebugMessage(String.Format("******  start = {0}  ******", start));
            Trace($"******  start = {start}  ******", LogLevel.Debug);
            //Logger.WriteDebugMessage(String.Format("******  now = {0}  ******", now));
            Trace($"******  now = {now}  ******", LogLevel.Debug);
            //Logger.WriteDebugMessage(String.Format("******  Start.Date = {0}  ******", start.Date));
            Trace($"******  Start.Date = {start.Date}  ******", LogLevel.Debug);
            //Logger.WriteDebugMessage(String.Format("******  now.Date = {0}  ******", now.Date));
            Trace($"******  now.Date = {now.Date}  ******", LogLevel.Debug);
            //Logger.WriteDebugMessage(String.Format("******  isAfter730EST value = {0}  ******", Convert.ToInt16(now.ToLocalTime().ToString("HHmm"))));
            Trace($"******  isAfter730EST value = {Convert.ToInt16(now.ToLocalTime().ToString("HHmm"))}  ******", LogLevel.Debug);

            if (start.Date == now.Date && start > now && isAfter730EST)
            {
                var action = string.Empty;
                if (addedPatients.Count > 0 && removedPatients.Count > 0)
                    action = "added to and removed from";
                else if (addedPatients.Count > 0)
                    action = "added to";
                else if (removedPatients.Count > 0)
                    action = "removed from";
                else
                    return;
                var email = new Email
                {
                    Subject = string.Format("Patients have been {0} your appointment", action),
                    mcs_RelatedServiceActivity = new EntityReference(sa.LogicalName, sa.Id)
                };
                //Logger.WriteDebugMessage("Creating email with subject: " + email.Subject);
                Trace($"Creating email with subject: {email.Subject}", LogLevel.Debug);

                OrganizationService.Create(email);
            }
            else
            {
                //Logger.WriteDebugMessage("No emails sent because Service Activity not scheduled for today: " + start.Date.ToShortDateString());
                Trace($"No emails sent because Service Activity not scheduled for today: {start.Date.ToShortDateString()}", LogLevel.Debug);
            }
        }

        public void GeneratePatientAttendanceChangeEmails(List<Guid> removedPatients, List<Guid> addPatients, ServiceAppointment serviceAppointment)
        {
            Trace($"Entered GeneratePatientAttendanceChangeEmails", LogLevel.Debug);
            var tsa = OrganizationService.Retrieve(cvt_resourcepackage.EntityLogicalName, serviceAppointment.cvt_relatedschedulingpackage.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true)).ToEntity<cvt_resourcepackage>();
            Trace($"Retrieved scheduling Package with ID: {tsa.Id}", LogLevel.Debug);
            var crmEmail = new Email();

            Trace(string.Concat("Serviceappointment is null: ", serviceAppointment == null), LogLevel.Debug);
            Trace(string.Concat("OrganizationService is null: ", OrganizationService == null), LogLevel.Debug);
            Trace(string.Concat("tsa is null: ", tsa == null), LogLevel.Debug);
            Trace(string.Concat("crmEmail is null: ", crmEmail == null), LogLevel.Debug);
            var saEmail = new ServiceAppointmentEmail(OrganizationService, Logger, crmEmail, serviceAppointment, tsa);
            Trace($"Created ServiceAppointmentEmail object", LogLevel.Debug);
            var proResources = saEmail.getPRGs("provider", OrganizationService);
            Trace($"proResources count: {proResources.Count}", LogLevel.Debug);
            var providerAPs = serviceAppointment.Resources.Where(ap => ap.PartyId.LogicalName == SystemUser.EntityLogicalName).ToList();
            Trace($"providerAPs count: {providerAPs.Count}", LogLevel.Debug);
            var clinicians = saEmail.GetRecipients(providerAPs, proResources);
            Trace($"clinicians count: {clinicians.Count}", LogLevel.Debug);

            //Must call GetPatientVirtualMeetingSpace in order to properly execute the SendPatientEmail (which at the outset checks the class variables set in GetPatientVirtualMeetingSpace)
            bool? isPatient;
            saEmail.getPatientVirtualMeetingSpace(out isPatient);
            foreach (var pat in removedPatients)
            {
                if (CvtHelper.ShouldGenerateVeteranEmail(pat, OrganizationService, pluginLogger) || serviceAppointment.cvt_Type.Value)
                {
                    saEmail.SendPatientEmail(clinicians, true, pluginLogger, pat);
                    //Logger.WriteDebugMessage("Cancellation email for group sent to patient with ID: " + pat);
                    Trace($"Cancellation email for group sent to patient with ID: {pat}", LogLevel.Debug);
                }
            }
            foreach (var pat in addPatients)
            {
                if (CvtHelper.ShouldGenerateVeteranEmail(pat, OrganizationService, pluginLogger) || serviceAppointment.cvt_Type.Value)
                {
                    saEmail.SendPatientEmail(clinicians, false, pluginLogger, pat);
                    //Logger.WriteDebugMessage("Booking email for group sent to patient with ID: " + pat);
                    Trace($"Booking email for group sent to patient with ID: {pat}", LogLevel.Debug);
                }
            }
        }

        public void GenerateBookingEmails(List<Guid> addedPatients)
        {
            foreach (var pat in addedPatients)
            {
                ActivityParty[] recipient = { new ActivityParty { PartyId = new EntityReference(Contact.EntityLogicalName, pat) } };

                var email = new Email
                {
                    To = recipient,
                    Subject = "Your Video Visit has been Scheduled",
                    mcs_RelatedServiceActivity = new EntityReference(ServiceAppointment.EntityLogicalName, PrimaryEntity.Id)
                };
                OrganizationService.Create(email);
                //Logger.WriteDebugMessage("Scheduled email for group sent to patient with ID: " + pat);
                Trace($"Scheduled email for group sent to patient with ID: {pat}", LogLevel.Debug);
            }
        }

        public List<Guid> GetCurrentPatients(out ServiceAppointment sa)
        {
            var newCustomers = new List<Guid>();
            using (var srv = new Xrm(OrganizationService))
            {
                sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == PrimaryEntity.Id);
                srv.LoadProperty(sa, "serviceappointment_activity_parties");
                newCustomers = sa.serviceappointment_activity_parties.Where(ap => ap.ParticipationTypeMask.Value == (int)ActivityPartyParticipationTypeMask.Customer).Select(ap => ap.PartyId.Id).ToList();
            }
            return newCustomers;
        }

        public List<Guid> GetOldPatients()
        {
            var customers = new List<Guid>();
            var pre = PluginExecutionContext.PreEntityImages["pre"];
            var patientsAtt = pre.Attributes.FirstOrDefault(k => k.Key == "customers");
            if (patientsAtt.Value != null)
            {
                EntityCollection ec = (EntityCollection)patientsAtt.Value;
                foreach (var entity in ec.Entities)
                {
                    customers.Add(entity.ToEntity<ActivityParty>().PartyId.Id);
                }
            }
            return customers;
        }

        public List<Guid> GetRemovedPatients(List<Guid> currentCustomers, List<Guid> oldCustomers)
        {
            if (currentCustomers.Count != 0)
                return oldCustomers.Except(currentCustomers).ToList();
            else
                return oldCustomers;
        }

        public List<Guid> GetAddedPatients(List<Guid> currentCustomers, List<Guid> oldCustomers)
        {
            if (oldCustomers.Count == 0)
                return currentCustomers;
            else
                return currentCustomers.Except(oldCustomers).ToList();
        }
    }
}
