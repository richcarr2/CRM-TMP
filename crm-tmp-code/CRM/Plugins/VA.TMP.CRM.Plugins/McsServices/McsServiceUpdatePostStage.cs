using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsServiceUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //plugin registered on deprecated entity 'mcs_services'
            var runner = new McsServiceUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
