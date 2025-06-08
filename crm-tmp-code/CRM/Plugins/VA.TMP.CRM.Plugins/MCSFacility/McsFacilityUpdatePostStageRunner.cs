using Microsoft.Xrm.Sdk;
using System;
using VA.TMP.DataModel;
using System.Linq;
using MCSShared;

namespace VA.TMP.CRM
{
    public class McsFacilityUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsFacilityUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            if (PluginExecutionContext.Depth > 1) { return; }
            using (var srv = new Xrm(OrganizationService))
            {               
                Entity thisFacility = (Entity)PluginExecutionContext.InputParameters["Target"];
                //Limit calling Update System Site function to change in name
                if (thisFacility.Attributes.Contains("mcs_name"))
                {
                    string[] roles = new string[] { "TMP Application Administrator", "System Administrator" };
                    if (CheckRole(PluginExecutionContext.InitiatingUserId, roles) == true)
                    {
                        //UpdatetheFacilityTeam(PluginExecutionContext, thisFacility.mcs_name);
                        UpdateRelatedFacilityTeams(thisFacility.Id, thisFacility.Attributes["mcs_name"].ToString());
                        UpdateRelatedFacilityPrivileges(thisFacility.Id, thisFacility.Attributes["mcs_name"].ToString());
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Facility Name (" + thisFacility.Attributes["mcs_name"].ToString() + ") change attempted by a non TMP App Admin or System Administrator role.");
                        throw new InvalidPluginExecutionException("You do not have TMP App Admin or System Administrator role. You need one of those roles for a Facility Name Change. Please contact Help Desk.");
                    }
                }     
                ////Limit calling Align Locations to change in Facility
                //if (thisFacility.Contains("mcs_businessunitid"))
                //{
                //    //Logger.WriteDebugMessage("If VISN changed, then could automatically kickoff VISN re-org.");
                //    //CvtHelper.AlignLocations(thisFacility, OrganizationService, Logger);
                //    //Logger.WriteDebugMessage("Checking if Re-ORg needs to be started.");
                //}
                //else
                //    Logger.WriteDebugMessage("No need to Align Locations");
            }
        }
        internal bool CheckRole(Guid user, string[] rolesneeded) {
            Boolean passed = false;
            using (var srv = new Xrm(OrganizationService))
            {
                var userRoles = srv.SystemUserRolesSet.Where(r => r.SystemUserId == user);

                foreach (var role in userRoles)
                {
                    var userRoles2 = srv.RoleSet.FirstOrDefault(r => r.RoleId == role.RoleId);
                    foreach (var need in rolesneeded)
                    {
                        if (userRoles2.Name == need)
                        {
                            return true;
                        }
                    }
                }
            }

            return passed;

        }
        /// <summary>
        /// Staged: Update the Facility Team (after F2BU)
        /// </summary>
        /// <param name="FacilityContext"></param>
        /// <param name="NewName"></param>
        /// Need to Add a pre-image to this plugin
        /// Need to change this plugin to sync
        internal void UpdatetheFacilityTeam(IPluginExecutionContext FacilityContext, string NewFacilityName)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "UpdatetheFacilityTeam";
                Logger.WriteDebugMessage("Checking for PreImage with prior name.");
                if (FacilityContext.PreEntityImages != null && 
                    FacilityContext.PreEntityImages.Contains("PreImage") && 
                    FacilityContext.PreEntityImages["PreImage"] is Entity)
                {
                    //get PreImageEntity
                    Entity PreImageEntity = FacilityContext.PreEntityImages["PreImage"];
                    if(PreImageEntity["mcs_name"] != null)
                    {               
                        var FacilityTeam = srv.TeamSet.FirstOrDefault(t => t.Name == PreImageEntity["mcs_name"].ToString());
                        if (FacilityTeam != null)
                        {
                            Team UpdateFacilityTeam = new Team()
                            {
                                Id = FacilityTeam.Id,
                                Name = NewFacilityName
                            };
                            OrganizationService.Update(UpdateFacilityTeam);
                        }                   
                    }
                }
            }
        }

        internal void UpdateRelatedFacilityTeams(Guid FacilityId, string NewFacilityName)
        {
            Logger.setMethod = "UpdateRelatedFacilityTeams";
            using (var srv = new Xrm(OrganizationService))
            {
                var relatedTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == FacilityId);

                if (relatedTeams == null)
                    return;
                Logger.WriteDebugMessage("Retrieved teams related to Facility.");
                int count = 0;
                string ignoredSiteTeams = string.Empty;
                foreach (var team in relatedTeams)
                {
                    string teamName = CvtHelper.ReturnRecordNameIfChanged(team, false, Logger, OrganizationService);
                    if (string.IsNullOrEmpty(teamName))
                    {
                        ignoredSiteTeams += team.Name + ",";
                    }
                    else
                    { 
                        Team updateTeamName = new Team()
                        {
                            Id = team.Id,
                            Name = teamName
                        };
                        try
                        {
                            var teamExists = srv.TeamSet.FirstOrDefault(t => t.Name == teamName);
                            if (teamExists == null)
                            {
                                if (team.Name != team.BusinessUnitId.Name) //Update when Team is not Business Unit Team
                                {
                                    OrganizationService.Update(updateTeamName);
                                    count++;
                                }
                            }
                            else
                            {
                                Logger.WriteDebugMessage("Failed to update team name " + team.Name + " to " + teamName + " as the Target team name already exists.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteDebugMessage("Failed to update team name from " + team.Name + " to " + teamName + ". Error message = " + ex.Message);
                        }
                    }
                }
                Logger.WriteDebugMessage(count + " team names updated." + ((ignoredSiteTeams != string.Empty) ? "\nThe Site/BU Teams ignored : " + ignoredSiteTeams : string.Empty));
            }
        }

        internal void UpdateRelatedFacilityPrivileges(Guid FacilityId, string NewFacilityName)
        {
            Logger.setMethod = "UpdateRelatedFacilityPrivileges";
            using (var srv = new Xrm(OrganizationService))
            {
                var relatedActivePrivs = srv.cvt_tssprivilegingSet.Where(p => p.cvt_PrivilegedAtId.Id == FacilityId && p.statecode.Value == 0);

                if (relatedActivePrivs == null)
                    return;
                Logger.WriteDebugMessage("Retrieved TSS Privileges related to Facility.");
                int count = 0;
                foreach (var priv in relatedActivePrivs)
                {
                    string privName = CvtHelper.ReturnRecordNameIfChanged(priv, false, Logger, OrganizationService);
                    if (!string.IsNullOrEmpty(privName))
                    {
                        cvt_tssprivileging updatePrivName = new cvt_tssprivileging()
                        {
                            Id = priv.Id,
                            cvt_name = privName
                        };
                        try
                        {
                            if (updatePrivName.cvt_name != string.Empty)
                            {
                                OrganizationService.Update(updatePrivName);
                                count++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteDebugMessage("Failed to update Telehealth Privileging name for " + priv.cvt_name + ". Error message = " + ex.Message);
                        }
                    }
                }
                Logger.WriteDebugMessage(count + " TSS Privileges' names were updated.");
            }
        }

        #endregion

        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_serviceplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}