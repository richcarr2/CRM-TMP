using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class SchedulingResourceCreatePreStageRunner : PluginRunner
    {
        public SchedulingResourceCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for Scheduling Resource Pre Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Ensure the name is correct
        /// </summary>
        public override void Execute()
        {
            //It must be related to a PS and that must be related to a SP
            CheckRequirements();

            Logger.WriteDebugMessage("About to retrieve the derived name.");
            cvt_schedulingresource thisRecord = PrimaryEntity.ToEntity<cvt_schedulingresource>();
            //Check Pre Name against format and update if needed
            var derivedName = CvtHelper.ReturnRecordNameIfChanged(thisRecord, true, Logger, OrganizationService);
            Logger.WriteDebugMessage("The derivedName came back, it is: " + derivedName.ToString());

            if (!String.IsNullOrEmpty(derivedName))
            {
                Logger.WriteDebugMessage(String.Format("The Scheduling Resource name should be different than {0}, updating it in the CreatePreStage to: {1}.", PrimaryEntity.Attributes["cvt_name"].ToString(), derivedName));
                PrimaryEntity.Attributes["cvt_name"] = (string)derivedName;
                Logger.WriteDebugMessage("New name as read from the PrimaryEntity: " + PrimaryEntity.Attributes["cvt_name"].ToString());
            }
            else
            {
                Logger.WriteDebugMessage("No change made to the name.  The Scheduling Resource name is already correct.");
            }

            //Checking that a SR is not for a Provider site - parent SP SFT
            if (thisRecord.cvt_participatingsite != null)
            {
                CvtHelper.ValidatePS(thisRecord.cvt_participatingsite.Id, thisRecord, Logger, OrganizationService);
            }

            Logger.WriteDebugMessage("End of PreStageCreate Execute method.");
        }     

        public void CheckRequirements()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                cvt_schedulingresource thisSR = PrimaryEntity.ToEntity<cvt_schedulingresource>();
                if (thisSR.cvt_participatingsite != null && thisSR.cvt_participatingsite.Id != Guid.Empty)
                {
                    Logger.WriteDebugMessage("There is a related Participating Site. Continuing.");
                    var thisPS = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == thisSR.cvt_participatingsite.Id);
                    if (thisPS != null && thisPS.cvt_scheduleable.Value == true)
                        throw new InvalidPluginExecutionException("customScheduling Resource cannot be added to a 'Can Be Scheduled' Participating Site.  Change this Participating Site to NO, save it, and try again.");
                    else
                        Logger.WriteDebugMessage("PS is not scheduleable, continuing.");

                    if (thisPS != null && thisPS.cvt_resourcepackage != null && thisPS.cvt_resourcepackage.Id != Guid.Empty)
                    {
                        var thisSP = srv.cvt_resourcepackageSet.FirstOrDefault(sp => sp.Id == thisPS.cvt_resourcepackage.Id);
                        if (thisSP != null)
                        {
                            Logger.WriteDebugMessage("Found this SP. Evaluating conditions now");
                            if (thisSP.cvt_groupappointment.Value == true && thisPS.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient)
                            {
                                Logger.WriteDebugMessage("SP is Group and PS is Patient, can only be Paired Resource Group.");
                                //Check this Scheduling Resource
                                if (thisSR.cvt_tmpresourcegroup != null && thisSR.cvt_tmpresourcegroup.Id != Guid.Empty)
                                {
                                    //It is a Resource Group
                                    if (thisSR.cvt_resourcetype != null && thisSR.cvt_resourcetype.Value != (int)mcs_resourcetype.PairedResourceGroup)
                                    {
                                        throw new InvalidPluginExecutionException("customScheduling Resource must be a Paired Resource Group on a Group's Patient Site.");
                                    }
                                }
                                else
                                    throw new InvalidPluginExecutionException("customScheduling Resource must be a Paired Resource Group on a Group's Patient Site.");
                            }

                            //Used to not be able to add any resources to SFT PRO, now you can
                            //if (thisSP.cvt_availabletelehealthmodality.Value == (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward && thisPS.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider)
                            //{
                            //    Logger.WriteDebugMessage("SP is SFT and PS is Provider, you cannot add add any resources.");
                            //    throw new InvalidPluginExecutionException("customScheduling Resource cannot be added on a SFT's Provider Site.");
                            //}
                        }
                        else
                            throw new InvalidPluginExecutionException("customScheduling Resource must be associated to a Scheduling Package.");

                    }
                    else
                        throw new InvalidPluginExecutionException("customScheduling Resource must be associated to a Participating Site.");
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