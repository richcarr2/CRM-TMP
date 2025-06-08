using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class PPEReviewCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new PPEReviewCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}