using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

