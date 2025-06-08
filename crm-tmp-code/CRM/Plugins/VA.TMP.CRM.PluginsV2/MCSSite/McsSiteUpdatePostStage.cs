using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsSiteUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsSiteUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
