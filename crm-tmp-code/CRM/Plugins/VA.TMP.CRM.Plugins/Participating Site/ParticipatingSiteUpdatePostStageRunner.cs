using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using VA.TMP.OptionSets;
using MCSShared;

namespace VA.TMP.CRM
{
    class ParticipatingSiteUpdatePostStageRunner : PluginRunner
    {
        public ParticipatingSiteUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override void Execute()
        {
            Logger.WriteDebugMessage("Starting Execute");
            //var participatingSite = PrimaryEntity.ToEntity<cvt_participatingsite>();
            using (var srv = new Xrm(OrganizationService))
            {
                //ONLY continue if usage is Scheduling
                //Get the PS, then the SP
                var thisParticipatingSite = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == PrimaryEntity.Id);
                if (thisParticipatingSite == null)
                {
                    Logger.WriteDebugMessage("Error: Participating Site could not be found.");
                    throw new InvalidPluginExecutionException("Error: Participating Site could not be found.");
                }

                var resourcePackage = srv.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == thisParticipatingSite.cvt_resourcepackage.Id);
                if (resourcePackage == null)
                {
                    Logger.WriteDebugMessage("Error: Participating Site's Scheduling Package could not be found.");
                    throw new InvalidPluginExecutionException("Error: Participating Site's Scheduling Package could not be found.");
                }
                //r4.6v4 Check the Usage Type for SP
                //By default, if no value or if false, then it is scheduling.
                //if true, then  it is TSA
                Logger.WriteDebugMessage("Checking Usage.");
                if (resourcePackage.cvt_usagetype != null && resourcePackage.cvt_usagetype.Value == true)
                {
                    //TSA Type
                    Logger.WriteDebugMessage("Usage Type is TSA, no service build needed.");
                    return;
                }
                else
                    Logger.WriteDebugMessage("Usage Type is Scheduling, continuing.");
                //throw new NotImplementedException();
                // get pr/post images to compare
                cvt_participatingsite psPre = PluginExecutionContext.PreEntityImages["PreImage"]?.ToEntity<cvt_participatingsite>();
                cvt_participatingsite psPost = PluginExecutionContext.PostEntityImages["PostImage"]?.ToEntity<cvt_participatingsite>();
                //Logger.WriteDebugMessage("Got Images.");

                #region COMPARE 'Schedulable' property
                // if the 'schedulable' field has changed, we want to call the "EntryFromParticipatingSite" Entry Point for Service Generation
                bool preSchedulable = false;
                bool postSchedulable = false;
                bool changedSchedulable = false;

                if (psPre != null)
                {
                    if (psPre.Attributes.Contains("cvt_scheduleable"))
                    {
                        var accessor = psPre["cvt_scheduleable"];
                        preSchedulable = (accessor.ToString() == "True") ? true : false;
                    }
                }

                //Logger.WriteDebugMessage("Step 1");
                if (psPost != null)
                {
                    if (psPost.Attributes.Contains("cvt_scheduleable"))
                    {
                        var accessor = psPost["cvt_scheduleable"];
                        postSchedulable = (accessor.ToString() == "True") ? true : false;
                    }
                }

                //Logger.WriteDebugMessage("Step 2");
                if (preSchedulable != postSchedulable)
                {
                    changedSchedulable = true;
                }
                //Logger.WriteDebugMessage("Finished checking for change to scheduleable.");
                #endregion

                #region Compare Pre/Post State
                // if the record status has changed, we want to call the "EntryFromParticipatingSite" Entry Point for Service Generation

                int preState = 0;
                int postState = 0;
                if (psPre != null)
                {
                    preState = (int)psPre.statuscode.Value;
                }
                if (psPost != null)
                {
                    postState = (int)psPost.statuscode.Value;
                }
                bool changedStatus = false;

                if (preState != postState)
                {
                    changedStatus = true;
                }
                //Logger.WriteDebugMessage("Finished checking for change to State.");
                #endregion

                #region Call Service Generation Entry Point

                if (changedSchedulable || changedStatus)
                {
                    Logger.WriteDebugMessage("Change to Scheduleable or State detected.");
                    CvtHelper.EntryFromParticipatingSite(thisParticipatingSite, Logger, OrganizationService);
                }
                else
                    Logger.WriteDebugMessage("Change to Scheduleable or State not detected.");
                #endregion
            }
        }

        public override string McsSettingsDebugField
        {
            get { return "cvt_participatingsiteplugin"; }
        }

    }
}
