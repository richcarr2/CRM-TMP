using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class CvtPatGroupDeletePreStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtPatGroupDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
