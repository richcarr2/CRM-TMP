using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class TeamCreatePreStageRunner : PluginRunner
    {
        public TeamCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Primary Functionality
        /// <summary>
        /// Execute method is the entry point into the runner class, and is what is called by the Actual Plugin: TeamCreatePreStage
        /// </summary>
        /// <remarks>
        /// The plugin checks that the new Team Record gets the correctly assigned BU based on facility (if selected), not based on the User - if no facility selected, CRM defaults to User.
        /// </remarks>
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != Team.EntityLogicalName)
                return;

            Team thisTeam = PrimaryEntity.ToEntity<Team>();
            if (thisTeam.cvt_Facility != null)
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var facility = srv.mcs_facilitySet.First(f => f.Id == thisTeam.cvt_Facility.Id);

                    if (facility.mcs_BusinessUnitId != null)
                    {
                        Logger.WriteDebugMessage("On create of team (" + thisTeam.Name + "), updated VISN to match listed Facility's VISN: " + facility.mcs_BusinessUnitId.Name);
                        PrimaryEntity.Attributes["businessunitid"] = facility.mcs_BusinessUnitId;
                    }
                }
            }

            //Create name
            var createName = CvtHelper.ReturnRecordNameIfChanged(thisTeam, true, Logger, OrganizationService);
            if (createName != "")
                PrimaryEntity.Attributes["name"] = createName;
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