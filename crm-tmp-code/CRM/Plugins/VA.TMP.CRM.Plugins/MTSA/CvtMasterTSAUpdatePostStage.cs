using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class CvtMasterTSAUpdatePostStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtMasterTSAUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
