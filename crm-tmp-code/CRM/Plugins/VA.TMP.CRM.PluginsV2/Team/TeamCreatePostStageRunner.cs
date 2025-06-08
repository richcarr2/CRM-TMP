using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using System;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class TeamCreatePostStageRunner : PluginRunner
    {
        public TeamCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Primary Functionality
        /// <summary>
        /// Execute method is the hook from the abstract runner class, and is what is called by the PluginRunner
        /// </summary>
        /// <remarks>
        /// The plugin checks that the new Team Record gets the correctly assigned base security roles.
        /// </remarks>
        public override void Execute()
        {
            //if (PluginExecutionContext.Depth > 1)
            //    return;
            CvtHelper.SetTeamRoles(PluginExecutionContext.PrimaryEntityId, Team.EntityLogicalName, OrganizationService, Logger);
        }

        #endregion

        #region AbstractClassRequiredMethods
        public override string McsSettingsDebugField
        {
            get { return "cvt_teamplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}