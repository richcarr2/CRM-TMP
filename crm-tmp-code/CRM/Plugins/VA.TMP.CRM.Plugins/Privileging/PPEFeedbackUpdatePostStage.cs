using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class PPEFeedbackUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new PPEFeedbackUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}