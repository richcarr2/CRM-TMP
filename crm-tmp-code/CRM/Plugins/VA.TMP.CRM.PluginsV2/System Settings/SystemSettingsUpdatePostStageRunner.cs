using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using MCSShared;
using System.Data;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace VA.TMP.CRM
{
    public class SystemSettingsUpdatePostStageRunner : PluginRunner
    {
        public SystemSettingsUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        DataTable resoucesDT;
        int numberMatched;
        //bool performSmartMatch = false;
        public override void Execute()
        {
            // TO DO check the name = IEN Import
            if (PrimaryEntity.Attributes.Contains("mcs_name"))
            {
                if (PrimaryEntity.Attributes["mcs_name"].ToString().Contains("- Successfully completed at"))
                {
                    //Logger.WriteDebugMessage("Successful, so don't run any update code again.");
                    return;
                }

                if (PrimaryEntity.Attributes["mcs_name"].ToString().Contains("IENMatching"))
                    ImportVistAClinicIENs();
                //else if (PrimaryEntity.Attributes["mcs_name"].ToString().ToLower().StartsWith("imdataimport"))
                //    ImportIMRecords();
            }
        }

        /// <summary>
        /// Import Inventory Managment Records
        /// </summary>
        private void ImportIMRecords()
        {
            Logger.setMethod = "ImportIMRecords";
            Logger.WriteDebugMessage("Initiated ImportIMRecords");
            //Get CSV resrouces and components from CSV note file
            resoucesDT = GetResCompData(PrimaryEntity.Id);
            //Adding new column to store errors
            resoucesDT.Columns.Add("ErrorDesc");

            //Filter distinct resrouces by msn and unique id
            string msnColumnName = resoucesDT.Columns[(int)ResourcesCSVColumns.MasterSerialNumber].ColumnName;
            string uidColumnName = resoucesDT.Columns[(int)ResourcesCSVColumns.UniqueID].ColumnName;
            DataView view = new DataView(resoucesDT);
            DataTable distResourcesDT = view.ToTable(true, msnColumnName);

            //Loop throught results
            using (var srv = new Xrm(OrganizationService))
            {
                srv.MergeOption = Microsoft.Xrm.Sdk.Client.MergeOption.NoTracking;
                var bulkAddUpdate = new ExecuteMultipleRequest
                {
                    Requests = new OrganizationRequestCollection(),
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = true
                    }
                };

                foreach (DataRow rt in distResourcesDT.Rows)
                {
                    //We need to have a unique identifier in the CSV file
                    if (string.IsNullOrEmpty(rt.Field<string>(msnColumnName)))
                        continue;
                    var column = msnColumnName;
                        //(string.IsNullOrEmpty() ? uidColumnName : msnColumnName);

                    var result = from rows in resoucesDT.AsEnumerable()
                                 where (rows.Field<string>(column) == rt.Field<string>(column))
                                 select rows;

                    //Check if it has already imported or not
                    var stgRes = srv.cvt_stagingresourceSet.FirstOrDefault(x => x.mcs_name == rt.Field<string>(column));
                    //if (stgRes == null)
                      //  ProcessIMRecords(result, srv, bulkAddUpdate) //Naveen
                }

                //Uncomment for bulk update
                //bulkAddUpdate = RunExecuteMultiple(bulkAddUpdate,true);

                // ExportResourcesToCSV();//Naveen
                // UpdateStagingResourceOwnership(srv);//Naveen
                // DeactivateSettingsRecord();//Naveen
                Logger.WriteDebugMessage("ImportIMRecords Completed Successfully");
            }
        }

        //private void UpdateStagingResourceOwnership(Xrm srv)
        //{
        //    Logger.setMethod = "UpdateStagingResourceOwnership";
        //    Logger.WriteDebugMessage("UpdateStagingResourceOwnership started");
        //    var stgResources =
        //        srv.cvt_stagingresourceSet.Where(
        //            i => i.mcs_RelatedSiteId != null && i.statecode.Value == (int)cvt_stagingresourceState.Active);
        //    foreach (var stgRes in stgResources)
        //    {
        //        try
        //        {
        //            CvtHelper.AssignOwner(stgRes, Logger, OrganizationService);
        //        }
        //        catch(Exception ex)
        //        {
        //            Logger.WriteToFile($"Error occured while assigning the {cvt_stagingresource.EntityLogicalName} record with id {stgRes.Id} to the Team\n{ex.Message}");
        //        }
        //    }

        //    McsSystemSettingsCreatePostStageRunner.AssignInventoryToNtthd(Logger, OrganizationService);

        //    McsSystemSettingsCreatePostStageRunner.UpdateUniqueIdwhenNotSet(Logger, OrganizationService);

        //    Logger.WriteDebugMessage("UpdateStagingResourceOwnership Completed");
        //}

        //private void DeactivateSettingsRecord()
        //{
        //    try
        //    {
        //        Logger.setMethod = "DeactivateRecord";
        //        SetStateRequest setStateRequest = new SetStateRequest()
        //        {
        //            EntityMoniker = new EntityReference
        //            {
        //                Id = PrimaryEntity.Id,
        //                LogicalName = mcs_setting.EntityLogicalName
        //            },
        //            State = new OptionSetValue((int)mcs_settingState.Inactive),
        //            Status = new OptionSetValue((int)mcs_setting_statuscode.Inactive)
        //        };
        //        OrganizationService.Execute(setStateRequest);
        //    }
        //    catch (Exception ex)
        //    {
        //        var message = $"Deactivation of settings record Failed. Error occurred while deactivating the settings record with Id: {PrimaryEntity.Id}\n{ex.Message}\n{ex.StackTrace}";
        //        Logger.WriteToFile(message);
        //    }
        //}

        /// <summary>
        /// process Invetory Management records
        /// </summary>
        /// <param name="result"></param>
        /// <param name="srv"></param>
        /// <param name="bulkAddUpdate"></param>
//        private void ProcessIMRecords(EnumerableRowCollection<DataRow> result, Xrm srv, ExecuteMultipleRequest bulkAddUpdate)
//        {
//            try
//            {

//#if DEBUG
//                //string uniqueId = result.ElementAt(0)[(int)ResourcesCSVColumns.UniqueID].ToString();
//                //LogIMCSVCustomMessage(result, $"Test Message {result.ElementAt(0)[(int)ResourcesCSVColumns.UniqueID].ToString()}");
//#endif

//                if (result == null || result.Count() == 0)
//                    return;

//                //Get master serial number
//                string ms = result.ElementAt(0)[(int) ResourcesCSVColumns.MasterSerialNumber].ToString();
//                string uid = result.ElementAt(0)[(int) ResourcesCSVColumns.UniqueID].ToString();

//                //check if exists using ms and uid 
//                mcs_resource res = null;

//                if (!string.IsNullOrEmpty(ms) && !ms.StartsWith("DUMMYSLNO") || !string.IsNullOrEmpty(uid))
//                    res = !string.IsNullOrEmpty(ms) && !ms.StartsWith("DUMMYSLNO")
//                        ? srv.mcs_resourceSet.Where(x => x.cvt_MasterSerialNumber == ms).FirstOrDefault()
//                        : srv.mcs_resourceSet.Where(x => x.cvt_uniqueid == uid).FirstOrDefault();

//                if (res == null)
//                {
//                    //Get res by components serial numbers
//                    var compSerials = result.AsEnumerable()
//                        .Where(r => r.Field<string>((int) ComponentsCSVColumns.SerialNumber) != string.Empty &&
//                                    !r.Field<string>((int) ComponentsCSVColumns.SerialNumber).StartsWith("DUMMYSLNO"))
//                        .Select(r => r.Field<string>((int) ComponentsCSVColumns.SerialNumber)).ToArray();

//                    //Use query expression for the IN clause becuase it's not supported in Linq to CRM(early bound) queries

//                    if (compSerials.Count() > 0)
//                    {
//                        var filter = new FilterExpression(LogicalOperator.And)
//                        {
//                            Conditions =
//                            {
//                                new ConditionExpression("cvt_MasterSerialNumber".ToLower(), ConditionOperator.In,
//                                    compSerials)
//                            }
//                        };
//                        var query = new QueryExpression(mcs_resource.EntityLogicalName)
//                        {
//                            ColumnSet = new ColumnSet(true),
//                            Criteria = filter
//                        };
//                        var resources = OrganizationService.RetrieveMultiple(query).Entities.ToList();

//                        //Not Supported Linq to CRM
//                        //var resources = (from mcsRes in srv.mcs_resourceSet
//                        //                 where compSerials.Contains(mcsRes.cvt_MasterSerialNumber)
//                        //                 select mcsRes).ToList();

//                        res = (resources.Count > 0 ? resources[0].ToEntity<mcs_resource>() : null);

//                        if (resources.Count > 1)
//                        {
//                            LogIMCSVCustomMessage(result,
//                                $"{resources.Count} resrouces returned for components  {string.Join(",", compSerials)}");
//                        }
//                    }

//                    if (res == null)
//                    {
//                        //Logger.WriteToFile($"No resrouce found for MS {ms} with uniqe id {uid} and components {string.Join(",", compSerials)}");
//                        //Does not exist, insert the records
//                    }
//                }


//                CreateNewResrouces(result, srv, bulkAddUpdate, res);
//            }
//            catch (FaultException<OrganizationServiceFault> ex)
//            {
//                Logger.WriteToFile(CvtHelper.BuildExceptionMessage(ex,$"{ex} {ex.Detail} {ex.Detail?.TraceText} \nProcessIMRecords - Master serial number {result?.ElementAt(0)[(int)ResourcesCSVColumns.MasterSerialNumber]}"));
//                LogIMCSVCustomMessage(result, $"{ex} {ex.Message} {ex.InnerException} {ex.Detail} {ex.Detail?.TraceText} ProcessIMRecords ");
//            }
//            catch (Exception ex)
//            {
//                Logger.WriteToFile(CvtHelper.BuildExceptionMessage(ex,
//                    $"{ex.ToString()} ProcessIMRecords - Master serial number {result?.ElementAt(0)[(int) ResourcesCSVColumns.MasterSerialNumber].ToString()}"));
//                LogIMCSVCustomMessage(result, $"{ex.ToString()} {ex.Message} {ex.InnerException} ProcessIMRecords ");
//            }
//        }

        /// <summary>
        /// Create new resources with its related components
        /// </summary>
        /// <param name="result"></param>
        /// <param name="srv"></param>
        /// <param name="bulkAddUpdate"></param>
        private void CreateNewResrouces(EnumerableRowCollection<DataRow> result, Xrm srv, ExecuteMultipleRequest bulkAddUpdate, mcs_resource tmpResource)
        {
            Relationship compRelationship = new Relationship("cvt_cvt_stagingresource_cvt_stagingcomponent_relatedresourceid");
            cvt_stagingresource stgResource = PopulateResrouceFromDataRow(result.ElementAt(0), srv);

            var compList = (from row in result.AsEnumerable()
                            select (Entity)PopulateComponentFromDataRow(row, srv)).ToList();

            //Find component that TMS system name has a value
            var compWithSysName = result.FirstOrDefault(x => x[(int)ComponentsCSVColumns.SystemName].ToString() != string.Empty);

            //Set TMS system name similar to the component TMS system name
            stgResource.cvt_tmssystemname = compWithSysName?[(int)ComponentsCSVColumns.SystemName].ToString();

            EntityCollection relatedCompoenents = new EntityCollection(compList);

            stgResource.RelatedEntities.Add(compRelationship, relatedCompoenents);

            //Get mismatch records for the resrouce
            if (tmpResource != null)
            {
                stgResource.cvt_action = new OptionSetValue((int)cvt_stagingresourcecvt_action.Match);
                stgResource.cvt_resourcetomatch = tmpResource.ToEntityReference();

                List<Entity> mismatch = CvtHelper.FindMismatch(tmpResource, stgResource);

                if (mismatch.Count > 0)
                {
                    Relationship mismatchRelationship = new Relationship("cvt_stagingresource_fieldmismatch");
                    EntityCollection mismatchedRecords = new EntityCollection(mismatch);
                    stgResource.RelatedEntities.Add(mismatchRelationship, mismatchedRecords);
                }
                CreateComponentsMismatchRecords(stgResource, tmpResource, compList, srv);
            }
            else
            {
                stgResource.cvt_action = new OptionSetValue((int)cvt_stagingresourcecvt_action.CreateNewResource);
            }


           
            //Prevent Duplicates
            var stgResCheck = srv.cvt_stagingresourceSet.FirstOrDefault(x => x.mcs_name == stgResource.cvt_MasterSerialNumber);
            if (stgResCheck == null)
            {
                srv.AddObject(stgResource);
                srv.SaveChanges();
                UpdateComponentsFieldMismatches(stgResource, srv);
            }

        }

        private void UpdateComponentsFieldMismatches(cvt_stagingresource stgResource, Xrm srv)
        {
            var mismatches = (from mismatch in srv.cvt_fieldmismatchSet
                             join comp in srv.cvt_stagingcomponentSet
                             on mismatch.cvt_stagingcomponentId.Id equals comp.Id
                             where mismatch.cvt_stagingcomponentId != null 
                             && comp.cvt_relatedresourceid.Id == stgResource.Id
                             select mismatch).ToList();

            foreach(var mismatch in mismatches)
            {
                if (!srv.IsAttached(mismatch))
                    srv.Attach(mismatch);

                mismatch.cvt_stagingresource = stgResource.ToEntityReference();
                srv.UpdateObject(mismatch);
            }

            if(mismatches.Count > 0)
            {
                srv.SaveChanges();
            }
        }

        private void CreateComponentsMismatchRecords(cvt_stagingresource stgResource, mcs_resource tmpResource, List<Entity> compList, Xrm srv)
        {
            foreach (cvt_stagingcomponent comp in compList)
            {
                var compToFind = srv.cvt_componentSet.FirstOrDefault(
                    x => x.cvt_serialnumber == comp.cvt_serialnumber &&
                    x.statecode == cvt_componentState.Active &&
                    x.cvt_relatedresourceid.Id == tmpResource.Id);

                if (compToFind != null)
                {
                    //Get mismatched records and set the relationship
                    comp.cvt_connectedcomponentid = compToFind.ToEntityReference();
                    comp.cvt_action = new OptionSetValue((int)cvt_stagingcomponentcvt_action.Match);

                    List<Entity> mismatch = CvtHelper.FindMismatch(compToFind, comp, stgResource);
                    if (mismatch.Count > 0)
                    {
                        // we need to flag the entity
                        Relationship mismatchRelationship = new Relationship("cvt_stagingcomponent_fieldmismatch");
                        EntityCollection mismatchedRecords = new EntityCollection(mismatch);
                        comp.RelatedEntities.Add(mismatchRelationship, mismatchedRecords);
                        
                        //Relationship mismatchRsRelationship = new Relationship("cvt_stagingresource_fieldmismatch");
                        //EntityCollection mismatchedRsRecords = new EntityCollection(mismatch);
                        //stgResource.RelatedEntities.Add(mismatchRelationship, mismatchedRecords);
                    }
                }
                
            }
        }

        /// <summary>
        /// Populate new component from DataRow
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="srv"></param>
        /// <returns></returns>
        private cvt_stagingcomponent PopulateComponentFromDataRow(DataRow dr, Xrm srv)
        {
            var compTypeStr = dr[(int)ComponentsCSVColumns.ComponentType].ToString().Trim('\"');
            var compType = srv.cvt_componenttypeSet.Where(x => x.cvt_name == compTypeStr).FirstOrDefault();
            var manufacturer = srv.cvt_manufacturerSet.Where(x => x.cvt_name == dr[(int)ComponentsCSVColumns.Manufacturer].ToString().Trim('\"')).FirstOrDefault();
            var modelNumber = srv.cvt_modelSet.Where(x => x.cvt_modelnumber == dr[(int)ComponentsCSVColumns.ModelNumber].ToString().Trim('\"')).FirstOrDefault();

            cvt_stagingcomponent comp = new cvt_stagingcomponent();

            //set the comp name as comp type
            comp.cvt_name = dr[(int)ComponentsCSVColumns.ComponentType].ToString();

            if (compType != null)
            {
                comp.cvt_ComponentType = compType.ToEntityReference();
                comp.cvt_ComponentType.Name = compType.cvt_name;
            }

            //     comp.cvt_OtherComponentType = dr[(int)ComponentsCSVColumns.ComponentTypeName].ToString();
            if (manufacturer != null)
            {
                comp.cvt_ManufacturerID = manufacturer.ToEntityReference();
                comp.cvt_ManufacturerID.Name = manufacturer.cvt_name;
            }

            //comp.cvt_model = dr[(int)ComponentsCSVColumns.Model].ToString();
            // comp.cvt_OtherModel = dr[(int)ComponentsCSVColumns.ModelName].ToString();
            if (modelNumber != null)
            {
                comp.cvt_ModelNumber = modelNumber?.ToEntityReference();
                comp.cvt_ModelNumber.Name = modelNumber.cvt_modelnumber;
            }
            comp.cvt_TMSSystemName = dr[(int)ComponentsCSVColumns.SystemName].ToString();
            comp.cvt_tmssystemtypedescription = dr[(int)ComponentsCSVColumns.TMSSystemTypeDescription].ToString();

            comp.cvt_e164alias = dr[(int)ComponentsCSVColumns.E164alias].ToString();

            comp.cvt_serialnumber = (dr[(int)ComponentsCSVColumns.SerialNumber].ToString().StartsWith("DUMMYSLNO") ? null : dr[(int)ComponentsCSVColumns.SerialNumber].ToString());
            comp.cvt_PartNumber = dr[(int)ComponentsCSVColumns.PartNumber].ToString();
            comp.cvt_ipaddress = dr[(int)ComponentsCSVColumns.IpAddress].ToString();
            // comp.cvt_eenumber = dr[(int)ComponentsCSVColumns.EENumber].ToString();

            //if (!string.IsNullOrEmpty(dr[(int)ComponentsCSVColumns.Status].ToString()))
            //{
            //    var statusStr = dr[(int)ComponentsCSVColumns.Status].ToString().Replace("/", "").Replace(" ", "");
            //    cvt_stagingcomponentcvt_status status = (cvt_stagingcomponentcvt_status)Enum.Parse(typeof(cvt_stagingcomponentcvt_status), statusStr);
            //    comp.cvt_status = new OptionSetValue((int)status);
            //    comp.FormattedValues["cvt_status"] = dr[(int)ComponentsCSVColumns.Status].ToString();
            //}
            comp.cvt_status = new OptionSetValue((int)cvt_stagingcomponentcvt_status.DeployedInstalled);
            comp.FormattedValues["cvt_status"] = "Deployed/Installed";
            comp.cvt_action = new OptionSetValue((int)cvt_stagingcomponentcvt_action.CreateNewComponent);

            return comp;
        }


        /// <summary>
        /// Log custom error within the generated CSV file
        /// </summary>
        /// <param name="res"></param>
        /// <param name="message"></param>
        private void LogIMCSVCustomMessage(EnumerableRowCollection<DataRow> result, string message)
        {
            try
            {
                if (result == null || result.Count() == 0)
                    return;
                DataRow resDR = result.ElementAt(0);
                string msnColumnName = resoucesDT.Columns[(int)ResourcesCSVColumns.MasterSerialNumber].ColumnName;
                string uidColumnName = resoucesDT.Columns[(int)ResourcesCSVColumns.UniqueID].ColumnName;
                var column = (string.IsNullOrEmpty(resDR.Field<string>(msnColumnName)) ? uidColumnName : msnColumnName);

                var resrouce = resoucesDT.AsEnumerable()
                        .Where(r => r.Field<string>(column) == resDR.Field<string>(column)).FirstOrDefault();

                if (resrouce != null)
                    resrouce["ErrorDesc"] = $"{message}. {resrouce["ErrorDesc"]}";
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.ToString() + " LogIMCSVCustomMessage ");

            }
        }

        /// <summary>
        /// Log custom error within the generated CSV file
        /// </summary>
        /// <param name="res"></param>
        /// <param name="message"></param>
        private void LogIMCSVCustomMessage(cvt_stagingresource res, string message)
        {
            try
            {

                if (res == null)
                    return;

                string msnColumnName = resoucesDT.Columns[(int)ResourcesCSVColumns.MasterSerialNumber].ColumnName;
                string uidColumnName = resoucesDT.Columns[(int)ResourcesCSVColumns.UniqueID].ColumnName;

                var column = (string.IsNullOrEmpty(res.cvt_MasterSerialNumber) ? uidColumnName : msnColumnName);
                var value = (string.IsNullOrEmpty(res.cvt_MasterSerialNumber) ? res.cvt_uniqueid : res.cvt_MasterSerialNumber);

                var resrouce = resoucesDT.AsEnumerable()
                        .Where(r => r.Field<string>(column) == value).FirstOrDefault();

                if (resrouce != null)
                {
                    resrouce["ErrorDesc"] = $"{message}. {resrouce["ErrorDesc"]}";
                }
                else
                {
                    Relationship compRelationship = new Relationship("cvt_mcs_resource_cvt_component_relatedresourceid");


                    var comps = res.RelatedEntities[compRelationship].Entities
                               .Select(e => e.ToEntity<cvt_component>())
                               .ToList();

                    foreach (cvt_component comp in comps)
                    {
                        //this record imported from CRM to update
                        //We are not obtaining text info for picklists and other related entity to avoid exceptions
                        DataRow dr = resoucesDT.NewRow();
                        dr[(int)ResourcesCSVColumns.MasterSerialNumber] = res.cvt_MasterSerialNumber;
                        dr[(int)ResourcesCSVColumns.CartType] = res.cvt_carttypeid?.Name;
                        //  dr[(int)ResourcesCSVColumns.LastCalibrationDate] = res.cvt_LastCalibrationDate.ToString();
                        //  dr[(int)ResourcesCSVColumns.LastEquipmentRefreshDate] = res.cvt_LastEquipmentRefreshDate.ToString();
                        dr[(int)ResourcesCSVColumns.POC] = res.cvt_relateduser?.Name;
                        //  dr[(int)ResourcesCSVColumns.Room] = res.cvt_room;
                        //  dr[(int)ResourcesCSVColumns.SupportedTHModality] = res.cvt_supportedmodality?.Value;
                        dr[(int)ResourcesCSVColumns.SystemTypes] = res.cvt_systemtype?.Value;
                        //dr[(int)ResourcesCSVColumns.EquipmentLocationType] = res.cvt_locationuse?.Value;
                        //  dr[(int)ResourcesCSVColumns.TMPSite] = res.mcs_RelatedSiteId?.Name;
                        //dr[(int)ResourcesCSVColumns.Type] = res.mcs_Type?.Value;
                        dr[(int)ResourcesCSVColumns.UniqueID] = res.cvt_uniqueid;
                        dr[(int)ResourcesCSVColumns.MedicalCenter] = res.cvt_medicalcentername;
                        dr[(int)ResourcesCSVColumns.Ifcapponumber] = res.cvt_ifcapponumber;
                        dr[(int)ComponentsCSVColumns.ComponentType] = comp.cvt_ComponentType?.Name;
                        //dr[(int)ComponentsCSVColumns.ComponentTypeName] = comp.cvt_OtherComponentType;
                        //   dr[(int)ComponentsCSVColumns.EENumber] = comp.cvt_eenumber;
                        dr[(int)ComponentsCSVColumns.Manufacturer] = comp.cvt_ManufacturerID?.Name;
                        // dr[(int)ComponentsCSVColumns.Model] = comp.cvt_model;
                        //  dr[(int)ComponentsCSVColumns.ModelName] = comp.cvt_OtherModel;
                        dr[(int)ComponentsCSVColumns.ModelNumber] = comp.cvt_ModelNumber;
                        dr[(int)ComponentsCSVColumns.PartNumber] = comp.cvt_PartNumber;
                        dr[(int)ComponentsCSVColumns.IpAddress] = comp.cvt_ipaddress;
                        dr[(int)ComponentsCSVColumns.SerialNumber] = comp.cvt_serialnumber;
                        // dr[(int)ComponentsCSVColumns.Status] = comp.cvt_status?.Value;
                        dr[(int)ComponentsCSVColumns.SystemName] = comp.cvt_TMSSystemName;
                        dr[(int)ComponentsCSVColumns.E164alias] = comp.cvt_e164alias;

                        dr["ErrorDesc"] = $"{message} resouces id {res.Id}, component id {comp.Id}. {dr["ErrorDesc"]}";

                        resoucesDT.Rows.Add(dr);
                    }

                }

            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.ToString() + " LogIMCSVCustomMessage ");

            }
        }

        /// <summary>
        /// Log custom error within the generated CSV file
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="message"></param>
        private void LogIMCSVCustomMessage(cvt_stagingcomponent comp, string message)
        {
            try
            {
                //this record imported from CRM to update
                //We are not obtaining text info for picklists and other related entity to avoid exceptions
                DataRow dr = resoucesDT.NewRow();

                dr[(int)ComponentsCSVColumns.ComponentType] = comp.cvt_ComponentType?.Name;
                //dr[(int)ComponentsCSVColumns.ComponentTypeName] = comp.cvt_OtherComponentType;
                //dr[(int)ComponentsCSVColumns.EENumber] = comp.cvt_eenumber;
                dr[(int)ComponentsCSVColumns.Manufacturer] = comp.cvt_ManufacturerID?.Name;
                //  dr[(int)ComponentsCSVColumns.Model] = comp.cvt_model;
                // dr[(int)ComponentsCSVColumns.ModelName] = comp.cvt_OtherModel;
                dr[(int)ComponentsCSVColumns.ModelNumber] = comp.cvt_ModelNumber;
                dr[(int)ComponentsCSVColumns.PartNumber] = comp.cvt_PartNumber;
                dr[(int)ComponentsCSVColumns.IpAddress] = comp.cvt_ipaddress;
                dr[(int)ComponentsCSVColumns.SerialNumber] = comp.cvt_serialnumber;
                //dr[(int)ComponentsCSVColumns.Status] = comp.cvt_status?.Value;
                dr[(int)ComponentsCSVColumns.SystemName] = comp.cvt_TMSSystemName;
                dr[(int)ComponentsCSVColumns.E164alias] = comp.cvt_e164alias;

                dr["ErrorDesc"] = $"{message} component id {comp.Id}. {dr["ErrorDesc"]}";

                resoucesDT.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.ToString() + " LogIMCSVCustomMessage ");
            }
        }

        /// <summary>
        /// Populate new Resouce from DataRow
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="srv"></param>
        /// <returns></returns>
        private cvt_stagingresource PopulateResrouceFromDataRow(DataRow dr, Xrm srv)
        {
            var cartType = srv.cvt_carttypeSet.Where(x => x.cvt_name == dr[(int)ResourcesCSVColumns.CartType].ToString().Trim('\"')).FirstOrDefault();
            //var site = srv.mcs_siteSet.Where(x => x.mcs_name == dr[(int)ResourcesCSVColumns.TMPSite].ToString().Trim('\"')).FirstOrDefault();
            var systemUser = srv.SystemUserSet.Where(x => x.FullName == dr[(int)ResourcesCSVColumns.POC].ToString().Trim('\"')).FirstOrDefault();
            //Get facility based on station code
            var facility = srv.mcs_facilitySet.Where(x => x.mcs_StationNumber == dr[(int)ResourcesCSVColumns.StationNumber].ToString().Trim()).FirstOrDefault();

            //We are importing resources of type Technology

            var res = new cvt_stagingresource();
            //We might need to use different names
            res.mcs_name = dr[(int)ResourcesCSVColumns.MasterSerialNumber].ToString();
            //res.mcs_RelatedSiteId = site?.ToEntityReference();
            if (cartType != null)
            {
                res.cvt_carttypeid = cartType.ToEntityReference();
                res.cvt_carttypeid.Name = cartType.cvt_name;
            }

            if (facility != null)
            {
                res.mcs_Facility = facility.ToEntityReference();
                res.mcs_Facility.Name = facility.mcs_name;
            }

            mcs_resourcetype type = mcs_resourcetype.Technology;
            res.mcs_Type = new OptionSetValue((int)type);
            res.FormattedValues["mcs_type"] = "Technology";

            res.cvt_MasterSerialNumber = (dr[(int)ResourcesCSVColumns.MasterSerialNumber].ToString().StartsWith("DUMMYSLNO") ? null : dr[(int)ResourcesCSVColumns.MasterSerialNumber].ToString());

            if (!string.IsNullOrEmpty(dr[(int)ResourcesCSVColumns.SystemTypes].ToString()))
            {
                Regex pattern = new Regex("[ )/]");
                var systemTypesStr = pattern.Replace(dr[(int)ResourcesCSVColumns.SystemTypes].ToString(), "").Replace("(", "_");
                cvt_stagingresourcecvt_systemtype systemType = (cvt_stagingresourcecvt_systemtype)Enum.Parse(typeof(cvt_stagingresourcecvt_systemtype), systemTypesStr);
                res.cvt_systemtype = new OptionSetValue((int)systemType);
                res.FormattedValues["cvt_systemtype"] = dr[(int)ResourcesCSVColumns.SystemTypes].ToString();
            }

            res.cvt_locationuse = new OptionSetValue((int)cvt_stagingresourcecvt_locationuse.ClinicBased);
            res.FormattedValues["cvt_locationuse"] = "Clinic Based";

            if (systemUser != null)
            {
                res.cvt_relateduser = systemUser?.ToEntityReference();
                res.cvt_relateduser.Name = systemUser.FullName;
            }

            res.cvt_uniqueid = dr[(int)ResourcesCSVColumns.UniqueID].ToString();
            res.cvt_medicalcentername = dr[(int)ResourcesCSVColumns.MedicalCenter].ToString();
            res.cvt_ifcapponumber = dr[(int)ResourcesCSVColumns.Ifcapponumber].ToString();

            if (res.mcs_Facility != null)
            {
                AssignOwner(res);
            }

            return res;
        }

        private void AssignOwner(cvt_stagingresource res)
        {
            Logger.setMethod = "AssignOwner";
            Entity findTeam = new Entity();
            CvtHelper.AssignOwnerStagingResource(res, Logger, OrganizationService, ref findTeam);

            if (findTeam != null && findTeam.Id != Guid.Empty)
                res.OwnerId = new EntityReference(Team.EntityLogicalName, findTeam.Id);

            Logger.WriteDebugMessage("Owner Assignment function Call Complete");
        }

        #region IENImport
        public void ImportVistAClinicIENs()
        {
            numberMatched = 0;
            using (var srv = new Xrm(OrganizationService))
            {
                var bulkUpdate = new ExecuteMultipleRequest
                {
                    Requests = new OrganizationRequestCollection(),
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = true,
                        ReturnResponses = true
                    }
                };
                var VistaExport = GetFileIENs(PrimaryEntity.Id, OrganizationService);
                var res = new mcs_resource();
                var clinics = srv.mcs_resourceSet.Where(r => r.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic && r.cvt_ien == null && r.statecode == mcs_resourceState.Active).
                    Select(r => new { r.Id, r.mcs_UserNameInput, r.cvt_ien, r.mcs_Type }).ToList();
                foreach (var clinic in clinics)
                {
                    var ien = MatchName(clinic.mcs_UserNameInput, VistaExport);
                    if (!string.IsNullOrEmpty(ien))
                    {

                        var request = new UpdateRequest()
                        {
                            Target = new mcs_resource
                            {
                                cvt_ien = ien,
                                Id = clinic.Id

                            }

                        };
                        bulkUpdate.Requests.Add(request);
                        if (bulkUpdate.Requests.Count % 1000 == 0)
                        {
                            bulkUpdate = RunExecuteMultiple(bulkUpdate);
                        }
                    }

                }
                //Ended loop with uneven number of submitted requests, so doing one final ExecuteMultiple
                Logger.WriteDebugMessage(string.Format("Additional Clinics to Process: {0}", bulkUpdate.Requests.Count));

                if (bulkUpdate.Requests.Count > 0)
                {
                    RunExecuteMultiple(bulkUpdate);
                }
                Logger.WriteToFile(string.Format("Matched: {0}.  Did not match: {1}.  Total Processed: {2}.", numberMatched, clinics.Count - numberMatched, clinics.Count));
            }
        }

        private ExecuteMultipleRequest RunExecuteMultiple(ExecuteMultipleRequest bulkUpdate, bool logCustomError = false)
        {
            try
            {
                ExecuteMultipleResponse resp = (ExecuteMultipleResponse)OrganizationService.Execute(bulkUpdate);
                var errors = resp.Responses.Where(r => r.Fault != null).ToList();
                var errorString = string.Empty;
                foreach (var error in errors)
                {
                    if (logCustomError)
                    {
                        OrganizationRequest orgRequest = bulkUpdate.Requests[error.RequestIndex];
                        Entity entity;
                        string message;
                        if (orgRequest.GetType().Equals(typeof(UpdateRequest)))
                        {
                            var request = (UpdateRequest)orgRequest;
                            entity = request.Target;
                            message = $"Failed to update request {error.RequestIndex}. Error details {error.Fault.Message}";
                        }
                        else
                        {
                            var request = (CreateRequest)orgRequest;
                            entity = request.Target;
                            message = $"Failed to create request {error.RequestIndex}. Error details {error.Fault.Message}";
                        }

                        if (entity.GetType().Equals(typeof(cvt_stagingresource)))
                            LogIMCSVCustomMessage((cvt_stagingresource)entity, message);
                        else
                            LogIMCSVCustomMessage((cvt_stagingcomponent)entity, message);
                    }
                    Logger.WriteToFile(string.Format("Failed to update/Create request #{0}.  Error details {1}", error.RequestIndex, ReadFaults(error.Fault, "")));
                }
                Logger.WriteDebugMessage(string.Format("Bulk Update/Create returned {0} errors", errors.Count));
                bulkUpdate.Requests.Clear();
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(string.Format("Failed to UpdateMultiple: {0}.  Full exception List {1}", CvtHelper.BuildExceptionMessage(ex)));
            }
            return bulkUpdate;
        }

        private string ReadFaults(OrganizationServiceFault fault, string errorMessage)
        {
            var message = string.Format("{0} \n{1}\n", errorMessage, fault.Message);
            if (fault.InnerFault != null)
                ReadFaults(fault.InnerFault, message);
            return message;
        }

        private string MatchName(string clinicName, List<KeyValuePair<string, string>> VistaExport, int depth = 1)
        {
            Logger.WriteDebugMessage(string.Format("Match attempt #{0} for {1}", depth, clinicName));
            var match = VistaExport.FirstOrDefault(vistaLocation => vistaLocation.Key == clinicName);
            var ien = match.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(ien))
                numberMatched++;
            //else if (performSmartMatch && depth < 5)
            //    ien = SmartMatchIen(clinicName, VistaExport, depth);
            return ien;
        }

        //private string SmartMatchIen(string clinicName, List<KeyValuePair<string, string>> VistaExport, int depth)
        //{
        //    depth++;
        //    var originalName = clinicName;
        //    if (clinicName.ToLower().StartsWith("zz") || clinicName.StartsWith("*"))
        //        clinicName = clinicName.Trim('*', 'z', 'Z').Trim();
        //    else if (clinicName.Contains("TH"))
        //        clinicName = clinicName.Replace("TH", "TELE");
        //    if (originalName != clinicName)
        //        return MatchName(clinicName, VistaExport, depth);
        //    else
        //        Logger.WriteDebugMessage(string.Format("Unable to find relevant transformation for {0}.  Moving on to next clinic.  ", clinicName));
        //    return string.Empty;
        //}

        private List<KeyValuePair<string, string>> GetFileIENs(Guid id, IOrganizationService orgService)
        {
            Logger.WriteDebugMessage("Getting File IEN List");
            //var note = new Annotation();
            //using (var srv = new Xrm(OrganizationService))
            //    note = srv.AnnotationSet.FirstOrDefault(n => n.ObjectId.Id == id);
            //if (note == null)
            //    throw new InvalidPluginExecutionException(string.Format("Unable to get Note for record: {0}", id));
            //return ConvertFileToList(note);

            var notes = new List<Annotation>();
            using (var srv = new Xrm(OrganizationService))
                notes = srv.AnnotationSet.Where(n => n.ObjectId.Id == id).ToList();
            var file = new List<KeyValuePair<string, string>>();
            foreach (var n in notes)
            {
                file.AddRange(ConvertFileToList(n));
            }
            return file;
        }


        /// <summary>
        /// Export resrouces with error description to CSV file
        /// </summary>
        //private void ExportResourcesToCSV()
        //{
        //    try
        //    {
        //        var dtr = resoucesDT.AsEnumerable()
        //                .Where(r => !string.IsNullOrEmpty(r.Field<string>("ErrorDesc")));

        //        if (dtr.Count() == 0)
        //            return;

        //        var dt = dtr.CopyToDataTable();

        //        StringBuilder csvFile = new StringBuilder();

        //        int columnCount = dt.Columns.Count;

        //        for (int i = 0; i < columnCount; i++)
        //        {
        //            csvFile.Append(dt.Columns[i]);

        //            if (i < columnCount - 1)
        //            {
        //                csvFile.Append(",");
        //            }
        //        }
        //        csvFile.AppendLine();

        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            for (int i = 0; i < columnCount; i++)
        //            {
        //                if (!Convert.IsDBNull(dr[i]))
        //                {
        //                    csvFile.Append(dr[i].ToString().Contains(",") ? "\"" + dr[i].ToString() + "\"" : dr[i].ToString());
        //                }

        //                if (i < columnCount - 1)
        //                {
        //                    csvFile.Append(",");
        //                }
        //            }

        //            csvFile.AppendLine();
        //        }

        //        byte[] filename = Encoding.ASCII.GetBytes(csvFile.ToString());
        //        string encodedData = System.Convert.ToBase64String(filename);
        //        Annotation annotaion = new Annotation();
        //        annotaion.ObjectId = PrimaryEntity.ToEntityReference();
        //        annotaion.ObjectTypeCode = PrimaryEntity.LogicalName;
        //        annotaion.Subject = "IMDataExport";
        //        annotaion.DocumentBody = encodedData;
        //        annotaion.MimeType = @"text/csv";
        //        annotaion.NoteText = "Exported CSV file";
        //        annotaion.FileName = "IMDataExport.csv";

        //        OrganizationService.Create(annotaion);

        //    }
        //    catch(Exception ex)
        //    {
        //        Logger.WriteToFile(ex.ToString());
        //    }
        //}
        private DataTable GetResCompData(Guid id)
        {
            Annotation note;
            using (var srv = new Xrm(OrganizationService))
                note = srv.AnnotationSet.Where(n => n.ObjectId.Id == id && n.FileName == "IMData.csv").FirstOrDefault();

            return ConvertNoteToDataTable(note);
        }

        /// <summary>
        /// Convert Note field to DataTable
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        private DataTable ConvertNoteToDataTable(Annotation note)
        {
            if (note == null)
                return new DataTable();

            var rawFile = Convert.FromBase64String(note.DocumentBody.ToString());
            var fileString = System.Text.Encoding.UTF8.GetString(rawFile);
            var lines = fileString.Split(Environment.NewLine.ToCharArray());

            DataTable dTable = new DataTable();
            int index = 0;
            foreach (var line in lines)
            {
                var items = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                if (index == 0)
                {
                    foreach (string item in items)
                    {
                        dTable.Columns.Add(item);
                    }
                }
                else
                {
                    char[] charsToTrim = { '"', ' ', '\u00a0','�' };
                    var dRow = dTable.NewRow();
                    for (int i = 0; i < items.Length; i++)
                    {
                            dRow[i] = items[i].Trim().Trim(charsToTrim);
                    }

                    dTable.Rows.Add(dRow);
                }
                ++index;
            }

            return dTable;
        }




        private List<KeyValuePair<string, string>> ConvertFileToList(Annotation note)
        {
            Logger.WriteGranularTimingMessage("Begin File Conversion");
            var IENList = new List<KeyValuePair<string, string>>();
            var rawFile = Convert.FromBase64String(note.DocumentBody.ToString());
            var fileString = System.Text.Encoding.UTF8.GetString(rawFile);

            var lineBreak = Environment.NewLine;
            var lines = fileString.Split(lineBreak.ToCharArray());
            foreach (var line in lines)
            {
                var items = line.Split(',');
                if (items.Length > 1)
                    IENList.Add(new KeyValuePair<string, string>(items[0], items[1]));
            }
            Logger.WriteGranularTimingMessage("Completed File Conversion");
            return IENList;
        }
        #endregion

        public override string McsSettingsDebugField
        {
            get { return "mcs_serviceplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }

        private Dictionary<string, List<string>> NameMatches = new Dictionary<string, List<string>> {
            { "TH", new List<string>() { "Tele", "TH", "Telehealth" } },
            { "Ind", new List<string>() { "Indiv", "Indi", "Individual" } }

        };

        private List<List<string>> NameGroups = new List<List<string>>
        {
            new List<string>() { "Audiology", "Aud", "Audio", "Audiololy" },
            new List<string>() { "GRP", "Group", "GP" },
            new List<string>() { "Ind", "Indiv", "Indi", "Individual" },
            new List<string>() { "Nutr","Nutri","Nut","Nutrition" }
        };

    }

}
