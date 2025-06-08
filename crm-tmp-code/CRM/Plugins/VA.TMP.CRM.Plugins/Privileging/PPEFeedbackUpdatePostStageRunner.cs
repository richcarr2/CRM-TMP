using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class PPEFeedbackUpdatePostStageRunner : PluginRunner
    {
        public PPEFeedbackUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for PPE Feedback Update Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Assigns the Owner
        /// </summary>
        public override void Execute()
        {
            var ThisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, cvt_ppefeedback.EntityLogicalName, Logger, OrganizationService);

            if (ThisRecord.Contains("cvt_anythingtoreport"))
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    //Check if anythingtoreport has a value.
                    if (ThisRecord.Attributes["cvt_anythingtoreport"] == null)
                        return;

                    //get the ppefeedback record.
                    var thisPPEFeedback = srv.cvt_ppefeedbackSet.FirstOrDefault(f => f.Id == ThisRecord.Id);

                    if (thisPPEFeedback == null)
                        return;

                    if (thisPPEFeedback.cvt_ppereview == null)
                        return;

                    //get Parent PPE Review record
                    var parentPPEReview = srv.cvt_ppereviewSet.FirstOrDefault(p => p.Id == thisPPEFeedback.cvt_ppereview.Id);

                    ////Update PPE Review record totals for Submitted and Outstanding
                    //var newOutstanding = parentPPEReview.cvt_outstandingppefeedbacks - 1;
                    //var newSubmitted = parentPPEReview.cvt_submittedppefeedbacks + 1;

                    //cvt_ppereview updatePPEReview = new cvt_ppereview()
                    //{
                    //    Id = parentPPEReview.Id,
                    //    cvt_outstandingppefeedbacks = newOutstanding,
                    //    cvt_submittedppefeedbacks = newSubmitted
                    //};
                    //OrganizationService.Update(updatePPEReview);

                    //AssignRequest this record to the PPE Review owner - (Home Facility C&P Team)
                    AssignRequest assignRequest = new AssignRequest()
                    {
                        Assignee = parentPPEReview.OwnerId,
                        Target = new EntityReference(ThisRecord.LogicalName, ThisRecord.Id)
                    };

                    OrganizationService.Execute(assignRequest);
                    Logger.WriteDebugMessage("Assigned Submitted PPE Feedback record to the owner of the parent PPE Review record owner.");
                }
            }          
        }        
        
        #region Additional Interface Methods/Properties
        /// <summary>
        /// Used for Debugging - turns on or off creation of log records for this particular entity
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_ppefeedback"; }
        }
        #endregion
    }
}