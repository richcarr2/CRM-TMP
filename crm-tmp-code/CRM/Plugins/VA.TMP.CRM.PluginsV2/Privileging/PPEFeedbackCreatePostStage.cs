using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class PPEFeedbackCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new PPEFeedbackCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}