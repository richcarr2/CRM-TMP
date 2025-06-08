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
    class ParticipatingSiteUpdatePreStageRunner : PluginRunner
    {
        public ParticipatingSiteUpdatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override string McsSettingsDebugField
        {
            get { return "cvt_participatingsiteplugin"; }
        }
        //Set plugin step to cvt_scheduleable and update of PS record.
        public override void Execute()
        {
            Logger.WriteDebugMessage("Starting.");
            var participatingSite = PrimaryEntity.ToEntity<cvt_participatingsite>();

            if (participatingSite.cvt_scheduleable != null && participatingSite.cvt_scheduleable.HasValue && participatingSite.cvt_scheduleable.Value == true)
            {
                Logger.WriteDebugMessage("To Be Scheduled = Yes, checking for requirements next.");
                CheckForRequirements();
                bool psVCChecks = CvtHelper.ValidatePS(participatingSite.Id, new cvt_schedulingresource(), Logger, OrganizationService);
                if (!psVCChecks)
                {
                    throw new InvalidPluginExecutionException("Participating Site did not pass requirements checks, cannot be put into Can Be Scheduled = Yes.");
                }
                Logger.WriteDebugMessage("About to retrieve the derived name.");
                //Check Pre Name against format and update if needed
                var derivedName = CvtHelper.ReturnRecordNameIfChanged(PrimaryEntity.ToEntity<cvt_participatingsite>(), false, Logger, OrganizationService);
                Logger.WriteDebugMessage("The derivedName came back, it is: " + derivedName.ToString());

                if (!String.IsNullOrEmpty(derivedName))
                {
                    Logger.WriteDebugMessage(String.Format("The Participating Site name should be different than {0}, updating it in the CreatePreStage to: {1}.", PrimaryEntity.Attributes["cvt_name"].ToString(), derivedName));
                    PrimaryEntity.Attributes["cvt_name"] = (string)derivedName;
                    Logger.WriteDebugMessage("New name as read from the PrimaryEntity: " + PrimaryEntity.Attributes["cvt_name"].ToString());
                }
                else
                {
                    Logger.WriteDebugMessage("No change made to the name.  The Participating Site name is already correct.");
                }
            }
        }

        public void CheckForRequirements()
        {
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var participatingSite = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == PrimaryEntity.Id);
                if (participatingSite == null)
                {
                    Logger.WriteDebugMessage("Error: Participating Site could not be found. Exiting.");
                    throw new InvalidPluginExecutionException("Error: Participating Site could not be found.");
                }

                if (participatingSite.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient)
                    Logger.WriteDebugMessage("No Minimum Requirement for Patient Side.");
                else if (participatingSite.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider)
                {
                    Logger.WriteDebugMessage("This is a Provider Side Location.");

                    //If it is SFT, then skip this check
                    var schedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(sp => sp.Id == participatingSite.cvt_resourcepackage.Id);
                    if (schedulingPackage != null && schedulingPackage.cvt_availabletelehealthmodality.Value == (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward)
                    {
                        Logger.WriteDebugMessage("SFT, therefore no Provider necessary.");
                        return;
                    }

                    //Make sure at lease one user is listed as a resource
                    var PSresources = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_participatingsite.Id == PrimaryEntity.Id);

                    if (PSresources == null)
                    {
                        Logger.WriteDebugMessage("Error: Provider Participating Site needs to have a resource. Exiting.");
                        throw new InvalidPluginExecutionException("Provider Participating Site needs to have a Provider listed as a resource before it can be put into Can Be Scheduled is Yes.");
                    }
                    Boolean foundUser = false;
                    Logger.WriteDebugMessage(String.Format("Found {0} Scheduling Resources. Checking them now.", PSresources.ToList().Count.ToString()));
                    foreach (cvt_schedulingresource record in PSresources)
                    {
                        Logger.WriteDebugMessage("Found Scheduling Resources. Checking them now.");
                        if (record.cvt_user != null && record.cvt_user.Id != Guid.Empty)
                        {
                            foundUser = true;
                            Logger.WriteDebugMessage("Found User as TMP Resource.");
                        }
                        //See if this is a Resource Group
                        else if (record.cvt_tmpresourcegroup != null)
                        {
                            Logger.WriteDebugMessage("Detected Resource Group. Getting the Group Resources within the RG: " + record.cvt_tmpresourcegroup.Name);
                            var grResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == record.cvt_tmpresourcegroup.Id);

                            foreach (mcs_groupresource gr in grResources)
                            {
                                if (gr.mcs_RelatedUserId != null && gr.mcs_RelatedUserId.Id != Guid.Empty)
                                {
                                    foundUser = true;
                                    Logger.WriteDebugMessage("Found User: " + gr.mcs_RelatedUserId.Name);
                                }
                            }
                        }
                    }

                    if (foundUser == false)
                    {
                        Logger.WriteDebugMessage("Error: Provider Participating Site needs to have a user. Exiting.");
                        throw new InvalidPluginExecutionException("Provider Participating Site needs to have a Provider listed as a resource before it can be put into Can Be Scheduled is Yes.");
                    }
                }
            }
        }
        public void GenerateFacilityApprovals()
        {
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var participatingSite = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == PrimaryEntity.Id);
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
                //Check if TSA is H/M
                if (resourcePackage.cvt_patientlocationtype?.Value ==(int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnect)
                {
                    Logger.WriteDebugMessage("Participating Site's Scheduling Package is H/M, no Facility Approvals needed. Exiting GenerateFacilityApprovals.");
                    return;
                }

                List<EntityReference> secondaryFacilities = srv.cvt_participatingsiteSet.Where(s => s.cvt_locationtype.Value != participatingSite.cvt_locationtype.Value && s.cvt_resourcepackage.Id == participatingSite.cvt_resourcepackage.Id).Select(s => s.cvt_facility).Distinct().ToList();

                if (secondaryFacilities != null)
                {
                    Logger.WriteDebugMessage("Found distinct secondary facilities: " + secondaryFacilities.Count);
                    CreateFacilityApprovalRecord(participatingSite, secondaryFacilities);
                }
            }
        }

        public void CreateFacilityApprovalRecord(cvt_participatingsite site, List<EntityReference> secondaryFacilities)
        {
            Logger.WriteDebugMessage("starting");
            var thisRecordisPat = site.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient;
            Logger.WriteDebugMessage("'Primary Site' is Patient = " + thisRecordisPat);

            foreach (var secondaryFacility in secondaryFacilities)
            {
                var provFacility = thisRecordisPat ? secondaryFacility : site.cvt_facility;
                var patFacility = thisRecordisPat ? site.cvt_facility : secondaryFacility;
                
                using (var srv = new Xrm(OrganizationService))
                {
                    Logger.WriteDebugMessage("Getting Resource Package.");
                    if (provFacility == null || provFacility.Id == Guid.Empty)
                    {
                        Logger.WriteDebugMessage("Provider Facility was not found.");
                        throw new InvalidPluginExecutionException("Provider Facility was not found.");
                    }
                    if (patFacility == null || patFacility.Id == Guid.Empty)
                    {
                        Logger.WriteDebugMessage("Patient Facility was not found.");
                        throw new InvalidPluginExecutionException("Patient Facility was not found.");
                    }
                    if (site.cvt_resourcepackage == null || site.cvt_resourcepackage.Id == Guid.Empty)
                    {
                        Logger.WriteDebugMessage("Scheduling Package not filled in on this Participating Site.");
                        throw new InvalidPluginExecutionException("Scheduling Package not filled in on this Participating Site.");
                    }
                   
                    var SchedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(RP => RP.Id == site.cvt_resourcepackage.Id);
                    Logger.WriteDebugMessage("ProFacId: " + provFacility.Id + ". PatFacId: " + patFacility.Id);
                    if (provFacility.Id != patFacility.Id)
                    {
                        Logger.WriteDebugMessage("Pat and Pro Facility are different, continue with Facility Approval create/check");
                        //Query to see if this approval already exists
                        var checkForRecord = srv.cvt_facilityapprovalSet.FirstOrDefault(fa => fa.cvt_resourcepackage.Id == site.cvt_resourcepackage.Id &&
                        fa.cvt_providerfacility.Id == provFacility.Id && fa.cvt_patientfacility.Id == patFacility.Id);

                        if (checkForRecord != null)
                        {
                            Logger.WriteDebugMessage("Found existing Facility Approval for this combination of Resource Package/Provider Facility/Patient Facility, no need to create another Facility Approval record.");
                            //break;
                        }
                        else
                        {
                            Logger.WriteDebugMessage("Getting Patient Facility approval teams.");
                            var PatSCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && T.cvt_Facility.Id == patFacility.Id && T.cvt_ServiceType.Id == SchedulingPackage.cvt_specialty.Id);

                            var PatCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == patFacility.Id);

                            Logger.WriteDebugMessage("Getting Provider Facility approval teams.");
                            var ProSCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && T.cvt_Facility.Id == provFacility.Id && T.cvt_ServiceType.Id == SchedulingPackage.cvt_specialty.Id);

                            var ProCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == provFacility.Id);

                        //    var HubCoSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.HubDirector && T.cvt_Facility.Id == site.cvt_facility.Id);

                            //Check if team exists and if there are team members.
                            var teamCheck = checkTeam(PatSCTeam, "Patient Service Chief", patFacility.Name);
                            teamCheck += checkTeam(PatCoSTeam, "Patient Chief of Staff", patFacility.Name);
                            teamCheck += checkTeam(ProSCTeam, "Provider Service Chief", provFacility.Name);
                            teamCheck += checkTeam(ProCoSTeam, "Provider Chief of Staff", provFacility.Name);

                            if (teamCheck != "")
                                throw new InvalidPluginExecutionException(teamCheck);

                            var facilityName = SchedulingPackage.cvt_intraorinterfacility.Value == 917290000? provFacility.Name: provFacility.Name + " -> " + patFacility.Name;

                            var facilityApproval = new cvt_facilityapproval
                            {                                
                                cvt_resourcepackage = site.cvt_resourcepackage,
                                cvt_patientfacility = patFacility,
                                cvt_providerfacility = provFacility,
                                cvt_name = facilityName,
                                cvt_ServiceChiefTeamPatient = new EntityReference(Team.EntityLogicalName, PatSCTeam.Id),
                                cvt_ChiefofStaffTeamPatient = new EntityReference(Team.EntityLogicalName, PatCoSTeam.Id),
                       //         cvt_chiefofstaffteamhubfacility = new EntityReference(Team.EntityLogicalName, HubCoSTeam.Id),

                                cvt_ServiceChiefTeamProvider = new EntityReference(Team.EntityLogicalName, ProSCTeam.Id),
                                cvt_ChiefofStaffTeamProvider = new EntityReference(Team.EntityLogicalName, ProCoSTeam.Id)
                                
                            };

                            OrganizationService.Create(facilityApproval);
                        }
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Detected Intrafacility pairing, do not create Facility Approval record.");
                        //Send Notification Email to SC Team
                        Email intrafacilityNotification = new Email()
                        {
                            Subject = "Notification of Intrafacility " + SchedulingPackage.cvt_specialty.Name + " Telehealth Services to " + site.cvt_site.Name,
                            RegardingObjectId = new EntityReference(cvt_participatingsite.EntityLogicalName, site.Id)
                        };

                        OrganizationService.Create(intrafacilityNotification);
                        Logger.WriteDebugMessage("Created Intrafacility Notification");
                    }
                }
            }
        }

        public string checkTeam(Team thisTeam, string check, string facility)
        {
            var message = "";

            if (thisTeam == null)
                message = check + " Team (at " + facility + ") is missing, it needs to be created. \n";
            else
            {
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
    }
}