using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class McsResourceCreatePostStageAsyncRunner:PluginRunner
    {
        #region Constructor
        public McsResourceCreatePostStageAsyncRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #endregion

        public override void Execute()
        {
            Logger.WriteDebugMessage("Starting");
        }


        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
        }
        #endregion

    }
}
