using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class McsResourceUpdatePostStageAsyncRunner:PluginRunner
    {
        #region Constructor
        public McsResourceUpdatePostStageAsyncRunner(IServiceProvider ServiceProvider) : base(ServiceProvider)
        { }
        #endregion

        public override void Execute()
        {
            Logger.WriteDebugMessage("Starting");
        }

        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
        }
    }
}
