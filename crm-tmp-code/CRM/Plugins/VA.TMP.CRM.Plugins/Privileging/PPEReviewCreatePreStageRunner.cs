using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class PPEReviewCreatePreStageRunner : PluginRunner
    {
        public PPEReviewCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for PPE Review Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Update the Next Date to tomorrow 6am
        /// </summary>
        public override void Execute()
        {
            //Update this PPE Review record's next date
            if (PrimaryEntity.Attributes.Contains("cvt_nextemail"))
            {               
                var initiatedDate = ((DateTime)PrimaryEntity.Attributes["cvt_initiateddate"]).ToLocalTime();
                var nextEmailDate = ((DateTime)PrimaryEntity.Attributes["cvt_nextemail"]).ToLocalTime();

                var tomorrowDate = new DateTime(nextEmailDate.Year, nextEmailDate.Month, nextEmailDate.Day + 1, 6, 0, 0);
                PrimaryEntity.Attributes["cvt_nextemail"] = tomorrowDate;
                Logger.WriteDebugMessage("Newly Updated Next Email Datetime as read from the context (prestage): " + PrimaryEntity.Attributes["cvt_nextemail"].ToString());

                //Update the name
                var PrivER = (EntityReference)PrimaryEntity.Attributes["cvt_telehealthprivileging"];
                var thisPriv = OrganizationService.Retrieve(cvt_tssprivileging.EntityLogicalName, PrivER.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                var priv = (cvt_tssprivileging)thisPriv;

                var name = priv.cvt_name;
                name += " Review (" + initiatedDate.ToString("yyyy/MM/dd") + ")";
                PrimaryEntity.Attributes["cvt_name"] = name;
                Logger.WriteDebugMessage("Built the name: " + name);

                Logger.WriteDebugMessage("Update the Team to do the summary.");
                using (var srv = new Xrm(OrganizationService))
                {
                    var findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == ((EntityReference)PrimaryEntity.Attributes["cvt_facility"]).Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.CredentialingandPrivileging);

                    if (findTeam != null)
                    {
                        Team updateTeam = new Team()
                        {
                            Id = findTeam.Id,
                            cvt_nextppeemail = tomorrowDate
                        };
                        OrganizationService.Update(updateTeam);
                        Logger.WriteDebugMessage("Updated the Team with the next PPE date.");
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