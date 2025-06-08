using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class CvtPatGroupCreatePreStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtPatGroupCreatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
