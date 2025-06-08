using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.Mappers
{
    internal static class MappingResolver
    {
        /// <summary>
        /// Site related fields Resolver Function
        /// </summary>
        /// <param name="organizationService">Organization Service</param>
        /// <param name="stationNumber">The Clinic's Station Number</param>
        /// <param name="logger">The logger object</param>
        internal static EntityReference SiteResolver(IOrganizationService organizationService, string stationNumber, string clinicIen, ILog logger)
        {
            EntityReference siteReference = null;
            using (var srv = new Xrm(organizationService))
            {
                //var site = srv.mcs_siteSet.FirstOrDefault(x => x.mcs_StationNumber == stationNumber);

                var sitelist = srv.mcs_siteSet.Where(x => x.mcs_StationNumber == stationNumber).ToList();


                if (sitelist == null || sitelist.Count == 0)
                {
                    logger.Error($"Site with Station Number: {stationNumber} does not exist");
                    return null;
                }
                else if (sitelist.Count == 1)
                {
                    siteReference = new EntityReference(mcs_site.EntityLogicalName, sitelist[0].mcs_siteId.Value);

                }

                else
                {
                    foreach (var site in sitelist)
                    {
                        var res = srv.mcs_resourceSet.Where(x => x.cvt_ien == clinicIen && x.mcs_RelatedSiteId != null && x.mcs_RelatedSiteId.Id == site.Id).FirstOrDefault();
                        if (res != null)
                        {
                            siteReference = new EntityReference(mcs_site.EntityLogicalName, site.mcs_siteId.Value);
                            return siteReference;
                        }
                    }
                    if (siteReference == null)
                    {
                        throw new MissingStationNumberException("Duplicate TMP sites found. Unable to determine site to add or update Clinic.");
                    }
                }

            }
            return siteReference;
        }

        /// <summary>
        /// Clinic Resolver Function - Retrives the clinic Id matching the Clinic IEN provided
        /// </summary>
        /// <param name="organizationService">Organization Service</param>
        /// <param name="clinicIen">The Clinic's Ien Number</param>
        /// <param name="stationNumber">The Facility Station Number</param>
        /// <returns>The Resource(Clinic) ID</returns>
        internal static Guid? ClinicResolver(IOrganizationService organizationService, string clinicIen, string stationNumber)
        {
            Guid? clinicId = null;
            using (var srv = new Xrm(organizationService))
            {
                var resources = (from r in srv.mcs_resourceSet
                                 join site in srv.mcs_siteSet on r.mcs_RelatedSiteId.Id equals site.mcs_siteId.Value
                                 where r.cvt_ien == clinicIen
                                 select new { r.mcs_resourceId, site.mcs_StationNumber });

                foreach (var res in resources)
                {
                    if (res.mcs_StationNumber == stationNumber)
                    {
                        clinicId = res.mcs_resourceId.Value;
                        break;
                    }
                }
            }
            return clinicId;
        }

        //internal static mcs_resource ClinicResolver(IOrganizationService organizationService, string facilityStationNumber, string clinicIen, string siteStationNumber, ILog logger)
        //{
        //    using (var svc = new Xrm(organizationService))
        //    {
        //        mcs_resource firstClinic = null;

        //        // Clinic
        //        if (string.IsNullOrEmpty(clinicIen) ||
        //            string.IsNullOrEmpty(facilityStationNumber) ||
        //            string.IsNullOrEmpty(siteStationNumber))
        //        {
        //            logger.Info($"INFO: ClinicIen: {clinicIen} or Institution: {facilityStationNumber} or Station Number: {siteStationNumber} is missing in the request");
        //            return firstClinic;
        //        }
        //        else
        //        {
        //            var resourceClinics = from c in svc.mcs_resourceSet
        //                                  where c.cvt_Institution == facilityStationNumber
        //                                  && c.cvt_ien == clinicIen
        //                                  && c.cvt_StationNumber == siteStationNumber
        //                                  && c.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic
        //                                  orderby c.ModifiedOn
        //                                  select c;

        //            if (resourceClinics == null || resourceClinics.ToList().Count().Equals(0))
        //                return firstClinic;

        //            logger.Debug($"# of Clinics found: {resourceClinics.ToList().Count()}");
        //            firstClinic = resourceClinics.First();

        //            logger.Debug("Clinic Id: " + firstClinic?.mcs_relatedResourceId?.Id);
        //            return firstClinic;
        //        }
        //    }
        //}

        internal static mcs_resource ClinicResolver(IOrganizationService organizationService, string facilityStationNumber, string clinicIen, string siteStationNumber, ILog logger, Guid? appointmentSiteId = null)
        {
            var clinicSite = ClinicSiteResolver(organizationService, facilityStationNumber, clinicIen, siteStationNumber, logger, appointmentSiteId);

            //return item1 - mcs_resource (clinic) from the Tuple<mcs_resource,mcs_site>
            return clinicSite.Item1;
        }

        internal static (mcs_resource, mcs_site) ClinicSiteResolver(IOrganizationService organizationService, string facilityStationNumber, string clinicIen, string siteStationNumber, ILog logger, Guid? appointmentSiteId = null)
        {
            mcs_facility facility = null;
            mcs_resource firstClinic = null;
            mcs_site firstSite = null;

            using (var service = new Xrm(organizationService))
            {
                var facilities = service.mcs_facilitySet.Where(i => i.mcs_StationNumber == facilityStationNumber).ToList();

                if (!facilities.Count().Equals(0))
                {
                    if (facilities.Count() > 1)
                    {
                        facilities.ForEach((f) =>
                        {
                            if (firstClinic != null) return;
                            facility = f;
                            var clinics = from c in service.mcs_resourceSet
                                          where c.mcs_Facility.Id == f.Id && c.cvt_ien == clinicIen
                                          && c.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic
                                          orderby c.CreatedOn
                                          select c;

                            if (clinics != null)
                            {
                                if (clinics.ToList().Count == 0)
                                {
                                    logger.Info($"Cannot find clinic with mcs_Facility.Id == {facility.Id} && cvt_ien == {clinicIen}");
                                    return;
                                }

                                var clinicCount = clinics.ToList().Count;
                                if (clinicCount > 1)
                                {
                                    var sites = appointmentSiteId.HasValue
                                        ? new List<mcs_site>() { service.mcs_siteSet.FirstOrDefault(s => s.Id == appointmentSiteId) }
                                        : string.IsNullOrEmpty(siteStationNumber)
                                            ? service.mcs_siteSet.Where(s => s.mcs_FacilityId.Id == f.Id).ToList()
                                            : service.mcs_siteSet.Where(s => s.mcs_StationNumber == siteStationNumber && s.mcs_FacilityId.Id == f.Id).ToList();

                                    if (sites != null)
                                    {
                                        if (sites.Count().Equals(1))
                                        {
                                            var clinic = clinics.FirstOrDefault(c => c.mcs_relatedResourceId != null && c.mcs_RelatedSiteId.Id == sites.First().Id);
                                            if (clinic != null) firstClinic = clinic;
                                            firstSite = sites.First();
                                        }
                                        else
                                        {
                                            sites.ForEach((s) =>
                                            {
                                                var clinic = clinics.FirstOrDefault(c => c.mcs_relatedResourceId != null && c.mcs_RelatedSiteId.Id == s.Id);
                                                if (clinic != null && !clinic.Id.Equals(Guid.Empty))
                                                {
                                                    logger.Debug("Clinic: " + firstClinic?.Id);
                                                    if (firstClinic == null) firstClinic = clinic;
                                                    firstSite = s;
                                                }
                                            });
                                        }
                                    }
                                    logger.Info($"Duplicates - If : Found {clinicCount} clinics with mcs_Facility.Id == {facility.Id} && cvt_ien == {clinicIen}");
                                }
                                else
                                {
                                    if (firstClinic == null) firstClinic = clinics.First();
                                    firstSite = service.mcs_siteSet.FirstOrDefault(s => s.Id == firstClinic.mcs_RelatedSiteId.Id);
                                }
                            }
                        });
                    }
                    else
                    {
                        facility = facilities.First();
                        var clinics = from c in service.mcs_resourceSet
                                      where c.mcs_Facility.Id == facility.Id && c.cvt_ien == clinicIen
                                      && c.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic
                                      orderby c.CreatedOn
                                      select c;

                        if (clinics != null)
                        {
                            if (clinics.ToList().Count == 0)
                            {
                                logger.Info($"Cannot find clinic with mcs_Facility.Id == {facility.Id} && cvt_ien == {clinicIen}");
                                return (firstClinic, firstSite);
                            }

                            var clinicCount = clinics.ToList().Count;
                            logger.Info("Clinic count: " + clinicCount);
                            if (clinicCount > 1)
                            {
                                var sites = appointmentSiteId.HasValue
                                    ? new List<mcs_site>() { service.mcs_siteSet.FirstOrDefault(s => s.Id == appointmentSiteId && s.statecode == 0) }
                                    : string.IsNullOrEmpty(siteStationNumber)
                                        ? service.mcs_siteSet.Where(s => s.mcs_FacilityId.Id == facility.Id && s.statecode == 0).ToList()
                                        : service.mcs_siteSet.Where(s => s.mcs_StationNumber == siteStationNumber && s.mcs_FacilityId.Id == facility.Id && s.statecode == 0).ToList();
                                logger.Info("Sites count: " + sites.Count);

                                if (sites != null)
                                {
                                    if (sites.Count().Equals(1))
                                    {
                                        var clinic = clinics.FirstOrDefault(c => c.mcs_RelatedSiteId.Id == sites.First().Id);
                                        if (clinic != null) firstClinic = clinic;
                                        firstSite = sites.First();
                                    }
                                    else
                                    {                                        
                                        sites.ForEach((s) =>
                                        {
                                            logger.Info("sites: " + s.Id);
                                            var clinic = clinics.FirstOrDefault(c => c.mcs_RelatedSiteId.Id == s.Id);
                                            if (clinic != null && !clinic.Id.Equals(Guid.Empty))
                                            {
                                                logger.Debug("Clinic: " + firstClinic.Id);
                                                if (firstClinic == null) firstClinic = clinic;
                                                firstSite = s;
                                            }
                                        });
                                    }
                                }
                                logger.Info($"Duplicates - Else: Found {clinicCount} clinics with mcs_Facility.Id == {facility.Id} && cvt_ien == {clinicIen}");
                            }
                            else
                            {
                                if (firstClinic == null) firstClinic = clinics.First();
                                firstSite = service.mcs_siteSet.FirstOrDefault(s => s.Id == firstClinic.mcs_RelatedSiteId.Id);
                            }
                        }
                    }
                }
            }

            return (firstClinic, firstSite);
        }

        internal static EntityReference DefaultProviderResolver(IOrganizationService organizationService, string email, ILog logger)
        {
            EntityReference defaultProviderReference = null;
            using (var srv = new Xrm(organizationService))
            {
                var defaultProvider = srv.SystemUserSet.FirstOrDefault(x => x.InternalEMailAddress == email);
                if (defaultProvider == null)
                {
                    logger.Info($"Default Provider (User) with Email: {email} does not exist");
                    return null;
                }
                if (defaultProvider.SystemUserId != null) defaultProviderReference = new EntityReference(SystemUser.EntityLogicalName, defaultProvider.SystemUserId.Value);
            }
            return defaultProviderReference;
        }
    }
}
