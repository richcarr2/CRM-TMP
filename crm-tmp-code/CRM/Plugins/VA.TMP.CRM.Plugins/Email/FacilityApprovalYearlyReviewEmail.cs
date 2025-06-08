using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using Microsoft.Xrm.Sdk.Query;

namespace VA.TMP.CRM
{
    public class FacilityApprovalYearlyReviewEmail
    {

        #region Constructor/Data Model for this type of email
        IOrganizationService OrganizationService;
        MCSLogger Logger;
        Email Email;
        MCSSettings McsSettings;

        public FacilityApprovalYearlyReviewEmail(IOrganizationService organizationService, MCSLogger logger, Email email)
        {
            OrganizationService = organizationService;
            Logger = logger;
            Email = email;
        }
        #endregion

        #region Entry point (Execute Method) for this class
        public void Execute()
        {
            Logger.WriteDebugMessage("Starting FacilityApprovalYearlyReviewEmail...");
            SendFacilityApprovalYearlyReviewEmail(Email.RegardingObjectId);
        }
        #endregion

        private void SendFacilityApprovalYearlyReviewEmail(EntityReference regardingObject)
        {
            var subject = "";
            var footerText = "For technical assistance, contact the National Telehealth Technology Help Desk (NTTHD) 866 651-3180 or (703) 234-4483, Monday through Saturday, 7 a.m. through 11 p.m. EST.";
            string body = "";

            if (regardingObject.LogicalName == cvt_facilityapproval.EntityLogicalName)
            {
                Logger.WriteDebugMessage("regardingObject: " + regardingObject.LogicalName);
                string specialty = "";
                string providerFacility = "";
                string patientFacility = "";
                string hubName = "";
                string initialApprovalTeam = "";
                string reviewDueDate = "";

                using (var srv = new Xrm(OrganizationService))
                {
                    var facilityApproval = srv.cvt_facilityapprovalSet.FirstOrDefault(fa => fa.Id == regardingObject.Id);
                    if (facilityApproval == null)
                        throw new InvalidPluginExecutionException("Error: Could not retrieve Facility Approval.");
                    if (facilityApproval.cvt_providerfacility != null)
                        providerFacility = facilityApproval.cvt_providerfacility.Name;
                    if (facilityApproval.cvt_patientfacility != null)
                        patientFacility = facilityApproval.cvt_patientfacility.Name;


                    var schedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(sp => sp.Id == facilityApproval.cvt_resourcepackage.Id);

                    if (schedulingPackage.cvt_hub != null && schedulingPackage.cvt_hub.Id != Guid.Empty)
                        hubName = schedulingPackage.cvt_hub.Name;
                    if (schedulingPackage == null)
                        throw new InvalidPluginExecutionException("Error: Could not retrieve Scheduling Package.");
                    if (schedulingPackage.cvt_specialty != null)
                        specialty = schedulingPackage.cvt_specialty.Name;


                    if (!String.IsNullOrEmpty(hubName))
                        initialApprovalTeam = "Hub Director";
                    else
                        initialApprovalTeam = "Provider and Patient FTCs";

                    Logger.WriteDebugMessage("initialApprovalTeam: " + initialApprovalTeam);

                    reviewDueDate = GetReviewDueDate(OrganizationService, regardingObject);

                    if (Email.Subject == "Facility Approval Yearly Review Initial Email")
                    {
                        subject = providerFacility + " and " + patientFacility + " agreement needs yearly review";
                        body = providerFacility + " and " + patientFacility + " agreement needs review by " + initialApprovalTeam + ", " + specialty + ", " + "telehealth service. To review, please click ";
                        body += GetRecordLink(regardingObject, OrganizationService, "here");
                        body += ". <b>This review is due by " + reviewDueDate + " or the associated record will be inactivated.</b><br/><br/>";
                        body += footerText;
                    }
                    else if (Email.Subject == "Facility Approval Yearly Review Final Warning Email")
                    {
                        subject = providerFacility + " and " + patientFacility + " agreement needs yearly review ASAP";
                        body = providerFacility + " and " + patientFacility + " agreement needs review by " + initialApprovalTeam + ", " + specialty + " telehealth service. To review, please click ";
                        body += GetRecordLink(regardingObject, OrganizationService, "here");
                        body += ".  <b>This review is due by " + reviewDueDate + " and the associated TSA will be inactivated 7 days from this message.</b><br/><br/>";
                        body += footerText;
                    }
                    else if (Email.Subject == "Facility Approval Yearly Review Overdue Email")
                    {
                        subject = providerFacility + " and " + patientFacility + " agreement’s yearly review is overdue and service is deactivated";
                        body = providerFacility + " and " + patientFacility + ", it has been more than a year since this Telehealth service agreement has been reviewed and signed by " + initialApprovalTeam + ", + " + specialty + " Telehealth service. To review, please click ";
                        body += GetRecordLink(regardingObject, OrganizationService, "here");
                        body += ". <b>This review was due by " + reviewDueDate + " and the associated TSA is no longer active.</b><br/><br/>";
                        body += footerText;
                    }

                    //Send emails
                    if (schedulingPackage.cvt_hub != null && schedulingPackage.cvt_hub.Id != Guid.Empty)
                    {
                        //Add Hub Team
                        var HubTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.HubDirector && (t.cvt_Facility.Id == schedulingPackage.cvt_hub.Id)).Distinct().ToList();

                        foreach (var result in HubTeams)
                        {
                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                        }
                    }
                    else
                    {
                        if (Email.Description == "Patient")
                        {
                            //Add FTC Team
                            var FTCTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.FTC && (t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id)).Distinct().ToList();

                            foreach (var result in FTCTeams)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                            }
                        }
                        else if (Email.Description == "Provider")
                        {
                            //Add FTC Team
                            var FTCTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.FTC && (t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();

                            foreach (var result in FTCTeams)
                            {
                                Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                            }
                        }

                    }

                }
            }

            Email.Subject = subject;
            Email.Description = body;
            Email.From = CvtHelper.GetWorkflowOwner("Approval Process - Get Owner", OrganizationService);
            if (Email.To != null)
                CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
        }

        internal static string GetReviewDueDate(IOrganizationService service, EntityReference regardingObject)
        {
            Entity facilityApproval = service.Retrieve(cvt_facilityapproval.EntityLogicalName, regardingObject.Id, new ColumnSet("cvt_reviewduedate"));
            return ((DateTime) facilityApproval.Attributes["cvt_reviewduedate"]).AddHours(-4).ToShortDateString();
        }

        internal static string GetRecordLink(EntityReference record, IOrganizationService OrganizationService, string clickHereText = "")
        {
            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, record.LogicalName);
            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            string url = servernameAndOrgname + "/userDefined/edit.aspx?etc=" + etc + "&id=" + record.Id;
            return String.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);
        }


    }
}