using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsGroupResourceCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsGroupResourceCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
