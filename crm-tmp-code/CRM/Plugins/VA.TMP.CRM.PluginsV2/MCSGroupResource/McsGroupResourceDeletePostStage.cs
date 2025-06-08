using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    public partial class McsGroupResourceDeletePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsGroupResourceDeletePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
