using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class PPEReviewCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new PPEReviewCreatePreStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}