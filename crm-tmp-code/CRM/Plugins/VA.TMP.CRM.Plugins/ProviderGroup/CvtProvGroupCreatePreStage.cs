using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class CvtProvGroupCreatePreStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtProvGroupCreatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
