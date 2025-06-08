using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceCreateSysRecordsPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceCreateSysRecordsPostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

