using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceDeletePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

