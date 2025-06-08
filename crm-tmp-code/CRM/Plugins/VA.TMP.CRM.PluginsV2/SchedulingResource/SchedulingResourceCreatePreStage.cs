using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class SchedulingResourceCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new SchedulingResourceCreatePreStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}