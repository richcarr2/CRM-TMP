using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsSiteDeletePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsSiteDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

