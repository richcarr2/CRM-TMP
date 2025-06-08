using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace MCSShared
{
    public static partial class CvtHelper
    {
        //1)cvt_participatingsite -  Status Reason change; To Be Scheduled = Yes or No
        //2)cvt_facilityapproval - Status Reason change; Into or Out of Approved Status

        #region Helper Functions
        /// <summary>
        /// Pass in the Participating Site and retrieve the type of Scheduling Package
        /// </summary>
        /// <returns>'VVC', 'Group', 'Individual'</returns>
        public static string parentSP(cvt_participatingsite PS, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                #region Get the SP and validate the correct fields exist
                Logger.WriteDebugMessage("Get this PS's parent SP.");

                if (PS.cvt_resourcepackage == null)
                {
                    Logger.WriteDebugMessage("No Scheduling Package was listed as the parent of this Participating Site. PSId: " + PS.Id);
                    return string.Empty;
                }

                var parentSP = srv.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == PS.cvt_resourcepackage.Id);
                if (parentSP == null)
                {
                    Logger.WriteDebugMessage("No Scheduling Package was retrieved as the parent of this Participating Site. PSId: " + PS.Id);
                    return string.Empty;
                }

                if (parentSP.cvt_patientlocationtype == null)
                {
                    Logger.WriteDebugMessage("No Patient Location Type is listed for this Scheduling Package. SPId: " + parentSP.Id);
                    return string.Empty;
                }

                if (parentSP.cvt_groupappointment == null)
                {
                    Logger.WriteDebugMessage("No Group/Individual indicator is listed for this Scheduling Package. SPId: " + parentSP.Id);
                    return string.Empty;
                }
                #endregion

                #region Return Type of parent SP
                if (parentSP.cvt_patientlocationtype.Value == (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone)
                {
                    return "VVC";
                }
                if (parentSP.cvt_groupappointment.Value == true)
                {
                    return "Group";
                }
                else
                    return "Individual";
                #endregion
            }
        }

        /// <summary>
        /// Pass in the Facility Approval and retrieve the type of Scheduling Package
        /// </summary>
        /// <returns>'VVC', 'Group', 'Individual'</returns>
        public static string parentSP(cvt_facilityapproval FA, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                #region Get the SP and validate the correct fields exist
                Logger.WriteDebugMessage("Get this FA's parent SP.");

                if (FA.cvt_resourcepackage == null)
                {
                    Logger.WriteDebugMessage("No Scheduling Package was listed as the parent of this Facility Approval. FAId: " + FA.Id);
                    return string.Empty;
                }

                var parentSP = srv.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == FA.cvt_resourcepackage.Id);
                if (parentSP == null)
                {
                    Logger.WriteDebugMessage("No Scheduling Package was retrieved as the parent of this Facility Approval. FAId: " + FA.Id);
                    return string.Empty;
                }

                if (parentSP.cvt_patientlocationtype == null)
                {
                    Logger.WriteDebugMessage("No Patient Location Type is listed for this Scheduling Package. SPId: " + parentSP.Id);
                    return string.Empty;
                }

                if (parentSP.cvt_groupappointment == null)
                {
                    Logger.WriteDebugMessage("No Group/Individual indicator is listed for this Scheduling Package. SPId: " + parentSP.Id);
                    return string.Empty;
                }
                #endregion

                #region Return Type of parent SP
                if (parentSP.cvt_patientlocationtype.Value == (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone)
                {
                    return "VVC";
                }
                if (parentSP.cvt_groupappointment.Value == true)
                {
                    return "Group";
                }
                else
                    return "Individual";
                #endregion
            }
        }

        public static bool IsVvcWithPatientParticipatingSite(Guid schedulingPackageId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                var parentSP = srv.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == schedulingPackageId);

                #region Return Type of parent SP
                var isVvcWithPatPS = false;
                if (parentSP.cvt_patientlocationtype.Value == (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone)
                {
                    var patPS = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.cvt_resourcepackage.Id == parentSP.Id && ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient 
                    && ps.cvt_scheduleable.Value == true && ps.statecode.Value == (int)cvt_participatingsiteState.Active);
                    if (patPS != null)
                        isVvcWithPatPS = true;
                }

                return isVvcWithPatPS;
                #endregion
            }
        }

        public static List<EntityReference> getOppositePSFromPS(cvt_participatingsite inputPS, Guid VCSiteId, bool returningPatient, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.WriteDebugMessage("Starting");
            List<EntityReference> result = new List<EntityReference>();

            using (var srv = new Xrm(OrganizationService))
            {
                #region Get the SP and then opposite PS
                Logger.WriteDebugMessage("Get this PS's parent SP.");

                if (inputPS.cvt_resourcepackage == null)
                {
                    Logger.WriteDebugMessage("No Scheduling Package was listed as the parent of this Participating Site. PSId: " + inputPS.Id);
                    return result;
                }

                int locationType = (returningPatient) ? (int)cvt_participatingsitecvt_locationtype.Patient : (int)cvt_participatingsitecvt_locationtype.Provider;
                var siblingOppositePS = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_resourcepackage.Id == inputPS.cvt_resourcepackage.Id
                        && ps.cvt_locationtype.Value == locationType
                        && ps.cvt_scheduleable.Value == true
                        && ps.statuscode.Value == (int)cvt_participatingsite_statuscode.Active);

                if (siblingOppositePS == null || siblingOppositePS.ToList().Count == 0)
                {
                    Logger.WriteDebugMessage("No opposing Participating Sites were retrieved as relating to this Participating Site. PSId: " + inputPS.Id);
                    return result;
                }

                #endregion

                #region Create List of PS; match site if VCSiteId value is present
                foreach (cvt_participatingsite ps in siblingOppositePS)
                {
                    if (VCSiteId != Guid.Empty)
                    {
                        if (ps.cvt_site.Id == VCSiteId)
                        {
                            result.Add(new EntityReference(cvt_participatingsite.EntityLogicalName, ps.Id));
                        }
                    }
                    else
                        result.Add(new EntityReference(cvt_participatingsite.EntityLogicalName, ps.Id));
                }
                #endregion
                //Return list of Participating Sites as Entity References
                return result;
            }
        }

        /// <summary>
        /// Build Constraint Based Group
        /// </summary>
        public static Guid BuildOutforSP(cvt_resourcepackage schPackage, StringBuilder strBuilder, IOrganizationService OrganizationService, int ReqCount, int constraintBasedGroupTypeCode, Boolean Site, MCSLogger Logger)
        {
            //constraintBasedGroupTypeCode
            //        Static = 0,
            //        Dynamic = 1,
            //        Implicit = 2

            //Correctly tag the XML
            var mainStrBuilder = BuildConstraintsXML(strBuilder);

            var constraintBasedGroupSetup = new ConstraintBasedGroup
            {
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, schPackage.OwningBusinessUnit.Id),
                Constraints = mainStrBuilder.ToString(),
                Name = schPackage.cvt_name,
                GroupTypeCode = new OptionSetValue(constraintBasedGroupTypeCode)
            };
            //Create the new Constraint Based Group
            var newConstraintGroup = OrganizationService.Create(constraintBasedGroupSetup);
            Logger.WriteDebugMessage("Newly created Constraint Based Group ID:" + newConstraintGroup);

            var newSpec = new ResourceSpec
            {
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, schPackage.OwningBusinessUnit.Id),
                ObjectiveExpression = @"<Expression><Body>udf ""Random""(factory,resource,appointment,request,leftoffset,rightoffset)</Body><Parameters><Parameter name=""factory"" /><Parameter name=""resource"" /><Parameter name=""appointment"" /><Parameter name=""request"" /><Parameter name=""leftoffset"" /><Parameter name=""rightoffset"" /></Parameters><Properties EvaluationInterval=""P0D"" evaluationcost=""0"" /></Expression>",
                RequiredCount = ReqCount,
                Name = "Selection Rule",
                GroupObjectId = newConstraintGroup,
                SameSite = Site
            };
            var specId = OrganizationService.Create(newSpec);
            Logger.WriteDebugMessage("Newly created ResourceSpec ID: " + specId);
            return specId;
        }

        /// <summary>
        /// Split the Valid Provider site into an array, Join to the SP list, Sort Alphabetically, Get distinct
        /// </summary>
        /// <param name="newValues">The newly generated Values</param>
        /// <param name="existingValues">The existing Values</param>
        /// <returns>The updated list in the form of comma separated string</returns>
        private static string OldConsolidateDistinctList(string newValues, string existingValues)
        {
            string consolidatedString = string.Empty;

            if (string.IsNullOrWhiteSpace(existingValues))
                consolidatedString = newValues;
            else if (string.IsNullOrWhiteSpace(newValues))
                consolidatedString = existingValues;
            else
            {
                var separator = ';';
                var newList = newValues.Split(separator).Concat(existingValues.Split(separator)).ToList();
                consolidatedString = string.Join(separator.ToString(), newList.OrderBy(q => q).Distinct());
            }

            return consolidatedString;
        }

        private static string ConsolidateDistinctList(string values)
        {
            char[] charsToTrim = { ' ', ';' };
            values = values.TrimEnd(charsToTrim);

            var separator = ';';
            string consolidatedString = values;
            var valueList = values.Split(separator).ToList();

            //Get Distinct and sort
            if (valueList.Count > 1)
            {
                consolidatedString = string.Join(separator.ToString(), valueList.OrderBy(q => q).Distinct());
            }

            consolidatedString = consolidatedString.TrimStart(charsToTrim);
            consolidatedString = consolidatedString.TrimStart(charsToTrim);
            return CvtHelper.ValidateLength(consolidatedString, 2500).TrimEnd(charsToTrim);
        }

        /// <summary>
        /// Method to update the Scheduling package with the Provider Sites, Patient Sites and Provider Details
        /// </summary>
        public static void UpdateSP(cvt_resourcepackage thisSP, Guid newServiceId, string serviceLog, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                cvt_resourcepackage updateSP = new cvt_resourcepackage
                {
                    Id = thisSP.Id
                };

                var updateCount = 0;
                Guid formerService = Guid.Empty;

                #region Service
                if (newServiceId == Guid.Empty) //No Valid Service
                {
                    //Set to null
                    formerService = (thisSP.cvt_relatedservice != null) ? thisSP.cvt_relatedservice.Id : Guid.Empty;
                    if (formerService != Guid.Empty)
                    {
                        Logger.WriteDebugMessage("Service Reference is now being set to null, update the SP.");
                        updateSP.cvt_relatedservice = null;
                        updateCount += 1;
                    }
                }
                else //Valid Service - always use the new id if present
                {
                    formerService = (thisSP.cvt_relatedservice != null) ? thisSP.cvt_relatedservice.Id : Guid.Empty;
                    updateSP.cvt_relatedservice = new EntityReference(Service.EntityLogicalName, newServiceId);
                    updateCount += 1;
                    //serviceLog += "Updating service.";
                }
                #endregion

                #region Service Log
                if (!string.IsNullOrWhiteSpace(serviceLog))
                {
                    if (thisSP.cvt_servicedetails != serviceLog)
                    {
                        updateSP.cvt_servicedetails = serviceLog;
                        updateCount += 1;
                    }
                }
                else
                {
                    updateSP.cvt_servicedetails = string.Empty;
                    updateCount += 1;
                }

                #endregion

                string providerSites = "";
                string patientSites = "";
                string providers = "";
                string patientVCs = "";
                string providerVCs = "";
                var relatedPS = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_resourcepackage.Id == thisSP.Id && ps.cvt_scheduleable.Value == true && ps.statecode.Value == (int)cvt_participatingsiteState.Active);
                foreach (cvt_participatingsite ps in relatedPS)
                {
                    Logger.WriteDebugMessage($"Name: {ps.cvt_name} IsPatient: {ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient}");
                    if (ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient)
                    {
                        patientSites += ps.cvt_site.Name + ";";
                        providerSites += ps.cvt_oppositevalidsites + ";";
                    }
                    else
                    {
                        patientSites += ps.cvt_oppositevalidsites + ";";
                        providerSites += ps.cvt_site.Name + ";";
                    }
                    patientVCs += ps.cvt_patientsitevistaclinics + ";";
                    providerVCs += ps.cvt_providersitevistaclinics + ";";
                    providers += ps.cvt_providers + ";";
                }

                #region Valid Providers
                var prositeUsersToUpdate = ConsolidateDistinctList(providers);

                //Compare it with the existing SP list to see if update is needed
                Logger.WriteDebugMessage($"String Comparison: thisSP.cvt_providers: {thisSP.cvt_providers} || prositeUsersToUpdate: {prositeUsersToUpdate}");
                if (thisSP.cvt_providers != prositeUsersToUpdate)
                {
                    updateSP.cvt_providers = prositeUsersToUpdate;
                    updateCount += 1;
                }
                #endregion

                #region Valid Provider Sites
                var providerSiteString = ConsolidateDistinctList(providerSites);

                //Compare it with the existing SP list to see if update is needed
                Logger.WriteDebugMessage($"String Comparison: thisSP.cvt_providersites: {thisSP.cvt_providersites} || providerSiteString: {providerSiteString}");
                if (thisSP.cvt_providersites != providerSiteString)
                {
                    updateSP.cvt_providersites = providerSiteString;
                    updateCount += 1;
                }
                #endregion

                #region Valid Patient Sites
                var patientSiteString = ConsolidateDistinctList(patientSites);

                //Compare it with the existing SP list to see if update is needed
                Logger.WriteDebugMessage($"String Comparison: thisSP.cvt_patientsites: {thisSP.cvt_patientsites} || patientSiteString: {patientSiteString}");
                if (thisSP.cvt_patientsites != patientSiteString)
                {
                    updateSP.cvt_patientsites = patientSiteString;
                    updateCount += 1;
                }
                #endregion

                #region Provider Site VCs
                var providerVCString = ConsolidateDistinctList(providerVCs);

                //Compare it with the existing SP list to see if update is needed
                Logger.WriteDebugMessage($"String Comparison: thisSP.cvt_providersitevistaclinics: {thisSP.cvt_providersitevistaclinics} || providerVCString: {providerVCString}");
                if (thisSP.cvt_providersitevistaclinics != providerVCString)
                {
                    updateSP.cvt_providersitevistaclinics = providerVCString;
                    updateCount += 1;
                }
                #endregion

                #region Patient Site VCs
                var patientVCString = ConsolidateDistinctList(patientVCs);

                //Compare it with the existing SP list to see if update is needed
                Logger.WriteDebugMessage($"String Comparison: thisSP.cvt_patientsitevistaclinics: {thisSP.cvt_patientsitevistaclinics} || patientVCString: {patientVCString}");
                if (thisSP.cvt_patientsitevistaclinics != patientVCString)
                {
                    updateSP.cvt_patientsitevistaclinics = patientVCString;
                    updateCount += 1;
                }
                #endregion

                #region Update SP
                if (updateCount > 0)
                {
                    OrganizationService.Update(updateSP);
                    Logger.WriteDebugMessage("SP Updated");
                    if (formerService != Guid.Empty)
                    {
                        //Search on the prior service and attempt to delete it.
                        var ServiceActivity = srv.ServiceAppointmentSet.FirstOrDefault(i => i.ServiceId.Id == formerService);
                        //If an associated SA doesn't exist, delete
                        if (ServiceActivity == null)
                        {
                            OrganizationService.Delete(Service.EntityLogicalName, formerService);
                            Logger.WriteDebugMessage("Prior Service successfully deleted.");
                        }
                        else
                            Logger.WriteDebugMessage("Prior Service could not be deleted, associated Service Activities exist.");
                    }
                }
                else
                    Logger.WriteDebugMessage("No new systematically built values to update on SP");

                #endregion
            }
        }

        public static void UpdatePS(cvt_participatingsite thisPS, string serviceLog, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            FinalUpdatePS(thisPS, string.Empty, string.Empty, string.Empty, string.Empty, serviceLog, Logger, OrganizationService, Guid.Empty, string.Empty);
        }
        public static void UpdatePS(cvt_participatingsite thisPS, string validOppositeSites, string providers, string providerVCs, string patientVCs, string serviceLog, MCSLogger Logger, IOrganizationService OrganizationService,  Guid newServiceId, string groupPatVariable)
        {
            FinalUpdatePS(thisPS, validOppositeSites, providers, providerVCs, patientVCs, serviceLog, Logger, OrganizationService, newServiceId, groupPatVariable);
        }

        public static void FinalUpdatePS(cvt_participatingsite thisPS, string validOppositeSites, string providers, string providerVCs, string patientVCs, string serviceLog, MCSLogger Logger, IOrganizationService OrganizationService, Guid newServiceId, string groupPatVariable)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                cvt_participatingsite updatePS = new cvt_participatingsite()
                {
                    Id = thisPS.Id
                };

                var updateCount = 0;
                Guid formerService = Guid.Empty;

                var foundPS = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == thisPS.Id);
                #region Service
                Logger.WriteDebugMessage("Assessing Service.");
                if (newServiceId == Guid.Empty) //No Valid Service
                {
                    //Set to null
                    formerService = (foundPS.cvt_relatedservice != null) ? foundPS.cvt_relatedservice.Id : Guid.Empty;
                    if (formerService != Guid.Empty)
                    {
                        Logger.WriteDebugMessage("Service Reference is now being set to null, update the PS.");
                        updatePS.cvt_relatedservice = null;
                        updateCount += 1;
                    }
                }
                else //Valid Service - always use the new id if present
                {
                    formerService = (foundPS.cvt_relatedservice != null) ? foundPS.cvt_relatedservice.Id : Guid.Empty;
                    updatePS.cvt_relatedservice = new EntityReference(Service.EntityLogicalName, newServiceId);
                    updateCount += 1;
                    //serviceLog += "Updating service.";
                }

                #endregion

                #region Service Log
                Logger.WriteDebugMessage("Assessing Service Log.");
                if (!string.IsNullOrWhiteSpace(serviceLog))
                {
                    if (foundPS.cvt_servicedetails != serviceLog)
                    {
                        updatePS.cvt_servicedetails = serviceLog;
                        updateCount += 1;
                    }
                }
                else
                {
                    updatePS.cvt_servicedetails = string.Empty;
                    updateCount += 1;
                }
                #endregion

                #region Opposite Sides
                Logger.WriteDebugMessage($"Assessing Opposite Sites: {validOppositeSites}");
                if (!string.IsNullOrWhiteSpace(validOppositeSites))
                {
                    var siteString = ValidateLength(ConsolidateDistinctList(validOppositeSites), 2500);
                    Logger.WriteDebugMessage($"ConsolidateDistinctList into siteString: {siteString}");
                    if (foundPS.cvt_oppositevalidsites != siteString)
                    {
                        updatePS.cvt_oppositevalidsites = siteString;
                        updateCount += 1;
                    }
                }
                else
                {
                    updatePS.cvt_oppositevalidsites = string.Empty;
                    updateCount += 1;
                }
                Logger.WriteDebugMessage($"Assessed Opposite Sites: {updatePS.cvt_oppositevalidsites}");
                #endregion

                #region Providers
                Logger.WriteDebugMessage($"Assessing Providers: {providers}");
                if (!string.IsNullOrWhiteSpace(providers))
                {
                    var providersString = ConsolidateDistinctList(providers);
                    Logger.WriteDebugMessage($"ConsolidateDistinctList into providersString: {providersString}");
                    if (foundPS.cvt_providers != providersString)
                    {
                        updatePS.cvt_providers = providersString;
                        updateCount += 1;
                    }
                }
                else
                {
                    updatePS.cvt_providers = string.Empty;
                    updateCount += 1;
                }
                Logger.WriteDebugMessage($"Assessed Providers: {updatePS.cvt_providers}");
                #endregion

                #region Group Pat Variable
                Logger.WriteDebugMessage($"Assessing Group Pat Variable: {groupPatVariable}");
                if (!string.IsNullOrWhiteSpace(groupPatVariable))
                {
                    if (foundPS.cvt_grouppatientbranch != groupPatVariable)
                    {
                        updatePS.cvt_grouppatientbranch = groupPatVariable;
                        updateCount += 1;
                    }
                }
                else
                {
                    updatePS.cvt_grouppatientbranch = string.Empty;
                    updateCount += 1;
                }
                Logger.WriteDebugMessage($"Assessed Group Pat Variable: {updatePS.cvt_grouppatientbranch}");
                #endregion

                #region Patient Site VCs
                Logger.WriteDebugMessage($"Assessing Patient VCs: {patientVCs}");
                
                //Compare it with the existing SP list to see if update is needed
                if (!string.IsNullOrWhiteSpace(patientVCs))
                {
                    var patientVCString = ConsolidateDistinctList(patientVCs);
                    Logger.WriteDebugMessage($"ConsolidateDistinctList into patientVCString: {patientVCString}");
                    if (thisPS.cvt_patientsitevistaclinics != patientVCString)
                    {
                        updatePS.cvt_patientsitevistaclinics = patientVCString;
                        updateCount += 1;
                    }
                }
                else
                {
                    updatePS.cvt_patientsitevistaclinics = string.Empty;
                    updateCount += 1;
                }
                Logger.WriteDebugMessage($"Assessed Patient VCs: {updatePS.cvt_patientsitevistaclinics}");
                #endregion

                #region Provider Site VCs
                Logger.WriteDebugMessage($"Assessing Provider VCs: {providers}");
                if (!string.IsNullOrWhiteSpace(providers))
                {
                    var providerVCString = ConsolidateDistinctList(providerVCs);
                    Logger.WriteDebugMessage($"ConsolidateDistinctList into providerVCString: {providerVCString}");
                    if (thisPS.cvt_providersitevistaclinics != providerVCString)
                    {
                        updatePS.cvt_providersitevistaclinics = providerVCString;
                        updateCount += 1;
                    }
                }
                else
                {
                    updatePS.cvt_providersitevistaclinics = string.Empty;
                    updateCount += 1;
                }
                Logger.WriteDebugMessage($"Assessed Provider VCs: {updatePS.cvt_providersitevistaclinics}");
                #endregion

                Logger.WriteDebugMessage("Assessing for a field change. UpdateCount = " + updateCount);
                if (updateCount > 0)
                {
                    OrganizationService.Update(updatePS);
                    Logger.WriteDebugMessage("Participating Site Updated. Name: " + foundPS.cvt_name);
                    if (formerService != Guid.Empty)
                    {
                        //Search on the prior service and attempt to delete it.
                        var ServiceActivity = srv.ServiceAppointmentSet.FirstOrDefault(i => i.ServiceId.Id == formerService);
                        //If an associated SA doesn't exist, delete
                        if (ServiceActivity == null)
                        {
                            OrganizationService.Delete(Service.EntityLogicalName, formerService);
                            Logger.WriteDebugMessage("Prior Service successfully deleted.");
                        }
                        else
                            Logger.WriteDebugMessage("Prior Service could not be deleted, associated Service Activities exist.");
                    }

                    var schedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(sp => sp.Id == foundPS.cvt_resourcepackage.Id);

                    //Update the Scheduling Package with the Patient Site, Provider Site and Providers string fields
                    if (schedulingPackage != null && !IsVvcWithPatientParticipatingSite(schedulingPackage.Id, OrganizationService, Logger))
                    {
                        Logger.WriteDebugMessage("About to UpdateSP from UpdatePS.");
                        //if the service is not built, then the current PS is not actually a valid addition.
                        var validPS = foundPS.cvt_site.Name;
                        if (newServiceId == null)
                            validPS = "";
                        var log = "This is a non-VVC non-Group Scheduling Package: Find Service Details on Patient Participating Sites.";
                        if (schedulingPackage.cvt_groupappointment.Value == true)
                            log = "This is a Group Scheduling Package: Find Service Details on Provider Participating Sites.";
                        
                        Logger.WriteDebugMessage($"Updating SP from this PS: {foundPS.cvt_site.Name}; validOppositeSites: {updatePS.cvt_oppositevalidsites}; providers: {updatePS.cvt_providers}; patVCs: {updatePS.cvt_patientsitevistaclinics}; proVCs: {updatePS.cvt_providersitevistaclinics}");
                        UpdateSP(schedulingPackage, Guid.Empty, log, Logger, OrganizationService);
                    }
                }
                else
                    Logger.WriteDebugMessage("No new systematically built values to update on PS");
            }
        }

        /// <summary>
        /// Build Constraint Based Group
        /// </summary>
        public static Guid CreateSpecId(Guid BuId, StringBuilder strBuilder, IOrganizationService OrganizationService, int ReqCount, int constraintBasedGroupTypeCode, Boolean Site, MCSLogger Logger)
        {
            //constraintBasedGroupTypeCode
            //        Static = 0,
            //        Dynamic = 1,
            //        Implicit = 2

            //Correctly tag the XML
            var mainStrBuilder = BuildConstraintsXML(strBuilder);

            var constraintBasedGroupSetup = new ConstraintBasedGroup
            {
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, BuId),
                Constraints = mainStrBuilder.ToString(),
                Name = "SystemGenerated CBG",
                GroupTypeCode = new OptionSetValue(constraintBasedGroupTypeCode)
            };
            //Create the new Constraint Based Group
            var newConstraintGroup = OrganizationService.Create(constraintBasedGroupSetup);
            Logger.WriteDebugMessage("Newly created Constraint Based Group ID: " + newConstraintGroup);

            var newSpec = new ResourceSpec
            {
                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, BuId),
                ObjectiveExpression = @"<Expression><Body>udf ""Random""(factory,resource,appointment,request,leftoffset,rightoffset)</Body><Parameters><Parameter name=""factory"" /><Parameter name=""resource"" /><Parameter name=""appointment"" /><Parameter name=""request"" /><Parameter name=""leftoffset"" /><Parameter name=""rightoffset"" /></Parameters><Properties EvaluationInterval=""P0D"" evaluationcost=""0"" /></Expression>",
                RequiredCount = ReqCount,
                Name = "Selection Rule",
                GroupObjectId = newConstraintGroup,
                SameSite = Site
            };
            var specId = OrganizationService.Create(newSpec);
            Logger.WriteDebugMessage("Newly created Resource Spec ID: " + specId);
            return specId;
        }
        #endregion

        #region Service Building Entry Point
        public static void EntryFromFacilityApproval(cvt_facilityapproval fa, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Service Build cannot be started from Facility Approval.");
            return;

            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                //Evaluate SP
                var type = parentSP(fa, OrganizationService, Logger);
                int locationType = 0;
                switch (type)
                {
                    //If non-group, get list of Patient Sites that belong to that Facility Approval
                    case "Individual":
                        locationType = (int)cvt_participatingsitecvt_locationtype.Patient;
                            break;
                    //If group, get list of Provider Sites that belong to that Facility Approval
                    case "Group":
                        locationType = (int)cvt_participatingsitecvt_locationtype.Provider;
                        break;
                    //VVC has no FA
                }
                if (locationType != 0)
                {
                    var participatingSites = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_resourcepackage.Id == fa.cvt_resourcepackage.Id
                            && ps.cvt_locationtype.Value == locationType
                            && ps.cvt_scheduleable.Value == true
                            && ps.statuscode.Value == (int)cvt_participatingsite_statuscode.Active);

                    if (participatingSites == null || participatingSites.ToList().Count == 0)
                    {
                        Logger.WriteDebugMessage("No Participating Sites were retrieved as relating to this Facility Approval. FAId: " + fa.Id);
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Found participating sites.  Passing records to ServiceBuildFromPSList.");
                        ServiceBuildFromPSList(participatingSites.ToList<cvt_participatingsite>(), Logger, OrganizationService);
                        
                    }
                }

                else
                    Logger.WriteDebugMessage("Error: VVC Scheduling Package should not have a Facility Approval record.");
            }
        }

        public static void EntryFromParticipatingSite(cvt_participatingsite ps, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                var psRetrieve = srv.cvt_participatingsiteSet.FirstOrDefault(p => p.Id == ps.Id);
                List<EntityReference> erList = GetERFromPS(psRetrieve, Guid.Empty, Logger, OrganizationService);

                Logger.WriteDebugMessage("Submitting erList with a count of " + erList.Count + " to ServiceBuildFromERList function.");
                //Submit erList to BuildService functions
                ServiceBuildFromERList(erList, Logger, OrganizationService);
            }
        }

        #endregion

        ///Build a service for each Patient PS that has 1+ Provider PS AND FA or no FA but is intrafacility
        ///If the checks pass in the Patient PS, then we can check for opposing Provider PS and verify FAs
        #region Determining the PS/SP to check
        public static List<EntityReference>     GetERFromPS(cvt_participatingsite ps, Guid VCSiteId, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                List<EntityReference> erList = new List<EntityReference>();

                string spType = parentSP(ps, OrganizationService, Logger);

                //Determine if the ProviderPS has a PatientPS that is the Site listed on the 690 VC, and if so, if parent SP is group, then record the Provider PS into one list, if non - group, record the Patient PS in another list.
                Logger.WriteDebugMessage("Retrieved Scheduling Package type.");
                if (ps.cvt_locationtype != null && ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider)
                {
                    Logger.WriteDebugMessage("PS being assessed is Provider.");
                    //Determine if VVC, non-VVC Group or non-VVC non-Group
                    switch (spType)
                    {
                        case "VVC": //SP is VVC (group or ind), then record the parent SP as the guid
                            Logger.WriteDebugMessage("SP is VVC");
                            erList.Add(new EntityReference(cvt_resourcepackage.EntityLogicalName, ps.cvt_resourcepackage.Id));
                            erList.AddRange(getOppositePSFromPS(ps, VCSiteId, true, OrganizationService, Logger));
                            break;
                        case "Individual": //SP is non-group, then record the opposite Patient PSs
                            Logger.WriteDebugMessage("SP is Individual");
                            erList.AddRange(getOppositePSFromPS(ps, VCSiteId, true, OrganizationService, Logger));
                            break;
                        case "Group": //SP is non VVC group, then record the Provider PS
                            Logger.WriteDebugMessage("SP is Group");
                            erList.Add(new EntityReference(cvt_participatingsite.EntityLogicalName, ps.Id));
                            break;
                    }
                }
                else //Patient
                {
                    Logger.WriteDebugMessage("PS being assessed is Patient.");
                    //Determine if VVC, non-VVC Group or non-VVC non-Group
                    switch (spType)
                    {
                        case "VVC": //SP is VVC (group or ind), then record the parent SP as the guid
                            Logger.WriteDebugMessage("SP is VVC");
                            if(IsVvcWithPatientParticipatingSite(ps.cvt_resourcepackage.Id, OrganizationService, Logger))
                                erList.Add(new EntityReference(cvt_participatingsite.EntityLogicalName, ps.Id));
                            break;
                        case "Individual": //SP is non-group, then record this Patient PS
                            erList.Add(new EntityReference(cvt_participatingsite.EntityLogicalName, ps.Id));
                            break;
                        case "Group": //SP is non VVC group, then record the opposite Provider PS
                            erList.AddRange(getOppositePSFromPS(ps, VCSiteId, false, OrganizationService, Logger));
                            break;
                    }
                }
                //Return distinct list of Entity References
                return erList.Distinct().ToList();
            }
        }

        /// <summary>
        /// Pass in the list of PSs, and then go generate the service for each PS
        /// </summary>
        /// <param name="psList"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        public static void ServiceBuildFromPSList(List<cvt_participatingsite> psList, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            foreach (cvt_participatingsite ps in psList)
            {
                ServiceBuild(ps, Logger, OrganizationService);
            }
        }

        /// <summary>
        /// Pass in the list of Entity References, and then go generate the service for each PS/SP depending
        /// </summary>
        /// <param name="list"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        public static void ServiceBuildFromERList(List<EntityReference> list, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                if (list.Count != 0)
                {
                    foreach (EntityReference i in list)
                    {
                        switch (i.LogicalName)
                        {
                            case cvt_participatingsite.EntityLogicalName:
                                Logger.WriteDebugMessage(String.Format("ER ({0}) is a PS.", i.Id));
                                var ps = srv.cvt_participatingsiteSet.FirstOrDefault(x => x.Id == i.Id);
                                if (ps != null)
                                {
                                    ServiceBuild(ps, Logger, OrganizationService);
                                }
                                break;
                            case cvt_resourcepackage.EntityLogicalName:
                                Logger.WriteDebugMessage(String.Format("ER ({0}) is a SP.", i.Id));
                                var sp = srv.cvt_resourcepackageSet.FirstOrDefault(x => x.Id == i.Id && x.statecode.Value ==(int)cvt_resourcepackageState.Active);
                                if (sp != null)
                                {
                                    ServiceBuild(sp, Logger, OrganizationService);
                                }
                                else
                                {
                                    Logger.WriteDebugMessage("SP can't be found or is not active.");
                                }
                                break;
                        }
                    }
                }
                else
                    Logger.WriteDebugMessage("No ERs to build service from.");
            }
        }

        public static string ValidatePatientPS(cvt_resourcepackage thisSP, cvt_participatingsite patientPS, MCSLogger Logger, IOrganizationService OrganizationService, out string outServiceLog, out string outPatActivityParty, out string outVCs, out string validationLogs)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                #region Declaration of Variables
                outServiceLog = "";
                outPatActivityParty = "";
                outVCs = "";
                validationLogs = string.Empty;
                var patActivityParty = "";
                var usersListed = ""; //Not used currently.
                //Build the service rules for this Provider PS
                var builder = new System.Text.StringBuilder("");
                var builderAND = new System.Text.StringBuilder("");
                var builderPairedOR = new System.Text.StringBuilder("");
                int countSR = 0;
                #endregion

                #region Retrieve Patient PS Resources
                //Retrieving the Active resources listed on the Patient PS.
                var patSR = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_participatingsite.Id == patientPS.Id && sr.statuscode.Value == (int)cvt_schedulingresource_statuscode.Active);
                int patSRCount = (patSR != null ? patSR.ToList().Count : 0);
                outServiceLog += String.Format("\nReviewed Patient Site ({0}). Found {1} Patient Resources.\n", patientPS.cvt_site.Name, patSRCount.ToString());
                Logger.WriteDebugMessage(String.Format("Found {0} Patient Scheduling Resources. ", patSRCount.ToString()));

                if (patSRCount == 0)
                {
                    Logger.WriteDebugMessage("No Resources needed. Finished patient side evaluation.");
                    outServiceLog += "No Resources needed.\n";

                    cvt_participatingsite updateEmptyPS = new cvt_participatingsite()
                    {
                        Id = patientPS.Id,
                        cvt_servicedetails = "Group: The Service should be generated on the Provider Participating Site."
                    };

                    if (thisSP.cvt_groupappointment.Value == true)
                        OrganizationService.Update(updateEmptyPS);
                    return string.Empty;
                }
                #endregion

                #region Loop through all of the Patient Site resources and add them to the builder
                //Resource/Equipment/User is listed as resources within the Constraints attribute of a Constraint Based Group
                //The Constraint Based Group is listed within the Resource Spec as the GroupObjectId
                //The Resource Spec is listed on the Service
                //Resource Spec contains required count 1/all
                //outServiceLog += "Evaluating Scheduling Resources.\n";

                Logger.WriteDebugMessage("Evaluating Patient Site Resources");
                //Build the service rules for this Provider PS
                foreach (var item in patSR)
                {
                    Logger.WriteDebugMessage("Starting loop for " + item.cvt_name);
                    if (item.cvt_schedulingresourcetype != null)
                    {
                        Guid sysResId = Guid.Empty;
                        var schedulingResourceRGBuilder = new System.Text.StringBuilder("");
                        switch (item.cvt_schedulingresourcetype.Value)
                        {
                            case (int)cvt_tsaresourcetype.ResourceGroup:
                                Logger.WriteDebugMessage("Resource Group");
                                outServiceLog += "Reviewed TMP Resource Group: " + item.cvt_name + ".\n";
                                //query RG and find out if it is paired
                                var resourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(rg => rg.Id == item.cvt_tmpresourcegroup.Id);
                                if (resourceGroup != null && resourceGroup.mcs_resourcegroupId != null)
                                {
                                    //Get all Child GRs and look for users and VCs
                                    var childGRs = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == resourceGroup.Id);
                                    foreach (mcs_groupresource gr in childGRs)
                                    {
                                        if (gr.mcs_RelatedUserId != null && gr.mcs_RelatedUserId.Id != Guid.Empty)
                                        {
                                            var childUser = srv.SystemUserSet.FirstOrDefault(su => su.Id == gr.mcs_RelatedUserId.Id);
                                            if (childUser != null)
                                            {
                                                usersListed += childUser.FullName + ";";
                                                Logger.WriteDebugMessage($"Found a User: {childUser.FullName}");
                                            }
                                        }
                                        else if (gr.mcs_RelatedResourceId != null && gr.mcs_RelatedResourceId.Id != Guid.Empty)
                                        {
                                            var childResource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == gr.mcs_RelatedResourceId.Id);
                                            ValidateClinicProperties(Logger, ref outVCs, ref validationLogs, childResource);
                                        }
                                    }

                                    //if Paired, GROUP all Paired as ORs
                                    if (resourceGroup.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                                    {
                                        builderPairedOR.Append(AddResourceToConstraintGroup(resourceGroup.mcs_resourcespecguid));
                                        countSR++;
                                    }
                                    else
                                    {
                                        //if not paired, Add as AND to the service
                                        builderAND.Append(AddResourceToConstraintGroup(resourceGroup.mcs_resourcespecguid));
                                        countSR++;
                                    }
                                    outServiceLog += $"Added TMP Resource Group: {resourceGroup.mcs_name}.\n";
                                }
                                break;
                            case (int)cvt_tsaresourcetype.SingleResource:
                                Logger.WriteDebugMessage("Single Resource");
                                var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == item.cvt_tmpresource.Id);
                                if (resource != null)
                                //Add as AND to the service
                                {
                                    Logger.WriteDebugMessage("Adding Equipment to AND Builder");
                                    outServiceLog += $"Added direct TMP Resource: {resource.mcs_name}.\n";
                                    builderAND.Append(AddResourceToConstraintGroup(resource.mcs_relatedResourceId.Id.ToString("B")));
                                    countSR++;

                                    //if VC - then add to provider VC field
                                    ValidateClinicProperties(Logger, ref outVCs, ref validationLogs, resource);
                                }
                                break;
                            case (int)cvt_tsaresourcetype.SingleProvider:
                            case (int)cvt_tsaresourcetype.SingleTelepresenter:
                                Logger.WriteDebugMessage("Single User");
                                //Add as AND to the service
                                var userRecord = srv.SystemUserSet.FirstOrDefault(su => su.Id == item.cvt_user.Id);
                                if (userRecord != null)
                                {
                                    usersListed += userRecord.FullName + ";";
                                    outServiceLog += $"Added direct User: {userRecord.FullName}.\n";
                                    Logger.WriteDebugMessage($"Found a User: {userRecord.FullName}");
                                    builderAND.Append(AddResourceToConstraintGroup(userRecord.Id.ToString("B")));
                                    countSR++;
                                }
                                break;
                        }
                    }
                    else //It is a Required Field
                    {
                        outServiceLog += item.cvt_name + " missing schedulingresourcetype, was not added.\n";
                        Logger.WriteDebugMessage("Error: Following Resource has no schedulingresourcetype: " + item.cvt_name);
                        //Could query against all three lookups to try to categorize the Scheduling Resource
                    }
                }
                #endregion

                #region Sorted Successfully
                if (countSR == 0)
                {
                    Logger.WriteDebugMessage("Did not add any SRs to the PAT PS, exiting.");
                    outServiceLog += "Did not successfully add any Scheduling Resources from the Patient PS.  Did not include this Patient PS in the service.\n";
                    return string.Empty;
                }

                //Buildout Paired here
                if (builderPairedOR.Length > 0)
                {
                    Logger.WriteDebugMessage("Adding rule of ORs for Paired Resource Groups.");
                    //Nest the Paired RGs as a choose 1 of ORs to join the other resources
                    builderAND.Append(AddResourceToConstraintGroup(BuildOutforSP(thisSP, builderPairedOR, OrganizationService, 1, 0, false, Logger).ToString("B")));
                }
                Logger.WriteDebugMessage("Finished Sorting through Patient Site Resources");

                //Create the Choose All with the nested Paired
                builder.Append(AddResourceToConstraintGroup(BuildOutforSP(thisSP, builderAND, OrganizationService, -1, 0, false, Logger).ToString("B")));
                Logger.WriteDebugMessage("outServiceLog: " + outServiceLog);
                #endregion
                
                #region Update PS
                //Return the Service Rules for this Patient Site if applicable

                outPatActivityParty = patActivityParty;
                outServiceLog += "Added this Patient PS to the Service.\n";        
                Logger.WriteDebugMessage("outServiceLog: " + outServiceLog);
                Logger.WriteDebugMessage("Exiting function. Builder: " + builder.ToString());

                int updateCount = 0;
                cvt_participatingsite updatePS = new cvt_participatingsite()
                {
                    Id = patientPS.Id
                };

                if (patientPS.cvt_patientsitevistaclinics != outVCs)
                {
                    updatePS.cvt_patientsitevistaclinics = outVCs;
                    updateCount++;
                }

                if (thisSP.cvt_groupappointment.Value == true)
                    updatePS.cvt_servicedetails = "Group: The Service should be generated on the Provider Participating Site.";

                if (updateCount > 0)
                    OrganizationService.Update(updatePS);

                return builder.ToString();
                #endregion
            }
        }

        private static void ValidateClinicProperties(MCSLogger Logger, ref string outVCs, ref string validationLogs, mcs_resource clinic)
        {
            if (clinic != null && clinic.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic)
            {
                Logger.WriteDebugMessage($"Found a VC: {clinic.mcs_name}");

                if (clinic.statecode.Value != (int)mcs_resourceState.Active)
                {
                    validationLogs += $"The status of associated clinic with name {clinic.mcs_name} is not active.\n";
                    Logger.WriteToFile($"The status of associated clinic with name {clinic.mcs_name} is not active.");

                }
                else
                if (string.IsNullOrEmpty(clinic.cvt_ien) || !int.TryParse(clinic.cvt_ien, out var ien))
                {
                    validationLogs += $"Associated clinic with Name:{clinic.mcs_name}, IEN: {clinic.cvt_ien} do not have the valid IEN.\n";
                    Logger.WriteToFile($"Associated clinic with Name:{clinic.mcs_name}, IEN: {clinic.cvt_ien} do not have the valid IEN.");
                }
                else
                {
                    outVCs += clinic.mcs_name + ";";
                }
            }
        }

        public static string buildResourceGroup(Guid ResourceGroupId, Guid buId, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");
            var strReturn = "";
            var builder = new System.Text.StringBuilder("");
            using (var srv = new Xrm(OrganizationService))
            {
                //Get all Group Resources
                var groupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == ResourceGroupId && gr.statuscode.Value == (int)mcs_groupresource_statuscode.Active);

                foreach (mcs_groupresource linker in groupResources)
                {
                    if (linker.mcs_RelatedResourceId != null)
                    {
                        //Get the Resource
                        var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == linker.mcs_RelatedResourceId.Id);
                        if (resource != null)
                        {
                            builder.Append(AddResourceToConstraintGroup(resource.mcs_relatedResourceId.Id.ToString()));
                        }
                    }
                    else if (linker.mcs_RelatedUserId != null)
                    {
                        builder.Append(AddResourceToConstraintGroup(linker.mcs_RelatedUserId.Id.ToString()));
                    }
                }
                if (builder.ToString() != string.Empty)
                {
                    //Create Resource Spec
                    Guid specId = CreateSpecId(buId, builder, OrganizationService, -1, 0, false, Logger);

                    //Add Resource Spec to SR
                    //Do we need to?  We won't ever reuse it, unless we can unravel it to check it

                    //Add SpecId to OR Builder
                    strReturn = specId.ToString();
                }              
            }
            return strReturn;
        }
        public static string ValidateProviderPS(cvt_resourcepackage thisSP, cvt_participatingsite providerPS, MCSLogger Logger, IOrganizationService OrganizationService, out string outProviders, out string outProviderSite, out string outServiceLog, out string outVCs, out string validationLogs)
        {
            {/*
            outServiceLog Clinic Individual example:
            Evaluating Provider Site: {name}, Patient Site: {name}.
            -Provider/Patient relationship is {intrafacility/interfacility}. Facility Approval: {Failed/Passed/Not Needed}.
            -SFT: {True/False}.
            -Found 0 Provider Scheduling Resources. No Resources found, only valid if SFT. Is SFT, {found/created} {SFT Technology @ Site}. Adding it to Provider side of Service. Finished evaluation.
            -Found 0 Provider Scheduling Resources. No Resources found, only valid if SFT. Is not SFT, Failed resource requirement check. Finished evaluation.
            -Found {n} Provider Scheduling Resources. Evaluating Scheduling Resources.

            --Due to VC Automation, skipped manually added TMP Resource VC: {0}.\n
            --Scheduling Resource had no schedulingresourcetype, was skipped: {name}

            outServiceLog VVC example:
            Evaluating Provider Site: {name}, No Patient Site: VVC.
            -Found 0 Provider Scheduling Resources. No Resources found, only valid if SFT. Is not SFT, Failed resource requirement check. Finished evaluation. 
            -Found {n} Provider Scheduling Resources. Evaluating Scheduling Resources.

             */
            }
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                #region Declaration of Variables
                outProviders = "";
                outProviderSite = "";
                outVCs = "";
                validationLogs = string.Empty;
                outServiceLog = $"\nReviewed Provider Site: {providerPS.cvt_site.Name}. ";

                bool isSFT = false;
                var builder = new System.Text.StringBuilder("");
                var builderAND = new System.Text.StringBuilder("");
                var builderPairedOR = new System.Text.StringBuilder("");
                int countSR = 0;
                cvt_participatingsite updateEmptyPS = new cvt_participatingsite()
                {
                    Id = providerPS.Id,
                    cvt_servicedetails = "The Service should be generated on the Patient Participating Site."
                };

                //SFT
                if (thisSP.cvt_availabletelehealthmodality != null && thisSP.cvt_availabletelehealthmodality.Value == (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward)
                {
                    isSFT = true;
                    //Fix for SFT to not book any resources
                    //outServiceLog += "No Provider Resources will be booked for SFT.\n";
                    //outProviderSite = providerPS.cvt_site.Name + ";";

                    //OrganizationService.Update(updateEmptyPS);
                    //return string.Empty;
                }
                #endregion

                #region Retrieve Provider PS Resources
                //Retrieving the resources listed on the Provider PS.
                var proSR = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_participatingsite.Id == providerPS.Id && sr.statuscode.Value == (int)cvt_schedulingresource_statuscode.Active);
                int proSRCount = (proSR != null ? proSR.ToList().Count : 0);

                outServiceLog += $"Found {proSRCount.ToString()} Provider Resources.\n";
                Logger.WriteDebugMessage($"Found {proSRCount.ToString()} Provider Resources. ");

                //If 0, then verify SFT or throw error
                if (proSRCount == 0)
                {
                    //Could be SFT Provider PS, in that case this is valid
                    if (isSFT)
                    {
                        outServiceLog += "No Provider Resources required for SFT.\n";
                        outProviderSite = providerPS.cvt_site.Name + ";";

                        OrganizationService.Update(updateEmptyPS);
                        return string.Empty;
                    }
                    else
                    {
                        //Return nothing to be added to the scheduling rules
                        outServiceLog += "No Resources found: Failed.\n";

                        if (thisSP.cvt_groupappointment.Value == false)
                            OrganizationService.Update(updateEmptyPS);
                        return string.Empty;
                    }
                }
                #endregion

                #region Loop through all of the Provider Site resources and add them to the builder
                //Resource/Equipment/User is listed as resources within the Constraints attribute of a Constraint Based Group
                //The Constraint Based Group is listed within the Resource Spec as the GroupObjectId
                //The Resource Spec is listed on the Service
                //Resource Spec contains required count 1/all

                Logger.WriteDebugMessage("Evaluating Provider Site Resources");
                //Build the service rules for this Provider PS
                foreach (var item in proSR)
                {
                    Logger.WriteDebugMessage("Starting loop for " + item.cvt_name);
                    if (item.cvt_schedulingresourcetype != null)
                    {
                        Guid sysResId = Guid.Empty;
                        var schedulingResourceRGBuilder = new System.Text.StringBuilder("");
                        switch (item.cvt_schedulingresourcetype.Value)
                        {
                            case (int)cvt_tsaresourcetype.ResourceGroup:
                                Logger.WriteDebugMessage("Resource Group");
                                outServiceLog += "Reviewed TMP Resource Group: " + item.cvt_name + ".\n";
                                //query RG and find out if it is paired
                                var resourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(rg => rg.Id == item.cvt_tmpresourcegroup.Id);
                                if (resourceGroup != null && resourceGroup.mcs_resourcegroupId != null)
                                {
                                    //Get all Child GRs and look for users and VCs
                                    var childGRs = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == resourceGroup.Id);
                                    foreach (mcs_groupresource gr in childGRs)
                                    {
                                        if (gr.mcs_RelatedUserId != null && gr.mcs_RelatedUserId.Id != Guid.Empty)
                                        {
                                            var childUser = srv.SystemUserSet.FirstOrDefault(su => su.Id == gr.mcs_RelatedUserId.Id);
                                            if (childUser != null)
                                            {
                                                outProviders += childUser.FullName + ";";
                                                Logger.WriteDebugMessage($"Found a User: {childUser.FullName}");
                                            }
                                        }
                                        else if (gr.mcs_RelatedResourceId != null && gr.mcs_RelatedResourceId.Id != Guid.Empty)
                                        {
                                            var childResource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == gr.mcs_RelatedResourceId.Id);
                                            ValidateClinicProperties(Logger, ref outVCs, ref validationLogs, childResource);
                                        }
                                    }

                                    //if Paired, GROUP all Paired as ORs
                                    if (resourceGroup.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                                    {
                                        //builderPairedOR.Append(AddResourceToConstraintGroup(resourceGroup.mcs_resourcespecguid));
                                        //Need to regen the resource spec so it is unique
                                        builderPairedOR.Append(AddResourceToConstraintGroup(buildResourceGroup(resourceGroup.Id, thisSP.OwningBusinessUnit.Id, Logger, OrganizationService)));
                                        countSR++;
                                    }
                                    else
                                    {
                                        //if not paired, Add as AND to the service
                                        builderAND.Append(AddResourceToConstraintGroup(resourceGroup.mcs_resourcespecguid));
                                        countSR++;
                                    }
                                    outServiceLog += $"Added TMP Resource Group: {resourceGroup.mcs_name}.\n";
                                }
                                break;
                            case (int)cvt_tsaresourcetype.SingleResource:
                                Logger.WriteDebugMessage("Single Resource");
                                var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == item.cvt_tmpresource.Id);
                                if (resource != null)
                                //Add as AND to the service
                                {
                                    Logger.WriteDebugMessage("Adding Equipment to AND Builder");
                                    outServiceLog += $"Added direct TMP Resource: {resource.mcs_name}.\n";
                                    builderAND.Append(AddResourceToConstraintGroup(resource.mcs_relatedResourceId.Id.ToString("B")));
                                    countSR++;

                                    //if VC - then add to provider VC field
                                    ValidateClinicProperties(Logger, ref outVCs, ref validationLogs, resource);
                                }
                                break;
                            case (int)cvt_tsaresourcetype.SingleProvider:
                            case (int)cvt_tsaresourcetype.SingleTelepresenter:
                                Logger.WriteDebugMessage("Single Provider");
                                //Add as AND to the service
                                var userRecord = srv.SystemUserSet.FirstOrDefault(su => su.Id == item.cvt_user.Id);
                                if (userRecord != null)
                                {
                                    outProviders += userRecord.FullName + ";";
                                    outServiceLog += $"Added direct Provider: {userRecord.FullName}.\n";
                                    Logger.WriteDebugMessage($"Found a User: {userRecord.FullName}");
                                    builderAND.Append(AddResourceToConstraintGroup(userRecord.Id.ToString("B")));
                                    countSR++;
                                }
                                break;
                        }
                    }
                    else //It is a Required Field
                    {
                        outServiceLog += item.cvt_name + " missing schedulingresourcetype, was not added.\n";
                        Logger.WriteDebugMessage("Error: Following Resource has no schedulingresourcetype: " + item.cvt_name);
                        //Could query against all three lookups to try to categorize the Scheduling Resource
                    }
                }
                #endregion

                #region Sorted Successfully
                if (countSR == 0)
                {
                    Logger.WriteDebugMessage("Did not add any SRs to the PRO PS, exiting.");
                    outServiceLog += "Did not successfully add any Scheduling Resources to the Provider PS.\n";
                    return string.Empty;
                }

                //Buildout Prov Only Paired here
                if (builderPairedOR.Length > 0)
                {
                    Logger.WriteDebugMessage("Adding rule of ORs for Paired Resource Groups.");
                    //Nest the Paired RGs as a choose 1 of ORs to join the other provider resources
                    builderAND.Append(AddResourceToConstraintGroup(BuildOutforSP(thisSP, builderPairedOR, OrganizationService, 1, 0, false, Logger).ToString("B")));
                }
                Logger.WriteDebugMessage("Finished Sorting through Provider Site Resources");

                //Create the Choose All with the nested Paired
                builder.Append(AddResourceToConstraintGroup(BuildOutforSP(thisSP, builderAND, OrganizationService, -1, 0, false, Logger).ToString("B")));

                //Return the Service Rules for this Provider Site
                outProviderSite = providerPS.cvt_site.Name + ";";

                outServiceLog += "Added this Provider PS to the Service. \n";

                if (isSFT)
                {
                    outServiceLog = "No Provider Resources will be booked for SFT.\n";
                }

                Logger.WriteDebugMessage("outServiceLog: " + outServiceLog);
                #endregion

                #region Update Pro PS
                int updateCount = 0;
                cvt_participatingsite updatePS = new cvt_participatingsite()
                {
                    Id = providerPS.Id
                };

                if (providerPS.cvt_providers != outProviders)
                {
                    updatePS.cvt_providers = outProviders;
                    updateCount++;
                }
                if (providerPS.cvt_providersitevistaclinics != outVCs)
                {
                    updatePS.cvt_providersitevistaclinics = outVCs;
                    updateCount++;
                }
                if (thisSP.cvt_patientlocationtype.Value == (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone)
                    updatePS.cvt_servicedetails = "VVC: The Service should be generated on the Scheduling Package.";
                else if (thisSP.cvt_groupappointment.Value == false)
                    updatePS.cvt_servicedetails = "Non-Group: The Service should be generated on the Patient Participating Site.";

                if (updateCount > 0)
                    OrganizationService.Update(updatePS);

                if (isSFT)
                    return string.Empty;
                else
                    return builder.ToString();
                #endregion
            }
        }
        #endregion

        #region Building the Service
        //Can pass in either patient or provider Participating Site? Yes
        //Overload: ServiceBuild for Participating Site - this is for Clinic based Individual or Group - non VVC
        public static void ServiceBuild(cvt_participatingsite psVar, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");

            using (var srv = new Xrm(OrganizationService))
            {
                List<EntityReference> buildingServiceFromPS = new List<EntityReference>();
                #region Record and Field validation
                var ps = (cvt_participatingsite)psVar;

                //Check for scheduleable
                if (ps.cvt_scheduleable == null || ps.cvt_scheduleable.Value == false)
                {
                    Logger.WriteDebugMessage("This Participating Site 'Can Be Scheduled' = No. Skipping build process.");
                    UpdatePS(psVar, "This Participating Site 'Can Be Scheduled' = No. Skipping build process.", Logger, OrganizationService);
                    return;
                }

                //Check for parent Scheduling package
                var schPackage = srv.cvt_resourcepackageSet.FirstOrDefault(sp => sp.Id == ps.cvt_resourcepackage.Id);
                if (schPackage == null)
                {
                    Logger.WriteDebugMessage("SP could not be found, skipping build process.");
                    UpdatePS(psVar, "SP could not be found, skipping build process.", Logger, OrganizationService);
                    return;
                }
                Logger.WriteDebugMessage("Finished Record and Field Validation section.");
                #endregion

                //Defaulted to Patient
                int oppLocationType = (int)cvt_participatingsitecvt_locationtype.Patient; 
                
                if (ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient)
                {
                    Logger.WriteDebugMessage("Participating Site is Patient. Gather interfacility Provider PS where the FA is approved and/or all of the intrafacility Provider PS.");
                    oppLocationType = (int)cvt_participatingsitecvt_locationtype.Provider;
                }
                else //For PROVIDER PS, so must be Group. Get List of Opposing Patient PS
                    Logger.WriteDebugMessage("Participating Site is Provider. Gather interfacility Patient PS where the FA is approved and/or all of the intrafacility Patient PS.");

                Logger.WriteDebugMessage("Querying for the related PS");
                List<cvt_participatingsite> relatedPS = new List<cvt_participatingsite>();
                var relatedInterPS = srv.cvt_participatingsiteSet.Where(oppPS =>
                                         oppPS.cvt_facility.Id != ps.cvt_facility.Id 
                                         && oppPS.cvt_locationtype.Value == oppLocationType 
                                         && oppPS.cvt_resourcepackage.Id == ps.cvt_resourcepackage.Id 
                                         && oppPS.cvt_scheduleable.Value == true 
                                         && oppPS.statecode.Value == (int)cvt_participatingsiteState.Active).ToList();
                Logger.WriteDebugMessage("Finished querying for the opposing Interfacility PS. Count: " + relatedInterPS.Count);

                foreach (cvt_participatingsite interPS in relatedInterPS)
                {
                    Guid provFacilityId = Guid.Empty;
                    Guid patFacilityId = Guid.Empty;

                    switch (ps.cvt_locationtype.Value)
                    {
                        case (int)cvt_participatingsitecvt_locationtype.Provider:
                            provFacilityId = ps.cvt_facility.Id;
                            patFacilityId = interPS.cvt_facility.Id;
                            break;
                        case (int)cvt_participatingsitecvt_locationtype.Patient:
                            provFacilityId = interPS.cvt_facility.Id;
                            patFacilityId = ps.cvt_facility.Id;
                            break;
                    }
                    //Query for FA
                    //var fa = srv.cvt_facilityapprovalSet.FirstOrDefault(thisFA => 
                    //            thisFA.cvt_providerfacility.Id == provFacilityId && 
                    //            thisFA.cvt_patientfacility.Id == patFacilityId && 
                    //            thisFA.statecode.Value == (int)cvt_facilityapprovalState.Active && 
                    //            thisFA.statuscode.Value == (int)cvt_facilityapproval_statuscode.Approved && 
                    //            thisFA.cvt_resourcepackage.Id == ps.cvt_resourcepackage.Id);

                    //Logger.WriteDebugMessage("FA statuscode =" + fa?.statuscode?.Value + ". Approved=917290000; Pending=1; Denied = 917290001");
                    //if (fa != null && fa?.statuscode?.Value == (int)cvt_facilityapproval_statuscode.Approved)
                    //{
                    //Automatically adding Inter PS now, Scheduling is not dependent on FA
                    relatedPS.Add(interPS);
                    //Logger.WriteDebugMessage("Passed FA check, adding interfacility PS to list: " + interPS.cvt_name);
                    //}
                    //else
                    //    Logger.WriteDebugMessage("Failed FA check, skipping interfacility PS: " + interPS.cvt_name);
                }

                var relatedIntraPS = srv.cvt_participatingsiteSet.Where(oppPS => 
                                        oppPS.cvt_facility.Id == ps.cvt_facility.Id 
                                        && oppPS.cvt_locationtype.Value == oppLocationType 
                                        && oppPS.cvt_resourcepackage.Id == ps.cvt_resourcepackage.Id 
                                        && oppPS.cvt_scheduleable.Value == true 
                                        && oppPS.statecode.Value == (int)cvt_participatingsiteState.Active).ToList();
                Logger.WriteDebugMessage("Finished querying for the opposing Intrafacility PS. Count: " + relatedIntraPS.Count);
                relatedPS.AddRange(relatedIntraPS);
                Logger.WriteDebugMessage("Total PS Count: " + relatedPS.Count);
                if (relatedPS == null || relatedPS.Count == 0)
                {
                    Logger.WriteDebugMessage("No opposite PS in a state to build the service, exiting service build.");
                    UpdatePS(psVar, "No opposite PS in a state to build the service, exiting service build.", Logger, OrganizationService);
                    return;
                }
                switch (ps.cvt_locationtype.Value)
                {
                    case (int)cvt_participatingsitecvt_locationtype.Provider:
                        //Loop through List, passing in Provider PS and List of Patient PS
                        CreateUpdateGroupService(schPackage, ps, relatedPS, Logger, OrganizationService);
                        break;
                    case (int)cvt_participatingsitecvt_locationtype.Patient:
                        //Loop through List, passing in Patient PS and List of Provider PS
                        CreateUpdateIndividualService(schPackage, relatedPS, ps, Logger, OrganizationService);
                        break;
                }
            }
        }

        //Overload: ServiceBuild for SchedulingPackage - this is for VVC
        public static void ServiceBuild(cvt_resourcepackage schedulingPackage, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                List<EntityReference> buildingServiceFromSP = new List<EntityReference>();

                var proSites = srv.cvt_participatingsiteSet.Where(p => p.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider
                                && p.cvt_resourcepackage.Id == schedulingPackage.Id
                                && p.cvt_scheduleable.Value == true
                                && p.statecode.Value == (int)cvt_participatingsiteState.Active).ToList();
                Logger.WriteDebugMessage("Finished querying for the PS. Count: " + proSites.Count);

                if (proSites == null || proSites.Count == 0)
                {
                    Logger.WriteDebugMessage("No child PS in a state to build the service, exiting service build.");
                    UpdateSP(schedulingPackage, Guid.Empty, "VVC Scheduling Package: No Provider Participating Sites in a state to build the service, could not generate the service.", Logger, OrganizationService);
                    return;
                }

                Logger.WriteDebugMessage("Building VVC Service");
                //Declaration of variables
                Guid resourceSpecId = Guid.Empty;
                string providers = "";
                string providerSites = "";
                string providerVCs = "";
                string serviceLog = "VA Video Connect Service:";
                int initialstatus = (int)service_initialstatuscode.Pending;
                var duration = (schedulingPackage.cvt_AppointmentLength != null) ? schedulingPackage.cvt_AppointmentLength.Value : 60;
                var startEvery = (schedulingPackage.cvt_StartAppointmentsEvery != null) ? schedulingPackage.cvt_StartAppointmentsEvery.ToString() : "60";

                try
                {
                    Logger.WriteDebugMessage("# pro sites: " + proSites.Count);
                    //loop through the Participating Provider Sites. 
                    var builderProviderSites = new System.Text.StringBuilder("");
                    foreach (cvt_participatingsite proPS in proSites)
                    {
                        var singleProviderSiteCBG = ValidateProviderPS(schedulingPackage, proPS, Logger, OrganizationService, out string validProviders, out string outValidSites, out string outLogOutput, out string outProVCs, out string outProValidationErrors);

                        if (!string.IsNullOrEmpty(outProValidationErrors))
                        {
                            Logger.WriteToFile($"{outProValidationErrors}\nParticipating Sites must have active VistA Clinics with valid IEN. No service generated.");
                            UpdateSP(schedulingPackage, Guid.Empty, $" {serviceLog} \n{outProValidationErrors}", Logger, OrganizationService);
                            return;
                        }

                        providers += validProviders;
                        providerSites += outValidSites;
                        providerVCs += outProVCs;
                        serviceLog += outLogOutput;
                        builderProviderSites.Append(singleProviderSiteCBG);
                    }

                    //Validation
                    Logger.WriteDebugMessage("providerSites: " + providerSites);
                    Logger.WriteDebugMessage("providers: " + providers);
                    Logger.WriteDebugMessage("serviceLog: " + serviceLog);
                    Logger.WriteDebugMessage("builderProviderSites.Length:" + builderProviderSites.Length);
                    if (builderProviderSites.Length == 0)
                    {
                        serviceLog += "\nNo Valid Provider Participating Sites assessed, could not generate the service.\n";
                        UpdateSP(schedulingPackage, Guid.Empty, serviceLog, Logger, OrganizationService);
                    }

                    if (builderProviderSites.Length > 0)
                        resourceSpecId = BuildOutforSP(schedulingPackage, builderProviderSites, OrganizationService, 1, 2, false, Logger);

                    Logger.WriteDebugMessage("Constructing the service");

                    Service newService = new Service
                    {
                        Name = schedulingPackage.cvt_name.ToString(),
                        AnchorOffset = 480,
                        Duration = duration,
                        InitialStatusCode = new OptionSetValue(initialstatus),
                        Granularity = "FREQ=MINUTELY;INTERVAL=" + startEvery + ";",
                        ResourceSpecId = new EntityReference(ResourceSpec.EntityLogicalName, resourceSpecId)
                    };

                    var newServiceId = OrganizationService.Create(newService);
                    Logger.WriteDebugMessage("Created new Service. Updating the Scheduling Package.");
                    serviceLog += "\nConstructed the service.";
                    UpdateSP(schedulingPackage, newServiceId, serviceLog, Logger, OrganizationService);

                    #region Catch
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("custom"))
                    {
                        Logger.WriteDebugMessage(ex.Message.Substring(6));

                        throw new InvalidPluginExecutionException(ex.Message.Substring(6));
                    }
                    else
                    {
                        Logger.setMethod = "Execute";
                        Logger.WriteToFile(ex.Message);
                        throw new InvalidPluginExecutionException(ex.Message);
                    }
                }
                #endregion
            }
        }

        public static void CreateUpdateIndividualService(cvt_resourcepackage thisSP, List<cvt_participatingsite> provSites, cvt_participatingsite patientPS, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");

            Guid resourceSpecId = Guid.Empty; 
            string providers = "";
            string providerSites = "";
            string providerVCs = "";
            string serviceLog = "Clinic:Clinic Service:";

            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    if (provSites == null)
                    {
                        Logger.WriteDebugMessage("No Provider Sites, exiting.");
                        UpdatePS(patientPS, "No Provider Sites, no service generated.", Logger, OrganizationService);
                        return;
                    }

                    if (patientPS.cvt_scheduleable == false)
                    {
                        Logger.WriteDebugMessage("This Patient PS is not scheduleable, no service generated.");
                        UpdatePS(patientPS, "This Patient PS is not scheduleable, no service generated.", Logger, OrganizationService);
                        return;
                    }
                    #region PROV SITE
                    Logger.WriteDebugMessage("Start Sorting through Provider Side");
                    var builderProviderSites = new System.Text.StringBuilder("");

                    foreach (var prov in provSites)
                    {
                        var singleProviderSiteCBG = ValidateProviderPS(thisSP, prov, Logger, OrganizationService, out string validProviders, out string outValidSites, out string outLogOutput, out string outProVCs, out string outProVCValidationErrors);
                        if (!string.IsNullOrEmpty(outProVCValidationErrors))
                        {
                            Logger.WriteToFile($"{outProVCValidationErrors}\nProvider Participating Sites must have active VistA Clinics with valid IEN. No service generated.");
                            UpdatePS(patientPS, $"{outProVCValidationErrors}Provider Participating Site must have active VistA Clinics with valid IEN. No service generated.", Logger, OrganizationService);
                            return;
                        }
                        providers += validProviders;
                        providerSites += outValidSites;
                        providerVCs += outProVCs;
                        serviceLog += outLogOutput;
                        builderProviderSites.Append(singleProviderSiteCBG);
                    }
                    Logger.WriteDebugMessage("Finished Sorting through Provider Side");
                    #endregion

                    #region PATIENT SITE
                    //After collecting all of the patient data, if builder is not empty, then add all patient
                    var builderPat = new System.Text.StringBuilder("");
                    Logger.WriteDebugMessage("Start Sorting through Patient Side");

                    var singlePatientSiteCBG = ValidatePatientPS(thisSP, patientPS, Logger, OrganizationService, out string logOutput, out string groupPatVariable, out string patVCs, out string validationErrors);
                    if (string.IsNullOrEmpty(patVCs) || !string.IsNullOrEmpty(validationErrors))
                    {
                        Logger.WriteDebugMessage($"{validationErrors}\nPatient Participating Site must have active VistA Clinics with valid IEN. No service generated.");
                        UpdatePS(patientPS, $"{validationErrors} Patient Participating Site must have active VistA Clinics with valid IEN. No service generated.", Logger, OrganizationService);
                        return;
                    }
                    serviceLog += logOutput;
                    builderPat.Append(singlePatientSiteCBG);

                    Logger.WriteDebugMessage("Finished Sorting through Patient Side");
                    #endregion

                    #region Build the Service Components
                    Logger.WriteDebugMessage("Starting Build the Service Components region");
                    //Validation
                    Logger.WriteDebugMessage("providerSites: " + providerSites);
                    Logger.WriteDebugMessage("providers: " + providers);
                    Logger.WriteDebugMessage("serviceLog: " + serviceLog);
                    Logger.WriteDebugMessage("builderProviderSites.Length:" + builderProviderSites.Length);
                    if (thisSP.cvt_availabletelehealthmodality.Value == (int)cvt_resourcepackagecvt_availabletelehealthmodality.ClinicalVideoTelehealth && builderProviderSites.Length == 0)
                    {
                        serviceLog += "\nNo Valid Provider Participating Sites assessed, no service constructed.\n";
                        UpdatePS(patientPS, serviceLog, Logger, OrganizationService);
                        return;
                    }
 
                    ////SFT - have to have Patient VC?
                    //if (builderPat.Length == 0 && thisSP.cvt_availabletelehealthmodality.Value == (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward)
                    //{
                    //    serviceLog += "\nNo Valid Patient Participating Site assessed, no service constructed.\n";
                    //    UpdatePS(patientPS, string.Empty, string.Empty, serviceLog, Logger, OrganizationService, srv, Guid.Empty, string.Empty, string.Empty);
                    //    return;
                    //}

                    if (builderPat.Length == 0 && builderProviderSites.Length == 0)
                    {
                        serviceLog += "\nNo Valid Provider or Patient Participating Site assessed, no service constructed.\n";
                        UpdatePS(patientPS, serviceLog, Logger, OrganizationService);
                        return;
                    }
                    var builder = new System.Text.StringBuilder("");
                    //If Ind C:C, then All Providers to 1 Patient
                    if (builderProviderSites.Length > 0)
                    {
                        builder.Append(AddResourceToConstraintGroup(BuildOutforSP(thisSP, builderProviderSites, OrganizationService, 1, 0, true, Logger).ToString("B")));
                        Logger.WriteDebugMessage("Adding Provider Builder to Main Builder.");
                    }

                    if (builderPat.Length > 0)
                    {
                        builder.Append(builderPat);
                        Logger.WriteDebugMessage("Adding Patient Builder to Main Builder.");
                    }
                    #endregion

                    #region Logic - Constructing the service
                    Logger.WriteDebugMessage("Constructing the service");
                    resourceSpecId = BuildOutforSP(thisSP, builder, OrganizationService, -1, 2, false, Logger);
                    
                    int initialstatus = (int)service_initialstatuscode.Pending;
                    var duration = (thisSP.cvt_AppointmentLength != null) ? thisSP.cvt_AppointmentLength.Value : 60;
                    var startEvery = (thisSP.cvt_StartAppointmentsEvery != null) ? thisSP.cvt_StartAppointmentsEvery.ToString() : "60";

                    Service newService = new Service
                    {
                        Name = patientPS.cvt_name.ToString(),
                        AnchorOffset = 480,
                        Duration = duration,
                        InitialStatusCode = new OptionSetValue(initialstatus),
                        Granularity = "FREQ=MINUTELY;INTERVAL=" + startEvery + ";",
                        ResourceSpecId = new EntityReference(ResourceSpec.EntityLogicalName, resourceSpecId)
                    };

                    //create the service
                    Logger.WriteDebugMessage("Creating new Service");
                    var newServiceId = OrganizationService.Create(newService);
                    serviceLog += "\nConstructed the service.\n";
                    Logger.WriteDebugMessage("Updating the Participating Site.");
                    UpdatePS(patientPS, providerSites, providers, providerVCs, patVCs, serviceLog,  Logger, OrganizationService, newServiceId, string.Empty);
                    #endregion

                    #region Catch
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("custom"))
                    {
                        Logger.WriteDebugMessage(ex.Message.Substring(6));

                        throw new InvalidPluginExecutionException(ex.Message.Substring(6));
                    }
                    else
                    {
                        Logger.setMethod = "Execute";
                        Logger.WriteToFile(ex.Message);
                        throw new InvalidPluginExecutionException(ex.Message);
                    }
                }
                #endregion
            }
        }

        public static void CreateUpdateGroupService(cvt_resourcepackage thisSP, cvt_participatingsite providerPS, List<cvt_participatingsite> patSites, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("Starting");
            var builder = new System.Text.StringBuilder("");
            var builderProvider = new System.Text.StringBuilder("");
            var builderProvider2 = new System.Text.StringBuilder("");
            var builderPatient = new System.Text.StringBuilder("");
            var builderBoth = new System.Text.StringBuilder("");
            //ALL has two branches, one of just PROVIDER, and one of BOT provider and patient
            Guid resourceSpecId = Guid.Empty;

            string patientSites = "";
            string providers = "";
            string serviceLog = "Group Service:";

            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    if (patSites == null)
                    {
                        Logger.WriteDebugMessage("No Patient Sites, exiting.");
                        UpdatePS(providerPS, "No Patient Sites, no service generated.", Logger, OrganizationService);
                        return;
                    }

                    if (providerPS.cvt_scheduleable == false)
                    {
                        Logger.WriteDebugMessage("This Patient PS is not scheduleable, no service generated.");
                        UpdatePS(providerPS, "This Provider PS is not scheduleable, no service generated.", Logger, OrganizationService);
                        return;
                    }

                    #region PROV SITE RESOURCES
                    Logger.WriteDebugMessage("Starting PROV SITE RESOURCES region.");
                    var singleProviderSiteCBG = ValidateProviderPS(thisSP, providerPS, Logger, OrganizationService, out string validProviders, out string outValidSites, out string outLogOutput, out string outProVCs, out string proValidationErrors);
                    var anotherProviderSiteCBG = ValidateProviderPS(thisSP, providerPS, Logger, OrganizationService, out string xvalidProviders, out string xoutValidSites, out string xoutLogOutput, out string xoutProVCs, out string proPsValidationErrors);
                    if (!string.IsNullOrEmpty(proValidationErrors))
                    {
                        Logger.WriteToFile($"{proValidationErrors}\nProvider Participating Site must have active VistA Clinics with valid IEN. No service generated.");
                        UpdatePS(providerPS, $"{proValidationErrors} Provider Participating Site must have active VistA Clinics with valid IEN. No service generated.", Logger, OrganizationService);
                        return;
                    }

                    Logger.WriteDebugMessage("Group. Provider Site CBG: " + singleProviderSiteCBG);
                    serviceLog += outLogOutput;

                    if (singleProviderSiteCBG != String.Empty)
                    {
                        //patientSites += outValidSites;
                        providers += validProviders;
                        builderProvider.Append(singleProviderSiteCBG);
                        builderProvider2.Append(anotherProviderSiteCBG);
                        Logger.WriteDebugMessage("Added CBG");
                    }
                    else
                        Logger.WriteDebugMessage("ValidateProviderPS for Provider PS" + providerPS.cvt_name + " returned no valid CBG.");


                    #endregion

                    #region PATIENT SITE RESOURCES
                    string groupPatVariable = string.Empty;
                    string patVCs = string.Empty;
                    foreach (var patientPSite in patSites)
                    {
                        //Retrieve all the patient site resources
                        Logger.WriteDebugMessage("Start Sorting through Patient Site Resources for " + patientPSite.cvt_name);
                        var singlePatientSiteCBG = ValidatePatientPS(thisSP, patientPSite, Logger, OrganizationService, out string logOutput, out string outGroupPatVariable, out patVCs, out string validationErrors);
                        if (string.IsNullOrEmpty(patVCs) || !string.IsNullOrEmpty(validationErrors))
                        {
                            Logger.WriteDebugMessage($"{validationErrors}\nPatient Participating Site must have active VistA Clinics with valid IEN. No service generated.");
                            UpdatePS(providerPS, $"{validationErrors} Patient Participating Site must have active VistA Clinics with valid IEN. No service generated.", Logger, OrganizationService);
                            return;
                        }
                        Logger.WriteDebugMessage("outGroupPatVariable: " + outGroupPatVariable);
                        serviceLog += logOutput;
                        if (singlePatientSiteCBG != string.Empty)
                        {
                            Logger.WriteDebugMessage("Valid Patient Site: " + patientPSite.cvt_name + ". Adding to Patient Builder.");
                            patientSites += patientPSite.cvt_site.Name + ";";
                            builderPatient.Append(singlePatientSiteCBG);

                            if (groupPatVariable == string.Empty && outGroupPatVariable != string.Empty)
                            {
                                Logger.WriteDebugMessage("Adding the Patient Branch Only variable. Id: " + groupPatVariable);
                                groupPatVariable = outGroupPatVariable;
                            }
                        }
                        else
                            Logger.WriteDebugMessage("Was not a Valid Patient Site: " + patientPSite.cvt_name);

                    }
                        Logger.WriteDebugMessage("Finished Sorting through Patient Site Resources");
                    #endregion

                    #region Build the Service Components
                    Logger.WriteDebugMessage("Starting Build the Service Components region");
                    //Validation
                    Guid providerSpec = Guid.Empty;

                    if (builderProvider.Length == 0)
                    {
                        serviceLog += "\nNo Valid Provider Participating Sites assessed, no service generated.\n";
                        UpdatePS(providerPS, serviceLog, Logger, OrganizationService);
                        Logger.WriteDebugMessage("Scheduling Resources must be listed on the Provider Side in order to build this service.");
                        return;
                    }
                    else if (builderProvider.Length > 0)
                    {
                        providerSpec = BuildOutforSP(thisSP, builderProvider2, OrganizationService, 1, 0, true, Logger);
                        builder.Append(AddResourceToConstraintGroup(providerSpec.ToString("B")));
                        providerSpec = BuildOutforSP(thisSP, builderProvider, OrganizationService, 1, 0, true, Logger);
                        builderBoth.Append(AddResourceToConstraintGroup(providerSpec.ToString("B")));
                        Logger.WriteDebugMessage("Adding Provider Builder to Main Builder.");
                    }

                    if (builderPatient.Length > 0)
                    {
                        builderBoth.Append(builderPatient);
                        Logger.WriteDebugMessage("Adding Patient Builder to Both Builder.");
                        resourceSpecId = BuildOutforSP(thisSP, builderBoth, OrganizationService, -1, 0, false, Logger);
                        builder.Append(AddResourceToConstraintGroup(resourceSpecId.ToString("B")));
                        Logger.WriteDebugMessage("Adding Both Builder to Main Builder.");
                    }
                    #endregion

                    #region Logic - Constructing the service
                    Logger.WriteDebugMessage("Constructing the group service.");
                    resourceSpecId = BuildOutforSP(thisSP, builder, OrganizationService, 1, 2, false, Logger);

                    int initialstatus = (int)service_initialstatuscode.Pending;
                    var duration = (thisSP.cvt_AppointmentLength != null) ? thisSP.cvt_AppointmentLength.Value : 60;
                    var startEvery = (thisSP.cvt_StartAppointmentsEvery != null) ? thisSP.cvt_StartAppointmentsEvery.ToString() : "60";

                    Service newService = new Service
                    {
                        Name = providerPS.cvt_name.ToString(),
                        AnchorOffset = 480,
                        Duration = duration,
                        InitialStatusCode = new OptionSetValue(initialstatus),
                        Granularity = "FREQ=MINUTELY;INTERVAL=" + startEvery + ";",
                        ResourceSpecId = new EntityReference(ResourceSpec.EntityLogicalName, resourceSpecId)
                    };

                    //create the service
                    Logger.WriteDebugMessage("Creating new Service");
                    var newServiceId = OrganizationService.Create(newService);
                    serviceLog += "\nConstructed the service.\n";
                    Logger.WriteDebugMessage("Updating the Provider Participating Site.");
                    UpdatePS(providerPS, patientSites, providers, outProVCs, patVCs, serviceLog, Logger, OrganizationService, newServiceId, groupPatVariable);
                    #endregion

                    #region Catch
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("custom"))
                    {
                        Logger.WriteDebugMessage(ex.Message.Substring(6));

                        throw new InvalidPluginExecutionException(ex.Message.Substring(6));
                    }
                    else
                    {
                        Logger.setMethod = "Execute";
                        Logger.WriteToFile(ex.Message);
                        throw new InvalidPluginExecutionException(ex.Message);
                    }
                }
                #endregion
            }
        }
        #endregion
    }
}

