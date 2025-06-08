using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceGroupUpdatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceGroupUpdatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
