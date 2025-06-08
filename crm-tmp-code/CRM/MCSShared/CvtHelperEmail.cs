using MCS.ApplicationInsights;
using MCSUtilities2011;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace MCSShared
{
    public static partial class CvtHelper
    {

        #region Email Related Functions
        //Overloaded method: creates a list from an Activty Party to populate a From, To, or CC field
        public static List<ActivityParty> SetPartyList(ActivityParty user)
        {
            List<ActivityParty> userList = new List<ActivityParty>();
            //user if exists
            if (user != null)
                userList.Add(user);
            return userList;
        }

        //Overloaded method: return empty list if userRef is null, otherwise get Activity party and call first SetEmailProperties method
        public static List<ActivityParty> SetPartyList(EntityReference userRef)
        {
            return userRef == null ? new List<ActivityParty>() :
                CvtHelper.SetPartyList(new ActivityParty() { PartyId = new EntityReference(userRef.LogicalName, userRef.Id) });
        }

        //Overloaded method: creates a list from an EntityCollection to populate a From, To, or CC field
        public static List<ActivityParty> SetPartyList(EntityCollection userCollection)
        {
            List<ActivityParty> userList = new List<ActivityParty>();

            foreach (var record in userCollection.Entities.Select(e => e).Distinct())
            {
                var activityParty = new ActivityParty() { PartyId = new EntityReference(record.LogicalName, record.Id) };
                userList.Add(activityParty);
            }
            return userList;
        }

        public static List<ActivityParty> SetPartyList(List<SystemUser> userList)
        {
            List<ActivityParty> users = new List<ActivityParty>();

            foreach (SystemUser u in userList.Select(e => e).Distinct())
            {
                var activityParty = new ActivityParty() { PartyId = new EntityReference(u.LogicalName, u.Id) };
                users.Add(activityParty);
            }
            return users;
        }

        //This method takes the email passed in and sends it
        public static void SendEmail(Email email, IOrganizationService OrganizationService, MCSLogger logger)
        {
            //check for pssc in the server url, if it is true, then do nothing
            var serverURL = CvtHelper.getServerURL(OrganizationService);
            if (!serverURL.Contains("pssc"))
            {
                try
                {
                    SendEmailRequest requestObj = new SendEmailRequest()
                    {
                        EmailId = (Guid)email.ActivityId,
                        IssueSend = true,
                        TrackingToken = ""
                    };
                    SendEmailResponse response = (SendEmailResponse)OrganizationService.Execute(requestObj);
                }
                catch (Exception ex)
                {
                    logger.WriteToFile($"Error occured while sending the email with id: {email.ActivityId.Value}\nDetails:{ex.Message} {ex}");
                }
            }
        }

        //Saves and Sends the email
        public static void UpdateSendEmail(Email email, IOrganizationService OrganizationService, MCSLogger logger)
        {
            try
            {
                if (email.To.Count() > 0)
                {
                    OrganizationService.Update(email);

                    if (email.Subject.Contains("Telehealth Appointment Notification for"))
                    {
                        var xml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='email'>
                                        <attribute name='subject' />                                        
                                        <order attribute='subject' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='mcs_relatedserviceactivity' operator='eq' uiname='Primary Care' uitype='serviceappointment' value='{email.mcs_RelatedServiceActivity.Id}' />
                                          <condition attribute='subject' operator='like' value='% + {email.Subject} + %' />
                                         <condition attribute='description' operator='like' value= '%This is an automated Message to notify you that a Telehealth Appointment has%' />
                                        </filter>
                                      </entity>
                                    </fetch>";

                        var response = OrganizationService.RetrieveMultiple(new FetchExpression(xml));
                        if (response != null && response.Entities != null && response.Entities.Count > 1)
                        {
                            logger.WriteDebugMessage($"Email with subject 'Telehealth Appointment Notification for' already been sent, not sending email again");
                            // OrganizationService.Delete(Email.EntityLogicalName, email.Id);
                            return;
                        }
                    }

                    SendEmail(email, OrganizationService, logger);
                }
                else
                {
                    email.Description = "No recipients found, could not send.  " + email.Description;
                    OrganizationService.Update(email);
                }
            }
            catch (Exception ex)
            {
                logger.WriteToFile($"Error occured while updating the email with id: {email.ActivityId.Value}\nDetails:{ex.Message} {ex}");
            }
        }

        public static string GenerateNeedHelpSection(bool isCancelled, out string plainTextHelpSection)
        {
            var testurl = "https://care.va.gov/vvc-app?name=TestPatient&conference=TestWaitingRoom@care.va.gov&pin=5678";
            var needHelpSection = (isCancelled) ? string.Empty : $"<b>Need Help?</b><br/><ul><li>If you would like to test your connection to VA Video Connect prior to your appointment, {CvtHelper.buildHTMLUrlAlt(testurl, "Click Here to Test")}"
                + $"<li>If you would like more information about VA Video Connect, please review VA Video Connect information at the following link: {CvtHelper.buildHTMLUrlAlt("https://mobile.va.gov/app/va-video-connect", "Additional VA Video Connect Information")}</ li > "
                + "<li>If you need technical assistance or want to do a test call with a VA help desk technician, please call the Office of Connected Care Help Desk (OCCHD) (866) 651 - 3180 or (703) 234 - 4483, available 24/7/365</li></ul><br/><br/>";
            needHelpSection += "<b>Need to Reschedule?</b><br/>Do not reply to this message. This message is sent from an unmonitored mailbox.  For any questions or concerns please contact your VA Facility or VA Clinical Team.<br/>"
               + "-----------------------------------------------------------------------------------------------------------------------------------------<br/>"
               + "Please do not reply to this message.It comes from an unmonitored mailbox.";

            plainTextHelpSection = (isCancelled) ? string.Empty : $"Need Help?\\n          o     If you would like to test your connection to VA Video Connect prior to your appointment, Click Here to Test: {testurl}"
                + $"\\n          o     If you would like more information about VA Video Connect, please review VA Video Connect information at the following link: https://mobile.va.gov/app/va-video-connect"
                + "\\n          o     If you need technical assistance or want to do a test call with a VA help desk technician, please call the Office of Connected Care Help Desk (OCCHD) (866) 651 - 3180 or (703) 234 - 4483, available 24/7/365\\n\\n";
            plainTextHelpSection += "Need to Reschedule?\\nDo not reply to this message. This message is sent from an unmonitored mailbox.  For any questions or concerns please contact your VA Facility or VA Clinical Team.\\n"
                + "-----------------------------------------------------------------------------------------------------------------------------------------\\n"
                + "Please do not reply to this message.It comes from an unmonitored mailbox.";

            return needHelpSection;
        }

        public static string EmailFooter()
        {
            return "<br/><br/>Please Do Not Reply to this message.  It comes from an unmonitored mailbox.  For any questions or concerns, please contact your VA Facility or VA Clinical Team.";
        }

        public static string TechnicalAssistanceEmailFooter(out string plainTextFooter)
        {
            plainTextFooter = "\\nTechnical Assistance\\n";
            var footer = "<br/><b>Technical Assistance</b><br/>";
            var commonText = "For technical assistance, clinicians should contact the Office of Connected Care Help Desk (OCCHD) (866) 651 - 3180 or (703) 234 - 4483, available 24/7/365 EST.{0}" +
                            "-----------------------------------------------------------------------------------------------------------------------------------------{0}" +
                            "Please do not reply to this message.It comes from an unmonitored mailbox.";
            plainTextFooter += commonText.Replace("{0}", "\\n");
            return footer + commonText.Replace("{0}", "<br/>");
        }

        public static bool ShouldGenerateVeteranEmail(Guid patientId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            bool shouldGenerateVeteranEmail = false;
            using (var srv = new Xrm(OrganizationService))
            {
                var patient = srv.ContactSet.FirstOrDefault(p => p.Id == patientId);
                if (patient != null && patient.cvt_TabletType != null)
                {
                    switch (patient.cvt_TabletType.Value)
                    {
                        case (int)Contactcvt_TabletType.PersonalDevice:
                            shouldGenerateVeteranEmail = true;
                            break;
                        case (int)Contactcvt_TabletType.VALoanedDevice:
                            if (patient.DoNotEMail != null && !patient.DoNotEMail.Value)
                            {
                                shouldGenerateVeteranEmail = true;
                            }
                            break;
                    }
                }
                Logger.WriteDebugMessage($"ShouldGenerateVeteranEmail for patient with name: {patient.FullName} and ID: {patientId} returned {shouldGenerateVeteranEmail}");
            }
            return shouldGenerateVeteranEmail;
        }

        public static bool ShouldGenerateVeteranEmail(Guid patientId, IOrganizationService OrganizationService, PluginLogger Logger)
        {
            bool shouldGenerateVeteranEmail = false;
            using (var srv = new Xrm(OrganizationService))
            {
                var patient = srv.ContactSet.FirstOrDefault(p => p.Id == patientId);
                if (patient != null && patient.cvt_TabletType != null)
                {
                    switch (patient.cvt_TabletType.Value)
                    {
                        case (int)Contactcvt_TabletType.PersonalDevice:
                            shouldGenerateVeteranEmail = true;
                            break;
                        case (int)Contactcvt_TabletType.VALoanedDevice:
                            if (patient.DoNotEMail != null && !patient.DoNotEMail.Value)
                            {
                                shouldGenerateVeteranEmail = true;
                            }
                            break;
                    }
                }
                Logger.Trace($"ShouldGenerateVeteranEmail for patient with name: {patient.FullName} and ID: {patientId} returned {shouldGenerateVeteranEmail}");
            }
            return shouldGenerateVeteranEmail;
        }


        ///// <summary>
        ///// Overload for basic generateEmailBody - displays the url as the message for "Click Here"
        ///// </summary>
        ///// <param name="record">ID of the email</param>
        ///// <param name="entityStringName">string name of the entity - to retrieve object type code</param>
        ///// <param name="customMessage">The message</param>
        ///// <returns></returns>
        //internal static string GenerateEmailBody(Guid record, string entityStringName, string customMessage, IOrganizationService OrganizationService)
        //{
        //    return GenerateEmailBody(record, entityStringName, customMessage, OrganizationService);
        //}

        /// <summary>
        /// Standard "boilerplate" E-Mail body
        /// </summary>
        /// <param name="record">ID of the email record</param>
        /// <param name="entityStringName">The string name of the object type code</param>
        /// <param name="customMessage">The custom string that goes into the email body</param>
        /// <param name="clickHereMessage">The message that is used as the display for the hyperlink</param>
        /// <returns>the body of the email</returns>
        internal static string GenerateEmailBody(Guid record, string entityStringName, string customMessage, IOrganizationService OrganizationService, string clickHereMessage = "")
        {
            string body;
            body = GetRecordLink(new EntityReference(entityStringName, record), OrganizationService, clickHereMessage);
            body += "<br/><br/>" + customMessage;
            body += EmailFooter();

            return body;
        }

        internal static string GetRecordLink(Entity record, IOrganizationService orgService, string clickHereText = "")
        {
            return GetRecordLink(new EntityReference(record.LogicalName, record.Id), orgService, clickHereText);
        }

        internal static string GetRecordLink(EntityReference record, IOrganizationService OrganizationService, string clickHereText = "")
        {
            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, record.LogicalName);
            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            string url = servernameAndOrgname + "/userDefined/edit.aspx?etc=" + etc + "&id=" + record.Id;
            return string.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);
        }

        internal static string GetEntityLink(Entity record, IOrganizationService orgService, string clickHereText = "")
        {
            return GetEntityLink(new EntityReference(record.LogicalName, record.Id), orgService, clickHereText);
        }

        internal static string GetEntityLink(EntityReference record, IOrganizationService OrganizationService, string clickHereText = "")
        {
            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            string url = servernameAndOrgname + "/main.aspx?newWindow=true&pagetype=entityrecord&etn=" + record.LogicalName + "&id=" + record.Id + "&extraqs=from_apptemail=true";
            return string.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);
        }

        internal static string ProviderSafetyChecks()
        {
            var safetyChecks = "During your initial assessment be sure to verify the following: ";
            safetyChecks += "<ul><li>Do you have any concerns about suicide?</li>";
            safetyChecks += "<li>The Patient verbally consents to the telehealth visit?</li>";
            safetyChecks += "<li>If the line drops, what number can I call you at?</li>";
            safetyChecks += "<li>What is the name, phone number, and relationship of the person we should contact in the case of an emergency?</li>";
            safetyChecks += "<li>What is your local 10 digit phone number for law enforcement in your community?</li>";
            safetyChecks += "<li>What is the address of your location during this visit?</li></ul>";
            safetyChecks += "<li>Are you in a safe and private place?</li></ul>";
            return safetyChecks;
        }

        internal static string VvcAppointmentInstructions(bool isCvtTablet, out string plainTextInstructions)
        {
            plainTextInstructions = $"\\n\\n{(isCvtTablet ? "VVC Tablet SIP Address dialing" : "VA Video Connect (VVC)")} Appointment Instructions:\\n"
                                  + $"          o     {(isCvtTablet ? "To dial a patient's VVC SIP Tablet, you must add SIP and colon as a prefix to any SIP Address, plus tablet number, as well as .bltablet to the end of each SIP VVC Tablet Address." : "Ensure you are in a private and safe space with good internet connectivity, and have the following information available:")}\\n"
                                   + $"{(isCvtTablet ? "          o     To more easily find the VVC tablet SIP address at the time of the visit, we recommend you do one of the following:\\n" : "          o     Phone number: How we can reach you by telephone, if the video call drops.")} \\n";
            plainTextInstructions += $"{(isCvtTablet ? "          o     Save the attached invitation to your Outlook Calendar\\n" : "          o     Address: Your location during the visit. \\n")}";
            plainTextInstructions += $"{(isCvtTablet ? "          o      Copy this invitation into your Outlook calendar and create a calendar entry at the time of the visit, and/ or\\n" : "          o     Emergency contact: Name, relationship, and phone number of a person who we can contact in an emergency. \\n\\n")}";
            plainTextInstructions += $"{(isCvtTablet ? "          o     Search your Outlook e-mail for the date or Last Initial / Last 4, on the day of the visit.\\n\\n" : " ")}";
            plainTextInstructions += $"{(isCvtTablet ? "          o     At the start of your VVC Appointment , remember (CAPS-Lock):\\n*Some information can be obtained prior to the VVC appointment by another member of the care team.\\n" : "")}";
            plainTextInstructions += $"{(isCvtTablet ? "          o     Consent*: Obtain or confirm that verbal consent for telehealth has been documented (Note: This is a one-time requirement for your service)\\n" : "")}";
            plainTextInstructions += $"{(isCvtTablet ? "          o     Address: Obtain or confirm the location and address of the patient to ensure they are in a safe place and for use in case of an emergency.\\n" : "")}";
            plainTextInstructions += $"{(isCvtTablet ? "          o     Phone Numbers:*\\nPatient's current phone number - for use if disconnected.\\nEmergency contact's phone number - for use in an emergency.\\n" : "")}";
            plainTextInstructions += $"{(isCvtTablet ? "          o     Survey the environment and identify all participants.\\n" : "\\n\\n")}";

            var apptInstructions = $"<br/><b>{(isCvtTablet ? "VVC Tablet SIP Address dialing" : "VA Video Connect (VVC)")} Appointment Instructions:</b>"
                              //+ $"<ol><li>{(isCvtTablet ? "To dial a patient’s VVC SIP Tablet, you must add SIP and colon as a prefix to any SIP Address, plus tablet number, as well as .bltablet to the end of each SIP VVC Tablet Address." : "The \"Click Here\" link above is unique to this visit.  You will need to click it to enter the Virtual Medical Room at the time of the appointment.")}</li>"
                              + $"<li>{(isCvtTablet ? "To dial a patient’s VVC SIP Tablet, you must add SIP and colon as a prefix to any SIP Address, plus tablet number, as well as .bltablet to the end of each SIP VVC Tablet Address." : "The \"Click Here\" link above is unique to <b>this visit</b>.  You will need to click it to enter the Virtual Medical Room at the time of the appointment.")}</li>"
                              + $"{(isCvtTablet ? "<li>To more easily find the VVC tablet SIP address at the time of the visit, we recommend you do one of the following:</li>" : "<li>To more easily find this unique link at the time of the visit, we recommend you do one of the following:</li>")} ";
            apptInstructions += $"{(isCvtTablet ? "<ul><li>Save the attached invitation to your Outlook Calendar</li>" : "<ul><li>Save the attached invitation to your Outlook Calendar</li>")}";
            apptInstructions += $"{(isCvtTablet ? "<li> Copy this invitation into your Outlook calendar and create a calendar entry at the time of the visit, and/ or</li>" : "<li> Copy this invitation into your Outlook calendar and create a calendar entry at the time of the visit, and/ or</li>")}";
            apptInstructions += $"{(isCvtTablet ? "< li>Search your Outlook e-mail for the date or Last Initial / Last 4, on the day of the visit.</li></ul>" : "<li>Search your Outlook e-mail for the date or Last Initial / Last 4, on the day of the visit.</li></ul>")}";
            apptInstructions += $"{(isCvtTablet ? "<li>At the start of your VVC Appointment , remember (CAPS):<br/><i>*Some information can be obtained prior to the VVC appointment by another member of the care team.</i></li>" : "<li>At the start of your VVC Appointment , remember (CAPS-Lock):<br/><i>*Some information can be obtained prior to the VVC appointment by another member of the care team.</i></li>")}";
            apptInstructions += $"{(isCvtTablet ? "<ul><li><b><u>C</u></b>onsent*: Obtain or confirm that verbal consent for telehealth has been documented (Note: This is a one-time requirement for your service)</li>" : "<ul><li><b><u>C</u></b>onsent*: Obtain or confirm that verbal consent for telehealth has been documented (Note: This is a one-time requirement for your service)</li>")}";
            apptInstructions += $"{(isCvtTablet ? "<li><b><u>A</u></b>ddress: Obtain or confirm the location and address of the patient to ensure they are in a safe place and for use in case of an emergency.</li>" : "<li><b><u>A</u></b>ddress: Obtain or confirm the location and address of the patient to ensure they are in a safe place and for use in case of an emergency.</li>")}";
            apptInstructions += $"{(isCvtTablet ? "<li><b><u>P</u></b>hone Numbers:*<br/>Patient’s current phone number – for use if disconnected.<br/>Emergency contact’s phone number – for use in an emergency.</li>" : "<li><b><u>P</u></b>hone Numbers:*<br/>Patient’s current phone number – for use if disconnected.<br/>Emergency contact’s phone number – for use in an emergency.</li>")}";
            apptInstructions += $"{(isCvtTablet ? "<li><b><u>S</u></b>urvey the environment and identify all participants.</li>" : "<li><b><u>S</u></b>urvey the environment and identify all participants.</li><br/><br/>")}";
            apptInstructions += $"{(isCvtTablet ? string.Empty : "<li><b><u>L</u></b>ock the virtual conference room once all participants have joined.</li>")}";
            apptInstructions += $"</ul></ol>";

            return apptInstructions;
        }

        /// <summary>
        /// This method creates the .ics attachment and appends it to the email
        /// </summary>
        /// <param name="email">This is the email that the attachment is attaching to</param>
        /// <param name="sa">The service appointment which </param>
        /// <param name="statusCode">The status of the email - which sets the status of the attachment as well as the subject of the email</param>
        internal static void CreateCalendarAppointmentAttachment(Email email, string tmpAppointmentLink, ServiceAppointment sa, int statusCode, string stethIP, IOrganizationService OrganizationService, MCSLogger Logger, string plainDescription)
        {
            //handles the creation of .ics attachments for Provider-side staff, Patient-side staff, and TMP Provider emails
            bool group = false;
            if (sa.mcs_groupappointment != null)
            {
                group = sa.mcs_groupappointment.Value;
            }
            Logger.WriteDebugMessage("Begin Creating Calendar Appointment");
            string schLocation = "See Description";
            string schSubject = "Upcoming Appointment";
            string schDescription = tmpAppointmentLink;
            if (string.IsNullOrWhiteSpace(plainDescription))
                plainDescription = schDescription;
            System.DateTime schBeginDate = (System.DateTime)sa.ScheduledStart;
            System.DateTime schEndDate = (System.DateTime)sa.ScheduledEnd;
            string sequence = "";
            string status = "CONFIRMED";
            string method = "";
            //if the appointment is canceled, send a cancellation notice based on the UID of the previous entry sent
            if (statusCode == (int)serviceappointment_statuscode.PatientCanceled || statusCode == (int)serviceappointment_statuscode.ClinicCanceled)
            {
                method = "METHOD:CANCEL\n";
                sequence = "SEQUENCE:1\n";
                status = "CANCELLED";
                schSubject = $"Cancelled: {schSubject}: Do Not Reply";
            }


            //attach a ClearSteth CVL file if a steth is in the components
            Logger.WriteDebugMessage("Begin cvlAttachment: stethIP value = " + stethIP);
            string cvlAtttachment = string.IsNullOrEmpty(stethIP) ? "" :
                "ATTACH;ENCODING=BASE64;VALUE=BINARY;X-FILENAME=invitation.cvl:" + Convert.ToBase64String(new ASCIIEncoding().GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?><CVL><IP>" + stethIP + "</IP ><Port>9005</Port><ConferenceId>12345</ConferenceId></CVL>")) + "\n";
            Logger.WriteDebugMessage("CVL attachment conversion finished.");
            try
            {
                // Extract the href link and embed it in the main content
                MatchCollection matches = Regex.Matches(plainDescription, @"<a href=.+?>.+?</a>");

                foreach (Match match in matches)
                {
                    // Finding the link
                    string link = match.Value.Substring("<a href=".Length, match.Value.IndexOf(">") - "<a href=".Length);

                    string firstPortion = match.Value.Substring(0, match.Value.LastIndexOf("</a>"));
                    string replacementContent = String.Concat(firstPortion, " (", link, ")</a>");

                    // Updating the actual content with the replacement content
                    plainDescription = plainDescription.Replace(match.Value, replacementContent);
                }

                //some of the HTML tags have to be replaced to format the text content
                plainDescription = plainDescription.Replace("<br/>", "\\n").Replace("<br />", "\\n").Replace("<b>", "").Replace("</b>", "").Replace("<u>", "").Replace("</u>", "").Replace("<i>", "").Replace("</i>", "").Replace("<li>", "          o     ").Replace("</li>", "\\n").Replace("<ul>", "").Replace("</ul>", "\\n");

                string htmlTagRegex = @"(<(.|\n)+?>)"; //"<.+?>"
                //remaining HTML tags (assumption is that any content between < and > is HTML tag) have to be just removed all together (replaced with blank).
                plainDescription = System.Text.RegularExpressions.Regex.Replace(plainDescription, htmlTagRegex, "").Trim();

                string att = "BEGIN:VCALENDAR\n" +
                                  "PRODID:-//VA//Veterans Affairs//EN\n" +
                                  method +
                                  "BEGIN:VEVENT\n" +
                                  cvlAtttachment +
                                  "UID:" + sa.Id + "\n" + sequence +
                                  "DTSTART:" + schBeginDate.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + "\n" +
                                  "DTEND:" + schEndDate.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + "\n" +
                                  "LOCATION:" + schLocation +
                                  //Use Description tag for email clients that cant handle x-alt-desc tag with HTML
                                  "\nDESCRIPTION:" + plainDescription +
                                  "\nSUMMARY:" + schSubject + "\nPRIORITY:3\n" +
                                  "STATUS:" + status + "\n" +
                                  //Include alternate description if the calendar client can handle html x-alt-desc tag
                                  "X-ALT-DESC;FMTTYPE=text/html:<html>" + schDescription.Replace("\n", "<br/>") + "</html>" + "\n" +
                              "END:VEVENT\n" + "END:VCALENDAR\n";

                ActivityMimeAttachment calendarAttachment = new ActivityMimeAttachment()
                {
                    ObjectId = new EntityReference(Email.EntityLogicalName, email.Id),
                    ObjectTypeCode = Email.EntityLogicalName,
                    //Subject = $"{subjectPrefix} Appointment",
                    Subject = $"{schSubject}",
                    Body = Convert.ToBase64String(
                            new ASCIIEncoding().GetBytes(att)),
                    //FileName = string.Format(CultureInfo.CurrentCulture, $"{subjectPrefix}-Appointment.ics")
                    FileName = string.Format(CultureInfo.CurrentCulture, $"{schSubject}.ics")
                };
                Logger.WriteDebugMessage("About to Create Calendar Appointment");
                OrganizationService.Create(calendarAttachment);
                Logger.WriteDebugMessage("Finished Creating Calendar Appointment");
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"Failed to Create CVL Attachment. Error: {ex.InnerException.Message}");
            }
            return;
        }

        internal static void CreateCalendarAppointmentAttachment(Email email, ServiceAppointment sa, int statusCode, string stethIP, IOrganizationService OrganizationService, MCSLogger Logger, string plainDescription)
        {
            bool group = false;
            if (sa.mcs_groupappointment != null)
            {
                group = sa.mcs_groupappointment.Value;
            }
            Logger.WriteDebugMessage("Begin Creating Calendar Appointment");
            string schLocation = "See Description";
            string subjectPrefix = "Telehealth Visit";
            if (email.Subject.Contains("VA Video Connect"))
            {
                subjectPrefix = "VA Video Connect";
                schLocation = "VA Video Connect";
            }
            else if (email.Subject.Contains("VVC SIP Tablet"))
            {
                subjectPrefix = "VVC SIP Tablet";
                schLocation = "VVC SIP Tablet";
            }
            string schSubject = group == true ? $"{subjectPrefix}- Group Appointment" :
                $"{subjectPrefix}-Single Appointment";
            string schDescription = email.Description;
            if (string.IsNullOrWhiteSpace(plainDescription))
                plainDescription = schDescription;
            System.DateTime schBeginDate = (System.DateTime)sa.ScheduledStart;
            System.DateTime schEndDate = (System.DateTime)sa.ScheduledEnd;
            string sequence = "";
            string status = "CONFIRMED";
            string method = "";
            //if the appointment is canceled, send a cancellation notice based on the UID of the previous entry sent
            if (statusCode == (int)serviceappointment_statuscode.PatientCanceled || statusCode == (int)serviceappointment_statuscode.ClinicCanceled)
            {
                method = "METHOD:CANCEL\n";
                sequence = "SEQUENCE:1\n";
                status = "CANCELLED";
                schSubject = $"Canceled: {subjectPrefix} : Do Not Reply";
            }

            //attach a ClearSteth CVL file if a steth is in the components
            Logger.WriteDebugMessage("Begin cvlAttachment: stethIP value = " + stethIP);
            string cvlAtttachment = string.IsNullOrEmpty(stethIP) ? "" :
                "ATTACH;ENCODING=BASE64;VALUE=BINARY;X-FILENAME=invitation.cvl:" + Convert.ToBase64String(new ASCIIEncoding().GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?><CVL><IP>" + stethIP + "</IP ><Port>9005</Port><ConferenceId>12345</ConferenceId></CVL>")) + "\n";
            Logger.WriteDebugMessage("CVL attachment conversion finished.");
            try
            {
                // Extract the href link and embed it in the main content
                MatchCollection matches = Regex.Matches(plainDescription, @"<a href=.+?>.+?</a>");

                foreach (Match match in matches)
                {
                    // Finding the link
                    string link = match.Value.Substring("<a href=".Length, match.Value.IndexOf(">") - "<a href=".Length);

                    string firstPortion = match.Value.Substring(0, match.Value.LastIndexOf("</a>"));
                    string replacementContent = String.Concat(firstPortion, " (", link, ")</a>");

                    // Updating the actual content with the replacement content
                    plainDescription = plainDescription.Replace(match.Value, replacementContent);
                }

                //some of the HTML tags have to be replaced to format the text content
                plainDescription = plainDescription.Replace("<br/>", "\\n").Replace("<br />", "\\n").Replace("<b>", "").Replace("</b>", "").Replace("<u>", "").Replace("</u>", "").Replace("<i>", "").Replace("</i>", "").Replace("<li>", "          o     ").Replace("</li>", "\\n").Replace("<ul>", "").Replace("</ul>", "\\n");

                string htmlTagRegex = @"(<(.|\n)+?>)"; //"<.+?>"
                //remaining HTML tags (assumption is that any content between < and > is HTML tag) have to be just removed all together (replaced with blank).
                plainDescription = System.Text.RegularExpressions.Regex.Replace(plainDescription, htmlTagRegex, "").Trim();

                string att = "BEGIN:VCALENDAR\n" +
                                  "PRODID:-//VA//Veterans Affairs//EN\n" +
                                  method +
                                  "BEGIN:VEVENT\n" +
                                  cvlAtttachment +
                                  "UID:" + sa.Id + "\n" + sequence +
                                  "DTSTART:" + schBeginDate.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + "\n" +
                                  "DTEND:" + schEndDate.ToUniversalTime().ToString("yyyyMMdd\\THHmmss\\Z") + "\n" +
                                  "LOCATION:" + schLocation +
                                  //Use Description tag for email clients that cant handle x-alt-desc tag with HTML
                                  "\nDESCRIPTION:" + plainDescription +
                                  "\nSUMMARY:" + schSubject + "\nPRIORITY:3\n" +
                                  "STATUS:" + status + "\n" +
                                  //Include alternate description if the calendar client can handle html x-alt-desc tag
                                  "X-ALT-DESC;FMTTYPE=text/html:<html>" + schDescription.Replace("\n", "<br/>") + "</html>" + "\n" +
                              "END:VEVENT\n" + "END:VCALENDAR\n";

                ActivityMimeAttachment calendarAttachment = new ActivityMimeAttachment()
                {
                    ObjectId = new EntityReference(Email.EntityLogicalName, email.Id),
                    ObjectTypeCode = Email.EntityLogicalName,
                    Subject = $"{subjectPrefix} Appointment",
                    Body = Convert.ToBase64String(
                            new ASCIIEncoding().GetBytes(att)),
                    FileName = string.Format(CultureInfo.CurrentCulture, $"{subjectPrefix}-Appointment.ics")
                };
                Logger.WriteDebugMessage("About to Create Calendar Appointment");
                OrganizationService.Create(calendarAttachment);
                Logger.WriteDebugMessage("Finished Creating Calendar Appointment");
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"Failed to Create CVL Attachment. Error: {ex.InnerException.Message}");
            }
            return;
        }

        #endregion

        #region Commonly Used Functions
        //TODO TO-DO: Consolidate with Email Automation function, should we add check for User's Email existing before adding to the AP List?
        /// <summary>
        /// Gets the list of team members and returns them as an activity party list to be added to whatever email we are using
        /// </summary>
        /// <param name="email"></param>
        /// <param name="TeamId"></param>
        /// <param name="originalParty"></param>
        /// <returns></returns>
        internal static List<ActivityParty> RetrieveFacilityTeamMembers(Email email, Guid TeamId, IEnumerable<ActivityParty> originalParty, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.WriteDebugMessage("starting RetrieveFacilityTeamMembers");
            using (var srv = new Xrm(OrganizationService))
            {
                var teamMembers = srv.TeamMembershipSet.Where(t => t.TeamId == TeamId).ToList();
                var recipientList = new List<ActivityParty>();

                if (originalParty != null)
                    recipientList.AddRange(originalParty);

                Logger.WriteDebugMessage("About to add members of team.");
                foreach (var member in teamMembers)
                {
                    var party = new ActivityParty()
                    {
                        ActivityId = new EntityReference(email.LogicalName, email.Id),
                        PartyId = new EntityReference(SystemUser.EntityLogicalName, member.SystemUserId.Value)
                    };
                    recipientList.Add(party);
                }
                Logger.WriteDebugMessage("Finished adding members of team.");
                return recipientList;
            }
        }
        #endregion
    }
}