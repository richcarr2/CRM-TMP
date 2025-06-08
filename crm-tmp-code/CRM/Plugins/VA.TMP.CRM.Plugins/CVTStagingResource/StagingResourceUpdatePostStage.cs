using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.CRM.CVTStagingResource
{
    public class StagingResourceUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new StagingResourceUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    
    }
}
