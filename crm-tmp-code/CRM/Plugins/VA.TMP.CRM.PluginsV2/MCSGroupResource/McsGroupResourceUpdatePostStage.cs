using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsGroupResourceUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsGroupResourceUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

