using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM.SchedulingPackage
{
    public class ResourcePackageCreatePostStage : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ResourcePackageCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);

        }
    }
}
