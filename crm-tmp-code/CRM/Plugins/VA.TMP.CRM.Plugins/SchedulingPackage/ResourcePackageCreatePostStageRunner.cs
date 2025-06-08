using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using VA.TMP.CRM;



namespace VA.TMP.CRM
{
    public class ResourcePackageCreatePostStageRunner : PluginRunner
    {
        //Email Email; //--new line
        public ResourcePackageCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        /// <summary>
        /// Entry Point for Resource Package Pre Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        public override void Execute()
        {

            if (PrimaryEntity.LogicalName != "cvt_resourcepackage")
            {
                return;
            }

            Entity resourcePackage = PluginExecutionContext.PostEntityImages["PostImage"] as Entity;

            if (resourcePackage == null)
            {
                return;
            }
             
            SPEmailNotification.ResourcePackage(OrganizationService, resourcePackage,null);
        }   
       
        #region Additional Interface Methods/Properties
        /// <summary>
        /// Used for Debugging - turns on or off creation of log records for this particular entity
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_ppereview"; }
        }
        #endregion
    }
}
