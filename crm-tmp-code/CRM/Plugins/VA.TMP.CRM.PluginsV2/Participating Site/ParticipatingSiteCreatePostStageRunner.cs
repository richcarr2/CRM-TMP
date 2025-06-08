using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using VA.TMP.OptionSets;
using MCSShared;

namespace VA.TMP.CRM
{
    class ParticipatingSiteCreatePostStageRunner : PluginRunner
    {
        public ParticipatingSiteCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }       
        public override string McsSettingsDebugField
        {
            get { return "cvt_participatingsiteplugin"; }
        }
        //Set plugin step to cvt_scheduleable and update of PS record.
        public override void Execute()
        {
            
            var participatingSite = PrimaryEntity.ToEntity<cvt_participatingsite>();

            //Only Attempt to generate Facility Approvals for PS that is now changed to Scheduleable
            //if (participatingSite["cvt_scheduleable"] != null && participatingSite.cvt_scheduleable.Value == true)
            Logger.WriteDebugMessage("Looking for Automatic Facility Approval switch");
            using (var srv = new Xrm(OrganizationService))
            {
                var thisParticipatingSite = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == PrimaryEntity.Id);
                var activeSettings = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings");
                if (activeSettings != null)
                {

                    if (activeSettings.cvt_AutomaticFacilityApproval != null && activeSettings.cvt_SystemUserAccount != null)
                    {
                        Logger.WriteDebugMessage("Found active settings and Automatic Approval switch is: " + activeSettings.cvt_AutomaticFacilityApproval.Value);
                        GenerateFacilityApprovals(thisParticipatingSite, activeSettings.cvt_AutomaticFacilityApproval.Value, activeSettings.cvt_SystemUserAccount.Id);
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Not passing in active settings.");
                        GenerateFacilityApprovals(thisParticipatingSite, false, Guid.Empty);
                    }
                }

                //Verify that the correct teams are in place
                VerifyTeams(thisParticipatingSite);

            }
        }

        public void GenerateFacilityApprovals(cvt_participatingsite participatingSite, bool AutomaticFA, Guid sysUserId)
        {
            Logger.setMethod = "GenerateFacilityApprovals";
            Logger.WriteDebugMessage("starting GenerateFacilityApprovals");

            using (var srv = new Xrm(OrganizationService))
            {
                if (participatingSite == null)
                {
                    Logger.WriteDebugMessage("Error: Participating Site could not be found. Exiting GenerateFacilityApprovals.");
                    throw new InvalidPluginExecutionException("Error: Participating Site could not be found.");
                }

                if (participatingSite.cvt_locationtype == null)
                {
                    Logger.WriteDebugMessage("Error: Participating Site's side is not specified. Exiting GenerateFacilityApprovals.");
                    throw new InvalidPluginExecutionException("Error: Participating Site's side is not specified."); ;
                }

                if (participatingSite.cvt_resourcepackage == null)
                {
                    Logger.WriteDebugMessage("Error: Participating Site is not related to a Scheduling Package. Exiting GenerateFacilityApprovals.");
                    throw new InvalidPluginExecutionException("Error: Participating Site is not related to a Scheduling Package.");
                }

                var resourcePackage = srv.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == participatingSite.cvt_resourcepackage.Id);
                if (resourcePackage == null)
                {

                    Logger.WriteDebugMessage("Error: Participating Site's Scheduling Package could not be found. Exiting GenerateFacilityApprovals.");
                    throw new InvalidPluginExecutionException("Error: Participating Site's Scheduling Package could not be found.");
                }

                //r4.6v4 Check the Usage Type for SP
                //By default, if no value or if false, then it is scheduling.
                //if true, then  it is TSA
                if (resourcePackage.cvt_usagetype == null || resourcePackage.cvt_usagetype.Value == false)
                {
                    //Scheduling Type
                    Logger.WriteDebugMessage("Usage Type is Scheduling, no Facility Approvals needed. Exiting GenerateFacilityApprovals.");

                    //Participating Site email notification-Naveen Dubbaka
                    ////commewntint out for URS
                    //Entity participatingSitePostImage = PluginExecutionContext.PostEntityImages["PostImage"] as Entity;
                    //SPEmailNotification.ResourcePackage(OrganizationService, resourcePackage, participatingSitePostImage);

                    return;
                }

                List<EntityReference> secondaryFacilities = srv.cvt_participatingsiteSet.Where(s => s.cvt_locationtype.Value != participatingSite.cvt_locationtype.Value && s.cvt_resourcepackage.Id == participatingSite.cvt_resourcepackage.Id).Select(s => s.cvt_facility).Distinct().ToList();

                if (secondaryFacilities != null)
                {
                    Logger.WriteDebugMessage("Found distinct secondary facilities: " + secondaryFacilities.Count);
                    CreateFacilityApprovalRecord(participatingSite, secondaryFacilities, AutomaticFA, sysUserId);
                }
            }
        }

        public void CreateFacilityApprovalRecord(cvt_participatingsite site, List<EntityReference> secondaryFacilities, bool AutomaticFA, Guid sysUserId)
        {
            Logger.setMethod = "CreateFacilityApprovalRecord";
            Logger.WriteDebugMessage("Starting CreateFacilityApprovalRecord");
            var thisRecordisPat = site.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient;
            Logger.WriteDebugMessage("'Primary Site' is Patient = " + thisRecordisPat);

            foreach (var secondaryFacility in secondaryFacilities)
            {
                var provFacility = thisRecordisPat ? secondaryFacility : site.cvt_facility;
                var patFacility = thisRecordisPat ? site.cvt_facility : secondaryFacility;
                using (var srv = new Xrm(OrganizationService))
                {
                    Logger.WriteDebugMessage("Getting Resource Package.");
                    var SchedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(RP => RP.Id == site.cvt_resourcepackage.Id);
                    Logger.WriteDebugMessage("ProFacId: " + provFacility.Id + ". PatFacId: " + patFacility.Id);
                    if (provFacility.Id != null && patFacility.Id != null)
                    {
                        Logger.WriteDebugMessage("Pat and Pro Facility are different, continue with Facility Approval create/check");
                        //Query to see if this approval already exists
                        var checkForRecord = srv.cvt_facilityapprovalSet.FirstOrDefault(fa => fa.cvt_resourcepackage.Id == site.cvt_resourcepackage.Id &&
                        fa.cvt_providerfacility.Id == provFacility.Id && fa.cvt_patientfacility.Id == patFacility.Id);

                        if (checkForRecord != null)
                        {
                            Logger.WriteDebugMessage("Found existing Facility Approval for this combination of Resource Package/Provider Facility/Patient Facility, no need to create another Facility Approval record.");
                            break;
                        }
                        else
                        {
                            Logger.WriteDebugMessage("Continuing with create, no existing FA found.");
                            EntityReference setSysUser = new EntityReference(SystemUser.EntityLogicalName, sysUserId);
                            DateTime setDT = DateTime.Now;
                            var teamCheck = "";

                            var facilityName = SchedulingPackage.cvt_intraorinterfacility.Value == 917290000 ? provFacility.Name : provFacility.Name + " -> " + patFacility.Name;

                            var facilityApproval = new cvt_facilityapproval
                            {
                                cvt_resourcepackage = site.cvt_resourcepackage,
                                cvt_patientfacility = patFacility,
                                cvt_providerfacility = provFacility,
                                cvt_name = facilityName,
                            };

                            if (AutomaticFA == true)
                            {
                                Logger.WriteDebugMessage("AutomaticFA == true, setting the FA status to Approved.");
                                facilityApproval.statuscode = new OptionSetValue((int)cvt_facilityapproval_statuscode.Approved);
                            }

                            //Hub
                            if (SchedulingPackage.cvt_hub != null && SchedulingPackage.cvt_hub.Id != Guid.Empty)
                            {
                                Logger.WriteDebugMessage("Getting Hub Facility Director approval team.");
                                var HubDirectorTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Facility.Id == SchedulingPackage.cvt_hub.Id && T.cvt_Type.Value == (int)Teamcvt_Type.HubDirector);
                                var HubCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == SchedulingPackage.cvt_hub.Id);
                                //setting chief of staff eteam hub facility
                                if (HubCoSTeam != null)
                                {
                                    facilityApproval.cvt_chiefofstaffteamhubfacility = new EntityReference(Team.EntityLogicalName, HubCoSTeam.Id);
                                }
                                //Check if team exists and if there are team members.
                                teamCheck += checkTeam(HubDirectorTeam, "Hub Director", SchedulingPackage.cvt_hub.Name);

                                if (teamCheck != "")
                                    throw new InvalidPluginExecutionException(teamCheck);

                                facilityApproval.cvt_hubfacility = new EntityReference(Team.EntityLogicalName, SchedulingPackage.cvt_hub.Id);
                                facilityApproval.cvt_HubDirectorTeam = new EntityReference(Team.EntityLogicalName, HubDirectorTeam.Id);

                                //facilityApproval.cvt_chiefofstaffteamhubfacility = new EntityReference(Team.EntityLogicalName, HubCoSTeam.Id);

                                if (AutomaticFA == true)
                                {
                                    facilityApproval.cvt_SigneeHubDirector = setSysUser;
                                    facilityApproval.cvt_DateSignedHubDirector = setDT;
                                    facilityApproval.cvt_ApprovalStatusHubDirector = new OptionSetValue((int)cvt_facilityapprovalstatus.Approve);
                                }
                            }
                            //Non-Hub
                            else
                            {
                                Logger.WriteDebugMessage("Getting SC approval teams for specialty.");

                                Logger.WriteDebugMessage(SchedulingPackage.cvt_specialty.Name + " - " + SchedulingPackage.cvt_specialty.Id);

                                var PatSCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && T.cvt_Facility.Id == patFacility.Id && T.cvt_ServiceType.Id == SchedulingPackage.cvt_specialty.Id);

                                var ProSCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && T.cvt_Facility.Id == provFacility.Id && T.cvt_ServiceType.Id == SchedulingPackage.cvt_specialty.Id);

                               // var HubCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.HubDirector && T.cvt_Facility.Id == site.cvt_facility.Id && T.cvt_ServiceType.Id == SchedulingPackage.cvt_specialty.Id);

                                Logger.WriteDebugMessage("About to check for teams.");

                                Logger.WriteDebugMessage("patFacility Name = " + patFacility.Name);
                                Logger.WriteDebugMessage("proFacility Name = " + provFacility.Name);

                                //Check if team exists and if there are team members.
                                teamCheck += checkTeam(PatSCTeam, "Patient Service Chief", patFacility.Name);
                                teamCheck += checkTeam(ProSCTeam, "Provider Service Chief", provFacility.Name);

                                Logger.WriteDebugMessage("Finished SC Team Check.");

                                if (teamCheck != "")
                                    throw new InvalidPluginExecutionException(teamCheck);

                                facilityApproval.cvt_ServiceChiefTeamPatient = new EntityReference(Team.EntityLogicalName, PatSCTeam.Id);
                                facilityApproval.cvt_ServiceChiefTeamProvider = new EntityReference(Team.EntityLogicalName, ProSCTeam.Id);
                             // facilityApproval.cvt_chiefofstaffteamhubfacility = new EntityReference(Team.EntityLogicalName, HubCoSTeam.Id);

                                if (AutomaticFA == true)
                                {
                                    facilityApproval.cvt_SigneePatientFTC = setSysUser;
                                    facilityApproval.cvt_DateSignedPatientFTC = setDT;
                                    facilityApproval.cvt_ApprovalStatusPatientFTC = new OptionSetValue((int)cvt_facilityapprovalstatus.Approve);

                                    facilityApproval.cvt_SigneeProviderFTC = setSysUser;
                                    facilityApproval.cvt_DateSignedProviderFTC = setDT;
                                    facilityApproval.cvt_ApprovalStatusProviderFTC = new OptionSetValue((int)cvt_facilityapprovalstatus.Approve);

                                    facilityApproval.cvt_SigneePatientSC = setSysUser;
                                    facilityApproval.cvt_DateSignedPatientSC = setDT;
                                    facilityApproval.cvt_ApprovalStatusPatientSC = new OptionSetValue((int)cvt_facilityapprovalstatus.Approve);

                                    facilityApproval.cvt_SigneeProviderSC = setSysUser;
                                    facilityApproval.cvt_DateSignedProviderSC = setDT;
                                    facilityApproval.cvt_ApprovalStatusProviderSC = new OptionSetValue((int)cvt_facilityapprovalstatus.Approve);
                                }
                            }

                            Logger.WriteDebugMessage("Getting COS approval teams.");
                            var PatCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == patFacility.Id);

                            var ProCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == provFacility.Id);

                          
                            //Check if team exists and if there are team members.                           
                            teamCheck += checkTeam(PatCoSTeam, "Patient Chief of Staff", patFacility.Name);
                            teamCheck += checkTeam(ProCoSTeam, "Provider Chief of Staff", provFacility.Name);

                            if (teamCheck != "")
                                throw new InvalidPluginExecutionException(teamCheck);

                            Logger.WriteDebugMessage("Finished Team Check for CoS teams");


                            Logger.WriteDebugMessage("Getting FTC Teams");
                            var PatFtcTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.FTC && T.cvt_Facility.Id == patFacility.Id);
                            var ProFtcTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.FTC && T.cvt_Facility.Id == provFacility.Id);

                            //Check if team exists and if there are team members.                           
                            teamCheck += checkTeam(PatFtcTeam, "FTC Approval Group", patFacility.Name);
                            teamCheck += checkTeam(ProFtcTeam, "FTC Approval Group", provFacility.Name);

                            if (teamCheck != "")
                                throw new InvalidPluginExecutionException(teamCheck);

                            Logger.WriteDebugMessage("Finished Team Check for FTC teams");

                            Logger.WriteDebugMessage("No teamCheck messages, continuing");
                            facilityApproval.cvt_ChiefofStaffTeamPatient = new EntityReference(Team.EntityLogicalName, PatCoSTeam.Id);                         
                            facilityApproval.cvt_ChiefofStaffTeamProvider = new EntityReference(Team.EntityLogicalName, ProCoSTeam.Id);
                            facilityApproval.cvt_FTCTeamPatient = new EntityReference(Team.EntityLogicalName, PatFtcTeam.Id);
                            facilityApproval.cvt_FTCTeamProvider = new EntityReference(Team.EntityLogicalName, ProFtcTeam.Id);                            

                            Logger.WriteDebugMessage("About to create the FA");
                            Logger.WriteDebugMessage("Automatic Facility Approval Switch: " + AutomaticFA);
                            if (AutomaticFA == true)
                            {
                                facilityApproval.cvt_SigneePatientCOS = setSysUser;
                                facilityApproval.cvt_DateSignedPatientCOS = setDT;
                                facilityApproval.cvt_ApprovalStatusPatientCOS = new OptionSetValue((int)cvt_facilityapprovalstatus.Approve);

                                facilityApproval.cvt_SigneeProviderCOS = setSysUser;
                                facilityApproval.cvt_DateSignedProviderCOS = setDT;
                                facilityApproval.cvt_ApprovalStatusProviderCOS = new OptionSetValue((int)cvt_facilityapprovalstatus.Approve);

                                facilityApproval.cvt_AutomaticallyApproved = true;
                            }
                            try
                            {
                                Logger.WriteDebugMessage("About to create the Facility Approval record.");
                                Guid faId = OrganizationService.Create(facilityApproval);

                                if (AutomaticFA == true)
                                {
                                    Logger.WriteDebugMessage("About to create the automatically approved note for the Facility Approval record.");
                                    //Create Note
                                    Annotation note = new Annotation()
                                    {
                                        Subject = "Automatically Approved",
                                        NoteText = "This interfacility TSA was automatically approved by the system during the transition to the Scheduling Package structure.",
                                        ObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, faId)
                                    };
                                    OrganizationService.Create(note);
                                }
                                else
                                {
                                    //send notification email to the FTC teams on each side
                                    try
                                    {
                                        CreateFTCNotification(faId);
                                        //CreateFTCNotification(ProFtcTeam, "provider", faId);
                                    }
                                    catch (Exception emEx)
                                    {
                                        Logger.WriteToFile("Error trying to create FTC Notification Email in ParticipatingSiteCreatePostStageRunner.  Error was: " + emEx.Message);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteToFile("Errored when trying to create the facility approval record. Error: " + ex.Message);
                            }
                        }
                    }
                    
                    else
                    {
                        //Logger.WriteDebugMessage("Detected Intrafacility pairing, do not create Facility Approval record.");

                        //if (AutomaticFA != true)
                        //{
                        //    //Send Notification Email to SC Team
                        //    Email intrafacilityNotification = new Email()
                        //    {
                        //        Subject = "Notification of Intrafacility " + SchedulingPackage.cvt_specialty.Name + " Telehealth Services to " + site.cvt_site.Name,
                        //        RegardingObjectId = new EntityReference(cvt_participatingsite.EntityLogicalName, site.Id)
                        //    };

                        //    OrganizationService.Create(intrafacilityNotification);
                        //    Logger.WriteDebugMessage("Created Intrafacility Notification");
                        //}
                        //else
                        //{
                        //    Logger.WriteDebugMessage("Bypassed Intrafacility Notification for Facility Approval because System-wide switch is on.");
                        //}
                    }
                }
            }
        }

        public string checkTeam(Team thisTeam, string check, string facility)
        {
            Logger.WriteDebugMessage("checkTeam for function.");
            var message = "";

            if (thisTeam == null)
                message = check + " Team (at " + facility + ") is missing, it needs to be created. \n";
            else
            {
                Logger.WriteDebugMessage("Checking the following team for members: " + thisTeam.Name);
                //Check for team members
                using (var srv = new Xrm(OrganizationService))
                {
                    var members = srv.TeamMembershipSet.Where(tm => tm.TeamId.Value == thisTeam.Id).ToList();
                    if (members == null || members.Count == 0)
                        message = thisTeam.Name + " has no members, users need to be added. \n";
                }
            }
            return message;
        }

        public void CreateFTCNotification(Guid faId)
        {
            Email FTCEmail = new Email()
            {
                Subject = "FTC Team Approval Requested",
                Description = "new message",
                RegardingObjectId = new EntityReference(cvt_facilityapproval.EntityLogicalName, faId)
            };

            OrganizationService.Create(FTCEmail);
        }

        public void VerifyTeams(cvt_participatingsite site)
        {
            Logger.WriteDebugMessage("Starting");
            var facility = site.cvt_facility;
            var teamCheck = "";

            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("Getting Resource Package.");
                if (site.cvt_resourcepackage == null || site.cvt_resourcepackage.Id == Guid.Empty)
                {
                    Logger.WriteDebugMessage("Scheduling Package not filled in on this Participating Site.");
                    throw new InvalidPluginExecutionException("Scheduling Package not filled in on this Participating Site.");
                }
                if (facility == null || facility.Id == Guid.Empty)
                {
                    Logger.WriteDebugMessage("Facility not filled in on this Participating Site.");
                    throw new InvalidPluginExecutionException("Facility not filled in on this Participating Site.");
                }

                var SchedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(RP => RP.Id == site.cvt_resourcepackage.Id);

                //Hub
                if (SchedulingPackage.cvt_hub != null && SchedulingPackage.cvt_hub.Id != Guid.Empty)
                {
                    var HubDirectorTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Facility.Id == SchedulingPackage.cvt_hub.Id && T.cvt_Type.Value == (int)Teamcvt_Type.HubDirector);
                    teamCheck += checkTeam(HubDirectorTeam, "Hub Director", SchedulingPackage.cvt_hub.Name);
                }
                else //Non-Hub
                {
                    var SCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && T.cvt_Facility.Id == facility.Id && T.cvt_ServiceType.Id == SchedulingPackage.cvt_specialty.Id);
                    teamCheck += checkTeam(SCTeam, "Service Chief", facility.Name);
                }

                var PatCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == facility.Id);
                var PatFtcTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.FTC && T.cvt_Facility.Id == facility.Id);                         
                teamCheck += checkTeam(PatCoSTeam, "Chief of Staff", facility.Name);
                teamCheck += checkTeam(PatFtcTeam, "FTC Approval Group", facility.Name);

                if (teamCheck != "")
                    throw new InvalidPluginExecutionException(teamCheck);
            }
        }
    }
}