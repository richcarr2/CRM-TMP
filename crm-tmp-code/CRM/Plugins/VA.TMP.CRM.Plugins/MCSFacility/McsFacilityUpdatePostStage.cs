using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsFacilityUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsFacilityUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
