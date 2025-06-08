using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceGroupDeletePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceGroupDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

