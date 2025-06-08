using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentEmail
    {
        #region Constructor/Data Model for Service Appointment Emails
        IOrganizationService OrganizationService;
        MCSLogger Logger;
        Email Email;
        ServiceAppointment ServiceAppointment;

        //WMC 11/15/2018 Changing variable type to SP;
        //mcs_services Tsa;
        cvt_resourcepackage Tsa;

        List<SystemUser> patTCTList = new List<SystemUser>();
        List<SystemUser> proTCTList = new List<SystemUser>();
        string patTCTs;
        string proTCTs;

        private string SiteLocal911Phone;
        private string SiteMainPhone;
        //private string ProRoom;
        //private string PatRoom;
        private string stethIP;
        private string PatientVirtualMeetingSpace;
        private string ProviderVirtualMeetingSpace;
        private cvt_component VirtualMeetingSpaceComponent;
        private bool isCvtTablet;
        private bool isVAIssuediOSDevice;

        // WMC 11/15/2018 Changing signature to use SP 
        //public ServiceAppointmentEmail(IOrganizationService organizationService, MCSLogger logger, Email email, ServiceAppointment serviceAppointment, mcs_services tsa)
        public ServiceAppointmentEmail(IOrganizationService organizationService, MCSLogger logger, Email email, ServiceAppointment serviceAppointment, cvt_resourcepackage tsa)
        {
            OrganizationService = organizationService;
            Logger = logger;
            Email = email;
            Logger.WriteDebugMessage(string.Concat("Serviceappointment is null: ", serviceAppointment == null));
            ServiceAppointment = serviceAppointment;
            Tsa = tsa;
        }
        #endregion

        #region Choose which Email to send
        /// <summary>
        /// Determine which email to populate and send, then call the appropriate function(s)
        /// </summary>
        public void Execute()
        {
            Logger.WriteDebugMessage($"Current SA State: { ServiceAppointment.StateCode.ToString()}");
            Logger.WriteDebugMessage($"****wmc***** CURRENT EMAIL SUBJECT: {Email.Subject} ************wmc***********");
            if ((ServiceAppointment.StateCode.Value == ServiceAppointmentState.Canceled || ServiceAppointment.StatusCode.Value == (int)serviceappointment_statuscode.ReservedScheduled) && Email.RegardingObjectId == null)
            {
                Logger.WriteDebugMessage($"*~*~*~ CURRENT EMAIL SUBJECT: {Email.Subject} ~*~*~*");
                if (Email.Subject.StartsWith("TMP Scheduler Action:"))
                {
                    Logger.WriteDebugMessage("Beginning Vista Reminder Email");
                    if (ServiceAppointment.StateCode.Value == ServiceAppointmentState.Canceled)
                    {
                        if (ServiceAppointment.cvt_notificationsent == null || ServiceAppointment.cvt_notificationsent.Value)
                            SendVistaReminder();
                        else
                        {
                            var notificationSent = ServiceAppointment.cvt_notificationsent == null ? "null" : ServiceAppointment.cvt_notificationsent.Value.ToString();
                            Logger.WriteDebugMessage($"Skipping TMP Scheduler Action Email Notification during Cancellation as Service Appointment Notification Sent is {notificationSent}");
                            OrganizationService.Delete(Email.EntityLogicalName, Email.Id);
                        }
                    }
                    else
                    {
                        SendVistaReminder();
                    }
                    Logger.WriteDebugMessage("Completed Vista Reminder Email");
                }
                else if (Email.Subject.Contains("Your Telephone Appointment has been canceled for"))
                {
                    Logger.WriteDebugMessage("Beginning Telephone Cancel Email");
                    if (ServiceAppointment.StateCode.Value == ServiceAppointmentState.Canceled)
                    {
                        if (ServiceAppointment.cvt_notificationsent == null || ServiceAppointment.cvt_notificationsent.Value)
                            NotifyParticipantsOfAppointment(true, "cancel");
                        else
                        {
                            var notificationSent = ServiceAppointment.cvt_notificationsent == null ? "null" : ServiceAppointment.cvt_notificationsent.Value.ToString();
                            Logger.WriteDebugMessage($"Skipping Service Activity Notification Email during Cancellation as Service Appointment Notification Sent is {notificationSent}");
                            OrganizationService.Delete(Email.EntityLogicalName, Email.Id);
                        }
                    }
                }
                else if (Email.Subject.Contains("Telehealth Appointment Notification for"))
                {
                    Logger.WriteDebugMessage("Beginning Service Activity Notification Email");
                    if (ServiceAppointment.StateCode.Value == ServiceAppointmentState.Canceled)
                    {
                        if (ServiceAppointment.cvt_notificationsent == null || ServiceAppointment.cvt_notificationsent.Value)
                            NotifyParticipantsOfAppointment(true, "cancel");
                        else
                        {
                            var notificationSent = ServiceAppointment.cvt_notificationsent == null ? "null" : ServiceAppointment.cvt_notificationsent.Value.ToString();
                            Logger.WriteDebugMessage($"Skipping Service Activity Notification Email during Cancellation as Service Appointment Notification Sent is {notificationSent}");
                            OrganizationService.Delete(Email.EntityLogicalName, Email.Id);
                        }
                    }
                    else
                    {
                        NotifyParticipantsOfAppointment(true, "");
                    }
                    Logger.WriteDebugMessage("Completed Service Activity Notification Email");
                }
                else if (Email.Subject.Contains("Patients have been"))
                {
                    Logger.WriteDebugMessage("Beginning Provider email notification of change in patients");
                    NotifyProviderOfPatientChange();
                    Logger.WriteDebugMessage("Completed Provider email notification of change in patients");
                }
                else if (Email.Subject.Contains("Your Video Visit has been"))
                {
                    Logger.WriteDebugMessage("Beginning Patient email notification of addition/removal from H/M Group SA");
                    NotifyPatientOfAdditionOrRemovalFromGroup(Email.Subject.Contains("Cancelled") || Email.Subject.Contains("Canceled"));
                    Logger.WriteDebugMessage("Completed Patient email notification of addition/removal from H/M Group SA");
                }
            }
            else if (Email.RegardingObjectId != null && Email.RegardingObjectId.LogicalName == Contact.EntityLogicalName)
            {
                Logger.WriteDebugMessage("Beginning Patient Email");
                CvtHelper.CreateCalendarAppointmentAttachment(Email, ServiceAppointment, ServiceAppointment.StatusCode.Value, "", OrganizationService, Logger, Email.cvt_PlainTextDescription);
                CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
                Logger.WriteDebugMessage("Completed Patient Email");
            }
            else
                return;
        }
        #endregion

        #region Vista Reminder Email
        internal void SendVistaReminder()
        {
            Logger.WriteDebugMessage("Beginning Vista Reminder");
            var provTeamMembers = new List<TeamMembership>();
            var patTeamMembers = new List<TeamMembership>();
            mcs_site patientSite = null;
            mcs_site providerSite = null;
            mcs_facility patFacility = null;
            mcs_facility proFacility = null;
            //var provFacilityId = Tsa.cvt_ProviderFacility.Id
            //var patFacilityId = Tsa.cvt_PatientFacility != null ? Tsa.cvt_PatientFacility.Id : Guid.Empty;
            var provFacilityId = Guid.Empty;
            var patFacilityId = Guid.Empty;
            var intraFacility = (provFacilityId == patFacilityId) ? true : false;
            bool groupAppt = (ServiceAppointment.mcs_groupappointment == false) ? false : true;
            bool apptVVC = (ServiceAppointment.cvt_Type == false) ? false : true;
            cvt_resourcepackage SchedulingPkg = null;

            using (var srv = new Xrm(OrganizationService))
            {
                SchedulingPkg = srv.cvt_resourcepackageSet.FirstOrDefault(r => r.cvt_resourcepackageId == ((EntityReference)ServiceAppointment.cvt_relatedschedulingpackage).Id);

                //clinic-based  = 0
                //VVC           = 1

                Logger.WriteDebugMessage("Attempting to get *PATIENT* site/facility from ServiceAppointment");
                if (ServiceAppointment.mcs_relatedsite != null)
                {
                    patientSite = srv.mcs_siteSet.FirstOrDefault(pat => pat.mcs_siteId == ((EntityReference)ServiceAppointment.mcs_relatedsite).Id);
                    //now get the facility from the TMPSite
                    patFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.mcs_facilityId == ((EntityReference)patientSite.mcs_FacilityId).Id);
                    patFacilityId = patFacility.Id;
                }
                else
                {
                    patFacilityId = (Tsa.cvt_patientfacility != null && Tsa.cvt_groupappointment.Value == true) ? Tsa.cvt_patientfacility.Id : Guid.Empty;

                }
                Logger.WriteDebugMessage("Got Patient site/facility");

                Logger.WriteDebugMessage("Attempting to get *Provider* site/facility from ServiceAppointment  ( A ) ");
                if (ServiceAppointment.mcs_relatedprovidersite != null)
                {
                    providerSite = srv.mcs_siteSet.FirstOrDefault(pro => pro.mcs_siteId == ((EntityReference)ServiceAppointment.mcs_relatedprovidersite).Id);
                    //now get the facility from the TMPSite
                    proFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.mcs_facilityId == ((EntityReference)providerSite.mcs_FacilityId).Id);
                    provFacilityId = proFacility.Id;
                }
                Logger.WriteDebugMessage("Got Provider site/facility");

                var provTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == provFacilityId && t.cvt_Type != null && t.cvt_Type.Value == 917290005);
                if (provTeam != null)
                    provTeamMembers = srv.TeamMembershipSet.Where(TM => TM.TeamId == provTeam.Id).ToList();
                else
                    Logger.WriteToFile("The provider side Scheduler Team was unable to be found for Service Activity: " + ServiceAppointment.Id);

                var patTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == 917290005);
                if (patTeam != null)
                    patTeamMembers = srv.TeamMembershipSet.Where(TM => TM.TeamId == patTeam.Id).ToList();
                else
                    Logger.WriteToFile("The patient side Scheduler Team was unable to be found for Service Activity: " + ServiceAppointment.Id);
            }

            Logger.WriteDebugMessage(string.Format("Retrieved {0} Pat Team Members and {1} Pro Team Members", patTeamMembers.Count, provTeamMembers.Count));
            bool provCheck = false;
            bool patCheck = false;
            EntityCollection provMembers = new EntityCollection();
            EntityCollection patMembers = new EntityCollection();
            var subSpecialty = ServiceAppointment.mcs_servicesubtype != null ? ServiceAppointment.mcs_servicesubtype.Id : Guid.Empty;

            if (provTeamMembers.Count == 0)
            {
                if (Logger != null && proFacility != null)
                {
                    Logger.WriteToFile("There are no members of the Scheduler team at " + proFacility?.mcs_name + ".  Please contact the FTC and ensure this is corrected.");
                }
            }
            else
            {
                foreach (TeamMembership tm in provTeamMembers)
                {
                    if (tm.SystemUserId != null)
                    {
                        if (FilterMembersBySpecialty(tm.SystemUserId.Value, ServiceAppointment.mcs_servicetype.Id, subSpecialty))
                        {
                            ActivityParty p = new ActivityParty()
                            {
                                PartyId = new EntityReference(SystemUser.EntityLogicalName, tm.SystemUserId.Value)
                            };
                            provMembers.Entities.Add(p);
                        }
                        if (ServiceAppointment.CreatedBy.Id == tm.SystemUserId.Value)
                            provCheck = true;
                    }
                }
            }
            //if (patTeamMembers.Count == 0 && !Tsa.cvt_Type.Value)
            if (patTeamMembers.Count == 0)
            {
                Logger.WriteToFile(string.Format("There are no members of the Scheduler team at {0}.  Please contact the FTC and ensure this is corrected.", patFacility != null ? patFacility.mcs_name : "\"No Facility Listed\""));
            }
            else
            {
                foreach (TeamMembership tm in patTeamMembers)
                {
                    if (tm.SystemUserId != null)
                    {
                        if (FilterMembersBySpecialty(tm.SystemUserId.Value, ServiceAppointment.mcs_servicetype.Id, subSpecialty))
                        {
                            ActivityParty p = new ActivityParty()
                            {
                                PartyId = new EntityReference(SystemUser.EntityLogicalName, tm.SystemUserId.Value)
                            };
                            patMembers.Entities.Add(p);
                        }
                        if (ServiceAppointment.CreatedBy.Id == tm.SystemUserId.Value)
                            patCheck = true;
                    }
                }
            }

            //If TSA is Store Forward and the scheduler is on the patient Scheduler team side OR if the scheduler is on both Scheduler Teams, then don't send email
            if ((SchedulingPkg.cvt_availabletelehealthmodality != null && SchedulingPkg.cvt_availabletelehealthmodality.Value == (int)mcs_servicescvt_AvailableTelehealthModalities.StoreandForward && patCheck) || (patCheck && provCheck))
            {
                DeleteVistaReminder("No need to send out email, Deleting Email.  TSA is SFT and Scheduler is on Pat Team OR Scheduler is on Both Pat and Prov Team");
            }
            else if (patCheck && provCheck)
            {
                DeleteVistaReminder("No need to send out email, Deleting Email.  Scheduler is on Both Pat and Prov Scheduler Teams.");
            }
            else if ((patCheck || patCheck) && Tsa.cvt_intraorinterfacility.Value == (int)cvt_resourcepackagecvt_intraorinterfacility.Intrafacility)
            {
                DeleteVistaReminder("No need to send out email, Deleting Email.  Scheduler is on Facility Scheduler Team and it is IntraFacility.");
            }
            else if (provCheck && Tsa.cvt_patientlocationtype.Value == (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnect)
            {
                DeleteVistaReminder("No need to send out email, Deleting Email.  Scheduler is on Provider Scheduler Team and it is VVC.");
            }
            else
            {
                Logger.WriteDebugMessage($"provCheck: {provCheck}; patCheck: {patCheck}");
                SetupVistaReminderEmail(ServiceAppointment, provMembers, patMembers, provCheck, patCheck);
            }
        }

        internal void DeleteVistaReminder(string debugMessage)
        {
            Logger.WriteDebugMessage(debugMessage);
            try
            {
                OrganizationService.Delete(Email.LogicalName, Email.Id);
                Logger.WriteDebugMessage("Email Deleted");
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Unable to Delete Email " + ex.Message + ".  Leaving email as is.");
            }
        }

        /// <summary>
        /// returns false if the user is not associated with the specialty on SA (or sub-specialty if listed)
        /// </summary>
        /// <param name="userId">id of user to check for specialties</param>
        /// <param name="specialty">specialty to check</param>
        /// <param name="subSpecialty">sub-specialty to check</param>
        /// <returns>true if user is associated with specialty/sub-specialty or false if not</returns>
        internal bool FilterMembersBySpecialty(Guid userId, Guid specialty, Guid subSpecialty)
        {
            var userAssociatedWithSpecialty = false;
            var userAssociatedWithSubSpecialty = false;
            var specialties = new List<mcs_servicetype>();
            var subSpecialties = new List<mcs_servicesubtype>();
            using (var srv = new Xrm(OrganizationService))
            {
                var user = srv.SystemUserSet.FirstOrDefault(u => u.Id == userId);

                //Retrieve related records through N:N association - CRM doesn't eager load, so have to call this.  It returns null if no items are in the list, so also need to null check before converting to List
                srv.LoadProperty(user, "cvt_systemuser_mcs_servicetype");
                var specialtyRelated = user.cvt_systemuser_mcs_servicetype;
                if (specialtyRelated != null)
                    specialties = specialtyRelated.ToList();

                if (subSpecialty != Guid.Empty)
                {
                    //Same comment as above
                    srv.LoadProperty(user, "cvt_systemuser_mcs_servicesubtype");
                    var subSpecialtyRelated = user.cvt_systemuser_mcs_servicesubtype;
                    if (subSpecialtyRelated != null)
                        subSpecialties = subSpecialtyRelated.ToList();
                    if (subSpecialties.Count == 0)
                    {
                        userAssociatedWithSubSpecialty = true;
                        Logger.WriteDebugMessage(string.Format("{0} has no sub-specialties listed, checking specialties", user.FullName));
                    }
                    else
                    {
                        var subMatch = subSpecialties.FirstOrDefault(s => s.Id == subSpecialty);
                        userAssociatedWithSubSpecialty = subMatch != null;
                        Logger.WriteDebugMessage(String.Format("{0} {1} {2} as a sub-specialty", user.FullName, userAssociatedWithSubSpecialty ? "has" : "does not have", subSpecialty));
                        return userAssociatedWithSubSpecialty;
                    }
                }
                if (specialties.Count == 0)
                {
                    userAssociatedWithSpecialty = true;
                    Logger.WriteDebugMessage("User has no specialties listed, auto-opting in " + user.FullName + " to emails");
                }
                else
                {
                    var match = specialties.FirstOrDefault(s => s.Id == specialty);
                    userAssociatedWithSpecialty = match != null;
                    Logger.WriteDebugMessage(String.Format("{0} {1} {2} as a specialty", user.FullName, userAssociatedWithSpecialty ? "has" : "does not have", specialty));
                }
            }
            return userAssociatedWithSpecialty;
        }

        internal void SetupVistaReminderEmail(ServiceAppointment serviceAppointment, EntityCollection provMembers, EntityCollection patMembers, bool provCheck, bool patCheck)
        {
            Logger.WriteDebugMessage("Beginning SetupVistaReminderEmail");
            #region variables
            mcs_facility patFacility = null;
            mcs_facility proFacility = null;
            mcs_site patientSite = null;
            mcs_site providerSite = null;
            string patStation = string.Empty;
            string proStation = string.Empty;
            string providerLocation = string.Empty;
            string patientLocation = string.Empty;
            int timeZone = 0;
            List<ActivityParty> To = new List<ActivityParty>();
            #endregion

            #region Setting prostation, patstation, pro/pat site/facility, FROM, Specialty
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("Attempting to get *PATIENT* site/facility from ServiceAppointment");
                if (ServiceAppointment.mcs_relatedsite != null)
                {
                    patientSite = srv.mcs_siteSet.FirstOrDefault(pat => pat.Id == ServiceAppointment.mcs_relatedsite.Id);
                    //now get the facility from the TMPSite
                    patFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.Id == patientSite.mcs_FacilityId.Id);
                    if (patFacility != null)
                    {
                        patStation = patFacility == null ? string.Empty : " (" + patFacility.mcs_StationNumber + ")";
                    }
                }
                else if (Tsa.cvt_groupappointment.Value && Tsa.cvt_patientfacility != null)
                {
                    Logger.WriteDebugMessage("Could not find Patient Facility from TMP Site.  Must be Group or VVC, so get the Patient Facility from the Scheduling Package.");
                    patFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.Id == Tsa.cvt_patientfacility.Id);
                    patStation = patFacility == null ? string.Empty : " (" + patFacility.mcs_StationNumber + ")";
                }
                Logger.WriteDebugMessage("Got Patient site/facility. Attempting to get *Provider* site/facility from ServiceAppointment");

                if (ServiceAppointment.mcs_relatedprovidersite != null)
                {
                    providerSite = srv.mcs_siteSet.FirstOrDefault(pro => pro.Id == ServiceAppointment.mcs_relatedprovidersite.Id);
                    //now get the facility from the TMPSite
                    proFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.Id == providerSite.mcs_FacilityId.Id);
                    if (proFacility != null)
                    {
                        proStation = proFacility == null ? string.Empty : " (" + proFacility.mcs_StationNumber + ")";
                    }
                    Logger.WriteDebugMessage("Got Provider site/facility");
                }
            }

            Email.From = CvtHelper.GetWorkflowOwner("Service Activity Notification", OrganizationService);
            var serviceType = Tsa.cvt_specialty.Name;
            Logger.WriteDebugMessage(string.Format("Retrieved pat {0} and pro {1} facilities and set the email sender {2}", patStation, proStation, Email.From.ToString()));
            if (Tsa.cvt_specialtysubtype != null)
                serviceType += " : " + Tsa.cvt_specialtysubtype.Name;
            #endregion

            #region Adding Scheduler Teams to TO, editing Subject
            Logger.WriteDebugMessage("provCheck");
            if (provCheck == false)
            {
                Logger.WriteDebugMessage("provCheck == false. User creating Appt is on the Provider Scheduler team, no need to send the email to that team now.");
                //Add Prov Scheduler Team Members to To Line
                foreach (ActivityParty ap in provMembers.Entities)
                {
                    To.Add(ap);
                    Logger.WriteDebugMessage($"Adding to email.TO: {ap.PartyId}");
                }
            }

            if (providerSite != null)
            {
                providerLocation = $"Provider Site: {providerSite.mcs_name}<br/>";
                Email.Subject = $"TMP Scheduler Action: {providerSite.mcs_name} for {serviceType}";
                timeZone = (int)providerSite.mcs_TimeZone;
            }
            else if (proFacility != null)
            {
                providerLocation = "Provider Facility: " + proFacility.mcs_name + "<br/>";
                Email.Subject = $"TMP Scheduler Action: {proFacility.mcs_name} for {serviceType}";
                timeZone = (int)proFacility.mcs_Timezone;
            }
            else
            {
                Logger.WriteDebugMessage("Error: Could not find a Provider Site or Provider Facility");
                Email.Subject = $"TMP Scheduler Action: {serviceType}";
                //Should I delete the email here?
            }
            Logger.WriteDebugMessage(providerLocation);

            Logger.WriteDebugMessage("patCheck");
            if (patCheck == false)
            {
                Logger.WriteDebugMessage("patCheck == false");
                //Add Pat Scheduler Team Members to To Line
                foreach (ActivityParty ap in patMembers.Entities)
                {
                    To.Add(ap);
                    Logger.WriteDebugMessage($"Adding to email.TO: {ap.PartyId}");
                }
            }
            if (patientSite != null)
            {
                patientLocation = "Patient Site: " + patientSite.mcs_name + "<br/>";
                Email.Subject += $" to {patientSite.mcs_name}";
                timeZone = (int)patientSite.mcs_TimeZone;
            }
            else if (patFacility != null)
            {
                patientLocation = "Patient Facility: " + patFacility.mcs_name + "<br/>";
                Email.Subject += $" to {patFacility.mcs_name}";
                timeZone = (int)patFacility.mcs_Timezone;
            }
            Logger.WriteDebugMessage(patientLocation);

            Logger.WriteDebugMessage("Sorting recipents in TO");
            //Method to select distinct recipients based on recipient ID (since entire Activity Party may not be duplicate)
            To = To.GroupBy(A => A.PartyId.Id).Select(g => g.First()).ToList<ActivityParty>();
            Logger.WriteDebugMessage("Adding to Email.TO");
            Email.To = To;
            #endregion

            #region Compiling Appointment Information variables
            Logger.WriteDebugMessage("Compiling Appointment Information");

            //Patient TimeZone is used if possible
            string fullDateTime = CvtHelper.GetTimeZoneString(timeZone, ServiceAppointment.ScheduledStart.Value, OrganizationService, Logger);
            string apptLength = ServiceAppointment.ScheduledDurationMinutes.Value.ToString();


            var patientAPs = ServiceAppointment?.Customers?.ToList();
            var patientInitials = string.Empty;
            if (patientAPs != null && patientAPs.Any())
            {
                foreach (ActivityParty ap in patientAPs)
                {
                    var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, ap.PartyId.Id, new ColumnSet("mcs_last4", "firstname", "lastname"));
                    patientInitials += $"{patient?.LastName.FirstOrDefault().ToString().ToUpper()}{patient?.mcs_Last4};";

                }
            }

            string patAbbrev = string.Empty;
            if (patientInitials != string.Empty)
                patAbbrev = $"Patient: {patientInitials.TrimEnd(';')}<br/>";
            var equips = ServiceAppointment.Resources.Where(ap => ap.PartyId.LogicalName == Equipment.EntityLogicalName).ToList();

            //Added to ensure that resources from child appointments (for Group Appts) are also retrieved and included in the list of resources
            var childEquips = GetApptResources(Equipment.EntityLogicalName);
            equips.AddRange(childEquips);

            var vistaClinics = "";
            var patVistaClinics = "Patient Vista Clinic Names: ";
            var proVistaClinics = "Provider Vista Clinic Names: ";
            Logger.WriteDebugMessage("Getting Vista Clinics");

            var patientSiteId = patientSite != null ? patientSite.Id : Guid.Empty;
            var providerSiteId = providerSite != null ? providerSite.Id : Guid.Empty;
            var patientFacilityId = patFacility != null ? patFacility.Id : Guid.Empty;
            Logger.WriteDebugMessage($"ProviderSiteId: {providerSiteId}");
            Logger.WriteDebugMessage($"PatientSiteId: {patientSiteId}");
            foreach (var equipment in equips)
            {
                var e = (Equipment)OrganizationService.Retrieve(Equipment.EntityLogicalName, equipment.PartyId.Id, new ColumnSet("mcs_relatedresource"));
                if (e.mcs_relatedresource == null)
                {
                    Logger.WriteDebugMessage("Orphaned Resource has been scheduled: " + e.Name + ".  Please fix this resource and rebuild the TSA (or just re-link the equipment with the TMP Resource).");
                    break;
                }
                var resource = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName, e.mcs_relatedresource.Id, new ColumnSet("mcs_name", "mcs_type", "mcs_relatedsiteid", "mcs_facility"));
                if (resource.mcs_Type != null && resource.mcs_Type?.Value == (int)mcs_resourcetype.VistaClinic)
                {
                    //Cycle through again to match to Patient or Provider
                    //print siteids and resourceids:

                    Logger.WriteDebugMessage($"Resource SiteID: {resource.mcs_RelatedSiteId?.Id}");

                    if (resource.mcs_RelatedSiteId?.Id == patientSiteId || resource.mcs_Facility?.Id == patientFacilityId)
                        patVistaClinics += resource.mcs_name + "; ";
                    else if (resource.mcs_RelatedSiteId?.Id == providerSiteId)
                        proVistaClinics += resource.mcs_name + "; ";
                    else
                        vistaClinics += resource.mcs_name + "; ";

                }
                else if (resource.mcs_Type != null && resource.mcs_Type?.Value == (int)mcs_resourcetype.Room && resource.mcs_RelatedSiteId?.Id == patientSiteId)
                {
                    string[] words = resource.mcs_name.Split('@');
                    if (words.Length == 2)
                        patientLocation += $" Room Number: {words[0]}; ";
                }
            }

            Logger.WriteDebugMessage($"Added vista clinics to Scheduler Action email. Pro: {proVistaClinics} Pat: {patVistaClinics} Non-matching: {vistaClinics}");
            var apptHeading = $"Appointment information:  <br/>";
            var displayTime = $"Date/Time: {fullDateTime} Appointment Length: {apptLength}; <br/>";

            var apptSection = $"{apptHeading }{displayTime}{patAbbrev}{providerLocation}{patientLocation}{proVistaClinics}<br/>{patVistaClinics}<br/>";
            if (vistaClinics != "")
                apptSection += $"Other Vista Clinic(s): {vistaClinics}";
            #endregion
            var body = CvtHelper.GenerateEmailBody(ServiceAppointment.Id, ServiceAppointment.EntityLogicalName, apptSection, OrganizationService, "Click here to open the Appointment in TMP");

            var status = ServiceAppointment.StatusCode.Value == (int)serviceappointment_statuscode.ReservedScheduled ? "scheduled" : "cancelled";
            var cancelReasonCode = (status == "cancelled") ? ServiceAppointment.StatusCode.Value : -1;
            var cancelReasonText = "";
            switch (cancelReasonCode)
            {
                case (int)serviceappointment_statuscode.ClinicCanceled:
                    cancelReasonText = " (Clinic Cancelled)";
                    break;
                case (int)serviceappointment_statuscode.PatientCanceled:
                    cancelReasonText = " (Patient Cancelled)";
                    break;
                //case 10:
                //    cancelReasonText = " (Patient No Show)";
                //    break;
                case (int)serviceappointment_statuscode.TechnologyFailure:
                    cancelReasonText = " (Technology Failure)";
                    break;
                case (int)serviceappointment_statuscode.SchedulingError:
                    cancelReasonText = " (Scheduling Error)";
                    break;
            }
            var proFacName = proFacility == null ? string.Empty : "The provider facility is: "+proFacility.mcs_name + proStation +".";
            var patFacName = patFacility == null ? string.Empty : "The patient facility is: "+patFacility.mcs_name + patStation + ".";

            //if (ServiceAppointment.cvt_Type != null && ServiceAppointment.cvt_Type.Value)
            //    patFacName = "VA Video Connect";

            // Email.Description = $"A {serviceType} telehealth appointment has been {status}{cancelReasonText} at a remote facility. Please verify that this patient has been {status} in VistA. The provider facility is: {proFacName}. The patient facility is: {patFacName}. {body}";


            Email.Description = $"A {serviceType} telehealth appointment has been {status}{cancelReasonText} at a remote facility. Please verify that this patient has been {status} in VistA. {proFacName} {patFacName} {body}";

            CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
        }

        internal List<ActivityParty> GetApptResources(string filter = "")
        {
            var childResources = new List<ActivityParty>();
            var childAppts = new List<Appointment>();
            using (var srv = new Xrm(OrganizationService))
            {
                childAppts = srv.AppointmentSet.Where(a => a.cvt_serviceactivityid.Id == ServiceAppointment.Id && a.ScheduledStart.Value == ServiceAppointment.ScheduledStart.Value).ToList();
            }
            foreach (var appt in childAppts)
            {
                //If there is no entityType filter listed, then just add all members of appointment requiredAttendees
                if (string.IsNullOrEmpty(filter))
                    childResources.AddRange(appt.RequiredAttendees);
                else
                {
                    foreach (var resource in appt.RequiredAttendees)
                    {
                        //PartyID should never be null, but added null check just in case.  
                        if (resource.PartyId != null && resource.PartyId.LogicalName == filter)
                            childResources.Add(resource);
                    }
                }
            }
            Logger.WriteDebugMessage("Appointment Resources retrieved for Service Activity: " + ServiceAppointment.Id);
            return childResources;
        }
        #endregion
        internal EntityCollection returnResourceEC(out EntityCollection outUsers)
        {
            Logger.WriteDebugMessage("Starting");
            EntityCollection users = new EntityCollection();
            EntityCollection equipmentResources = new EntityCollection();
            var resources = ServiceAppointment.GetAttributeValue<EntityCollection>("resources");
            resources.Entities.AddRange(GetApptResources(string.Empty));

            //Get the users from the resource list (filter out equipment)
            Logger.WriteDebugMessage("About to Split Resources into Users and Equipment.");
            foreach (var res in resources.Entities)
            {
                var party = res.ToEntity<ActivityParty>();
                if (party.PartyId.LogicalName == SystemUser.EntityLogicalName)
                {
                    Logger.WriteDebugMessage("AP is User.");
                    ActivityParty p = new ActivityParty()
                    {
                        PartyId = new EntityReference(SystemUser.EntityLogicalName, party.PartyId.Id)
                    };
                    users.Entities.Add(p);
                }
                else
                {
                    Logger.WriteDebugMessage("AP is Equipment.");
                    Equipment e = (Equipment)OrganizationService.Retrieve(Equipment.EntityLogicalName, party.PartyId.Id, new ColumnSet("equipmentid", "mcs_relatedresource"));
                    if (e.mcs_relatedresource != null)
                    {
                        mcs_resource equip = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName, e.mcs_relatedresource.Id, new ColumnSet(true));
                        //Sort all of these either Pro or Pat and Tech or Room
                        equipmentResources.Entities.Add(equip);
                    }
                }
            }
            Logger.WriteDebugMessage("Split Resources into Users and Equipment.");
            outUsers = users;
            return equipmentResources;
        }
        internal string returnTeamMemberPhones(Guid teamId, out List<SystemUser> outTCTList)
        {
            Logger.setMethod = "returnTeamMemberPhones";
            Logger.WriteGranularTimingMessage("Starting returnTeamMemberPhones");

            string AllTctPhones = string.Empty;
            List<SystemUser> TCTList = new List<SystemUser>();
            using (var srv = new Xrm(OrganizationService))
            {
                var teamMembers = srv.TeamMembershipSet.Where(tm => tm.TeamId.Value == teamId).ToList();

                if (teamMembers.Count != 0)
                {
                    Logger.WriteDebugMessage("Found Team Members: " + teamMembers.Count);
                    foreach (TeamMembership result in teamMembers)
                    {
                        SystemUser tct = (SystemUser)OrganizationService.Retrieve(SystemUser.EntityLogicalName, result.SystemUserId.Value, new ColumnSet("mobilephone", "cvt_officephone", "firstname", "lastname", "internalemailaddress"));

                        var TCTName = tct.FirstName + " " + tct.LastName;
                        var TCTPhone = tct.MobilePhone;  //Use the TCT number

                        if (TCTPhone == null)
                            TCTPhone = tct.cvt_officephone;  //Use the TCT number
                        //TCTEmail = tct.InternalEMailAddress;
                        TCTList.Add(tct);

                        //Add TCT Phone to string if exists
                        if (TCTPhone != null)
                        {
                            if (AllTctPhones != String.Empty)
                                AllTctPhones += " OR ";
                            AllTctPhones += TCTName + " at " + TCTPhone;
                        }
                    }
                }
                outTCTList = TCTList;
                return AllTctPhones;
            }


        }
        #region Scheduled/Canceled Service Appointment Notification Email
        /// <summary>
        /// Primary function which generates the Service Activity notification (including ical)
        /// </summary>
        /// <param name="isHomeMobileGroup">Home Mobile Groups have different types of notifications, so this is sent on subsequent emails (also causing iCals to not get sent to Provider)</param>
        /// <param name="action">Is either Addition, Cancelation, or Addition and Cancelation</param>
        internal void NotifyParticipantsOfAppointment(bool isCreateOrCancel, string action)
        {
            Logger.WriteDebugMessage("Beginning NotifyParticipantsOfAppointment Method");
            mcs_site patientSite = null;
            mcs_site providerSite = null;
            mcs_facility patFacility = null;
            mcs_facility proFacility = null;

            bool _phoneappt = ServiceAppointment.cvt_TelephoneCall == true ? true : false;

            Logger.WriteDebugMessage("***** Current Action: " + action.ToString() + " *****");
            using (var srv = new Xrm(OrganizationService))
            {
                #region Provider and Patient Site/Facility
                Logger.WriteDebugMessage("Attempting to get *PATIENT* site/facility from ServiceAppointment");
                if (ServiceAppointment.mcs_relatedsite != null)
                {
                    patientSite = srv.mcs_siteSet.FirstOrDefault(pat => pat.mcs_siteId == ((EntityReference)ServiceAppointment.mcs_relatedsite).Id);
                    //now get the facility from the TMPSite
                    patFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.mcs_facilityId == ((EntityReference)patientSite.mcs_FacilityId).Id);
                    Logger.WriteDebugMessage("Got Patient site/facility from SA.");

                    if (patFacility == null && Tsa.cvt_patientfacility != null)
                    {
                        patFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.mcs_facilityId == ((EntityReference)Tsa.cvt_patientfacility).Id); ;
                        if (patFacility != null)
                            Logger.WriteDebugMessage("Got Patient facility from SP.");
                    }
                }

                Logger.WriteDebugMessage("Attempting to get *Provider* site/facility from ServiceAppointment ( C )");
                if (ServiceAppointment.mcs_relatedprovidersite != null)
                {
                    Logger.WriteDebugMessage("check 4");
                    providerSite = srv.mcs_siteSet.FirstOrDefault(pro => pro.mcs_siteId == ((EntityReference)ServiceAppointment.mcs_relatedprovidersite).Id);
                    //now get the facility from the TMPSite
                    proFacility = srv.mcs_facilitySet.FirstOrDefault(xFac => xFac.mcs_facilityId == ((EntityReference)providerSite.mcs_FacilityId).Id);
                    Logger.WriteDebugMessage("Got Provider site/facility");
                }

                #endregion

                Logger.WriteDebugMessage("Attempting to get TCT Teams");
                #region TCT Teams
                //Get Pro TCTs
                if (providerSite?.cvt_tctteam != null)
                    proTCTs = returnTeamMemberPhones(providerSite.cvt_tctteam.Id, out proTCTList);
                else if (providerSite != null)
                {
                    //Find TCT Team
                    var relatedProSiteTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Type.Value == (int)Teamcvt_Type.Staff && t.cvt_TMPSite.Id == providerSite.Id);
                    proTCTs = returnTeamMemberPhones(relatedProSiteTeam.Id, out proTCTList);
                }
                Logger.WriteDebugMessage("Got TCT Teams");

                //If CVT to Home, there is no patient site
                if ((ServiceAppointment.cvt_Type != true) && (ServiceAppointment.mcs_groupappointment != true))
                {
                    //Get Pat TCTs
                    if (patientSite?.cvt_tctteam != null)
                        patTCTs = returnTeamMemberPhones(patientSite.cvt_tctteam.Id, out patTCTList);
                    else if (patientSite != null)
                    {
                        var relatedPatSiteTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Type.Value == (int)Teamcvt_Type.Staff && t.cvt_TMPSite.Id == patientSite.Id);
                        patTCTs = returnTeamMemberPhones(relatedPatSiteTeam.Id, out patTCTList);
                    }
                    //Get the Site's Emergency Contact Info
                    if (patientSite?.cvt_Local911 != null)
                        SiteLocal911Phone = patientSite?.cvt_Local911; //Use the Local 911
                    if (patientSite?.cvt_phone != null)
                        SiteMainPhone = patientSite?.cvt_phone; //Use the Site Phone
                }
                #endregion
                Logger.WriteDebugMessage("Attempting to Split Resources into Users vs Equipment");
                #region Split Resources into Users vs Equipment
                //Get the resources listed on the service activity
                EntityCollection users = new EntityCollection();
                EntityCollection equipmentResources = new EntityCollection();
                equipmentResources = returnResourceEC(out users);
                Logger.WriteDebugMessage($"Total Users: {users?.TotalRecordCount}");
                Logger.WriteDebugMessage($"Total Equipment: {equipmentResources?.TotalRecordCount}");
                #endregion

                #region Sorting through Resources
                //select which users to send the email to (providers/patients): null means provider side, 1 means patient side (and 0 means both)
                var spProResources = getPRGs("provider", OrganizationService);
                var spPatResources = getPRGs("patient", OrganizationService);

                var providerResources = GetRecipients(users, spProResources);
                var patientResources = GetRecipients(users, spPatResources);
                var clinicianList = GetRecipients(users, spProResources);

                Logger.WriteDebugMessage("About to classify techs.");
                var providerTechs = ClassifyResources(equipmentResources, spProResources, (int)mcs_resourcetype.Technology);
                var patientTechs = ClassifyResources(equipmentResources, spPatResources, (int)mcs_resourcetype.Technology);
                Logger.WriteDebugMessage("About to classify rooms.");
                var providerRooms = ClassifyResources(equipmentResources, spProResources, (int)mcs_resourcetype.Room);
                var patientRooms = ClassifyResources(equipmentResources, spPatResources, (int)mcs_resourcetype.Room);

                Logger.WriteDebugMessage("Finished Classifying Resources. Building Body.");
                //Logger.WriteDebugMessage($"Total ProUsers matched: {providerResources?.Count().ToString()}");
                //Logger.WriteDebugMessage($"Total PatUsers matched: {patientResources?.Count().ToString()}");

                //format body of the email (telepresenters can be duplicated)
                var patSiteString = patientSite != null ? patientSite?.mcs_name : string.Empty;
                if (patSiteString == string.Empty || patSiteString == null)
                {
                    patSiteString = (ServiceAppointment.mcs_groupappointment.Value && !ServiceAppointment.cvt_Type.Value && Tsa?.cvt_patientfacility != null) ? Tsa?.cvt_patientfacility?.Name : "VA Video Connect";
                }
                #endregion

                #region Build Email
                var timeZone = CvtHelper.GetSiteTimeZoneCode(ServiceAppointment, OrganizationService, Logger);
                var fullDateTime = CvtHelper.GetTimeZoneString(timeZone, ServiceAppointment.ScheduledStart.Value, OrganizationService, Logger);
                var apptLength = ServiceAppointment.ScheduledDurationMinutes?.ToString();
                string emailSubject = Email.Subject;

                Logger.WriteDebugMessage($"About to call formatNotificationEmailBody to populate  message description.");
                //Create New email

                Email.Description = formatNotificationEmailBody(providerSite, patientSite, providerTechs, patientTechs, providerRooms, patientRooms, patientResources, providerResources, isCreateOrCancel, action, fullDateTime, apptLength, out emailSubject, out var emailPlainTextdescription, out string patientSideBody, out var emailPlainTextdescriptionPatientSide, out string patientTelephoneAndVCCBody, out string emailpatTypeResourceReqBody);
                Logger.WriteDebugMessage("About to format Email Subject");
                Email.Subject = (string.IsNullOrEmpty(emailSubject) ? Email.Subject : emailSubject).Trim();
               // if ((Email.Subject.IndexOf(patSiteString) == -1))
               // {
                    Email.Subject += " " + patSiteString + " " + fullDateTime;
                    Logger.WriteDebugMessage($"Adding patient site string to the subject");
               // }
                //else
                    Logger.WriteDebugMessage($"Email subject already contains Patient site string");

                Email.Subject = Email.Subject.Contains("canceled") ? Email.Subject.Replace("canceled", "cancelled") : Email.Subject;
                Email.Subject = Email.Subject.Contains("Canceled") ? Email.Subject.Replace("Canceled", "Cancelled") : Email.Subject;
                Logger.WriteDebugMessage($"after emailSubject: { Email.Subject }");
                Logger.WriteDebugMessage("Email Subject Complete");
                //Combine the lists and then add them as the email recipients
                //providerResources.AddRange(patientResources);
                Email.From = CvtHelper.GetWorkflowOwner("Service Activity Notification", OrganizationService);
                providerResources.AddRange(proTCTList);
                Email.To = CvtHelper.SetPartyList(providerResources);

                List<SystemUser> patientTo = patientResources;
                patientTo.AddRange(patTCTList);

                Email patientSideEmail = new Email
                {
                    Description = patientSideBody,
                    Subject = "SKIP",
                    To = CvtHelper.SetPartyList(patientTo),
                    From = Email.From,
                    mcs_RelatedServiceActivity = Email.mcs_RelatedServiceActivity,
                    OwnerId = Email.OwnerId
                };

                Logger.WriteDebugMessage("Sorting recipents in TO");
                //Method to select distinct recipients based on recipient ID (since entire Activity Party may not be duplicate)
                Email.To = Email.To.GroupBy(A => A.PartyId.Id).Select(g => g.First()).ToList();
                patientSideEmail.To = patientSideEmail.To.GroupBy(A => A.PartyId.Id).Select(g => g.First()).ToList();
                // OrganizationService.Update(email);

                Logger.WriteDebugMessage($"*~*~*~ CURRENT EMAIL SUBJECT: {Email.Subject}");

                if ((Email.Subject.Substring(0, 26) == "New Patient Added to Group") || (Email.Subject.Substring(0, 30) == "New Patient Removed from Group"))
                { CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger); }

                //Send a Calendar Appointment if the appointment is scheduled (if canceled, send cancellation update)
                //If just a patient addition or removal, dont attach calendar or send additional patient email.  
                if (isCreateOrCancel)
                {
                    Logger.WriteDebugMessage("isCreateOrCancel = true. Handing Provider Side specific Email.");
                    CvtHelper.CreateCalendarAppointmentAttachment(Email, ServiceAppointment, ServiceAppointment.StatusCode.Value, stethIP, OrganizationService, Logger, emailPlainTextdescription);
                    Logger.WriteDebugMessage("UpdateSendEmail Provider Side specific Email.");
                    CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);

                    if (string.IsNullOrEmpty(action) && (ServiceAppointment.cvt_notificationsent == null || !ServiceAppointment.cvt_notificationsent.Value))
                        OrganizationService.Update(new ServiceAppointment { Id = ServiceAppointment.Id, cvt_notificationsent = true });

                    if (ServiceAppointment.cvt_Type == false || (ServiceAppointment.cvt_Type == true && ServiceAppointment.cvt_patientsiteresourcesrequired == true && ServiceAppointment.cvt_TelephoneCall == true) || (ServiceAppointment.cvt_Type == true && ServiceAppointment.cvt_patientsiteresourcesrequired == true && ServiceAppointment.cvt_TelephoneCall == false))
                    {
                        Logger.WriteDebugMessage("Handling Patient Side specific Email.");

                        if ((ServiceAppointment.cvt_Type == true && ServiceAppointment.cvt_patientsiteresourcesrequired == true && ServiceAppointment.cvt_TelephoneCall == true) || (ServiceAppointment.cvt_Type == true && ServiceAppointment.cvt_patientsiteresourcesrequired == true && ServiceAppointment.cvt_TelephoneCall == false))
                        {
                            if (ServiceAppointment.mcs_relatedsite != null)
                            {

                                EntityReference relatedSite = ServiceAppointment.GetAttributeValue<EntityReference>("mcs_relatedsite");
                                Entity relatedSiteTeam = OrganizationService.Retrieve(relatedSite.LogicalName, relatedSite.Id, new ColumnSet("cvt_tctteam"));

                                EntityReference team = relatedSiteTeam.GetAttributeValue<EntityReference>("cvt_tctteam");
                                if (team != null)
                                {

                                    var toListCount = RetrieveTeamMembers(OrganizationService, patientSideEmail, team.Id);
                                    if (toListCount.Count > 0)
                                    {
                                        patientSideEmail.To = RetrieveTeamMembers(OrganizationService, patientSideEmail, team.Id);
                                    }

                                }

                            }

                            if (ServiceAppointment.cvt_Type == true && ServiceAppointment.cvt_patientsiteresourcesrequired == true && ServiceAppointment.cvt_TelephoneCall == false)
                            {
                                Email patientTypeTelephone = new Email
                                {
                                    Description = emailpatTypeResourceReqBody,
                                    Subject = Email.Subject,
                                    To = patientSideEmail.To,
                                    From = Email.From,
                                    mcs_RelatedServiceActivity = Email.mcs_RelatedServiceActivity,
                                    OwnerId = Email.OwnerId
                                };

                                var patTypeResourceCreate = OrganizationService.Create(patientTypeTelephone);
                                Email patEmailGet = (Email)OrganizationService.Retrieve(Email.EntityLogicalName, patTypeResourceCreate, new ColumnSet(true));
                                patEmailGet.Subject = Email.Subject;
                                CvtHelper.CreateCalendarAppointmentAttachment(patEmailGet, ServiceAppointment, ServiceAppointment.StatusCode.Value, stethIP, OrganizationService, Logger, emailPlainTextdescriptionPatientSide);
                                Logger.WriteDebugMessage("UpdateSendEmail Patient Side specific Email.");
                                CvtHelper.UpdateSendEmail(patEmailGet, OrganizationService, Logger);
                            }
                            else if (ServiceAppointment.cvt_Type == true && ServiceAppointment.cvt_patientsiteresourcesrequired == true && ServiceAppointment.cvt_TelephoneCall == true)
                            {
                                patientSideEmail["description"] = patientTelephoneAndVCCBody;
                            }

                        }

                        if (ServiceAppointment.cvt_Type == false || (ServiceAppointment.cvt_Type == true && ServiceAppointment.cvt_patientsiteresourcesrequired == true && ServiceAppointment.cvt_TelephoneCall == true))
                        {
                            var patSideEmail = OrganizationService.Create(patientSideEmail);
                            Email patEmail = (Email)OrganizationService.Retrieve(Email.EntityLogicalName, patSideEmail, new ColumnSet(true));
                            patEmail.Subject = Email.Subject;
                            CvtHelper.CreateCalendarAppointmentAttachment(patEmail, ServiceAppointment, ServiceAppointment.StatusCode.Value, stethIP, OrganizationService, Logger, emailPlainTextdescriptionPatientSide);
                            Logger.WriteDebugMessage("UpdateSendEmail Patient Side specific Email.");
                            CvtHelper.UpdateSendEmail(patEmail, OrganizationService, Logger);
                        }
                    }

                    // Only send patient email for VA Video Connect TSAs (but not for groups on update)
                    // Future Phase TODO: use static VMRs and notify patient to hit desktop icon
                    bool isCancelled = (ServiceAppointment.StatusCode.Value == 9 || ServiceAppointment.StatusCode.Value == 917290000 || ServiceAppointment.StatusCode.Value == 917290001 || ServiceAppointment.StatusCode.Value == 917290008 || ServiceAppointment.StatusCode.Value == 917290009) ? true : false; //Cancellation 

                    bool? patient = null;
                    var meetingSpace = getPatientVirtualMeetingSpace(out patient);
                    if (meetingSpace == string.Empty)
                        meetingSpace = "Please Contact Your Clinician for Web Meeting Details";

                    Logger.WriteDebugMessage("Phone Appointment?: " + _phoneappt.ToString());
                    Logger.WriteDebugMessage("State Code?: " + ServiceAppointment.StateCode.Value.ToString());
                    if ((!_phoneappt) || (ServiceAppointment.StateCode == ServiceAppointmentState.Canceled))
                    {
                        //Create and Send Email to Patient/Veteran (copy sender from Provider Email)                       
                        Logger.WriteDebugMessage($"Total Clinicians being passed into SendPatientEmail: {clinicianList.Count().ToString()}");
                        SendPatientEmail(clinicianList, isCancelled); //VVC
                    }
                }
                else
                {
                    Logger.WriteDebugMessage("isCreateOrCancel = false, deleting emails.");
                    OrganizationService.Delete(Email.EntityLogicalName, Email.Id);
                }
                #endregion
            }
        }

        internal static List<ActivityParty> RetrieveTeamMembers(IOrganizationService OrganizationService, Email email, Guid TeamId)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var teamMembers = srv.TeamMembershipSet.Where(t => t.TeamId == TeamId).ToList();
                var recipientList = new List<ActivityParty>();

                foreach (var member in teamMembers)
                {
                    var user = srv.SystemUserSet.FirstOrDefault(u => u.Id == member.SystemUserId && u.IsDisabled == false);

                    if (user != null)
                    {
                        if ((!String.IsNullOrEmpty(user.InternalEMailAddress)))
                        {
                            var party = new ActivityParty()
                            {
                                ActivityId = new EntityReference(email.LogicalName, email.Id),
                                PartyId = new EntityReference(SystemUser.EntityLogicalName, user.Id)
                            };
                            recipientList.Add(party);
                        }

                    }
                }
                return recipientList;
            }
        }

        /// <summary>
        /// Take the various input lists and format the Email body
        /// </summary>
        /// <param name="providerTechs">List of Technology type resources for the provider side</param>
        /// <param name="patientTechs">List of Technology type resources for the patient side</param>
        /// <param name="providerRooms">List of Room type resources for the provider side</param>
        /// <param name="patientRooms">List of Room type resources for the patient side</param>
        /// <param name="telepresenters">List of Users for the patient side</param>
        /// <param name="providers">List of Users for the provider side</param>
        /// <param name="isCreateOrCancel">indicates whether this is the initial notification or a modification (addition/removal of patient)</param>
        /// <remarks>if this is just a patient addition/removal, then there is a separate email for the patient and the provider doesn't get an iCal change</remarks>
        /// <param name="action">string value indicating whether it is an addition, removal, or both</param>
        /// <param name="convertedDate"></param>
        /// <returns></returns>
        internal string formatNotificationEmailBody(mcs_site providerSite, mcs_site patientSite, List<mcs_resource> providerTechs, List<mcs_resource> patientTechs, List<mcs_resource> providerRooms, List<mcs_resource> patientRooms, List<SystemUser> telepresenters, List<SystemUser> providers, bool isCreateOrCancel, string action, string fullDateTime, string apptLength, out string subject, out string emailPlainTextDescription, out string emailBodyPatSide, out string emailPlainTextDescriptionPatSide, out string patientTelephoneAndVCCBody, out string emailpatTypeResourceReqBody)
        {
            Logger.WriteDebugMessage("Starting Formatting Email Body");
            string emailBody = string.Empty;
            emailPlainTextDescription = string.Empty;

            emailBodyPatSide = string.Empty;
            emailPlainTextDescriptionPatSide = string.Empty;
            patientTelephoneAndVCCBody = string.Empty;
            emailpatTypeResourceReqBody = string.Empty;

            subject = string.Empty;
            int showHelper = 1;

            if (!isCreateOrCancel)
            {
                var timeZone = CvtHelper.GetSiteTimeZoneCode(ServiceAppointment, OrganizationService, Logger);
                var currentDateTime = CvtHelper.GetTimeZoneString(timeZone, ServiceAppointment.ModifiedOn.Value, OrganizationService, Logger);
                var subjectAction = action;
                if (action.Contains("Addition") && action.Contains("Cancellation"))
                    subjectAction = "Added/Removed in";
                else if (action.Contains("Addition"))
                    subjectAction = "Added to";
                else if (action.Contains("Cancellation"))
                    subjectAction = "Removed from";

                subject = $"New Patient {subjectAction} Group VA Video Connect (VVC) Appointment on {currentDateTime}";
                var numPatients = ServiceAppointment.Customers == null ? 0 : ServiceAppointment.Customers.ToList().Count;
                emailBody += emailPlainTextDescription += $"This is an automated Message to notify you that a there has been a patient {action} to your Group Telehealth Appointment scheduled at {fullDateTime}. " +
                        $"You now have {numPatients} patients scheduled for this appointment. The details are listed below: ";

                emailBody += $"<br /><br />";
                emailPlainTextDescription += "\\n\\n";
            }

            if (action == "cancel")
            {
                emailBody += $"This is an automated message to notify you that a Telehealth Appointment previously scheduled for {fullDateTime}, for {apptLength} minutes, has been <font color='red'><u>cancelled</u></font>.  Please open the attachment and click \"Remove from Calendar\" to remove this event from your calendar.  The details are listed below: </br></br>";
                emailPlainTextDescription += $"This is an automated message to notify you that a Telehealth Appointment previously scheduled for {fullDateTime}, for {apptLength} minutes, has been cancelled.  Please open the attachment and click \"Remove from Calendar\" to remove this event from your calendar.  The details are listed below: \\n\\n";
            }
            else //if (ServiceAppointment.StatusCode.Value == (int)serviceappointment_statuscode.ReservedScheduled)
            {
                emailBody += $"This is an automated Message to notify you that a Telehealth Appointment has been <font color='green'><u>Scheduled</u></font> for {fullDateTime}, for {apptLength} minutes. Please open the attachment and click \"Save and Close\" to add this event to your calendar.  The details are listed below: <br/><br/>";
                emailPlainTextDescription += $"This is an automated Message to notify you that a Telehealth Appointment has been Scheduled for {fullDateTime}, for {apptLength} minutes. Please open the attachment and click \"Save and Close\" to add this event to your calendar.  The details are listed below: \\n\\n";
            }

            //Provider variables:
            EntityCollection users = new EntityCollection();
            EntityCollection equipmentResources = new EntityCollection();
            equipmentResources = returnResourceEC(out users);
            var providerSiteId = providerSite != null ? providerSite.Id : Guid.Empty;
            var spProResources = getPRGs("provider", OrganizationService);

            #region ProRooms
            Logger.WriteDebugMessage("ProRooms section.");
            //string providerRoomsString = null;
            //string providerRoomsStringPlain = null;
            string providerRoomsString = string.Empty;
            string providerRoomsStringPlain = string.Empty;
            string ProRoom = "";
            foreach (mcs_resource r in providerRooms)
            {
                providerRoomsString += "<b><u>Room:</u></b> " + r.mcs_name;
                providerRoomsStringPlain += "Room: " + r.mcs_name;
                if (r.cvt_phone != null)
                {
                    providerRoomsString += (providerRoomsString == null) ? r.cvt_phone : ", " + r.cvt_phone;
                    providerRoomsStringPlain += (providerRoomsStringPlain == null) ? r.cvt_phone : ", " + r.cvt_phone;
                    ProRoom += r.cvt_phone;
                }
                providerRoomsString += "<br/>";
                providerRoomsStringPlain += "\\n";
            }

            #endregion
            #region ProVC
            Logger.WriteDebugMessage("ProVC section.");
            string proVistaClinics = null;

            var proVCs = ClassifyResources(equipmentResources, spProResources, (int)mcs_resourcetype.VistaClinic);
            foreach (var record in proVCs)
            {
                proVistaClinics += $" {record?.mcs_name}; ";
            }
            #endregion
            #region ProTech
            Logger.WriteDebugMessage("ProTech section.");
            string providerTechsString = "";
            string providerTechsStringPlain = "";

            var proTechs = ClassifyResources(equipmentResources, spProResources, (int)mcs_resourcetype.Technology);
            //foreach (var record in proTechs)
            //{
            //    providerTechsString += $" {record?.mcs_name}; ";
            //}
            foreach (mcs_resource r in proTechs)
            {
                providerTechsString += r?.mcs_name;
                providerTechsStringPlain += r?.mcs_name;
                if (r?.cvt_relateduser != null && r?.cvt_relateduser.Id != Guid.Empty)
                {
                    SystemUser poc = (SystemUser)OrganizationService.Retrieve(SystemUser.EntityLogicalName, r.cvt_relateduser.Id, new ColumnSet("fullname", "mobilephone", "cvt_officephone", "cvt_teleworkphone"));
                    providerTechsString += "; POC Name: " + poc?.FullName + "; ";
                    providerTechsStringPlain += "; POC Name: " + poc?.FullName + "; ";
                    var phone = poc?.MobilePhone ?? poc?.cvt_officephone;

                    //If TSA is telework (true), then add that number here as well.
                    providerTechsString += ((Tsa.cvt_providerlocationtype != null) && (Tsa.cvt_providerlocationtype.Value == (int)cvt_resourcepackagecvt_providerlocationtype.Telework) && (poc.cvt_TeleworkPhone != null)) ? "POC Telework Phone #: " + poc.cvt_TeleworkPhone + "; " : "";
                    providerTechsStringPlain += ((Tsa.cvt_providerlocationtype != null) && (Tsa.cvt_providerlocationtype.Value == (int)cvt_resourcepackagecvt_providerlocationtype.Telework) && (poc.cvt_TeleworkPhone != null)) ? "POC Telework Phone #: " + poc.cvt_TeleworkPhone + "; " : "";

                    providerTechsString += (phone != null) ? "POC Phone #: " + phone : "";
                    providerTechsStringPlain += (phone != null) ? "POC Phone #: " + phone : "";
                }
                providerTechsString += "<br/>";
                providerTechsStringPlain += "\\n";

                providerTechsString += getComponents(r, ServiceAppointment, out string outProComponentPlain);
                providerTechsStringPlain += outProComponentPlain;
            }
            #endregion
            #region Providers
            Logger.WriteDebugMessage("Providers section.");
            //string providersString = null;
            //string providersStringPlain = null;
            string providersString = string.Empty;
            string providersStringPlain = string.Empty;

            //string providersStringGuest = null;
            //string providersStringPlainGuest = null;
            string providersStringGuest = string.Empty;
            string providersStringPlainGuest = string.Empty;

            foreach (SystemUser t in providers)
            {
                Logger.WriteDebugMessage($"Assessing a provider. {t?.FullName}");
                var phone = t?.cvt_officephone ?? t?.MobilePhone;
                providersString += "<b><u>Provider:</u></b> " + t?.FullName;
                providersStringGuest += "<b><u>Provider:</u></b> " + t?.FullName;
                providersStringPlain += "Provider: " + t?.FullName;
                providersStringPlainGuest += "Provider: " + t?.FullName;

                providersString += (phone != null) ? "; Phone: " + phone : "";
                providersStringGuest += (phone != null) ? "; Phone: " + phone : "";
                providersStringPlain += (phone != null) ? "; Phone: " + phone : "";
                providersStringPlainGuest += (phone != null) ? "; Phone: " + phone : "";

                //If TSA is telework (true), then add that number here as well.
                if ((Tsa?.cvt_providerlocationtype != null) && (Tsa?.cvt_providerlocationtype.Value == (int)cvt_resourcepackagecvt_providerlocationtype.Telework))
                {
                    //Check user for telework number                  
                    providersString += (t?.cvt_TeleworkPhone != null) ? "; Telework Phone: " + t?.cvt_TeleworkPhone + ";" : "";
                    providersStringGuest += (t?.cvt_TeleworkPhone != null) ? "; Telework Phone: " + t?.cvt_TeleworkPhone + ";" : "";
                    providersStringPlain += (t?.cvt_TeleworkPhone != null) ? "; Telework Phone: " + t?.cvt_TeleworkPhone + ";" : "";
                    providersStringPlainGuest += (t?.cvt_TeleworkPhone != null) ? "; Telework Phone: " + t?.cvt_TeleworkPhone + ";" : "";
                }

                if (Tsa != null && Tsa.cvt_patientlocationtype != null && Tsa.cvt_patientlocationtype.Value != (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnect)
                {
                    providersString += "<br/><ul>";
                    providersStringGuest += "<br/><ul>";
                    providersStringPlain += "\\n";
                    providersStringPlainGuest += "\\n";
                    Logger.WriteDebugMessage("About to Add/Build URLs.");
                    providersString += (t?.cvt_StaticVMRLink != "" && t?.cvt_StaticVMRLink != null) ? $"<li>{CvtHelper.buildHTMLUrl(t?.cvt_StaticVMRLink, "Provider VMR Host Link Here")}</li>" : "<li>No Provider VMR Host Link</li>";
                    providersString += (t?.cvt_dialerurl != "" && t?.cvt_dialerurl != null) ? $"<li>{CvtHelper.buildHTMLUrl(t?.cvt_dialerurl, "VVC Dialer Link Here")}</li></ul>" : "<li>No VVC Dialer Link</li></ul>";
                    providersStringGuest += (t?.cvt_providervmrguestlink != "" && t?.cvt_providervmrguestlink != null) ? $"<li>{CvtHelper.buildHTMLUrl(t?.cvt_providervmrguestlink, "Provider VMR Guest Link Here")}</li></ul>" : "<li>No Provider VMR Guest Link</li></ul>";

                    providersStringPlain += $"-Provider VMR Host Link Here: {t?.cvt_StaticVMRLink}\\n-VVC Dialer Link Here: {t?.cvt_dialerurl}\\n";
                    providersStringPlainGuest += $"-Provider VMR Guest Link Here: {t?.cvt_providervmrguestlink}\\n";
                    Logger.WriteDebugMessage("Finished adding URLs.");
                }
            }
            Logger.WriteDebugMessage("Finished Provider section.");
            #endregion

            //PROVIDER SITE INFORMATION:
            /*
             Provider Site Information: 
             Room: Room 811 @ 554 
             Vista Clinic: GLE TH PACT PC TEST NP PRO @ Glendale, CO CBOC (554) ; 
             Technologies: RBVC 811 : 15729 @ 554; POC Name: Supreme, Technician; POC Telework Phone #: 303-123-4567;POC Phone #: 303-986-5432
                Provider: Ibee, Provider; Phone: 303-986-5432; Telework Phone: 303-123-4567;
                • Codec, Hardware; CEVN Alias: 9512267
                • Provider VMR Host Link Here
                • Provider VMR Guest Link Here
                • VVC Dialer Link Here
             */
            //emailBody += $"<b><font color='#0070c0'>Provider Site Information: </font></b><br/>{providerRoomsString}<b><u>Vista Clinic:</u></b>  {proVistaClinics}<br/><b><u>Technologies:</u></b> <br/>{providerTechsString}<br/>";
            //emailBodyPatSide += $"<b><font color='#0070c0'>Provider Site Information: </font></b><br/>{providerRoomsString}<b><u>Vista Clinic:</u></b>  {proVistaClinics}<br/><b><u>Technologies:</u></b> <br/>{providerTechsString}<br/>";
            //emailPlainTextDescription += $"Provider Site Information: \\n{providerRoomsStringPlain}Vista Clinic: {proVistaClinics}\\nTechnologies: <br/>{providerTechsStringPlain}\\n";
            //emailPlainTextDescriptionPatSide += $"Provider Site Information: \\n{providerRoomsStringPlain}Vista Clinic: {proVistaClinics}\\nTechnologies: <br/>{providerTechsStringPlain}\\n";

            emailBody += $"<b><font color='#0070c0'>Provider Site Information: </font></b><br/><b><u>Technologies:</u></b> <br/>{providerTechsString}<br/>";
            emailpatTypeResourceReqBody = $"<b><font color='#0070c0'>Provider Site Information: </font></b><br/><b><u>Technologies:</u></b> <br/>{providerTechsString}<br/>";
            emailBodyPatSide += $"<b><font color='#0070c0'>Provider Site Information: </font></b><br/><b><u>Technologies:</u></b> <br/>{providerTechsString}<br/>";
            emailPlainTextDescription += $"Provider Site Information: \\n{providerRoomsStringPlain}\\nTechnologies: <br/>{providerTechsStringPlain}\\n";
            emailPlainTextDescriptionPatSide += $"Provider Site Information: \\n{providerRoomsStringPlain}\\nTechnologies: <br/>{providerTechsStringPlain}\\n";

            emailBody += $"{providersString}<br/>";
            emailpatTypeResourceReqBody += $"{providersString}<br/>";
            emailBodyPatSide += $"{providersStringGuest}<br/>";
            emailPlainTextDescription += $"{providersStringPlain}\\n";
            emailPlainTextDescriptionPatSide += $"{providersStringPlainGuest}\\n";

            /*
            Patient Site Information: 
            Patient: (Last and First Initials only with Patient's last FOUR)
            Room: Bldg. Mountain Towers, Room 817 @ 554; Note: The patient care site is NOT DEA registered. 
            Vista Clinic: GLE TH PACT PC TEST NP PAT @ Glendale, CO CBOC (554) ; 
            Technologies: PC Cart 817 : CEVN ID @ 554; POC Name: Mata, Sam; POC Phone #: 303-986-5432
                • Camera, Videoconferencing / Web
             */

            #region Patient
            Logger.WriteDebugMessage("Patient section.");
            var patientAPs = ServiceAppointment?.Customers?.ToList();
            var patientInitials = string.Empty;
            if (patientAPs != null && patientAPs.Any())
            {
                foreach (ActivityParty ap in patientAPs)
                {
                    var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, ap.PartyId.Id, new ColumnSet("mcs_last4", "firstname", "lastname"));
                    patientInitials += $"{patient?.LastName.FirstOrDefault().ToString().ToUpper()}{patient?.mcs_Last4};";

                }
            }
            #endregion

            emailBody += $"<br/><b><font color='#0070c0'>Patient Site Information: </font></b><br/>";
            emailpatTypeResourceReqBody += $"<br/><b><font color='#0070c0'>Patient Site Information: </font></b><br/>";
            emailBodyPatSide += $"<br/><b><font color='#0070c0'>Patient Site Information: </font></b><br/>";
            emailPlainTextDescription += $"\\nPatient Site Information: \\n";
            emailPlainTextDescriptionPatSide += $"\\nPatient Site Information: \\n";

            #region PatientInitials
            Logger.WriteDebugMessage("Patient Initials section.");
            if (patientInitials != string.Empty)
            {
                emailBody += $"<b><u>Patient:</u></b>  {patientInitials.TrimEnd(';')}<br/>";
                emailpatTypeResourceReqBody += $"<b><u>Patient:</u></b>  {patientInitials.TrimEnd(';')}<br/>";
                emailBodyPatSide += $"<b><u>Patient:</u></b>  {patientInitials.TrimEnd(';')}<br/>";
                emailPlainTextDescription += $"Patient: {patientInitials.TrimEnd(';')}\\n";
                emailPlainTextDescriptionPatSide += $"Patient: {patientInitials.TrimEnd(';')}\\n";
            }
            #endregion
            #region PatRooms
            Logger.WriteDebugMessage("Patient Rooms section.");
            // string patientRoomsString = null;
            // string patientRoomsStringPlain = null;
            string patientRoomsString = string.Empty;
            string patientRoomsStringPlain = string.Empty;
            string DEALicensed = "";

            DEALicensed = (ServiceAppointment?.cvt_Type != null && ServiceAppointment?.cvt_Type.Value == true) ? "This is a VA Video Connect visit; consider Ryan Haight regulations prior to prescribing any controlled medications." : "";

            Logger.WriteDebugMessage("DEAL Licensed.");
            //foreach (var room in patRooms)
            //{
            //    patientLocation += $" {room?.mcs_name}; ";
            //}
            if (patientRooms != null) {
                Logger.WriteDebugMessage($"MCS Resources {patientRooms.Count}");
                
            }
            foreach (mcs_resource r in patientRooms)
            {
                Logger.WriteDebugMessage($"For each patientRooms");

                patientRoomsString += "<b><u>Room:</u></b> " + r.mcs_name;
                patientRoomsStringPlain += "Room: " + r.mcs_name;
                if (r.cvt_phone != null)
                {
                    patientRoomsString += (patientRoomsString == null) ? r.cvt_phone : ", " + r.cvt_phone;
                    patientRoomsStringPlain += (patientRoomsString == null) ? r.cvt_phone : ", " + r.cvt_phone;
                }
                if (DEALicensed == "" && r.mcs_RelatedSiteId != null)
                {
                    var resourceSite = (mcs_site)OrganizationService.Retrieve(mcs_site.EntityLogicalName, r.mcs_RelatedSiteId.Id, new ColumnSet(true));

                    if (resourceSite?.cvt_DEALicensed != null && resourceSite?.cvt_DEALicensed.Value == true)
                    {
                        patientRoomsString += ";  <u><b>Note: The patient care site is DEA registered.</u></b>";
                        patientRoomsStringPlain += ";  Note: The patient care site is DEA registered.";
                    }
                    else
                    {
                        patientRoomsString += ";  <u><b>Note: The patient care site is NOT DEA registered.</u></b>";
                        patientRoomsStringPlain += ";  Note: The patient care site is NOT DEA registered.";
                    }
                }
                patientRoomsString += "<br/>";
                patientRoomsStringPlain += "\\n";
            }
            #endregion
            Logger.WriteDebugMessage("Before Tech Type");
            bool _phoneappt = ServiceAppointment.cvt_TelephoneCall.HasValue && ServiceAppointment.cvt_TelephoneCall.Value ? true : false;
            Logger.WriteDebugMessage("TechnologyType:" + ServiceAppointment?.tmp_TechnologyType?.Value);
            isCvtTablet = ServiceAppointment?.tmp_TechnologyType?.Value == 100000000 ? true : false;

            Logger.WriteDebugMessage("isCvtTablet:" + isCvtTablet);
            if (ServiceAppointment?.cvt_Type != true) //Not VVA
            {
                var patientSiteId = patientSite != null ? patientSite.Id : Guid.Empty;
                var spPatResources = getPRGs("patient", OrganizationService);

                Logger.WriteDebugMessage("Getting Vista Clinics");
                var patVCs = ClassifyResources(equipmentResources, spPatResources, (int)mcs_resourcetype.VistaClinic);
                var patRooms = ClassifyResources(equipmentResources, spPatResources, (int)mcs_resourcetype.Room);
                var patTechs = ClassifyResources(equipmentResources, spPatResources, (int)mcs_resourcetype.Technology);
                var groupPatientSiteText = returnGroupPSSites(spPatResources, patVCs, patRooms, patTechs);
                //var patLocation = patientSite != null ? $"Patient Site: {patientSite.mcs_name}" : (Tsa.cvt_groupappointment == true && Tsa.cvt_patientfacility != null ? $"Patient Facility: {Tsa.cvt_patientfacility.Name}" : "");
                var patLocation = patientSite != null ? $"Patient Site: {patientSite.mcs_name}" : (Tsa.cvt_groupappointment == true ? $"Patient Site: {groupPatientSiteText}" : "");

                #region PatVC
                Logger.WriteDebugMessage("Patient VC section.");
                var patVistaClinics = "";
                foreach (var vc in patVCs)
                {
                    patVistaClinics += $" {vc?.mcs_name}; ";
                }
                #endregion  
                #region PatTechs
                Logger.WriteDebugMessage("Patient Techs section.");
                //string patientTechsString = null;
                //string patientTechsStringPlain = null;
                string patientTechsString = string.Empty;
                string patientTechsStringPlain = string.Empty;

                foreach (mcs_resource r in patientTechs)
                {
                    patientTechsString += r?.mcs_name;
                    patientTechsStringPlain += r?.mcs_name;
                    if (r?.cvt_relateduser != null && r?.cvt_relateduser.Id != Guid.Empty)
                    {
                        SystemUser poc = (SystemUser)OrganizationService.Retrieve(
                            SystemUser.EntityLogicalName, r.cvt_relateduser.Id, new ColumnSet("fullname", "mobilephone", "cvt_officephone"));
                        patientTechsString += "; POC Name: " + poc?.FullName + "; ";
                        patientTechsStringPlain += "; POC Name: " + poc?.FullName + "; ";

                        var phone = poc?.MobilePhone ?? poc?.cvt_officephone;
                        patientTechsString += "POC Phone #: " + phone;
                        patientTechsStringPlain += "POC Phone #: " + phone;
                    }
                    patientTechsString += "<br/>";
                    patientTechsStringPlain += "\\n";

                    patientTechsString += getComponents(r, ServiceAppointment, out string outPatComponentPlain);
                    patientTechsStringPlain += outPatComponentPlain;
                }
                //var patientDevice = "";
                //foreach (var tech in patTechs)
                //{
                //    patientDevice += $" {tech?.cvt_systemtype.ToString()}; ";
                //}
                //string telepresentersString = null;
                //foreach (SystemUser t in telepresenters)
                //{
                //    var phone = t.cvt_officephone ?? t.MobilePhone;
                //    telepresentersString += "<b><u>Telepresenter:</u></b> " + t.FullName + ": " + phone + "<br/>";
                //}
                #endregion
                Logger.WriteDebugMessage("Finished creating patient string values.");

                emailBody += $"{patientRoomsString}<b><u>Technologies:</u></b> {patientTechsString}<br/>";
                emailBodyPatSide += $"{patientRoomsString}<b><u>Technologies:</u></b> {patientTechsString}<br/>";
                emailPlainTextDescription += $"{patientRoomsStringPlain}Technologies: {patientTechsStringPlain}\\n";
                emailPlainTextDescriptionPatSide += $"{patientRoomsStringPlain}Technologies: {patientTechsStringPlain}\\n";


                /*
                Group Call-in Information:
                H.323 (Alias) dial: 9991020X
                SIP Dial-in: 9991020X@evn.va.gov

                Telephone Contact Information:
                    • To direct dial the room: 303-372-7806
                    • To reach the main phone number for the patient side clinic: 303-987-6543
                    • No Team Members listed on TCT Site Team.
                 */
                var alias = (Tsa != null && Tsa.cvt_alias != null) ? Tsa.cvt_alias : "";

                //if (Tsa != null && Tsa.cvt_patientlocationtype != null && Tsa.cvt_patientlocationtype.Value != (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnect)
                //{
                emailBody += $"<b><font color='#0070c0'>Group Call-in Information: </font></b><br/>H.323 (Alias) dial: {alias}<br/>";
                emailBodyPatSide += $"<b><font color='#0070c0'>Group Call-in Information: </font></b><br/>H.323 (Alias) dial: {alias}<br/>";
                emailPlainTextDescription += $"Group Call-in Information: \\nH.323 (Alias) dial: {alias}\\n";
                emailPlainTextDescriptionPatSide += $"Group Call-in Information: \\nH.323 (Alias) dial: {alias}\\n";

                var aliastext = alias != "" ? $"{alias}@evn.va.gov" : "";
                emailBody += $"SIP Dial-in: {aliastext}<br/>";
                emailBodyPatSide += $"SIP Dial-in: {aliastext}<br/>";
                emailPlainTextDescription += $"SIP Dial-in: {aliastext}\\n";
                emailPlainTextDescriptionPatSide += $"SIP Dial-in: {aliastext}\\n";

                Logger.WriteDebugMessage("Finished Group Call in section.");
                //}

                if (ProRoom != null || proTCTs != String.Empty)
                {
                    emailBody += "<br/><u><b>Telephone Contact Information:</u></b><br/> <ul>";
                    emailBodyPatSide += "<br/><u><b>Telephone Contact Information:</u></b><br/> <ul>";

                    emailBody += (ProRoom != null && ProRoom != "") ? "<li>To direct dial the room: " + ProRoom + "</li>" : "";
                    emailBodyPatSide += (ProRoom != null && ProRoom != "") ? "<li>To direct dial the room: " + ProRoom + "</li>" : "";

                    emailBody += (proTCTs != String.Empty) ? "<li>To contact the TCTs at the provider site, call " + proTCTs + ".</li><br/>" : "<li>No Team Members listed on TCT Site Team.</li></ul><br/>";
                    emailBodyPatSide += (proTCTs != String.Empty) ? "<li>To contact the TCTs at the provider site, call " + proTCTs + ".</li><br/>" : "<li>No Team Members listed on TCT Site Team.</li></ul><br/>";

                    emailPlainTextDescription += "\\nTelephone Contact Information:\\n";
                    emailPlainTextDescription += (ProRoom != null && ProRoom != "") ? "-To direct dial the room: " + ProRoom : "";
                    emailPlainTextDescription += (proTCTs != String.Empty) ? "-To contact the TCTs at the provider site, call " + proTCTs + ".\\n" : "-No Team Members listed on TCT Site Team.\\n";

                    emailPlainTextDescriptionPatSide += "\\nTelephone Contact Information:\\n";
                    emailPlainTextDescriptionPatSide += (ProRoom != null && ProRoom != "") ? "-To direct dial the room: " + ProRoom : "";
                    emailPlainTextDescriptionPatSide += (proTCTs != String.Empty) ? "-To contact the TCTs at the provider site, call " + proTCTs + ".\\n" : "-No Team Members listed on TCT Site Team.\\n";
                }

                Logger.WriteDebugMessage("Finished Telephone Contact Information section.");
                emailBody += $"<br/><b><font color='#0070c0'>Veterans in home Emergency Use Only- e911 Instructions</font></b><br/>Call 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location (address) where the Patient is currently located.<br/>";
                emailBodyPatSide += $"<br/><b><font color='#0070c0'>Veterans in home Emergency Use Only- e911 Instructions</font></b><br/>Call 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location (address) where the Patient is currently located.<br/>";
                emailPlainTextDescription += $"Veterans in home Emergency Use Only- e911 Instructions\\nCall 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location (address) where the Patient is currently located.\\n";
                emailPlainTextDescriptionPatSide += $"Veterans in home Emergency Use Only- e911 Instructions\\nCall 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location   (address) where the Patient is currently located.\\n";
                emailBody += CvtHelper.TechnicalAssistanceEmailFooter(out string techAssistanceEmailFooterPlainText);
                emailBodyPatSide += CvtHelper.TechnicalAssistanceEmailFooter(out string extra);
                emailPlainTextDescription += techAssistanceEmailFooterPlainText;
                emailPlainTextDescriptionPatSide += techAssistanceEmailFooterPlainText;
            }
            else  //Condition for VVC
            {
                Logger.WriteDebugMessage("Starting VVC section.");

                if (!_phoneappt) // this is not a "Phone Appointment"
                {
                    bool? patient = null;
                    var meetingSpace = getPatientVirtualMeetingSpace(out patient);
                    string sipAddress = string.Empty;
                    if (ServiceAppointment.tmp_SipAddress != null)
                         sipAddress = ServiceAppointment.tmp_SipAddress;
                    if (meetingSpace == string.Empty)
                        meetingSpace = "Please Contact Your Clinician for Web Meeting Details";

                    if (ServiceAppointment.StateCode.Value != ServiceAppointmentState.Canceled)
                    {
                        if (string.IsNullOrEmpty(subject))
                            subject = "Your VA Video Connect (VVC) Appointment has been scheduled for ";

                        //Change to read the ProviderVirtualMeetingSpace on the patient record.
                        // https://pexipdemo.com/px/vatest/#/?name=ProviderName&join=1&media=&escalate=1&conference=vatest@pexipdemo.com&pin=1234  
                        if (patient == true || !string.IsNullOrEmpty(ProviderVirtualMeetingSpace))
                        {
                            emailBody += "<br/><b>Join the appointment</b><br/>" + CvtHelper.buildHTMLUrlAlt(ProviderVirtualMeetingSpace, "Click Here to Join the VA Video Connect appointment ") + "<br/>";
                            emailpatTypeResourceReqBody += "<br/><b>Join the appointment</b><br/>" + CvtHelper.buildHTMLUrlAlt(ProviderVirtualMeetingSpace, "Click Here to Join the VA Video Connect appointment ") + "<br/>";
                            emailPlainTextDescription += $"\\nJoin the appointment\\n{ProviderVirtualMeetingSpace}\\n";

                            var conf = getParamValue(ProviderVirtualMeetingSpace, "conference=");
                            var cid = getParamValue(ProviderVirtualMeetingSpace, "pin=");

                            if (!string.IsNullOrEmpty(conf) && !string.IsNullOrEmpty(cid))
                            {
                                emailBody += $"<br/><font size='2'>(To join this VVC appointment manually or through a video conferencing device: Alias: {conf} Host PIN: {cid})</font>";
                                emailpatTypeResourceReqBody += $"<br/><font size='2'>(To join this VVC appointment manually or through a video conferencing device: Alias: {conf} Host PIN: {cid})</font>";
                            }

                        }
                        else if (isCvtTablet)
                        {
                            subject = "Your VVC SIP Tablet Appointment has been scheduled for ";
                            emailBody += $"<br/><b>To join the appointment:</b><br/><font size='4' color='#0070c0' face='Tahoma'>Dial the VVC Tablet SIP Address: {sipAddress}</font>";
                            emailpatTypeResourceReqBody += $"<br/><b>To join the appointment:</b><br/><font size='4' color='#0070c0' face='Tahoma'>Dial the VVC Tablet SIP Address: {sipAddress}</font>";
                            emailPlainTextDescription += $"\\nTo join the appointment:\\nDial the VVC Tablet SIP Address: {sipAddress}";
                        }
                        else
                        {
                            emailBody += meetingSpace + "<br/>";
                            emailpatTypeResourceReqBody += meetingSpace + "<br/>";
                            emailPlainTextDescription += meetingSpace + "\\n";
                        }

                        emailBody += "<br />" + CvtHelper.VvcAppointmentInstructions(isCvtTablet, out var plainTextAppointmentInstructions);
                        emailpatTypeResourceReqBody += "<br />" + CvtHelper.VvcAppointmentInstructions(isCvtTablet, out var TextAppointmentInstructions);
                        emailPlainTextDescription += plainTextAppointmentInstructions;

                        emailBody += $"<br/><b><font color='#0070c0'>Emergency Use Only- e911 Instructions</font></b><br/>Call 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location (address) where the Patient is currently located.<br/>";
                        emailpatTypeResourceReqBody += $"<br/><b><font color='#0070c0'>Emergency Use Only- e911 Instructions</font></b><br/>Call 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location (address) where the Patient is currently located.<br/>";
                        emailPlainTextDescription += $"\\nEmergency Use Only- e911 Instructions\\nCall 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location         (address) where the Patient is currently located.\\n";
                        emailBody += CvtHelper.TechnicalAssistanceEmailFooter(out string techAssistanceEmailFooterPlainText);
                        emailpatTypeResourceReqBody += CvtHelper.TechnicalAssistanceEmailFooter(out string techAssistanceEmailFooterPlainText1);
                        emailPlainTextDescription += techAssistanceEmailFooterPlainText;
                        showHelper = 0;
                    }
                }
                else // this *IS* a "Phone Appointment"
                {
                    Logger.WriteDebugMessage("This is a Telephone Appointment.");
                    bool? patient = null;
                    var timeZone = CvtHelper.GetSiteTimeZoneCode(ServiceAppointment, OrganizationService, Logger);
                    string _startson = CvtHelper.GetTimeZoneString(timeZone, ServiceAppointment.ScheduledStart.Value, OrganizationService, Logger);
                    string _endson = CvtHelper.GetTimeZoneString(timeZone, ServiceAppointment.ScheduledEnd.Value, OrganizationService, Logger);
                    string _vcm = string.Empty;
                    Logger.WriteDebugMessage("ProvidersStringPlain: " + providersStringPlain);
                    providersStringPlain += ":  :  ";
                    string[] _proinfo = providersStringPlain.Split(':');
                    Logger.WriteDebugMessage("providerRoomsStringPlain: " + providerRoomsStringPlain);
                    string[] _proroom = providerRoomsStringPlain.Split('\\');
                    Logger.WriteDebugMessage("phone check 1");

                    //SystemUser _provider = null;
                    cvt_resourcepackage SchedulingPkg = null;
                    Service _service = null;
                    mcs_site _patSite = null;
                    mcs_site _proSite = null;
                    mcs_setting _activeSetting = null;
                    Logger.WriteDebugMessage("phone check 2");
                    //        var activeSettings = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings");
                    //        if (activeSettings != null)
                    //        {
                    //            //Get email field and configuration switch
                    //            recipient = activeSettings.cvt_URL; //"blank.email.com";
                    //            switchVar = (activeSettings.cvt_UseMVI != null) ? activeSettings.cvt_UseMVI.Value : false;
                    //        }
                    //        else
                    //        {
                    //            Logger.WriteDebugMessage("Couldn't find Active Settings, defaulting to create message.");
                    //        }


                    using (var srv = new Xrm(OrganizationService))
                    {
                        Logger.WriteDebugMessage("about to grab entities");
                        _activeSetting = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings");
                        if (_activeSetting != null)
                        {
                            if (_activeSetting.cvt_VirtualCareManager != null)
                            {
                                _vcm = _activeSetting.cvt_VirtualCareManager.ToString();
                            }
                        }
                        SchedulingPkg = srv.cvt_resourcepackageSet.FirstOrDefault(r => r.cvt_resourcepackageId == ((EntityReference)ServiceAppointment.cvt_relatedschedulingpackage).Id);
                        Logger.WriteDebugMessage("got SP");
                        _service = srv.ServiceSet.FirstOrDefault(s => s.ServiceId == ((EntityReference)SchedulingPkg.cvt_relatedservice).Id);
                        Logger.WriteDebugMessage("got service");
                        //_provider = srv.SystemUserSet.FirstOrDefault(u => u.SystemUserId == ((EntityReference)ServiceAppointment.cvt_relatedproviderid).Id);
                        //Logger.WriteDebugMessage("got provider");
                        if (ServiceAppointment.mcs_relatedsite != null)
                        {
                            _patSite = srv.mcs_siteSet.FirstOrDefault(f => f.mcs_siteId == ((EntityReference)ServiceAppointment.mcs_relatedsite).Id);
                            Logger.WriteDebugMessage("got pat site");
                        }

                        if (ServiceAppointment.mcs_relatedprovidersite != null)
                        {
                            _proSite = srv.mcs_siteSet.FirstOrDefault(f => f.mcs_siteId == ((EntityReference)ServiceAppointment.mcs_relatedprovidersite).Id);
                            Logger.WriteDebugMessage("got pro site");
                        }
                    }
                    Logger.WriteDebugMessage("retrieved entities");

                    var _specialty = SchedulingPkg.cvt_specialty.Name.ToString();
                    var _proFac = SchedulingPkg.cvt_providerfacility.Name.ToString();

                    Logger.WriteDebugMessage("begin message construction now");
                    Logger.WriteDebugMessage("begin subject construction");
                    if (ServiceAppointment.StateCode.Value != ServiceAppointmentState.Canceled)
                    {
                        if (ServiceAppointment.StateCode.Value != ServiceAppointmentState.Canceled)
                        {
                            if (string.IsNullOrEmpty(subject))
                                subject = "Scheduled Telephone Appointment Notification for " + _specialty;
                        }
                        else //we're canceling
                        {
                            subject = "Your Telephone Appointment has been cancelled for  " + _startson;
                        }
                    }
                    Logger.WriteDebugMessage("subject construction complete");
                    emailBody = string.Empty;
                    emailPlainTextDescription = string.Empty;

                    if (ServiceAppointment.StateCode.Value == ServiceAppointmentState.Canceled)
                    {
                        emailBody += "This is an automated message to notify you that a <b><font color='#0070c0'>Telephone</font></b> appointment has been <font color='red'><u>Cancelled</u></font> for " + _startson + "<br/>";
                        emailPlainTextDescription += "This is an automated message to notify you that a  Telephone  appointment has been  CANCELLED  for " + _startson + "\n";
                    }
                    else
                    {
                        emailBody += "This is an automated message to notify you that a <b><font color='#0070c0'>Telephone</font></b> appointment has been <font color='green'><u>Scheduled</u></font> for " + _startson + "<br/>";
                        emailPlainTextDescription += "This is an automated message to notify you that a  Telephone  appointment has been  SCHEDULED  for " + _startson + "\n";
                    }

                    emailBody += "Please open the attachment and click \"Save and Close\" to add this event to your calendar.  The details are listed below:<br/><br/>";

                    emailPlainTextDescription += "Please open the attachment and click \"Save and Close\" to add this event to your calendar.  The details are listed below:\n\n";

                    emailBody += "<b><font color='#0070c0' size='4'>Appointment Information:</font></b><br/>";
                    patientTelephoneAndVCCBody = "<b><font color='#0070c0' size='4'>Appointment Information:</font></b><br/>";
                    emailPlainTextDescription += "Appointment Information:\n";

                    emailBody += "Start Time: " + _startson + "<br/>";
                    patientTelephoneAndVCCBody += "Start Time: " + _startson + "<br/>";
                    emailPlainTextDescription += "Start Time: " + _startson + "\n";

                    emailBody += "End Time: " + _endson + "<br/><br/>";
                    patientTelephoneAndVCCBody += "End Time: " + _endson + "<br/><br/>";
                    emailPlainTextDescription += "Start Time: " + _endson + "\n\n";

                    emailBody += "<b><font color='#0070c0' size='4'>Provider Site Information: </font></b><br/>";
                    patientTelephoneAndVCCBody += "<b><font color='#0070c0' size='4'>Provider Site Information: </font></b><br/>";
                    emailPlainTextDescription += "Provider Site Information:\n";
                    if (_proroom == null) { Logger.WriteDebugMessage("ProRoom Array is null"); }
                    Logger.WriteDebugMessage("ProRoom Array Elements: " + _proroom.Length);
                    emailBody += "Room: " + _proroom[0] != null ? _proroom[0] : null;
                    patientTelephoneAndVCCBody += "Room: " + _proroom[0] != null ? _proroom[0] : null;
                    emailPlainTextDescription += "Room: " + _proroom[0] != null ? _proroom[0] : null;

                    // emailBody += "<br/>Vista Clinic: " + SchedulingPkg.cvt_providersitevistaclinics.ToString() + "<br/><br/>";
                    //emailPlainTextDescription += "\nVista Clinic: " + SchedulingPkg.cvt_providersitevistaclinics.ToString() + "\n\n";

                    if (_proinfo == null) { Logger.WriteDebugMessage("ProInfo Array is null"); }
                    Logger.WriteDebugMessage("ProInfo Array Elements: " + _proinfo.Length);
                    emailBody += "Provider: " + _proinfo[1] + "<br/>";
                    patientTelephoneAndVCCBody += "Provider: " + _proinfo[1] + "<br/>";
                    emailPlainTextDescription += "Provider: " + _proinfo[1] + "\n";

                    emailBody += "Phone: " + _proinfo[2] + "<br/><br/>";
                    patientTelephoneAndVCCBody += "Phone: " + _proinfo[2] + "<br/><br/>";
                    emailPlainTextDescription += "Phone: " + _proinfo[2] + "\n\n";

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 1");
                    // emailBody += "Provider Email:" + _provider.InternalEMailAddress + "<br/><br/>";
                    emailBody += "<b><font color='#0070c0' size='4'>Patient Information: </font></b><br/>";
                    patientTelephoneAndVCCBody += "<b><font color='#0070c0' size='4'>Patient Information: </font></b><br/>";
                    emailPlainTextDescription += "Patient Information: \n";

                    emailBody += "Patient Facility: ";
                    patientTelephoneAndVCCBody += "Patient Facility: ";
                    emailPlainTextDescription += "Patient Facility: ";// + _patSite!=null?_patSite.mcs_FacilityId.Name.ToString():"--" + "<br/>";
                    if (_patSite != null)
                    {
                        Logger.WriteDebugMessage("Continuing Email Body Construction -- 1a");
                        emailBody += _patSite.mcs_FacilityId.Name + "<br/>";
                        patientTelephoneAndVCCBody += _patSite.mcs_FacilityId.Name + "<br/>";
                        emailPlainTextDescription += _patSite.mcs_FacilityId.Name + "\n";
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Continuing Email Body Construction -- 1b");
                        emailBody += "--- <br/>";
                        patientTelephoneAndVCCBody += "--- <br/>";
                        emailPlainTextDescription += "---\n";
                    }
                    emailBody += "Patient Site: ";
                    patientTelephoneAndVCCBody += "Patient Site: ";
                    emailPlainTextDescription += "Patient Site: ";
                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 2");
                    if (_patSite != null)
                    {
                        Logger.WriteDebugMessage("Continuing Email Body Construction -- 2a");
                        emailBody += _patSite.mcs_name.ToString() + "<br/>";
                        patientTelephoneAndVCCBody += _patSite.mcs_name.ToString() + "<br/>";
                        emailPlainTextDescription += _patSite.mcs_name.ToString() + "\n";
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Continuing Email Body Construction -- 2b");
                        emailBody += "--- <br/>";
                        patientTelephoneAndVCCBody += "--- <br/>";
                        emailPlainTextDescription += "---\n";
                    }

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 3");
                    emailBody += "Patient: " + patientInitials.TrimEnd(';') + "<br/>";
                    patientTelephoneAndVCCBody += "Patient: " + patientInitials.TrimEnd(';') + "<br/>";
                    emailPlainTextDescription += "Patient: " + patientInitials.TrimEnd(';') + "\n";

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 4");
                    emailBody += "Vista Clinic" + string.Empty + "<br/>";
                    patientTelephoneAndVCCBody += "Vista Clinic" + string.Empty + "<br/>";
                    emailPlainTextDescription += "Vista Clinic" + string.Empty + "\n";

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 5");
                    emailBody += "Patient Verified Phone Number:" + ServiceAppointment.cvt_PatientVerifiedPhone.ToString() + "<br/>";
                    patientTelephoneAndVCCBody += "Patient Verified Phone Number:" + ServiceAppointment.cvt_PatientVerifiedPhone.ToString() + "<br/>";

                    emailPlainTextDescription += "Patient Verified Phone Number:" + ServiceAppointment.cvt_PatientVerifiedPhone.ToString() + "\n";

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 6");
                    if ((ServiceAppointment.cvt_PatientVerifiedEmail != null) && (ServiceAppointment.cvt_PatientVerifiedEmail != ""))
                    {
                        Logger.WriteDebugMessage("Continuing Email Body Construction -- 6a");
                        emailBody += "Patient Verified Email: <a href='mailto:" + ServiceAppointment.cvt_PatientVerifiedEmail.ToString() + "'>" + ServiceAppointment.cvt_PatientVerifiedEmail.ToString() + "<br/><br/>";
                        patientTelephoneAndVCCBody += "Patient Verified Email: <a href='mailto:" + ServiceAppointment.cvt_PatientVerifiedEmail.ToString() + "'>" + ServiceAppointment.cvt_PatientVerifiedEmail.ToString() + "<br/><br/>";
                        emailPlainTextDescription += "Patient Verified Email: " + ServiceAppointment.cvt_PatientVerifiedEmail.ToString() + "\n\n";
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Continuing Email Body Construction -- 6b");
                        emailBody += "Patient Verified Email:---<br/><br/>";
                        patientTelephoneAndVCCBody += "Patient Verified Email:---<br/><br/>";
                        emailPlainTextDescription += "Patient Verified Email:---\n,";
                    }

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 7");
                    emailBody += "<a href='" + _vcm + "'><b><font color='#0070c0' size='4'>Virtual Care Manager (VCM)</font></b></a><br/><br/>";
                    patientTelephoneAndVCCBody += "<a href='" + _vcm + "'><b><font color='#0070c0' size='4'>Virtual Care Manager (VCM)</font></b></a><br/><br/>";
                    emailPlainTextDescription += "Link to Virtual Care Manager(VCM): " + _vcm + "\n\n";

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 8");
                    emailBody += "<b><font color='#0070c0' size='4'>Phone Center Instructions:</font></b><br/>";
                    patientTelephoneAndVCCBody += "<b><font color='#0070c0' size='4'>Phone Center Instructions:</font></b><br/>";
                    emailPlainTextDescription += "Phone Center Instructions\n";

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 9");
                    if ((SchedulingPkg.cvt_TelephoneInstructions != null) && (SchedulingPkg.cvt_TelephoneInstructions != ""))
                    {
                        emailBody += SchedulingPkg.cvt_TelephoneInstructions.ToString() + "<br/><br/>";
                        patientTelephoneAndVCCBody += SchedulingPkg.cvt_TelephoneInstructions.ToString() + "<br/><br/>";
                        emailPlainTextDescription += SchedulingPkg.cvt_TelephoneInstructions.ToString() + "\n\n";
                    }
                    else //no telephone instructions specified on the SP
                    {
                        emailBody += "----<br/><br/>";
                        patientTelephoneAndVCCBody += "----<br/><br/>";
                        emailPlainTextDescription += "----\n\n";
                    }

                    emailBody += "<br/><b><font color='#0070c0'>Veterans in home Emergency Use Only- e911 Instructions</font></b><br/>Call 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location (address) where the Patient is currently located.<br/><br>";
                    patientTelephoneAndVCCBody += "<br/><b><font color='#0070c0'>Veterans in home Emergency Use Only- e911 Instructions</font></b><br/>Call 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location (address) where the Patient is currently located.<br/><br>";
                    emailPlainTextDescription += "Veterans in home Emergency Use Only- e911 Instructions:\nCall 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location       (address) where the Patient is currently located.\n\n";

                    Logger.WriteDebugMessage("Continuing Email Body Construction -- 10");
                    emailBody += CvtHelper.TechnicalAssistanceEmailFooter(out string techAssistanceEmailFooterPlainText) + "<br/></br>";
                    patientTelephoneAndVCCBody += "<br/><b>Technical Assistance</b><br/> For technical assistance, clinicians should contact the Office of Connected Care Help Desk (OCCHD) (866) 651 - 3180 or (703) 234 - 4483, available 24/7/365.<br/>" +
                                    "-----------------------------------------------------------------------------------------------------------------------------------------<br/>" +
                                    "Please do not reply to this message.It comes from an unmonitored mailbox." + "<br/></br>"; ;
                    emailPlainTextDescription += techAssistanceEmailFooterPlainText + "\n\n";
                    Logger.WriteDebugMessage("VVC Telephone Message body complete.");
                    Logger.WriteDebugMessage(emailBody);
                    Logger.WriteDebugMessage(emailPlainTextDescription);
                }
            }

            if (ServiceAppointment.cvt_Type == true)
            {
                if (!_phoneappt)
                {
                    if (showHelper == 1)
                    {
                        if (ServiceAppointment.StateCode.Value != ServiceAppointmentState.Canceled)
                        {
                            //emailBody += $"<br/><b>Emergency Use Only- e911 Instructions</b><br/>Call 267-908-6605 to speak with an agent who can put you in touch with a 911 operator at the Patient's location. You must have the physical location(address) where the Patient is currently located.<br/>";
                            emailBody += $"{(isCvtTablet ? "" : "If you plan to use an iPhone or iPad for your appointment, download the free VA Video Connect (VVC) app from the Apple App store. <br/>")}";
                            emailBodyPatSide += $"{(isCvtTablet ? "" : "If you plan to use an iPhone or iPad for your appointment, download the free VA Video Connect (VVC) app from the Apple App store. <br/>")}";
                            emailBody += $"{CvtHelper.buildHTMLUrlAlt("https://itunes.apple.com/us/app/va-video-connect/id1224250949?mt=8", "Click Here to download the VVC iOS app.")}<br/><br/>";
                            emailBodyPatSide += $"{CvtHelper.buildHTMLUrlAlt("https://itunes.apple.com/us/app/va-video-connect/id1224250949?mt=8", "Click Here to download the VVC iOS app.")}<br/><br/>";

                            emailPlainTextDescription += $"{(isCvtTablet ? "" : "If you plan to use an iPhone or iPad for your appointment, download the free VA Video Connect (VVC) app from the Apple App store. \\n")}";
                            emailPlainTextDescriptionPatSide += $"{(isCvtTablet ? "" : "If you plan to use an iPhone or iPad for your appointment, download the free VA Video Connect (VVC) app from the Apple App store. \\n")}";
                            emailPlainTextDescription += "Click Here to download the VVC iOS app: https://itunes.apple.com/us/app/va-video-connect/id1224250949?mt=8 \\n\\n";
                            emailPlainTextDescriptionPatSide += "Click Here to download the VVC iOS app: https://itunes.apple.com/us/app/va-video-connect/id1224250949?mt=8 \\n\\n";

                            emailBody += CvtHelper.GenerateNeedHelpSection(false, out var plainTextHelpSection);
                            emailBodyPatSide += CvtHelper.GenerateNeedHelpSection(false, out var extra2);
                            emailPlainTextDescription += plainTextHelpSection;
                            emailPlainTextDescriptionPatSide += plainTextHelpSection;
                        }
                        else
                        {
                            subject = (isCvtTablet) ? "Your VVC SIP Tablet Appointment has been cancelled for " : "Your VA Video Connect (VVC) Appointment has been cancelled for ";
                            emailBody += CvtHelper.TechnicalAssistanceEmailFooter(out string techAssistanceEmailFooterPlainText);
                            emailpatTypeResourceReqBody += CvtHelper.TechnicalAssistanceEmailFooter(out string techAssistanceEmailFooterPlainText1);
                            emailBodyPatSide += CvtHelper.TechnicalAssistanceEmailFooter(out string extra3);
                            emailPlainTextDescription += techAssistanceEmailFooterPlainText;
                            emailPlainTextDescriptionPatSide += techAssistanceEmailFooterPlainText;
                        }
                    }
                }

            }

            Logger.WriteDebugMessage("Finishing Formatting Email Body");
            return emailBody;
        }

        public string returnGroupPSSites(List<cvt_schedulingresource> patientSRs, List<mcs_resource> patientVCs, List<mcs_resource> patientRooms, List<mcs_resource> patientTechs)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                string output = string.Empty;
                List<Guid> possiblePatPS = new List<Guid>();

                foreach (cvt_schedulingresource record in patientSRs)
                {
                    if (record.cvt_participatingsite != null)
                        possiblePatPS.Add(record.cvt_participatingsite.Id);
                }

                possiblePatPS = possiblePatPS.Distinct().ToList();
                //Loop through all of the possible Patient PS.
                foreach (Guid record in possiblePatPS)
                {
                    var relevantSRs = patientSRs.Where(p => p.Attributes.Contains("cvt_participatingsite") && p.cvt_participatingsite.Id == record).ToList();
                    if (relevantSRs != null)
                    {
                        //Add this information
                        var patPS = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == record);
                        //$"Patient Site: {patPS.cvt_site.Name} Device Type: () Room Number ()";
                        var result = MatchGroupPatResources(relevantSRs, patientVCs, patientRooms, patientTechs);
                        if (result != string.Empty)
                            output += $"{patPS.cvt_site.Name} {result}; ";
                    }
                }
                return output;
            }
        }
        #endregion

        #region PatientEmail
        internal void SendPatientEmail(List<SystemUser> provs, bool isCancelled, Guid? patientId = null)
        {
            bool _phoneappt = ServiceAppointment.cvt_TelephoneCall == true ? true : false;

            if (!_phoneappt)
            {
                //Modify caller to populate Virtual Meeting Space variables
                if ((VirtualMeetingSpaceComponent != null && VirtualMeetingSpaceComponent.Id != Guid.Empty || PatientVirtualMeetingSpace?.IndexOf("Please Contact Your") == -1) || ((_phoneappt) && (ServiceAppointment.StateCode == ServiceAppointmentState.Canceled)))
                // "Please Contact Your" indexOf check is only relevant if we do not want to generate email at all given the scenario where no virtual meeting space is provided
                {
                    //Get the Patient and their timezone
                    List<ActivityParty> patientAPs = new List<ActivityParty>();
                    if (patientId != null)
                        patientAPs.Add(new ActivityParty { PartyId = new EntityReference(Contact.EntityLogicalName, patientId.Value) });
                    else
                        patientAPs = ServiceAppointment.Customers.ToList();
                    if (patientAPs == null || patientAPs.ToList().Count == 0)
                        Logger.WriteToFile("No Patient was found to receive the email for following Service Activity: " + ServiceAppointment.Id);
                    else
                    {
                        //Getting the Subject Specialty, Specialty Sub Type
                        var serviceText = (Tsa.cvt_specialty != null) ? "<b>Specialty:</b> " + Tsa.cvt_specialty.Name + "<br />" : "";
                        serviceText += (Tsa.cvt_specialtysubtype != null) ? "<b>Specialty Sub Type:</b> " + Tsa.cvt_specialtysubtype.Name + "<br />" : "";

                        var clinicians = string.Empty;
                        Logger.WriteDebugMessage($"Total Providers passed into SendPatientEmail: {provs.Count().ToString()}");
                        foreach (SystemUser user in provs)
                        {
                            if (!string.IsNullOrEmpty(clinicians))
                                clinicians += "; ";
                            clinicians += $"{user.FirstName?.FirstOrDefault()}{user.LastName?.FirstOrDefault()}";
                        }
                        if (provs.Count == 1)
                            clinicians = "<b>Clinician:</b> " + clinicians + "<br />";
                        else if (provs.Count > 1)
                            clinicians = "<b>Clinicians:</b> " + clinicians + "<br />";

                        //if you can't find "Please Contact Your" that means a real url was entered, so use it as a hyperlink, otherwise, display the "Please Contact Your..." message as it comes across
                        var meetingSpace = PatientVirtualMeetingSpace.IndexOf("Please Contact Your") == -1 ?
                            CvtHelper.buildHTMLUrl(PatientVirtualMeetingSpace, "Click Here to Join the VA Video Connect appointment")
                            : "<b>Your VA Video Connect appointment was not found, " + PatientVirtualMeetingSpace + "</b>";

                        var dynamicBody = isCvtTablet ? "Your provider will call your CVT tablet for the appointment." : "Please click the following link to access the virtual medical room.  This will take you into the virtual waiting room until your provider joins.<br />";

                        //Set up difference in Scheduled vs Cancelation text
                        var descrStatus = "reminder about your";
                        //var attachmentText = "<br /><br />A calendar appointment is attached to this email; you can open the attachment and save it to your calendar.";
                        var desktopLink = "https://vaots.blackboard.com/bbcswebdav/institution/CVT/tmp/email-link/vvc-drod-vet.docx";
                        var iosLink = "https://vaots.blackboard.com/bbcswebdav/institution/CVT/tmp/email-link/vvc-ios-vet.docx";
                        var trainingLink = "<br /><br />For information on how to use VMRs from Desktop and Android tablet devices, please <a href=" + desktopLink + ">Click Here</a>" +
                            "<br />For information on how to use VMRs from iOS/Apple devices (e.g. iPad, iPhone, etc.), please <a href=" + iosLink + ">Click Here</a>";

                        foreach (var patientAP in patientAPs)
                        {
                            if (CvtHelper.ShouldGenerateVeteranEmail(patientAP.PartyId.Id, OrganizationService, Logger))
                            {
                                var recipient = new ActivityParty()
                                {
                                    PartyId = new EntityReference(Contact.EntityLogicalName, patientAP.PartyId.Id)
                                };
                                Logger.WriteDebugMessage("Sending Patient Email to " + patientAP.PartyId.Name);
                                var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, recipient.PartyId.Id, new ColumnSet(true));

                                //Setup variables to get timeZone conversion properly
                                DateTime timeConversion = ServiceAppointment.ScheduledStart.Value;
                                //TODO - This is no longer the source of sending patient a cancelation - if patient was removed, it was canceled for that particular patient

                                var fullDate = CvtHelper.GetTimeZoneString(patient.cvt_TimeZone, timeConversion, OrganizationService, Logger);
                                var subject = String.Empty;
                                //Creating the Subject text

                                subject = "Your VA Video Connect (VVC) Appointment has been ";

                                subject += (isCancelled) ? "cancelled" : "scheduled";
                                subject += " for " + fullDate.Trim();
                                Logger.WriteDebugMessage("Local Time: " + fullDate);
                                if (isCancelled) //Canceled
                                {
                                    descrStatus = "cancellation notice for your previously scheduled";
                                    //attachmentText = "<br /><br />A calendar appointment cancelation is attached to this email, you can open the attachment and click \"Remove from Calendar\" to remove this event from your calendar.";
                                    dynamicBody = "";
                                    meetingSpace = "";
                                    trainingLink = "";
                                }

                                var description = $"This is a {descrStatus} VA Video Connect appointment.<br/> <b>Appointment Information:</b><br/>Date/Time: {fullDate}<br />"
                                                + $"{clinicians}";
                                if (!isCancelled)
                                    description += $"<br /><br /><b>Join the appointment:</b><br />"
                                                + $"{(!isCvtTablet ? meetingSpace : "")}<br /><br />{AppointmentInstructions()}<br />If you plan to now use an iPhone or iPad for your appointment, download the free VA Video Connect (VVC) app from the Apple App store. "
                                                + $"{CvtHelper.buildHTMLUrl("https://itunes.apple.com/us/app/va-video-connect/id1224250949?mt=8", "Click Here to download the VVC iOS app.")}";
                                //+ $"{attachmentText}{trainingLink}"
                                description += $"<br /><br />{CvtHelper.GenerateNeedHelpSection(isCancelled, out var plainText)}";

                                List<ActivityParty> sender = new List<ActivityParty>();
                                if (Email.From == null || Email.From.ToList().Count == 0)
                                    sender = CvtHelper.GetWorkflowOwner("Service Activity Notification", OrganizationService);
                                else
                                {
                                    foreach (var item in Email.From)
                                    {
                                        sender.Add(new ActivityParty { PartyId = new EntityReference(SystemUser.EntityLogicalName, item.PartyId.Id) });
                                    }
                                }
                                Email patientEmail = new Email()
                                {
                                    Subject = subject,
                                    Description = description,
                                    mcs_RelatedServiceActivity = new EntityReference(ServiceAppointment.EntityLogicalName, ServiceAppointment.Id),
                                    RegardingObjectId = new EntityReference(Contact.EntityLogicalName, patientAP.PartyId.Id),
                                    From = sender,
                                    To = CvtHelper.SetPartyList(recipient)
                                };

                                OrganizationService.Create(patientEmail);
                                Logger.WriteDebugMessage("Patient Email Created Successfully");
                            }
                        }
                    }
                }
            }
            else if ((_phoneappt) && (isCancelled))
            {
                Logger.WriteDebugMessage("Entered 'Cancel Phone' if clause.....");
                List<ActivityParty> patientAPs = new List<ActivityParty>();
                if (patientId != null)
                    patientAPs.Add(new ActivityParty { PartyId = new EntityReference(Contact.EntityLogicalName, patientId.Value) });
                else
                    patientAPs = ServiceAppointment.Customers.ToList();
                if (patientAPs == null || patientAPs.ToList().Count == 0)
                    Logger.WriteToFile("No Patient was found to receive the email for following Service Activity: " + ServiceAppointment.Id);
                else
                {
                    //Getting the Subject Specialty, Specialty Sub Type
                    var serviceText = (Tsa.cvt_specialty != null) ? "<b>Specialty:</b> " + Tsa.cvt_specialty.Name + "<br />" : "";
                    serviceText += (Tsa.cvt_specialtysubtype != null) ? "<b>Specialty Sub Type:</b> " + Tsa.cvt_specialtysubtype.Name + "<br />" : "";

                    var clinicians = string.Empty;
                    Logger.WriteDebugMessage($"Total Providers passed into SendPatientCancelEmail: {provs.Count().ToString()}");
                    foreach (SystemUser user in provs)
                    {
                        if (!string.IsNullOrEmpty(clinicians))
                            clinicians += "; ";
                        clinicians += $"{user.FirstName?.FirstOrDefault()}{user.LastName?.FirstOrDefault()}";
                    }
                    if (provs.Count == 1)
                        clinicians = "<b>Clinician:</b> " + clinicians + "<br />";
                    else if (provs.Count > 1)
                        clinicians = "<b>Clinicians:</b> " + clinicians + "<br />";

                    //if you can't find "Please Contact Your" that means a real url was entered, so use it as a hyperlink, otherwise, display the "Please Contact Your..." message as it comes across
                    //Set up difference in Scheduled vs Cancelation text
                    var descrStatus = "reminder about your";
                    //var attachmentText = "<br /><br />A calendar appointment is attached to this email; you can open the attachment and save it to your calendar.";

                    foreach (var patientAP in patientAPs)
                    {
                        if (CvtHelper.ShouldGenerateVeteranEmail(patientAP.PartyId.Id, OrganizationService, Logger))
                        {
                            var recipient = new ActivityParty()
                            {
                                PartyId = new EntityReference(Contact.EntityLogicalName, patientAP.PartyId.Id)
                            };
                            Logger.WriteDebugMessage("Sending Patient Email to " + patientAP.PartyId.Name);
                            var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, recipient.PartyId.Id, new ColumnSet(true));

                            //Setup variables to get timeZone conversion properly
                            DateTime timeConversion = ServiceAppointment.ScheduledStart.Value;
                            //TODO - This is no longer the source of sending patient a cancelation - if patient was removed, it was canceled for that particular patient

                            var fullDate = CvtHelper.GetTimeZoneString(patient.cvt_TimeZone, timeConversion, OrganizationService, Logger);
                            var subject = String.Empty;
                            //Creating the Subject text

                            subject = "Your Telephone Appointment has been ";

                            subject += (isCancelled) ? "cancelled" : "scheduled";
                            subject += " for " + fullDate.Trim();
                            Logger.WriteDebugMessage("Local Time: " + fullDate);
                            if (isCancelled) //Canceled
                            {
                                descrStatus = "cancellation notice for your previously scheduled";
                                //attachmentText = "<br /><br />A calendar appointment cancelation is attached to this email, you can open the attachment and click \"Remove from Calendar\" to remove this event from your calendar.";
                            }

                            var description = $"This is a {descrStatus} Telephone appointment.<br /><br /> <b>Appointment Information:</b><br />Date/Time: {fullDate}<br />"
                                            + $"{clinicians}";
                            //if (!isCancelled)
                            //    description += $"<br /><br /><b>Join the appointment:</b><br />"
                            //                + $"{(!isCvtTablet ? meetingSpace : "")}<br /><br />{AppointmentInstructions()}<br />If you plan to now use an iPhone or iPad for your appointment, download the free VA Video Connect (VVC) app from the Apple App store. "
                            //                + $"{CvtHelper.buildHTMLUrl("https://itunes.apple.com/us/app/va-video-connect/id1224250949?mt=8", "Click Here to download the VVC iOS app.")}";
                            //+ $"{attachmentText}{trainingLink}"
                            //description += $"<br /><br />{CvtHelper.GenerateNeedHelpSection(isCancelled, out var plainText)}";

                            List<ActivityParty> sender = new List<ActivityParty>();
                            if (Email.From == null || Email.From.ToList().Count == 0)
                                sender = CvtHelper.GetWorkflowOwner("Service Activity Notification", OrganizationService);
                            else
                            {
                                foreach (var item in Email.From)
                                {
                                    sender.Add(new ActivityParty { PartyId = new EntityReference(SystemUser.EntityLogicalName, item.PartyId.Id) });
                                }
                            }
                            Email patientEmail = new Email()
                            {
                                Subject = subject,
                                Description = description,
                                mcs_RelatedServiceActivity = new EntityReference(ServiceAppointment.EntityLogicalName, ServiceAppointment.Id),
                                RegardingObjectId = new EntityReference(Contact.EntityLogicalName, patientAP.PartyId.Id),
                                From = sender,
                                To = CvtHelper.SetPartyList(recipient)
                            };

                            OrganizationService.Create(patientEmail);
                            Logger.WriteDebugMessage("Patient Email Created Successfully");
                        }
                    }
                }
            }
            else
            {
                Logger.WriteDebugMessage($"No VMR information could be found, so did not generate an email to the Patient. VirtualMeetingSpaceComponent != null: {VirtualMeetingSpaceComponent != null}; PatientVirtualMeetingSpace: {PatientVirtualMeetingSpace}");
            }
        }

        internal string AppointmentInstructions()
        {
            var safetyChecks = "<b>VA Video Connect Appointment Instructions:</b><br />Ensure you are in a private and safe place with good internet connectivity, and have the following information available:";
            safetyChecks += "<ul><li><b>Phone number:</b> How we can reach you by telephone, if the video call drops</li>";
            safetyChecks += "<li><b>Address:</b> Your location during the visit.</li>";
            safetyChecks += "<li><b>Emergency Contact:</b> Name, phone number, and relationship of a person who we can contact in an emergency.</li>";
            safetyChecks += "<li>Please dress as you would to see your provider in-person.</li></ul>";

            return safetyChecks;
        }
        #endregion

        #region H/M Groups
        public void NotifyProviderOfPatientChange()
        {
            Logger.WriteDebugMessage("Beginning NotifyProviderOfPatientChange Method");
            var action = string.Empty;
            if (Email.Subject.Contains("added") && Email.Subject.Contains("removed"))
                action = "Addition and Cancellation";
            else if (Email.Subject.Contains("added"))
                action = "Addition";
            else if (Email.Subject.Contains("removed"))
                action = "Cancellation";
            NotifyParticipantsOfAppointment(false, action);
            CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
            Logger.WriteDebugMessage($"{action} email sent to providers/telepresenters");
        }

        /* public void NotifyProviderOfPatientChange()
         {
             var action = string.Empty;
             if (Email.Subject.Contains("added") && Email.Subject.Contains("removed"))
             {
                 action = "Addition and Cancelation";
             }
             else if (Email.Subject.Contains("added"))
             {
                 action = "Addition";
             }
             else if (Email.Subject.Contains("removed"))
             {
                 action = "Cancelation";
             }
             NotifyParticipantsOfAppointment(false, action);



             CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
             Logger.WriteDebugMessage($"{action} email sent to providers/telepresenters");
         }*/

        public void NotifyPatientOfAdditionOrRemovalFromGroup(bool isCancelled)
        {
            //TODO - work on bug with adding patients to existing appt - Null Reference error occuring here
            Logger.WriteDebugMessage("Start");
            var bookedUsers = ServiceAppointment.Resources.Where(ap => ap.PartyId.LogicalName == SystemUser.EntityLogicalName).ToList<Entity>();
            var ec = new EntityCollection(bookedUsers);
            var tsaProResources = getPRGs("provider", OrganizationService);

            var clinicians = GetRecipients(ec, tsaProResources);
            bool? isPatSpace = null;
            getPatientVirtualMeetingSpace(out isPatSpace);
            SendPatientEmail(clinicians, isCancelled); //Group?
        }
        #endregion

        #region SA Notification Helpers
        internal List<cvt_schedulingresource> getPRGs(string location, IOrganizationService OrgService)
        {
            List<cvt_schedulingresource> schedulingResources = new List<cvt_schedulingresource>();
            using (var srv = new Xrm(OrganizationService))
            {
                int locationType = 0;

                if (Tsa != null && Tsa.Id != Guid.Empty)
                {
                    if (location == "provider")
                    {
                        locationType = (int)cvt_participatingsitecvt_locationtype.Provider;
                    }
                    else if (location == "patient")
                    {
                        locationType = (int)cvt_participatingsitecvt_locationtype.Patient;
                    }
                    var participatingSites = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_resourcepackage.Id == Tsa.Id && ps.statecode.Value == (int)cvt_participatingsiteState.Active && ps.cvt_locationtype.Value == locationType && ps.cvt_scheduleable.Value == true);



                    foreach (cvt_participatingsite record in participatingSites)
                    {
                        //Get all SR and add them into the same list
                        var schResources = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_participatingsite.Id == record.Id && sr.statecode.Value == (int)cvt_schedulingresourceState.Active).ToList();
                        if (schResources != null)
                        {
                            schedulingResources.AddRange(schResources);
                        }
                    }
                }
                return schedulingResources;
            }
        }

        /// <summary>
        /// looks to find a group resource record for the group and user listed
        /// </summary>
        /// <param name="user">user Id of the group resource</param>
        /// <param name="group">group id of the group resource</param>
        /// <returns>true if a record exists for the group passed in and the user passed it</returns>
        internal bool MatchResourceToGroup(Guid userId, Guid groupId)
        {
            using (var srv = new Xrm(OrganizationService))
                return srv.mcs_groupresourceSet.FirstOrDefault(gr => gr.mcs_relatedResourceGroupId.Id == groupId && gr.mcs_RelatedUserId.Id == userId) != null;
        }

        /// <summary>
        /// looks to find a group resource record for the group and resource listed
        /// </summary>
        /// <param name="resource">resource record of the group resource</param>
        /// <param name="group">resource record of the group resource</param>
        /// <returns>true if a record exists for the group passed in and the resource passed it</returns>

        internal bool MatchResourceToGroup(mcs_resource resource, mcs_resourcegroup group)
        {
            using (var srv = new Xrm(OrganizationService))
                return srv.mcs_groupresourceSet.FirstOrDefault(gr => gr.mcs_relatedResourceGroupId.Id == group.Id && gr.mcs_RelatedResourceId.Id == resource.Id) != null;
        }

        internal List<SystemUser> GetRecipients(List<ActivityParty> users, List<cvt_schedulingresource> prgs)
        {
            var userEc = new EntityCollection();
            foreach (var user in users)
            {
                userEc.Entities.Add(user);
            }
            return GetRecipients(userEc, prgs);
        }

        /// <summary>
        /// returns the list of users who are being booked as either single resources or in part of a group
        /// </summary>
        /// <param name="users"></param>
        /// <param name="prgs"></param>
        /// <returns></returns>
        internal List<SystemUser> GetRecipients(EntityCollection users, List<cvt_schedulingresource> schRes)
        {
            List<SystemUser> recipients = new List<SystemUser>();
            var singles = schRes.Where(p => p.Attributes.Contains("cvt_schedulingresourcetype") && p.cvt_schedulingresourcetype.Value == (int)cvt_tsaresourcetype.SingleProvider).ToList();


            var groups = schRes.Where(p => p.Attributes.Contains("cvt_schedulingresourcetype") && p.cvt_schedulingresourcetype.Value == (int)cvt_tsaresourcetype.ResourceGroup).ToList();
            Logger.WriteDebugMessage("Sorting through single User results.");
            foreach (var singleSR in singles)
            {
                if (singleSR.Attributes.Contains("cvt_user") && singleSR.cvt_user != null)
                {
                    SystemUser singleUser = (SystemUser)OrganizationService.Retrieve(SystemUser.EntityLogicalName, singleSR.cvt_user.Id, new ColumnSet(true));

                    if (singleUser != null && singleUser.Id != Guid.Empty)
                    {
                        foreach (ActivityParty u in users.Entities)
                        {
                            Logger.WriteDebugMessage($"userFromSP: {singleUser.Id} userfromSA: {u.PartyId.Id}");
                            if (singleUser.Id == u.PartyId.Id)
                            {
                                Logger.WriteDebugMessage("User matched and added.");
                                recipients.Add(singleUser);
                                break;
                            }
                            else
                                Logger.WriteDebugMessage("User did not match and was not added.");
                        }
                    }
                }
            }
            Logger.WriteDebugMessage("Sorting through Group results.");
            foreach (var groupSR in groups)
            {
                if (groupSR.Attributes.Contains("cvt_tmpresourcegroup") && groupSR.cvt_tmpresourcegroup != null)
                {
                    mcs_resourcegroup group = (mcs_resourcegroup)OrganizationService.Retrieve(
                            mcs_resourcegroup.EntityLogicalName, groupSR.cvt_tmpresourcegroup.Id, new ColumnSet(true));
                    //if group type value is any of the "user-type" groups (provider or paired or telepresenter)
                    if ((group.mcs_Type.Value == (int)mcs_resourcetype.Provider) || (group.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup) ||
                        (group.mcs_Type.Value == (int)mcs_resourcetype.TelepresenterImager))
                    {
                        //if the user selected is in the resource group, return true and add the user to the entitycollection
                        foreach (ActivityParty u in users.Entities)
                        {
                            var user = (SystemUser)OrganizationService.Retrieve(SystemUser.EntityLogicalName, u.PartyId.Id, new ColumnSet(true));
                            if (MatchResourceToGroup(user.Id, group.Id))
                            {
                                Logger.WriteDebugMessage("User matched in Group and added.");
                                recipients.Add(user);
                            }
                            else
                                Logger.WriteDebugMessage("User did not match in Group and was not added.");
                        }
                    }
                }
            }
            return recipients.GroupBy(A => A.SystemUserId.Value).Select(g => g.First()).ToList(); ;
        }

        /// <summary>
        /// filters down the list of all equipment on SA based on criteria provided
        /// </summary>
        /// <param name="equipment">collection of mcs_resources that correspond to the equipment in the resources field on the sa</param>
        /// <param name="tsa">SP for the Service Activity</param>
        /// <param name="prgs">cvt_schedulingresource for all resources on the SP</param>
        /// <param name="equipType">type of mcs_resource (room, tech, vista clinic, etc.)</param>
        /// <returns>the list of mcs_resources based on the filters listed (pro or pat location and equipment type)</returns>
        internal List<mcs_resource> ClassifyResources(EntityCollection equipment, List<cvt_schedulingresource> schRes, int? equipType)
        {
            Logger.WriteDebugMessage("Starting classify function.");
            List<mcs_resource> relevantResources = new List<mcs_resource>();

            var singles = schRes.Where(sr => sr.cvt_schedulingresourcetype.Value == (int)cvt_tsaresourcetype.SingleResource).ToList();
            var groups = schRes.Where(sr => sr.cvt_schedulingresourcetype.Value == (int)cvt_tsaresourcetype.ResourceGroup).ToList();

            Logger.WriteDebugMessage("Sorting through single resource results.");
            foreach (cvt_schedulingresource singleSR in singles)
            {
                if (singleSR.Attributes.Contains("cvt_tmpresource") && singleSR.cvt_tmpresource != null)
                {
                    Logger.WriteDebugMessage("Single = " + singleSR.cvt_name);
                    foreach (mcs_resource r in equipment.Entities)
                    {
                        if (r.mcs_Type?.Value != equipType && equipType != null)
                            continue;

                        mcs_resource resource = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName,
                                singleSR.cvt_tmpresource.Id, new ColumnSet(true));
                        if (resource != null && resource.Id == r.Id)
                            relevantResources.Add(r);
                    }
                }
            }

            Logger.WriteDebugMessage("Sorting through group resource results.");
            foreach (cvt_schedulingresource groupSR in groups)
            {
                if (groupSR != null && groupSR.Attributes.Contains("cvt_tmpresourcegroup") && groupSR.cvt_tmpresourcegroup != null)
                {
                    Logger.WriteDebugMessage("Group = " + groupSR.cvt_name);
                    mcs_resourcegroup group = (mcs_resourcegroup)OrganizationService.Retrieve(mcs_resourcegroup.EntityLogicalName,
                                groupSR.cvt_tmpresourcegroup.Id, new ColumnSet(true));
                    if (group != null && group.mcs_Type != null)
                    {
                        if (group.mcs_Type?.Value == (int)mcs_resourcetype.Room || group.mcs_Type?.Value == (int)mcs_resourcetype.Technology || group.mcs_Type?.Value == (int)mcs_resourcetype.PairedResourceGroup)
                        {
                            foreach (mcs_resource r in equipment.Entities)
                            {
                                if (r.mcs_Type?.Value != equipType && equipType != null)
                                    continue;

                                if (MatchResourceToGroup(r, group))
                                {
                                    relevantResources.Add(r);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Logger.WriteDebugMessage("Exiting classify function.");
            return relevantResources;
        }

        internal string MatchGroupPatResources(List<cvt_schedulingresource> schRes, List<mcs_resource> patVC, List<mcs_resource> patRooms, List<mcs_resource> patTech)
        {
            Logger.WriteDebugMessage("Starting");
            //$"Patient Site: {patPS.cvt_site.Name} Device Type: () Room Number ()";
            var output = string.Empty;
            var roomText = string.Empty;
            var deviceText = string.Empty;

            bool matchedResToPS = false;

            var singles = schRes.Where(sr => sr.cvt_schedulingresourcetype.Value == (int)cvt_tsaresourcetype.SingleResource).ToList();
            var groups = schRes.Where(sr => sr.cvt_schedulingresourcetype.Value == (int)cvt_tsaresourcetype.ResourceGroup).ToList();

            Logger.WriteDebugMessage("Sorting through single resource results.");
            foreach (cvt_schedulingresource singleSR in singles)
            {
                Logger.WriteDebugMessage("Single = " + singleSR.cvt_name);
                if (singleSR.Attributes.Contains("cvt_tmpresource") && singleSR.cvt_tmpresource != null)
                {
                    foreach (mcs_resource r in patVC)
                    {
                        if (singleSR.cvt_tmpresource.Id == r.Id)
                            matchedResToPS = true;
                    }
                    foreach (mcs_resource r in patRooms)
                    {
                        if (singleSR.cvt_tmpresource.Id == r.Id)
                        {
                            roomText += r.mcs_name + ";";
                            matchedResToPS = true;
                        }
                    }
                    foreach (mcs_resource r in patTech)
                    {
                        if (singleSR.cvt_tmpresource.Id == r.Id)
                        {
                            deviceText += r.mcs_name + ";";
                            matchedResToPS = true;
                        }
                    }
                }
            }

            Logger.WriteDebugMessage("Sorting through Group results.");
            foreach (cvt_schedulingresource groupSR in groups)
            {
                if (groupSR != null && groupSR.Attributes.Contains("cvt_tmpresourcegroup") && groupSR.cvt_tmpresourcegroup != null)
                {
                    Logger.WriteDebugMessage("Group = " + groupSR.cvt_name);
                    mcs_resourcegroup group = (mcs_resourcegroup)OrganizationService.Retrieve(mcs_resourcegroup.EntityLogicalName,
                                groupSR.cvt_tmpresourcegroup.Id, new ColumnSet(true));
                    if (group != null && group.mcs_Type != null)
                    {
                        foreach (mcs_resource r in patVC)
                        {
                            if (MatchResourceToGroup(r, group))
                                matchedResToPS = true;
                        }
                        foreach (mcs_resource r in patRooms)
                        {
                            if (MatchResourceToGroup(r, group))
                            {
                                roomText += r.mcs_name + ";";
                                matchedResToPS = true;
                            }
                        }
                        foreach (mcs_resource r in patTech)
                        {
                            if (MatchResourceToGroup(r, group))
                            {
                                deviceText += r.mcs_name + ";";
                                matchedResToPS = true;
                            }
                        }

                    }
                }

            }
            Logger.WriteDebugMessage("Exiting");
            output = $" Device Type: {deviceText} Room Number {roomText}";
            if (matchedResToPS == true)
                return output;
            else
                return string.Empty;
        }
        internal string getPatientVirtualMeetingSpace(out bool? patientSpace)
        {
            Logger.WriteDebugMessage("Getting Virtual Meeting Space");
            patientSpace = null;
            isVAIssuediOSDevice = false;
            if (ServiceAppointment.mcs_PatientUrl != null && ServiceAppointment.mcs_providerurl != null)
            {
                PatientVirtualMeetingSpace = ServiceAppointment.mcs_PatientUrl;
                ProviderVirtualMeetingSpace = ServiceAppointment.mcs_providerurl;
                Logger.WriteDebugMessage("Virtual Meeting Space is from Service Activity Record: " + PatientVirtualMeetingSpace + ", " + ProviderVirtualMeetingSpace);
            }
            
            var patientAP = ServiceAppointment.Customers.FirstOrDefault();
            if (patientAP == null || patientAP.PartyId == null)
                return string.Empty;
            Logger.WriteDebugMessage(ServiceAppointment.Customers.ToList().Count().ToString() + " patients " + patientAP.PartyId.Name.ToString());
            var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, patientAP.PartyId.Id, new ColumnSet(true));
            Logger.WriteDebugMessage("Contact: " + patient.FullName + " VMR: " + patient.cvt_PatientVirtualMeetingSpace);

            if (patient != null && patient.cvt_TabletType != null)
            {
                //-Patient has a Technology Type “GFE Tablet” and “Do Not Allow Emails” is marked “Allow”
                //-Patient has a Technology Type “Home / Mobile Device”
                if ((patient.cvt_TabletType.Value == (int)Contactcvt_TabletType.VAIssuediOSDevice && !patient.DoNotEMail.Value) || patient.cvt_TabletType.Value == (int)Contactcvt_TabletType.PersonalVAVideoConnectDevice)
                {
                    if (patient.cvt_PatientVirtualMeetingSpace != null)
                    {
                        patientSpace = true;
                        PatientVirtualMeetingSpace = patient.cvt_PatientVirtualMeetingSpace;
                        ProviderVirtualMeetingSpace = patient.cvt_ProviderVirtualMeetingSpace;
                    }
                }
                else if (patient.cvt_TabletType.Value == (int)Contactcvt_TabletType.VAIssuediOSDevice && patient.DoNotEMail.Value)
                {
                    PatientVirtualMeetingSpace = patient.cvt_staticvmrlink;
                    ProviderVirtualMeetingSpace = patient.cvt_staticvmrlink;
                    isVAIssuediOSDevice = true;
                }
            }
            else if ((VirtualMeetingSpaceComponent != null) && (VirtualMeetingSpaceComponent.Id != new Guid()))
                PatientVirtualMeetingSpace = VirtualMeetingSpaceComponent.cvt_webinterfaceurl;
            else
                PatientVirtualMeetingSpace = "Please Contact Your TCT for Web Meeting Details";
            Logger.WriteDebugMessage(PatientVirtualMeetingSpace + ": Virtual Meeting Space is from Patient record = " + patientSpace.ToString());

            return PatientVirtualMeetingSpace;
        }

        internal string getParamValue(string url, string key)
        {
            var result = string.Empty;
            var parameter = url.Split('&').LastOrDefault(s => s.ToLower().Contains(key));
            var parameterKeyValue = parameter != null ? parameter.Split('=') : null;
            if (parameterKeyValue != null && parameterKeyValue.Count() == 2)
                result = parameterKeyValue[1];
            return result;
        }

        internal string getComponents(mcs_resource technology, ServiceAppointment SA, out string plainText)
        {
            //Get all the components and include the CEVN Alias and IP Addresses for each.  Return the formatted string with a line for each Component
            //virtualMeetingSpace = null;
            Logger.WriteDebugMessage("Begin Get Component List");
            string components = null;
            plainText = "";
            using (var context = new Xrm(OrganizationService))
            {
                var compList = context.cvt_componentSet.Where(c => c.cvt_relatedresourceid.Id == technology.Id);
                foreach (cvt_component c in compList)
                {
                    if (components == null)
                        components += "<ul>";
                    components += "<li>" + c.cvt_name;
                    Logger.WriteDebugMessage("Component Item Name: " + c.cvt_name);

                    plainText += $"-{c.cvt_name}";
                    switch (c.cvt_name)
                    {
                        case "Codec, Hardware":
                        case "1700":
                        case "C-Series Codec":
                        case "DX-Series Codec":
                        case "Edge 95 Codec":
                        case "EX-Series Codec":
                        case "Hardware codec":
                        case "HDX PolyCom Codec":
                        case "MXP Codec":
                        case "MXP":
                        case "SX-Series Codec":
                            if (c.cvt_cevnalias != null)
                            {
                                components += "; CEVN Alias: " + c.cvt_cevnalias;
                                plainText += "; CEVN Alias: " + c.cvt_cevnalias;
                            }
                            break;
                        case "Telemedicine Encounter Management":
                        case "Telemed Encounter Management":
                        case "TEMS (Telemedicine Encounter Management Software)":
                        case "TEMS (Telemed Encounter Management Software)":
                            if (c.cvt_ipaddress != null)
                            {
                                components += "; IP Address: " + CvtHelper.buildHTMLUrl(c.cvt_ipaddress);
                                plainText += $"; IP Address: {c.cvt_ipaddress}";
                            }
                            break;
                        case "CVT Patient Tablet":
                            if (c.cvt_serialnumber != null)
                            {
                                components += "; Serial Number: " + c.cvt_serialnumber;
                                plainText += "; Serial Number: " + c.cvt_serialnumber;
                            }
                            break;
                        case "Virtual Meeting Space":
                            VirtualMeetingSpaceComponent = c;
                            break;
                        case "Digital Stethoscope Peripheral":
                            stethIP = c.cvt_ipaddress;
                            Logger.WriteDebugMessage("Component IP number: " + c.cvt_ipaddress);
                            break;
                    }
                    //Send URL
                    var url = "";
                    var contact = getDummyContact();
                    var secondaryEntities = new List<Entity>();
                    secondaryEntities.Add(SA);
                    secondaryEntities.Add(contact);
                    if (UrlBuilder.TryGetUrl(OrganizationService, this.GetType().ToString(), c, secondaryEntities, out url))
                        components += "; <a href=" + url + ">" + url + "</a>";
                    components += "</li>";
                }
                if (components != null)
                    components += "</ul>";
            }
            return components;
        }

        internal Contact getDummyContact()
        {
            Contact c = new Contact();
            using (var srv = new Xrm(OrganizationService))
            {
                c = srv.ContactSet.FirstOrDefault();
            }
            return c;
        }
        #endregion

    }
}
