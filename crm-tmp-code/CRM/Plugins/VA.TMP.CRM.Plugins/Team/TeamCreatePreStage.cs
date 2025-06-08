using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class TeamCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new TeamCreatePreStageRunner(serviceProvider);
            runner.Execute();
        }
    }
}