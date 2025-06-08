using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsSiteUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsSiteUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            if (PluginExecutionContext.Depth > 1) { return; }
            Entity siteContext = PrimaryEntity.ToEntity<mcs_site>();
            using (var srv = new Xrm(OrganizationService))
            {
                var thisSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == PluginExecutionContext.PrimaryEntityId);
                if (thisSite == null)                         
                    return;

                //Update the Site Team Name if the Name changes
                if (siteContext.Contains("mcs_name"))
                    UpdateSiteRelatedRecords(McsHelper.getStringValue("mcs_name"), thisSite);  
                
                //Limit calling Update System Site function to change in name, timezone 
                if ((siteContext.Contains("mcs_name")) || (siteContext.Contains("mcs_timezone")))
                    UpdateSystemSite(McsHelper.getEntRefID("mcs_relatedactualsiteid"));
                else
                    Logger.WriteDebugMessage("No need to update System Site record");
                        
                //Limit calling Align Locations to change in Facility
                if (siteContext.Contains("mcs_facilityid"))
                {
                    CvtHelper.AlignLocations(thisSite, OrganizationService, Logger);
                    Logger.WriteDebugMessage("Checking if Site Team needs to be updated.");
                    UpdateSiteTeam(thisSite.Id);
                }
                else
                    Logger.WriteDebugMessage("No need to Align Locations");
            }
        }
        /// <summary>
        /// Update the Site's Related Records with the Name change: Site Team, TSS Resource, TSS Resource Group
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="mcsSite"></param>
        internal void UpdateSiteRelatedRecords(string Name, mcs_site mcsSite)
        {
            Logger.setMethod = "UpdateSiteRelatedRecords";
            Logger.WriteDebugMessage("starting UpdateSiteRelatedRecords");
            var count = 0;
            var countTotal = 0;
            var entName = "";
            var message = "Updated records for: " + Name + ".";
            var siteText = mcsSite.mcs_StationNumber != null ? mcsSite.mcs_StationNumber : mcsSite.mcs_name;

            //Need to Update the Names for All TSS Resources and TSS Resource Groups (since the Name changed)
            using (var srv = new Xrm(OrganizationService))
            {
                #region TMP Resource

                var siteResources = srv.mcs_resourceSet.Where(r => r.mcs_RelatedSiteId.Id == mcsSite.Id);
                count = 0;
                countTotal = 0;
                entName = "TMP Resource";
                
                foreach (mcs_resource res in siteResources)
                {
                    countTotal++;
                    try
                    {
                        var name = CvtHelper.DeriveName(res, true, Logger, OrganizationService, siteText);
                        if (name != "")
                        {
                            mcs_resource updateRes = new mcs_resource()
                            {
                                Id = res.Id,
                                mcs_name = name
                            };
                            OrganizationService.Update(updateRes);
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                            Logger.WriteDebugMessage(String.Format("Failed to update {0}. Error: {1}", entName, ex?.Message));
                    }
                }
                Logger.WriteDebugMessage("Finished updating related TMP Resources.");

                message += String.Format(" {0}/{1} {2}.", count, countTotal, entName);
                #endregion

                #region TMP Resource Group

                var siteResourceGroups = srv.mcs_resourcegroupSet.Where(rg => rg.mcs_relatedSiteId.Id == mcsSite.Id);
                count = 0;
                countTotal = 0;
                entName = "TMP Resource Group";

                foreach (mcs_resourcegroup rg in siteResourceGroups)
                {
                    countTotal++;
                    try
                    {
                        var name = CvtHelper.DeriveName(rg, true, Logger, OrganizationService, siteText);
                        if (name != "")
                        {
                            mcs_resourcegroup updateRG = new mcs_resourcegroup()
                            {
                                Id = rg.Id,
                                mcs_name = name
                            };
                            OrganizationService.Update(updateRG);
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteDebugMessage(String.Format("Failed to update {0}. Error: {1}", entName, ex.Message));
                    }
                }
                Logger.WriteDebugMessage("Finished updating related TMP Resource Groups.");

                message += String.Format(" {0}/{1} {2}.", count, countTotal, entName);
                #endregion 

                #region PS
                var participatingSite = srv.cvt_participatingsiteSet.Where(i => i.cvt_site.Id == mcsSite.Id);

                count = 0;
                countTotal = 0;
                entName = "PS";

                foreach (cvt_participatingsite ps in participatingSite)
                {
                    countTotal++;
                    try
                    {
                        var name = CvtHelper.DeriveName(ps, true, Logger, OrganizationService, mcsSite.mcs_name);
                        if (name != "")
                        {
                            cvt_participatingsite updatePS = new cvt_participatingsite()
                            {
                                Id = ps.Id,
                                cvt_name = name
                            };
                            OrganizationService.Update(updatePS);
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteDebugMessage(String.Format("Failed to update {0}. Error: {1}", entName, ex?.Message));
                    }
                }
                Logger.WriteDebugMessage("Finished updating related PSs.");

                message += String.Format(" {0}/{1} {2}.", count, countTotal, entName);
                #endregion
            }

            #region Site Team
            //check for the Site Team in the Lookup field
            if (mcsSite.cvt_TSSSiteTeam == null)
                return;

            if (Name != mcsSite.cvt_TSSSiteTeam.Name)
            {
                Team siteTeam = new Team()
                {
                    Id = mcsSite.cvt_TSSSiteTeam.Id,
                    Name = Name
                };
                try
                {
                    OrganizationService.Update(siteTeam);
                }
                catch (Exception ex)
                {
                    Logger.WriteDebugMessage("Failed to update Site Team's Name. Error: " + ex?.Message);
                }
            }

            #endregion

            Logger.WriteDebugMessage("Updated Site Team's Name: " + Name);
            Logger.WriteToFile(message);
        }

        internal void UpdateSystemSite(Guid id)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "UpdateSystemSite";
                Logger.WriteTxnTimingMessage("starting UpdateSystemSite");
                var systemSite = srv.SiteSet.FirstOrDefault(i => i.Id == id);
                if (systemSite == null) return;
                Logger.WriteTxnTimingMessage("Retrieved system site");

                var updateSystemSite = new Site
                {
                    Id = systemSite.Id,
                    Name = McsHelper.getStringValue("mcs_name"),
                    TimeZoneCode = McsHelper.getIntValue("mcs_timezone"),
                };

                OrganizationService.Update(updateSystemSite);
                Logger.WriteDebugMessage("System Site record updated");
            }
        }

        //Update the bu of the siteteam
        internal void UpdateSiteTeam(Guid id)
        {
            Logger.setMethod = "UpdateSiteTeam";
            Logger.WriteDebugMessage("starting UpdateSiteTeam");

            var tssSite = (mcs_site)OrganizationService.Retrieve(mcs_site.EntityLogicalName, id, new ColumnSet(true));

            if (tssSite != null)
            {
                var siteTeamLookup = tssSite.cvt_TSSSiteTeam;
                if (siteTeamLookup == null)
                    return;

                var siteTeam = (Team)OrganizationService.Retrieve(Team.EntityLogicalName, siteTeamLookup.Id, new ColumnSet(true));
                if (siteTeam == null)
                    return;

                //Check for BU
                if (siteTeam.BusinessUnitId.Id != tssSite.mcs_BusinessUnitId.Id)
                {
                    CvtHelper.UpdateSiteTeam(siteTeam.Id, tssSite.mcs_BusinessUnitId.Id, Logger, OrganizationService);

                    //Since All TSS Resources, TSS Resource Groups, and Components should be owned by this Team, their ownership should be correct.
                    

                }
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
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}