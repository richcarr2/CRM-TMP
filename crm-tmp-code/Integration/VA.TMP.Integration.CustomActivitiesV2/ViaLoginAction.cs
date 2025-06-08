using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace VA.TMP.Integration.CustomActivities
{
    public sealed class VIALoginAction : CodeActivity
    {
        [Input("StationCode")]
        public InArgument<string> StationNumber { get; set; }

        [Input("SamlToken")]
        public InArgument<string> SamlToken { get; set; }

        [Input("AccessCode")]
        public InArgument<string> AccessCode { get; set; }

        [Input("VerifyCode")]
        public InArgument<string> VerifyCode { get; set; }

        [Output("UserDuz")]
        public OutArgument<string> UserDuz { get; set; }

        [Output("ErrorMessage")]
        public OutArgument<string> ErrorMessage { get; set; }

        [Output("SuccessfulLogin")]
        public OutArgument<bool> SuccessfulLogin { get; set; }

        public int VimtTimeout { get; set; }

        public string VimtUrl { get; set; }

        public string ErrorTracker { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            var runner = new ViaLoginActionRunner(executionContext);
            var error = "";
            try
            {
                var inputCollection = new AttributeCollection
                {
                    { "StationNumber", StationNumber.Get(executionContext) },
                    { "SamlToken", SamlToken.Get(executionContext) },
                    { "AccessCode", AccessCode.Get(executionContext) },
                    { "VerifyCode", VerifyCode.Get(executionContext) }
                };

                runner.RunCustomAction(inputCollection);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                executionContext.GetExtension<ITracingService>().Trace("Unable to handle Exception in Custom Action: " + error);
            }
            finally
            {
                SuccessfulLogin.Set(executionContext, runner.SuccessfulLogin);
                if (runner.SuccessfulLogin)
                    UserDuz.Set(executionContext, runner.UserDuz);
                else
                {
                    error = string.IsNullOrEmpty(error) ? runner.ErrorMessage : runner.ErrorMessage + " | " + error;
                    ErrorMessage.Set(executionContext, error);
                }
            }
        }
    }
}
