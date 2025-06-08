using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class EmailAutomationCreatePostStageRunner : PluginRunner
    {
        public EmailAutomationCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        //Declare global variables
        public int ClosedTimeSpan;

        #region Implementation
        /// <summary>
        /// Called by PluginRunner - Decide which email to send out (aka which branch of the plugin to run)
        /// </summary>
        public override void Execute()
        {
            var name = PrimaryEntity.Attributes["cvt_name"].ToString();
            if (name.Contains("System Automated: Inventory Notification"))
            {
                TracingService.Trace("Starting Inventory Notification automatic emails");
                InventoryNotificationSummary();
            }
            else if (name.Contains("System Automated"))
            {
                TracingService.Trace("Starting PPE Review automatic emails");
                PPEReviewSummary();
                //Should we try to send all PPEFeedbacks through this process as well?
            }
            #region 4.8 Enhancement - Yearly Review
            //Initiate Yearly Review Email
            else if (name.Contains("Facility Approval Yearly Review Initial Email"))
            {
                TracingService.Trace("Starting Yearly Review automatic emails - Initial Email");
                TracingService.Trace("Starting Yearly Review automatic emails - Initial Email");
                YearlyReviewInitial();
            }
            //Final Warning Email
            else if (name.Contains("Facility Approval Yearly Review Final Warning Email"))
            {
                TracingService.Trace("Starting Yearly Review automatic emails - Final Warning Email");
                TracingService.Trace("Starting Yearly Review automatic emails - Final Warning Email");
                YearlyReviewFinalWarning();
            }
            //Overdue Email
            else if (name.Contains("Facility Approval Yearly Review Overdue Email"))
            {
                TracingService.Trace("Starting Yearly Review automatic emails - Overdue Email");
                TracingService.Trace("Starting Yearly Review automatic emails - Overdue Email");
                YearlyReviewOverdue();
            }
            #endregion
        }

        #endregion

        #region Commonly Used Functions
        internal List<ActivityParty> RetrieveTeamMembers(Email email, Guid TeamId, out string unapprovedUsers)
        {
            TracingService.Trace("Starting RetrieveFacilityTeamMembers function");
            unapprovedUsers = "";

            using (var srv = new Xrm(OrganizationService))
            {
                var teamMembers = srv.TeamMembershipSet.Where(t => t.TeamId == TeamId).ToList();
                var recipientList = new List<ActivityParty>();

                foreach (var member in teamMembers)
                {
                    var user = srv.SystemUserSet.FirstOrDefault(u => u.Id == member.SystemUserId);

                    if (user == null)
                    {
                        TracingService.Trace("Team member is not a user.");
                        break;

                    }
                    else
                    {
                        TracingService.Trace("Checking for the User's Email Address.");
                        if ((!String.IsNullOrEmpty(user.InternalEMailAddress)))
                        {
                            var party = new ActivityParty()
                            {
                                ActivityId = new EntityReference(email.LogicalName, email.Id),
                                PartyId = new EntityReference(SystemUser.EntityLogicalName, user.Id)
                            };
                            recipientList.Add(party);
                        }
                        else
                        {
                            if (unapprovedUsers != "")
                                unapprovedUsers += "; ";

                            unapprovedUsers += user.FullName;
                        }
                    }
                }
                return recipientList;
            }
        }
        #endregion

        public int GetBusinessClosuresInPastXDays(int timespan)
        {
            var numberOfBusinessClosures = 0;
            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var calendar = srv.CalendarSet.FirstOrDefault(c => c.Name == "Business Closure Calendar");
                    if (calendar == null)
                        return 0;
                    if (calendar.CalendarRules == null)
                        srv.LoadProperty(calendar, "CalendarRules");
                    if (calendar.CalendarRules == null || calendar.CalendarRules.ToList().Count() == 0)
                        return 0;
                    var holidays = calendar.CalendarRules.Select(r => r.EffectiveIntervalEnd).Distinct().Where(endTime =>
                    { return endTime.Value.AddDays(timespan) > DateTime.Now; }
                        ).ToList();
                    numberOfBusinessClosures = holidays.Count;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(CvtHelper.BuildExceptionMessage(ex, "Unable to retrieve Holidays, assuming no holidays were within specified timeframe for \"Recently Closed\" PPE Reviews "));
            }
            return numberOfBusinessClosures;
        }

        #region PPE Review/Feedback
        internal void PPEReviewSummary()
        {
            Logger.setMethod = "FindOpenPPEReviews";
            TracingService.Trace("Starting");
            string error = "";
            int successCount = 0;

            ClosedTimeSpan = DateTime.Today.DayOfWeek == DayOfWeek.Monday ? 3 : 1;
            ClosedTimeSpan += GetBusinessClosuresInPastXDays(ClosedTimeSpan);

            DateTime compareDate = DateTime.Today.AddDays(-1 * ClosedTimeSpan);

            using (var srv = new Xrm(OrganizationService))
            {
                //Contains facilities with both open or recently closed PPE Reviews
                var activePpeFacilities = (from facility in srv.mcs_facilitySet
                                           join review in srv.cvt_ppereviewSet on facility.mcs_facilityId equals review.cvt_facility.Id
                                           join feedback in srv.cvt_ppefeedbackSet on review.cvt_ppereviewId equals feedback.cvt_ppereview.Id
                                           where review.cvt_outstandingppefeedbacks.Value > 0
                                           || (review.cvt_outstandingppefeedbacks == 0
                                               && review.ModifiedOn.Value > compareDate
                                               && feedback.cvt_anythingtoreport.Value == (int)cvt_ppefeedbackcvt_anythingtoreport.Yes)
                                           select new
                                           {
                                               review.cvt_facility,
                                               review.cvt_specialty
                                           }).Distinct().ToList();
                TracingService.Trace("Checking for Facilities with Open PPEReviews.");
                if (activePpeFacilities == null || activePpeFacilities.Count == 0)
                {
                    UpdateEmailAutomation(PrimaryEntity.Id, "No Open or Recently Closed PPE Reviews. No emails sent.", "");
                    return;
                }
                TracingService.Trace("Found Facility/Specialty Combinations: " + activePpeFacilities.Count);
                foreach (var item in activePpeFacilities)
                {
                    var team = srv.TeamSet.FirstOrDefault(t => t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && t.cvt_ServiceType.Id == item.cvt_specialty.Id && t.cvt_Facility.Id == item.cvt_facility.Id);

                    if (team == null)
                    {
                        error += String.Format("Could Not Find {0} Service Chief Team for {1}.", item.cvt_specialty.Name, item.cvt_facility.Name);
                        continue;
                    }
                    TracingService.Trace("Found SC Team at the Facility.");

                    //Create the emails
                    Email newEmail = new Email()
                    {
                        RegardingObjectId = new EntityReference(cvt_emailautomation.EntityLogicalName, PrimaryEntity.Id),
                        From = CvtHelper.GetWorkflowOwner("Privileging: PPE Submitted", OrganizationService),
                        Subject = "Daily PPE feedback tracking summary"
                    };
                    Guid newEmailID = OrganizationService.Create(newEmail);
                    TracingService.Trace("Created the email object.");

                    Email newPPEReviewSummary = new Email()
                    {
                        Id = newEmailID,
                        To = null
                    };
                    var unapprovedUsers = "";
                    newPPEReviewSummary.To = RetrieveTeamMembers(newPPEReviewSummary, team.Id, out unapprovedUsers);


                    if (unapprovedUsers != "")
                        error += $"No Email addresses for the following users {unapprovedUsers} on the team: {team.Name}.\n";
                    else
                        TracingService.Trace("All Users listed on the team have approved emails. ");

                    TracingService.Trace("Count for " + item.cvt_facility.Name + ", TO Count: " + newPPEReviewSummary.To.Count().ToString());
                    if (newPPEReviewSummary.To.Count() == 0)
                    {
                        error += String.Format("No Team members for {0}", team.Name);
                        continue;
                    }

                    //Edit the E-mail body with the summary grid
                    var etc = CvtHelper.GetEntityTypeCode(OrganizationService, cvt_ppereview.EntityLogicalName);
                    string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
                    string url = servernameAndOrgname + "/userDefined/edit.aspx?etc=" + etc + "&id=";

                    TracingService.Trace("Building the email body.");
                    var openPPETable = BuildOpenPpeReviewTable(item.cvt_facility, item.cvt_specialty, url, srv);
                    var recentlyClosedPPETable = BuildRecentlyClosedPPEReviewTable(item.cvt_facility, item.cvt_specialty, url, srv);

                    var customMessage = !string.IsNullOrEmpty(openPPETable) ? "<b>Active PPE Reviews at " + item.cvt_facility.Name + "</b><br/>" + openPPETable + "<br/><br/>" : "No Active PPE Reviews at " + item.cvt_facility.Name;
                    customMessage += !string.IsNullOrEmpty(recentlyClosedPPETable) ? "<b>Recently Closed PPE Reviews with Something to Report</b><br/>" + recentlyClosedPPETable : "No Recently Closed PPE Reviews with Anything to Report";
                    customMessage += "<br/><br/>This is an automated notification from the Telehealth Management Platform.";
                    newPPEReviewSummary.Description = customMessage;

                    //Only Send email if there are either recently closed or currently open PPE Reviews
                    if (!string.IsNullOrEmpty(openPPETable + recentlyClosedPPETable))
                    {
                        CvtHelper.UpdateSendEmail(newPPEReviewSummary, OrganizationService, Logger);
                        successCount++;
                    }
                }

                string summary = "Successfully sent out " + successCount + " PPE Review summaries.";
                //Update the Email Automation record
                UpdateEmailAutomation(PrimaryEntity.Id, summary, error);

            }
        }

        internal string BuildOpenPpeReviewTable(EntityReference facility, EntityReference specialty, string url, Xrm srv)
        {
            var tableString = string.Empty;
            TracingService.Trace("Building the table string for open PPE Reviews.");
            tableString = "<table style = 'width:100%'><tr>";
            //Table Headings
            tableString += HtmlTableHeaderItem("Provider Name");
            tableString += HtmlTableHeaderItem("# of Received Feedback");
            tableString += HtmlTableHeaderItem("# of Requested Feedback");
            tableString += HtmlTableHeaderItem("Initiation Date");
            tableString += HtmlTableHeaderItem("Due Date");
            tableString += HtmlTableHeaderItem("Requests Escalated");
            tableString += HtmlTableHeaderItem("PPE Review record");

            tableString += "</tr >";

            var openReviews = srv.cvt_ppereviewSet.Where(r =>
                    r.cvt_facility.Id == facility.Id &&
                    r.cvt_specialty.Id == specialty.Id &&
                    r.cvt_outstandingppefeedbacks != 0 &&
                    r.cvt_outstandingppefeedbacks != null &&
                    r.cvt_submittedppefeedbacks != null).ToList();
            foreach (cvt_ppereview open in openReviews)
            {
                var escalated = (open.cvt_escalated.Value == true) ? "Yes" : "No";
                tableString += "<tr>";
                tableString += createHtmlTableDataElement(open.cvt_provider.Name);
                tableString += createHtmlTableDataElement(open.cvt_submittedppefeedbacks.ToString());
                tableString += createHtmlTableDataElement(open.cvt_requestedppefeedbacks.ToString());
                tableString += createHtmlTableDataElement(open.cvt_initiateddate.Value.ToString("MM/dd/yyyy"));
                tableString += createHtmlTableDataElement(open.cvt_duedate.Value.ToString("MM/dd/yyyy"));
                tableString += createHtmlTableDataElement(escalated);
                tableString += createHtmlTableDataElement("<a href=\"" + url + open.Id + "\">View Record</a>"); //clickable URL
                tableString += "</tr>";
            }

            tableString += "</table >";
            if (openReviews.Count == 0)
            {
                TracingService.Trace("No Open PPE Reviews Found, skipping 'Active PPE Reviews' table");
                return string.Empty;
            }
            else
                return tableString;
        }

        internal string BuildRecentlyClosedPPEReviewTable(EntityReference facility, EntityReference specialty, string url, Xrm srv)
        {
            TracingService.Trace("Building Closed PPE Review Table");

            DateTime timeBarrier = DateTime.Today.AddDays(-ClosedTimeSpan);

            var recentlyClosedReviews = (from review in srv.cvt_ppereviewSet
                                         join feedback in srv.cvt_ppefeedbackSet on review.Id equals feedback.cvt_ppereview.Id
                                         where
                                             review.cvt_facility.Id == facility.Id &&
                                             review.cvt_specialty.Id == specialty.Id &&
                                             review.cvt_submittedppefeedbacks != null &&
                                             review.cvt_outstandingppefeedbacks != null &&
                                             review.cvt_outstandingppefeedbacks == 0 &&
                                             review.ModifiedOn.Value > timeBarrier &&
                                             feedback.cvt_anythingtoreport.Value == (int)cvt_ppefeedbackcvt_anythingtoreport.Yes
                                         select review).Distinct().ToList();

            if (recentlyClosedReviews == null || recentlyClosedReviews.Count == 0)
            {
                TracingService.Trace("No Recently Closed PPE Reviews Reporting Feedback Found, skipping 'Recently Closed PPE Reviews' table");
                return string.Empty;
            }

            TracingService.Trace("Finished Retrieving Closed PPE Review dataset.");

            var tableString = "<table style = 'width:100%'><tr>";
            //Table Headings
            tableString += HtmlTableHeaderItem("Provider Name");
            tableString += HtmlTableHeaderItem("# of Received Feedback");
            tableString += HtmlTableHeaderItem("Initiation Date");
            tableString += HtmlTableHeaderItem("Due Date");
            tableString += HtmlTableHeaderItem("Requests Escalated");
            tableString += HtmlTableHeaderItem("Closed Date");
            tableString += HtmlTableHeaderItem("PPE Review record");
            tableString += "</tr >";
            foreach (var review in recentlyClosedReviews)
            {
                var escalated = (review.cvt_escalated.Value == true) ? "Yes" : "No";
                tableString += "<tr>";
                tableString += createHtmlTableDataElement(review.cvt_provider.Name);
                tableString += createHtmlTableDataElement(review.cvt_submittedppefeedbacks.ToString());
                tableString += createHtmlTableDataElement(review.cvt_initiateddate.Value.ToString("MM/dd/yyyy"));
                tableString += createHtmlTableDataElement(review.cvt_duedate.Value.ToString("MM/dd/yyyy"));
                tableString += createHtmlTableDataElement(escalated);
                tableString += createHtmlTableDataElement(review.ModifiedOn.Value.ToString("MM/dd/yyyy"));
                tableString += createHtmlTableDataElement("<a href=\"" + url + review.Id + "\">View Record</a>");
                tableString += "</tr>";
            }
            tableString += "</table>";
            return tableString;
        }

        internal string HtmlTableHeaderItem(string element)
        {
            return "<th>" + element + "</th>";
        }

        /// <summary>
        /// Using Existing Formatting from V1
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal string createHtmlTableDataElement(string element)
        {
            //Using existing formatting from V1 - commented out what would normally be expected for Table Data Elements
            return HtmlTableHeaderItem(element);
            //return "<td>" + element + "</td>";
        }

        internal void UpdateEmailAutomation(Guid recordId, string summary, string error)
        {
            TracingService.Trace("About to update the Email Automation record.");

            var name = PrimaryEntity.Attributes["cvt_name"].ToString();
            var now = DateTime.Today;

            cvt_emailautomation updateRecord = new cvt_emailautomation()
            {
                Id = recordId,
                cvt_summary = summary,
                cvt_errors = error,
                cvt_name = name + " Emails " + now.ToString("yyyy/MM/dd")
            };
            OrganizationService.Update(updateRecord);
            TracingService.Trace("Updated Email Automation record.");
        }

        #endregion

        #region Inventory Notification
        internal void InventoryNotificationSummary()
        {
            Logger.setMethod = "InventoryNotificationSummary";
            TracingService.Trace("Starting");
            var error = string.Empty;
            string summary = string.Empty;

            using (var srv = new Xrm(OrganizationService))
            {
                GenerateFacilityEmailNotification(srv, ref error, ref summary);

                GenerateSiteEmailNotification(srv, ref error, ref summary);

                //Update the Email Automation record
                UpdateEmailAutomation(PrimaryEntity.Id, summary, error);

            }
        }

        private void GenerateFacilityEmailNotification(Xrm srv, ref string error, ref string summary)
        {
            Logger.setMethod = "GenerateFacilityEmailNotification";
            TracingService.Trace("Starting");
            try
            {
                var successCount = 0;

                //Contains facilities without sites
                var activeInventoryFacilities = (from facility in srv.mcs_facilitySet
                                                 join inventory in srv.cvt_stagingresourceSet on facility.mcs_facilityId equals inventory.mcs_Facility.Id
                                                 where inventory.mcs_RelatedSiteId.Id == null && inventory.statecode == (int)cvt_stagingresourceState.Active
                                                 select new
                                                 {
                                                     inventory.mcs_Facility
                                                 }).Distinct().ToList();

                TracingService.Trace("Checking for Facilities Staging Records Needing Site.");
                if (activeInventoryFacilities.Count == 0)
                {
                    UpdateEmailAutomation(PrimaryEntity.Id, "No Active Staging Records Needing Site. No emails sent.", string.Empty);
                    return;
                }
                TracingService.Trace($"Found {activeInventoryFacilities.Count} Active Staging Records Needing Site.");
                var facilities = string.Empty;

                foreach (var item in activeInventoryFacilities)
                {
                    var team = srv.TeamSet.FirstOrDefault(
                        t =>
                            t.cvt_Facility.Id == item.mcs_Facility.Id && t.cvt_Type != null &&
                            t.cvt_Type.Value == (int)Teamcvt_Type.FTC);

                    if (team == null)
                    {
                        error +=
                            $"FTC Team not found for Facility: {item.mcs_Facility.Name}.\n";
                    }
                    else
                    {
                        TracingService.Trace($"Found FTC Team {team.Name} at the Facility {item.mcs_Facility.Name}");

                        //Create the emails
                        var newEmail = new Email()
                        {
                            RegardingObjectId = new EntityReference(cvt_emailautomation.EntityLogicalName, PrimaryEntity.Id),
                            From = CvtHelper.GetWorkflowOwner("Email Automation: Inventory Notification", OrganizationService),
                            Subject = "Pending TMP Data Validation At Your Facility"
                        };

                        if (newEmail.From == null)
                        {
                            Logger.WriteToFile("Workflow process with name: 'Email Automation: Inventory Notification' is not available\n Hence aborting the email generation process");
                            break;
                        }
                        var newEmailId = OrganizationService.Create(newEmail);
                        TracingService.Trace("Created the email object.");

                        Email newInventoryNotificationSummary = new Email()
                        {
                            Id = newEmailId,
                            To = null
                        };
                        string unapprovedUsers;
                        newInventoryNotificationSummary.To = RetrieveTeamMembers(newInventoryNotificationSummary, team.Id,
                            out unapprovedUsers);


                        if (unapprovedUsers != "")
                            error += $"No Email addresses for the following users {unapprovedUsers} on the team: {team.Name}.\n";
                        else
                            TracingService.Trace("All Users listed on the team have approved emails.");

                        TracingService.Trace("Count for " + item.mcs_Facility.Name + ", TO Count: " +
                                                 newInventoryNotificationSummary.To.Count().ToString());

                        if (!newInventoryNotificationSummary.To.Any())
                        {
                            error += $"No Team members for {team.Name}\n";
                        }
                        else
                        {

                            //Edit the E-mail body with the summary grid
                            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, cvt_stagingresource.EntityLogicalName);
                            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
                            string url = servernameAndOrgname +
                                         $"main.aspx?etc={etc}&extraqs=%3fpagemode%3diframe%26sitemappath%3dResources%257cExtensions%257ccvt_stagingresource&pagetype=entitylist&viewid=%7bB7DA4B94-BDD3-E711-80E5-000D3A007BF6%7d&viewtype=1039";

                            var resource = srv.cvt_stagingresourceSet.Where(inv => inv.mcs_Facility.Id == item.mcs_Facility.Id &&
                                                                                        inv.statecode ==
                                                                                        (int)cvt_stagingresourceState.Active &&
                                                                                        inv.mcs_RelatedSiteId.Id == null).ToList();
                            TracingService.Trace(
                                $"Resource count needing site for the facility {item.mcs_Facility.Name} is {resource.Count}");

                            //Only Send email if there are one or more resources requiring site specification
                            if (resource.Count > 0)
                            {
                                TracingService.Trace("Building the email body.");
                                var customMessage =
                                    $"There are currently {resource.Count} resource staging records that require a site specification at your facility. " +
                                    "Please follow the link below to select the correct site for the staging record.<br/><br/>" +
                                    $"<a href='{url}'>{url}</a><br/><br/>This is an automated notification from the Telehealth Management Platform. You are receiving this email as you are a part of the team : {team.Name}.";
                                newInventoryNotificationSummary.Description = customMessage;

                                try
                                {
                                    CvtHelper.UpdateSendEmail(newInventoryNotificationSummary, OrganizationService, Logger);
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteToFile($"Error occured while sending the email {ex.Message}");
                                }

                                facilities += item.mcs_Facility.Name + ", ";
                                successCount++;
                            }
                        }
                    }
                }

                summary += $"Successfully sent out {successCount} Inventory Notifications summaries for the facilities {facilities}\n";
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"Error occured while generating Facility email notification\n{ex.Message}");
            }
        }

        private void GenerateSiteEmailNotification(Xrm srv, ref string error, ref string summary)
        {
            Logger.setMethod = "GenerateSiteEmailNotification";
            TracingService.Trace("Starting");
            var successCount = 0;
            try
            {
                //Contains sites thet needs validation and approval
                var activeInventorySites = (from site in srv.mcs_siteSet
                                            join inventory in srv.cvt_stagingresourceSet on site.mcs_siteId.Value equals inventory.mcs_RelatedSiteId.Id
                                            where inventory.cvt_approvalstatus.Value != (int)cvt_approvalstatus.Approved && inventory.statecode == (int)cvt_stagingresourceState.Active
                                            select new
                                            {
                                                inventory.mcs_RelatedSiteId
                                            }).Distinct().ToList();

                TracingService.Trace("Checking for Sites Staging Records Needing validation.");
                if (activeInventorySites.Count == 0)
                {
                    UpdateEmailAutomation(PrimaryEntity.Id, "No Active Staging Records Needing validation. No emails sent.", string.Empty);
                    return;
                }
                TracingService.Trace($"Found {activeInventorySites.Count} Active Staging Records needing validation.");
                var sites = string.Empty;

                foreach (var item in activeInventorySites)
                {
                    var team = srv.TeamSet.FirstOrDefault(
                                            t =>
                                                t.cvt_TMPSite.Id == item.mcs_RelatedSiteId.Id && t.cvt_Type != null &&
                                                t.cvt_Type.Value == (int)Teamcvt_Type.Staff);

                    if (team == null)
                    {
                        error +=
                            $"TCT Team not found for Site: {item.mcs_RelatedSiteId.Name}.\n";
                    }
                    else
                    {
                        TracingService.Trace($"Found TCT Team {team.Name} at the Site {item.mcs_RelatedSiteId.Name}");

                        //Create the emails
                        var newEmail = new Email()
                        {
                            RegardingObjectId = new EntityReference(cvt_emailautomation.EntityLogicalName, PrimaryEntity.Id),
                            From = CvtHelper.GetWorkflowOwner("Email Automation: Inventory Notification", OrganizationService),
                            Subject = "Pending TMP Data Validation at your Site"
                        };
                        if (newEmail.From == null)
                        {
                            Logger.WriteToFile("Workflow process with name: 'Email Automation: Inventory Notification' is not available\n Hence aborting the email generation process");
                            break;
                        }

                        var newEmailId = OrganizationService.Create(newEmail);
                        TracingService.Trace("Created the email object.");

                        var newInventoryNotificationSummary = new Email()
                        {
                            Id = newEmailId,
                            To = null
                        };
                        string unapprovedUsers;
                        newInventoryNotificationSummary.To = RetrieveTeamMembers(newInventoryNotificationSummary, team.Id,
                            out unapprovedUsers);


                        if (unapprovedUsers != "")
                            error += $"No Email addresses for the following users {unapprovedUsers} on the team: {team.Name}.\n";
                        else
                            TracingService.Trace("All Users listed on the team have approved emails.");

                        TracingService.Trace($"Count for {item.mcs_RelatedSiteId.Name}, TO Count: {newInventoryNotificationSummary.To.Count()}");

                        if (!newInventoryNotificationSummary.To.Any())
                        {
                            error += $"No Team members for {team.Name}\n";
                        }
                        else
                        {
                            //Edit the E-mail body with the summary grid
                            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, cvt_stagingresource.EntityLogicalName);
                            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
                            string url = servernameAndOrgname +
                                         $"main.aspx?etc={etc}&extraqs=%3fpagemode%3diframe%26sitemappath%3dResources%257cExtensions%257ccvt_stagingresource&pagetype=entitylist&viewid=%7b3153F55F-1DC3-E711-80E5-000D3A007BF6%7d&viewtype=1039";

                            var resource = srv.cvt_stagingresourceSet.Where(inv => inv.mcs_RelatedSiteId.Id == item.mcs_RelatedSiteId.Id &&
                                                                                        inv.statecode ==
                                                                                        (int)cvt_stagingresourceState.Active &&
                                                                                        inv.cvt_approvalstatus.Value != (int)cvt_approvalstatus.Approved).ToList();
                            TracingService.Trace(
                                $"Resource count needing validation for the site {item.mcs_RelatedSiteId.Name} is {resource.Count}");

                            //Only Send email if there are one or more resources requiring data validation
                            if (resource.Count > 0)
                            {
                                TracingService.Trace("Building the email body.");
                                var customMessage =
                                    $"There are currently {resource.Count} resource staging records that require data validation at your site. " +
                                    "Please follow the link below to validate the data in the record.<br/><br/>" +
                                    $"<a href='{url}'>{url}</a><br/><br/>This is an automated notification from the Telehealth Management Platform. You are receiving this email as you are a part of the team : {team.Name}.";
                                newInventoryNotificationSummary.Description = customMessage;

                                try
                                {
                                    CvtHelper.UpdateSendEmail(newInventoryNotificationSummary, OrganizationService, Logger);
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteToFile($"Error occured while sending the email {ex.Message}");
                                }

                                sites += item.mcs_RelatedSiteId.Name + ", ";
                                successCount++;
                            }
                        }
                    }
                }

                summary += $"Successfully sent out {successCount} Inventory Notifications summaries for the sites {sites}\n";
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"Error occured while generating Inventory Site email notification\n{ex.Message}");
            }
        }


        #endregion

        #region Implementing additional interface methods
        public override string McsSettingsDebugField
        {
            get { return "cvt_ppereview"; }
        }
        #endregion

        #region Yearly Review Notifications
        internal void YearlyReviewInitial()
        {
            Logger.setMethod = "YearlyReviewIntial";
            TracingService.Trace("Begin Yearly Review - Initial");
            DateTime initalReviewDate = DateTime.Today;
            DateTime reviewDueDate = initalReviewDate.AddDays(60);

            //Retrieve TSA records where reviewduedate = today + 60 days
            EntityCollection retrievedTSAs = RetrieveTSAs(reviewDueDate);

            // For each retrieved TSA, send email
            for (int i = 0; i < retrievedTSAs.Entities.Count; i++)
            {
                TracingService.Trace("Initial - For each TSA");
                Entity retrievedTSA = OrganizationService.Retrieve("cvt_facilityapproval", retrievedTSAs.Entities[i].Id, new ColumnSet("cvt_approvalstatushubdirector", "cvt_approvalstatuspatientftc", "cvt_approvalstatusproviderftc", "statuscode", "cvt_hubfacility"));

                //If Hub Director Approval Status is Approve, Reviewed and Confirmed, or Reviewed and Updated
                if (retrievedTSA.Attributes.Contains("cvt_hubfacility"))
                {
                    TracingService.Trace("Initial - Send email for Hub");
                    //Set hub director to review pending;
                    retrievedTSA["cvt_approvalstatushubdirector"] = new OptionSetValue(803750000);
                    OrganizationService.Update(retrievedTSA);

                    //Create Email for Hub Director
                    Email hubDirectorEmail = new Email()
                    {
                        Subject = "Facility Approval Yearly Review Initial Email",
                        Description = "Hub Director",
                        RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                    };
                    OrganizationService.Create(hubDirectorEmail);
                    TracingService.Trace("Initial Yearly Review Email created for Hub Director");
                }
                else
                {
                    TracingService.Trace("Initial - Send email for FTC");
                    //Set FTC Approval Statuses to review pending (803750000)
                    retrievedTSA["cvt_approvalstatuspatientftc"] = new OptionSetValue(803750000);
                    retrievedTSA["cvt_approvalstatusproviderftc"] = new OptionSetValue(803750000);
                    OrganizationService.Update(retrievedTSA);

                    //Create Email for FTC PAT
                    Email patFTCEmail = new Email()
                    {
                        Subject = "Facility Approval Yearly Review Initial Email",
                        Description = "Patient",
                        RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                    };
                    OrganizationService.Create(patFTCEmail);
                    TracingService.Trace("Initial Yearly Review Email created for Patient FTC");

                    //Create Email for FTC PRO
                    Email proFTCEmail = new Email()
                    {
                        Subject = "Facility Approval Yearly Review Initial Email",
                        Description = "Provider",
                        RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                    };
                    OrganizationService.Create(proFTCEmail);
                    TracingService.Trace("Initial Yearly Review Email created for Provider FTC");
                }
            }
            TracingService.Trace("Finished Yearly Review - Initial");

        }

        internal void YearlyReviewFinalWarning()
        {
            Logger.setMethod = "YearlyReviewFinalWarning";
            TracingService.Trace("Begin Yearly Review - Final Warning");
            DateTime finalWarningDate = DateTime.Today;
            DateTime reviewDueDate = finalWarningDate.AddDays(7);

            //Retrieve TSA records where reviewduedate = today + 7 days
            EntityCollection retrievedTSAs = RetrieveTSAs(reviewDueDate);

            //For each retrieved TSA, send email
            for (int i = 0; i < retrievedTSAs.Entities.Count; i++)
            {
                TracingService.Trace("Final - For each TSA");
                Entity retrievedTSA = OrganizationService.Retrieve("cvt_facilityapproval", retrievedTSAs.Entities[i].Id, new ColumnSet("cvt_approvalstatushubdirector", "cvt_approvalstatuspatientftc", "cvt_approvalstatusproviderftc", "statuscode", "cvt_hubfacility"));

                //If Hub Director Approval Status is Review Pending
                if (retrievedTSA.Attributes.Contains("cvt_hubfacility"))
                {
                    TracingService.Trace("Check if Hub TSA is Final Warning");
                    if (retrievedTSA.Attributes.Contains("cvt_approvalstatushubdirector") && ((OptionSetValue)retrievedTSA["cvt_approvalstatushubdirector"]).Value == 803750000)
                    {
                        //Create Email for Hub Director
                        Email hubDirectorEmail = new Email()
                        {
                            Subject = "Facility Approval Yearly Review Final Warning Email",
                            Description = "Hub Director",
                            RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                        };
                        OrganizationService.Create(hubDirectorEmail);
                        TracingService.Trace("Facility Approval Yearly Review Final Warning Email created for Hub Director");
                    }
                }
                else
                {
                    TracingService.Trace("Check if FTC TSA is Final Warning");
                    //Send Email to Patient FTC if Review Pending
                    if (retrievedTSA.Attributes.Contains("cvt_approvalstatuspatientftc") && ((OptionSetValue)retrievedTSA["cvt_approvalstatuspatientftc"]).Value == 803750000)
                    {
                        TracingService.Trace("Final - Send email for FTC - Patient");
                        //Create Email for FTC PAT
                        Email patFTCEmail = new Email()
                        {
                            Subject = "Facility Approval Yearly Review Final Warning Email",
                            Description = "Patient",
                            RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                        };
                        OrganizationService.Create(patFTCEmail);
                        TracingService.Trace("Facility Approval Yearly Review Final Warning Email created for Patient FTC");
                    }

                    //Send Email to Provider FTC if Review Pending
                    if (retrievedTSA.Attributes.Contains("cvt_approvalstatusproviderftc") && ((OptionSetValue)retrievedTSA["cvt_approvalstatusproviderftc"]).Value == 803750000)
                    {
                        TracingService.Trace("Final - Send email for FTC - Provider");
                        //Create Email for FTC PRO
                        Email proFTCEmail = new Email()
                        {
                            Subject = "Facility Approval Yearly Review Final Warning Email",
                            Description = "Provider",
                            RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                        };
                        OrganizationService.Create(proFTCEmail);
                        TracingService.Trace("Facility Approval Yearly Review Final Warning Email created for Provider FTC");
                    }

                }
            }
            TracingService.Trace("Begin Yearly Review - Final Warning");
        }

        internal void YearlyReviewOverdue()
        {
            Logger.setMethod = "YearlyReviewOverdue";
            TracingService.Trace("Begin Yearly Review - Overdue");
            DateTime overdueDate = DateTime.Today;
            DateTime reviewDueDate = overdueDate.AddDays(-1);

            //Retrieve TSA records where reviewduedate = today - 1 day (overdue)
            EntityCollection retrievedTSAs = RetrieveTSAs(reviewDueDate);

            // For each retrieved TSA, send email
            for (int i = 0; i < retrievedTSAs.Entities.Count; i++)
            {
                TracingService.Trace("Overdue - For each TSA");
                Entity retrievedTSA = OrganizationService.Retrieve("cvt_facilityapproval", retrievedTSAs.Entities[i].Id, new ColumnSet("cvt_approvalstatushubdirector", "cvt_approvalstatuspatientftc", "cvt_approvalstatusproviderftc", "cvt_hubfacility"));

                //If Hub Director Approval Status is Review Pending
                if (retrievedTSA.Attributes.Contains("cvt_hubfacility"))
                {
                    TracingService.Trace("Check if Hub is overdue");
                    if (retrievedTSA.Attributes.Contains("cvt_approvalstatushubdirector") && ((OptionSetValue)retrievedTSA["cvt_approvalstatushubdirector"]).Value == 803750000) { 
                            //Deactivate and set TSA to expire
                            UpdateFacilityApprovalStatus(retrievedTSAs.Entities[i].Id);

                            //Create Email for Hub Director
                            Email hubDirectorEmail = new Email()
                            {
                                Subject = "Facility Approval Yearly Review Overdue Email",
                                Description = "Hub Director",
                                RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                            };
                            OrganizationService.Create(hubDirectorEmail);
                            TracingService.Trace("Facility Approval Yearly Review Overdue Email created for Hub Director");
                        
                    }
                }
                else
                {
                    TracingService.Trace("Check if FTC is overdue");

                    //Send Email to Patient FTC if Review Pending
                    if (retrievedTSA.Attributes.Contains("cvt_approvalstatuspatientftc") && ((OptionSetValue)retrievedTSA["cvt_approvalstatuspatientftc"]).Value == 803750000)
                    {
                        TracingService.Trace("Overdue - Send email for FTC - Patient");
                        //Deactivate and set TSA to expire
                        UpdateFacilityApprovalStatus(retrievedTSAs.Entities[i].Id);

                        //Create Email for FTC PAT
                        Email patFTCEmail = new Email()
                        {
                            Subject = "Facility Approval Yearly Review Overdue Email",
                            Description = "Patient",
                            RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                        };
                        OrganizationService.Create(patFTCEmail);
                        TracingService.Trace("Facility Approval Yearly Review Overdue Email created for Patient FTC");
                    }

                    //Send Email to Provider FTC if Review Pending
                    if (retrievedTSA.Attributes.Contains("cvt_approvalstatusproviderftc") && ((OptionSetValue)retrievedTSA["cvt_approvalstatusproviderftc"]).Value == 803750000)
                    {
                        TracingService.Trace("Overdue - Send email for FTC - Provider");
                        //Deactivate and set TSA to expire
                        UpdateFacilityApprovalStatus(retrievedTSAs.Entities[i].Id);

                        //Create Email for FTC PRO
                        Email proFTCEmail = new Email()
                        {
                            Subject = "Facility Approval Yearly Review Overdue Email",
                            Description = "Provider",
                            RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, retrievedTSAs.Entities[i].Id)
                        };
                        OrganizationService.Create(proFTCEmail);
                        TracingService.Trace("Facility Approval Yearly Review Overdue Email created for Provider FTC");
                    }
                }
            }

            TracingService.Trace("Finished Yearly Review - Overdue");

        }

        internal EntityCollection RetrieveTSAs(DateTime reviewDueDate)
        {
            TracingService.Trace("Begin RetrieveTSAs");
            //Create Query Expression.

            TimeSpan oneDay = new TimeSpan(0, 23, 59, 59);
            DateTime dateBegin = reviewDueDate;
            DateTime dateEnd = dateBegin + oneDay;

            QueryExpression query = new QueryExpression("cvt_facilityapproval") { ColumnSet = new ColumnSet("cvt_reviewduedate", "statuscode") };
            query.Criteria.AddCondition(new ConditionExpression("cvt_reviewduedate", ConditionOperator.GreaterEqual, dateBegin));
            query.Criteria.AddCondition(new ConditionExpression("cvt_reviewduedate", ConditionOperator.LessEqual, dateEnd));
            query.Criteria.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 917290000));

            // Obtain results from the query expression.
            EntityCollection retrievedTSAs = OrganizationService.RetrieveMultiple(query);
            TracingService.Trace("RetrieveTSAs");
            TracingService.Trace("End RetrieveTSAs");
            return retrievedTSAs;
        }

        //Set TSA to Expired and Deactivate if Overdue
        internal void UpdateFacilityApprovalStatus(Guid facilityApprovalID)
        {
            //Change State of FA
            SetStateRequest changeFA = new SetStateRequest()
            {
                EntityMoniker = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalID),
                State = new OptionSetValue((int)cvt_facilityapprovalState.Inactive),
                Status = new OptionSetValue((int)cvt_facilityapproval_statuscode.Expired)
            };

            OrganizationService.Execute(changeFA);
        }

        #endregion
    }
}
