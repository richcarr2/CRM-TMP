using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class ParticipatingSiteCreatePreStageRunner : PluginRunner
    {
        public ParticipatingSiteCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for Participating Site Pre Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Ensure the name is correct
        /// </summary>
        public override void Execute()
        {
            Logger.WriteDebugMessage("About to retrieve the derived name.");
            //Check Pre Name against format and update if needed
            var derivedName = CvtHelper.ReturnRecordNameIfChanged(PrimaryEntity.ToEntity<cvt_participatingsite>(), true, Logger, OrganizationService);
            Logger.WriteDebugMessage("The derivedName came back, it is: " + derivedName.ToString());

            if (!String.IsNullOrEmpty(derivedName))
            {
                Logger.WriteDebugMessage(String.Format("The Participating Site name should be different than {0}, updating it in the CreatePreStage to: {1}.", PrimaryEntity.Attributes["cvt_name"].ToString(), derivedName));
                PrimaryEntity.Attributes["cvt_name"] = (string)derivedName;
                Logger.WriteDebugMessage("New name as read from the PrimaryEntity: " + PrimaryEntity.Attributes["cvt_name"].ToString());
            }
            else
            {
                Logger.WriteDebugMessage("No change made to the name.  The Participating Site name is already correct.");
            }

            VerifyRequirements(PrimaryEntity.ToEntity<cvt_participatingsite>());
            Logger.WriteDebugMessage("End of PreStageCreate Execute method.");
        }

        internal void VerifyRequirements(cvt_participatingsite thisRecord)
        {
            //Query Scheduling Package
            using (var srv = new Xrm(OrganizationService))
            {
                //Verify this isn't a duplicate PS
                var duplicatePS = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.cvt_resourcepackage.Id == thisRecord.cvt_resourcepackage.Id && ps.cvt_site.Id == thisRecord.cvt_site.Id && ps.cvt_locationtype == thisRecord.cvt_locationtype);
                if (duplicatePS != null)
                    throw new InvalidPluginExecutionException("This Site has already been added in this location type on this Scheduling Package. Please make edits to the existing Participating Site.");
                //Get Scheduling Package
                if (!thisRecord.Contains("cvt_resourcepackage") || thisRecord.cvt_resourcepackage == null)
                    throw new InvalidPluginExecutionException("Participating Site's Scheduling Package is missing.");
                var parentSchedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(sp => sp.Id == thisRecord.cvt_resourcepackage.Id);
                if (parentSchedulingPackage == null)
                    throw new InvalidPluginExecutionException("Scheduling Package could not be retrieved.");
                if (parentSchedulingPackage.cvt_patientlocationtype == null)
                    throw new InvalidPluginExecutionException("Associated Scheduling Package is missing a Patient Location Type.");
                if (parentSchedulingPackage.cvt_providerfacility == null)
                    throw new InvalidPluginExecutionException("Associated Scheduling Package is missing a Provider Facility.");
                var isGroup = parentSchedulingPackage.cvt_groupappointment.Value;
                if (isGroup && parentSchedulingPackage.cvt_patientfacility == null)
                    throw new InvalidPluginExecutionException("Associated Group Scheduling Package is missing a Patient Facility.");
                if (parentSchedulingPackage.cvt_intraorinterfacility == null)
                    throw new InvalidPluginExecutionException("Associated Scheduling Package is missing an intrafacility or interfacility indicator.");
                var isIntra = (parentSchedulingPackage.cvt_intraorinterfacility.Value == (int)cvt_resourcepackagecvt_intraorinterfacility.Intrafacility) ? true : false;
                //Get TMP Site
                if (!thisRecord.Contains("cvt_site") || thisRecord.cvt_site == null)
                    throw new InvalidPluginExecutionException("Participating Site's Site is missing.");
                var thisPSSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == thisRecord.cvt_site.Id);

                //Get PS fields
                if (!thisRecord.Contains("cvt_locationtype") || thisRecord.cvt_locationtype == null)
                    throw new InvalidPluginExecutionException("Participating Site's Location type is missing.");
                var thisPSLocation = thisRecord.cvt_locationtype.Value;

                //Provider
                if (thisPSLocation == (int)cvt_participatingsitecvt_locationtype.Provider)
                {
                    //All Participating Sites where Location Type=Provider are TMP Sites that exist underneath the Facility specified in "Provider Facility." 
                    if (thisPSSite.mcs_FacilityId.Id != parentSchedulingPackage.cvt_providerfacility.Id)
                        throw new InvalidPluginExecutionException("This TMP Site does not exist under the Provider Facility specified on the Scheduling Package. Please choose a different Site or create a new Scheduling Package for that facility.");
                }
                //Patient
                else if (thisPSLocation == (int)cvt_participatingsitecvt_locationtype.Patient)
                {
                    if (!isGroup)
                    {
                        if (isIntra) //non-group intra
                        {
                            if (thisPSSite.mcs_FacilityId.Id != parentSchedulingPackage.cvt_providerfacility.Id)
                                throw new InvalidPluginExecutionException("This TMP Site does not exist under the Provider Facility specified on this Intrafacility Scheduling Package. Please choose a different Site or create a new Interfacility Scheduling Package.");
                        }
                        else //Non group inter
                        {
                            if (thisPSSite.mcs_FacilityId.Id == parentSchedulingPackage.cvt_providerfacility.Id)
                                throw new InvalidPluginExecutionException("Intrafacility sites cannot be added to this Interfacility Scheduling Package. Add a TMP Site from a different facility or create a new Intrafacility Scheduling Package.");
                        }
                    }
                    else //Group Patient, must match the facility listed
                    {
                        //Verify that Patient Facility has data
                        if (parentSchedulingPackage.cvt_patientfacility == null)
                            throw new InvalidPluginExecutionException("Associated Group Scheduling Package is missing a Patient Facility.");
                        if (isIntra) //group intra
                        {
                            if (thisPSSite.mcs_FacilityId.Id != parentSchedulingPackage.cvt_patientfacility.Id)
                                throw new InvalidPluginExecutionException("This TMP Site does not exist under the Patient Facility specified on this Intrafacility Scheduling Package. Please choose a different Site or create a new Interfacility Scheduling Package.");
                        }
                        else //group inter
                        {
                            if (thisPSSite.mcs_FacilityId.Id != parentSchedulingPackage.cvt_patientfacility.Id)
                                throw new InvalidPluginExecutionException("Intrafacility sites cannot be added to this Interfacility Scheduling Package. Add a TMP Site from a different facility or create a new Intrafacility Scheduling Package.");
                        }
                    }
                }
            }
        }

        #region Additional Interface Methods/Properties
        /// <summary>
        /// Used for Debugging - turns on or off creation of log records for this particular entity
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_ppereview"; }
        }
        #endregion
    }
}