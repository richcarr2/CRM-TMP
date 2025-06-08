using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.CRM
{
    public class ParticipatingSiteUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ParticipatingSiteUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
