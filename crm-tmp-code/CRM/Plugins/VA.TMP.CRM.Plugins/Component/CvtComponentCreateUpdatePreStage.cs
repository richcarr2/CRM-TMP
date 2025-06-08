using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class CvtComponentCreateUpdatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtComponentCreateUpdatePreStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
