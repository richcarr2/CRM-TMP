using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsResourceSetStatePreStageRunner : PluginRunner
    {
        #region Constructor
        private static string _Prefix = "zz";
        public McsResourceSetStatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var mcsResource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == PrimaryEntity.Id);
                if (mcsResource != null && mcsResource.mcs_Type != null && mcsResource.mcs_Type.Value == 251920000)
                {
                    UpdateResourceName(mcsResource);
                }
            }
        }

        private void UpdateResourceName(mcs_resource mcsResource)
        {
            if (!string.IsNullOrEmpty(mcsResource.mcs_name) && !mcsResource.mcs_name.StartsWith(_Prefix))
            {
                var resource = new mcs_resource
                {
                    mcs_name = _Prefix + mcsResource.mcs_name,
                    mcs_resourceId = mcsResource.Id
                };
                OrganizationService.Update(resource);
            }
        }

        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
        }
 
        #endregion
    }
}