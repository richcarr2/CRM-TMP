using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsServicesCreatePostStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsServicesCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
