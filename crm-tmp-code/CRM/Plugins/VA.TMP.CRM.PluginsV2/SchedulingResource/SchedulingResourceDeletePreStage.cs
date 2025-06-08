using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class SchedulingResourceDeletePreStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new SchedulingResourceDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
