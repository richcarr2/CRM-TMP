using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.VistaIntegrationResult
{
    public class VistaIntegrationResultUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new VistaIntegrationResultUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
