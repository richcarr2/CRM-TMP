using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.IntegrationResult
{
    public class IntegrationResultUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new IntegrationResultUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
