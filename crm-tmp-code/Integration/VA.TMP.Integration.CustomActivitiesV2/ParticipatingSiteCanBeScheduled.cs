using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace VA.TMP.Integration.CustomActivities
{
    public sealed class ParticipatingSiteCanBeScheduled : CodeActivity
    {
        [Input("ParticipatingSite")]
        [ReferenceTarget("cvt_participatingsite")]
        public InArgument<EntityReference> ParticipatingSite { get; set; }

        [Output("ErrorMessage")]
        public OutArgument<string> ErrorMessage { get; set; }

        public string ErrorTracker { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            var runner = new ParticipatingSiteCanBeScheduledRunner(executionContext);
            var error = "";
            try
            {
                var ps = ParticipatingSite.Get<EntityReference>(executionContext);
                var inputCollection = new AttributeCollection();

                if (ps != null)
                {
                    inputCollection.Add(new System.Collections.Generic.KeyValuePair<string, object>("ParticipatingSiteId", ps.Id));
                    runner.RunCustomAction(inputCollection);
                }
                else
                {
                    ErrorMessage.Set(executionContext, "Participating Site is missing");
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                executionContext.GetExtension<ITracingService>().Trace("Unable to handle Exception in Custom Action: " + error);
            }
            finally
            {
                error = string.IsNullOrEmpty(error) ? runner.ErrorMessage : runner.ErrorMessage + " | " + error;
                ErrorMessage.Set(executionContext, error);
            }
        }
    }
}
