using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class McsResourceCreateUpdatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceCreateUpdatePreStageRunner(serviceProvider);
            runner.Execute();
        }
    }
}