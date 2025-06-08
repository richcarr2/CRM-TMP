using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsSiteDeletePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsSiteDeletePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            DeleteSite(PluginExecutionContext.PrimaryEntityId);
        }

        internal void DeleteSite(Guid thisId)
        {
            Logger.WriteDebugMessage("Starting Delete System Site");
           
            using (var srv = new Xrm(OrganizationService))
            {
                var mcsSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == thisId);
                var systemSite = srv.SiteSet.FirstOrDefault(i => i.Id == mcsSite.mcs_RelatedActualSiteId.Id);

                var siteTeam = srv.TeamSet.FirstOrDefault(i => i.Name == mcsSite.mcs_name );
                if (siteTeam != null)
                {
                    Logger.WriteDebugMessage("Attempting to Delete Site Team");
                    OrganizationService.Delete(siteTeam.LogicalName , siteTeam.Id);
                    Logger.WriteDebugMessage(siteTeam.Name + " Site Team has been deleted.");
                }

                var staffTeam = srv.TeamSet.FirstOrDefault(i => i.Name.Contains(mcsSite.mcs_name) && i.Name.Contains("Staff"));
                if (staffTeam != null)
                {
                    Logger.WriteDebugMessage("Attempting to Delete Staff Team");
                    OrganizationService.Delete(staffTeam.LogicalName, staffTeam.Id);
                    Logger.WriteDebugMessage(staffTeam.Name + " Staff Team has been deleted.");
                }

                //If a System Resource does not exist, we will return because the Plugin will have nothing to delete. 
                if (systemSite == null)
                    return;
                Logger.WriteDebugMessage("Got System Site:" + systemSite.Name);

                //Checking to see if any Resources have this target MCS Site associated with it.
                Logger.WriteDebugMessage("About to check for related TMP Resources.");
                var mcsResource = srv.mcs_resourceSet.FirstOrDefault(i => i.mcs_RelatedSiteId.Id == thisId);
                //If an associated Group Resource does exist, we will throw an exception with a message and stop the delete of the MCS Site, to prevent orphan data.
                if (mcsResource != null)
                {
                    Logger.WriteDebugMessage("Resource Exists:" + mcsResource.mcs_name);                                    
                    throw new InvalidPluginExecutionException("customPlease check for related TMP Resources that this Site is associated with. Site cannot be deleted until associations are removed.");
                }
                else
                {
                    Logger.WriteDebugMessage("No related TMP Resources found.");
                }

                //get a PS
                Logger.WriteDebugMessage("About to check for related Participating Sites.");
                var participatingSite = srv.cvt_participatingsiteSet.FirstOrDefault(i => i.cvt_site.Id == thisId);
                if (participatingSite != null)
                {
                    Logger.WriteDebugMessage("Participating Site Exists: " + participatingSite.cvt_name);
                    throw new InvalidPluginExecutionException("customPlease check for related Participating Sites that this Site is associated with. Site cannot be deleted until associations are removed.");
                }
                else
                {
                    Logger.WriteDebugMessage("No related Participating Sites found.");
                }

                //If no exceptions are thrown from validation checks, we will actually delete the System Site associated with the MCS Site.              
                OrganizationService.Delete(systemSite.LogicalName, systemSite.Id);               
                Logger.WriteDebugMessage("TMP Site's System Site was deleted.");
            }     
        }
        #endregion

        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_siteplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }   
        #endregion
    }
}