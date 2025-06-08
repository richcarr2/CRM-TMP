using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class ResourcePackageCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ResourcePackageCreatePreStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}