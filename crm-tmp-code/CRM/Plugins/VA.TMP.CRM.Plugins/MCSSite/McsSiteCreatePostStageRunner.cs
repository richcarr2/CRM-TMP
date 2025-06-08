using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// 1. Creates system site from this TMP Site.
    /// 2. Creates new Site Team.  Associates TMP Site Team security role.
    /// 3. Updates the TMP Site with the appropriate VISN from the Facility listed.
    /// 4. Updates the TMP Site with the reference to the newly created system site.
    /// 5. Create a default TCT Team. Updates TMP Site with TCT Team
    /// </summary>
    public class McsSiteCreatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsSiteCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion
        #region Internal Methods/Properties
        public override void Execute()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var thisSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == PluginExecutionContext.PrimaryEntityId);
                CreateSystemSite(thisSite);
                CvtHelper.AlignLocations(thisSite, OrganizationService, Logger);
                CreateTCTTeam(thisSite);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// 1. Creates system site from this TMP Site.
        /// 2. Creates new Site Team.  Associates TMP Site Team security role.
        /// 3. Updates the TMP Site with the reference to the newly created system site.
        /// </summary>
        /// <param name="thisSite"></param>
        internal void CreateSystemSite(mcs_site thisSite)
        {
            Logger.setMethod = "CreateSystemSite";
            Logger.WriteDebugMessage("starting CreateSystemSite");

            //Setting attributes for new System Site. 
            Site systemSite = new Site()
            {
                Name = thisSite.mcs_name,
                TimeZoneCode = (thisSite.mcs_TimeZone != null) ? thisSite.mcs_TimeZone : 0
            };

            //System Site Created. 
            var newSite = OrganizationService.Create(systemSite);
            Logger.WriteDebugMessage("The system 'Site' record has been created for " + thisSite.mcs_name + ".");
            using (var srv = new Xrm(OrganizationService))
            {
                //Query for the Facilities BU and use that to set the Team's BU
                var parentFacility = srv.mcs_facilitySet.FirstOrDefault(i => i.Id == thisSite.mcs_FacilityId.Id);

                if (parentFacility == null)
                {
                    Logger.WriteDebugMessage("Parent Facility not found, stopping logic.");
                    throw new InvalidPluginExecutionException("Parent Facility not found, please verify the facility is filled in and try again.");
                }
                //Creating new team based off of the new Site that was created. 
                Team systemTeam = new Team()
                {
                    Name = thisSite.mcs_name
                };

                if (parentFacility.mcs_BusinessUnitId != null)
                {
                    systemTeam.BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, parentFacility.mcs_BusinessUnitId.Id);
                    
                }
                else
                {
                    Logger.WriteDebugMessage("No BU found for parent Facility.");
                }

                var newTeam = OrganizationService.Create(systemTeam);
                Logger.WriteDebugMessage("The new Site team was created.  About to find and associate TMP Site Team role.");

                // Find the role.
                var roles = srv.RoleSet.FirstOrDefault(r => r.Name == "TMP Site Team" && r.BusinessUnitId.Id == parentFacility.mcs_BusinessUnitId.Id);

                if (roles != null)
                {
                    OrganizationService.Associate(Team.EntityLogicalName, newTeam, new Relationship("teamroles_association"),
                        new EntityReferenceCollection() { new EntityReference(Role.EntityLogicalName, roles.Id) });
                    Logger.WriteDebugMessage("Associated TMP Site Team Role to newly created Site Team.");
                }
                else
                {
                    Logger.WriteDebugMessage("TMP Site Team role not found for BU: " + parentFacility.mcs_BusinessUnitId.Name);
                    throw new InvalidPluginExecutionException("Error: TMP Site Team role was not found for the parent Business Unit.");
                }

                //Logger.WriteDebugMessage("About to update ownership of TMP Site to new Team.");
                //AssignRequest assignRequest = new AssignRequest()
                //{
                //    Assignee = new EntityReference(Team.EntityLogicalName, newTeam),
                //    Target = new EntityReference(mcs_site.EntityLogicalName, PluginExecutionContext.PrimaryEntityId)
                //};

                //OrganizationService.Execute(assignRequest);
                //Logger.WriteDebugMessage("Reassigned TMP Site to newly created Site Team.");
                Logger.WriteDebugMessage("About to update TMP Site with the site and site team references.");

                //Updates the mcs_Site with a reference to the System Site that was created and Site Team so they are appropiately associated
                mcs_site mcsSiteUpdate = new mcs_site()
                {
                    Id = PluginExecutionContext.PrimaryEntityId,
                    mcs_RelatedActualSiteId = new EntityReference(Site.EntityLogicalName, newSite),
                    cvt_TSSSiteTeam = new EntityReference(Team.EntityLogicalName, newTeam)
                };
                OrganizationService.Update(mcsSiteUpdate);
                Logger.WriteDebugMessage("TMP Site (" + thisSite.mcs_name + ") was updated with system site reference. Finished CreateSystemSite method.");
            }
        }

        internal void CreateTCTTeam(mcs_site thisSite)
        {
            Logger.setMethod = "CreateTCTTeam";
            Logger.WriteDebugMessage("starting CreateTCTTeam");

            Guid teamId = McsSystemSettingsCreatePostStageRunner.CreateTCTTeam(thisSite, OrganizationService, Logger);
            
            if (teamId != Guid.Empty && thisSite.Id != Guid.Empty)
            {
                Logger.WriteDebugMessage("Created TCT Team. ID: " + teamId);
                //Update TMP Site with this team
                mcs_site updateSiteRecord = new mcs_site()
                {
                    Id = thisSite.Id,
                    cvt_tctteam = new EntityReference()
                    {
                        Id = teamId,
                        LogicalName = Team.EntityLogicalName
                    }
                };

                OrganizationService.Update(updateSiteRecord);
                Logger.WriteDebugMessage(String.Format("Updated TMP Site: {0} with TCT Team.", thisSite.mcs_name));

            }
            else
                Logger.WriteDebugMessage("Failed to create TCT Team.  Guid is Empty.");

            Logger.WriteDebugMessage("ending CreateTCTTeam");
        }
        #endregion
        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_siteplugin"; }
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