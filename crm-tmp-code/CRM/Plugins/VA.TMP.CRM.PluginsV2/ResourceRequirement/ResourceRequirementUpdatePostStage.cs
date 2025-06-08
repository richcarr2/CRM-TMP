using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class ResourceRequirementUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ResourceRequirementUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
