using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class CvtMasterTSACreatePostStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtMasterTSACreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
