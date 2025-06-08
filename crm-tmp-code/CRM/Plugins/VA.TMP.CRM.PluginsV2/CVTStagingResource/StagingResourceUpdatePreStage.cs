using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM.CVTStagingResource
{
    public class StagingResourceUpdatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new StagingResourceUpdatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }

    }
}
