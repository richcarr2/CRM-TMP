using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class MissingResourceActionPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new MissingResourceActionPostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
