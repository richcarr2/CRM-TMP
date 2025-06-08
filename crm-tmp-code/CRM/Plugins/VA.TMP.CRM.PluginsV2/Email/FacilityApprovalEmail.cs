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

namespace VA.TMP.CRM
{
    public class FacilityApprovalEmail
    {
        #region Constructor/Data Model for this type of email
        IOrganizationService OrganizationService;
        MCSLogger Logger;
        Email Email;
        MCSSettings McsSettings;

        public FacilityApprovalEmail(IOrganizationService organizationService, MCSLogger logger, Email email)
        {
            OrganizationService = organizationService;
            Logger = logger;
            Email = email;
        }
        #endregion

        #region Entry point (Execute Method) for this class
        public void Execute()
        {
            SendFacilityApprovalEmail(Email.RegardingObjectId);
        }
        #endregion

        #region Populate and send the email

        //Send appropriate email based on subject line and type
        private void SendFacilityApprovalEmail(EntityReference regardingObject)
        {
            var subject = "";
            var footerText = "For technical assistance, contact the National Telehealth Technology Help Desk (NTTHD) 866 651-3180 or (703) 234-4483, Monday through Saturday, 7 a.m. through 11 p.m. EST.";
            string body = "";


            if (regardingObject.LogicalName == cvt_participatingsite.EntityLogicalName)
            {
                //Send email to intrafacility
                //Subject: Notification of Intrafacility[Specialty] Telehealth Services to[New Site]

                //Body:
                subject = Email.Subject;
                string newSite = null;
                string side = null;
                string specialtyText = null;
                using (var srv = new Xrm(OrganizationService))
                {
                    var participatingSite = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == regardingObject.Id);
                    if (participatingSite == null)
                        throw new InvalidPluginExecutionException("Error: Could not retrieve participating site.");
                    if (participatingSite.cvt_site != null)
                        newSite = participatingSite.cvt_site.Name;
                    if (participatingSite.cvt_locationtype != null)
                    {
                        switch (participatingSite.cvt_locationtype.Value)
                        {
                            case (int)cvt_participatingsitecvt_locationtype.Patient:
                                side = "Patient";
                                break;
                            case (int)cvt_participatingsitecvt_locationtype.Provider:
                                side = "Provider";
                                break;
                        }
                    }

                    var schedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(sp => sp.Id == participatingSite.cvt_resourcepackage.Id);
                    if (schedulingPackage == null)
                        throw new InvalidPluginExecutionException("Error: Could not retrieve scheduling package.");
                    if (schedulingPackage.cvt_specialty != null)
                        specialtyText = schedulingPackage.cvt_specialty.Name;

                    var tmpSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == participatingSite.cvt_site.Id);
                    if (tmpSite == null)
                        throw new InvalidPluginExecutionException("Error: Could not retrieve TMP site.");


                    if (schedulingPackage.cvt_hub != null && schedulingPackage.cvt_hub.Id != Guid.Empty)
                    {
                        //Add Hub Director - should only ever be one
                        var HubDirectorTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == schedulingPackage.cvt_hub.Id && t.cvt_Type.Value == (int)Teamcvt_Type.HubDirector).Distinct().ToList();

                        foreach (var result in HubDirectorTeams)
                        {
                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                        }

                    }
                    else
                    {
                        //Add Service Chief Team - should only ever be one
                        var SCTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == tmpSite.mcs_FacilityId.Id && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == schedulingPackage.cvt_specialty.Id).Distinct().ToList();

                        foreach (var result in SCTeams)
                        {
                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                        }
                    }
                    //Recipient: Facility Specialty Service Chief Team, Facility Chief of Staff Team


                    //Add Chief of Staff Team - should only ever be one
                    var COSTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == tmpSite.mcs_FacilityId.Id && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff).Distinct().ToList();

                    foreach (var result in COSTeams)
                    {
                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                    }
                }

                if (newSite == null || side == null || specialtyText == null)
                    throw new InvalidPluginExecutionException("Error: Could not retrieve values for email.");

                body += "This notification is to inform you that " + newSite + " has been added as a " + side + " Site for the " + specialtyText + " Service. No approvals are required. Pending resource set-up for " + newSite + ", " + specialtyText + " services can now be scheduled for Telehealth for this Site.<br/><b>";

                body += GetRecordLink(regardingObject, OrganizationService, "Click here to view the new Participating Site for the Scheduling Package.");
                body += "</b><br/><br/>";
            }
            else if (regardingObject.LogicalName == cvt_facilityapproval.EntityLogicalName)
            {
                string specialty = "";
                string providerFacility = "";
                string patientFacility = "";
                string hubName = "";
                string prevApprover = "";

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

                    if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                    {
                        footerText = "<br/><b>Assistance:</b><br/>For technical assistance, clinicians should contact the Office of Connected Care Help Desk (OCCHD) (866) 651 - 3180 or (703) 234 - 4483, available 24/7/365.";
                    }

                    string specialtysubtype = schedulingPackage.cvt_specialtysubtype != null ? specialty + ": " + schedulingPackage.cvt_specialtysubtype.Name : specialty;

                    var hubproviderFacilitySubject = providerFacility;
                    var hubproviderFacilityBody = string.Empty;

                    if (facilityApproval.cvt_hubfacility != null)
                    {
                        if (facilityApproval.cvt_hubfacility.Id != facilityApproval.cvt_providerfacility.Id)
                        {
                            hubproviderFacilitySubject = facilityApproval.cvt_hubfacility.Name + "/" + providerFacility;
                            hubproviderFacilityBody = " with a home facility of " + providerFacility;
                        }
                    }

                    if (Email.Subject == "TSAAPPROVED")
                    {
                        if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                        {
                            var TsaText = schedulingPackage.cvt_hub != null ? "- HUB TSA Notification Email" : "- TSA Notification Email";

                            subject = "Approval Completed: " + specialtysubtype + " Telehealth Services for " + hubproviderFacilitySubject + TsaText;
                            body = Email.Description;
                        }
                        else
                        {
                            subject = "Approval Completed: " + specialty + " Telehealth Services for " + providerFacility + " to " + patientFacility;
                            body = Email.Description;
                        }


                    }
                    else
                    {

                        if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                        {
                            subject = "Approval Required: " + specialtysubtype + " Telehealth Services for " + hubproviderFacilitySubject;
                        }
                        else
                        {
                            subject = "Approval Required: " + specialty + " Telehealth Services for " + providerFacility + " to " + patientFacility;

                        }

                        if (schedulingPackage.cvt_hub != null && schedulingPackage.cvt_hub.Id != Guid.Empty)
                        {
                            if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                            {
                                body = "A Telehealth Service Agreement has been created for a(n) " + specialtysubtype + " service from at your Facility, " + facilityApproval?.cvt_hubfacility?.Name + hubproviderFacilityBody + "  and awaiting your approval.<br/><br/>";
                            }
                            else
                            {
                                body += "The mail notification is coming from Hub Facility in " + hubName + " requesting approval for services provided by the credentialed provider(s) whose home facility is " + providerFacility + ". This approval will acknowledge the provider(s) is credentialed in good standing at " + providerFacility + " and the patient facility " + patientFacility + " will accept proxy privileging and approve the required clinical resources, emergency procedures for the services rendered.<br/><br/>A Telehealth Service Agreement (TSA) is awaiting your approval.<br/><br/>";
                            }
                        }
                        else
                        {
                            if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                            {
                                subject = "Approval Required: " + specialtysubtype + " Telehealth Services for " + providerFacility;
                                body = "A Telehealth Service Agreement has been created for a(n) " + specialtysubtype + " service at your Facility, " + providerFacility + " and awaiting your approval.<br/><br/>";
                            }
                            else
                            {
                                body = "A Telehealth Service Agreement has been created for a(n) " + specialty + " service from " + providerFacility + " to " + patientFacility + ". A Telehealth Service Agreement (TSA) is awaiting your approval.<br/><br/>";
                            }
                        }
                    }

                    //Send actual approval emails for the SC and COS approvals
                    switch (Email.Subject)
                    {
                        case "FTC Team Approval Requested":
                            if (schedulingPackage.cvt_hub != null && schedulingPackage.cvt_hub.Id != Guid.Empty)
                            {
                                //Add Hub Team and 2 COS teams
                                var HubTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.HubDirector && (t.cvt_Facility.Id == schedulingPackage.cvt_hub.Id)).Distinct().ToList();

                                foreach (var result in HubTeams)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }
                                if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                                {
                                    subject += " - HUB Director  Email";

                                    body += "<b>Approval Required By: </b><br/>HUB Director Team(s):<br/>";
                                    body += GetRecordLink(facilityApproval.cvt_HubDirectorTeam, OrganizationService, "HUB Director Approval Group Team @ " + facilityApproval.cvt_providerfacility?.Name + "</b>");
                                    body += "<br/><br/>";
                                    body += "<ol>";
                                    body += "<li>";
                                    body += "<b>To Review:</b><br/>";
                                    body += "Click " + BuildReportLink(regardingObject, OrganizationService, McsSettings, "this link") + " to view the Telehealth Service Agreement.";
                                    body += "</li>";
                                    body += "<br/>";
                                    body += "<li>";
                                    body += "<b>To Take Action:</b><br/>";
                                    body += GetRecordLink(regardingObject, OrganizationService, "Click here to take action on the TSA.</b>");
                                    body += "</li>";
                                    body += "</ol>";
                                    body += "<br/>";
                                    body += "<b>Next in line for the TSA Approval Process are the Chief(s) of Staff:</b><br/>Chief of Staff Team(s):<br/> ";
                                    body += GetRecordLink(facilityApproval.cvt_ChiefofStaffTeamProvider, OrganizationService, "Chief of Staff Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</b>");
                                    // body += "<br/><br/>";

                                    if (facilityApproval.cvt_hubfacility?.Id != facilityApproval.cvt_providerfacility?.Id)
                                    {
                                        //  body += "Chief of Staff Team(s):<br/>";
                                        body += "<br/><br/> " + GetRecordLink(facilityApproval.cvt_ChiefofStaffTeamPatient, OrganizationService, "Chief of Staff Approval Group Team @" + facilityApproval.cvt_hubfacility?.Name + "</b>");
                                    }
                                    body += "<br/><br/>" + footerText;

                                }
                                else
                                {
                                    body += "Hub Approval Process goes to Hub Director Team for approval first, then to both Chief of Staff Teams simultaneously.<br/><br/>";
                                }


                            }
                            else
                            {
                                //Add FTC Team - should be two
                                var FTCTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.FTC && (t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id || t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();

                                foreach (var result in FTCTeams)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }
                                if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                                {
                                    subject += " - FTC Email";
                                    body += "<b>Approval Required By: </b><br/>Facility Telehealth Coordinators (FTC) Team(s):<br/>";
                                    body += GetRecordLink(facilityApproval.cvt_FTCTeamProvider, OrganizationService, "FTC Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</b>");
                                    body += "<br/><br/>";
                                    body += "<ol>";
                                    body += "<li>";
                                    body += "<b>To Review:</b><br/>";
                                    body += "Click " + BuildReportLink(regardingObject, OrganizationService, McsSettings, "this link") + " to view the Telehealth Service Agreement.";
                                    body += "</li>";
                                    body += "<br/>";
                                    body += "<li>";
                                    body += "<b>To Take Action:</b><br/>";
                                    body += GetRecordLink(regardingObject, OrganizationService, "Click here to take action on the TSA.</b>");
                                    body += "</li>";
                                    body += "</ol>";
                                    body += "<br/>";
                                    body += "<b>Next in line for the TSA Approval Process are: </b><br/>Service Chief Team(s):<br/> ";
                                    body += GetRecordLink(facilityApproval.cvt_ServiceChiefTeamProvider, OrganizationService, "Service Chief Approval Group Team  @" + facilityApproval.cvt_providerfacility?.Name + "</b>");
                                    body += "<br/><br/>";
                                    body += "Chief of Staff Team(s):<br/>";
                                    body += GetRecordLink(facilityApproval.cvt_ChiefofStaffTeamProvider, OrganizationService, "Chief of Staff Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</b><br/><br/>");
                                    body += footerText;
                                }
                                else
                                {
                                    body += "The Service Chief and Chief of Staff Teams are next in line for the TSA Approval Process.<br/><br/>";
                                }
                            }
                            break;
                        case "Service Chief Approval Requested":
                            if (schedulingPackage.cvt_hub != null && schedulingPackage.cvt_hub.Id != Guid.Empty)
                            {
                                //Add Hub Team and 2 COS teams
                                var HubTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.HubDirector && (t.cvt_Facility.Id == schedulingPackage.cvt_hub.Id)).Distinct().ToList();

                                foreach (var result in HubTeams)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }

                                body += "Hub Approval Process goes to Hub Director Team for approval first, then to both Chief of Staff Teams simultaneously.<br/><br/>";
                            }
                            else
                            {
                                //Add Service Chief Team - should be one since its branched
                                List<Team> SCTeams = null;
                                if (Email.Description == "Patient") // send to the PATIENT SC team
                                {
                                    SCTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == schedulingPackage.cvt_specialty.Id && (t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id)).Distinct().ToList();
                                }
                                else if (Email.Description == "Provider")// its to the PROVIDER SC Team
                                {
                                    SCTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == schedulingPackage.cvt_specialty.Id && (t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();
                                }
                                if (SCTeams != null)
                                {
                                    foreach (var result in SCTeams)
                                    {
                                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                    }
                                }
                                if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                                {
                                    subject += " - Specialty Service Chief Email";
                                    //body += "<b>Previous Approval(s): </b><br/>The ";
                                    //body += GetRecordLink(facilityApproval.cvt_FTCTeamProvider, OrganizationService, "FTC Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    //body += " has already approved this TSA.";
                                    //body += "<br/>";
                                    //body += "Previous Approver: " + facilityApproval.ModifiedBy.Name + "<br/><br/>";
                                    body += "<b>Approval Required By: </b><br/>Service Chief Team(s): <br/>";
                                    body += GetRecordLink(facilityApproval.cvt_ServiceChiefTeamProvider, OrganizationService, "Service Chief Approval Group Team  @" + facilityApproval.cvt_providerfacility?.Name + "</b>");
                                    body += "<br/><br/>";
                                    body += "<ol>";
                                    body += "<li>";
                                    body += "<b>To Review:</b><br/>";
                                    body += "Click " + BuildReportLink(regardingObject, OrganizationService, McsSettings, "this link") + " to view the Telehealth Service Agreement.";
                                    body += "</li>";
                                    body += "<br/>";
                                    body += "<li>";
                                    body += "<b>To Take Action:</b><br/>";
                                    body += GetRecordLink(regardingObject, OrganizationService, "Click here to take action on the TSA.</b>");
                                    body += "</li>";
                                    body += "</ol>";
                                    body += "<br/>";

                                    body += "<b>Previous Approval(s): </b><br/>The ";
                                    body += GetRecordLink(facilityApproval.cvt_FTCTeamProvider, OrganizationService, "FTC Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    body += " has already approved this TSA.";
                                    body += "<br/>";
                                    body += "Previous Approver: " + facilityApproval.ModifiedBy.Name + "<br/><br/>";

                                    body += "<b>Next in line for the TSA Approval Process are: </b><br/>Chief of Staff Team(s):<br/> ";
                                    body += GetRecordLink(facilityApproval.cvt_ChiefofStaffTeamProvider, OrganizationService, "Chief of Staff Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</b><br/><br/>");




                                    body += footerText;
                                }
                                else
                                {
                                    body += "The FTC Team has already approved this TSA.<br/>";
                                    body += "Previous Approver: " + facilityApproval.ModifiedBy.Name + "<br/><br/>";
                                    body += "The Service Chief and Chief of Staff Teams are next in line for the TSA Approval Process.<br/><br/>";
                                }
                            }
                            break;
                        case "Chief of Staff Approval Requested":
                            if (Email.Description == "Patient")
                            {
                                //Add Chief of Staff Team - should only ever be one
                                var COSTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff).Distinct().ToList();

                                foreach (var result in COSTeams)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }

                                if (facilityApproval.cvt_SigneePatientSC != null)
                                {
                                    prevApprover = facilityApproval.cvt_SigneePatientSC.Name;
                                    body += "The FTC and Service Chief Teams have already approved this TSA.<br/>";
                                    body += "Previous Approver: " + prevApprover + "<br/><br/>";
                                }
                            }
                            else if (Email.Description == "Provider")
                            {
                                //Add Chief of Staff Team - should only ever be one
                                var COSTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff).Distinct().ToList();

                                foreach (var result in COSTeams)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }

                                if (facilityApproval.cvt_SigneeProviderSC != null && schedulingPackage.cvt_intraorinterfacility.Value != 917290000)
                                {
                                    prevApprover = facilityApproval.cvt_SigneeProviderSC.Name;
                                    body += "The FTC and Service Chief Teams have already approved this TSA.<br/>";
                                    body += "Previous Approver: " + prevApprover + "<br/><br/>";
                                }

                                if (facilityApproval.cvt_SigneeProviderSC != null && schedulingPackage.cvt_intraorinterfacility.Value == 917290000)
                                {
                                    subject += " - COS Email";
                                    //body += "<b>Previous Approval(s): </b><br/>The ";
                                    //body += GetRecordLink(facilityApproval.cvt_FTCTeamProvider, OrganizationService, "FTC Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    //body += " has already approved this TSA.";
                                    //body += "<br/>";
                                    //body += "Previous Approver: " + facilityApproval.cvt_SigneeProviderSC.Name + "<br/><br/>";
                                    //body +="The "+GetRecordLink(facilityApproval.cvt_FTCTeamProvider, OrganizationService, schedulingPackage.cvt_specialty?.Name + " Service Chief Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    //body += " has already approved this TSA.";
                                    //body += "<br/>";  
                                    //body += "Previous Approver: " + facilityApproval.cvt_SigneeProviderSC.Name + "<br/><br/>";
                                    body += "<b>Approval Required By: </b><br/>Chief of Staff Team(s): <br/>";
                                    body += GetRecordLink(facilityApproval.cvt_ChiefofStaffTeamProvider, OrganizationService, "Chief of Staff Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</b>");
                                    body += "<br/><br/>";
                                    body += "<ol>";
                                    body += "<li>";
                                    body += "<b>To Review:</b><br/>";
                                    body += "Click " + BuildReportLink(regardingObject, OrganizationService, McsSettings, "this link") + " to view the Telehealth Service Agreement.";
                                    body += "</li>";
                                    body += "<br/>";
                                    body += "<li>";
                                    body += "<b>To Take Action:</b><br/>";
                                    body += GetRecordLink(regardingObject, OrganizationService, "Click here to take action on the TSA.</b>");
                                    body += "</li>";
                                    body += "</ol>";
                                    body += "<br/>";
                                    body += "<b>Previous Approval(s): </b><br/>The ";
                                    body += GetRecordLink(facilityApproval.cvt_FTCTeamProvider, OrganizationService, "FTC Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    body += " has already approved this TSA.";
                                    body += "<br/>";
                                    body += "Previous Approver: " + facilityApproval.cvt_SigneeProviderSC.Name + "<br/><br/>";
                                    body += "The " + GetRecordLink(facilityApproval.cvt_ServiceChiefTeamProvider, OrganizationService, " Service Chief Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    body += " has already approved this TSA.";
                                    body += "<br/>";
                                    body += "Previous Approver: " + facilityApproval.cvt_SigneeProviderSC.Name + "<br/><br/>";

                                    body += footerText;

                                }
                            }
                            else if (Email.Description == "Hub")
                            {
                                //Add Chief of Staff Teams
                                //var COSTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && (t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id || t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();

                                //foreach (var result in COSTeams)
                                //{
                                //    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                //}

                                if (facilityApproval.cvt_SigneeHubDirector != null && schedulingPackage.cvt_intraorinterfacility.Value != 917290000)//Inter
                                {
                                    var COSTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && (t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id || t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();

                                    foreach (var result in COSTeams)
                                    {
                                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                    }

                                    prevApprover = facilityApproval.cvt_SigneeHubDirector.Name;
                                    body += "Previous Approver: " + prevApprover + "<br/><br/>";
                                }

                                if (facilityApproval.cvt_SigneeHubDirector != null && schedulingPackage.cvt_intraorinterfacility.Value == 917290000)//Intra
                                {
                                    if (facilityApproval.cvt_hubfacility?.Id != facilityApproval.cvt_providerfacility?.Id)
                                    {
                                        var COSTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && (t.cvt_Facility.Id == facilityApproval.cvt_hubfacility.Id || t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();

                                        foreach (var result in COSTeams)
                                        {
                                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                        }
                                    }
                                    else
                                    {
                                        var providerCOSTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id).Distinct().ToList();

                                        foreach (var result in providerCOSTeams)
                                        {
                                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                        }
                                    }

                                    subject += " - CHIEF(S) OF STAFF Email";

                                    //body += "<b>Previous Approval(s): </b><br/>The ";
                                    //body += GetRecordLink(facilityApproval.cvt_HubDirectorTeam, OrganizationService, "HUB DIRECTOR Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    //body += " has already approved this TSA.";
                                    //body += "<br/>";
                                    //body += "Previous Approver: " + facilityApproval.cvt_SigneeHubDirector?.Name + "<br/><br/>";
                                    body += "<b>Approval Required By: </b><br/>Chief of Staff Team(s): <br/>";
                                    body += GetRecordLink(facilityApproval.cvt_ChiefofStaffTeamProvider, OrganizationService, "Chief of Staff Approval Group Team @ " + facilityApproval.cvt_providerfacility?.Name + "</b>");
                                    //body += "<br/><br/>";
                                    if (facilityApproval.cvt_hubfacility?.Id != facilityApproval.cvt_providerfacility?.Id)
                                    {
                                        //body += "Chief of Staff Team(s):<br/>";
                                        body += "<br/><br/>" + GetRecordLink(facilityApproval.cvt_ChiefofStaffTeamPatient, OrganizationService, "Chief of Staff Approval Group Team @" + facilityApproval.cvt_hubfacility?.Name + "</b><br/><br/>");
                                    }
                                    body += "<ol>";
                                    body += "<li>";
                                    body += "<b>To Review:</b><br/>";
                                    body += "Click " + BuildReportLink(regardingObject, OrganizationService, McsSettings, "this link") + " to view the Telehealth Service Agreement.";
                                    body += "</li>";
                                    body += "<br/>";
                                    body += "<li>";
                                    body += "<b>To Take Action:</b><br/>";
                                    body += GetRecordLink(regardingObject, OrganizationService, "Click here to take action on the TSA.</b>");
                                    body += "</li>";
                                    body += "</ol>";
                                    body += "<br/>";

                                    body += "<b>Previous Approval(s): </b><br/>The ";
                                    body += GetRecordLink(facilityApproval.cvt_HubDirectorTeam, OrganizationService, "HUB Director Approval Group Team @" + facilityApproval.cvt_providerfacility?.Name + "</ b>");
                                    body += " has already approved this TSA.";
                                    body += "<br/>";
                                    body += "Previous Approver: " + facilityApproval.cvt_SigneeHubDirector?.Name + "<br/><br/>";


                                    body += footerText;

                                }


                            }
                            break;
                        case "TSAAPPROVED":
                            if (schedulingPackage.cvt_hub != null && schedulingPackage.cvt_hub.Id != Guid.Empty)
                            {
                                if (schedulingPackage.cvt_intraorinterfacility.Value == 917290000)//Intra
                                {
                                    if (facilityApproval.cvt_hubfacility?.Id != facilityApproval.cvt_providerfacility?.Id)
                                    {
                                        var COSTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification && (t.cvt_Facility.Id == facilityApproval.cvt_hubfacility.Id || t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();

                                        foreach (var result in COSTeams)
                                        {
                                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                        }
                                    }
                                    else
                                    {
                                        var providerCOSTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification && t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id).Distinct().ToList();

                                        foreach (var result in providerCOSTeams)
                                        {
                                            Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                        }
                                    }

                                }
                                else//inter
                                {
                                    //Add TSA Approval Teams
                                    var HubTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification && (t.cvt_Facility.Id == schedulingPackage.cvt_hub.Id)).Distinct().ToList();

                                    var providerTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification && (t.cvt_Facility.Id == schedulingPackage.cvt_providerfacility.Id)).Distinct().ToList();


                                    foreach (var result in HubTeams)
                                    {
                                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger).Distinct().ToList();

                                    }

                                    foreach (var result in providerTeams)
                                    {
                                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger).Distinct().ToList();

                                    }

                                    var FTCTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification && (t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id)).Distinct().ToList();

                                    foreach (var result in FTCTeams)
                                    {
                                        Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                    }


                                    //body += "Hub Approval Process goes to Hub Director Team for approval first, then to both Chief of Staff Teams simultaneously.<br/><br/>";
                                }
                            }
                            else
                            {
                                //Add TSA Approval Teams
                                var FTCTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification && (t.cvt_Facility.Id == facilityApproval.cvt_patientfacility.Id || t.cvt_Facility.Id == facilityApproval.cvt_providerfacility.Id)).Distinct().ToList();

                                foreach (var result in FTCTeams)
                                {
                                    Email.To = CvtHelper.RetrieveFacilityTeamMembers(Email, result.Id, Email.To, OrganizationService, Logger);
                                }

                                //body += "The Service Chief and Chief of Staff Teams are next in line for the TSA Approval Process.<br/><br/>";
                            }
                            break;
                    }
                    if (Email.Subject != "TSAAPPROVED" && schedulingPackage.cvt_intraorinterfacility.Value != 917290000)
                    {
                        //body += "<b>Open the attachment to view the Telehealth Service Agreement.<br/><br/>";
                        body += "Click <b>" + BuildReportLink(regardingObject, OrganizationService, McsSettings, "this link") + "</b> to view the Telehealth Service Agreement.";
                        body += "<br/><br/> ";
                        body += GetRecordLink(regardingObject, OrganizationService, "Click here to take action on the TSA.</b><br/><br/>");
                        //body += BuildReportLink(regardingObject, OrganizationService, "Click this link for report.</b><br/><br/>");
                        body += footerText;
                    }
                }
            }

            Email.Subject = subject;
            Email.Description = body;
            //Email.From = CvtHelper.GetWorkflowOwner("TSA Approval Step 1 - Awaiting Prov FTC", OrganizationService);
            Email.From = CvtHelper.GetWorkflowOwner("Approval Process - Get Owner", OrganizationService);
            if (Email.To != null)
                CvtHelper.UpdateSendEmail(Email, OrganizationService, Logger);
        }

        #endregion

        #region Email Helpers

        internal string BuildReportLink(EntityReference record, IOrganizationService OrganizationService, MCSSettings McsSettings, string clickHereText = "")
        {
            string strTsaReportGuid = string.Empty;

            using (var srv = new Xrm(OrganizationService))
            {
                //var TSANote = srv.AnnotationSet.Where(n => n.ObjectId.Id == Email.RegardingObjectId.Id).OrderByDescending(n => n.CreatedOn).First(n => n.NoteText.Contains("Approved by"));
                var settings = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings" && s.statecode == mcs_settingState.Active);
                Logger.WriteDebugMessage("about to get tsaReportGuid");
                strTsaReportGuid = settings.cvt_tsareportgiud;
                Logger.WriteDebugMessage("got tsaReportGuid");
            }

            //try
            //{

            //    var TsaReportGuid = McsSettings.GetSingleSetting("cvt_tsareportgiud", "string");

            //    strTsaReportGuid = (TsaReportGuid != null) ? TsaReportGuid : string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    Logger.WriteDebugMessage("Error Retrieving TsaReportGuid: " + ex.Message);
            //}

            Logger.WriteDebugMessage("**********BEGIN BuildReportLink**********");

            // report string:
            // DO NOT USE https://internalcrm.cvt15.dev.crm.vrm.vba.va.gov/CVT15/crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0.rdl&id=%7b8c3dfc38-c28f-e811-80e8-000d3a007bf6%7d&records=%7bF7B7FE1C-F894-E811-943F-0050568DA084%7d&recordstype=10076
            //            https://internalcrm.cvt15.dev.crm.vrm.vba.va.gov/CVT15/crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0.rdl&id=%7bdcf51896-4bf4-e811-80e8-000d3a007bf6%7d&records=%7b9D1C4B6F-C7F4-E811-9445-C38FD7F6185C%7d&recordstype=10076
            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, record.LogicalName);
            Logger.WriteDebugMessage("**********TSA REPORT 'ETC': " + etc + "**********");
            Logger.WriteDebugMessage("********** BEGIN GETTING TSA REPORT GUID **********");
            try
            {
                Logger.WriteDebugMessage("**********TSA REPORT GUID: " + strTsaReportGuid + "**********");
            }
            catch (Exception x)
            {
                Logger.WriteDebugMessage("MESSAGE: " + x.Message + "Inner Exception: " + x.InnerException);
            }
            Logger.WriteDebugMessage("********** SUCCESS!  RETRIEVED TSA REPORT GUID! **********");

            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            //string url = servernameAndOrgname + "crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0.rdl&id=%7bdcf51896-4bf4-e811-80e8-000d3a007bf6%7d&records=%7b" + record.Id.ToString() + "%7d&recordstype=" + etc.ToString();
            string url = servernameAndOrgname + "crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0_v2.rdl&id=%7b761d0e50-9ab7-e911-a9e1-000d3a05c4f0%7d&records=%7b" + record.Id.ToString() + "%7d&recordstype=" + etc.ToString();
            //string url = servernameAndOrgname + "crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0.rdl&id=%7bdcf51896-4bf4-e811-80e8-000d3a007bf6%7d&records=%7b" + record.Id.ToString() + "%7d&recordstype=10076";
            return String.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);

        }

        internal static string GetRecordLink(EntityReference record, IOrganizationService OrganizationService, string clickHereText = "")
        {
            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, record.LogicalName);
            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            string url = servernameAndOrgname + "/userDefined/edit.aspx?etc=" + etc + "&id=" + record.Id;
            return String.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);
        }

        /// <summary>
        /// Returns a string value representing the body of the email for TSA approval notification
        /// </summary>
        /// <param name="email">the object representing the email which is being sent</param>
        /// <param name="record">the Guid of the TSA which is causing this notification to be sent</param>
        /// <param name="entityStringName">the entity logical name of the tsa (i.e. "cvt_resourcepackage")</param>
        /// <returns></returns>
        private string ApprovalEmailBody()
        {
            var approver = String.Empty;
            var nextTeam = String.Empty;
            var FTC = String.Empty;
            var patFacility = String.Empty;
            //Get the Previous approvers by querying most recent note
            using (var srv = new Xrm(OrganizationService))
            {
                var TSANote = srv.AnnotationSet.Where(n => n.ObjectId.Id == Email.RegardingObjectId.Id).OrderByDescending(n => n.CreatedOn).First(n => n.NoteText.Contains("Approved by"));
                //most recent approver
                approver = TSANote.CreatedBy.Name;
                //replacing with reference to Facility Approval - WMC 11/15/2018
                //var tsa = srv.mcs_servicesSet.FirstOrDefault(t => t.Id == Email.RegardingObjectId.Id);
                var tsa = srv.cvt_facilityapprovalSet.FirstOrDefault(t => t.Id == Email.RegardingObjectId.Id);
                patFacility = tsa.cvt_patientfacility == null ? String.Empty : " To " + tsa.cvt_patientfacility.Name;


                //removing -- WMC 11/15/2018
                //per Anna, there are NO 'IntraFacility' FAs
                //if (tsa.cvt_ServiceScope.Value == 917290001)
                //    patFacility = " (Intrafacility)";

                //Get the next approver up and get the FTC who created the TSA (assumed to be provider side) and the FTC who first approved the TSA (assumed to be patient side)
                //removing -- WMC 11/15/2018
                // statuses no longer valid
                //switch (tsa.statuscode.Value)
                //{
                //    case (int)mcs_services_statuscode.ApprovedbyPatFTC:
                //        nextTeam = "Patient Service Chief Team";
                //        goto case 0;
                //    case (int)mcs_services_statuscode.ApprovedbyProvFTC:
                //        nextTeam = "Provider Service Chief Team";
                //        goto case 0;
                //    case (int)mcs_services_statuscode.ApprovedbyProvServiceChief:
                //        nextTeam = "Provider Chief of Staff Team";
                //        goto case 0;
                //    case (int)mcs_services_statuscode.ApprovedbyProvChiefofStaff:
                //        nextTeam = "Patient Service Chief Team";
                //        goto case -1;
                //    case (int)mcs_services_statuscode.ApprovedbyPatServiceChief:
                //        nextTeam = "Patient Chief of Staff Team";
                //        goto case -1;
                //    case 0: //if Provider side - get the user who created the TSA - assumed to be the Provider FTC
                //        FTC = srv.SystemUserSet.FirstOrDefault(u => u.Id == tsa.CreatedBy.Id).FullName;
                //        break;
                //    case -1: //If patient side - get user who first approved the TSA - assumed to be the Patient FTC
                //        var firstApprover = srv.AnnotationSet.Where(n => n.ObjectId.Id == Email.RegardingObjectId.Id).OrderBy(n => n.CreatedOn).First(n => n.NoteText.Contains("Approved by"));
                //        FTC = firstApprover.CreatedBy.Name;
                //        break;
                //}
            }

            //TODO: Add patient facility, change spacing
            //get the FTC for whichever side the TSA is awaiting approval
            string hyperlink = CvtHelper.GetRecordLink(Email.RegardingObjectId, OrganizationService);
            string OpsManual = "http://vaww.infoshare.va.gov/sites/telehealth/docs/tmp-user-tsa-appr.docx";
            string RollOut = "http://vaww.telehealth.va.gov/quality/tmp/index.asp";
            string emailBody = String.Format("A Telehealth Service Agreement (TSA), {0} is awaiting your approval. <br/><ul><li>Previous Approver: {1}</li>" +
                "<li>{2} is the next in line for the TSA Approval Process. </ul>The hyperlink below will take you to the Telehealth Service Agreement.  If you wish to make changes to the TSA prior to approval, please contact {3}.  If you choose to approve the TSA, please select the Green Button on the top left corner.  If you choose to decline approval, please select the Red Button on the top left corner.<br/><br/><b>Click here to take action on the TSA</b>: {4} <br/><br/>", Email.RegardingObjectId.Name + patFacility, approver, nextTeam, FTC, hyperlink);
            string loginNotes = String.Format("Note: A password is not required to access TMP.  Your credentials are passed from Windows authentication used to log on to your computer.  Simply click the link above.  For first time access, or access after a long period of time, you may be prompted to choose \"VA Accounts\" on a pop-up form.  After that, clicking the link will take you directly to the TSA.  <br/><br/>To see a brief tutorial for approvers, click this link: {0} <br/><br/>To access all resources (training materials, operations manual, etc.) for TMP users, click this link: {1}", "<a href=\"" + OpsManual + "\">" + OpsManual + "</a>", "<a href=\"" + RollOut + "\">" + RollOut + "</a>");

            return emailBody + loginNotes;
        }

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
                //replacing with reference to Facility Approval- WMC 11/15/2018
                var tsa = srv.cvt_facilityapprovalSet.First(t => t.Id == tsaID);
                var proFacilityId = tsa.cvt_providerfacility.Id;
                var patFacilityId = tsa.cvt_patientfacility != null ? tsa.cvt_patientfacility.Id : Guid.Empty;
                var team = new Team();

                //removing -- WMC 11/15/2018
                //status codes no longer valid
                switch (tsa.statuscode.Value)
                {
                    case (int)mcs_services_statuscode.Draft: //For Draft TSAs, send notification that TSA has been created for their site. 
                        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);
                        break;
                    //case (int)mcs_services_statuscode.ApprovedbyPatFTC://Approved by Patient Site FTC (get Provider Site FTC Team) - Workflow Step 1
                    //    team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);
                    //    break;
                    //case (int)mcs_services_statuscode.ApprovedbyProvFTC://Approved by Provider Site FTC (get Provider Service Chief Team) - Workflow Step 2
                    //    team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId &&
                    //        t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == tsa.cvt_servicetype.Id);
                    //    break;
                    //case (int)mcs_services_statuscode.ApprovedbyProvServiceChief://Approved by Provider Service Chief (get Prov Chief of Staff Team) - Workflow Step 3
                    //    team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff);
                    //    break;
                    //case (int)mcs_services_statuscode.ApprovedbyProvChiefofStaff://Approved by Provider Site Chief of Staff (Get Patient Site Service Chief) - Workflow Step 5
                    //    if (patFacilityId != Guid.Empty)
                    //        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId &&
                    //            t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == tsa.cvt_servicetype.Id);
                    //    break;
                    //case (int)mcs_services_statuscode.ApprovedbyPatServiceChief://Approved by Patient Site Service Chief (Get Patient Site Chief of Staff) - Workflow Step 6
                    //    if (patFacilityId != Guid.Empty)
                    //        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff);
                    //    break;
                    //case (int)mcs_services_statuscode.UnderRevision://Get both side FTCs whether it is in Denied status or in Under Revision
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
                        //case (int)mcs_services_statuscode.Production: //PROD - Get Both sides notification team for TSA Notification email
                        //    team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification);
                        //    if (team != null)
                        //        teamMembers = (List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList());

                        //    //repurpose team variable to get patient facility (prov facility team members have already been added above) and add team members from pat facility (if not intrafacility)
                        //    if (patFacilityId != Guid.Empty && patFacilityId != proFacilityId)
                        //    {
                        //        team = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == patFacilityId && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.TSANotification);
                        //        if (team != null)
                        //        {
                        //            if (teamMembers.Count == 0)
                        //                teamMembers = (List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList());
                        //            else
                        //                teamMembers.AddRange((List<TeamMembership>)(srv.TeamMembershipSet.Where(t => t.TeamId == team.Id).ToList()));
                        //        }
                        //    }
                        //    break;
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