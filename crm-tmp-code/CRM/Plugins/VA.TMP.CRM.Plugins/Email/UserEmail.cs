using MCSShared;
using MCSUtilities2011;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class UserEmail
    {
        #region Constructor/Data Model for Privileging Email Class
        IOrganizationService OrganizationService;
        MCSLogger Logger;
        Email Email;
        string CustomMessage;

        public UserEmail(IOrganizationService organizationService, MCSLogger logger, Email email)
        {
            OrganizationService = organizationService;
            Logger = logger;
            Email = email;
        }
        #endregion 

        #region Entry Point (Execute Method) - Determine which email to send
        public void Execute()
        {
            //if (Email.Subject.IndexOf("Action Required: OPPE/FPPE Feedback") != -1)
            if (Email.Subject == "Action Required: Update Your Provider Preferences in TMP")
            {
                Logger.WriteDebugMessage("Provider Preferences Email needs to be sent.");
                SendProviderPreferencesEmail(Email);
                Logger.WriteDebugMessage("Provider Preferences Email sent.");
            }
        }
        #endregion

        internal void SendProviderPreferencesEmail(Email email)
        {
            Logger.WriteDebugMessage("Starting");
            if (Email.To != null)
            {
                Guid sysUser = Guid.Empty;
                foreach (ActivityParty to in email.To)
                {
                    if (to.PartyId.Id != null)
                        sysUser = to.PartyId.Id;
                }

                if (sysUser != Guid.Empty)
                {
                    //Generate body and then send
                    CustomMessage = "Thank you for participating in the Telehealth community.<br/><br/>As part of your telehealth provider role, please review and edit your preferences prior to administering your next remote visit. Investing a few minutes to communicate your expectations and requirements will help your remote team better support your telehealth visits.<br/><br/>" + CvtHelper.GetRecordLink(new EntityReference(SystemUser.EntityLogicalName, sysUser), OrganizationService, "Click here and fill in your specifications in the text box marked Provider Preferences.");
                    CustomMessage += "<br/>" + CvtHelper.TechnicalAssistanceEmailFooter(out var plainTextFooter);

                    Email.Description = CustomMessage;

                    //Get the owner of the workflow for the From field
                    Email.From = CvtHelper.GetWorkflowOwner("Privileging: PPE Submitted", OrganizationService);

                    CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
                }
                else
                {
                    Logger.WriteDebugMessage("TO list is empty.  Could not fill out and send email.");
                }
            }
            else
            {
                Logger.WriteDebugMessage("TO list is empty.  Could not fill out and send email.");
            }
        }
    }
}
