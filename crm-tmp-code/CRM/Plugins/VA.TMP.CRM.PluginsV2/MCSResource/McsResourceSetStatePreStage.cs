using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceSetStatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceSetStatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

