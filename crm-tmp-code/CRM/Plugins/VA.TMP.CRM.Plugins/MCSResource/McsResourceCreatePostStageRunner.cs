using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    /// <summary>
    ///1.) If the user had hit create resource from the resource group, create a group resource record to join the resource and resource group.
    ///2.) If the user had hit create resource from the provider resource group grid, create a provider resource group record to join the TSA or MTSA to the resource
    ///      a.) There are 2 text fields which are used to designate if it was from a TSA or MTSA
    ///3.) If the user had hit create resource from the patient resource group grid, create a patient resource group record to join the TSA or MTSA to the resource
    ///4.) Create a system resource (really equipment) to actually have the calendar for this resource
    ///5.) Find the default calendar and make the new system resource have that calendar instead of the OOB one.
    ///6.) Assign the resource to the site team.
    /// </summary>
    public class McsResourceCreatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        public override void Execute()
        {
            if (PluginExecutionContext.InputParameters.Contains("Target") && PluginExecutionContext.InputParameters["Target"] is Entity)
            {
                var thisResource = CvtHelper.ValidateReturnRecord(PrimaryEntity, mcs_resource.EntityLogicalName, Logger, OrganizationService);
                //Create the system resource
                var equipmentId = CreateEquipment(PluginExecutionContext.PrimaryEntityId);

                BackfillResourceDetails(equipmentId);
                //see if they are creating this from resource group
                var quickCreateOptionResourceGroup = McsHelper.getStringValue("mcs_resourcegroupguid");
                var doQuickCreateResourceGroup = quickCreateOptionResourceGroup != null;
                Logger.WriteDebugMessage(String.Format("doQuickCreate:{0}{1}", doQuickCreateResourceGroup, doQuickCreateResourceGroup));

                if (doQuickCreateResourceGroup)
                    QuickCreateGroupResource(PluginExecutionContext.PrimaryEntityId);
                else
                    Logger.WriteDebugMessage("Not doing resource group quick create.");

                //if the mcs_tsaguid is present then it is because they are doing a quick create
                //but we have to know which one, pat or prov.
                //These 3 fields are populated from jscript on a ribbon button.
                var quickCreateOptionPatProv = McsHelper.getStringValue("mcs_tsaguid");
                var quickCreatePat = McsHelper.getBoolValue("mcs_createpatientr");
                var quickCreateProv = McsHelper.getBoolValue("mcs_createproviderr");                
                var doQuickCreatePatProv = quickCreateOptionPatProv != null;

                if (!doQuickCreatePatProv)
                {
                    Logger.WriteDebugMessage("Not doing Quick Create for TSA, maybe MTSA");
                    //if the mcs_mastertsaguid is present then it is because they are doing a quick create
                    quickCreateOptionPatProv = McsHelper.getStringValue("cvt_mastertsaguid");
                    doQuickCreatePatProv = quickCreateOptionPatProv != null;
                }
                Logger.WriteDebugMessage(String.Format("doQuickCreatePatProv:{0}{1}", doQuickCreatePatProv, quickCreateOptionPatProv));

                if (doQuickCreatePatProv)
                {
                    if (quickCreatePat) //call the method to do a quick create for pat group
                        QuickCreatePatientResourceGroup(PluginExecutionContext.PrimaryEntityId);
                    else if (quickCreateProv)//call the method to do a quick create for provider group
                        QuickCreateProviderResourceGroup(PluginExecutionContext.PrimaryEntityId);
                }
            }
        }

        #region Methods
        /// <summary>
        /// Create the Group Resource record
        /// </summary>
        /// <param name="primaryEntityId"></param>
        internal void QuickCreateGroupResource(Guid primaryEntityId)
        {
            Logger.setMethod = "QuickCreateGroupResource";
            Logger.WriteDebugMessage("Starting");
            try
            {
                var newGroupResource = new mcs_groupresource()
                    {
                        mcs_relatedResourceGroupId = new EntityReference(mcs_resourcegroup.EntityLogicalName, new Guid(McsHelper.getStringValue("mcs_resourcegroupguid"))),
                        mcs_relatedSiteId = new EntityReference(mcs_site.EntityLogicalName, McsHelper.getEntRefID("mcs_relatedsiteid")),
                        mcs_RelatedResourceId = new EntityReference(mcs_resource.EntityLogicalName, primaryEntityId),
                        mcs_name = McsHelper.getStringValue("mcs_name"),
                        mcs_Type = new OptionSetValue(McsHelper.getOptionSetValue("mcs_type"))
                    };

                OrganizationService.Create(newGroupResource);
                Logger.WriteDebugMessage("Created group resource record. Ending QuickCreateGroupResource");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
            }
        }

        /// <summary>
        /// Create New Patient Resource Group
        /// </summary>
        /// <param name="primaryEntityId"></param>
        internal void QuickCreatePatientResourceGroup(Guid primaryEntityId)
        {
            try
            {
                Logger.setMethod = "QuickCreatePatientResourceGroup";
                Logger.WriteDebugMessage("Starting");
                var newGroupResource = new cvt_patientresourcegroup()
                {
                    cvt_TSAResourceType = new OptionSetValue((int)cvt_tsaresourcetype.ResourceGroup),
                    cvt_type = new OptionSetValue(McsHelper.getOptionSetValue("mcs_type")),
                    cvt_relatedsiteid = new EntityReference(mcs_site.EntityLogicalName, McsHelper.getEntRefID("mcs_relatedsiteid")),
                    cvt_RelatedResourceId = new EntityReference(mcs_resource.EntityLogicalName, primaryEntityId),
                    cvt_name = McsHelper.getStringValue("mcs_name")
                };

                var tsapresent = McsHelper.getStringValue("cvt_relatedtsaid");
                if (tsapresent != null)
                    newGroupResource.cvt_RelatedTSAid = new EntityReference("cvt_facilityapproval", new Guid(tsapresent));

                OrganizationService.Create(newGroupResource);
                Logger.WriteDebugMessage("Created Patient resource group record. Ending QuickCreatePatientResourceGroup");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile("Error creating new Group Resource:" + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Error creating new Group Resource:" + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        /// <summary>
        /// Create New Provider Resource Group
        /// </summary>
        /// <param name="primaryEntityId"></param>
        internal void QuickCreateProviderResourceGroup(Guid primaryEntityId)
        {
            try
            {
                Logger.setMethod = "QuickCreateProviderResourceGroup";
                Logger.WriteDebugMessage("Starting");
                var newGroupResource = new cvt_providerresourcegroup()
                {
                    cvt_TSAResourceType = new OptionSetValue((int)cvt_tsaresourcetype.ResourceGroup),
                    cvt_Type = new OptionSetValue(McsHelper.getOptionSetValue("mcs_type")),
                    cvt_relatedsiteid = new EntityReference(mcs_site.EntityLogicalName, McsHelper.getEntRefID("mcs_relatedsiteid")),
                    cvt_RelatedResourceId = new EntityReference(mcs_resource.EntityLogicalName, primaryEntityId),
                    cvt_name = McsHelper.getStringValue("mcs_name")
                };
                
                var mastertsapresent = McsHelper.getStringValue("cvt_mastertsaguid");
                if (mastertsapresent != null)
                    newGroupResource.cvt_RelatedMasterTSAId = new EntityReference(cvt_mastertsa.EntityLogicalName, new Guid(mastertsapresent));

                //var tsapresent = McsHelper.getStringValue("mcs_tsaguid");
                var tsapresent = McsHelper.getStringValue("cvt_relatedtsaid");
                if (tsapresent != null)
                    newGroupResource.cvt_RelatedTSAid = new EntityReference(cvt_facilityapproval.EntityLogicalName, new Guid(tsapresent));

                OrganizationService.Create(newGroupResource);
                Logger.WriteDebugMessage("Created Provider resource group record. Ending QuickCreateProviderResourceGroup.");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile("Error creating new Group Resource:" + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Error creating new Group Resource:" + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        internal void BackfillResourceDetails(Guid EquipmentId)
        {
                var ThisResource = OrganizationService.Retrieve(mcs_resource.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<mcs_resource>();
                if (ThisResource.mcs_RelatedSiteId == null)
                    return;
                var ResourceSite = OrganizationService.Retrieve(mcs_site.EntityLogicalName, ThisResource.mcs_RelatedSiteId.Id, new ColumnSet(true)).ToEntity<mcs_site>();

                //Associating newly created equipment back to original Resource
                var UpdateResource = new mcs_resource()
                {
                    Id = PrimaryEntity.Id,
                    mcs_relatedResourceId = new EntityReference(Equipment.EntityLogicalName, EquipmentId),
                    cvt_Identifier = PrimaryEntity.Id.ToString()
                };
                Logger.WriteDebugMessage($"Updated Equipment Reference {EquipmentId} on Resource record");

                if (ResourceSite != null)
                {
                    //Update TMP Resource's VISN field
                    if (ResourceSite.mcs_BusinessUnitId != null && ((ThisResource.mcs_BusinessUnitId == null) || (ThisResource.mcs_BusinessUnitId.Id != ResourceSite.mcs_BusinessUnitId.Id)))
                        UpdateResource.mcs_BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, ResourceSite.mcs_BusinessUnitId.Id);

                //Update TMP Resource's Facility field
                    var thisResourceFacility = (ThisResource.mcs_Facility != null) ? ThisResource.mcs_Facility.Id : Guid.Empty;
                    var siteFacility = (ResourceSite.mcs_FacilityId != null) ? ResourceSite.mcs_FacilityId.Id : Guid.Empty;

                if ((thisResourceFacility == Guid.Empty && siteFacility != Guid.Empty) || (thisResourceFacility != siteFacility))
                    {
                        if (ResourceSite.mcs_FacilityId != null)
                        {
                            UpdateResource.mcs_Facility = new EntityReference(mcs_facility.EntityLogicalName,
                                ResourceSite.mcs_FacilityId.Id);
                        }
                    }
                }

                OrganizationService.Update(UpdateResource);
                Logger.WriteDebugMessage("TSS Resource Updated");
        }

        /// <summary>
        /// Starting the creation of a System Resource which will be associated with target MCS Resource.
        /// </summary>
        /// <param name="ResourceId"></param>
        internal Guid CreateEquipment(Guid ResourceId)
        {
            Logger.WriteDebugMessage("Starting CreateEquipment");

            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var ThisResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == ResourceId);

                    if (ThisResource == null)
                        throw new InvalidPluginExecutionException($"Resource with Id: {ResourceId} not found, Equipment Creation Terminated.");

                    if (ThisResource.mcs_RelatedSiteId == null)
                        throw new InvalidPluginExecutionException("All Resources must have a Site.");

                    var RelatedSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == ThisResource.mcs_RelatedSiteId.Id);
                    if (RelatedSite == null)
                        throw new InvalidPluginExecutionException("Site not found, Equipment Creation Terminated.");

                    var facility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == RelatedSite.mcs_FacilityId.Id);
                    if (facility == null)
                        throw new InvalidPluginExecutionException("Facility not found, Equipment Creation Terminated.");

                    Logger.WriteDebugMessage("Initiating Equipment Creation. Name: " + ThisResource.mcs_name);


                    var NewRelatedEquipment = new Equipment
                    {
                        Name = ThisResource.mcs_name ?? "Equipment",
                        cvt_type = ThisResource.mcs_Type,
                        cvt_capacity = ThisResource.cvt_capacity != null ? ThisResource.cvt_capacity.Value : 1,
                        SiteId = RelatedSite.mcs_RelatedActualSiteId,
                        TimeZoneCode = RelatedSite.mcs_TimeZone,
                        mcs_relatedresource = new EntityReference(mcs_resource.EntityLogicalName, ResourceId),
                        BusinessUnitId = facility.mcs_BusinessUnitId ?? RelatedSite.mcs_BusinessUnitId
                    };

                    if (ThisResource.mcs_Type != null && ThisResource.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic) //For Vista Clinics
                        NewRelatedEquipment.cvt_capacity = (ThisResource.cvt_vistacapacity != null) ? ThisResource.cvt_vistacapacity.Value : 1;

                    var CreatedEquipment = OrganizationService.Create(NewRelatedEquipment);
                    Logger.WriteDebugMessage("Equipment Added: " + CreatedEquipment.ToString());

                    //Updating the default calendar hours for the System Resource that was created. 
                    CvtHelper.ChangeNewlyCreatedCalendar(CreatedEquipment, OrganizationService, Logger, McsSettings);

                    Logger.setMethod = "CreateEquipment";
                    Logger.WriteDebugMessage("Calendar Updated");
                    return CreatedEquipment;
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
        #endregion

        #region Additional Interface methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}