using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class EMailCreatePostStageRunner : PluginRunner
    {
        public EMailCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        //Declare global variables
        //string customMessage;

        #region Implementation
        /// <summary>
        /// Called by PluginRunner - Decide which email to send out (aka which branch of the plugin to run)
        /// </summary>
        public override void Execute()
        {
            Logger.WriteDebugMessage("Beginning Email Create");

            Email email = (Email)OrganizationService.Retrieve(Email.EntityLogicalName.ToString(), PrimaryEntity.Id, new ColumnSet(true));

            using (var srv = new Xrm(OrganizationService))
            {
                if (email.Contains("mcs_relatedserviceactivity"))
                {
                    var serviceAppointment = srv.ServiceAppointmentSet.FirstOrDefault(x => x.Id == email.GetAttributeValue<EntityReference>("mcs_relatedserviceactivity").Id);
                    OptionSetValue apptmodality = null;

                    if (serviceAppointment == null) throw new InvalidPluginExecutionException("Service Appointment cannot be null.");
                    //Retrieve the Appointment Modality and passes sending emails if modality is set to VVC Test Call
                    if (serviceAppointment.Contains("tmp_appointmentmodality"))
                    {
                        Logger.WriteDebugMessage("Getting Appointment Modality from SA");
                        apptmodality = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
                        Logger.WriteDebugMessage(apptmodality.Value.ToString());

                        if (apptmodality.Value == 178970008)
                        {
                            Logger.WriteDebugMessage("Modality is set to VVS Test Call, Exitting");
                            return;
                        }
                    } 
                }
            }

            if (email.Subject.StartsWith("FW:") || email.Subject.StartsWith("RE:") || email.Subject.StartsWith("SKIP"))
                return;
            if (email.StatusCode.Value != (int)email_statuscode.Draft)
            {
                Logger.WriteDebugMessage("Only emails in draft status can be modified. This email is not, exiting.");
                return;
            }
            Logger.WriteDebugMessage("Starting plugin for Email with Subject: '" + email.Subject + "'");

            if (email.mcs_RelatedServiceActivity != null)
            {
                Logger.WriteDebugMessage("Related ServiceAvtivity = '" + email.mcs_RelatedServiceActivity.Id + "'");
            }

            if (email.mcs_RelatedServiceActivity != null)
            {
                Logger.WriteDebugMessage("RelatedServiceActivity is not Null");
                //Retrieve and Use relatedAppt and TSA throughout ServiceAppointment Email Execute Functions
                ServiceAppointment relatedAppt = (ServiceAppointment)OrganizationService.Retrieve(
                    ServiceAppointment.EntityLogicalName.ToString(), email.mcs_RelatedServiceActivity.Id, new ColumnSet(true));

                //sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == PrimaryEntity.Id);
                //replacing with reference to Scheduling Package - WMC 11/15/2018
                //mcs_services tsa = (mcs_services)OrganizationService.Retrieve(mcs_services.EntityLogicalName, relatedAppt.mcs_relatedtsa.Id, new ColumnSet(true));

                if (relatedAppt != null && relatedAppt.cvt_relatedschedulingpackage != null)
                {
                    Logger.WriteDebugMessage("~~~~*****  Chcking for SP before creating Email  *****~~~~");
                    cvt_resourcepackage schedulingPackage = (cvt_resourcepackage)OrganizationService.Retrieve(cvt_resourcepackage.EntityLogicalName, relatedAppt.cvt_relatedschedulingpackage.Id, new ColumnSet(true));
                    Logger.WriteDebugMessage("~~~~*****  Creating NEW SA Email  *****~~~~");

                    if (PluginExecutionContext.Depth > 3)
                    {
                        TracingService.Trace("Depth count " + PluginExecutionContext.Depth.ToString());
                        Logger.WriteDebugMessage("Depth count " + PluginExecutionContext.Depth.ToString());
                    }
                    var SAEmail = new ServiceAppointmentEmail(OrganizationService, Logger, email, relatedAppt, schedulingPackage);
                    SAEmail.Execute();

                    //if (PluginExecutionContext.Depth > 3)
                    //{
                    //    TracingService.Trace("Depth count " + PluginExecutionContext.Depth.ToString());
                    //    return;
                    //}
                    //var SAEmail = new ServiceAppointmentEmail(OrganizationService, Logger, email, relatedAppt, schedulingPackage);
                    //SAEmail.Execute();
                }
                else
                    Logger.WriteToFile("Error: Could not find Appt or related SP. Could not continue to set up the Scheduling Email.");
            }
            if (email.RegardingObjectId != null)
            {
                //replacing with reference to Scheduling Package - WMC 11/15/2018
                //if (email.RegardingObjectId.LogicalName == mcs_services.EntityLogicalName)
                if (email.RegardingObjectId.LogicalName == cvt_resourcepackage.EntityLogicalName)
                {
                    var TsaEmail = new TsaEmail(OrganizationService, Logger, email);
                    TsaEmail.Execute();
                    Logger.WriteDebugMessage("Completed Send TSA Email");
                }
                //Facility Approval Emails
                else if (email.RegardingObjectId.LogicalName == cvt_facilityapproval.EntityLogicalName || email.RegardingObjectId.LogicalName == cvt_participatingsite.EntityLogicalName)
                {
                    Logger.WriteDebugMessage("Facility Approval Emails");
                    //4.8 Enhancement - Check if this is a Yearly Review Email
                    if (email.Subject == "Facility Approval Yearly Review Initial Email" || email.Subject == "Facility Approval Yearly Review Final Warning Email" || email.Subject == "Facility Approval Yearly Review Overdue Email")
                    {
                        Logger.WriteDebugMessage("Yearly Review Email: " + email.Subject);
                        var FAYearlyReviewEmail = new FacilityApprovalYearlyReviewEmail(OrganizationService, Logger, email);
                        FAYearlyReviewEmail.Execute();
                        Logger.WriteDebugMessage("FAYearlyReviewEmail executed");
                    }
                    //Otherwise, create a normal Facility Approval Email
                    else
                    {
                        var FAEmail = new FacilityApprovalEmail(OrganizationService, Logger, email);
                        FAEmail.Execute();
                    }
                }
                //Guest Emails
                else if (email.RegardingObjectId.LogicalName == cvt_nonvaemail.EntityLogicalName)
                {
                    Logger.WriteDebugMessage("RegardingObjectId is the Guest Email");
                    var GstEmail = new GuestEmail(OrganizationService, Logger, email);
                    GstEmail.Execute();
                    Logger.WriteDebugMessage("Email Sent to Veteran's Guest");
                }
                else
                {
                    var privilegingEmail = new PrivilegingEmail(OrganizationService, Logger, email);
                    privilegingEmail.Execute();
                }
            }
            if (email.Subject == "Action Required: Update Your Provider Preferences in TMP")
            {
                Logger.WriteDebugMessage("Beginning Provider Preferences Email");
                var userEmail = new UserEmail(OrganizationService, Logger, email);
                userEmail.Execute();
            }

        }
        #endregion

        #region Debug Field for Logging
        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }
        #endregion
    }
}