using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class ResourcePackageCreatePreStageRunner : PluginRunner
    {
        public ResourcePackageCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for Resource Package Pre Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Ensure the name is correct
        /// </summary>
        public override void Execute()
        {
            Logger.WriteDebugMessage("About to retrieve the derived name.");
            //Check Pre Name against format and update if needed
            var derivedName = CvtHelper.ReturnRecordNameIfChanged(PrimaryEntity.ToEntity<cvt_resourcepackage>(), true, Logger, OrganizationService);
            Logger.WriteDebugMessage("The derivedName came back, it is: " + derivedName.ToString());

            if (!String.IsNullOrEmpty(derivedName))
            {
                Logger.WriteDebugMessage(String.Format("The Resource Package name should be different than {0}, updating it in the CreatePreStage to: {1}.", PrimaryEntity.Attributes["cvt_name"].ToString(), derivedName));
                PrimaryEntity.Attributes["cvt_name"] = (string)derivedName;
                Logger.WriteDebugMessage("New name as read from the PrimaryEntity: " + PrimaryEntity.Attributes["cvt_name"].ToString());
            }
            else
            {
                Logger.WriteDebugMessage("No change made to the name.  The Resource Package name is already correct.");
            }
            VerifyCreate();
            CheckForTeams();
            Logger.WriteDebugMessage("End of PreStageCreate Execute method.");
        }

        //On Create, User must be on the FTC team of the provider facility
        public void VerifyCreate()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var thisRecord = PrimaryEntity.ToEntity<cvt_resourcepackage>();
                if (thisRecord.cvt_specialty == null)
                    throw new InvalidPluginExecutionException("Scheduling Package needs to have a Specialty listed.");
                bool overrideRoleFound = false;
                //Should we create a workaround for System Administrators/App Admins/Field App Admins
                var userRoles = srv.SystemUserRolesSet.Where(r => r.SystemUserId == thisRecord.CreatedBy.Id);
                foreach (SystemUserRoles record in userRoles)
                {
                    var role = srv.RoleSet.FirstOrDefault(ro => ro.Id == record.RoleId);
                    if (role.Name.ToLower() == "System Administrator".ToLower())
                    {
                        overrideRoleFound = true;
                        Logger.WriteDebugMessage("User is a System Administrator.");
                        break;
                    }
                    else if (role.Name.ToLower() == "TMP Application Administrator".ToLower())
                    {
                        overrideRoleFound = true;
                        Logger.WriteDebugMessage("User is TMP Application Administrator.");
                        break;
                    }
                    else if (role.Name.ToLower() == "TMP Field Application Administrator".ToLower())
                    {
                        overrideRoleFound = true;
                        Logger.WriteDebugMessage("User is a TMP Field Application Administrator.");
                        break;
                    }
                }
                var providerFTCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.FTC && T.cvt_Facility.Id == thisRecord.cvt_providerfacility.Id);
                if (providerFTCTeam == null) 
                    throw new InvalidPluginExecutionException("Scheduling Package's Provider Facility needs an existing FTC Team.");
                //search for team membership for this user and 
                var userFTC = srv.TeamMembershipSet.FirstOrDefault(tm => tm.TeamId.Value == providerFTCTeam.Id && tm.SystemUserId.Value == thisRecord.CreatedBy.Id);
                if (userFTC == null)
                {
                    if (overrideRoleFound == false)
                        throw new InvalidPluginExecutionException("Current user must be on the Provider Facility FTC team.");
                    else
                        Logger.WriteDebugMessage("Current user is not on the Provider Facility FTC team, but has an override Role.");
                }
                else
                    Logger.WriteDebugMessage("Correct - Current User is on the Provider Facility FTC Team.");

                if (thisRecord.cvt_hub != null && thisRecord.cvt_hub.Id != Guid.Empty)
                {
                    //Hub SP being created, need to verify that the user is on the hub team, or that they have an override role.
                    var providerHubTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.HubTSAManager && T.cvt_Facility.Id == thisRecord.cvt_hub.Id);
                    if (providerHubTeam == null)
                        throw new InvalidPluginExecutionException("Scheduling Package's Hub Facility needs an existing Hub Team.");
                    //search for team membership for this user and 
                    var userHub = srv.TeamMembershipSet.FirstOrDefault(tm => tm.TeamId.Value == providerHubTeam.Id && tm.SystemUserId.Value == thisRecord.CreatedBy.Id);
                    if (userHub == null)
                    {
                        if (overrideRoleFound == false)
                            throw new InvalidPluginExecutionException("Current user must be on the Facility Hub team.");
                        else
                            Logger.WriteDebugMessage("Current user is not on the Faciltiy Hub team, but has an override Role.");
                    }
                    else
                        Logger.WriteDebugMessage("Correct - Current User is on the Facility Hub Team.");
                }
            }
        }


        //Check for Hub Teams
        public void CheckForTeams()
        {
            Logger.setMethod = "CheckForHubTeams";
            using (var srv = new Xrm(OrganizationService))
            {
                var thisRecord = PrimaryEntity.ToEntity<cvt_resourcepackage>();
                var provFacility = thisRecord.cvt_providerfacility != null ? thisRecord.cvt_providerfacility.Id : Guid.Empty;
                var hub = thisRecord.cvt_hub != null ? thisRecord.cvt_hub.Id : Guid.Empty;
                if (provFacility != null && provFacility != Guid.Empty)
                {
                    Logger.WriteDebugMessage("Getting Provider Facility approval teams.");
                    var provFTCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.FTC && T.cvt_Facility.Id == provFacility);
                    var provSCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && T.cvt_Facility.Id == provFacility && T.cvt_ServiceType.Id ==thisRecord.cvt_specialty.Id);
                    var provCOSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == provFacility);


                    //Check if team exists and if there are team members.
                    var teamCheck = checkTeam(provFTCTeam, "Provider FTC");
                    teamCheck += checkTeam(provSCTeam, "Provider Service Chief");
                    teamCheck += checkTeam(provCOSTeam, "Provider Chief of Staff");

                    if (teamCheck != "")
                        throw new InvalidPluginExecutionException(teamCheck);
                    Logger.WriteDebugMessage("Provider Teams exists.");
                }
                else
                    throw new InvalidPluginExecutionException("Must have Provider Facility listed.");

                //If Group and interfacility, then check for Patient Facility Teams
                if (thisRecord.cvt_groupappointment.HasValue && thisRecord.cvt_groupappointment.Value == true)
                {
                    if (thisRecord.cvt_intraorinterfacility != null && thisRecord.cvt_intraorinterfacility.Value == (int)cvt_resourcepackagecvt_intraorinterfacility.Interfacility)
                    {
                        var patFacility = thisRecord.cvt_patientfacility != null ? thisRecord.cvt_patientfacility.Id : Guid.Empty;
                        if (patFacility != null && patFacility != Guid.Empty)
                        {
                            if (patFacility == provFacility)
                                throw new InvalidPluginExecutionException("Provider and Patient Facilities cannot be the same for an Interfacility Scheduling Package. Please select a different facility or create an Intrafacility Scheduling Package.");

                            Logger.WriteDebugMessage("Getting Patient Facility approval teams.");
                            var patFTCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.FTC && T.cvt_Facility.Id == patFacility);
                            var patSCTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief && T.cvt_Facility.Id == patFacility && T.cvt_ServiceType.Id == thisRecord.cvt_specialty.Id);
                            var patCOSTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff && T.cvt_Facility.Id == patFacility);

                            //Check if team exists and if there are team members.
                            var teamCheck = checkTeam(patFTCTeam, "Patient FTC");
                            teamCheck += checkTeam(patSCTeam, "Patient Service Chief");
                            teamCheck += checkTeam(patCOSTeam, "Patient Chief of Staff");

                            if (teamCheck != "")
                                throw new InvalidPluginExecutionException(teamCheck);
                            Logger.WriteDebugMessage("Patient Teams exists.");
                        }
                        else
                            throw new InvalidPluginExecutionException("Must have Patient Facility listed.");
                    }
                }

                if (hub != null && hub != Guid.Empty)
                {
                    Logger.WriteDebugMessage("Getting Hub Facility approval teams.");
                    var hubDirectorTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.HubDirector && T.cvt_Facility.Id == hub);
                    var hubTSAManagerTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.HubTSAManager && T.cvt_Facility.Id == hub);

                    //Check if team exists and if there are team members.
                    var teamCheck = checkTeam(hubDirectorTeam, "Hub Director");
                    teamCheck += checkTeam(hubTSAManagerTeam, "Hub TSA Manager");

                    if (teamCheck != "")
                        throw new InvalidPluginExecutionException(teamCheck);
                    Logger.WriteDebugMessage("Hub Director and TSA Manager Teams exists.");
                }
            }
        }

        public string checkTeam(Team thisTeam, string check)
        {
            var message = "";

            if (thisTeam == null)
                message = check + " Team is missing, it needs to be created. \n";
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
        #region Additional Interface Methods/Properties
        /// <summary>
        /// Used for Debugging - turns on or off creation of log records for this particular entity
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_ppereview"; }
        }
        #endregion
    }
}