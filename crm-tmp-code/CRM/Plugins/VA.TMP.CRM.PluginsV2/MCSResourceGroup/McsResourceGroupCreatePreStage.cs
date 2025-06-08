using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceGroupCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceGroupCreatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
