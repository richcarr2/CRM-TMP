using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class RequirementGroupCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new RequirementGroupCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}