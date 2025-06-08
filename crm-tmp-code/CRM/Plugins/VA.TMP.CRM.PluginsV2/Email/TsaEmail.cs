using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class TsaEmail
    {
        #region Constructor/Data Model for this type of email
        IOrganizationService OrganizationService;
        MCSLogger Logger;
        Email Email;

        public TsaEmail(IOrganizationService organizationService, MCSLogger logger, Email email)
        {
            OrganizationService = organizationService;
            Logger = logger;
            Email = email;
        }
        #endregion

        #region Entry point (Execute Method) for this class
        public void Execute()
        {
            Logger.WriteDebugMessage("Attempted to Send TSA Email using deprecated class 'TSAEmail.cs'");
            //SendTSAEmail(Email.RegardingObjectId.Id);
        }
        #endregion

        #region Populate and send the email

        //Send appropriate email based on subject line (denial, under revision, reminder to take action, waiting for approval)
        //commented - we are no longer using TSA's (Replaced with SP's)
        //private void SendTSAEmail(Guid tsaID)
        //{
        //    var customMessage = string.Empty;
        //    switch (Email.Subject)
        //    {
        //        case "A Telehealth Service Agreement has been denied": //Denial
        //            Email.Description = CvtHelper.GenerateEmailBody(tsaID, "cvt_resourcepackage", "The following Telehealth Service Agreement has been Denied.  Please review the notes to see the Denial Reason and correct any mistakes if applicable.", OrganizationService, "Click Here to view this TSA");
        //            break;
        //        case "TSA under revision": //Revision
        //            customMessage = "The following Telehealth Service Agreement is Under Revision.";
        //            break;
        //        case "Please Take Action on the following TSA": //Reminder
        //            customMessage = "This is a reminder that the following Telehealth Service Agreement is waiting for you to take action.";
        //            break;
        //        case "FYI: A Telehealth Service Agreement has been completed": //Production Notification
        //            Email.Description = TSANotificationText();
        //            break;
        //        case "A TSA to your Facility has been created": //Notify patient site that TSA was created
        //            Email.Description = CvtHelper.GenerateEmailBody(tsaID, "cvt_resourcepackage", "Please coordinate with the Provider Site FTC to set up the following TSA.  Once all the details are finalized, it is the responsibility of the Patient Site FTC to begin the Signature collection process.", OrganizationService, "Click Here to view this TSA");
        //            break;
        //        default:
        //            if (Email.Subject.Contains("Telehealth Service Agreement is awaiting your approval")) //Approval
        //                Email.Description = ApprovalEmailBody();
        //            else
        //            {
        //                Logger.WriteToFile("Unable to match email subject to valid TSA email type, exiting plugin");
        //                return;
        //            }
        //            break;
        //    }
            ////Get Team Members will query the Team Members table and return an Activity Party List of the people listed on the team specified
            ////If cant find team members, log the error and continue attempting to populate the message description
        //    try
        //    {
        //        Email.To = GetTeamMembers(tsaID);
        //        Logger.WriteDebugMessage("Populated Email Recipients");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //    }
        //    if (Email.Description == null)
        //        Email.Description = CvtHelper.GenerateEmailBody(tsaID, "cvt_resourcepackage", customMessage, OrganizationService, "Click Here to approve/deny this TSA");
        //    //Get the owner of the workflow for the From field
        //    if (Email.From.Count() == 0)
        //        Email.From = CvtHelper.GetWorkflowOwner("TSA Approval Step 1 - Awaiting Prov FTC", OrganizationService);
        //    Logger.WriteDebugMessage("Sending TSA Email");
        //    CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
        //    Logger.WriteDebugMessage("TSA Email Sent");
        //}

        #endregion

        #region TSA Email Helpers
        /// <summary>
        /// Returns a string value representing the body of the email for TSA approval notification
        /// </summary>
        /// <param name="email">the object representing the email which is being sent</param>
        /// <param name="record">the Guid of the TSA which is causing this notification to be sent</param>
        /// <param name="entityStringName">the entity logical name of the tsa (i.e. "mcs_services")</param>
        /// <returns></returns>
        //private string ApprovalEmailBody()
        //{
        //    var approver = String.Empty;
        //    var nextTeam = String.Empty;
        //    var FTC = String.Empty;
        //    var patFacility = String.Empty;
        //    //Get the Previous approvers by querying most recent note
        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        var TSANote = srv.AnnotationSet.Where(n => n.ObjectId.Id == Email.RegardingObjectId.Id).OrderByDescending(n => n.CreatedOn).First(n => n.NoteText.Contains("Approved by"));
        //        //most recent approver
        //        approver = TSANote.CreatedBy.Name;
        //        var tsa = srv.cvt_resourcepackageSet.FirstOrDefault(t => t.Id == Email.RegardingObjectId.Id);

        //        //have to get the patient facility

        //        patFacility = tsa.cvt_PatientFacility == null ? String.Empty : " To " + tsa.cvt_PatientFacility.Name;
        //        if (tsa.cvt_ServiceScope.Value == 917290001)
        //            patFacility = " (Intrafacility)";
        //        //Get the next approver up and get the FTC who created the TSA (assumed to be provider side) and the FTC who first approved the TSA (assumed to be patient side)
        //        //removing -- WMC 11/15/2018
        //        //status codes no longer valid
        //        //switch (tsa.statuscode.Value)
        //        //{
        //        //    case (int)mcs_services_statuscode.ApprovedbyPatFTC:
        //        //        nextTeam = "Provider FTC Team";
        //        //        goto case 0;
        //        //    case (int)mcs_services_statuscode.ApprovedbyProvFTC:
        //        //        nextTeam = "Provider Service Chief Team";
        //        //        goto case 0;
        //        //    case (int)mcs_services_statuscode.ApprovedbyProvServiceChief:
        //        //        nextTeam = "Provider Chief of Staff Team";
        //        //        goto case 0;
        //        //    case (int)mcs_services_statuscode.ApprovedbyProvChiefofStaff:
        //        //        nextTeam = "Patient Service Chief Team";
        //        //        goto case -1;
        //        //    case (int)mcs_services_statuscode.ApprovedbyPatServiceChief:
        //        //        nextTeam = "Patient Chief of Staff Team";
        //        //        goto case -1;
        //        //    case 0: //if Provider side - get the user who created the TSA - assumed to be the Provider FTC
        //        //        FTC = srv.SystemUserSet.FirstOrDefault(u => u.Id == tsa.CreatedBy.Id).FullName;
        //        //        break;
        //        //    case -1: //If patient side - get user who first approved the TSA - assumed to be the Patient FTC
        //        //        var firstApprover = srv.AnnotationSet.Where(n => n.ObjectId.Id == Email.RegardingObjectId.Id).OrderBy(n => n.CreatedOn).First(n => n.NoteText.Contains("Approved by"));
        //        //        FTC = firstApprover.CreatedBy.Name;
        //        //        break;
        //        //}
        //    }

        //    //TODO: Add patient facility, change spacing
        //    //get the FTC for whichever side the TSA is awaiting approval
        //    string hyperlink = CvtHelper.GetRecordLink(Email.RegardingObjectId, OrganizationService);
        //    string OpsManual = "http://vaww.infoshare.va.gov/sites/telehealth/docs/tmp-user-tsa-appr.docx";
        //    string RollOut = "http://vaww.telehealth.va.gov/quality/tmp/index.asp";
        //    string emailBody = String.Format("A Telehealth Service Agreement (TSA), {0} is awaiting your approval. <br/><ul><li>Previous Approver: {1}</li>" +
        //        "<li>{2} is the next in line for the TSA Approval Process. </ul>The hyperlink below will take you to the Telehealth Service Agreement.  If you wish to make changes to the TSA prior to approval, please contact {3}.  If you choose to approve the TSA, please select the Green Button on the top left corner.  If you choose to decline approval, please select the Red Button on the top left corner.<br/><br/><b>Click here to take action on the TSA</b>: {4} <br/><br/>", Email.RegardingObjectId.Name + patFacility, approver, nextTeam, FTC, hyperlink);
        //    string loginNotes = String.Format("Note: A password is not required to access TMP.  Your credentials are passed from Windows authentication used to log on to your computer.  Simply click the link above.  For first time access, or access after a long period of time, you may be prompted to choose \"VA Accounts\" on a pop-up form.  After that, clicking the link will take you directly to the TSA.  <br/><br/>To see a brief tutorial for approvers, click this link: {0} <br/><br/>To access all resources (training materials, operations manual, etc.) for TMP users, click this link: {1}", "<a href=\"" + OpsManual + "\">" + OpsManual + "</a>", "<a href=\"" + RollOut + "\">" + RollOut + "</a>");

        //    return emailBody + loginNotes;
        //}

        /// <summary>
        /// Returns the text string for the TSA Approval Notification Email
        /// </summary>
        /// <returns></returns>
        private string TSANotificationText()
        {
            var tsaUrl = CvtHelper.GetRecordLink(Email.RegardingObjectId, OrganizationService);
            return String.Format("For your information, a Telehealth Service Agreement (TSA), {0} has been approved.  <br/>The hyperlink below will take you to the Telehealth Service Agreement. \n\nClick here to view the TSA: {1} <br /><br />Note: A password is not required to access TMP. Your credentials are passed from Windows authentication used to log on to your computer. Simply click the link above. For first time access, or access after a long period of time, you may be prompted to choose \"VA Accounts\" on a pop-up form.  After that, clicking the link will take you directly to the TSA.  <br /><br />To access all resources (training materials, operations manual, etc.) for TMP users, click this link: {2}", Email.RegardingObjectId.Name, tsaUrl, String.Format("<a href=\"{0}\">{0}</a>", "http://vaww.telehealth.va.gov/quality/tmp/index.asp"));
        }

        /// <summary>
        /// Query the team membership table to get the team appropriate for the status of the TSA.  return the list of activity parties corresponding to the system users that are members of the team.  
        /// </summary>
        /// <param name="email">This is the email record that is being built and eventually gets sent out</param>
        /// <param name="tsaID">This is the ID of the TSA that generated this email.  Based on the status of the TSA, a specific team will be selected</param>
        /// <returns>Activity Party List corresponding to System users that are members of the team</returns>
        private List<ActivityParty> GetTeamMembers(Guid tsaID)
        {
            //Status Listing: 917290002==Approved by Pat FTC, 917290000==Prov FTC, 917290001==Prov SC, 917290004==Prov CoS, 917290005==Pat SC, 917290006==Pending Privileging, 251920000==PROD, 917290003==DENIED, 917290007==UNDER REVISION; 917290008==Approved by Prov C&P
            var members = new List<ActivityParty>();
            var teamMembers = new List<TeamMembership>();
            using (var srv = new Xrm(OrganizationService))
            {
                //Get both the Patient and Provider Facility to check for the teams on both sides
                var tsa = srv.mcs_servicesSet.First(t => t.Id == tsaID);
                var proFacilityId = tsa.cvt_ProviderFacility.Id;
                var patFacilityId = tsa.cvt_PatientFacility != null ? tsa.cvt_PatientFacility.Id : Guid.Empty;
                var team = new Team();
                switch (tsa.statuscode.Value)
                {
                    case (int)mcs_services_statuscode.Draft: //For Draft TSAs, send notification that TSA has been created for their site. 
                        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);
                        break;
                    case (int)mcs_services_statuscode.ApprovedbyPatFTC://Approved by Patient Site FTC (get Provider Site FTC Team) - Workflow Step 1
                        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);
                        break;
                    case (int)mcs_services_statuscode.ApprovedbyProvFTC://Approved by Provider Site FTC (get Provider Service Chief Team) - Workflow Step 2
                        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId &&
                            t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == tsa.cvt_servicetype.Id);
                        break;
                    case (int)mcs_services_statuscode.ApprovedbyProvServiceChief://Approved by Provider Service Chief (get Prov Chief of Staff Team) - Workflow Step 3
                        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff);
                        break;
                    case (int)mcs_services_statuscode.ApprovedbyProvChiefofStaff://Approved by Provider Site Chief of Staff (Get Patient Site Service Chief) - Workflow Step 5
                        if (patFacilityId != Guid.Empty)
                            team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId &&
                                t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == tsa.cvt_servicetype.Id);
                        break;
                    case (int)mcs_services_statuscode.ApprovedbyPatServiceChief://Approved by Patient Site Service Chief (Get Patient Site Chief of Staff) - Workflow Step 6
                        if (patFacilityId != Guid.Empty)
                            team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff);
                        break;
                    case (int)mcs_services_statuscode.UnderRevision://Get both side FTCs whether it is in Denied status or in Under Revision
                    case (int)mcs_services_statuscode.Denied:
                        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);
                        if (team != null)
                            teamMembers = (List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList());

                        //repurpose team variable to get patient facility (prov facility team members have already been added above) and add team members from pat facility
                        if (patFacilityId != Guid.Empty)
                        {
                            team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);
                            if (team != null)
                            {
                                if (teamMembers.Count == 0)
                                    teamMembers = (List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList());
                                else
                                    teamMembers.AddRange((List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList()));
                            }
                        }
                        break;
                    case (int)mcs_services_statuscode.Production: //PROD - Get Both sides notification team for TSA Notification email
                        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification);
                        if (team != null)
                            teamMembers = (List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList());

                        //repurpose team variable to get patient facility (prov facility team members have already been added above) and add team members from pat facility (if not intrafacility)
                        if (patFacilityId != Guid.Empty && patFacilityId != proFacilityId)
                        {
                            team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification);
                            if (team != null)
                            {
                                if (teamMembers.Count == 0)
                                    teamMembers = (List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList());
                                else
                                    teamMembers.AddRange((List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList()));
                            }
                        }
                        break;
                }
                if (team == null)
                    throw new InvalidPluginExecutionException("No Team was found to receive this email, please verify the team is set up");
                if (teamMembers.Count == 0) //if you havent already added the team members (everthing other than prod notification, under revision and denial) then add now
                    teamMembers = (List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList());
                foreach (var member in teamMembers)
                {
                    var party = new ActivityParty()
                    {
                        ActivityId = new EntityReference(Email.LogicalName, Email.Id),
                        PartyId = new EntityReference(SystemUser.EntityLogicalName, member.SystemUserId.Value)
                    };
                    members.Add(party);
                }
            }
            if (members.Count == 0)
                members.AddRange(Email.To);
            return members;
        }

        #endregion
    }
}
