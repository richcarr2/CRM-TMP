using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class ImportFieldValidationUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ImportFieldValidationUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
