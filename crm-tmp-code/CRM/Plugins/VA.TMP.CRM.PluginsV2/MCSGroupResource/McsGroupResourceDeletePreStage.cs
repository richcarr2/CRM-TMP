using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsGroupResourceDeletePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsGroupResourceDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

