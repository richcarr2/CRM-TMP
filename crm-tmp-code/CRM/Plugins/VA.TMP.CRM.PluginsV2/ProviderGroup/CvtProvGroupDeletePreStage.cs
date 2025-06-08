using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class CvtProvGroupDeletePreStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtProvGroupDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
