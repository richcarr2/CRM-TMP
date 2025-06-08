using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class PPEReviewCreatePostStageRunner : PluginRunner
    {
        public PPEReviewCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for PPE Review Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Assigns the Owner
        /// </summary>
        public override void Execute()
        {
            var ThisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, cvt_ppereview.EntityLogicalName, Logger, OrganizationService);
            using (var srv = new Xrm(OrganizationService))
            {
                //Query for this PPE Review record
                var thisReview = srv.cvt_ppereviewSet.FirstOrDefault(r => r.Id == ThisRecord.Id);
                if (thisReview == null)
                    return;

                //Query for all Proxy Privs related to this Home Priv
                var proxyPrivs = srv.cvt_tssprivilegingSet.Where(t => t.cvt_ReferencedPrivilegeId.Id == thisReview.cvt_telehealthprivileging.Id);
                if (proxyPrivs == null)
                    return;

                //Create PPE Feedback record for each Proxy Priv
                var initDate = (DateTime)thisReview.CreatedOn;
                var count = 0;
                foreach (cvt_tssprivileging proxy in proxyPrivs)
                {
                    //To get the owner
                    var proxySCTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == proxy.cvt_PrivilegedAtId.Id && t.cvt_Type.Value == 917290001 && t.cvt_ServiceType.Id == proxy.cvt_ServiceTypeId.Id);

                    //Create PPE Feedback record
                    cvt_ppefeedback newFeedback = new cvt_ppefeedback()
                    {
                        cvt_facility = proxy.cvt_PrivilegedAtId,
                        cvt_name = proxy.cvt_name + " Feedback (" + initDate.ToString("yyyy/MM/dd") + ")",
                        cvt_nextemail = thisReview.cvt_nextemail,
                        cvt_ppereview = new EntityReference(cvt_ppereview.EntityLogicalName, thisReview.Id),
                        cvt_proxyprivileging = new EntityReference(cvt_tssprivileging.EntityLogicalName, proxy.Id),
                        OwnerId = (proxySCTeam != null) ? new EntityReference(Team.EntityLogicalName, proxySCTeam.Id) : proxy.OwnerId
                    };
                    OrganizationService.Create(newFeedback);
                    Logger.WriteDebugMessage("Created a new feedback.");
                    count++;
                }

                //Update this PPE Review record with Requested and Outstanding #s
                cvt_ppereview updatePPEReview = new cvt_ppereview()
                {
                    Id = thisReview.Id,
                    cvt_outstandingppefeedbacks = count,
                    cvt_requestedppefeedbacks = count,
                    cvt_submittedppefeedbacks = 0
                };
                OrganizationService.Update(updatePPEReview);

                //Assign this record to the Priv Facility SC Team
                CvtHelper.AssignOwner(ThisRecord, Logger, OrganizationService);
                Logger.WriteDebugMessage("Assigned PPE Review record to the owner of the Home Privilege.");
            }
        }        
        
        #region Additional Interface Methods/Properties
        /// <summary>
        /// Used for Debugging - turns on or off creation of log records for this particular entity
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_ppereview"; }
        }
        #endregion
    }
}