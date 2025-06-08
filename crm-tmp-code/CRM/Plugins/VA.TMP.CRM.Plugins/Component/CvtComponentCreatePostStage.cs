using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class CvtComponentCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtComponentCreateUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
