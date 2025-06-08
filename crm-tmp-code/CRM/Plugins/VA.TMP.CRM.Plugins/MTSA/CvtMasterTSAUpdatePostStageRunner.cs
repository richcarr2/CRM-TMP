using VA.TMP.DataModel;
using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class CvtMasterTSAUpdatePostStageRunner : PluginRunner
    {
        public CvtMasterTSAUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override void Execute()
        {
            if (PluginExecutionContext.Depth > 1 || PrimaryEntity.Attributes.Contains("ownerid"))
                return;
            var ThisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, cvt_mastertsa.EntityLogicalName, Logger, OrganizationService);
            UpdateMTSAStrings((cvt_mastertsa)ThisRecord, Logger, OrganizationService);
            CvtHelper.AssignOwner(ThisRecord, Logger, OrganizationService);
        }

        /// <summary>
        /// Compare and update Provider and VistA Clinic strings on the MTSA record
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="PrimaryEntity"></param>
        public static void UpdateMTSAStrings(cvt_mastertsa ThisMTSA, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.setMethod = "UpdateMTSAStrings";
            Logger.WriteDebugMessage("starting UpdateMTSAStrings");

            string providerVistaClinics = "", providers = "";
            var updateMTSA = new Entity();

            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var getProviderResources = from provGroups in srv.cvt_providerresourcegroupSet
                                               where provGroups.cvt_RelatedMasterTSAId.Id == ThisMTSA.Id
                                               where provGroups.statecode == 0
                                               select new
                                               {
                                                   provGroups.cvt_RelatedResourceId,
                                                   provGroups.cvt_TSAResourceType,
                                                   provGroups.cvt_Type,
                                                   provGroups.cvt_RelatedUserId,
                                                   provGroups.cvt_RelatedResourceGroupid
                                               };

                    Logger.WriteDebugMessage(string.Format("Completed MTSA Provider Site Resources retrieval. Count: {0}.", getProviderResources.ToList().Count.ToString()));

                    foreach (var provGroups in getProviderResources)
                    {
                        if (provGroups.cvt_Type != null && provGroups.cvt_Type.Value == (int)mcs_resourcetype.VistaClinic && provGroups.cvt_RelatedResourceId != null)
                            providerVistaClinics += provGroups.cvt_RelatedResourceId.Name.ToString() + " ; ";

                        else if (provGroups.cvt_TSAResourceType.Value == (int)cvt_tsaresourcetype.SingleProvider && provGroups.cvt_RelatedUserId != null)
                            providers += provGroups.cvt_RelatedUserId.Name.ToString() + " ; ";

                        else if (provGroups.cvt_TSAResourceType.Value == (int)cvt_tsaresourcetype.ResourceGroup && 
                            provGroups.cvt_RelatedResourceGroupid != null)
                        {
                            var groupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == provGroups.cvt_RelatedResourceGroupid.Id).ToList();
                            if (provGroups.cvt_Type.Value == (int)mcs_resourcetype.Provider || provGroups.cvt_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                            {
                                //TODO: Query the group for the children, loop through and get the providers names.
                                foreach (var groupResource in groupResources)
                                {
                                    if (groupResource.mcs_RelatedUserId != null) //Only providers should be listed on the master TSA, so no check of the user type is needed
                                        providers += groupResource.mcs_RelatedUserId.Name;
                                }
                            }
                            if (provGroups.cvt_Type.Value == (int)mcs_resourcetype.VistaClinic || provGroups.cvt_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                            {
                                foreach (var groupResource in groupResources)
                                {
                                    if (groupResource.mcs_RelatedResourceId != null)
                                    {
                                        var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == groupResource.mcs_RelatedResourceId.Id);
                                        if (resource.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic)
                                            providerVistaClinics += resource.mcs_name;
                                    }
                                }
                            }
                        }
                        //Another branch is VistA Clinic GROUPS
                        //TODO: Query the group for the children, loop through and get the providers names.

                        //Another branch is Paired
                        //TODO: Query the group for the children, loop through and get the providers names.
                    }

                    Logger.WriteDebugMessage(string.Format("Stringified MTSA Provider Site Resources. Providers: {0}; Vista Clinics: {1}.", providers, providerVistaClinics));
                    Entity stringHolder = new Entity() {
                        Attributes = new AttributeCollection()  {
                        { "provider", providers },
                        { "provVista", providerVistaClinics }
                        }};

                    updateMTSA = CvtHelper.UpdateField(updateMTSA, ThisMTSA, stringHolder, "provider", "cvt_providers", true);
                    updateMTSA = CvtHelper.UpdateField(updateMTSA, ThisMTSA, stringHolder, "provVista", "cvt_providersitevistaclinics", true);

                    if (updateMTSA.Attributes.Count > 0)
                    {
                        updateMTSA.Id = ThisMTSA.Id;
                        OrganizationService.Update(updateMTSA);
                        Logger.WriteDebugMessage("MTSA: Providers and/or Provider VistA Clinic fields were updated.");
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
            }
            Logger.WriteDebugMessage("ending UpdateMTSAStrings");
        }
        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_mastertsaplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        #endregion
    }
}