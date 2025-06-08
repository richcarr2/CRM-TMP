using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public class CvtPatGroupCreatePostStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtPatGroupCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
