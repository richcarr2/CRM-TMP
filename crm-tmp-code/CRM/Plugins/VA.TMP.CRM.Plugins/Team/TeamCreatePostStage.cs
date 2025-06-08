using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class TeamCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new TeamCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}