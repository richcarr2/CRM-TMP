using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsServicesDeletePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //plugin registered on Deprecated entity 'mcs_services'
            var runner = new McsServicesDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

