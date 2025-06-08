using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// The purpose of this plugin is to create the system service entries needed for services to work - this is done on create and may have to be updated on update depending on
    /// the business rule that is decided.
    /// </summary>
    public class McsServiceUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsServiceUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            Logger.WriteDebugMessage("plugin registered on deprecated entity 'mcs_services'");
            //Logger.setMethod = "Execute";
            ////Assign owner
            //var thisTSA = CvtHelper.ValidateReturnRecord(PrimaryEntity, mcs_services.EntityLogicalName, Logger, OrganizationService);

            ////start the timing for the submethod
            //if (PluginExecutionContext.Depth == 1 && (PrimaryEntity.Contains("cvt_bulkaddresource")) ||
            //    PrimaryEntity.Contains("cvt_bulkaddresourcegroup") ||
            //    PrimaryEntity.Contains("cvt_bulkadduser") ||
            //    PrimaryEntity.Contains("cvt_bulkremoveresource") ||
            //    PrimaryEntity.Contains("cvt_bulkremoveresourcegroup") ||
            //    PrimaryEntity.Contains("cvt_bulkremoveuser"))
            //{
            //    BulkEditResource(thisTSA);
            //}

            //Logger.WriteGranularTimingMessage("Starting CreateUpdateService");
            //CvtHelper.CreateUpdateService(PluginExecutionContext.PrimaryEntityId, Logger, OrganizationService, McsSettings);
         }
        #endregion
        #region Bulk Edit
        /// <summary>
        /// Try to Add, Remove or Replace resources based on Bulk Edit fields.
        /// </summary>
        /// <param name="TSA"></param>
        //internal void BulkEditResource(Entity TSA)
        //{
        //    try
        //    {
        //        Logger.WriteDebugMessage("checking for user input fields");
        //        mcs_services thisTSA = TSA.ToEntity<mcs_services>();

        //        //Validate that User belongs to either Provider or Patient Facility
        //        var validUser = true;
        //        //validUser = validateUser(thisTSA, OrganizationService);
                
        //        if (validUser == true)
        //        {
        //            //Remove Resource
        //            if (thisTSA.cvt_BulkRemoveResource != null)
        //            {
        //                Boolean success = deleteRecord(thisTSA.cvt_BulkRemoveResource, TSA.Id);
        //                if (success == false)
        //                    CvtHelper.CreateNote(new EntityReference { Id = TSA.Id, LogicalName = thisTSA.LogicalName }, "Remove Resource Failed: " + thisTSA.cvt_BulkRemoveResource.Name, "Bulk Edit Action", OrganizationService);
        //            }
        //            //Add Resource
        //            if (thisTSA.cvt_BulkAddResource != null)
        //            {
        //                Boolean success = addRecord(thisTSA.cvt_BulkAddResource.Id, TSA.Id, "Resource");
        //                if (success == false)
        //                    CvtHelper.CreateNote(new EntityReference { Id = TSA.Id, LogicalName = thisTSA.LogicalName }, "Add Resource Failed: " + thisTSA.cvt_BulkAddResource.Name, "Bulk Edit Action", OrganizationService);
        //            }
        //            //Remove User
        //            if (thisTSA.cvt_BulkRemoveUser != null)
        //            {
        //                Boolean success = deleteRecord(thisTSA.cvt_BulkRemoveUser, TSA.Id);
        //                if (success == false)
        //                    CvtHelper.CreateNote(new EntityReference { Id = TSA.Id, LogicalName = thisTSA.LogicalName }, "Remove User Failed: " + thisTSA.cvt_BulkRemoveUser.Name, "Bulk Edit Action", OrganizationService);
        //            }
        //            //Add User
        //            if (thisTSA.cvt_BulkAddUser != null)
        //            {
        //                Boolean success = addRecord(thisTSA.cvt_BulkAddUser.Id, TSA.Id, "User");
        //                if (success == false)
        //                    CvtHelper.CreateNote(new EntityReference { Id = TSA.Id, LogicalName = thisTSA.LogicalName }, "Add User Failed: " + thisTSA.cvt_BulkAddUser.Name, "Bulk Edit Action", OrganizationService);
        //            }
        //            //Remove Resource Group
        //            if (thisTSA.cvt_BulkRemoveResourceGroup != null)
        //            {
        //                Boolean success = deleteRecord(thisTSA.cvt_BulkRemoveResourceGroup, TSA.Id);
        //                if (success == false)
        //                    CvtHelper.CreateNote(new EntityReference { Id = TSA.Id, LogicalName = thisTSA.LogicalName }, "Remove Resource Group Failed: " + thisTSA.cvt_BulkRemoveResourceGroup.Name, "Bulk Edit Action", OrganizationService);
        //            }
        //            //Add Resource Group
        //            if (thisTSA.cvt_BulkAddResourceGroup != null)
        //            {
        //                Boolean success = addRecord(thisTSA.cvt_BulkAddResourceGroup.Id, TSA.Id, "Resource Group");
        //                if (success == false)
        //                    CvtHelper.CreateNote(new EntityReference { Id = TSA.Id, LogicalName = thisTSA.LogicalName }, "Add Resource Group Failed: " + thisTSA.cvt_BulkAddResourceGroup.Name, "Bulk Edit Action", OrganizationService);
        //            }
        //        }
        //        else
        //        {
        //            CvtHelper.CreateNote(new EntityReference { Id = TSA.Id, LogicalName = thisTSA.LogicalName }, "User does not belong to either Provider/Patient facility and is not authorized to make this change. " + thisTSA.cvt_BulkAddResourceGroup.Name, "Bulk Edit Action", OrganizationService);
        //        }
        //    }
        //    catch (FaultException<OrganizationServiceFault> ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //    }
        //}

        /// <summary>
        /// Look for resource on both sides and try to delete
        /// </summary>
        /// <param name="removeVar"></param>
        /// <param name="TSAId"></param>
        /// <returns>Boolean on success</returns>
        //internal Boolean deleteRecord(EntityReference removeVar, Guid TSAId)
        //{
        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        try
        //        {
        //            var count = 0;
        //            var thisTSA = srv.mcs_servicesSet.FirstOrDefault(i => i.Id == TSAId);
        //            //Search for any providersiteresource where TSS resource = removeVar
        //            var getProvResources = from provGroups in srv.cvt_providerresourcegroupSet
        //                                   where provGroups.cvt_RelatedTSAid.Id == TSAId
        //                                   where provGroups.statecode == 0
        //                                   where provGroups.cvt_RelatedResourceId.Id == removeVar.Id ||
        //                                        provGroups.cvt_RelatedUserId.Id == removeVar.Id ||
        //                                        provGroups.cvt_RelatedResourceGroupid.Id == removeVar.Id

        //                                   select new
        //                                   {
        //                                       provGroups.Id,
        //                                       provGroups.cvt_name
        //                                   };

        //            //if match, delete providersiteresource record 
        //            foreach (var provGroups in getProvResources)
        //            {
        //                OrganizationService.Delete(cvt_providerresourcegroup.EntityLogicalName, provGroups.Id);
        //                CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Removed from Provider Site: " + provGroups.cvt_name, "Bulk Edit Action",
        //                    OrganizationService);
        //                count += 1;
        //            }
                     
        //            //Search for any patientsiteresource where TSS resource = removeVar
        //            var getPatResources = from patGroups in srv.cvt_patientresourcegroupSet
        //                                  where patGroups.cvt_RelatedTSAid.Id == TSAId
        //                                  where patGroups.statecode == 0
        //                                  where patGroups.cvt_RelatedResourceId.Id == removeVar.Id ||
        //                                      patGroups.cvt_RelatedUserId.Id == removeVar.Id ||
        //                                      patGroups.cvt_RelatedResourceGroupid.Id == removeVar.Id

        //                                  select new
        //                                  {
        //                                      patGroups.Id,
        //                                      patGroups.cvt_name
        //                                  };

        //            //if match, delete patientsiteresource record 
        //            foreach (var patGroups in getPatResources)
        //            {
        //                OrganizationService.Delete(cvt_patientresourcegroup.EntityLogicalName, patGroups.Id);
        //                CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Removed from Patient Site: " + patGroups.cvt_name, "Bulk Edit Action", OrganizationService);
        //                count += 1;
        //            }

        //            //Check if didn't find any to delete
        //            if (count == 0)
        //            {
        //                //var removeVarRecord = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == removeVar);
        //                CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Skipped Remove. Failed to find resource on TSA: " + removeVar.Name, "Bulk Edit Action", OrganizationService);
        //            }
        //            return true;
        //        }
        //        catch (FaultException<OrganizationServiceFault> ex)
        //        {
        //            Logger.WriteToFile(ex.Message);
        //            return false;
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.WriteToFile(ex.Message);
        //            return false;
        //        }
        //    }
        //}

        /// <summary>
        /// Try to match the resource's site with Pro or Pat, then add Either ProviderResource or PatientResource
        /// </summary>
        /// <param name="addVar"></param>
        /// <param name="TSAId"></param>
        /// <param name="varType"></param>
        /// <returns>Boolean on success</returns>
        //internal Boolean addRecord(Guid addVar, Guid TSAId, String varType)
        //{
        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        try
        //        {
        //            var thisTSA = srv.mcs_servicesSet.FirstOrDefault(i => i.Id == TSAId);
        //            Guid provSiteId = (thisTSA.cvt_relatedprovidersiteid != null) ? thisTSA.cvt_relatedprovidersiteid.Id : Guid.Empty;
        //            Guid patSiteId = (thisTSA.cvt_relatedpatientsiteid != null) ? thisTSA.cvt_relatedpatientsiteid.Id : Guid.Empty;
        //            Guid patFacilityId = (thisTSA.cvt_PatientFacility != null) ? thisTSA.cvt_PatientFacility.Id : Guid.Empty;
        //            Guid addVarLocationId = Guid.Empty;

        //            //Build out Entity, but check which one to cast and create.
        //            var newObject = new Entity();
        //            //var newObject = new cvt_providerresourcegroup();
        //            String matchedSide = "";

        //            switch (varType)
        //            {
        //                case "Resource":
        //                    //Get Site of Resource
        //                    var thisResource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == addVar);
        //                    if (thisResource != null)
        //                    {
        //                        addVarLocationId = thisResource.mcs_RelatedSiteId.Id;
        //                        newObject["cvt_relatedresourceid"] = new EntityReference(mcs_resource.EntityLogicalName, addVar);
        //                        newObject["cvt_type"] = thisResource.mcs_Type; //Dynamic
        //                        newObject["cvt_name"] = thisResource.mcs_name;
        //                        newObject["cvt_tsaresourcetype"] = new OptionSetValue(1); //Single Resource
        //                        newObject["cvt_resourcespecguid"] = thisResource.mcs_resourcespecguid;
        //                        newObject["cvt_constraintgroupguid"] = thisResource.mcs_constraintgroupguid;
        //                    }
        //                    break;

        //                case "User":
        //                    //Get Site of User
        //                    var thisUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == addVar);
        //                    if (thisUser != null)
        //                    {
        //                        addVarLocationId = thisUser.cvt_site.Id;
        //                        newObject["cvt_relateduserid"] = new EntityReference(SystemUser.EntityLogicalName, addVar);
        //                        newObject["cvt_name"] = thisUser.FullName;
        //                        newObject["cvt_tsaresourcetype"] = new OptionSetValue(2); //Single Provider
        //                    }
        //                    break;

        //                case "Resource Group":
        //                    //Get Site of Resource Group
        //                    var thisResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == addVar);
        //                    if (thisResourceGroup != null)
        //                    {
        //                        addVarLocationId = thisResourceGroup.mcs_relatedSiteId.Id;
        //                        newObject["cvt_relatedresourcegroupid"] = new EntityReference(mcs_resourcegroup.EntityLogicalName, addVar);
        //                        newObject["cvt_type"] = thisResourceGroup.mcs_Type; //Dynamic
        //                        newObject["cvt_name"] = thisResourceGroup.mcs_name;
        //                        newObject["cvt_tsaresourcetype"] = new OptionSetValue(0); //Resource Group
        //                        newObject["cvt_resourcespecguid"] = thisResourceGroup.mcs_resourcespecguid;
        //                        newObject["cvt_constraintgroupguid"] = thisResourceGroup.mcs_constraintgroupguid;
        //                    }
        //                    break;
        //            }
        //            if (addVarLocationId == provSiteId)
        //                matchedSide = "Provider";
        //            else if (patSiteId != Guid.Empty) //Not Group
        //            {
        //                if (addVarLocationId == patSiteId)
        //                    matchedSide = "Patient";
        //            }
        //            else if (patFacilityId != Guid.Empty) //Group
        //            {
        //                //Search for Sites related to the Patient Facility                    
        //                var getSites = from childSites in srv.mcs_siteSet
        //                               where childSites.mcs_FacilityId.Id == patFacilityId
        //                               where childSites.statecode == 0

        //                               select new
        //                               {
        //                                   childSites.Id,
        //                               };
        //                //Loop through and compare addVarLocationId to each SiteId in resultset
        //                foreach (var result in getSites)
        //                {
        //                    //If match, then set "Patient" (Group)
        //                    if (result.Id == addVarLocationId)
        //                        matchedSide = "Patient";
        //                }
        //            }

        //            //Create appropriate record
        //            newObject["cvt_relatedsiteid"] = new EntityReference(mcs_site.EntityLogicalName, addVarLocationId);
        //            newObject["cvt_relatedtsaid"] = new EntityReference(mcs_services.EntityLogicalName, TSAId);
        //            var count = 0;
        //            switch (matchedSide)
        //            {
        //                case "Provider":
        //                    //Cast
        //                    newObject.LogicalName = cvt_providerresourcegroup.EntityLogicalName;
        //                    var newProvObj = newObject.ToEntity<cvt_providerresourcegroup>();

        //                    //First Check if this object currently exists Provider site
        //                    var getProvResources = from provGroups in srv.cvt_providerresourcegroupSet
        //                                           where provGroups.cvt_RelatedTSAid.Id == TSAId
        //                                           where provGroups.statecode == 0
        //                                           where provGroups.cvt_RelatedResourceId.Id == addVar ||
        //                                                 provGroups.cvt_RelatedUserId.Id == addVar ||
        //                                                 provGroups.cvt_RelatedResourceGroupid.Id == addVar

        //                                           select new
        //                                           {
        //                                               provGroups.Id
        //                                           };

        //                    foreach (var provGroups in getProvResources)
        //                    {
        //                        count += 1;
        //                    }
        //                    //Create Record
        //                    if (count == 0)
        //                    {
        //                        OrganizationService.Create(newObject);
        //                        Logger.WriteDebugMessage("new Provider Site Resource Created");
        //                        CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Added to Provider Site: " + newProvObj.cvt_name, "Bulk Edit Action", OrganizationService);
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        Logger.WriteDebugMessage("Provider Site Resource could not be added.  It already exists on the TSA.");
        //                        CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Resource Already Associated, Failed to Add to Provider Site: " + newProvObj.cvt_name, "Bulk Edit Action", OrganizationService);
        //                    }
        //                    break;
        //                case "Patient":
        //                    //Cast
        //                    if (varType == "User")
        //                        newObject["cvt_tsaresourcetype"] = new OptionSetValue(3); //Single Telepresenter
        //                    newObject.LogicalName = cvt_patientresourcegroup.EntityLogicalName;
        //                    var newPatObj = newObject.ToEntity<cvt_patientresourcegroup>();

        //                    //First Check if this object currently exists Patient site
        //                    var getPatResources = from patGroups in srv.cvt_patientresourcegroupSet
        //                                          where patGroups.cvt_RelatedTSAid.Id == TSAId
        //                                          where patGroups.statecode == 0
        //                                          where patGroups.cvt_RelatedResourceId.Id == addVar ||
        //                                              patGroups.cvt_RelatedUserId.Id == addVar ||
        //                                              patGroups.cvt_RelatedResourceGroupid.Id == addVar

        //                                          select new
        //                                          {
        //                                              patGroups.Id
        //                                          };

        //                    foreach (var patGroups in getPatResources)
        //                    {
        //                        count += 1;
        //                    }
        //                    //Create Record
        //                    if (count == 0)
        //                    {
        //                        OrganizationService.Create(newPatObj);
        //                        Logger.WriteDebugMessage("new Patient Site Resource Created");
        //                        CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Added to Patient Site: " + newPatObj.cvt_name, "Bulk Edit Action", OrganizationService);
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        Logger.WriteDebugMessage("Patient Site Resource could not be added.  It already exists on the TSA.");
        //                        CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Resource Already Associated, Failed to Add to Patient Site: " + newPatObj.cvt_name, "Bulk Edit Action", OrganizationService);
        //                    }
        //                    break;
        //                default:
        //                    Logger.WriteDebugMessage("Resource does not belong to Provider or Patient Site. Resource was not added.");
        //                        CvtHelper.CreateNote(new EntityReference { Id = TSAId, LogicalName = thisTSA.LogicalName }, "Resource does not belong to Provider or Patient Site, Failed to Add: " + newObject["cvt_name"], "Bulk Edit Action", OrganizationService);
        //                    break;
        //            }
        //            return true;
        //        }
        //        catch (FaultException<OrganizationServiceFault> ex)
        //        {
        //            Logger.WriteToFile(ex.Message);
        //            return false;
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.WriteToFile(ex.Message);
        //            return false;
        //        }
        //    }
        //}

        //TODO - to be deleted upon confirmation it is no longer needed
        //internal Boolean validateUser(mcs_services TSA, IOrganizationService OrganizationService)
        //{
        //    var AuthorizedUser = false;
        //    try
        //    {
        //        using (var srv = new Xrm(OrganizationService))
        //        {


        //            //Get Authority of User
        //            var me = PluginExecutionContext.InitiatingUserId;
        //            var myAuthority = 0;
        //            var getMySecurityRoles = from secRoles in srv.SystemUserRolesSet
        //                                     join roleName in srv.RoleSet
        //                                     on secRoles.RoleId equals roleName.RoleId
        //                                     where secRoles.SystemUserId == me
        //                                     select new
        //                                     {
        //                                         roleName.Name
        //                                     };

        //            foreach (var result in getMySecurityRoles)
        //            {
        //                switch (result.Name)
        //                {
        //                    //Check if App Admin or Sys Admin = 3
        //                    case "System Administrator":
        //                    case "TMP Application Administrator":
        //                        myAuthority = 3;
        //                        break;
        //                    //Check if VISN Lead = 2
        //                    case "TSS VISN Lead":
        //                        if (myAuthority < 2)
        //                            myAuthority = 2;
        //                        break;
        //                    //Check if FTC = 1
        //                    case "TSS FTC":
        //                        if (myAuthority < 1)
        //                            myAuthority = 1;
        //                        break;
        //                    //None = 0
        //                }
        //            }
        //            //If me is App Admin or Sys Admin, return true
        //            switch (myAuthority)
        //            {
        //                case 3:
        //                    return true;
        //                case 0:
        //                    return false;
        //            }

        //            //Get VISN or Facility of User
        //            var thisUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == me);
        //            Guid userVISNId = (thisUser.BusinessUnitId != null) ? thisUser.BusinessUnitId.Id : Guid.Empty;
        //            Guid userFacilityId = (thisUser.cvt_facility != null) ? thisUser.cvt_facility.Id : Guid.Empty;
        //            Guid userSiteId = (thisUser.cvt_site != null) ? thisUser.cvt_site.Id : Guid.Empty;
        //            //If User's Facility is null but has a Site, then look for that Site's Facility
        //            if (userFacilityId == Guid.Empty && userSiteId != Guid.Empty)
        //            {
        //                var userSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == userSiteId);
        //                userFacilityId = (userSite.mcs_FacilityId != null) ? userSite.mcs_FacilityId.Id : Guid.Empty;
        //            }

        //            //Get Site or Facility of TSA
        //            var thisTSA = srv.mcs_servicesSet.FirstOrDefault(i => i.Id == TSA.Id);
        //            Guid provSiteId = (thisTSA.cvt_relatedprovidersiteid != null) ? thisTSA.cvt_relatedprovidersiteid.Id : Guid.Empty;
        //            Guid patSiteId = (thisTSA.cvt_relatedpatientsiteid != null) ? thisTSA.cvt_relatedpatientsiteid.Id : Guid.Empty;
        //            Guid patFacilityId = (thisTSA.cvt_PatientFacility != null) ? thisTSA.cvt_PatientFacility.Id : Guid.Empty;

        //            //Get final two Facilities/VISNs for eventual comparison
        //            //Provider Facility/VISN
        //            var provSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == provSiteId);
        //            Guid tsaProvFacility = (provSite.mcs_FacilityId != null) ? provSite.mcs_FacilityId.Id : Guid.Empty;
        //            Guid tsaProvVISN = (provSite.mcs_BusinessUnitId != null) ? provSite.mcs_BusinessUnitId.Id : Guid.Empty;

        //            //Patient Facility
        //            Guid tsaPatFacility = patFacilityId;
        //            Guid tsaPatVISN = Guid.Empty;
        //            if (patFacilityId == Guid.Empty) //Single so get the Facility and VISN off of the Site
        //            {
        //                var patSite = srv.mcs_siteSet.FirstOrDefault(i => i.Id == patSiteId);
        //                tsaPatFacility = (patSite.mcs_FacilityId != null) ? patSite.mcs_FacilityId.Id : Guid.Empty;
        //                tsaPatVISN = (patSite.mcs_BusinessUnitId != null) ? patSite.mcs_BusinessUnitId.Id : Guid.Empty;
        //            }
        //            else //Group so get the VISN off of the Pat Facility 
        //            {
        //                var patFacility = srv.mcs_facilitySet.FirstOrDefault(i => i.Id == patFacilityId);
        //                tsaPatVISN = (patFacility.mcs_BusinessUnitId != null) ? patFacility.mcs_BusinessUnitId.Id : Guid.Empty;
        //            }

        //            //Determine Authority
        //            switch (myAuthority)
        //            {
        //                case 2: //VISN Lead
        //                    if ((userVISNId == tsaProvVISN) || (userVISNId == tsaPatVISN))
        //                        AuthorizedUser = true;
        //                    break;
        //                case 1: //FTC
        //                    if ((userFacilityId == tsaProvFacility) || (userFacilityId == tsaPatFacility))
        //                        AuthorizedUser = true;
        //                    break;
        //            }
        //        }
        //    }
        //    catch (FaultException<OrganizationServiceFault> ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //        return false;
        //    }
        //    return AuthorizedUser;
        //}

        #endregion
 
        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_tsaplugin"; }
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