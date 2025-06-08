using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM.Import
{
    public class ImportPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ImportPostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider); 
        }
    }
}
