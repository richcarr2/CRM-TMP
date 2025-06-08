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
    public class PrivilegingEmail
    {
        #region Constructor/Data Model for Privileging Email Class
        IOrganizationService OrganizationService;
        MCSLogger Logger;
        Email Email;
        string CustomMessage;

        public PrivilegingEmail(IOrganizationService organizationService, MCSLogger logger, Email email)
        {
            OrganizationService = organizationService;
            Logger = logger;
            Email = email;
        }
        #endregion 

        #region Entry Point (Execute Method) - Determine which email to send
        public void Execute()
        {
            switch (Email.RegardingObjectId.LogicalName)
            {
                //Regarding Object: TSS Privileging
                case cvt_tssprivileging.EntityLogicalName:
                    SendPrivilegingEmail(Email.RegardingObjectId.Id, Email.RegardingObjectId.LogicalName);
                    Logger.WriteDebugMessage("Completed Send Privileging Email");
                    break;
                //Regarding Object: PPE Review
                case cvt_ppereview.EntityLogicalName:
                    SendPPEReviewEmail(Email.RegardingObjectId.Id, Email.RegardingObjectId.LogicalName);
                    Logger.WriteDebugMessage("Completed PPE Review Email");
                    break;
                //Regarding Object: PPE Feedback
                case cvt_ppefeedback.EntityLogicalName:
                    SendPPEFeedbackEmail(Email.RegardingObjectId.Id, Email.RegardingObjectId.LogicalName);
                    Logger.WriteDebugMessage("Completed PPE Feedback Email");
                    break;
                //Regarding Object: Provider Site Resource
                case cvt_providerresourcegroup.EntityLogicalName:
                case mcs_groupresource.EntityLogicalName:
                    SendTSAProviderEmail(Email.RegardingObjectId.Id, Email.RegardingObjectId.LogicalName);
                    Logger.WriteDebugMessage("Completed Add Prov to TSA Email");
                    break;
            }

        }
        #endregion

        #region TSS Privilege e-mails
        internal void SendPrivilegingEmail(Guid tssprivilegeId, string recordType)
        {
            Logger.WriteDebugMessage("Starting SendPrivilegingEmail");
            using (var srv = new Xrm(OrganizationService))
            {
                //Get the related TSS Privileging record
                cvt_tssprivileging tssprivileging = (cvt_tssprivileging)OrganizationService.Retrieve(cvt_tssprivileging.EntityLogicalName, tssprivilegeId, new ColumnSet(true));
                if (tssprivileging.cvt_PrivilegedAtId != null) //Always filled
                {
                    #region variables
                    //Notification of Privileging Status Change
                    List<Team> TOTeam = new List<Team>();
                    //Establish parameters to clean up queries
                    List<ActivityParty> recipient = new List<ActivityParty>();
                    List<Team> homeCPTeam = new List<Team>();
                    List<Team> proxyCPTeam = new List<Team>();
                    #endregion

                    #region ifRegarding =home
                    Boolean isRegardingPrivHome = true;
                    cvt_tssprivileging homePrivRecord = tssprivileging;
                    cvt_tssprivileging proxyPrivRecord = new cvt_tssprivileging();

                    #endregion
                    #region ifRegarding =proxy
                    //Regarding is Proxy, overwrite homeProvRecord and isRegardingPrivHome
                    if ((tssprivileging.cvt_TypeofPrivileging != null) && (tssprivileging.cvt_TypeofPrivileging.Value == 917290001) && (tssprivileging.cvt_ReferencedPrivilegeId != null))
                    {
                        isRegardingPrivHome = false;
                        homePrivRecord = (cvt_tssprivileging)OrganizationService.Retrieve(cvt_tssprivileging.EntityLogicalName, tssprivileging.cvt_ReferencedPrivilegeId.Id, new ColumnSet(true));
                        proxyPrivRecord = tssprivileging;
                        proxyCPTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == proxyPrivRecord.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == 917290003).Distinct().ToList();
                    }
                    #endregion

                    //Home CPTeam is always set
                    homeCPTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == homePrivRecord.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == 917290003).Distinct().ToList();

                    #region Privilege Status Change
                    if (Email.Subject.IndexOf("Notification of Privileging Status Change") != -1)
                    {
                        Logger.WriteDebugMessage("Privilege Status Change branch");
                        #region Record is Active
                        //Check if record is inactive or active
                        if (tssprivileging.statecode.Value == cvt_tssprivilegingState.Active)
                        {
                            CustomMessage = String.Format("This is to notify all affected Facilities that this provider is now privileged at {0}.  If this provider possessed proxy privileges for telemedicine purposes at your facility, those privileges may be reinstated.<br/><br/>This provider may now be included in Telehealth Service Agreements and scheduling for this provider may commence.", tssprivileging.cvt_PrivilegedAtId.Name);
                            CustomMessage += "<br/><br/>Please get the new privileging documents from the home facility.";

                            //TO FTC, Service Chief and C&P Teams (Proxy Privileging Facilities)
                            //Loop through each Proxy Privilege
                            var proxys = srv.cvt_tssprivilegingSet.Where(p => p.cvt_ReferencedPrivilegeId.Id == tssprivileging.Id);

                            foreach (cvt_tssprivileging proxy in proxys)
                            {
                                List<Team> FTCTeam = new List<Team>();
                                List<Team> SCTeam = new List<Team>();
                                List<Team> CPTeam = new List<Team>();
                                FTCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == proxy.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == (int)Teamcvt_Type.FTC).Distinct().ToList();
                                SCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == proxy.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && p.cvt_ServiceType.Id == proxy.cvt_ServiceTypeId.Id).Distinct().ToList();
                                CPTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == proxy.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == (int)Teamcvt_Type.CredentialingandPrivileging).Distinct().ToList();

                                //Loop the results into the TO field
                                foreach (var result in FTCTeam)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }
                                foreach (var result in SCTeam)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }
                                foreach (var result in CPTeam)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }
                            }


                            //Enable User Record.
                            SetStateRequest requestEnable = new SetStateRequest()
                            {
                                EntityMoniker = new EntityReference(SystemUser.EntityLogicalName, tssprivileging.cvt_ProviderId.Id),
                                State = new OptionSetValue(0),//1=disabled, 0=enabled
                                Status = new OptionSetValue(-1)
                            };

                            OrganizationService.Execute(requestEnable);
                            //Discuss: Automatically reactivate TSS Privileging for Proxy?

                        }
                        #endregion
                        #region Record is Deactivated
                        else //Deactivate
                        {
                            CustomMessage = String.Format("This is to notify all affected Facilities that this provider is no longer privileged at {0}.If this provider possessed proxy privileges for telemedicine purposes at your facility, they are no longer in effect.<br/><br/>This provider will need to be replaced on any existing Service Agreements or new Service Agreements will need to be composed for a new provider.<br/><br/>Any Service Activities that have been scheduled for this provider will need to be rescheduled with another.", tssprivileging.cvt_PrivilegedAtId.Name);

                            //TO FTC, Service Chief and C&P Teams
                            List<Team> FTCTeam = new List<Team>();
                            List<Team> SCTeam = new List<Team>();
                            List<Team> CPTeam = new List<Team>();
                            FTCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == tssprivileging.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == (int)Teamcvt_Type.FTC).Distinct().ToList();
                            SCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == tssprivileging.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && p.cvt_ServiceType.Id == tssprivileging.cvt_ServiceTypeId.Id).Distinct().ToList();
                            CPTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == tssprivileging.cvt_PrivilegedAtId.Id && p.cvt_Type.Value == (int)Teamcvt_Type.CredentialingandPrivileging).Distinct().ToList();

                            //Loop the results into the TO field
                            foreach (var result in FTCTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                            }
                            foreach (var result in SCTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                            }
                            foreach (var result in CPTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                            }

                            //Disable User Record.
                            //Remvoves the value from the field
                            SystemUser provUpdate = new SystemUser()
                            {
                                Id = tssprivileging.cvt_ProviderId.Id,
                                cvt_disable = null
                            };

                            //Disable the provider's user record here
                            SetStateRequest requestDisable = new SetStateRequest()
                            {
                                EntityMoniker = new EntityReference(SystemUser.EntityLogicalName, tssprivileging.cvt_ProviderId.Id),
                                State = new OptionSetValue(1),
                                Status = new OptionSetValue(-1)
                            };

                            OrganizationService.Update(provUpdate);
                            OrganizationService.Execute(requestDisable);
                            //Automatically disable Proxy TSS Privileging records
                            var proxys = srv.cvt_tssprivilegingSet.Where(p => p.cvt_ReferencedPrivilegeId.Id == tssprivileging.Id);

                            foreach (cvt_tssprivileging proxy in proxys)
                            {
                                SetStateRequest disableProxy = new SetStateRequest()
                                {
                                    EntityMoniker = new EntityReference(cvt_tssprivileging.EntityLogicalName, proxy.Id),
                                    State = new OptionSetValue(1),
                                    Status = new OptionSetValue(-1)
                                };
                                OrganizationService.Execute(disableProxy);
                            }
                            Logger.WriteDebugMessage("Disabled all Proxy Privileges.");
                        }
                        #endregion

                    }
                    #endregion
                    #region Initial Privileging
                    //Initial Privileging
                    if (Email.Subject.IndexOf("Telehealth Notification: A Provider is now privileged") != -1)
                    {
                        Logger.WriteDebugMessage("Initial Privileging branch");
                        if (isRegardingPrivHome) //Home
                        {
                            foreach (var cp in homeCPTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, cp.Id, Email.To, OrganizationService, Logger);
                            }
                            //Add FTC and SC team
                            addFacilityTeamstoEmail(homePrivRecord.cvt_PrivilegedAtId.Id, homePrivRecord.cvt_ServiceTypeId.Id);
                            CustomMessage = "A Home Privilege has been granted at Facility: " + homePrivRecord.cvt_PrivilegedAtId.Name;
                        }
                        else //Proxy
                        {
                            foreach (var cp in proxyCPTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, cp.Id, Email.To, OrganizationService, Logger);
                            }
                            //Add FTC and SC team
                            addFacilityTeamstoEmail(proxyPrivRecord.cvt_PrivilegedAtId.Id, homePrivRecord.cvt_ServiceTypeId.Id);

                            foreach (var cp in homeCPTeam)
                            {
                                Email.Cc = CvtHelper.RetrieveFacilityTeamMembers(Email, cp.Id, Email.To, OrganizationService, Logger);
                            }
                            //Add FTC and SC team
                            addFacilityTeamstoEmail(homePrivRecord.cvt_PrivilegedAtId.Id, homePrivRecord.cvt_ServiceTypeId.Id);

                            CustomMessage = "A Proxy Privilege has been granted at Facility: " + proxyPrivRecord.cvt_PrivilegedAtId.Name;
                            CustomMessage += "<br/>Home Privilege: The provider's HOME privileging is at Facility: " + homePrivRecord.cvt_PrivilegedAtId.Name;
                            CustomMessage += "<br/>Reminder: Please enter the provider into your local PPE process.";
                        }
                    }
                    #endregion
                    #region Renewal
                    //Check if E-mail subject = "Renewal"
                    else if (Email.Subject.IndexOf("Telehealth Notification: Upcoming Renewal for a Provider") != -1)
                    {
                        Logger.WriteDebugMessage("Renewal branch");
                        if (isRegardingPrivHome == true)
                        {
                            //Update Home/Primary TSS Privilege //If Status Reason = Privileged; set to In Renewal
                            if (homePrivRecord.statuscode.Value == 917290001)
                            {
                                //Declare new object
                                cvt_tssprivileging homeRecord = new cvt_tssprivileging()
                                {
                                    Id = homePrivRecord.Id,
                                    statuscode = new OptionSetValue(917290002)
                                };
                                OrganizationService.Update(homeRecord);
                            }
                            foreach (var cp in homeCPTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, cp.Id, Email.To, OrganizationService, Logger);
                            }

                            var homeSC = srv.TeamSet.Where(t => t.cvt_Facility.Id == homePrivRecord.cvt_PrivilegedAtId.Id && t.cvt_ServiceType.Id == homePrivRecord.cvt_ServiceTypeId.Id && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief);
                            foreach (var sc in homeSC)
                            {
                                Email.Cc = CvtHelper.RetrieveFacilityTeamMembers(Email, sc.Id, Email.To, OrganizationService, Logger);
                            }

                            //Edit the E-mail body
                            CustomMessage = homePrivRecord.cvt_ProviderId.Name + "'s Home Privilege is up for renewal at Facility: " + homePrivRecord.cvt_PrivilegedAtId.Name;
                            CustomMessage += "<br/>The privileges are due to expire on " + homePrivRecord.cvt_ExpirationDate + ".";
                            CustomMessage += "<br/><br/>Note: Home Privilege has been set to 'In Renewal' status.";
                        }
                    }
                    #endregion
                    #region Suspended
                    //Else if Suspended
                    else if (Email.Subject.IndexOf("Telehealth Notification: A Provider's Privileging has been Suspended") != -1)
                    {
                        Logger.WriteDebugMessage("Privilege Suspended branch");
                        if (isRegardingPrivHome == true)
                        {
                            foreach (var cp in homeCPTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, cp.Id, Email.To, OrganizationService, Logger);
                            }
                            //Update the provider's record
                            SystemUser provider = (SystemUser)OrganizationService.Retrieve(SystemUser.EntityLogicalName, tssprivileging.cvt_ProviderId.Id, new ColumnSet(true));
                            provider.cvt_disable = true;
                            OrganizationService.Update(provider);

                            //Edit the E-mail body
                            CustomMessage = "A provider's HOME privileging has been suspended at Facility: " + homePrivRecord.cvt_PrivilegedAtId.Name;
                            CustomMessage += "<br/>Note: THE PROVIDER'S USER RECORD HAS BEEN DISABLED.  This Provider can no longer be scheduled in the system.";
                            CustomMessage += "<br/>Suspension: The suspension occurred at Facility: " + homePrivRecord.cvt_PrivilegedAtId.Name;
                        }
                        else
                        {
                            foreach (var cp in homeCPTeam)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, cp.Id, Email.To, OrganizationService, Logger);
                            }
                            foreach (var cp in proxyCPTeam)
                            {
                                Email.Cc = CvtHelper.RetrieveFacilityTeamMembers(Email, cp.Id, Email.Cc, OrganizationService, Logger);
                            }

                            //Edit the E-mail body
                            CustomMessage = "A provider's PROXY privileging has been suspended at Facility: " + proxyPrivRecord.cvt_PrivilegedAtId.Name;
                            CustomMessage += "<br/>Note: This Provider is still schedulable in the system.";
                            CustomMessage += "<br/>Suspension: The suspension occurred at Facility:" + proxyPrivRecord.cvt_PrivilegedAtId.Name;
                            CustomMessage += "<br/>Home Privilege: The provider's HOME privileging is at Facility: " + homePrivRecord.cvt_PrivilegedAtId.Name;
                        }
                    }
                    #endregion

                    //Generate body and then send
                    CustomMessage += "<br/>Reminder: Notify all pertinent C&P Officers and Service Chiefs.";
                    Email.Description = CvtHelper.GenerateEmailBody(tssprivilegeId, "cvt_tssprivileging", CustomMessage, OrganizationService, "Please click this link to view the Privileging record.");

                    //Get the owner of the workflow for the From field
                    Email.From = CvtHelper.GetWorkflowOwner("Privileging: PPE Submitted", OrganizationService);

                    if (Email.To != null)
                    {
                        CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
                    }
                }
            }
        }

        internal void addFacilityTeamstoEmail(Guid facility, Guid specialtyId)
        {
            Logger.WriteDebugMessage("Starting addFacilityTeamstoEmail");
            if (facility != null)
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    //Provider
                    List<Team> FTCTeam = new List<Team>();
                    List<Team> SCTeam = new List<Team>();
                    FTCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == facility && p.cvt_Type.Value == (int)Teamcvt_Type.FTC).Distinct().ToList();
                    SCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == facility && p.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && p.cvt_ServiceType.Id == specialtyId).Distinct().ToList();

                    //Loop the results into the TO field
                    foreach (var result in FTCTeam)
                    {
                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                    }
                    Logger.WriteDebugMessage("Added FTC Team members to the TO.");
                    foreach (var result in SCTeam)
                    {
                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                    }
                    Logger.WriteDebugMessage("Added SC Team members to the TO.");
                }
            }
        }
        #endregion 

        #region PPE Review/Feedback
        internal void SendPPEReviewEmail(Guid ppeId, string recordType)
        {
            Logger.setMethod = "SendPPEReviewEmail";
            Logger.WriteDebugMessage("Starting SendPPEReviewEmail");
            using (var srv = new Xrm(OrganizationService))
            {
                //Check system generated e-mail
                if (Email.Subject.IndexOf("PPE feedback tracking") != -1)
                {
                    //Get the owner of the workflow for the From field
                    Email.From = CvtHelper.GetWorkflowOwner("Privileging: PPE Submitted", OrganizationService);

                    Logger.WriteDebugMessage("Get the PPE related to the email");
                    cvt_ppereview review = (cvt_ppereview)OrganizationService.Retrieve(cvt_ppereview.EntityLogicalName, ppeId, new ColumnSet(true));

                    //Find the Privilege record associated and navigate to that record
                    if (review.cvt_telehealthprivileging != null)
                    {
                        cvt_tssprivileging ppePriv = (cvt_tssprivileging)OrganizationService.Retrieve(cvt_tssprivileging.EntityLogicalName, review.cvt_telehealthprivileging.Id, new ColumnSet(true));

                        Guid homeServiceType = ppePriv.cvt_ServiceTypeId != null ? ppePriv.cvt_ServiceTypeId.Id : Guid.Empty;
                        List<Team> homeSCTeams = new List<Team>();

                        //Add Service Chief Team - should only ever be one
                        homeSCTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == ppePriv.cvt_PrivilegedAtId.Id && t.cvt_Type.Value == 917290001 && t.cvt_ServiceType.Id == homeServiceType).Distinct().ToList();

                        foreach (var result in homeSCTeams)
                        {
                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                        }

                        //Completed Feedback portion
                        if (Email.Subject.IndexOf("Completed") != -1)
                        {
                            CustomMessage = String.Format("<br/><br/>PPE Feedback collection that was initiated for {0} was completed.<br/><br/>Please set the next PPE Review Date on the Telehealth Privileging record. Link above.", ppePriv.cvt_ProviderId.Name);
                            Email.Description = CvtHelper.GenerateEmailBody(ppePriv.Id, ppePriv.LogicalName, CustomMessage, OrganizationService, "Please click this link to view the Telehealth Privileging record.");
                            if (Email.To != null)
                                CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
                            return;
                        }
                    }
                }
            }
        }

        internal void SendPPEFeedbackEmail(Guid feedbackId, string recordType)
        {
            Logger.setMethod = "SendPPEFeedbackEmail";
            Logger.WriteDebugMessage("Starting SendPPEFeedbackEmail");
            using (var srv = new Xrm(OrganizationService))
            {
                //Check system generated e-mail
                if (Email.Subject.IndexOf("Action Required: OPPE/FPPE Feedback") != -1)
                {
                    //Get the owner of the workflow for the From field
                    Email.From = CvtHelper.GetWorkflowOwner("Privileging: PPE Submitted", OrganizationService);

                    Logger.WriteDebugMessage("Get the PPE related to the email");
                    cvt_ppefeedback feedback = (cvt_ppefeedback)OrganizationService.Retrieve(cvt_ppefeedback.EntityLogicalName, feedbackId, new ColumnSet(true));

                    //Get the ppe_review record for the Due Date
                    if (feedback.cvt_ppereview == null)
                    {
                        Logger.WriteDebugMessage("Can't find PPE Review, exiting");
                        return;
                    }
                    cvt_ppereview ppeReview = (cvt_ppereview)OrganizationService.Retrieve(cvt_ppereview.EntityLogicalName, feedback.cvt_ppereview.Id, new ColumnSet(true));
                    var dueDate = (DateTime)ppeReview.cvt_duedate;
                    var initiatedDate = (DateTime)ppeReview.cvt_initiateddate;

                    if (dueDate == null || initiatedDate == null)
                    {
                        Logger.WriteDebugMessage("No due or initiated date found, exiting");
                        return;
                    }
                    var daysRemaining = (dueDate - DateTime.Today);
                    var daysElapsed = (DateTime.Today - initiatedDate);

                    Boolean isEscalation = false;
                    if (Email.Subject.IndexOf("overdue") != -1)
                    {
                        isEscalation = true;
                        Logger.WriteDebugMessage("Overdue, setting isEscalation = true.");
                    }
                    //Find the Privilege record associated and navigate to that record
                    if (feedback.cvt_proxyprivileging != null)
                    {
                        cvt_tssprivileging proxyPriv = (cvt_tssprivileging)OrganizationService.Retrieve(cvt_tssprivileging.EntityLogicalName, feedback.cvt_proxyprivileging.Id, new ColumnSet(true));

                        Guid homeServiceType = proxyPriv.cvt_ServiceTypeId != null ? proxyPriv.cvt_ServiceTypeId.Id : Guid.Empty;
                        List<Team> proxySCTeams = new List<Team>();
                        List<Team> proxyCoSTeams = new List<Team>();
                        var team = "";
                        if (isEscalation)
                        {
                            if (proxyPriv.cvt_PrivilegedAtId == null)
                            {
                                Logger.WriteDebugMessage("Proxy Priv is missing Facility, exiting.");
                                return;
                            }
                            proxyCoSTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == proxyPriv.cvt_PrivilegedAtId.Id && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff).Distinct().ToList();
                            var proxyCOSTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proxyPriv.cvt_PrivilegedAtId.Id && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff);
                            if (proxyCOSTeam != null && feedback.cvt_responseescalated == null)
                            {
                                //Set the response requested field
                                cvt_ppefeedback updateFeedback = new cvt_ppefeedback()
                                {
                                    Id = feedback.Id,
                                    cvt_responseescalated = new EntityReference(Team.EntityLogicalName, proxyCOSTeam.Id)

                                };
                                OrganizationService.Update(updateFeedback);
                                Logger.WriteDebugMessage("Updated the PPE Feedback's Escalation Request Team with " + proxyCOSTeam.Name);
                            }
                        }
                        //Service Chief Team
                        proxySCTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == proxyPriv.cvt_PrivilegedAtId.Id && t.cvt_Type.Value == 917290001 && t.cvt_ServiceType.Id == homeServiceType).Distinct().ToList();

                        //Depending if it is feedback or escalation                      
                        foreach (var result in proxySCTeams)
                        {
                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                        }
                        var proxySCTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proxyPriv.cvt_PrivilegedAtId.Id && t.cvt_Type.Value == 917290001 && t.cvt_ServiceType.Id == homeServiceType);
                        if (proxySCTeam != null && feedback.cvt_responserequested == null)
                        {
                            //Set the response requested field
                            cvt_ppefeedback updateFeedback = new cvt_ppefeedback()
                            {
                                Id = feedback.Id,
                                cvt_responserequested = new EntityReference(Team.EntityLogicalName, proxySCTeam.Id)

                            };
                            OrganizationService.Update(updateFeedback);
                            Logger.WriteDebugMessage("Updated the PPE Feedback's Request Team with " + proxySCTeam.Name);
                        }

                        if (isEscalation)
                        {
                            //Set the Cc to the SC team
                            Email.Cc = Email.To;
                            Email.To = null;

                            //Set the To to the CoS team
                            foreach (var result in proxyCoSTeams)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                            }
                        }

                        if (ppeReview.cvt_provider == null)
                        {
                            Logger.WriteDebugMessage("Provider data isn't filled in on PPE Review, exiting.");
                            return;
                        }
                        if (ppeReview.cvt_specialty == null)
                        {
                            Logger.WriteDebugMessage("Specialty data isn't filled in on PPE Review, exiting.");
                            return;
                        }
                        var prov = srv.SystemUserSet.FirstOrDefault(su => su.Id == ppeReview.cvt_provider.Id);
                        var provEmail = (prov != null) ? prov.InternalEMailAddress : "";

                        string url = CvtHelper.getServerURL(OrganizationService) + "/userDefined/edit.aspx?etc=" + CvtHelper.GetEntityTypeCode(OrganizationService, feedback.LogicalName) + "&id=" + feedback.Id;

                        Logger.WriteDebugMessage("Building email body");
                        //Custom email text
                        string link = "<a href=\"" + url + "\">Link</a>";
                        CustomMessage = "";
                        //Edit the E-mail body
                        if (isEscalation)
                        {
                            foreach (ActivityParty ap in Email.Cc)
                            {
                                var user = srv.SystemUserSet.FirstOrDefault(u => u.Id == ap.PartyId.Id);
                                if (user != null)
                                {
                                    if (team != "")
                                        team += ", ";
                                    team += user.FirstName + " " + user.LastName;
                                }
                            }
                            CustomMessage = String.Format("{0} days ago, a request was sent to these people: {1}, for feedback to the Service Chief at the Provider’s Home Facility for PPE purposes.<br><br>This feedback is overdue.  Please take action to have the staff at our facility provider the required information as soon as possible.<br/><br/>Thank you.<br/><br/>", daysElapsed.ToString("%d"), team);
                        }

                        CustomMessage += "The following Telehealth provider’s clinical work is being reviewed as part of a focused or ongoing professional practice evaluation.<br/>";
                        CustomMessage += "<b>" + ppeReview.cvt_provider.Name + ";  " + ppeReview.cvt_specialty.Name + "</b><br/>";
                        CustomMessage += "<b>" + provEmail + "</b><br/><br/>";

                        CustomMessage += "As part of this evaluation, we must collect specific information from each facility where the provider is delivering Telehealth services. Unless already reported, the specific information needed includes:";
                        CustomMessage += "<ul><li>Any adverse outcomes related to the provider’s performance of their privileges </li>";
                        CustomMessage += "<li>Any complaints about the provider from patients, staff, etc</li></ul><br/><br/>";

                        CustomMessage += "As part of the evaluation, we area also interested in any positive feedback noted about the provider’s clinical care<br/><br/>";

                        CustomMessage += "Instructions and Next Steps<br/>";
                        CustomMessage += "<ol><li>Click the following link to enter the reporting record: " + link + "</li>";
                        CustomMessage += "<li>If you have something to report, positive or negative, related to this provider’s performance, please record a “Yes” and click “Save and Close.”<br/>If you have nothing to report,  positive or negative, related to this provider’s performance, please record a “No” and click “Save and Close.” </li>";
                        CustomMessage += "<li>Your feedback will be sent automatically  to the provider’s Service Chief at the provider’s home facility (AKA Privileging Facility)";
                        CustomMessage += "<ul><li>If you recorded a “Yes” you will be contacted by secure email for your report.</li><li>If you recorded a “No” no further action is needed</li></ul></ol>";


                        //Standard email text
                        CustomMessage += "<br/><br/>Thank you.<br/><br/>This is an automated notification from the Telehealth Management Platform.";
                        Email.Description = CustomMessage;
                        if (Email.To != null)
                            CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
                    }
                }
            }
        }

        internal void SendTSAProviderEmail(Guid recordId, string EntityName)
        {
            Logger.WriteDebugMessage("starting SendTSAProviderEmail");
            if (Email.Subject.Contains("Changing provider(s) for telemedicine service"))
            {
                //Get the owner of the workflow for the From field
                Logger.WriteDebugMessage("Adding the From");
                Email.From = CvtHelper.GetWorkflowOwner("Privileging: PPE Submitted", OrganizationService);
                using (var srv = new Xrm(OrganizationService))
                {
                    if (EntityName == cvt_providerresourcegroup.EntityLogicalName)
                    {
                        Logger.WriteDebugMessage("EntityName = cvt_providerresourcegroup");
                        var prg = srv.cvt_providerresourcegroupSet.FirstOrDefault(p => p.Id == recordId && p.cvt_RelatedTSAid != null);
                        //Replacing with scheduling package
                        //mcs_services tsa = null;
                        cvt_facilityapproval tsa = null;
                        if (prg != null)
                            tsa = srv.cvt_facilityapprovalSet.FirstOrDefault(t => t.Id == prg.cvt_RelatedTSAid.Id);

                        Logger.WriteDebugMessage("Retrieved prg and tsa");
                        addTSATeamstoEmail(tsa);
                    }
                    else if (EntityName == mcs_groupresource.EntityLogicalName)
                    {
                        Logger.WriteDebugMessage("EntityName = mcs_groupresource");
                        var gr = srv.mcs_groupresourceSet.FirstOrDefault(g => g.Id == recordId);
                        var relatedPRGs = srv.cvt_providerresourcegroupSet.Where(p => p.cvt_RelatedResourceGroupid.Id == gr.mcs_relatedResourceGroupId.Id && p.cvt_RelatedTSAid != null);
                        foreach (cvt_providerresourcegroup item in relatedPRGs)
                        {
                            var tsa = srv.cvt_facilityapprovalSet.FirstOrDefault(t => t.Id == item.cvt_RelatedTSAid.Id);
                            addTSATeamstoEmail(tsa);
                        }
                    }
                    if (Email.To != null)
                        CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
                    else
                        Logger.WriteDebugMessage("No users listed in TO of email.");
                }
            }
        }

        /// <summary>
        /// Adds BOTH the FTC and SC Teams to the existing email
        /// </summary>
        /// <param name="tsa"></param>
        internal void addTSATeamstoEmail(cvt_facilityapproval tsa)
        {
            Logger.WriteDebugMessage("Starting addTSATeamstoEmail");
            if (tsa != null)
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    cvt_resourcepackage resourcePkg = srv.cvt_resourcepackageSet.FirstOrDefault(r => r.cvt_resourcepackageId == tsa.cvt_resourcepackage.Id);
                    //Provider
                    List<Team> FTCTeam = new List<Team>();
                    List<Team> SCTeam = new List<Team>();
                    FTCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == tsa.cvt_providerfacility.Id && p.cvt_Type.Value == (int)Teamcvt_Type.FTC).Distinct().ToList();
                    Logger.WriteDebugMessage("Retrieved Prov FTC Teams: " + FTCTeam.Count);
                    SCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == tsa.cvt_providerfacility.Id && p.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && p.cvt_ServiceType.Id == resourcePkg.cvt_specialty.Id).Distinct().ToList();
                    Logger.WriteDebugMessage("Retrieved Prov SC Teams: " + SCTeam.Count);
                    //Loop the results into the TO field
                    foreach (var result in FTCTeam)
                    {
                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                    }
                    Logger.WriteDebugMessage("Added FTC Team members to Email TO.");
                    foreach (var result in SCTeam)
                    {
                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                    }
                    Logger.WriteDebugMessage("Added SC Team members to Email TO.");
                    if (tsa.cvt_patientfacility.Id != tsa.cvt_providerfacility.Id)
                    {
                        Logger.WriteDebugMessage("TSA is interfacility.");

                        //Patient
                        FTCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == tsa.cvt_patientfacility.Id && p.cvt_Type.Value == (int)Teamcvt_Type.FTC).Distinct().ToList();
                        Logger.WriteDebugMessage("Retrieved Pat FTC Teams: " + FTCTeam.Count);
                        SCTeam = srv.TeamSet.Where(p => p.cvt_Facility.Id == tsa.cvt_patientfacility.Id && p.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && p.cvt_ServiceType.Id == resourcePkg.cvt_specialty.Id).Distinct().ToList();
                        Logger.WriteDebugMessage("Retrieved Pat SC Teams: " + SCTeam.Count);

                        //Loop the results into the TO field
                        foreach (var result in FTCTeam)
                        {
                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                        }
                        foreach (var result in SCTeam)
                        {
                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                        }
                    }
                    else
                    {
                        Logger.WriteDebugMessage("TSA is intrafacility.");
                    }
                }
            }
        }
        #endregion
    }
}
