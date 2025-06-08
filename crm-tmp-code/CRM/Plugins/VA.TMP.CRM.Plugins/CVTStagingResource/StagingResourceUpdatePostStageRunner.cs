using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM.CVTStagingResource
{

    /// <summary>
    /// This plugin responsible for processing invetory record's actions, such as approval, rejecton... etc 
    /// </summary>
    public class StagingResourceUpdatePostStageRunner : PluginRunner
    {
        private List<Entity> compToDeactivate = new List<Entity>();

        #region Constructor

        public StagingResourceUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion
        public override string McsSettingsDebugField
        {
            get { return "mcs_stagingresourceplugin"; }
        }

        public override void Execute()
        {

            if (PrimaryEntity.LogicalName != cvt_stagingresource.EntityLogicalName)
                return;
            try
            {
                cvt_stagingresource stgRes = PrimaryEntity.ToEntity<cvt_stagingresource>();
                cvt_stagingresource stgResPre = PluginExecutionContext.PreEntityImages["PreImage"]?.ToEntity<cvt_stagingresource>();
                cvt_stagingresource stgResPost = PluginExecutionContext.PostEntityImages["PostImage"]?.ToEntity<cvt_stagingresource>();

                if (PluginExecutionContext.Depth > 1 || PrimaryEntity.Attributes.Contains("ownerid"))
                {
                    Logger.WriteDebugMessage($"PluginExecutionContext.Depth: {PluginExecutionContext.Depth} and OwnerId Presence is {PrimaryEntity.Attributes.Contains("ownerid")}");
                    return;
                }

                if (PrimaryEntity.Attributes.Contains("mcs_facility") ||
                    PrimaryEntity.Attributes.Contains("mcs_relatedsiteid"))
                {
                    Logger.WriteDebugMessage(
                        $"Facility/Site updated on the Staging Resource Record: {stgRes.Id}. Initiating Owner Assignment");

                    CvtHelper.AssignOwner(stgResPost, Logger, OrganizationService);
                    Logger.WriteDebugMessage("Owner Assignment function Call Complete");
                }

                //Check the approval first
                if (stgRes.cvt_approvalstatus != null)
                {
                    using (var srv = new Xrm(OrganizationService))
                    {
                        cvt_stagingresource stgResource = srv.cvt_stagingresourceSet.Where(x => x.Id == stgRes.Id).First();

                        if (stgRes.cvt_approvalstatus.Value == (int)cvt_approvalstatus.Approved)
                        {
                            ApproveInventoryRecord(stgResource, srv);
                        }
                        else
                        {
                            RejectInventoryRecord(stgResource, srv);
                        }
                    }
                }
                //Check if resrouce to match has been set or changed
                else if (stgRes != null && stgResPre != null)
                {
                    if (stgRes.cvt_resourcetomatch == null || !stgRes.cvt_resourcetomatch.Equals(stgResPre.cvt_resourcetomatch))
                    {
                        using (var srv = new Xrm(OrganizationService))
                        {
                            //if changed or deleted do we need to delete existing mismatched records???
                            if (stgResPre != null)
                            {
                                if (stgResPre.cvt_resourcetomatch != null)
                                {
                                    //Delete mismatch records
                                    cvt_stagingresource stgResource = srv.cvt_stagingresourceSet.Where(x => x.Id == stgRes.Id).First();

                                    var mismatches = srv.cvt_fieldmismatchSet.Where(x => x.cvt_stagingresource.Id == stgResource.Id && x.statecode == cvt_fieldmismatchState.Active).ToList<Entity>();

                                    DeleteMismatchResRecords(mismatches);

                                    var comp = srv.cvt_stagingcomponentSet.Where(x => x.cvt_relatedresourceid.Id == stgResource.Id && x.statecode.Value == cvt_stagingcomponentState.Active).ToList<Entity>();

                                    DeleteMismatchCompRecords(comp, srv);

                                    srv.SaveChanges();
                                }
                            }

                            if (stgRes.cvt_resourcetomatch != null)
                            {
                                //Get resource
                                mcs_resource resource = srv.mcs_resourceSet.Where(x => x.Id == stgRes.cvt_resourcetomatch.Id).First();
                                cvt_stagingresource stgResource = (cvt_stagingresource)OrganizationService.Retrieve(cvt_stagingresource.EntityLogicalName, stgRes.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));

                                List<Entity> mismatch = CvtHelper.FindMismatch(resource, stgResource);

                                AddMismatchRecordsToContext(mismatch, srv);

                                var comp = srv.cvt_stagingcomponentSet.Where(x => x.cvt_relatedresourceid.Id == stgResource.Id && x.statecode.Value == cvt_stagingcomponentState.Active).ToList();

                                CreateComponentsMismatchRecords(stgResource, resource, comp, srv);

                                srv.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        #region Reject Methods

        /// <summary>
        /// Deactivate res/comp records upon rejection
        /// </summary>
        /// <param name="stgRes"></param>
        /// <param name="srv"></param>
        private void RejectInventoryRecord(cvt_stagingresource stgRes, Xrm srv)
        {
            var stgComps = srv.cvt_stagingcomponentSet.Where(x => x.cvt_relatedresourceid.Id == stgRes.Id && x.statecode == cvt_stagingcomponentState.Active).ToList();

            var mismatches = srv.cvt_fieldmismatchSet.Where(x => x.cvt_stagingresource.Id == stgRes.Id && x.statecode == cvt_fieldmismatchState.Active).ToList<Entity>();

            UpdateStagingComponentsApproval(srv, cvt_approvalstatus.Rejected, stgComps);
            srv.SaveChanges();

            Utilities.DeactivateRecords(stgComps.ToList<Entity>(), (int)cvt_stagingcomponentState.Inactive, (int)cvt_stagingcomponent_statuscode.Inactive);
            Utilities.DeactivateRecords(mismatches, (int)cvt_fieldmismatchState.Inactive, (int)cvt_fieldmismatch_statuscode.Inactive);
        }

        private void UpdateStagingComponentsApproval(Xrm srv, cvt_approvalstatus approval, List<cvt_stagingcomponent> stgComps)
        {
            var activeComps = stgComps.Where(x => x.statecode == cvt_stagingcomponentState.Active);
            foreach (cvt_stagingcomponent comp in activeComps)
            {
                comp.cvt_approvalstatus = new OptionSetValue((int)approval);
                srv.UpdateObject(comp);
            }
        }
        #endregion

        #region Approval Methods
        /// <summary>
        /// Aprove Inventory with all related components
        /// </summary>
        /// <param name="stgRes"></param>
        /// <param name="srv"></param>
        private void ApproveInventoryRecord(cvt_stagingresource stgRes, Xrm srv)
        {
            bool isNR = IsThereNewResourceMismatches(stgRes, srv);
            bool isNC = IsThereNewComponentsMismatches(stgRes, srv);
            if (isNR || isNC)
            {
                //TODO: set approval flag
                stgRes.cvt_approvalstatus = null;
                stgRes.cvt_approvalresult = true;
                srv.UpdateObject(stgRes);
                srv.SaveChanges();
            }
            else
            {

                //Validation should occur before approving the inventory

                mcs_resource resource;

                if (stgRes.cvt_resourcetomatch == null)
                {
                    resource = new mcs_resource();
                    resource.cvt_importaction = new OptionSetValue((int)cvt_importaction.New);
                    srv.AddObject(resource);
                }
                else
                {
                    resource = srv.mcs_resourceSet.Where(x => x.Id == stgRes.cvt_resourcetomatch.Id && x.statecode == mcs_resourceState.Active).First();
                    resource.cvt_importaction = new OptionSetValue((int)cvt_importaction.Update);
                    srv.UpdateObject(resource);
                }

                UpdateResource(resource, stgRes);
                srv.SaveChanges();

                var stgComps = ApproveComponents(resource, stgRes, srv);
                UpdateStagingComponentsApproval(srv, cvt_approvalstatus.Approved, stgComps);

                srv.SaveChanges();



                //deactivate records
                Utilities.DeactivateRecord(stgRes, (int)cvt_stagingresourceState.Inactive, (int)cvt_stagingresource_statuscode.Inactive);
                Utilities.DeactivateRecords(stgComps.ToList<Entity>(), (int)cvt_stagingcomponentState.Inactive, (int)cvt_stagingcomponent_statuscode.Inactive);
                Utilities.DeactivateRecords(compToDeactivate, (int)cvt_componentState.Inactive, (int)cvt_component_statuscode.Inactive);
            }
        }


        private void UpdateResource(mcs_resource resource, cvt_stagingresource stgRes)
        {
            //Create new resource if null
            if (resource == null)
                resource = new mcs_resource();

            if (stgRes.mcs_RelatedSiteId != null)
            {
                resource.mcs_RelatedSiteId = stgRes.mcs_RelatedSiteId;
            }

            //we need to ask if we need to override
            if (!string.IsNullOrEmpty(stgRes.cvt_MasterSerialNumber) && !stgRes.cvt_MasterSerialNumber.StartsWith("DUMMY"))
            {
                resource.cvt_MasterSerialNumber = stgRes.cvt_MasterSerialNumber;
            }

            if (stgRes.mcs_Type != null)
            {
                resource.mcs_Type = stgRes.mcs_Type;
            }

            if (stgRes.cvt_uniqueid != null)
            {
                resource.cvt_uniqueid = stgRes.cvt_uniqueid;
            }

            //if (stgRes.mcs_BusinessUnitId != null)
            //{
            //    resource.mcs_BusinessUnitId = stgRes.mcs_BusinessUnitId;
            //}

            //if (stgRes.mcs_BusinessUnitId != null)
            //{
            //    resource.mcs_BusinessUnitId = stgRes.mcs_BusinessUnitId;
            //}

            if (stgRes.cvt_room != null)
            {
                resource.cvt_room = stgRes.cvt_room;
            }

            //Fixing cvt_systemtype bug, sometimes the value comes as 100000004, which does not exist
            if (stgRes.cvt_systemtype != null && (stgRes.cvt_systemtype.Value >= 917290000 || stgRes.cvt_systemtype.Value <= 917290006))
            {
                resource.cvt_systemtype = new OptionSetValue(stgRes.cvt_systemtype.Value);
            }

            if (stgRes.cvt_supportedmodality != null)
            {
                resource.cvt_supportedmodality = new OptionSetValue(stgRes.cvt_supportedmodality.Value);
            }

            //Fixing primarytype bug, sometimes the value comes as 100000004, which does not exist
            if (resource.cvt_primaryusertype != null && (resource.cvt_primaryusertype.Value < 803750000 || resource.cvt_primaryusertype.Value > 917290001))
            {
                resource.cvt_primaryusertype = null;
            }

            if (stgRes.cvt_carttypeid != null)
            {
                resource.cvt_CartTypeId = stgRes.cvt_carttypeid;
            }

            //if (stgRes.mcs_Facility != null)
            //{
            //    resource.mcs_Facility = stgRes.mcs_Facility;
            //}

            if (stgRes.cvt_locationuse != null)
            {
                resource.cvt_locationuse = new OptionSetValue(stgRes.cvt_locationuse.Value);
            }

            if (stgRes.cvt_relateduser != null)
            {
                resource.cvt_relateduser = stgRes.cvt_relateduser;
            }

        }


        private List<cvt_stagingcomponent> ApproveComponents(mcs_resource resource, cvt_stagingresource stgRes, Xrm srv)
        {
            //Relationship stgCompRelationship = new Relationship("cvt_cvt_stagingresource_cvt_stagingcomponent_relatedresourceid");
            //srv.LoadProperty(stgRes, stgCompRelationship);

            //var stgComps = stgRes.RelatedEntities[stgCompRelationship].Entities
            //      .Select(e => e.ToEntity<cvt_stagingcomponent>())
            //      .Where(x=>x.statecode == cvt_stagingcomponentState.Active).ToList();
            var stgComps = srv.cvt_stagingcomponentSet.Where(x => x.cvt_relatedresourceid.Id == stgRes.Id && x.statecode == cvt_stagingcomponentState.Active).ToList();

            if (stgRes.cvt_resourcetomatch == null)
                ApproveNewComponents(resource, stgRes, stgComps, srv);
            else
                ApproveUpdateComponents(resource, stgRes, stgComps, srv);

            return stgComps;

        }

        private void ApproveUpdateComponents(mcs_resource resource, cvt_stagingresource stgRes, List<cvt_stagingcomponent> stgComps, Xrm srv)
        {

            var comps = srv.cvt_componentSet.Where(x => x.cvt_relatedresourceid.Equals(resource.ToEntityReference()) && x.statecode.Value == cvt_componentState.Active).ToList();

            //var comps = resource.RelatedEntities[compRelationship].Entities;
            //.Select(e => e.ToEntity<cvt_component>());

            foreach (cvt_stagingcomponent stgComp in stgComps)
            {

                //add compoenents that match the staging and exist in other resources to the deactivate list
                AddOtherCompToDeactiveList(stgRes, stgComp, srv);
                //if there is a compoenet exists in other resrouce and selected then ignore the staging one


                cvt_component compToFind = null;

                if (stgComp.cvt_connectedcomponentid != null)
                {
                    compToFind = srv.cvt_componentSet.FirstOrDefault(x => x.Id == stgComp.cvt_connectedcomponentid.Id && x.statecode == cvt_componentState.Active);
                    if (compToFind != null)
                    {
                        UpdateComponent(compToFind, stgComp);
                        compToFind.cvt_importaction = new OptionSetValue((int)cvt_importaction.Update);
                        srv.UpdateObject(compToFind);
                    }
                }

                if (compToFind == null)
                {
                    var comp = new cvt_component();
                    UpdateComponent(comp, stgComp);
                    comp.cvt_relatedresourceid = resource.ToEntityReference();
                    comp.cvt_importaction = new OptionSetValue((int)cvt_importaction.New);
                    comp.cvt_relatedresourceid = resource.ToEntityReference();
                    srv.AddObject(comp);
                }

                //if (stgComp.cvt_serialnumber != null)
                //{
                //    compToFind = comps.FirstOrDefault(x => x.GetAttributeValue<string>("cvt_serialnumber") == stgComp.cvt_serialnumber);
                //    //var compRes = comps
                //    if (compToFind != null)
                //    {
                //        //srv.Attach(compToFind);
                //        UpdateComponent(compToFind, stgComp);
                //        srv.UpdateObject(compToFind);
                //    }

                //}

            }
        }

        private void ApproveNewComponents(mcs_resource resource, cvt_stagingresource stgRes, List<cvt_stagingcomponent> stgComps, Xrm srv)
        {
            Relationship compRelationship = new Relationship("cvt_mcs_resource_cvt_component_relatedresourceid");
            EntityCollection relatedCompoenents = new EntityCollection();


            foreach (cvt_stagingcomponent stgComp in stgComps)
            {
                //add compoenents that match the staging and exist in other resources to the deactivate list
                AddOtherCompToDeactiveList(stgRes, stgComp, srv);
                //if there is a compoenet exists in other resrouce and selected then ignore the staging one
                cvt_component comp = null;
                if (stgComp.cvt_connectedcomponentid != null)
                {
                    comp = srv.cvt_componentSet.FirstOrDefault(x => x.Id == stgComp.cvt_connectedcomponentid.Id && x.statecode == cvt_componentState.Active);
                    if (comp != null)
                    {
                        UpdateComponent(comp, stgComp);
                        comp.cvt_importaction = new OptionSetValue((int)cvt_importaction.Update);
                        srv.UpdateObject(comp);
                    }
                }
                else
                {
                    comp = new cvt_component();
                    UpdateComponent(comp, stgComp);
                    comp.cvt_importaction = new OptionSetValue((int)cvt_importaction.New);
                    comp.cvt_relatedresourceid = resource.ToEntityReference();
                    srv.AddObject(comp);
                    //relatedCompoenents.Entities.Add(comp);
                }
            }

            //resource.RelatedEntities.Add(compRelationship, relatedCompoenents);

        }


        /// <summary>
        /// Add other components, components sharing same serial number and needs to be deactivated
        /// </summary>
        /// <param name="stgRes"></param>
        /// <param name="stgComp"></param>
        /// <param name="srv"></param>
        private void AddOtherCompToDeactiveList(cvt_stagingresource stgRes, cvt_stagingcomponent stgComp, Xrm srv)
        {
            if (!string.IsNullOrEmpty(stgComp.cvt_serialnumber))
            {
                var comps = srv.cvt_componentSet.Where(x => x.cvt_serialnumber == stgComp.cvt_serialnumber && x.statecode.Value == cvt_componentState.Active).ToList<cvt_component>();

                if (stgComp.cvt_connectedcomponentid != null)
                {
                    comps = comps.Where(x => x.Id != stgComp.cvt_connectedcomponentid.Id).ToList();
                }

                if (stgRes.cvt_resourcetomatch != null)
                {
                    comps = comps.Where(x => x.cvt_relatedresourceid != null && x.cvt_relatedresourceid.Id != stgRes.cvt_resourcetomatch.Id).ToList();
                }

                compToDeactivate.AddRange(comps);
            }
        }


        private void UpdateComponent(cvt_component comp, cvt_stagingcomponent stgComp)
        {
            if (comp == null)
                comp = new cvt_component();

            if (stgComp.cvt_ComponentType != null)
            {
                comp.cvt_ComponentType = stgComp.cvt_ComponentType;
                comp.cvt_name = stgComp.cvt_ComponentType.Name;
            }

            if (stgComp.cvt_status != null)
            {
                cvt_componentcvt_status status = (cvt_componentcvt_status)Enum.Parse(typeof(cvt_componentcvt_status), ((cvt_stagingcomponentcvt_status)stgComp.cvt_status.Value).ToString());
                comp.cvt_status = new OptionSetValue((int)status);
            }

            if (stgComp.cvt_ManufacturerID != null)
            {
                comp.cvt_ManufacturerID = stgComp.cvt_ManufacturerID;
            }

            if (stgComp.cvt_ModelNumber != null)
            {
                comp.cvt_ModelNumber = stgComp.cvt_ModelNumber;
            }

            if (stgComp.cvt_serialnumber != null && !stgComp.cvt_serialnumber.StartsWith("DUMMY"))
            {
                comp.cvt_serialnumber = stgComp.cvt_serialnumber;
            }

            if (stgComp.cvt_e164alias != null)
            {
                comp.cvt_e164alias = stgComp.cvt_e164alias;
            }

            if (stgComp.cvt_PartNumber != null)
            {
                comp.cvt_PartNumber = stgComp.cvt_PartNumber;
            }

            if (stgComp.cvt_ipaddress != null)
            {
                comp.cvt_ipaddress = stgComp.cvt_ipaddress;
            }

            if (stgComp.cvt_cevnalias != null)
            {
                comp.cvt_cevnalias = stgComp.cvt_cevnalias;
            }

            if (stgComp.cvt_TMSSystemName != null)
            {
                comp.cvt_TMSSystemName = stgComp.cvt_TMSSystemName;
            }

            if (stgComp.cvt_tmsid != null)
            {
                comp.cvt_tmsid = stgComp.cvt_tmsid;
            }

            if (stgComp.cvt_eenumber != null)
            {
                comp.cvt_eenumber = stgComp.cvt_eenumber;
            }

        }
        #endregion
        #region Mismatch methods
        private void AddMismatchRecordsToContext(List<Entity> mismatches, Xrm srv)
        {
            foreach (cvt_fieldmismatch mismatch in mismatches)
            {
                srv.AddObject(mismatch);
            }
        }
        private void DeleteMismatchResRecords(List<Entity> mismatches)
        {
            foreach (cvt_fieldmismatch mismatch in mismatches)
            {
                OrganizationService.Delete(cvt_fieldmismatch.EntityLogicalName, mismatch.Id);
            }
        }

        private void DeleteMismatchCompRecords(List<Entity> stagingComponent, Xrm srv)
        {
            Relationship mismatchRelationship = new Relationship("cvt_stagingcomponent_fieldmismatch");
            foreach (cvt_stagingcomponent comp in stagingComponent)
            {
                comp.cvt_connectedcomponentid = null;
                comp.cvt_action = new OptionSetValue((int)cvt_stagingcomponentcvt_action.CreateNewComponent);
                srv.UpdateObject(comp);
                var mismatches = srv.cvt_fieldmismatchSet.Where(x => x.cvt_stagingcomponentId.Id == comp.Id && x.statecode == cvt_fieldmismatchState.Active).ToList<Entity>();

                foreach (cvt_fieldmismatch mismatch in mismatches)
                {
                    OrganizationService.Delete(cvt_fieldmismatch.EntityLogicalName, mismatch.Id);
                }
            }
        }


        private bool IsThereNewResourceMismatches(cvt_stagingresource stgResource, Xrm srv)
        {
            bool exist = false;

            List<Entity> mismatchToCompare = new List<Entity>();
            //If connected comp is selected, then get the component from there
            if (stgResource.cvt_resourcetomatch != null)
            {
                List<cvt_fieldmismatch> resstgMismatches = srv.cvt_fieldmismatchSet.Where(x => x.cvt_stagingresource.Id == stgResource.Id && x.cvt_componentid == null).ToList();

                mcs_resource resource = srv.mcs_resourceSet.Where(x => x.Id == stgResource.cvt_resourcetomatch.Id && x.statecode == mcs_resourceState.Active).First();

                mismatchToCompare = CvtHelper.FindMismatch(resource, stgResource);

                foreach (Entity mismatch in mismatchToCompare)
                {
                    cvt_fieldmismatch newMismatch = mismatch.ToEntity<cvt_fieldmismatch>();
                    cvt_fieldmismatch mexist = resstgMismatches.Where(x => x.cvt_fieldschemaname == newMismatch.cvt_fieldschemaname && x.cvt_tmpinternalname == newMismatch.cvt_tmpinternalname).FirstOrDefault();

                    if (mexist == null)
                    {
                        exist = true;
                        srv.AddObject(newMismatch);
                    }
                }
            }


            return exist;
        }




        private bool IsThereNewComponentsMismatches(cvt_stagingresource stgResource, Xrm srv)
        {

            var compList = srv.cvt_stagingcomponentSet.Where(x => x.cvt_relatedresourceid.Id == stgResource.Id && x.statecode.Value == cvt_stagingcomponentState.Active).ToList();
            // mcs_resource tmpResource = srv.mcs_resourceSet.Where(x => x.Id == stgResource.cvt_resourcetomatch.Id && x.statecode == mcs_resourceState.Active).First();

            bool exist = false;
            List<Entity> fieldMismatches = new List<Entity>();
            foreach (cvt_stagingcomponent comp in compList)
            {

                List<cvt_fieldmismatch> compMismatches = srv.cvt_fieldmismatchSet.Where(x => x.cvt_stagingcomponentId.Id == comp.Id).ToList();
                List<Entity> mismatchToCompare = new List<Entity>();
                //If connected comp is selected, then get the component from there
                if (comp.cvt_connectedcomponentid != null)
                {
                    cvt_component connComp = srv.cvt_componentSet.First(x => x.Id == comp.cvt_connectedcomponentid.Id);
                    mismatchToCompare = CvtHelper.FindMismatch(connComp, comp, stgResource);


                    foreach (Entity mismatch in mismatchToCompare)
                    {
                        cvt_fieldmismatch newMismatch = mismatch.ToEntity<cvt_fieldmismatch>();
                        cvt_fieldmismatch mexist = compMismatches.Where(x => x.cvt_fieldschemaname == newMismatch.cvt_fieldschemaname && x.cvt_tmpinternalname == newMismatch.cvt_tmpinternalname).FirstOrDefault();

                        if (mexist == null)
                        {
                            exist = true;
                            srv.AddObject(newMismatch);
                        }
                    }
                }
                //Else search for it
                //else
                //{
                //    var compToFind = srv.cvt_componentSet.FirstOrDefault(
                //        x => x.cvt_serialnumber == comp.cvt_serialnumber &&
                //        x.cvt_relatedresourceid.Equals(tmpResource.ToEntityReference()) &&
                //        x.statecode.Value == cvt_componentState.Active);

                //    if (compToFind != null)
                //    {
                //        //Get mismatched records and set the relationship
                //        mismatchToCompare = CvtHelper.FindMismatch(compToFind, comp, stgResource);

                //    }

                //}


            }

            return exist;
        }

        private void CreateComponentsMismatchRecords(cvt_stagingresource stgResource, mcs_resource tmpResource, List<cvt_stagingcomponent> compList, Xrm srv)
        {
            foreach (cvt_stagingcomponent comp in compList)
            {
                var compToFind = srv.cvt_componentSet.FirstOrDefault(
                    x => x.cvt_serialnumber == comp.cvt_serialnumber &&
                    x.cvt_relatedresourceid.Equals(tmpResource.ToEntityReference()) &&
                    x.statecode.Value == cvt_componentState.Active);

                if (compToFind != null)
                {
                    //Get mismatched records and set the relationship
                    comp.cvt_connectedcomponentid = compToFind.ToEntityReference();
                    comp.cvt_action = new OptionSetValue((int)cvt_stagingcomponentcvt_action.Match);

                    //Get mismatched records and set the relationship
                    List<Entity> mismatch = CvtHelper.FindMismatch(compToFind, comp, stgResource);

                    AddMismatchRecordsToContext(mismatch, srv);
                }
                else
                {
                    comp.cvt_action = new OptionSetValue((int)cvt_stagingcomponentcvt_action.CreateNewComponent);
                    comp.cvt_connectedcomponentid = null;
                }
            }
        }
        #endregion
    }
}
