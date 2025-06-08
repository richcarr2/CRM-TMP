using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class SystemSettingsUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new SystemSettingsUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
