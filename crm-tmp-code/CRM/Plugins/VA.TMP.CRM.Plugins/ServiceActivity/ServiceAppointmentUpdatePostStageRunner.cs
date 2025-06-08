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
    public class ServiceAppointmentUpdatePostStageRunner : PluginRunner
    {
        public ServiceAppointmentUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }

        public override void Execute()
        {
            //Write function to generate email for patient(s) that just got removed
            var sa = new ServiceAppointment();
            var currentPatients = GetCurrentPatients(out sa);
            var oldPatients = GetOldPatients();
            Logger.WriteDebugMessage(string.Format("Retrieved previously scheduled {0} and currently scheduled {1} Patients", oldPatients.Count, currentPatients.Count));
            var removedPatients = GetRemovedPatients(currentPatients, oldPatients);
            var addedPatients = GetAddedPatients(currentPatients, oldPatients);
            Logger.WriteDebugMessage("Ran Compare");
            if (removedPatients.Count > 0 || addedPatients.Count > 0)
            {
                Logger.WriteDebugMessage(removedPatients.Count + " patients were removed and " + addedPatients.Count + " were added");
                GenerateProviderEmail(removedPatients, addedPatients, sa);
                GeneratePatientAttendanceChangeEmails(removedPatients, addedPatients, sa);
                Logger.WriteDebugMessage("Provider and Patient emails sent for patient attendance change");
            }
            if (removedPatients.Count == 0 && addedPatients.Count == 0)
            {
                Logger.WriteDebugMessage("No emails sent from patient change");
            }
        }

        public void GenerateProviderEmail(List<Guid> removedPatients, List<Guid> addedPatients, ServiceAppointment sa)
        {
            var start = sa.ScheduledStart.Value;
            var now = DateTime.Now;
            var isAfter730EST = Convert.ToInt16(now.ToLocalTime().ToString("HHmm")) > 730;
            Logger.WriteDebugMessage(String.Format("******  start = {0}  ******", start));
            Logger.WriteDebugMessage(String.Format("******  now = {0}  ******", now));
            Logger.WriteDebugMessage(String.Format("******  Start.Date = {0}  ******", start.Date));
            Logger.WriteDebugMessage(String.Format("******  now.Date = {0}  ******", now.Date));
            Logger.WriteDebugMessage(String.Format("******  isAfter730EST value = {0}  ******", Convert.ToInt16(now.ToLocalTime().ToString("HHmm"))));

            if(start.Date == now.Date && start > now && isAfter730EST)
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
                Logger.WriteDebugMessage("Creating email with subject: " + email.Subject);
                
                OrganizationService.Create(email);
            }
            else
                Logger.WriteDebugMessage("No emails sent because Service Activity not scheduled for today: " + start.Date.ToShortDateString());
        }

        public void GeneratePatientAttendanceChangeEmails(List<Guid> removedPatients, List<Guid> addPatients, ServiceAppointment serviceAppointment)
        {
            var tsa = OrganizationService.Retrieve(cvt_resourcepackage.EntityLogicalName, serviceAppointment.cvt_relatedschedulingpackage.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true)).ToEntity<cvt_resourcepackage>();
            var saEmail = new ServiceAppointmentEmail(OrganizationService, Logger, new Email(), serviceAppointment, tsa);
            var proResources = saEmail.getPRGs("provider", OrganizationService);
            var providerAPs = serviceAppointment.Resources.Where(ap => ap.PartyId.LogicalName == SystemUser.EntityLogicalName).ToList();
            var clinicians = saEmail.GetRecipients(providerAPs, proResources);

            //Must call GetPatientVirtualMeetingSpace in order to properly execute the SendPatientEmail (which at the outset checks the class variables set in GetPatientVirtualMeetingSpace)
            bool? isPatient;
            saEmail.getPatientVirtualMeetingSpace(out isPatient);
            foreach(var pat in removedPatients)
            {
                if (CvtHelper.ShouldGenerateVeteranEmail(pat, OrganizationService, Logger) || serviceAppointment.cvt_Type.Value)
                {
                    saEmail.SendPatientEmail(clinicians, true, pat);
                    Logger.WriteDebugMessage("Cancellation email for group sent to patient with ID: " + pat);
                }
            }
            foreach(var pat in addPatients)
            {
                if (CvtHelper.ShouldGenerateVeteranEmail(pat, OrganizationService, Logger) || serviceAppointment.cvt_Type.Value)
                {
                    saEmail.SendPatientEmail(clinicians, false, pat);
                    Logger.WriteDebugMessage("Booking email for group sent to patient with ID: " + pat);
                }
            }
        }

        public void GenerateBookingEmails(List<Guid> addedPatients)
        {
            foreach(var pat in addedPatients)
            {
                ActivityParty[] recipient = { new ActivityParty { PartyId = new EntityReference(Contact.EntityLogicalName, pat) } };

                var email = new Email
                {
                    To = recipient,
                    Subject = "Your Video Visit has been Scheduled",
                    mcs_RelatedServiceActivity = new EntityReference(ServiceAppointment.EntityLogicalName, PrimaryEntity.Id)
                };
                OrganizationService.Create(email);
                Logger.WriteDebugMessage("Scheduled email for group sent to patient with ID: " + pat);
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
