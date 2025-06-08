using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using MCSShared;
using Microsoft.Xrm.Sdk.Query;
using MCSUtilities2011;

namespace VA.TMP.CRM.Facility_Approval
{
    class FacilityApprovalUpdatePostStageRunner : PluginRunner
    {
        public FacilityApprovalUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override string McsSettingsDebugField
        {
            get { return "cvt_facilityapprovalplugin"; }
        }
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_facilityapproval.EntityLogicalName)
            {
                return;
            }
            var thisRecord = PrimaryEntity.ToEntity<cvt_facilityapproval>();
            var hubDirector = thisRecord.cvt_ApprovalStatusHubDirector?.Value;
            var patFTC = thisRecord.cvt_ApprovalStatusPatientFTC?.Value;
            var proFTC = thisRecord.cvt_ApprovalStatusProviderFTC?.Value;

            //4.8 Enhancement - Feature 5056 - Yearly Review of Facility Approvals
            //Check Review Approval Status
            if (hubDirector == 803750002 || patFTC == 803750002 || proFTC == 803750002 || patFTC == 803750001 || proFTC == 803750001 || hubDirector == 803750001)
            {
                CheckReviewStatus(thisRecord);
            }
            else
            {
                //Check 4x Status
                CheckApprovalStatus(thisRecord);
            }


        }

        internal void CheckApprovalStatus(cvt_facilityapproval thisSave)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var facilityApprovalRecord = srv.cvt_facilityapprovalSet.FirstOrDefault(FA => FA.Id == thisSave.Id);

                if (facilityApprovalRecord == null)
                {
                    Logger.WriteToFile("FA approval record could not be found, exiting FA Approval Status check.");
                    return;
                }
                var anyDenied = false;
                var allApproved = false;
                var pendingCount = 0;
                var pendingCountHub = 0;
                bool isActive = false;

                if (facilityApprovalRecord != null)
                {
                    isActive = CheckRecordStatus((int)facilityApprovalRecord.statecode.Value);

                    var patCOS = facilityApprovalRecord.cvt_ApprovalStatusPatientCOS?.Value;
                    var proCOS = facilityApprovalRecord.cvt_ApprovalStatusProviderCOS?.Value;
                    var patSC = facilityApprovalRecord.cvt_ApprovalStatusPatientSC?.Value;
                    var proSC = facilityApprovalRecord.cvt_ApprovalStatusProviderSC?.Value;
                    var patFTC = facilityApprovalRecord.cvt_ApprovalStatusPatientFTC?.Value;
                    var proFTC = facilityApprovalRecord.cvt_ApprovalStatusProviderFTC?.Value;

                    //check for non-approved/non-denied fields
                    if (facilityApprovalRecord.cvt_ApprovalStatusHubDirector?.Value == 917290000)
                    {
                        pendingCountHub += 1;
                    }

                    if (facilityApprovalRecord.cvt_ApprovalStatusPatientCOS?.Value == 917290000)
                    {
                        pendingCount += 1;
                        pendingCountHub += 1;
                    }
                    if (facilityApprovalRecord.cvt_ApprovalStatusProviderCOS?.Value == 917290000)
                    {
                        pendingCount += 1;
                        pendingCountHub += 1;
                    }
                    if (facilityApprovalRecord.cvt_ApprovalStatusPatientSC?.Value == 917290000)
                    {
                        pendingCount += 1;
                    }
                    if (facilityApprovalRecord.cvt_ApprovalStatusProviderSC?.Value == 917290000)
                    {
                        pendingCount += 1;
                    }
                    if (facilityApprovalRecord.cvt_ApprovalStatusPatientFTC?.Value == 917290000)
                    {
                        pendingCount += 1;
                    }
                    if (facilityApprovalRecord.cvt_ApprovalStatusProviderFTC?.Value == 917290000)
                    {
                        pendingCount += 1;
                    }


                    //If Hub, so check hub director and pat/pro COS
                    if (facilityApprovalRecord.cvt_hubfacility != null && facilityApprovalRecord.cvt_hubfacility.Id != Guid.Empty)
                    {
                        var hubDirector = facilityApprovalRecord.cvt_ApprovalStatusHubDirector?.Value;

                        allApproved = CheckApprovedStatus(hubDirector, anyDenied, allApproved, out anyDenied);
                        allApproved = CheckApprovedStatus(patCOS, anyDenied, allApproved, out anyDenied);
                        allApproved = CheckApprovedStatus(proCOS, anyDenied, allApproved, out anyDenied);
                    }
                    //If not hub, so check both pat and prov SC/COS
                    // also check pat and prov FTC
                    else
                    {
                        allApproved = CheckApprovedStatus(patFTC, anyDenied, allApproved, out anyDenied);
                        allApproved = CheckApprovedStatus(patSC, anyDenied, allApproved, out anyDenied);
                        allApproved = CheckApprovedStatus(patCOS, anyDenied, allApproved, out anyDenied);

                        allApproved = CheckApprovedStatus(proFTC, anyDenied, allApproved, out anyDenied);
                        allApproved = CheckApprovedStatus(proSC, anyDenied, allApproved, out anyDenied);
                        allApproved = CheckApprovedStatus(proCOS, anyDenied, allApproved, out anyDenied);
                    }
                    //Update the status of this record to Denied
                    if (anyDenied == true && facilityApprovalRecord.statuscode.Value != (int)cvt_facilityapproval_statuscode.Denied)
                    {
                        Logger.WriteDebugMessage("anyDenied = true and status is not denied, atemmpting to change to denied.");
                        UpdateFacilityApprovalStatus(facilityApprovalRecord.Id, (int)cvt_facilityapproval_statuscode.Denied);

                        //entry point to Build Service at 'EntryFromFacilityApproval' for new DENIED (was previously Approved) FA
                        Logger.WriteDebugMessage("Updating Service - FA: " + facilityApprovalRecord.cvt_name + " was previously Approved and is now Denied.");
                        CvtHelper.EntryFromFacilityApproval(facilityApprovalRecord, Logger, OrganizationService);

                    }
                    else if (anyDenied == true && facilityApprovalRecord.statuscode.Value == (int)cvt_facilityapproval_statuscode.Denied)
                    {
                        //Correct status
                        Logger.WriteDebugMessage("Should be Denied and is already Denied, don't change the status");
                    }
                    //Update the status of this record to Approved
                    else if (allApproved == true && facilityApprovalRecord.statuscode.Value != (int)cvt_facilityapproval_statuscode.Approved && (pendingCount == 0 || pendingCountHub == 0))
                    {
                        Logger.WriteDebugMessage("allApproved = true and status is not approved, atemmpting to change to approved.");
                        UpdateFacilityApprovalStatus(facilityApprovalRecord.Id, (int)cvt_facilityapproval_statuscode.Approved);

                        //4.8 Enhancement - Feature 5056 - Yearly Review of Facility Approvals
                        //Set Review Due Date
                        SetReviewDueDate(facilityApprovalRecord.Id);

                        //entry point to Build Service at 'EntryFromFacilityApproval' for new APPROVED FA
                        Logger.WriteDebugMessage("Updating Service - FA: " + facilityApprovalRecord.cvt_name + " is now Approved.");
                        CvtHelper.EntryFromFacilityApproval(facilityApprovalRecord, Logger, OrganizationService);

                        var schedulingPackageRecord = srv.cvt_resourcepackageSet.FirstOrDefault(SP => SP.Id == facilityApprovalRecord.cvt_resourcepackage.Id);

                        if (schedulingPackageRecord == null)
                        {
                            Logger.WriteToFile("SP record could not be found, exiting FA Approval Status check.");
                            return;
                        }

                        //create EntityReference
                        EntityReference erRecord = new EntityReference("cvt_facilityapproval", facilityApprovalRecord.Id);

                        if (facilityApprovalRecord.cvt_hubfacility != null)
                        {
                            //Send notification to TSA Notification Team
                            Email tsaNotification = new Email()
                            {
                                Subject = "TSAAPPROVED",
                                Description = facilityApprovalRecord.cvt_providerfacility.Name + " and " + facilityApprovalRecord.cvt_patientfacility.Name + " have now been approved by the Hub Director and the Patient and Provider Chief of Staff to conduct a hub interfacility " + schedulingPackageRecord.cvt_specialty.Name + ", " + schedulingPackageRecord.cvt_specialtysubtype.Name + " telehealth service.<br/><br>/To view the approvals and generate the Telehealth Service Agreement, please " + GetRecordLink(erRecord, OrganizationService, "click here.<br/><br/>"),
                                RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalRecord.Id)

                            };
                            //OrganizationService.Create(tsaNotification);
                        }
                        else
                        {
                            //Send notification to TSA Notification Team

                            if (schedulingPackageRecord.cvt_specialtysubtype != null) //there is a Specialty Subtype
                            {
                                Email tsaNotification = new Email()
                                {
                                    Subject = "TSAAPPROVED",
                                    Description = facilityApprovalRecord.cvt_providerfacility.Name + " and " + facilityApprovalRecord.cvt_patientfacility.Name + " have now been approved by Provider and Patient FTCs, Service Chiefs and Chief of Staff to conduct an interfacility " + schedulingPackageRecord.cvt_specialty.Name + ", " + schedulingPackageRecord.cvt_specialtysubtype.Name + " telehealth service.<br/><br/>To view the approvals and generate the Telehealth Service Agreement, please " + GetRecordLink(erRecord, OrganizationService, "click here.<br/><br/>"),
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalRecord.Id)

                                };
                                OrganizationService.Create(tsaNotification);
                            }
                            else  //no specialty subtype
                            {
                                Email tsaNotification = new Email()
                                {
                                    Subject = "TSAAPPROVED",
                                    Description = facilityApprovalRecord.cvt_providerfacility.Name + " and " + facilityApprovalRecord.cvt_patientfacility.Name + " have now been approved by Provider and Patient FTCs, Service Chiefs and Chief of Staff to conduct an interfacility " + schedulingPackageRecord.cvt_specialty.Name + " telehealth service.<br/><br/>To view the approvals and generate the Telehealth Service Agreement, please " + GetRecordLink(erRecord, OrganizationService, "click here.<br/><br/>"),
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalRecord.Id)

                                };
                                OrganizationService.Create(tsaNotification);
                            }


                        }
                    }
                    else if (facilityApprovalRecord.statuscode.Value == (int)cvt_facilityapproval_statuscode.Approved)
                    {
                        TracingService.Trace("cvt_facilityapproval_statuscode.Approved");
                        var schedulingPackageRecord = srv.cvt_resourcepackageSet.FirstOrDefault(SP => SP.Id == facilityApprovalRecord.cvt_resourcepackage.Id);

                        if (schedulingPackageRecord == null)
                        {
                            TracingService.Trace("schedulingPackageRecord == null");
                            Logger.WriteToFile("SP record could not be found, exiting FA Approval Status check.");
                            return;
                        }

                        var providerfacilityText = string.Empty;
                        var reviewText = string.Empty;
                        var intrafacility = string.Empty;
                        var hubText = string.Empty;
                        var patientText = string.Empty;
                        if (schedulingPackageRecord.cvt_intraorinterfacility.Value == 917290000)
                        {
                            TracingService.Trace("Non hub == intra ");
                            providerfacilityText = facilityApprovalRecord.cvt_providerfacility.Name;
                            reviewText = "<b>Review:</b><br/>";
                            intrafacility = "Intrafacility ";
                            hubText = ", Chief(s) of Staff to conduct an ";
                            patientText = "FTCs,";
                        }
                        else
                        {
                            providerfacilityText = facilityApprovalRecord.cvt_providerfacility.Name + " and " + facilityApprovalRecord.cvt_patientfacility.Name;
                            reviewText = string.Empty;
                            intrafacility = "interfacility ";
                            hubText = "and the Patient and Provider Chief of Staff to conduct a hub ";
                            patientText = "and Patient FTCs,";
                        }

                        //create EntityReference
                        EntityReference erRecord = new EntityReference("cvt_facilityapproval", facilityApprovalRecord.Id);

                        if (facilityApprovalRecord.cvt_hubfacility != null)
                        {
                            TracingService.Trace("facilityApprovalRecord.cvt_hubfacility != null");
                            TracingService.Trace("Two emails");
                            Email tsaNotification = new Email()
                            {
                                Subject = "TSAAPPROVED",
                                Description = facilityApprovalRecord.cvt_providerfacility.Name + " have now been approved by the HUB Director " + hubText + intrafacility + schedulingPackageRecord?.cvt_specialty?.Name + ", " + schedulingPackageRecord?.cvt_specialtysubtype?.Name + " telehealth service.<br/><br/>" + reviewText + "To view the approvals and generate the Telehealth Service Agreement, please " + BuildReportLink(erRecord, OrganizationService, McsSettings, "click here."),

                                RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalRecord.Id)
                            };
                            OrganizationService.Create(tsaNotification);
                        }
                        else
                        {
                            TracingService.Trace("Non hub == ");
                            //Send notification to TSA Notification Team

                            if (schedulingPackageRecord.cvt_specialtysubtype != null) //there is a Specialty Subtype
                            {
                                TracingService.Trace("Non nocvt_specialtysubtype != nulle == intra ");
                                Email tsaNotification = new Email()
                                {
                                    Subject = "TSAAPPROVED",
                                    Description = providerfacilityText + " have now been approved by Provider " + patientText + " Service Chiefs and Chief of Staff to conduct an " + intrafacility + schedulingPackageRecord?.cvt_specialty?.Name + ", " + schedulingPackageRecord?.cvt_specialtysubtype?.Name + " telehealth service.<br/><br/>" + reviewText + "To view the approvals and generate the Telehealth Service Agreement, please " + BuildReportLink(erRecord, OrganizationService, McsSettings, "click here."),
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalRecord.Id)

                                };
                                OrganizationService.Create(tsaNotification);
                            }
                            else  //no specialty subtype
                            {
                                TracingService.Trace("Non no specialty subtype == intra ");
                                Email tsaNotification = new Email()
                                {
                                    Subject = "TSAAPPROVED",
                                    Description = providerfacilityText + " have now been approved by Provider " + patientText + " Service Chiefs and Chief of Staff to conduct an " + intrafacility + schedulingPackageRecord?.cvt_specialty?.Name + " telehealth service.<br/><br/>" + reviewText + "To view the approvals and generate the Telehealth Service Agreement, please " + BuildReportLink(erRecord, OrganizationService, McsSettings, "click here."),
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalRecord.Id)

                                };
                                OrganizationService.Create(tsaNotification);
                            }


                        }
                        //Correct status
                        Logger.WriteDebugMessage("Should be Approved and is already Approved, don't change the status");
                    }
                    else if (facilityApprovalRecord.statuscode.Value == (int)cvt_facilityapproval_statuscode.Pending)
                    {

                        var schedulingPackageRecord = srv.cvt_resourcepackageSet.FirstOrDefault(SP => SP.Id == facilityApprovalRecord.cvt_resourcepackage.Id);
                        TracingService.Trace("schedulingPackageRecord " + schedulingPackageRecord.cvt_intraorinterfacility.Value);

                        if (schedulingPackageRecord.cvt_intraorinterfacility.Value == 917290000)
                        {

                            TracingService.Trace("inside");
                            //Check for just changed values (provider FTC Team)
                            var isProFTCChanged = thisSave.cvt_ApprovalStatusProviderFTC?.Value;
                            Logger.WriteDebugMessage("isProFTCChanged=" + isProFTCChanged);
                            TracingService.Trace("isProFTCChanged=" + isProFTCChanged);
                            if (isProFTCChanged != null && isProFTCChanged == (int)cvt_facilityapprovalstatus.Approve && proSC == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY)
                            {
                                Logger.WriteDebugMessage("Inside isProFTCChanged=" + isProFTCChanged);
                                //Send email to PRO SC - Normal TSA Approval Email
                                Email proSCEmail = new Email()
                                {
                                    Subject = "Service Chief Approval Requested",
                                    Description = "Provider",
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                };
                                OrganizationService.Create(proSCEmail);

                            }

                            //Check for just changed values (provider SC Team)
                            var isProSCChanged = thisSave.cvt_ApprovalStatusProviderSC?.Value;
                            TracingService.Trace("isProFTCChanged=" + isProFTCChanged);
                            Logger.WriteDebugMessage("isProSCChanged=" + isProFTCChanged);
                            if (isProSCChanged != null && isProSCChanged == (int)cvt_facilityapprovalstatus.Approve && proCOS == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY)
                            {
                                Logger.WriteDebugMessage("Inisde isProSCChanged=" + isProFTCChanged);
                                //Send email to PRO SC - Normal TSA Approval Email
                                Email proSCEmail = new Email()
                                {
                                    Subject = "Chief of Staff Approval Requested",
                                    Description = "Provider",
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                };
                                OrganizationService.Create(proSCEmail);

                            }

                            //Check for just changed values:
                            var isHubDirectorChanged = thisSave.cvt_ApprovalStatusHubDirector?.Value;
                            Logger.WriteDebugMessage("isHubDirectorChanged=" + isHubDirectorChanged);
                            var facilityApproval = srv.cvt_facilityapprovalSet.FirstOrDefault(FA => FA.Id == thisSave.Id);
                            if (isHubDirectorChanged != null && isHubDirectorChanged == (int)cvt_facilityapprovalstatus.Approve && (proCOS != (int)cvt_facilityapprovalstatus.Approve))
                            {//Send email to PRO and PAT COS                                
                                if (facilityApproval.cvt_hubfacility?.Id != null)
                                {
                                    Email COSEmail = new Email()
                                    {
                                        Subject = "Chief of Staff Approval Requested",
                                        Description = "Hub",
                                        RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                    };
                                    OrganizationService.Create(COSEmail);
                                }
                            }


                            if ((facilityApprovalRecord.cvt_ApprovalStatusProviderFTC?.Value == (int)cvt_facilityapprovalstatus.Approve && facilityApprovalRecord.cvt_ApprovalStatusProviderSC?.Value == (int)cvt_facilityapprovalstatus.Approve && facilityApprovalRecord.cvt_ApprovalStatusProviderCOS?.Value == (int)cvt_facilityapprovalstatus.Approve) || (facilityApprovalRecord.cvt_hubfacility?.Id == facilityApprovalRecord.cvt_providerfacility?.Id && facilityApprovalRecord.cvt_ApprovalStatusHubDirector?.Value == (int)cvt_facilityapprovalstatus.Approve && facilityApprovalRecord.cvt_ApprovalStatusProviderCOS?.Value == (int)cvt_facilityapprovalstatus.Approve) || (facilityApprovalRecord.cvt_hubfacility?.Id != facilityApprovalRecord.cvt_providerfacility?.Id && facilityApprovalRecord.cvt_ApprovalStatusHubDirector?.Value == (int)cvt_facilityapprovalstatus.Approve && facilityApprovalRecord.cvt_ApprovalStatusProviderCOS?.Value == (int)cvt_facilityapprovalstatus.Approve && facilityApprovalRecord.cvt_ApprovalStatusPatientCOS?.Value == (int)cvt_facilityapprovalstatus.Approve))
                            {
                                SetReviewDueDate(facilityApprovalRecord.Id);

                                //Update Review Due Date Field
                                Entity updateFA = OrganizationService.Retrieve("cvt_facilityapproval", facilityApprovalRecord.Id, new ColumnSet("cvt_reviewduedate"));
                                DateTime approvalDateUTC = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second).AddHours(-4);
                                DateTime approvalDate = new DateTime(approvalDateUTC.Year, approvalDateUTC.Month, approvalDateUTC.Day, 3, 59, 59).AddYears(1).AddDays(31); //Adding 31 Days due to UTC time conversion
                                updateFA["cvt_reviewduedate"] = DateTime.SpecifyKind(approvalDate, DateTimeKind.Utc);
                                updateFA["statuscode"] = new OptionSetValue(917290000);
                                OrganizationService.Update(updateFA);
                            }

                        }
                        else
                        {
                            //Check for just changed values (provider FTC Team)
                            var isProFTCChanged = thisSave.cvt_ApprovalStatusProviderFTC?.Value;
                            Logger.WriteDebugMessage("isProFTCChanged=" + isProFTCChanged);
                            if (isProFTCChanged != null && isProFTCChanged == (int)cvt_facilityapprovalstatus.Approve && proSC == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY)
                            {
                                //Send email to PRO SC - Normal TSA Approval Email
                                Email proSCEmail = new Email()
                                {
                                    Subject = "Service Chief Approval Requested",
                                    Description = "Provider",
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                };
                                OrganizationService.Create(proSCEmail);

                            }

                            //Check for just changed values (patient FTC Team)
                            var isPatFTCChanged = thisSave.cvt_ApprovalStatusPatientFTC?.Value;
                            Logger.WriteDebugMessage("isPatFTCChanged=" + isPatFTCChanged);
                            if (isPatFTCChanged != null && isPatFTCChanged == (int)cvt_facilityapprovalstatus.Approve && patSC == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY)
                            {//Send email to PAT SC
                                Email patSCEmail = new Email()
                                {
                                    Subject = "Service Chief Approval Requested",
                                    Description = "Patient",
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                };
                                OrganizationService.Create(patSCEmail);
                            }

                            //Check for just changed values (Patient SC Team):
                            var isPatSCChanged = thisSave.cvt_ApprovalStatusPatientSC?.Value;
                            Logger.WriteDebugMessage("isPatSCChanged=" + isPatSCChanged);
                            // Check for SC Signee lookup update in the save object and if CoS is pending, then send new CoS Email
                            if (isPatSCChanged != null && isPatSCChanged == (int)cvt_facilityapprovalstatus.Approve && patCOS == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY)
                            {//Send email to PAT COS
                                Email patCOSEmail = new Email()
                                {
                                    Subject = "Chief of Staff Approval Requested",
                                    Description = "Patient",
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                };
                                OrganizationService.Create(patCOSEmail);
                            }


                            //Check for just changed values (Provider SC Team):
                            var isProSCChanged = thisSave.cvt_ApprovalStatusProviderSC?.Value;
                            Logger.WriteDebugMessage("isProSCChanged=" + isProSCChanged);
                            if (isProSCChanged != null && isProSCChanged == (int)cvt_facilityapprovalstatus.Approve && proCOS == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY)
                            {//Send email to PRO COS
                                Email proCOSEmail = new Email()
                                {
                                    Subject = "Chief of Staff Approval Requested",
                                    Description = "Provider",
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                };
                                OrganizationService.Create(proCOSEmail);
                            }

                            //Check for just changed values:
                            var isHubDirectorChanged = thisSave.cvt_ApprovalStatusHubDirector?.Value;
                            Logger.WriteDebugMessage("isHubDirectorChanged=" + isHubDirectorChanged);
                            if (isHubDirectorChanged != null && isHubDirectorChanged == (int)cvt_facilityapprovalstatus.Approve && (proCOS == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY || patCOS == (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY))
                            {//Send email to PRO and PAT COS
                                Email COSEmail = new Email()
                                {
                                    Subject = "Chief of Staff Approval Requested",
                                    Description = "Hub",
                                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisSave.Id)
                                };
                                OrganizationService.Create(COSEmail);
                            }
                        }
                    }
                    //Update the status of this record to Pending
                    else if (facilityApprovalRecord.statuscode.Value != (int)cvt_facilityapproval_statuscode.Pending && allApproved == false && anyDenied == false)
                    {
                        Logger.WriteDebugMessage("allApproved = false and anyDenied = false, status is not pending, and should be changed to pending.");
                        UpdateFacilityApprovalStatus(facilityApprovalRecord.Id, (int)cvt_facilityapproval_statuscode.Pending);
                        //entry point to Build Service at 'EntryFromFacilityApproval' for PENDING FA
                        Logger.WriteDebugMessage("Updating Service - FA: " + facilityApprovalRecord.cvt_name + ". Status of FA has changed to 'Pending'.");
                        CvtHelper.EntryFromFacilityApproval(facilityApprovalRecord, Logger, OrganizationService);
                    }

                    if (!isActive)
                    {
                        //entry point to Build Service at 'EntryFromFacilityApproval' for PENDING FA
                        Logger.WriteDebugMessage("Updating Service - FA: " + facilityApprovalRecord.cvt_name + ". State of FA is Inactive.");
                        CvtHelper.EntryFromFacilityApproval(facilityApprovalRecord, Logger, OrganizationService);
                    }


                }

            }
        }

        internal static string GetRecordLink(EntityReference record, IOrganizationService OrganizationService, string clickHereText = "")
        {
            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, record.LogicalName);
            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            string url = servernameAndOrgname + "/userDefined/edit.aspx?etc=" + etc + "&id=" + record.Id;
            return String.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);
        }

        internal string BuildReportLink(EntityReference record, IOrganizationService OrganizationService, MCSSettings McsSettings, string clickHereText = "")
        {
            string strTsaReportGuid = string.Empty;

            using (var srv = new Xrm(OrganizationService))
            {
                //var TSANote = srv.AnnotationSet.Where(n => n.ObjectId.Id == Email.RegardingObjectId.Id).OrderByDescending(n => n.CreatedOn).First(n => n.NoteText.Contains("Approved by"));
                var settings = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings" && s.statecode == mcs_settingState.Active);
                Logger.WriteDebugMessage("about to get tsaReportGuid");
                strTsaReportGuid = settings.cvt_tsareportgiud;
                Logger.WriteDebugMessage("got tsaReportGuid");
            }           

            Logger.WriteDebugMessage("**********BEGIN BuildReportLink**********");
           
            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, record.LogicalName);
            Logger.WriteDebugMessage("**********TSA REPORT 'ETC': " + etc + "**********");
            Logger.WriteDebugMessage("********** BEGIN GETTING TSA REPORT GUID **********");
            try
            {
                Logger.WriteDebugMessage("**********TSA REPORT GUID: " + strTsaReportGuid + "**********");
            }
            catch (Exception x)
            {
                Logger.WriteDebugMessage("MESSAGE: " + x.Message + "Inner Exception: " + x.InnerException);
            }
            Logger.WriteDebugMessage("********** SUCCESS!  RETRIEVED TSA REPORT GUID! **********");

            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            //string url = servernameAndOrgname + "crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0.rdl&id=%7bdcf51896-4bf4-e811-80e8-000d3a007bf6%7d&records=%7b" + record.Id.ToString() + "%7d&recordstype=" + etc.ToString();
            string url = servernameAndOrgname + "crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0_v2.rdl&id=%7b761d0e50-9ab7-e911-a9e1-000d3a05c4f0%7d&records=%7b" + record.Id.ToString() + "%7d&recordstype=" + etc.ToString();
            //string url = servernameAndOrgname + "crmreports/viewer/viewer.aspx?action=run&context=records&helpID=TSA2.0.rdl&id=%7bdcf51896-4bf4-e811-80e8-000d3a007bf6%7d&records=%7b" + record.Id.ToString() + "%7d&recordstype=10076";
            return String.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);

        }

        internal Boolean CheckApprovedStatus(int? intApprovalStatus, Boolean anyDenied, Boolean allApproved, out Boolean anyDeniedOut)
        {
            switch (intApprovalStatus)
            {
                case (int)cvt_facilityapprovalstatus.Deny:
                    anyDenied = true;
                    break;
                case (int)cvt_facilityapprovalstatus.Approve:
                    allApproved = true;
                    break;
                case (int)cvt_facilityapprovalstatus.ClickheretoAPPROVEorDENY:
                    allApproved = false;
                    break;
                default:
                    allApproved = false;
                    break;
            }
            anyDeniedOut = anyDenied;
            return allApproved;
        }

        internal Boolean CheckRecordStatus(int? statecode)
        {
            bool isActive = true;

            if (statecode != null)
            {
                switch (statecode)
                {
                    case (int)cvt_facilityapprovalState.Inactive:
                        isActive = false;
                        break;
                    case (int)cvt_facilityapprovalState.Active:
                        isActive = true;
                        break;
                }
            }
            else
            {
                isActive = false;
            }


            return isActive;
        }

        internal void UpdateFacilityApprovalStatus(Guid facilityApprovalID, int newStatus)
        {
            //Change State of FA
            SetStateRequest changeFA = new SetStateRequest()
            {
                EntityMoniker = new EntityReference(cvt_facilityapproval.EntityLogicalName, facilityApprovalID),
                State = new OptionSetValue((int)cvt_facilityapprovalState.Active),
                Status = new OptionSetValue(newStatus)
            };

            OrganizationService.Execute(changeFA);
        }

        //4.8 Enhancement - Feature 5056 - Yearly Review of Facility Approvals
        internal void SetReviewDueDate(Guid facilityApprovalID)
        {
            //Update Review Due Date Field
            Entity updateFA = OrganizationService.Retrieve("cvt_facilityapproval", facilityApprovalID, new ColumnSet("cvt_reviewduedate"));
            DateTime approvalDateUTC = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second).AddHours(-4);
            DateTime approvalDate = new DateTime(approvalDateUTC.Year, approvalDateUTC.Month, approvalDateUTC.Day, 3, 59, 59).AddYears(1).AddDays(31); //Adding 31 Days due to UTC time conversion
            updateFA["cvt_reviewduedate"] = DateTime.SpecifyKind(approvalDate, DateTimeKind.Utc);
            OrganizationService.Update(updateFA);
        }

        //4.8 Enhancement - Feature 5056 - Send emails to COS/SC if FCT Approval Team has updated status to "Reviewed and Updated" - both provider and patient side
        internal void CreateReviewedandUpdatedEmail(cvt_facilityapproval thisRecord)
        {
            Logger.WriteDebugMessage("Create Reviewed and Updated Email");

            //Send 2 emails for Hub
            if (thisRecord.cvt_ApprovalStatusHubDirector?.Value == 803750002)
            {
                //Send email to patient COS
                Email patCOSEmail = new Email()
                {
                    Subject = "Chief of Staff Approval Requested",
                    Description = "Patient",
                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisRecord.Id)
                };
                OrganizationService.Create(patCOSEmail);

                //Send email to PRO COS
                Email proCOSEmail = new Email()
                {
                    Subject = "Chief of Staff Approval Requested",
                    Description = "Provider",
                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisRecord.Id)
                };
                OrganizationService.Create(proCOSEmail);
                Logger.WriteDebugMessage("Emails created for Provider and Patient COS");
            }
            //If Patient or Provider is "Reviewed and Updated" send emails to Patient and Provider COS and SC
            else
            {
                //Send email to patient SC
                Email patSCEmail = new Email()
                {
                    Subject = "Service Chief Approval Requested",
                    Description = "Patient",
                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisRecord.Id)
                };
                OrganizationService.Create(patSCEmail);

                Logger.WriteDebugMessage("Emails created for Patient COS and SC");

                //Send email to PRO SC
                Email proSCEmail = new Email()
                {
                    Subject = "Service Chief Approval Requested",
                    Description = "Provider",
                    RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, thisRecord.Id)
                };
                OrganizationService.Create(proSCEmail);
                Logger.WriteDebugMessage("Emails created for Provider COS and SC");

            }


        }

        //4.8 Enhancement - This updates Review Due Date when Status is set to "Reviewed and Confirmed"
        internal void CheckReviewStatus(cvt_facilityapproval thisSave)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var facilityApprovalRecord = srv.cvt_facilityapprovalSet.FirstOrDefault(FA => FA.Id == thisSave.Id);

                if (facilityApprovalRecord == null)
                {
                    Logger.WriteToFile("FA approval record could not be found, exiting FA Approval Status check.");
                    return;
                }

                if (facilityApprovalRecord != null)
                {
                    var patFTC = facilityApprovalRecord.cvt_ApprovalStatusPatientFTC?.Value;
                    var proFTC = facilityApprovalRecord.cvt_ApprovalStatusProviderFTC?.Value;
                    var hubDirector = facilityApprovalRecord.cvt_ApprovalStatusHubDirector?.Value;

                    //If PATFTC is Reviewed and Updated
                    if (thisSave.cvt_ApprovalStatusPatientFTC?.Value != null && thisSave.cvt_ApprovalStatusPatientFTC?.Value == 803750002)
                    {
                        Logger.WriteDebugMessage("Since the Approval Status has been changed to Reviewed and Updated, change overall status to Pending");
                        UpdateFacilityApprovalStatus(thisSave.Id, (int)cvt_facilityapproval_statuscode.Pending);

                        //If PROFTC is Reviewed and Confirmed OR Reviewed and Updated, send emails
                        if (proFTC == 803750001 || thisSave.cvt_ApprovalStatusProviderFTC?.Value == 803750001 || proFTC == 803750002 || thisSave.cvt_ApprovalStatusProviderFTC?.Value == 803750002)
                        {
                            //Send emails to SC
                            CreateReviewedandUpdatedEmail(thisSave);
                        }

                    }
                    //If PROFTC is Reviewed and Updated
                    else if (thisSave.cvt_ApprovalStatusProviderFTC?.Value != null && thisSave.cvt_ApprovalStatusProviderFTC?.Value == 803750002)
                    {
                        Logger.WriteDebugMessage("Since the Approval Status has been changed to Reviewed and Updated, change overall status to Pending");
                        UpdateFacilityApprovalStatus(thisSave.Id, (int)cvt_facilityapproval_statuscode.Pending);

                        //If PATFTC is Reviewed and Confirmed OR Reviewed and Updated
                        if (patFTC == 803750001 || thisSave.cvt_ApprovalStatusPatientFTC?.Value == 803750001 || patFTC == 803750002 || thisSave.cvt_ApprovalStatusPatientFTC?.Value == 803750002)
                        {
                            CreateReviewedandUpdatedEmail(thisSave);
                        }
                    }
                    //If PATFTC is Reviewed and Confirmed
                    else if (thisSave.cvt_ApprovalStatusPatientFTC?.Value != null && thisSave.cvt_ApprovalStatusPatientFTC?.Value == 803750001)
                    {   //If PROFTC is Reviewed and Confirmed
                        if (proFTC == 803750001 || thisSave.cvt_ApprovalStatusProviderFTC?.Value == 803750001)
                        {
                            //Update Review Due Date
                            SetReviewDueDate(facilityApprovalRecord.Id);
                        }
                        //Else if PROFTC is Reviewed and Updated 
                        else if (proFTC == 803750002 || thisSave.cvt_ApprovalStatusProviderFTC?.Value == 803750002)
                        {
                            //Send emails to SC
                            CreateReviewedandUpdatedEmail(thisSave);
                        }
                    }
                    //If PROFTC is Reviewed and Confirmed
                    else if (thisSave.cvt_ApprovalStatusProviderFTC?.Value != null && thisSave.cvt_ApprovalStatusProviderFTC?.Value == 803750001)
                    {   //If PATFTC is Reviewed and Confirmed
                        if (patFTC == 803750001 || thisSave.cvt_ApprovalStatusPatientFTC?.Value == 803750001)
                        {
                            //Update Review Due Date
                            SetReviewDueDate(facilityApprovalRecord.Id);
                        }
                        //Else if PATFTC is Reviewed and Updated
                        else if (patFTC == 803750002 || thisSave.cvt_ApprovalStatusPatientFTC?.Value == 803750002)
                        {
                            CreateReviewedandUpdatedEmail(thisSave);
                        }
                    }
                    //Else If Hub Director is Reviewed and Confirmed
                    else if (thisSave.cvt_ApprovalStatusHubDirector?.Value != null && thisSave.cvt_ApprovalStatusHubDirector?.Value == 803750001)
                    {
                        //Update Review Due Date
                        SetReviewDueDate(facilityApprovalRecord.Id);
                    }
                    //Else if Hub Director is Reviewed and Updated, update status to pending and send emails
                    else if (thisSave.cvt_ApprovalStatusHubDirector?.Value != null && thisSave.cvt_ApprovalStatusHubDirector?.Value == 803750002)
                    {
                        Logger.WriteDebugMessage("Since the Approval Status has been changed to Reviewed and Updated, change overall status to Pending");
                        UpdateFacilityApprovalStatus(thisSave.Id, (int)cvt_facilityapproval_statuscode.Pending);
                        CreateReviewedandUpdatedEmail(thisSave);
                    }
                }
            }
        }

    }
}

