using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsResourceDeletePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceDeletePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            CvtHelper.ValidateReturnRecord(PrimaryEntity, mcs_resource.EntityLogicalName, Logger, OrganizationService);
            CheckComponents(PrimaryEntity.Id);
            CheckAssociationsDeleteEquipment(PrimaryEntity.Id);
        }

        /// <summary>
        /// Checking Resource for child components. Prevent Delete.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        internal void CheckComponents(Guid resourceId)
        {
            Logger.setMethod = "CheckComponents";
            Logger.WriteDebugMessage("Starting CheckComponents");
            using (var srv = new Xrm(OrganizationService))
            {
                //Query for a components associated to this resouce
                var ChildComponents = srv.cvt_componentSet.FirstOrDefault(c => c.cvt_relatedresourceid.Id == resourceId);

                if (ChildComponents != null)
                    throw new InvalidPluginExecutionException("customDelete canceled. Retry delete after you have re-assigned all related Components to another Technology Resource.");
            }
            Logger.WriteDebugMessage("Ending CheckComponents");
        }

        /// <summary>
        /// Check for associations, if found, prevent Delete of Equipment
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        internal void CheckAssociationsDeleteEquipment(Guid resourceId)
        {
            Logger.setMethod = "CheckAssociationsDeleteEquipment";
            Logger.WriteDebugMessage("Starting CheckAssociationsDeleteEquipment");
            using (var srv = new Xrm(OrganizationService))
            {
                //Query for an equipment record associated to this resource
                var SystemEquipment = srv.EquipmentSet.FirstOrDefault(i => i.mcs_relatedresource.Id == resourceId);

                //No Equipment to delete, return
                if (SystemEquipment == null)
                    return;
                Logger.WriteDebugMessage("Retrieved the Resource's Equipment record: " + SystemEquipment.Name);

                //Check for Activity Parties
                var ActivityParties = srv.ActivityPartySet.Where(ap => ap.PartyId.Id == SystemEquipment.Id).ToList();
                if (ActivityParties.Count > 0)
                    throw new InvalidPluginExecutionException(String.Format("customTSS Resource Delete prevented. Reason: Did not delete because it exists on {0} activity record(s).", ActivityParties.Count.ToString()));
                //If associations exist, we will throw an exception with a message and stop the delete of the MCS Resource, to prevent orphan data.
                var CustomValidationMessage = "";

                //Check for Patient Site Resource associations 
                var PatientResources = srv.cvt_patientresourcegroupSet.Where(prg => prg.cvt_RelatedResourceId.Id == resourceId).ToList();
                if (PatientResources.Count > 0)
                    CustomValidationMessage += string.Format(" Patient Site Resources ({0}).", PatientResources.Count.ToString());

                //Check for Provider Site Resource associations 
                var ProviderResources = srv.cvt_providerresourcegroupSet.Where(prg => prg.cvt_RelatedResourceId.Id == resourceId).ToList();
                if (ProviderResources.Count > 0)
                    CustomValidationMessage += string.Format(" Provider Site Resources ({0}).", ProviderResources.Count.ToString());

                //Check for Group Resource associations
                var GroupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_RelatedResourceId.Id == resourceId).ToList();
                if (GroupResources.Count > 0)
                    CustomValidationMessage += string.Format(" Group Resources ({0}).", GroupResources.Count.ToString());

                if (CustomValidationMessage != "")
                    throw new InvalidPluginExecutionException("customPlease check Left Nav on Resource form for the following associations.  Resource cannot be deleted until these associations are removed." + CustomValidationMessage);
                else
                {
                    OrganizationService.Delete(Equipment.EntityLogicalName, SystemEquipment.Id);
                    Logger.WriteDebugMessage("System Resource Deleted");
                }
            }
            Logger.WriteDebugMessage("Ending CheckAssociationsDeleteEquipment");
        }
        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
        }
        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }   
        #endregion
    }
}