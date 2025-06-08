using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class ParticipatingSiteCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ParticipatingSiteCreatePreStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}