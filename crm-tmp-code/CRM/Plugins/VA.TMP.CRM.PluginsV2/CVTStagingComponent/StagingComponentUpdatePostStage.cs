using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.CRM.CVTStagingComponent
{
    public class StagingComponentUpdatePostStage: IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new StagingComponentUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }

    }
}
