using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceGroupCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceGroupCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
