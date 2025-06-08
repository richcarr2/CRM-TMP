using System;
using System.Activities;
using System.Linq;
using MCSShared;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.Vista;
using VA.TMP.Integration.Plugins.Helpers;

namespace VA.TMP.Integration.CustomActivities
{
    public class ParticipatingSiteCanBeScheduledRunner : CustomActionRunner
    {
        public ParticipatingSiteCanBeScheduledRunner(CodeActivityContext context) : base(context) { }

        public override string McsSettingsDebugField => "cvt_participatingsiteplugin";

        //private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        public Guid ParticipatingSiteId { get; set; }

        public string ErrorMessage { get; private set; }

        public override void Execute(AttributeCollection attributes)
        {
            if(Guid.TryParse(attributes.FirstOrDefault(a => a.Key.Equals("ParticipatingSiteId")).Value.ToString(), out var siteId))
            {
                Logger.WriteDebugMessage($"Participating Site Id: {siteId}");
                ParticipatingSiteId = siteId;
                ProcessParticipatingSite();
            }
            else
            {
                ErrorMessage = "Error: Participating Site Id is Missing";
            }
        }

        private void ProcessParticipatingSite()
        {
            using (var svc = new Xrm(OrganizationService))
            {
                var participatingSite = svc.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id.Equals(ParticipatingSiteId));

                if (participatingSite == null) ErrorMessage = $"Error: No Participating Site was found for Id: {ParticipatingSiteId}";

                var resourcePackage = svc.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == participatingSite.cvt_resourcepackage.Id);

                if (resourcePackage == null)
                {
                    Logger.WriteDebugMessage("Error: Participating Site's Scheduling Package could not be found.");
                    ErrorMessage = "Error: Participating Site's Scheduling Package could not be found.";
                }

                Logger.WriteDebugMessage("Checking Usage.");
                if (resourcePackage.cvt_usagetype != null && resourcePackage.cvt_usagetype.Value == true)
                {
                    //TSA Type
                    Logger.WriteDebugMessage("Usage Type is TSA, no service build needed.");
                    return;
                }
                else
                    Logger.WriteDebugMessage("Usage Type is Scheduling, continuing.");

                var scheduleable = participatingSite.cvt_scheduleable.Value;

                #region Call Service Generation Entry Point

                if (scheduleable)
                {
                    Logger.WriteDebugMessage("Change to Scheduleable or State detected.");
                    CvtHelper.EntryFromParticipatingSite(participatingSite, Logger, OrganizationService);
                }
                else
                    Logger.WriteDebugMessage("Participating Site is not Scheduleable.");
                #endregion
            }
        }
    }
}
