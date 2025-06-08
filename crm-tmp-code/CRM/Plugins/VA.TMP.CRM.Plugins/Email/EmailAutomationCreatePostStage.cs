using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class EmailAutomationCreatePostStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new EmailAutomationCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}