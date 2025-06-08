using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceGroupUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceGroupUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
