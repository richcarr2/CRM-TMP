using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class McsResourceCreateUpdatePreStageRunner : PluginRunner
    {
        public McsResourceCreateUpdatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Primary Functionality

        /// <summary>
        /// Execute method is the entry point into the runner class, and is what is called by the Actual Plugin: McsResourceCreateUpdatePreStage
        /// </summary>
        /// <remarks>
        /// The plugin checks that the resource record has the default cart type set if the cart is not selected by the user. 
        /// The component type allowed for the resource is filtered based on the cart type selected. 
        /// </remarks>
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != mcs_resource.EntityLogicalName)
                return;

            mcs_resource resource = PrimaryEntity.ToEntity<mcs_resource>();

            if (PrimaryEntity.Attributes.Contains("cvt_systemtype"))
                using (var srv = new Xrm(OrganizationService))
                {
                    //Set the default Cart Type for the resource when the Cart type is not available
                    if (resource.cvt_systemtype != null &&
                        resource.cvt_systemtype.Value !=
                        (int)cvt_carttypecvt_ResourceSystemType.TelehealthPatientCartSystem)
                    {
                        var cartType =
                            srv.cvt_carttypeSet.FirstOrDefault(
                                c => c.cvt_ResourceSystemType.Value == resource.cvt_systemtype.Value);

                        if (cartType == null)
                        {
                            Logger.WriteToFile("Default Cart Type not found for the Resource System Type" +
                                               resource.cvt_systemtype.Value);
                        }
                        else
                        {
                            PrimaryEntity.Attributes["cvt_carttypeid"] =
                                new EntityReference(cvt_carttype.EntityLogicalName, cartType.cvt_carttypeId.Value);
                        }
                    }
                }

            //This code is to ensure that the site doesn't get re-assigned to a different station when the HealthShare has the same station number
            //This could happen when there is multiple site with the same station number in the system
            if (PrimaryEntity.Attributes.Contains("mcs_relatedsiteid") && DoesClinicStationNumberMatchWithCurrentRecordOnClinicUpdate(resource))
            {
                PrimaryEntity.Attributes.Remove("mcs_relatedsiteid");
            }

            //Check Pre Name against format and update if needed
            var derivedName = CvtHelper.ReturnRecordNameIfChanged(resource,
                PluginExecutionContext.MessageName.ToLower() == "create", Logger, OrganizationService);
            Logger.WriteDebugMessage("The derivedName came back, it is: " + derivedName.ToString());

            if (!String.IsNullOrEmpty(derivedName))
            {
                Logger.WriteDebugMessage(
                    "The TMP Resource name should be different, updating it in the UpdatePreStage: " + derivedName + ".");
                PrimaryEntity.Attributes["mcs_name"] = (string)derivedName;
                Logger.WriteDebugMessage("New name as read from the PrimaryEntity: " +
                                         PrimaryEntity.Attributes["mcs_name"].ToString());
            }
            else
            {
                Logger.WriteDebugMessage(
                    "The TMP Resource name should be same as PreStage, so make sure the name is not updated.");
                if (PrimaryEntity.Attributes.Contains("mcs_name"))
                    PrimaryEntity.Attributes.Remove("mcs_name");
            }

            Logger.WriteDebugMessage("End of PreStage Execute method.");
        }

        /// <summary>
        /// This method is to identify whether the the update is being made from HealthShare and ignore any changes to the related site when the Station Number being updated matches with the one in TMP
        /// </summary>
        /// <param name="clinic">The updates to the Clinic record object</param>
        /// <returns>whether the station number matches the earlier value or not</returns>
        private bool DoesClinicStationNumberMatchWithCurrentRecordOnClinicUpdate(mcs_resource clinic)
        {
            var isMatch = false;
            //The Station Number in the clinic resource object currently can only be updated through the HealthShare update or through the Clinic Initial upload (SSIS Package) from the spreadsheet exported from CDW 
            if (PluginExecutionContext.MessageName == "Update" && clinic.mcs_RelatedSiteId != null && !string.IsNullOrWhiteSpace(clinic.cvt_StationNumber))
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var resource = (from f in srv.mcs_facilitySet
                        join site in srv.mcs_siteSet on f.mcs_facilityId.Value equals site.mcs_FacilityId.Id
                        join c in srv.mcs_resourceSet on site.mcs_siteId.Value equals c.mcs_RelatedSiteId.Id
                        where c.mcs_resourceId.Value == clinic.mcs_resourceId.Value
                        select new { siteStationNumber = site.mcs_StationNumber, facilityStationNumber = f.mcs_StationNumber }).FirstOrDefault();

                    if (resource != null && (clinic.cvt_StationNumber == resource.facilityStationNumber || resource.siteStationNumber.Contains(clinic.cvt_StationNumber)))
                    {
                        isMatch = true;
                    }
                }
            }
            return isMatch;
        }

        #endregion

        #region AbstractClassRequiredMethods
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}