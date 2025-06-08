using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace VA.TMP.CRM.Guest_Email
{
    public class GuestEmailCreatePostStageRunner : PluginRunner
    {
        public GuestEmailCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        //Declare global variables
        bool invalid = false;
        bool validemail = false;
        EntityReference sa = null;
        EntityReference pat = null;
        string initials = string.Empty;
        string fName = string.Empty;
        string lName = string.Empty;

        #region Implementation

        public override void Execute()
        {
            var thisRecord = PrimaryEntity.ToEntity<cvt_nonvaemail>();
            SendEmail(thisRecord);
        }
        internal void SendEmail(cvt_nonvaemail thisMessage)
        {

            try
            {
                var emailaddress = PrimaryEntity.Attributes["cvt_email"].ToString();
                if ((emailaddress != null) && (emailaddress != ""))  //<--the email address has something inside
                {
                    //check that the email is a valid one
                    validemail = IsValidEmail(emailaddress);
                    if (validemail) // do stuff
                    {
                        //get the parent ServiceAppointment from the record. 
                        sa = (EntityReference)PrimaryEntity.Attributes["cvt_serviceappointment"];
                        //get the patient from the record.
                        pat = (EntityReference)PrimaryEntity.Attributes["cvt_patient"];

                        // get the veteran's initials
                        using (var service = new Xrm(OrganizationService))
                        {
                            var guestEmailRecord = service.cvt_nonvaemailSet.FirstOrDefault(em => em.Id == thisMessage.Id);
                            var veteran = service.ContactSet.FirstOrDefault(i => i.ContactId == pat.Id);
                            if (veteran != null)
                            {
                                if (veteran.FirstName != null)
                                {
                                    fName = veteran.FirstName.ToString();
                                    initials += fName.Substring(0, 1) + ". ";
                                }
                                if (veteran.LastName != null)
                                {
                                    lName = veteran.LastName.ToString();
                                    initials += lName.Substring(0, 1) + ". ";
                                }
                            }

                            //create the email message
                            Email gstEmail = new Email()
                            {
                                Subject = "[External] Guest Invitation to Video Health Appointment for a Veteran you know",
                                Description = initials + "," + emailaddress,
                                //ToRecipients = emailaddress,
                                RegardingObjectId = new EntityReference(cvt_nonvaemail.EntityLogicalName, guestEmailRecord.Id),
                                mcs_RelatedServiceActivity = sa
                                //RegardingObjectId = new EntityReference(sa.LogicalName, sa.Id)
                                //RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalRecord.Id)
                            };
                            OrganizationService.Create(gstEmail);
                            
                        }

                    }
                    else // the email address is not formatted properly.
                    {
                        Logger.WriteDebugMessage("Invalid Email address entered (improper format).  Exiting.");
                        return;
                    }

                }
                else //we can't use a record with an empty email address!
                {
                    Logger.WriteDebugMessage("Attempt made to create Guest Email record with no email address.  Exiting.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteDebugMessage(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        #endregion
        #region Email Format Check
        public bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (string.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid email format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
        #endregion

        public override string McsSettingsDebugField
        {
            get { return "cvt_guestemailplugin"; }
        }
    }
}
