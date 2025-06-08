using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsGroupResourceCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsGroupResourceCreatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
