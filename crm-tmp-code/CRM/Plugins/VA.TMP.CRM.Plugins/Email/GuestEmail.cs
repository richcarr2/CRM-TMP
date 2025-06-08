using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class GuestEmail
    {
        #region Constructor/Data Model for this type of email

        IOrganizationService OrganizationService;
        MCSLogger Logger;
        Email Email;

        public GuestEmail(IOrganizationService organizationService, MCSLogger logger, Email email)
        {
            OrganizationService = organizationService;
            Logger = logger;
            Email = email;
        }

        #endregion

        #region Entry point (Execute Method) for this class
        public void Execute()
        {
            Logger.WriteDebugMessage("about to call 'SendGuestEmail'");
            //throw new InvalidOperationException("in Execute method");


            string desc = Email.Description;
           // Logger.WriteDebugMessage(desc);
           // Logger.WriteDebugMessage("1");
            char[] delimiter = { ',' };
           // Logger.WriteDebugMessage("2");
            string[] workstring = desc.Split(delimiter);
           // Logger.WriteDebugMessage("3");
            string vetInitials = workstring[0];
           // Logger.WriteDebugMessage("4");
            string emailaddress = workstring[1];
           // Logger.WriteDebugMessage("5");

            var guest = new Contact()
            {
                LastName = "Guest " + vetInitials,
                FirstName = "Veteran ",
                //LastName = emailaddress ,
                //FirstName = vetInitials,
                EMailAddress1 = emailaddress
            };

            Logger.WriteDebugMessage("regarding object id:" + Email.RegardingObjectId.ToString());

            guest.Id = OrganizationService.Create(guest);
            Logger.WriteDebugMessage("Guest Created - about to send email");
            SendGuestEmail(Email.RegardingObjectId, guest);
            Logger.WriteDebugMessage("email sent");
            OrganizationService.Delete("contact", guest.Id);

        }
        #endregion

        #region Create and send the email message

        //Send appropriate email based on subject line and type
        private void SendGuestEmail(EntityReference regardingObject, Contact guest)
        {
            var initials = string.Empty;
            var proInitials = string.Empty;
            var subject = string.Empty;
            var footerText = $"For technical assistance, contact the National Telehealth Technology Help Desk (NTTHD) 866 651-3180 or (703) 234-4483, Monday through Saturday, 7 a.m. through 11 p.m. EST.";
            string body = string.Empty;
            string PatientVirtualMeetingSpace = string.Empty;
            bool? patientSpace;

            //create a contact record so CRM can send out an email.  We'll delete it later.

            var guestAP = new ActivityParty
            {
                PartyId = new EntityReference(Contact.EntityLogicalName, guest.ContactId.Value)
            };



            Logger.WriteDebugMessage("about to get ServiceActivity");
            ServiceAppointment sa = (ServiceAppointment)OrganizationService.Retrieve("serviceappointment", Email.mcs_RelatedServiceActivity.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Logger.WriteDebugMessage("Success! got ServiceActivity");
            Logger.WriteDebugMessage("about to get provider");

            if (sa.Contains("cvt_relatedproviderid"))
            {
                EntityReference erPro = (EntityReference)sa.Attributes["cvt_relatedproviderid"];

                Entity pro = OrganizationService.Retrieve("systemuser", erPro.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                if (pro.Attributes["firstname"] != null)
                {
                    proInitials += pro.Attributes["firstname"].ToString().Substring(0, 1);
                }
                if (pro.Attributes["lastname"] != null)
                {
                    proInitials += pro.Attributes["lastname"].ToString().Substring(0, 1);
                }
            }
            Logger.WriteDebugMessage("Success! got provider");


            if (sa.Contains("mcs_patienturl"))
            {
                Logger.WriteDebugMessage("about to get 'mcs_patientUrl' from  ServiceActivity");
                PatientVirtualMeetingSpace = sa.Attributes["mcs_patienturl"].ToString();
                Logger.WriteDebugMessage("Success! got patientUrl");

            }


            if ((PatientVirtualMeetingSpace == null) || (PatientVirtualMeetingSpace == string.Empty))
            {
                PatientVirtualMeetingSpace = getPatientVirtualMeetingSpace(sa, out patientSpace);
                Logger.WriteDebugMessage("Success! got PatientVirtualMeetingSpace");
            }


            if (regardingObject.LogicalName == cvt_nonvaemail.EntityLogicalName)
            {
                //Send email to veteran's guest
                //Subject: [External] Guest Invitation to Video Health Appointment for a Veteran you know
                Logger.WriteDebugMessage("about to create the email message");
                subject = Email.Subject;

                string desc = Email.Description;
                char[] delimiter = { ',' };
                string[] workstring = desc.Split(delimiter);
                string vetInitials = workstring[0];
                string emailaddress = workstring[1];

                using (var srv = new Xrm(OrganizationService))
                {
                    var scheduled = ((DateTime)sa.Attributes["scheduledstart"]);

                    body += $"A veteran with Initials \"" + vetInitials + "\" has requested that you join their scheduled Veteran Health Administration video health appointment.<br/><br/>";
                    body += $"<font size='4' color='#000000' face='Tahoma'><b>Appointment information: </b></font><br/>Date/Time: " + scheduled.ToString("MM/dd/yyyy h:mm tt") + "<br/>";
                    body += $"Clinician: " + proInitials + "<br/><br/>";
                    body += $"<font size='4' color='#000000' face='Tahoma'><b>Join the appointment: </b></font><br/>";
                    body += $"" + CvtHelper.buildHTMLUrlAlt(input: PatientVirtualMeetingSpace, clickDisplay: "Click Here to Join the VA Video Connect appointment<br/><br/> ") + "<br/>";
                    body += $"<font size='4' color='#000000' face='Tahoma'><b>VA Video Connect (VVC) Appointment Instructions:</b></font><br/>";
                    body += $"Ensure you are in a private place with good internet connectivity.<br/>";
                    body += $"Be ready to identify yourself when the appointment starts.<br/>";
                    body += $"You will be able to join the appointment 15 minutes before the scheduled appointment time. <br/><br/>";
                    body += $"If you plan to use an iPhone or iPad for your appointment, download the free VA Video Connect (VVC) app from the Apple App store.<br/>";
                    body += $"{CvtHelper.buildHTMLUrlAlt("https://itunes.apple.com/us/app/va-video-connect/id1224250949?mt=8", "Click Here to download the VVC iOS app.")}";
                    body += $"<br /><br />{CvtHelper.GenerateNeedHelpSection(false, out var plainText)}";
                }
                Email.To = new ActivityParty[] { guestAP };
                Email.Description = body;
                Email.From = CvtHelper.GetWorkflowOwner("TSA Approval Step 1 - Awaiting Prov FTC", OrganizationService);
                //throw new InvalidOperationException("at the end");
                CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
            }
        }
        #endregion
        #region Get Patient Meeting Space
        internal string getPatientVirtualMeetingSpace(ServiceAppointment ServiceAppointment, out bool? patientSpace)
        {
            Logger.WriteDebugMessage("Inside getPatientVirtualMeetingSpace method");
            string PatientVirtualMeetingSpace = string.Empty;
            string ProviderVirtualMeetingSpace = string.Empty;
            Logger.WriteDebugMessage("Getting Virtual Meeting Space");
            patientSpace = null;
            bool isVAIssuediOSDevice = false;
            bool isCvtTablet = false;

            if (ServiceAppointment.Contains("mcs_patienturl"))
            {
                Logger.WriteDebugMessage("mcs_patienturl field is present");
                if (ServiceAppointment.Contains("mcs_providerurl"))
                {
                    Logger.WriteDebugMessage("mcs_providerurl field is present");
                    if (ServiceAppointment.Attributes["mcs_patienturl"] != null && ServiceAppointment.Attributes["mcs_providerurl"] != null)
                    {
                        PatientVirtualMeetingSpace = ServiceAppointment.Attributes["mcs_PatientUrl"].ToString();
                        ProviderVirtualMeetingSpace = ServiceAppointment.Attributes["mcs_providerurl"].ToString();
                        Logger.WriteDebugMessage("Virtual Meeting Space is from Service Activity Record: " + PatientVirtualMeetingSpace + ", " + ProviderVirtualMeetingSpace);
                    }
                }
            }

            var patientAP = ServiceAppointment.Customers.FirstOrDefault();

            //if (patientAP == null || patientAP.PartyId == null)
            //    return string.Empty;
            Logger.WriteDebugMessage("Retreiving Patient record");

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
            //else if ((VirtualMeetingSpaceComponent != null) && (VirtualMeetingSpaceComponent.Id != new Guid()))
            //    PatientVirtualMeetingSpace = VirtualMeetingSpaceComponent.cvt_webinterfaceurl;
            else
                PatientVirtualMeetingSpace = "Please Contact Your TCT for Web Meeting Details";
            Logger.WriteDebugMessage(PatientVirtualMeetingSpace + ": Virtual Meeting Space is from Patient record = " + patientSpace.ToString());

            return PatientVirtualMeetingSpace;
        }
        #endregion
    }
}
