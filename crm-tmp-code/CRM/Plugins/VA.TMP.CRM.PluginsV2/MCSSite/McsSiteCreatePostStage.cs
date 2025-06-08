using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class mcsSiteCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsSiteCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
