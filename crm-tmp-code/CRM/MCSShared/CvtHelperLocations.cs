using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace MCSShared
{
    public static partial class CvtHelper
    {
        //Denormalized Data Integrity Functions
        #region Align Locations

        /// <summary>
        /// OVERLOAD: Updated TSS Resource record - with a new TSS Site
        /// </summary>
        /// <param name="tssresource"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void AlignLocations(mcs_resource tssresource, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            //Update this one TSS Resources - Facility based on Site, and check VISN based on Facility
            if (tssresource.mcs_RelatedSiteId != null)
                AlignOneTSSResource(tssresource.Id, OrganizationService, Logger);
        }

        /// <summary>
        /// OVERLOAD: Updated TSS Site record.  Can only update the Facility on this record.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void AlignLocations(mcs_site site, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            //Update TSS Sites - VISN based on Facility -Possibly only update 1, if the site changed facility that is in another visn
            if (site.mcs_FacilityId != null)
                AlignTSSSiteVISNData(site.mcs_FacilityId.Id, OrganizationService, Logger);

            //Update Users - Facility based on Site
            AlignUserFacilityData(site.Id, OrganizationService, Logger);

            //Update TSS Resources - Facility based on Site, VISN based on Facility
            AlignMultipleTSSResourceFacilityVISNData(site.Id, OrganizationService, Logger);
        }

        //New Plugin for Facility Update and step
        /// <summary>
        /// OVERLOAD: Updated Facility record - with a new VISN
        /// </summary>
        /// <param name="facility"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void AlignLocations(mcs_facility facility, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            try
            {
                Logger.setMethod = "AlignLocations";
                Logger.WriteGranularTimingMessage("Starting AlignLocations");

                //Update TSS Sites - VISN based on Facility
                AlignTSSSiteVISNData(facility.Id, OrganizationService, Logger);

                //Update TSS Resources - Facility based on Site and VISN based on Facility
                AlignMultipleTSSResourceFacilityVISNData(facility.Id, OrganizationService, Logger);

                //AlignSystemUserVISNData(facility.Id, OrganizationService, Logger);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Failed CvtHelper.AlignLocations. " + CvtHelper.BuildExceptionMessage(ex) + ex.StackTrace);
            }
        }

        /// <summary>
        /// TSS Site Entity
        /// Align VISN from Facility
        /// </summary>
        /// <param name="thisFacilityId"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void AlignTSSSiteVISNData(Guid thisFacilityId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "AlignTSSSiteVISNData";
                Logger.WriteTxnTimingMessage("starting AlignTSSSiteVISNData");
                var thisFacility = srv.mcs_facilitySet.FirstOrDefault(i => i.Id == thisFacilityId);
                var countUpdate = 0;

                if (thisFacility.mcs_BusinessUnitId != null)
                {

                    var retrieveTSSSites = srv.mcs_siteSet.Where(s => s.mcs_FacilityId.Id == thisFacilityId).ToList();

                    //Loop through all of the TSS Sites at this facility to Update the VISN field
                    foreach (var tsssite in retrieveTSSSites)
                    {
                        var updateThisTSSSite = new Entity(mcs_site.EntityLogicalName)
                        {
                            Id = tsssite.Id
                        };
                        updateThisTSSSite.Attributes.Add("mcs_businessunitid", new EntityReference(thisFacility.mcs_VISN.LogicalName, thisFacility.mcs_VISN.Id)); 
                        //updateThisTSSSite = UpdateField(updateThisTSSSite, tsssite, thisFacility, "mcs_businessunitid", "mcs_businessunitid");
                        if (updateThisTSSSite.Attributes.Count() > 0)
                        {
                            OrganizationService.Update(updateThisTSSSite);
                            countUpdate += 1;
                            Logger.WriteTxnTimingMessage(tsssite.mcs_name + "'s VISN was updated");
                        }
                    }
                }
                Logger.WriteTxnTimingMessage("ending AlignTSSSiteVISNData.");
                Logger.WriteDebugMessage(countUpdate + " TSS Sites' VISN were updated.");
            }
        }

        /// <summary>
        /// Users Entity
        /// Align Facility from Site
        /// </summary>
        /// <param name="thisSiteId"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void AlignUserFacilityData(Guid thisSiteId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "AlignUserFacilityData";
                Logger.WriteTxnTimingMessage("starting AlignUserFacilityData");
                //Need to retrieve Site again, because it has changed
                var thisUpdatedSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == thisSiteId);
                var retrieveUsers = srv.SystemUserSet.Where(u => u.cvt_site.Id == thisSiteId).ToList();

                var countUpdate = 0;
                //Retrieve all Users with this TSS Site
                foreach (var user in retrieveUsers)
                {
                    var updateThisUser = new Entity(SystemUser.EntityLogicalName)
                    {
                        Id = user.Id
                    };
                    updateThisUser = UpdateField(updateThisUser, user, thisUpdatedSite, "cvt_facility", "mcs_facilityid", false);
                    if (updateThisUser.Attributes.Count() > 0)
                    {
                        OrganizationService.Update(updateThisUser);
                        countUpdate += 1;
                        Logger.WriteTxnTimingMessage(user.FullName + " Facility was updated based on Site");
                    }
                }
                Logger.WriteTxnTimingMessage("ending AlignUserFacilityData.");
                Logger.WriteDebugMessage(countUpdate + " Users were updated.");
            }
        }


        internal static int AlignOneTSSResource(Guid thistssresourceId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "AlignOneTSSResource";
                Logger.WriteTxnTimingMessage("starting AlignOneTSSResource");
                var thisresource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == thistssresourceId);
                var resourcesSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == thisresource.mcs_RelatedSiteId.Id);
                var sitesFacility = srv.mcs_facilitySet.FirstOrDefault(i => i.Id == resourcesSite.mcs_FacilityId.Id);
                Logger.WriteDebugMessage("Retrieved Resource, Site, Facility");

                var updateThisTSSResource = new Entity(mcs_resource.EntityLogicalName)
                {
                    Id = thistssresourceId
                };
                updateThisTSSResource = UpdateField(updateThisTSSResource, thisresource, resourcesSite, "mcs_facility", "mcs_facilityid");
                updateThisTSSResource = UpdateField(updateThisTSSResource, thisresource, sitesFacility, "mcs_businessunitid", "mcs_visn");
                if (updateThisTSSResource.Attributes.Count > 0)
                {
                    OrganizationService.Update(updateThisTSSResource);
                    Logger.WriteDebugMessage("Updated TSS Resource");
                    return 1;
                }
                else
                    return 0;
            }
        }
        /// <summary>
        /// TSS Resource Entity
        /// Align Facility from Site
        /// Calls AlignTSSResourceVISNData at end
        /// </summary>
        /// <param name="thisSiteId"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void AlignMultipleTSSResourceFacilityVISNData(Guid thisSiteId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "AlignMultipleTSSResourceFacilityVISNData";
                Logger.WriteTxnTimingMessage("starting AlignMultipleTSSResourceFacilityVISNData");
                //Need to retrieve Site again, because it has changed
                var thisUpdatedSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == thisSiteId);
                var retrieveTSSResources = from r in srv.mcs_resourceSet
                                           where r.mcs_RelatedSiteId.Id == thisSiteId
                                           select new
                                           {
                                               r.Id
                                           };
                var countUpdate = 0;
                //Loop through all of the TSS resources with this site
                foreach (var tssresource in retrieveTSSResources)
                {
                    countUpdate += AlignOneTSSResource(tssresource.Id, OrganizationService, Logger);
                }
                Logger.setMethod = "AlignMultipleTSSResourceFacilityVISNData";
                Logger.WriteTxnTimingMessage("ending AlignTSSResourceFacilityData.");
                Logger.WriteDebugMessage(countUpdate + " TSS Resources' records were updated.");
            }
        }

        /// <summary>
        /// Align VISN from Facility for all TSS Resource Entities listed in the facility
        /// </summary>
        /// <param name="thisFacilityId">ID of Facility to align VISN of resources</param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void AlignTSSResourceVISNData(Guid thisFacilityId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "AlignTSSResourceVISNData";
                Logger.WriteTxnTimingMessage("starting AlignTSSResourceVISNData");
                //Need to retrieve Site again, because it has changed
                var thisFacility = srv.mcs_facilitySet.FirstOrDefault(i => i.Id == thisFacilityId);
                var retrieveTSSResources = srv.mcs_resourceSet.Where(r => r.mcs_Facility.Id == thisFacilityId);

                var countUpdate = 0;
                //Loop through all of the TSS resources with this facility
                foreach (var tssresource in retrieveTSSResources)
                {
                    var updateThisTSSResource = new Entity(mcs_resource.EntityLogicalName)
                    {
                        Id = tssresource.Id
                    };
                    updateThisTSSResource = UpdateField(updateThisTSSResource, tssresource, thisFacility, "mcs_businessunitid", "mcs_businessunitid", false);
                    if (updateThisTSSResource.Attributes.Count() > 0)
                    {
                        OrganizationService.Update(updateThisTSSResource);
                        countUpdate += 1;
                        Logger.WriteTxnTimingMessage(tssresource.mcs_name + "'s VISN was updated");
                    }
                }
                Logger.WriteTxnTimingMessage("ending AlignTSSResourceVISNData.");
                Logger.WriteDebugMessage(countUpdate + " TSS Resources' VISN were updated.");
            }
        }
        #endregion

    }
}