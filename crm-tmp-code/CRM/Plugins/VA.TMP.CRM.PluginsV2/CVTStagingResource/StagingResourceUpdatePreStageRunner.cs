using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM.CVTStagingResource
{
    public class StagingResourceUpdatePreStageRunner : PluginRunner
    {
        #region Constructor

        public StagingResourceUpdatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion

        #region AbstractClassRequiredMethods
        public override string McsSettingsDebugField
        {
            get { return "mcs_stagingresourceplugin"; }
        }
        #endregion

        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_stagingresource.EntityLogicalName)
                return;
            try
            {
                cvt_stagingresource thisStagingResource = PrimaryEntity.ToEntity<cvt_stagingresource>();

                //Making sure no approval before resolving all Import Field Mismatches
                if (thisStagingResource.cvt_approvalstatus != null && thisStagingResource.cvt_approvalstatus.Value == (int)cvt_approvalstatus.Approved)
                {
                    using (var srv = new Xrm(OrganizationService))
                    {
                        var mismatches = srv.cvt_fieldmismatchSet.Where(x => x.cvt_stagingresource.Id == thisStagingResource.Id && x.statecode == cvt_fieldmismatchState.Active).ToList<Entity>();

                        if(mismatches.Count > 0)
                        {
                            throw new InvalidPluginExecutionException("Please resolve all Import Field Mismatches before you proceed with the approval.");
                        }
                    }
                }

                if (PrimaryEntity.Attributes.Contains("cvt_resourcetomatch") &&
                    thisStagingResource.cvt_resourcetomatch != null)
                {
                    mcs_resource tmpResource;
                    using (var srv = new Xrm(OrganizationService))
                        tmpResource = srv.mcs_resourceSet.FirstOrDefault(
                            s => s.mcs_resourceId.Value == thisStagingResource.cvt_resourcetomatch.Id);

                    if (tmpResource?.mcs_RelatedSiteId != null)
                    {
                        PrimaryEntity.Attributes["mcs_relatedsiteid"] = tmpResource.mcs_RelatedSiteId;
                    }
                }

                if (PrimaryEntity.Attributes.Contains("mcs_relatedsiteid") && thisStagingResource.mcs_RelatedSiteId != null)
                {
                    mcs_site site;

                    using (var srv = new Xrm(OrganizationService))
                        site = srv.mcs_siteSet.FirstOrDefault(
                            s => s.mcs_siteId.Value == thisStagingResource.mcs_RelatedSiteId.Id);
                    if (site == null)
                    {
                        Logger.WriteToFile($"Site with {thisStagingResource.mcs_RelatedSiteId.Name} with id:{thisStagingResource.mcs_RelatedSiteId.Id}  not found");
                        return;
                    }

                    if (site.mcs_FacilityId != null)
                    {
                        PrimaryEntity.Attributes["mcs_facility"] = site.mcs_FacilityId;
                    }
                    else
                    {
                        Logger.WriteToFile($"Site {thisStagingResource.mcs_RelatedSiteId.Name} do not have the facility associated");
                        return;
                    }

                }

                if (PrimaryEntity.Attributes.Contains("mcs_facility") && thisStagingResource.mcs_Facility != null)
                {
                    mcs_facility facility;
                    using (var srv = new Xrm(OrganizationService))
                        facility = srv.mcs_facilitySet.FirstOrDefault(
                            f => f.mcs_facilityId.Value == thisStagingResource.mcs_Facility.Id);

                    if (facility?.mcs_VISN != null)
                    {
                        PrimaryEntity.Attributes["mcs_businessunitid"] = facility.mcs_VISN;
                    }
                    else
                    {
                        Logger.WriteToFile($"Facility {thisStagingResource.mcs_Facility.Name} with id:{thisStagingResource.mcs_Facility.Id} do not have the VISN associated");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

    }
}
