using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class EMailCreatePostStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new EMailCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}