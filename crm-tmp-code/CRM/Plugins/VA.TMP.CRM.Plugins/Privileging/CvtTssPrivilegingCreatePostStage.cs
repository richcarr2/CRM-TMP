using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class CvtTssPrivilegingCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtTssPrivilegingCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}