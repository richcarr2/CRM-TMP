using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class McsSystemSettingsCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsSystemSettingsCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
