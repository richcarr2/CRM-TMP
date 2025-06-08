using MCSShared;
using MCSUtilities2011;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class McsSystemSettingsCreatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsSystemSettingsCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #endregion

        #region Primary Functionality
        /// <summary>
        /// Execute method is the hook from the abstract runner class, and is what is called by the PluginRunner
        /// </summary>
        /// <remarks>
        /// Depending on the name of the System Settings Record, this plugin runs a bulk update on the appropriate records
        /// </remarks>
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != mcs_setting.EntityLogicalName)
                return;
            Logger.WriteDebugMessage(String.Format("At the top of:{0}", GetType()));
            string name = PrimaryEntity.Attributes["mcs_name"].ToString();

            if (name.ToLower().StartsWith("f2bu"))
            {
                Logger.WriteDebugMessage("Run the F2BU.");
                RunGenericChain(name);
            }
            else if (name.ToLower() == "active settings" || name.ToLower() == "imdataimport")
                return;
            else if (name.Contains('|'))
            {
                var methods = name.Split('|');
                foreach (string method in methods)
                {
                    Logger.WriteDebugMessage(string.Format("Invoking method RunGenericFunction with the parameter: {0}", method));
                    RunGenericFunction(method);
                }
            }
            else
                RunGenericFunction(name);

            DeactivatePluginSettingsRecord(PrimaryEntity);
        }

        /// <summary>
        /// Method to invoke name of actual function
        /// </summary>
        /// <param name="SettingName"></param>
        internal void RunGenericFunction(string SettingName)
        {
            Logger.WriteDebugMessage(string.Format("Starting method RunGenericFunction with the parameter: {0}", SettingName));
            Type type = this.GetType();
            try
            {
                Logger.WriteDebugMessage(string.Format("Validating whether the method '{0}' is available", SettingName));
                Logger.WriteDebugMessage(string.Format("Invoking the method '{0}'", SettingName));

                if (!string.IsNullOrWhiteSpace(SettingName))
                {
                    MethodInfo mi = type.GetMethod(SettingName);
                    if (mi == null)
                        throw new InvalidPluginExecutionException("Unable to Find method named: " + SettingName);
                    else
                        mi.Invoke(this, null);
                }
                Logger.WriteDebugMessage("Ending " + SettingName);
            }
            catch (Exception ex)
            {
                //Update Plugin Settings job with a "- failed at time"
                DateTime myNow = DateTime.Now;

                var name = SettingName + " - Failed at " + myNow.ToString("MM/dd/yy HH:mm:ss");

                if (MethodName != "")
                    name += " on " + MethodName;

                mcs_setting updateSettings = new mcs_setting()
                {
                    Id = PrimaryEntity.Id,
                    mcs_name = name
                };
                OrganizationService.Update(updateSettings);

                Logger.WriteToFile("Failed to invoke Function: " + SettingName + ". " + CvtHelper.BuildExceptionMessage(ex) + ex.StackTrace);
                throw new InvalidPluginExecutionException("Failed to invoke Function: " + SettingName);
            }
        }

        /// <summary>
        /// Check the system settings and either create a message or send an email when the System Settings is done.
        /// </summary>
        /// <param name="success"></param>
        //internal void FinishAlert(Boolean success)
        //{
        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        Logger.setMethod = "FinishAlert";




        //        //Figure out who to send this email to or to create it as a message//or both

        //        var switchVar = false; //true == send email alert
        //        var recipient = "";

        //        //Check for Active Settings configuration
        //        var activeSettings = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings");
        //        if (activeSettings != null)
        //        {
        //            //Get email field and configuration switch
        //            recipient = activeSettings.cvt_URL; //"blank.email.com";
        //            switchVar = (activeSettings.cvt_UseMVI != null) ? activeSettings.cvt_UseMVI.Value : false;
        //        }
        //        else
        //        {
        //            Logger.WriteDebugMessage("Couldn't find Active Settings, defaulting to create message.");
        //        }

        //        //Create message for success or failure.
        //        //Name of plugin and time.
        //        //Duration of plugin running.
        //        //Initiator of the job
        //        String pluginName = PrimaryEntity.Attributes["mcs_name"].ToString();
        //        String pluginResult = ((success == true) ? "Succeeded" : "Failed");
        //        SystemUser pluginInitiator = (SystemUser)PrimaryEntity.Attributes["createdby"];

        //        DateTime pluginStopped = DateTime.Now;
        //        DateTime pluginStarted = new DateTime(PrimaryEntity.Attributes["createdon"]);
        //        String pluginStartedString = PrimaryEntity.Attributes["createdon"].ToString();
        //        String pluginStoppedString = pluginStopped.ToString("MM/dd/yy HH:mm:ss");
        //        TimeSpan pluginDuration = pluginStopped - pluginStarted;


        //        var message = "Plugin name: " + pluginName + "/n/n";
        //        message += "Initiated: " + pluginInitiator + "/n";
        //        message += "Status: " + pluginResult + "/n";
        //        message += "Started: " + pluginStartedString + "/n";
        //        message += "Stopped: " + pluginStoppedString + "/n";
        //        message += "Duration: " + pluginDuration + "/n";


        //        if (switchVar)
        //        {
        //            //Send email
        //            Email emailAlert = new Email()
        //            {
        //                Subject = "",
        //                From = "",
        //                To = (ActivityParty)pluginInitiator,

        //            };

        //            OrganizationService.Create(emailAlert);
        //        }
        //        else
        //        {
        //            //Create Message

        //        }

        //        Logger.WriteDebugMessage("Exiting alert function.");
        //    }
        //}

        /// <summary>
        /// Clean up the current record.
        /// </summary>
        /// <param name="entity"></param>
        internal void DeactivatePluginSettingsRecord(Entity entity)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "DeactivatePluginSettingsRecord";

                //Update Plugin Settings job with a "- failed at time"
                DateTime myNow = DateTime.Now;
                var name = PrimaryEntity.Attributes["mcs_name"].ToString() + " - Successfully completed at " + myNow.ToString("MM/dd/yy HH:mm:ss");

                mcs_setting updateSettings = new mcs_setting()
                {
                    Id = PrimaryEntity.Id,
                    mcs_name = name
                };
                OrganizationService.Update(updateSettings);

                SetStateRequest req = new SetStateRequest();

                //the entity you want to change the state of
                req.EntityMoniker = new EntityReference(entity.LogicalName, entity.Id);

                //Set new state
                req.State = new OptionSetValue((int)mcs_settingState.Inactive);

                //Pick an option from the status reason.
                req.Status = new OptionSetValue(2);

                SetStateResponse resp = (SetStateResponse)OrganizationService.Execute(req);
                Logger.WriteDebugMessage("Deactivated Plugin Settings Record");
            }
        }

        public void TestFailure()
        {
            Logger.setMethod = "TestFailure";
            using (var srv = new Xrm(OrganizationService))
            {
                EntityReferenceCollection RolesToAssociate = new EntityReferenceCollection();
                Logger.WriteDebugMessage("About to error");
                OrganizationService.Associate(SystemUser.EntityLogicalName, Guid.Empty, new Relationship("systemuserprofiles_association"), RolesToAssociate);
            }
        }

        public void TestSuccess()
        {
            Logger.setMethod = "TestSuccess";
            Logger.WriteDebugMessage("TestSuccess, should deactivate.");
        }
        #endregion

        static string MethodName = "";

        /// <summary>
        /// Function to Update different data conditions
        /// </summary>
        public void UpdateTMPData()
        {

            MethodName = "setupTCTTeams";
            setupTCTTeams();

            //MethodName = "UpdateTCTTeamData";
            //UpdateTCTTeamData();

            MethodName = "UpdateBUTeams";
            UpdateBUTeams();

            MethodName = "UpdateTeamSecurityRoles";
            UpdateTeamSecurityRoles();

            MethodName = "UpdateTMPResourceNames";
            UpdateTMPResourceNames();

            MethodName = "UpdateTSSRGAllRequiredNames";
            UpdateTSSRGAllRequiredNames();

            //MethodName = "UpdateTsaSiteUserNames";
            //UpdateTsaSiteUserNames();

        }

        #region Facility to Business Unit level
        /// <summary>
        /// Determine which F2BU method to call and which step is next
        /// </summary>
        public void RunGenericChain(string name)
        {
            var jobs = new Dictionary<int, string>();
            jobs.Add(1, "SaveUserRoles");
            jobs.Add(2, "CreateBUs");
            jobs.Add(3, "AssignFacilityOwnership");
            jobs.Add(4, "UpdateTeam");
            jobs.Add(5, "MoveUsers");
            jobs.Add(6, "FixRecordOwnership");
            jobs.Add(7, "UpdateEquipBUs");
            jobs.Add(8, "Finish");

            var methodName = name.Length == 4 ? jobs[1] : name.Substring(4); //If nothing but f2bu is supplied, then start at the 1st job, otherwise run from name listed

            var currentStepNumber = jobs.ContainsValue(methodName) ? jobs.First(kvp => kvp.Value == methodName).Key : 0;
            if (currentStepNumber == 0)
            {
                Logger.WriteToFile("Unable to find step with name: " + methodName);
                return;
            }

            RunGenericFunction(methodName);

            var nextStepText = string.Empty;
            jobs.TryGetValue(currentStepNumber + 1, out nextStepText);
            if (string.IsNullOrEmpty(nextStepText))
            {
                Logger.WriteDebugMessage("Finished Final Step");
                return;
            }
            mcs_setting nextSetting = new mcs_setting()
            {
                mcs_name = "f2bu" + nextStepText
            };
            Logger.WriteDebugMessage(string.Format("Finished step {0}.  Beginning step {1}: {2}", currentStepNumber, currentStepNumber + 1, nextStepText));
            OrganizationService.Create(nextSetting);
        }

        /// <summary>
        /// F2BU: marks the "save security roles" bit field as true so that the user plugin fires copying all of their 
        /// roles into a concatenated string field, which is later used to copy security roles as a user is moved from one BU to another
        /// </summary>
        /// <remarks>Step 1</remarks>
        public void SaveUserRoles()
        {
            Logger.setMethod = "SaveUserRoles";
            Logger.WriteDebugMessage("Starting method");
            using (var srv = new Xrm(OrganizationService))
            {
                //TO-DO, Check to see if we can filter out users with no roles
                //Query for all Users for that Facility and re-assign the security roles
                var completeUserSet = srv.SystemUserSet.ToList();
                Logger.WriteDebugMessage("Starting user security role save. Users found: " + completeUserSet.Count);
                int counter = 0;
                foreach (SystemUser foundUser in completeUserSet)
                {
                    if (foundUser.FullName == "INTEGRATION" || foundUser.FullName == "SYSTEM")
                        break;
                    //Save Security Roles through User plugin
                    SystemUser saveRoles = new SystemUser()
                    {
                        Id = foundUser.Id,
                        cvt_SaveSecurityRoles = true
                    };
                    try
                    {
                        OrganizationService.Update(saveRoles);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(string.Format("Unable to Update User {0}.  Exception Message: {1}", foundUser.FullName, ex.Message));
                    }
                }
                Logger.WriteDebugMessage(String.Format("F2BU: Saved updated {0}/{1} users.", counter, completeUserSet.ToList().Count));
            }
            Logger.WriteDebugMessage("Finished method");
        }

        /// <summary>
        /// F2BU: Create BUs from Facilities
        /// </summary>
        /// <remarks>step 2</remarks>
        public void CreateBUs()
        {
            Logger.setMethod = "CreateBUs";
            Logger.WriteDebugMessage("Starting method");

            using (var srv = new Xrm(OrganizationService))
            {
                var completeFacilitySet = srv.mcs_facilitySet;
                int counterCreate = 0;
                int counterUpdate = 0;
                foreach (mcs_facility currentFacility in completeFacilitySet)
                {
                    //Create new Business Unit mirroring the Facility
                    BusinessUnit newFacilityBU = new BusinessUnit()
                    {
                        Name = currentFacility.mcs_name,
                        ParentBusinessUnitId = currentFacility.mcs_BusinessUnitId
                    };
                    Guid newBUid = Guid.Empty;
                    try
                    {
                        newBUid = OrganizationService.Create(newFacilityBU);
                        counterCreate++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(string.Format("Unable to create BU {0}.  Exception message: {1}", newFacilityBU.Name, ex.Message));
                        continue;
                    }
                    EntityReference newlyCreatedFacilityBU = new EntityReference(BusinessUnit.EntityLogicalName, newBUid);

                    //Update Facility to reference prior VISN
                    mcs_facility updateFacility = new mcs_facility()
                    {
                        Id = currentFacility.Id,
                        //mcs_VISN = currentFacility.mcs_BusinessUnitId, //parent BU
                        mcs_BusinessUnitId = newlyCreatedFacilityBU
                    };
                    updateFacility.Attributes["mcs_visn"] = currentFacility.mcs_BusinessUnitId;
                    try
                    {
                        OrganizationService.Update(updateFacility);
                        counterUpdate++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(String.Format("Unable to Update Facility Reference to New BU {0}.  Exception message: {1}", currentFacility.mcs_name, ex.Message));
                    }
                }
                Logger.WriteDebugMessage(String.Format("F2BU: Found Facilities: {0}. Created {1} Business Units. Updated {2} Facilities.", completeFacilitySet.ToList().Count, counterCreate, counterUpdate));
            }
            Logger.WriteDebugMessage("Finished method");
        }

        /// <summary>
        /// F2BU: assign ownership of Facility to new BU teams, populate VISN field from BU and update BU to point to facility-level BU (instead of VISN).  
        /// </summary>
        /// <remarks>step 3</remarks>
        public void AssignFacilityOwnership()
        {
            Logger.setMethod = "AssignFacilityOwnership";
            Logger.WriteDebugMessage("Starting method");
            using (var srv = new Xrm(OrganizationService))
            {
                var completeFacilitySet = srv.mcs_facilitySet;
                int counterReassign = 0;
                int counterUpdate = 0;
                foreach (mcs_facility currentFacility in completeFacilitySet)
                {
                    try
                    {
                        //Commit an Assign Request to update the ownership of the Facility to the new Team of the BU
                        var newBUTeam = srv.TeamSet.FirstOrDefault(t => t.Name == currentFacility.mcs_name && t.BusinessUnitId.Id == currentFacility.mcs_BusinessUnitId.Id);
                        if (newBUTeam != null)
                        {
                            EntityReferenceCollection RolesToAssociate = new EntityReferenceCollection();
                            //Get security roles
                            var visnTeamRole = srv.RoleSet.FirstOrDefault(sr => sr.BusinessUnitId.Id == currentFacility.mcs_BusinessUnitId.Id && sr.Name == "TMP BU Team");
                            RolesToAssociate.Add(new EntityReference(Role.EntityLogicalName, visnTeamRole.Id));
                            var tssUserRole = srv.RoleSet.FirstOrDefault(sr => sr.BusinessUnitId.Id == currentFacility.mcs_BusinessUnitId.Id && sr.Name == "TMP User");
                            RolesToAssociate.Add(new EntityReference(Role.EntityLogicalName, tssUserRole.Id));

                            //Remove the roles if the ream already has them, so there is no attempt to insert a duplicate key
                            var existingRoles = srv.TeamRolesSet.Where(tr => tr.TeamId == newBUTeam.Id && (tr.RoleId == visnTeamRole.Id || tr.RoleId == tssUserRole.Id)).ToList();
                            foreach (var teamRole in existingRoles)
                            {
                                RolesToAssociate.Remove(new EntityReference(Role.EntityLogicalName, teamRole.RoleId.Value));
                            }

                            //Assign security roles to the team
                            OrganizationService.Associate(Team.EntityLogicalName, newBUTeam.Id, new Relationship(Team.EntityLogicalName.ToLower() + "roles_association"), RolesToAssociate);


                            EntityReference newlyCreatedFacilityBUTeam = new EntityReference()
                            {
                                Id = newBUTeam.Id,
                                LogicalName = Team.EntityLogicalName
                            };

                            AssignRequest assignFacility = new AssignRequest()
                            {
                                Assignee = newlyCreatedFacilityBUTeam,
                                Target = new EntityReference()
                                {
                                    Id = currentFacility.Id,
                                    LogicalName = mcs_facility.EntityLogicalName
                                }
                            };
                            OrganizationService.Execute(assignFacility);
                            counterReassign++;
                        }

                        //Update VISN and BU fields for facility if the BU is still pointing to the VISN instead of the facility
                        if (currentFacility.mcs_BusinessUnitId != null && currentFacility.mcs_BusinessUnitId.Name.Contains("VISN"))
                        {
                            var updateFacility = new mcs_facility()
                            {
                                Id = currentFacility.Id,
                                mcs_VISN = currentFacility.mcs_BusinessUnitId
                            };
                            if (newBUTeam != null)
                                updateFacility.mcs_BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, newBUTeam.Id);
                            OrganizationService.Update(updateFacility);
                            counterUpdate++;
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.WriteDebugMessage(string.Format("Error Reassigning ownership of facility {0}.  Error message is: {1}",
                            currentFacility.mcs_name, CvtHelper.BuildExceptionMessage(ex)));
                    }
                }
                Logger.WriteDebugMessage(String.Format("F2BU: Found Facilities: {0}. Reassigned ownership of {1} Facilities.  Copied BU into VISN for {2} Facilities",
                                        completeFacilitySet.ToList().Count, counterReassign, counterUpdate));
            }
            Logger.WriteDebugMessage("Finished method");
        }

        /// <summary>
        /// F2BU: assign teams from VISN to Facility
        /// </summary>
        /// <remarks>step 4</remarks>
        public void UpdateTeam()
        {
            Logger.setMethod = "UpdateTeam";
            Logger.WriteDebugMessage("Starting method");
            using (var srv = new Xrm(OrganizationService))
            {
                var completeFacilitySet = srv.mcs_facilitySet;
                int counterTeams = 0;
                int counterTeamTotal = 0;
                foreach (mcs_facility currentFacility in completeFacilitySet)
                {
                    //Query for all Teams for that Facility and re-assign the security roles
                    var facilityTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == currentFacility.Id);
                    Logger.WriteDebugMessage("Starting team - security role updates. (facility) " + currentFacility.mcs_name + " teams found: " + facilityTeams.ToList().Count);

                    EntityReference newlyCreatedFacilityBU = new EntityReference()
                    {
                        Id = currentFacility.mcs_BusinessUnitId.Id,
                        LogicalName = BusinessUnit.EntityLogicalName
                    };
                    counterTeamTotal += facilityTeams.ToList().Count;
                    foreach (Team foundTeam in facilityTeams)
                    {
                        if (!string.IsNullOrEmpty(foundTeam.Description) && foundTeam.Description.Contains("default"))
                            continue;
                        //Set Team to new BU
                        try
                        {
                            SetParentTeamRequest updateTeam = new SetParentTeamRequest()
                            {
                                TeamId = foundTeam.Id,
                                BusinessId = newlyCreatedFacilityBU.Id
                            };
                            OrganizationService.Execute(updateTeam);

                            var updateTeamVisn = new Team()
                            {
                                Id = foundTeam.Id,
                                mcs_VISN = currentFacility.mcs_VISN
                            };
                            OrganizationService.Update(updateTeamVisn);

                            //Re-add security roles that are now dropped
                            int addedRoles, removedRoles;
                            CvtHelper.UpdateSecurityRoles(foundTeam.Id, Team.EntityLogicalName, OrganizationService, Logger, out addedRoles, out removedRoles);
                            counterTeams++;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile(string.Format("Failed to update team with facility: {0}.  Error: {1}", foundTeam.Name, CvtHelper.BuildExceptionMessage(ex)));
                        }
                    }
                }

                //Next get the site teams and move their BUs to the facility for the site team (no facility currently listed) and add the facility for that site
                var teamsWithoutFacilities = srv.TeamSet.Where(t => t.cvt_Facility == null).ToList();
                foreach (var team in teamsWithoutFacilities)
                {
                    if (team.Name.Contains("VISN"))
                    {
                        var updateTeam = new Team()
                        {
                            Id = team.Id,
                            mcs_VISN = team.BusinessUnitId
                        };
                        try
                        {
                            OrganizationService.Update(updateTeam);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile(string.Format("Unable to Update VISN Team: {0}.  Error: {1}", team.Name, CvtHelper.BuildExceptionMessage(ex)));
                        }
                        continue;
                    }
                    var facility = srv.mcs_facilitySet.FirstOrDefault(f => f.mcs_name == team.Name);
                    if (facility == null)
                    {
                        var site = srv.mcs_siteSet.FirstOrDefault(s => s.mcs_name == team.Name);
                        if (site != null && site.mcs_FacilityId != null)
                        {
                            var sitesFacility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == site.mcs_FacilityId.Id);

                            var facilityBU = srv.BusinessUnitSet.FirstOrDefault(BU => BU.Name == sitesFacility.mcs_name);
                            if (facilityBU != null)
                            {
                                Team updateTeam = new Team()
                                {
                                    Id = team.Id,
                                    cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, sitesFacility.Id)
                                };
                                if (sitesFacility.mcs_VISN != null)
                                    updateTeam.mcs_VISN = sitesFacility.mcs_VISN;

                                try
                                {
                                    OrganizationService.Update(updateTeam);

                                    SetParentTeamRequest reassignTeamBU = new SetParentTeamRequest()
                                    {
                                        TeamId = team.Id,
                                        BusinessId = facilityBU.Id
                                    };
                                    OrganizationService.Execute(reassignTeamBU);

                                    //Re-add security roles that are now dropped
                                    int addedRoles, removedRoles;
                                    CvtHelper.UpdateSecurityRoles(team.Id, Team.EntityLogicalName, OrganizationService, Logger, out addedRoles, out removedRoles);
                                    counterTeams++;
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteToFile(string.Format("Failed to Update team without Facility: {0}.  Error: {1}", updateTeam.Name, CvtHelper.BuildExceptionMessage(ex)));
                                }
                            }
                        }
                        else
                            Logger.WriteToFile(String.Format("Unable to retrieve facility for team {0}, skipping change BUs", team.Name));
                    }
                    else //For system created Facility BU teams - backfill the Facility field
                    {
                        Team updateTeam = new Team()
                        {
                            Id = team.Id,
                            cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, facility.Id),
                        };
                        OrganizationService.Update(updateTeam);
                    }
                }
                Logger.WriteDebugMessage(String.Format("F2BU: Found Teams: {0}. Reassigned ownership of {1} Teams.", counterTeamTotal, counterTeams));
            }
            Logger.WriteDebugMessage("Finished method");
        }

        /// <summary>
        /// F2BU: Move users; Update user; restore user security role
        /// </summary>
        /// <remarks>step 5</remarks>
        public void MoveUsers()
        {
            Logger.setMethod = "UpdateUsers";
            Logger.WriteDebugMessage("Starting method");
            using (var srv = new Xrm(OrganizationService))
            {
                var completeFacilitySet = srv.mcs_facilitySet;
                int counterTeams = 0;
                int counterTeamTotal = 0;
                foreach (mcs_facility currentFacility in completeFacilitySet)
                {
                    //Query for all Users for that Facility and re-assign the security roles
                    var facilityUsers = srv.SystemUserSet.Where(u => u.cvt_facility.Id == currentFacility.Id);
                    if (facilityUsers != null)
                    {
                        EntityReference newlyCreatedFacilityBU = new EntityReference()
                        {
                            Id = currentFacility.mcs_BusinessUnitId.Id,
                            LogicalName = BusinessUnit.EntityLogicalName
                        };
                        Logger.WriteDebugMessage("Starting user - security role updates. (facility) " + currentFacility.mcs_name + " users found: " + facilityUsers.ToList().Count);
                        foreach (SystemUser foundUser in facilityUsers)
                        {
                            //Set Users to new BU
                            SetBusinessSystemUserRequest updateUser = new SetBusinessSystemUserRequest()
                            {
                                UserId = foundUser.Id,
                                BusinessId = newlyCreatedFacilityBU.Id,
                                ReassignPrincipal = new EntityReference(foundUser.LogicalName, foundUser.Id)
                            };
                            try
                            {
                                OrganizationService.Execute(updateUser);
                                var user = new SystemUser()
                                {
                                    Id = foundUser.Id,
                                    mcs_VISN = currentFacility.mcs_VISN
                                };

                                OrganizationService.Update(user);
                                //Re-add security roles that are now dropped
                                int addedRoles, removedRoles;
                                CvtHelper.UpdateSecurityRoles(foundUser.Id, foundUser.LogicalName, OrganizationService, Logger, out addedRoles, out removedRoles);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteToFile(string.Format("Failed to update user {0}, error: {1}", foundUser.FullName, CvtHelper.BuildExceptionMessage(ex)));
                            }
                        }
                    }
                    else
                        Logger.WriteDebugMessage("No Users were found at this facility: " + currentFacility.mcs_name);
                }
                Logger.WriteDebugMessage(String.Format("F2BU: Found Users: {0}. Reassigned ownership of {1} Teams.", counterTeamTotal, counterTeams));
            }
            Logger.WriteDebugMessage("Finished method");
        }

        /// <summary>
        /// Updates the Business Unit of All Equipment records
        /// </summary>
        public void UpdateEquipBUs()
        {
            Logger.WriteDebugMessage("Updating BUs on all Equipment Records");
            var updatedCount = 0;
            using (var srv = new Xrm(OrganizationService))
            {
                var equipmentRecords = srv.EquipmentSet.ToList();
                foreach (var equipment in equipmentRecords)
                {
                    try
                    {
                        //sequence - Equipment --> TSS Resource --> TSS Site --> Facility --> BU
                        var resourceId = equipment.mcs_relatedresource != null ? equipment.mcs_relatedresource.Id : Guid.Empty;
                        if (resourceId != Guid.Empty)
                        {
                            var resource = OrganizationService.Retrieve(mcs_resource.EntityLogicalName, resourceId, new ColumnSet(true)).ToEntity<mcs_resource>();
                            var tssSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == resource.mcs_RelatedSiteId.Id);
                            var facility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == tssSite.mcs_FacilityId.Id);
                            var facilityBU = facility.mcs_VISN;
                            //var facilityBU = srv.BusinessUnitSet.FirstOrDefault(bu => bu.Name == tssSite.mcs_FacilityId.Name);
                            if (facilityBU == null)
                                Logger.WriteToFile("No Facility Business Unit Found");
                            if (facilityBU != null && facilityBU.Id != equipment.BusinessUnitId.Id)
                            {
                                SetBusinessEquipmentRequest req = new SetBusinessEquipmentRequest()
                                {
                                    BusinessUnitId = facilityBU.Id,
                                    EquipmentId = equipment.Id
                                };
                                var resp = OrganizationService.Execute(req);
                                updatedCount++;
                            }

                        }
                        else
                            Logger.WriteToFile(string.Format("Equipment not linked to a resource - orphaned equipment record: {0}", equipment.Id));

                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile("Unable to update equipment: " + ex.Message);
                    }
                }
                Logger.WriteDebugMessage(string.Format("Equipment BUs updated {0}/{1}", updatedCount, equipmentRecords.Count));
            }
        }

        /// <summary>
        /// F2BU: Finished Facility to Business Unit Chain - just outputs that it finished for logging purposes
        /// </summary>
        /// <remarks>step 8</remarks>
        public void Finish()
        {
            Logger.WriteDebugMessage("Finished Chain");
        }
        #endregion

        #region Send Emails
        public void BulkEmailProviderPreferenceAction()
        {
            Logger.setMethod = "BulkEmailProviderPreferenceAction";
            Logger.WriteGranularTimingMessage("Starting");
            using (var srv = new Xrm(OrganizationService))
            {
                int count = 0;
                int errorCount = 0;
                var activeProviderSet = srv.SystemUserSet.Where(s => s.cvt_type.Value == (int)SystemUsercvt_type.ClinicianProvider && s.IsDisabled == false);
                foreach (SystemUser prov in activeProviderSet)
                {
                    try
                    {
                        var providerAP = new ActivityParty
                        {
                            PartyId = new EntityReference(SystemUser.EntityLogicalName, prov.Id)
                        };

                        Email provPreferenceEmail = new Email()
                        {
                            Subject = "Action Required: Update Your Provider Preferences in TMP",
                            To = new ActivityParty[] { providerAP }
                        };

                        OrganizationService.Create(provPreferenceEmail);
                        Logger.WriteDebugMessage("Provider Preference Email Created for " + prov.FullName);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Logger.WriteDebugMessage("Error: " + ex.InnerException);
                    }
                }
                Logger.WriteDebugMessage("Number of Provider Preference emails created: " + count);
                Logger.WriteDebugMessage("Number of Provider Preference emails errored: " + errorCount);
            }
        }
        #endregion
        #region Scheduler Teams
        /// <summary>
        /// Call this function to setup Scheduler Teams for Facilities where Users are affilitated and have a 'TMP Scheduler' role
        /// </summary>
        public void setupSchedulingTeams()
        {
            Logger.setMethod = "setupSchedulingTeams";
            Logger.WriteGranularTimingMessage("Starting setupSchedulingTeams");
            using (var srv = new Xrm(OrganizationService))
            {
                //LINQ Query to get all Users with TMP Scheduler Type
                var schedulerUsers = (from u in srv.SystemUserSet
                                      join ur in srv.SystemUserRolesSet on u.Id equals ur.SystemUserId.Value
                                      join r in srv.RoleSet on ur.RoleId equals r.RoleId
                                      where r.Name.Contains("TMP Scheduler")
                                      where u.cvt_facility != null
                                      where u.IsDisabled.Value != true
                                      select new
                                      {
                                          u.Id,
                                          u.cvt_facility
                                      }).ToList();

                var facilities = srv.mcs_facilitySet.ToList();
                foreach (var fac in facilities)
                {
                    Logger.WriteDebugMessage("Beginning Loop for " + fac.mcs_name);
                    var thisFacilitySchedulers = new List<Guid>();

                    //Filter Scheduler type users from LINQ query to only use this facility
                    foreach (var user in schedulerUsers)
                    {
                        if (user.cvt_facility != null && user.cvt_facility.Id == fac.Id)
                            thisFacilitySchedulers.Add(user.Id);
                    }
                    Guid teamId = new Guid();
                    var existingTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == fac.Id && t.cvt_Type != null && t.cvt_Type.Value == 917290005);
                    if (existingTeam == null)
                        teamId = CreateSchedulerTeam(fac);
                    else
                        teamId = existingTeam.Id;

                    //If there are schedulers for this facility, then go ahead and add them to the Scheduler Team
                    if (thisFacilitySchedulers.Count > 0)
                    {
                        AddMembersTeamRequest amtr = new AddMembersTeamRequest()
                        {
                            TeamId = teamId,
                            MemberIds = thisFacilitySchedulers.ToArray(),
                        };
                        try
                        {
                            OrganizationService.Execute(amtr);
                            Logger.WriteDebugMessage("Team Members Added to " + fac.mcs_name);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Unable to Add all team members, looping through individual team members: " + ex.Message);
                            foreach (var user in thisFacilitySchedulers)
                            {
                                try
                                {
                                    AddMembersTeamRequest req = new AddMembersTeamRequest()
                                    {
                                        TeamId = teamId,
                                        MemberIds = new Guid[1] { user }
                                    };
                                    OrganizationService.Execute(req);
                                    Logger.WriteDebugMessage("Added individual Member to Scheduler Team for " + fac.mcs_name);
                                }
                                catch (Exception ex2)
                                {
                                    Logger.WriteToFile(String.Format("Failed to add individual member {0} : {1}", user, ex2.Message));
                                }
                            }
                        }
                    }
                }
                Logger.WriteGranularTimingMessage("Finished setupSchedulingTeams");
                Logger.WriteDebugMessage("Scheduler Team Setup Complete");
            }
        }

        /// <summary>
        /// Creates a new Team with the passed in Facility, it is typed Scheduler
        /// </summary>
        /// <param name="facility"></param>
        public Guid CreateSchedulerTeam(mcs_facility facility)
        {
            var team = new Team()
            {
                Name = String.Format("Scheduler Team for {0} ({1})", facility.mcs_name, facility.mcs_StationNumber),
                cvt_Type = new OptionSetValue(917290005),
                BusinessUnitId = facility.mcs_BusinessUnitId,
                cvt_Facility = new EntityReference() { Id = facility.Id, Name = facility.mcs_name, LogicalName = facility.LogicalName }
            };
            return OrganizationService.Create(team);
        }

        #endregion

        #region TCT Teams
        /// <summary>
        /// Updates TCT Team to Staff
        /// </summary>
        public void UpdateStaff()
        {
            Logger.setMethod = "UpdateStaff";
            Logger.WriteGranularTimingMessage("Starting UpdateStaff");
            using (var srv = new Xrm(OrganizationService))
            {
                var countTeams = 0;
                var allTeams = 0;
                var failedTeams = 0;

                //Update all Active MTSAs
                var tctTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.Staff).ToList();
                foreach (Team record in tctTeams)
                {
                    allTeams++;
                    try
                    {
                        var team = srv.TeamSet.FirstOrDefault(t => t.Id == record.Id);
                        if (team != null)
                        {
                            Team updateTeam = new Team()
                            {
                                Id = record.Id,
                                Name = CvtHelper.DeriveName(record, false, Logger, OrganizationService)
                            };

                            if (updateTeam.Name != "")
                            {
                                OrganizationService.Update(updateTeam);
                                countTeams++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile("Failed to update Team. Name: " + record.Name + ". GUID: " + record.Id + ". Error: " + ex.Message);
                        failedTeams++;
                    }
                }

                Logger.WriteToFile(String.Format("Updated {0}/{1} Teams.", countTeams, allTeams));
                Logger.WriteToFile(String.Format("Failed to update {0} Teams.", failedTeams));
            }

            Logger.WriteGranularTimingMessage("Finished UpdateStaff");
            Logger.WriteDebugMessage("UpdateStaff Complete");
        }
        /// <summary>
        /// Call this function to setup TCT Teams for TMP Sites where Users are affilitated and are typed 'TCT'
        /// </summary>
        public void setupTCTTeams()
        {
            Logger.setMethod = "setupTCTTeams";
            Logger.WriteGranularTimingMessage("Starting setupTCTTeams");
            using (var srv = new Xrm(OrganizationService))
            {
                //LINQ Query to get all Users with TMP Scheduler Type
                var tctUsers = (from u in srv.SystemUserSet
                                where u.cvt_site != null
                                where u.cvt_type.Value == (int)SystemUsercvt_type.TCTStaff
                                where u.IsDisabled.Value != true
                                select new
                                {
                                    u.Id,
                                    u.cvt_site
                                }).ToList();

                var sites = srv.mcs_siteSet.Where(s => s.statecode == mcs_siteState.Active).ToList();
                foreach (var site in sites)
                {
                    Logger.WriteDebugMessage("Beginning Loop for " + site.mcs_name);
                    var thisSiteTCTs = new List<Guid>();

                    //Filter TCT type users from LINQ query to only use this site
                    foreach (var user in tctUsers)
                    {
                        if (user.cvt_site != null && user.cvt_site.Id == site.Id)
                            thisSiteTCTs.Add(user.Id);
                    }
                    Guid teamId = new Guid();
                    var existingTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite.Id == site.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.Staff);
                    if (existingTeam == null)
                        teamId = CreateTCTTeam(site, OrganizationService, Logger);
                    else
                        teamId = existingTeam.Id;

                    //Update TMP Site with this team
                    mcs_site updateSiteRecord = new mcs_site()
                    {
                        Id = site.Id,
                        cvt_tctteam = new EntityReference()
                        {
                            Id = teamId,
                            LogicalName = Team.EntityLogicalName
                        }
                    };


                    try
                    {
                        if (teamId != Guid.Empty)
                        {
                            OrganizationService.Update(updateSiteRecord);
                            Logger.WriteDebugMessage(String.Format("Updated TMP Site {0} with TCT Team.", site.mcs_name));
                        }
                        else
                            Logger.WriteDebugMessage("No TCT Team found/created for Site: " + site.mcs_name + ". Check the TMP Site's ownership.");
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteDebugMessage("Error When Updating TMP Site. Message: " + ex.InnerException.Message);
                    }



                    //If there are TCTs for this site, then go ahead and add them to the TCT Team
                    if (thisSiteTCTs.Count > 0)
                    {
                        AddMembersTeamRequest amtr = new AddMembersTeamRequest()
                        {
                            TeamId = teamId,
                            MemberIds = thisSiteTCTs.ToArray(),
                        };
                        try
                        {
                            OrganizationService.Execute(amtr);
                            Logger.WriteDebugMessage("Team Members Added to " + site.mcs_name);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Unable to Add all team members, looping through individual team members: " + ex.Message);
                            foreach (var user in thisSiteTCTs)
                            {
                                try
                                {
                                    AddMembersTeamRequest req = new AddMembersTeamRequest()
                                    {
                                        TeamId = teamId,
                                        MemberIds = new Guid[1] { user }
                                    };
                                    OrganizationService.Execute(req);
                                    Logger.WriteDebugMessage("Added individual Member to TCT Team for " + site.mcs_name);
                                }
                                catch (Exception ex2)
                                {
                                    Logger.WriteToFile(String.Format("Failed to add individual member {0} : {1}", user, ex2.Message));
                                }
                            }
                        }
                    }
                }
                Logger.WriteGranularTimingMessage("Finished setupTCTTeams");
                Logger.WriteDebugMessage("TCT Team Setup Complete");
            }
        }

        /// <summary>
        /// Creates a new Team with the passed in site, it is typed TCT
        /// </summary>
        /// <param name="site"></param>
        public static Guid CreateTCTTeam(mcs_site site, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("starting SystemSettings.CreateTCTTeam method");
                Guid newTeam = new Guid();
                var team = new Team()
                {
                    Name = String.Format("TCT Team for {0} ({1})", site.mcs_name, site.mcs_StationNumber),
                    cvt_Type = new OptionSetValue((int)Teamcvt_Type.Staff),
                    BusinessUnitId = null,
                    cvt_TMPSite = new EntityReference() { Id = site.Id, Name = site.mcs_name, LogicalName = site.LogicalName },
                    mcs_VISN = site.mcs_BusinessUnitId
                };

                //Get the Facility BU
                if (site.OwningTeam != null)
                {

                    var OwningTeam = srv.TeamSet.FirstOrDefault(t => t.Id == site.OwningTeam.Id);
                    if (OwningTeam != null)
                    {
                        Logger.WriteDebugMessage("Owning team result returned. Using that record's BU.");
                        EntityReference FacilityBU = new EntityReference()
                        {
                            LogicalName = BusinessUnit.EntityLogicalName,
                            Name = OwningTeam.BusinessUnitId.Name,
                            Id = OwningTeam.BusinessUnitId.Id
                        };

                        team.BusinessUnitId = FacilityBU;
                    }
                    else
                        Logger.WriteDebugMessage("No owning team found based on guid. looking for BU elsewhere.");
                }
                else
                    Logger.WriteDebugMessage("site.OwningTeam is null, looking for BU elsewhere.");


                if (team.BusinessUnitId == null)
                {
                    Logger.WriteDebugMessage("Looking for BU elsewhere.");

                    var siteTeam = srv.TeamSet.FirstOrDefault(t => t.Name == site.mcs_name);

                    if (siteTeam != null)
                    {
                        EntityReference foundFacilityBU = new EntityReference()
                        {
                            LogicalName = BusinessUnit.EntityLogicalName,
                            Name = siteTeam.BusinessUnitId.Name,
                            Id = siteTeam.BusinessUnitId.Id
                        };
                        team.BusinessUnitId = foundFacilityBU;
                    }
                    else
                    {
                        Logger.WriteDebugMessage("No Site Team found, no TCT Team created.");
                        return newTeam;
                    }
                }
                try
                {
                    var teamNameExists = srv.TeamSet.FirstOrDefault(t => t.Name == team.Name);
                    if (teamNameExists != null)
                    {
                        Logger.WriteDebugMessage("Team with that name already exists, using that one instead of creating a new one. Team Name: " + teamNameExists.Name);
                        return teamNameExists.Id;
                    }
                    newTeam = OrganizationService.Create(team);
                    Logger.WriteDebugMessage("TCT Team was successfully created. Name: " + team.Name);
                }
                catch (Exception ex)
                {
                    Logger.WriteDebugMessage("Error When Creating TCT Team. Message: " + ex.InnerException.Message);
                }

                return newTeam;
            }
        }

        /// <summary>
        //  TSA / MTSA entities are Deprecated
        /// Updates all MTSAs and TSAs with the corresponding TCTs teams.
        /// </summary>
        //public void UpdateTCTTeamData()
        //{
        //    Logger.setMethod = "UpdateTCTTeamData";
        //    Logger.WriteGranularTimingMessage("Starting UpdateTCTTeamData");
        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        var countMtsas = 0;
        //        var allMtsas = 0;
        //        var failedMtsas = 0;

        //        var countTsas = 0;
        //        var allTsas = 0;
        //        var failedTsas = 0;



        //        //Update all Active MTSAs 
        //        var activeMtsas = srv.cvt_mastertsaSet.Where(s => s.statecode == cvt_mastertsaState.Active).ToList();
        //        foreach (cvt_mastertsa record in activeMtsas)
        //        {
        //            allMtsas++;
        //            try
        //            {
        //                var proTCT = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite.Id == record.cvt_RelatedSiteid.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.Staff);
        //                if (proTCT != null)
        //                {
        //                    cvt_mastertsa updateMTSA = new cvt_mastertsa()
        //                    {
        //                        Id = record.Id,
        //                        cvt_providersitetctteam = new EntityReference()
        //                        {
        //                            Id = proTCT.Id,
        //                            Name = proTCT.Name,
        //                            LogicalName = Team.EntityLogicalName
        //                        }
        //                    };
        //                    if (record.cvt_providersitetctteam == null || record.cvt_providersitetctteam.Id != updateMTSA.cvt_providersitetctteam.Id)
        //                    {
        //                        OrganizationService.Update(updateMTSA);
        //                        countMtsas++;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.WriteToFile("Failed to update MTSA. Name: " + record.cvt_name + ". GUID: " + record.Id + ". Error: " + ex.Message);
        //                failedMtsas++;
        //            }
        //        }

        //        //Update all Active TSAs
        //        var activeTsas = srv.mcs_servicesSet.Where(s => s.statecode == mcs_servicesState.Active).ToList();
        //        foreach (mcs_services record in activeTsas)
        //        {
        //            var isTsaUpdate = false;
        //            allTsas++;

        //            try
        //            {
        //                mcs_services updateTSA = new mcs_services()
        //                {
        //                    Id = record.Id
        //                };

        //                var proTCT = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite.Id == record.cvt_relatedprovidersiteid.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.Staff);
        //                if (proTCT != null)
        //                {
        //                    if (record.cvt_providersitetctteam == null || record.cvt_providersitetctteam.Id != proTCT.Id)
        //                    {
        //                        updateTSA.cvt_providersitetctteam = new EntityReference()
        //                        {
        //                            Id = proTCT.Id,
        //                            Name = proTCT.Name,
        //                            LogicalName = Team.EntityLogicalName
        //                        };
        //                        isTsaUpdate = true;
        //                    }
        //                }
        //                //if GROUP or HM Pat TCT Team is not null, update to null
        //                if ((record.cvt_groupappointment != null && record.cvt_groupappointment.Value == true) || (record.cvt_Type != null && record.cvt_Type.Value == true))
        //                {
        //                    if (record.cvt_patientsitetctteam != null)
        //                    {
        //                        updateTSA.cvt_patientsitetctteam = null;
        //                        isTsaUpdate = true;
        //                    }
        //                }
        //                else //Not Group or H/M
        //                {
        //                    var patTCT = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite.Id == record.cvt_relatedpatientsiteid.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.Staff);
        //                    if (patTCT != null)
        //                    {
        //                        if (record.cvt_patientsitetctteam == null || record.cvt_patientsitetctteam.Id != patTCT.Id)
        //                        {
        //                            updateTSA.cvt_patientsitetctteam = new EntityReference()
        //                            {
        //                                Id = patTCT.Id,
        //                                Name = patTCT.Name,
        //                                LogicalName = Team.EntityLogicalName
        //                            };
        //                            isTsaUpdate = true;
        //                        }
        //                    }
        //                }

        //                if (isTsaUpdate)
        //                {
        //                    OrganizationService.Update(updateTSA);
        //                    countTsas++;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.WriteToFile("Failed to update TSA. Name: " + record.mcs_name + ". GUID: " + record.Id + ". Error: " + ex.Message);
        //                failedTsas++;
        //            }
        //        }

        //        Logger.WriteToFile(String.Format("Updated {0}/{1} MTSAs.", countMtsas, allMtsas));
        //        Logger.WriteToFile(String.Format("Updated {0}/{1} TSAs.", countTsas, allTsas));
        //        Logger.WriteToFile(String.Format("Failed to update {0} MTSAs.", failedMtsas));
        //        Logger.WriteToFile(String.Format("Failed to update {0} TSAs.", failedTsas));
        //    }

        //    Logger.WriteGranularTimingMessage("Finished UpdateTCTTeamData");
        //    Logger.WriteDebugMessage("UpdateTCTTeamData Complete");
        //}
        #endregion

        #region Field Security
        //public void UpdateAllFacilityBUTeams()
        //{
        //    Logger.setMethod = "UpdateAllFacilityBUTeams";
        //    Logger.WriteGranularTimingMessage("Starting UpdateAllFacilityBUTeams");
        //    using (var srv = new Xrm(OrganizationService))
        //    {

        //        var activeFacilities = srv.mcs_facilitySet.Where(f => f.statecode.Value == mcs_facilityState.Active);
        //        foreach (mcs_facility result in activeFacilities)
        //        {
        //            //Find Facility's BU Team

        //            //Update Team
        //            OrganizationService.Associate()
        //        }

        //        var sites = srv.mcs_siteSet.Where(s => s.statecode == mcs_siteState.Active).ToList();
        //        foreach (var site in sites)
        //        {
        //            Logger.WriteDebugMessage("Beginning Loop for " + site.mcs_name);
        //            var thisSiteTCTs = new List<Guid>();

        //            //Filter TCT type users from LINQ query to only use this site
        //            foreach (var user in tctUsers)
        //            {
        //                if (user.cvt_site != null && user.cvt_site.Id == site.Id)
        //                    thisSiteTCTs.Add(user.Id);
        //            }
        //            Guid teamId = new Guid();
        //            var existingTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite.Id == site.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.TCT);
        //            if (existingTeam == null)
        //                teamId = CreateTCTTeam(site, OrganizationService, Logger);
        //            else
        //                teamId = existingTeam.Id;

        //            //Update TMP Site with this team
        //            mcs_site updateSiteRecord = new mcs_site()
        //            {
        //                Id = site.Id,
        //                cvt_tctteam = new EntityReference()
        //                {
        //                    Id = teamId,
        //                    LogicalName = Team.EntityLogicalName
        //                }
        //            };


        //            try
        //            {
        //                if (teamId != Guid.Empty)
        //                {
        //                    OrganizationService.Update(updateSiteRecord);
        //                    Logger.WriteDebugMessage(String.Format("Updated TMP Site {0} with TCT Team.", site.mcs_name));
        //                }
        //                else
        //                    Logger.WriteDebugMessage("No TCT Team found/created for Site: " + site.mcs_name + ". Check the TMP Site's ownership.");
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.WriteDebugMessage("Error When Updating TMP Site. Message: " + ex.InnerException.Message);
        //            }



        //            //If there are TCTs for this site, then go ahead and add them to the TCT Team
        //            if (thisSiteTCTs.Count > 0)
        //            {
        //                AddMembersTeamRequest amtr = new AddMembersTeamRequest()
        //                {
        //                    TeamId = teamId,
        //                    MemberIds = thisSiteTCTs.ToArray(),
        //                };
        //                try
        //                {
        //                    OrganizationService.Execute(amtr);
        //                    Logger.WriteDebugMessage("Team Members Added to " + site.mcs_name);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.WriteToFile("Unable to Add all team members, looping through individual team members: " + ex.Message);
        //                    foreach (var user in thisSiteTCTs)
        //                    {
        //                        try
        //                        {
        //                            AddMembersTeamRequest req = new AddMembersTeamRequest()
        //                            {
        //                                TeamId = teamId,
        //                                MemberIds = new Guid[1] { user }
        //                            };
        //                            OrganizationService.Execute(req);
        //                            Logger.WriteDebugMessage("Added individual Member to TCT Team for " + site.mcs_name);
        //                        }
        //                        catch (Exception ex2)
        //                        {
        //                            Logger.WriteToFile(String.Format("Failed to add individual member {0} : {1}", user, ex2.Message));
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        Logger.WriteGranularTimingMessage("ending UpdateAllFacilityBUTeams");
        //        Logger.WriteDebugMessage("ending UpdateAllFacilityBUTeams");
        //    }
        //}

        #endregion

        #region TSA/Services
        /// <summary>
        /// Rebuilds the Group TSAs with the new service format
        /// </summary>
        // TSA is deprecated
        //public void RebuildGroupTSA()
        //{
        //    Logger.setMethod = "RebuildGroupTSA";
        //    Logger.WriteDebugMessage("starting RebuildGroupTSA");
        //    try
        //    {
        //        using (var srv = new Xrm(OrganizationService))
        //        {
        //            var groupTSAs = srv.mcs_servicesSet.Where(s => s.cvt_groupappointment == true && s.statuscode.Value == 251920000).ToList();
        //            var updatedCount = 0;
        //            if (groupTSAs.Count > 0)
        //            {
        //                Logger.WriteDebugMessage(String.Format("Found {0} Group TSAs in Production status.  Updating all records.", groupTSAs.Count().ToString()));
        //                foreach (var tsa in groupTSAs)
        //                {
        //                    try
        //                    {
        //                        CvtHelper.CreateUpdateService(tsa.Id, Logger, OrganizationService, McsSettings);
        //                        updatedCount += 1;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Logger.WriteToFile(String.Format("Unable to rebuild group TSA service: {0} {1} due to exception: {2}", tsa.mcs_name, tsa.Id, ex.Message));
        //                        CvtHelper.CreateNote(new EntityReference(mcs_services.EntityLogicalName, tsa.Id), "Unable to bulk rebuild Group TSA's service.", "Bulk Group Rebuild", OrganizationService);
        //                    }
        //                }
        //                Logger.WriteDebugMessage(String.Format("Function finished. Rebuilt {0}/{1} Group TSAs.", updatedCount, groupTSAs.Count));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteDebugMessage("Error with function RebuildGroupTSA. Exception: " + ex.Message);
        //    }
        //}

        /// <summary>
        /// Set Services to go initially to Pending, and integration will set to reserved-scheduled
        /// VA Video Connect Redesign Catchup
        /// </summary>
        public void SetInitialStatusesOnServices()
        {
            Logger.WriteDebugMessage("Beginning Update Initial Statuses on Services.");
            var individualIds = new List<Guid>();
            var groupIds = new List<Guid>();
            using (var srv = new Xrm(OrganizationService))
            {
                //get list of Services that need to be changed to have pending initial status
                individualIds = (from s in srv.ServiceSet
                                     //join ps in srv.cvt_participatingsiteSet on s.Id equals ps.cvt_relatedservice.Id
                                 join sp in srv.cvt_resourcepackageSet on s.Id equals sp.cvt_relatedservice.Id
                                 where
                                 !sp.cvt_groupappointment.Value
                                 select s.Id).ToList();
                groupIds = (from s in srv.ServiceSet
                                //join tsa in srv.mcs_servicesSet on s.Id equals tsa.mcs_RelatedServiceId.Id
                            join sp in srv.cvt_resourcepackageSet on s.Id equals sp.cvt_relatedservice.Id
                            where
                            sp.cvt_groupappointment.Value
                            select s.Id).ToList();
            }

            Logger.WriteDebugMessage(string.Format("Found {0} individual services and {1} group services to Update", individualIds.Count, groupIds.Count));
            foreach (Guid id in individualIds)
            {
                try
                {
                    var updatedService = new Service
                    {
                        Id = id,
                        InitialStatusCode = new OptionSetValue((int)service_initialstatuscode.Pending)
                    };
                    OrganizationService.Update(updatedService);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(String.Format("Failed to update individual service {0} to initial status of \"Pending\".  Error: {1}", id, ex.Message));
                }
            }
            foreach (Guid id in groupIds)
            {
                try
                {
                    var updatedService = new Service
                    {
                        Id = id,
                        InitialStatusCode = new OptionSetValue((int)service_initialstatuscode.Reserved)
                    };
                    OrganizationService.Update(updatedService);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(String.Format("Failed to update group service {0} to initial status of \"Reserved Scheduled\".  Error: {1}", id, ex.Message));
                }

            }
            Logger.WriteDebugMessage("Finished Update Initial Statuses on Services.");
        }

        /// <summary>
        /// Evaluates and shares interfacility TSAs with the Patient teams
        /// </summary>
        public void ShareInterFacilityTSA()
        {
            Logger.setMethod = "ShareInterFacilityTSA";
            using (var srv = new Xrm(OrganizationService))
            {
                //Logger.WriteDebugMessage("About to get all of the active TSAs.");
                var allTSAs = srv.cvt_facilityapprovalSet.Where(t => t.statecode.Value == (int)cvt_facilityapprovalState.Active);
                Logger.WriteDebugMessage(String.Format("Analyzing {0} active TSAs.", allTSAs.ToList().Count));
                var countSharedTSAs = 0;
                foreach (var tsa in allTSAs)
                {
                    bool shared = VA.TMP.CRM.McsServicesCreatePostStageRunner.EnableInterFacilityTSA(tsa, Logger, OrganizationService);
                    if (shared)
                        countSharedTSAs++;
                }
                Logger.setMethod = "ShareInterFacilityTSA";
                Logger.WriteDebugMessage("Finished sharing " + countSharedTSAs + " existing InterFacility TSAs.");
            }
        }
        #endregion

        #region Data Updates
        public void DeleteUnlinkedSP()
        {
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var spList = srv.cvt_resourcepackageSet.Where(x => x.statecode.Value == 0);
                var count = 0;
                foreach (cvt_resourcepackage record in spList)
                {
                    var childPS = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_resourcepackage.Id == record.Id).ToList();
                    if (childPS != null && childPS.Count == 0)
                    {
                        try
                        {
                            OrganizationService.Delete(record.LogicalName, record.Id);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Failed to Delete Scheduling Package Name: " + record.cvt_name + ". Exception: " + ex.Message);
                        }
                    }
                }
                Logger.WriteDebugMessage("Finished running the Scheduling Package Clean Up Job. Deleted=" + count);
            }
        }
        public void TestDeleteUnlinkedSP()
        {
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var spList = srv.cvt_resourcepackageSet.Where(x => x.statecode.Value == 0);
                var count = 0;
                foreach (cvt_resourcepackage record in spList)
                {
                    var childPS = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_resourcepackage.Id == record.Id).ToList();
                    if (childPS != null && childPS.Count == 0)
                    {
                        try
                        {
                            count++;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Failed to Delete Scheduling Package Name: " + record.cvt_name + ". Exception: " + ex.Message);
                        }
                    }
                }
                Logger.WriteDebugMessage("Finished running the Scheduling Package Clean Up Job. Found=" + count);
            }
        }
        public void UpdateSPNames()
        {
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var spList = srv.cvt_resourcepackageSet.Where(x => x.statecode.Value == 0);
                foreach (cvt_resourcepackage record in spList)
                {
                    var derivedName = CvtHelper.ReturnRecordNameIfChanged(record, false, Logger, OrganizationService);
                    if (derivedName != record.cvt_name && !String.IsNullOrEmpty(derivedName))
                    {
                        cvt_resourcepackage updateRG = new cvt_resourcepackage()
                        {
                            Id = record.Id,
                            cvt_name = derivedName
                        };
                        Logger.WriteDebugMessage("The Scheduling Package name should be different, updating it now: " + derivedName + ".");
                        try
                        {
                            OrganizationService.Update(updateRG);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Failed to update Scheduling Package Name: " + record.cvt_name + ". Exception: " + ex.Message);
                        }
                    }

                }

                Logger.WriteDebugMessage("Finished running the Update Scheduling Package Names Job. ");
            }
        }

        public void UpdateSPUsage()
        {
            Logger.WriteDebugMessage("starting");
            using (var srv = new Xrm(OrganizationService))
            {
                var spList = srv.cvt_resourcepackageSet.Where(x => x.statecode.Value == 0 && x.cvt_usagetype == null);
                foreach (cvt_resourcepackage record in spList)
                {
                    cvt_resourcepackage updateSP = new cvt_resourcepackage()
                    {
                        Id = record.Id,
                        cvt_usagetype = false
                    };

                    try
                    {
                        OrganizationService.Update(updateSP);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile("Failed to update Scheduling Package's Usage: " + record.cvt_name + ". Exception: " + ex.Message);
                    }
                }
                Logger.WriteDebugMessage("Finished running the Update Scheduling Package Usage Type Job. ");
            }
        }
        public void UpdateTSSRGNames()
        {
            Logger.setMethod = "UpdateTSSRGNames";
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var rgList = srv.mcs_resourcegroupSet.Where(x => x.statecode.Value == 0);
                foreach (mcs_resourcegroup rg in rgList)
                {
                    var derivedName = CvtHelper.ReturnRecordNameIfChanged(rg, false, Logger, OrganizationService);
                    if (derivedName != rg.mcs_name && !String.IsNullOrEmpty(derivedName))
                    {
                        mcs_resourcegroup updateRG = new mcs_resourcegroup()
                        {
                            Id = rg.Id,
                            mcs_name = derivedName
                        };
                        Logger.WriteDebugMessage("The TSS Resource Group name should be different, updating it now: " + derivedName + ".");
                        try
                        {
                            OrganizationService.Update(updateRG);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Failed to update TSS Resource Group Name: " + rg.mcs_name + ". Exception: " + ex.Message);
                        }
                    }

                }


                Logger.WriteDebugMessage("Finished running the Update TSS Resource Group Names Job. ");
            }
        }

        public void UpdateTSSRGAllRequiredNames()
        {
            Logger.setMethod = "UpdateTSSRGAllRequiredNames";
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var rgList = srv.mcs_resourcegroupSet.Where(x => x.statecode.Value == 0
                    && !x.mcs_name.Contains("Paired Resource Group")
                    && x.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                    .OrderBy(x => x.CreatedOn);

                foreach (mcs_resourcegroup rg in rgList)
                {
                    var derivedName = CvtHelper.ReturnRecordNameIfChanged(rg, false, Logger, OrganizationService);
                    if (derivedName != rg.mcs_name && !String.IsNullOrEmpty(derivedName))
                    {
                        mcs_resourcegroup updateRG = new mcs_resourcegroup()
                        {
                            Id = rg.Id,
                            mcs_name = derivedName
                        };
                        Logger.WriteDebugMessage("The TSS Resource Group name should be different, updating it now: " + derivedName + ".");
                        try
                        {
                            OrganizationService.Update(updateRG);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Failed to update TSS Resource Group Name: " + rg.mcs_name + ". Exception: " + ex.Message);
                        }
                    }

                }

                Logger.WriteDebugMessage("Finished running the Update TSS Resource Group Names Job. ");
            }
        }

        public void UpdateTMPResourceNames()
        {
            try
            {
                Logger.setMethod = "UpdateTMPResourceNames";
                Logger.WriteDebugMessage("starting");
                using (var srv = new Xrm(OrganizationService))
                {
                    var resourceList = from r in srv.mcs_resourceSet
                                       where r.statecode.Value == 0
                                       orderby r.CreatedOn
                                       select r;

                    foreach (mcs_resource resource in resourceList)
                    {
                        var derivedName = CvtHelper.ReturnRecordNameIfChanged(resource, false, Logger,
                            OrganizationService);

                        if (string.IsNullOrWhiteSpace(derivedName))
                        {
                            Logger.WriteDebugMessage("Resource Name is correct, exiting Naming code. Record GUID: " + resource.Id);
                        }
                        else if (derivedName != resource.mcs_name)
                        {
                            mcs_resource updateResource = new mcs_resource()
                            {
                                Id = resource.Id,
                                mcs_name = derivedName
                            };
                            Logger.WriteDebugMessage("The TMP Resource name should be different, updating it now: " +
                                                     derivedName + ".");
                            try
                            {
                                OrganizationService.Update(updateResource);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteToFile(string.Format("Failed to update TMP Resource Name: {0}, Id: {1}. Exception: {2}", resource.mcs_name, resource.Id, ex.Message));
                            }
                        }
                    }
                    Logger.WriteDebugMessage("Finished running the Update TMP Resource Names Job. ");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Failed to update TMP Resource Name. Exception: " + ex.Message);
            }
        }

        public void UpdateTESNames()
        {
            try
            {
                Logger.setMethod = "UpdateTESNames";
                Logger.WriteDebugMessage("starting");
                using (var srv = new Xrm(OrganizationService))
                {
                    var resourceList = from r in srv.mcs_resourceSet
                                       where r.statecode.Value == 0
                                       where r.cvt_systemtype.Value == (int)mcs_resourcecvt_systemtype.TransportableSystem
                                       orderby r.CreatedOn
                                       select r;

                    foreach (mcs_resource resource in resourceList)
                    {
                        var derivedName = CvtHelper.ReturnRecordNameIfChanged(resource, false, Logger,
                            OrganizationService);

                        if (string.IsNullOrWhiteSpace(derivedName))
                        {
                            Logger.WriteDebugMessage("Resource Name is correct, exiting Naming code. Record GUID: " + resource.Id);
                        }
                        else if (derivedName != resource.mcs_name)
                        {
                            mcs_resource updateResource = new mcs_resource()
                            {
                                Id = resource.Id,
                                mcs_name = derivedName
                            };
                            Logger.WriteDebugMessage("The TMP Resource name should be different, updating it now: " +
                                                     derivedName + ".");
                            try
                            {
                                OrganizationService.Update(updateResource);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteToFile(string.Format("Failed to update TMP Resource Name: {0}, Id: {1}. Exception: {2}", resource.mcs_name, resource.Id, ex.Message));
                            }
                        }
                    }
                    Logger.WriteDebugMessage("Finished running the Update TMP Resource Names for TES records Job. ");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Failed to update TMP Resource Name. Exception: " + ex.Message);
            }
        }

        public void UpdateTMPResources()
        {
            Logger.setMethod = "UpdateTMPResources";
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                string resourceIds = string.Empty;

                var resourceList = from r in srv.mcs_resourceSet where r.statecode.Value == (int)mcs_resourceState.Active && r.cvt_systemtype.Value != (int)mcs_resourcecvt_systemtype.TelehealthPatientCartSystem select r;
                foreach (mcs_resource resource in resourceList)
                {
                    var updateResource = new mcs_resource()
                    {
                        Id = resource.Id,
                    };

                    //Set the default cart type for the system types other than Telehealth Patient Cart Systems
                    if (resource.cvt_CartTypeId == null && resource.cvt_systemtype != null && resource.cvt_systemtype.Value != (int)mcs_resourcecvt_systemtype.TelehealthPatientCartSystem)
                    {
                        var cartType = srv.cvt_carttypeSet.FirstOrDefault(c => c.cvt_ResourceSystemType.Value == resource.cvt_systemtype.Value);

                        if (cartType == null)
                        {
                            Logger.WriteToFile("Default Cart Type not found for the Resource System Type " + resource.cvt_systemtype.Value);
                        }
                        else
                        {
                            var cartTypeId = new EntityReference(cvt_carttype.EntityLogicalName, cartType.Id);
                            updateResource.cvt_CartTypeId = cartTypeId;
                            resourceIds += string.Format("Id:{0}, ", resource.Id);
                            try
                            {
                                OrganizationService.Update(updateResource);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteToFile("Failed to update TMP Resource: " + resource.Id.ToString() + ". Exception: " + ex.Message);
                            }
                        }
                    }
                }

                Logger.WriteToFile("The following TMP resource were updated\n" + resourceIds);
                Logger.WriteDebugMessage("Finished running the Update TMP Resources Job. ");
            }
        }

        public void SwapImportedClinicsStopCodeValues()
        {
            Logger.WriteDebugMessage("Starting - SwapImportedClinicsStopCodeValues");

            using (var srv = new Xrm(OrganizationService))
            {
                //Find Resources (that are VC and Active) where the Equipment is an activity party listed on a SA (that is closed or scheduled).
                Logger.WriteDebugMessage("Retrieving VCs");

                string parameters = PrimaryEntity.Attributes["mcs_unexpectedmessage"].ToString();
                string[] pars;
                var startDate = DateTime.Today.AddMonths(-2);
                var endDate = DateTime.Now;
                var modifiedBefore = DateTime.Now;
                string createdBy = "DEV\\SVC_NPTMPDeploy";

                if (string.IsNullOrWhiteSpace(parameters))
                {
                    Logger.WriteToFile($"SwapImportedClinicsStopCodeValues - Parameters is null. Exiting without processing");
                    return;
                }
                else
                {
                    Logger.WriteDebugMessage($"SwapImportedClinicsStopCodeValues - Parameters - {parameters}");
                    pars = parameters.Split(';');
                }

                if (pars.Length > 0)
                {
                    if (pars[0] != null)
                    {
                        if (!DateTime.TryParse(pars[0], out startDate))
                        {
                            Logger.WriteToFile($"Start Date Parameter: {pars[0]} could not be converted to date time");
                            return;
                        }
                    }

                    if (pars[1] != null)
                    {
                        if (!DateTime.TryParse(pars[1], out endDate))
                        {
                            Logger.WriteToFile($"End Date Parameter: {pars[1]} could not be converted to date time");
                            return;
                        }
                    }

                    if (pars[2] != null)
                    {
                        createdBy = pars[2];
                    }

                    if (pars[3] != null)
                    {
                        if (!DateTime.TryParse(pars[3], out modifiedBefore))
                        {
                            Console.WriteLine("Modified On Parameter: " + pars[3] + " could not be converted to date time");
                            return;
                        }
                    }
                }

                var clinics = (from r in srv.mcs_resourceSet
                               join s in srv.SystemUserSet on r.CreatedBy.Id equals s.Id
                               where r.statecode.Value == (int)mcs_resourceState.Active
                                     && r.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic
                                     && s.DomainName.Contains(createdBy)
                                     && r.CreatedOn.Value >= startDate && r.CreatedOn.Value < endDate.AddDays(1) && r.ModifiedOn.Value < modifiedBefore
                               orderby r.mcs_name
                               select new mcs_resource { Id = r.mcs_resourceId.Value, cvt_primarystopcode = r.cvt_primarystopcode, cvt_secondarystopcode = r.cvt_secondarystopcode }).ToList();

                Logger.WriteToFile($"{clinics.Count} clinics retrieved successfully imported/created by {createdBy} between the date time: {startDate} and {endDate.AddDays(1)}. Initiating the clinic stop code swap update process");

                var updateCount = 0;
                var sameValuesCount = 0;
                foreach (var clinic in clinics)
                {
                    if (clinic.cvt_secondarystopcode == clinic.cvt_primarystopcode)
                        sameValuesCount++;
                    else
                    {
                        var updatedClinic = new mcs_resource
                        {
                            cvt_primarystopcode = clinic.cvt_secondarystopcode,
                            cvt_secondarystopcode = clinic.cvt_primarystopcode,
                            Id = clinic.Id
                        };

                        try
                        {
                            OrganizationService.Update(updatedClinic);
                            updateCount++;
                        }
                        catch (Exception e)
                        {
                            Logger.WriteToFile($"Error occured while updating clinic with Id: {clinic.Id}. Error: {e.Message} {e.InnerException}");
                        }
                    }

                    if (updateCount % 1000 == 0)
                        Logger.WriteDebugMessage($"Processing {updateCount} of {clinics.Count} Clinics. Skipped {sameValuesCount} records having same stop code values");
                }
                Logger.WriteToFile($"Total Count: {clinics.Count} Updated: {updateCount} Same Value (update not needed): {sameValuesCount}");
            }
        }

        public void PurgePluginLogs()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "PurgePluginLogs";
                var logs = (from l in srv.mcs_logSet
                    orderby l.CreatedOn
                    select l.Id).ToList();

                Logger.WriteDebugMessage($"Retrieved {logs.Count} plugin log records. Initiating delete");

                foreach (var id in logs)
                    DeleteRecord(mcs_log.EntityLogicalName, id);

                if(logs.Count > 0)
                    PurgePluginLogs();

                Logger.WriteDebugMessage("PurgePluginLogs Complete");
            }
        }

        public void MarkUsedVC()
        {
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                //Find Resources (that are VC and Active) where the Equipment is an actvity party listed on a SA (that is closed or scheduled).
                Logger.WriteDebugMessage("Retrieving VCs used on SAs.");

                //Ultimately need to return a list of resources
                var resourceSAList = (from r in srv.mcs_resourceSet
                                          //Resources are representation of Equipment, which can actually be booked against the SA or the Appt
                                      join e in srv.EquipmentSet on r.mcs_relatedResourceId.Id equals e.Id
                                      //The Equipment is listed against the SA or Appt by a multiselect lookup field, which is associated through an ActivityParty
                                      join ap in srv.ActivityPartySet on e.EquipmentId.Value equals ap.PartyId.Id
                                      join sa in srv.ServiceAppointmentSet on ap.ActivityId.Id equals sa.ActivityId.Value
                                      //Resource is still active
                                      where r.statecode.Value == (int)mcs_resourceState.Active
                                      //Resource is VC
                                      && r.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic

                                      && ap.ParticipationTypeMask.Value == 10 //Resource (5 is RequiredAttendee)

                                      && (sa.StateCode.Value == ServiceAppointmentState.Scheduled || sa.StateCode.Value == ServiceAppointmentState.Closed)
                                      //SA is actually scheduled or closed
                                      select new
                                      {
                                          r.mcs_resourceId,
                                          r.cvt_scheduled
                                      }).Distinct().ToList();

                Logger.WriteDebugMessage("Retrieving VCs used on BRs.");
                var resourceApptList = (from r in srv.mcs_resourceSet
                                            //Resources are representation of Equipment, which can actually be booked against the SA or the Appt
                                        join e in srv.EquipmentSet on r.mcs_relatedResourceId.Id equals e.Id
                                        //The Equipment is listed against the SA or Appt by a multiselect lookup field, which is associated through an ActivityParty
                                        join ap in srv.ActivityPartySet on e.EquipmentId.Value equals ap.PartyId.Id
                                        join appt in srv.AppointmentSet on ap.ActivityId.Id equals appt.ActivityId.Value
                                        //Resource is still active
                                        where r.statecode.Value == (int)mcs_resourceState.Active
                                        //Resource is VC
                                        && r.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic

                                        && ap.ParticipationTypeMask.Value == 5 //Resource (5 is RequiredAttendee)
                                                                               //Appt is actually scheduled or closed
                                        && (appt.StateCode.Value == AppointmentState.Scheduled || appt.StateCode.Value == AppointmentState.Completed)

                                        select new
                                        {
                                            r.mcs_resourceId,
                                            r.cvt_scheduled
                                        }).Distinct().ToList();

                int updated = 0;
                int counted = 0;
                Logger.WriteDebugMessage("Looping through VCs used on SAs. Count=" + resourceSAList.Count);

                foreach (var item in resourceSAList)
                {
                    if (item.cvt_scheduled == null || item.cvt_scheduled.Value != true)
                    {
                        try
                        {
                            var previouslyUsedVC = new mcs_resource()
                            {
                                Id = item.mcs_resourceId.Value,
                                cvt_scheduled = true
                            };
                            //Mark VC as used
                            OrganizationService.Update(previouslyUsedVC);
                            updated++;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Failed to update VC for SA. GUID: " + item.mcs_resourceId.Value + ". Error Message: " + ex.InnerException);
                        }
                    }
                    else
                        counted++;
                    //already set to true, no need to update
                }
                Logger.WriteToFile("SA: Number of VCs marked as previously used/in use so not updated again: " + counted);
                Logger.WriteToFile("SA: Number of VCs marked as previously used/in use and not already marked and thus updated: " + updated);
                Logger.WriteDebugMessage("Looping through VCs used on BRs.");
                foreach (var item in resourceApptList)
                {
                    if (item.cvt_scheduled == null || item.cvt_scheduled.Value != true)
                    {
                        try
                        {
                            var previouslyUsedVC = new mcs_resource()
                            {
                                Id = item.mcs_resourceId.Value,
                                cvt_scheduled = true
                            };
                            //Mark VC as used
                            OrganizationService.Update(previouslyUsedVC);
                            updated++;
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile("Failed to update VC for BR. GUID: " + item.mcs_resourceId.Value + ". Error Message: " + ex.InnerException);
                        }
                    }
                    else
                        counted++;
                    //already set to true, no need to update
                }

                Logger.WriteToFile("Total: Number of VCs marked as previously used/in use so not updated again: " + counted);
                Logger.WriteToFile("Total: Number of VCs marked as previously used/in use and not already marked and thus updated: " + updated);
                Logger.WriteDebugMessage("Finished running the MarkUsedVC Job. ");
            }
        }

        public void UpdateComponent()
        {
            Logger.setMethod = "UpdateComponent";
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                //Update the status to Deployed/Installed where the current value is either Acquisition Process or Deployment Process
                string componentIds = string.Empty;
                var componentList = from c in srv.cvt_componentSet where c.statecode.Value == cvt_componentState.Active && c.cvt_status.Value > (int)cvt_componentcvt_status.RMA select new cvt_component { Id = c.Id, cvt_status = c.cvt_status };

                foreach (cvt_component component in componentList)
                {
                    var updateComponent = new cvt_component()
                    {
                        Id = component.Id,
                        cvt_status = new OptionSetValue((int)cvt_componentcvt_status.DeployedInstalled)
                    };
                    Logger.WriteDebugMessage("The Component with Id: " + component.Id.ToString() + ", Status is set to " + component.cvt_status.Value + ", updating it now to Deployed/Installed.");
                    componentIds += component.Id.ToString() + ", " + component.cvt_status.Value + "\r\n";
                    try
                    {
                        OrganizationService.Update(updateComponent);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile("Failed to update Component: " + component.Id.ToString() + ". Exception: " + ex.Message);
                    }
                }

                Logger.WriteToFile("The following component were updated\n" + componentIds);
                Logger.WriteDebugMessage("Finished running the Update Component Job. ");
            }
        }


        //requires refactor of the ProviderSiteResource entity
        //public void UpdateTsaSiteUserNames()
        //{
        //    try
        //    {
        //        Logger.setMethod = "UpdateTsaSiteUserNames";
        //        Logger.WriteDebugMessage("starting");
        //        Logger.WriteTxnTimingMessage("UpdateTsaSiteUserNames Started");

        //        using (var srv = new Xrm(OrganizationService))
        //        {
        //            var tsas = from svc in srv.cvt_facilityapprovalSet
        //                       where svc.statecode == 0 orderby svc.CreatedOn
        //                       select svc;
        //            var totalCount = tsas.ToList().Count;
        //            var counter = 0;
        //            foreach (var tsa in tsas)
        //            {
        //                try
        //                {
        //                    string patsiteUsers = string.Empty;
        //                    string prositeUsers = string.Empty;
        //                    #region PROVIDER SITE RESOURCES
        //                    //Retrieve all the provider site resources
        //                    Logger.WriteDebugMessage("Start sorting through Provider Site Resources");
        //                    var getProvResources = from provGroups in srv.cvt_providerresourcegroupSet
        //                                           where provGroups.cvt_RelatedTSAid.Id == tsa.Id
        //                                           where provGroups.statecode == 0
        //                                           select new
        //                                           {
        //                                               provGroups.Id,
        //                                               provGroups.cvt_RelatedResourceId,
        //                                               provGroups.cvt_resourcespecguid,
        //                                               provGroups.cvt_TSAResourceType,
        //                                               provGroups.cvt_Type,
        //                                               provGroups.cvt_RelatedUserId,
        //                                               provGroups.cvt_RelatedResourceGroupid,
        //                                               provGroups.cvt_name
        //                                           };

        //                    Logger.WriteDebugMessage($"Generate the value for the Providers (cvt_providers) field for the TSA: {tsa.mcs_name} with Id {tsa.Id} update - Started\n# of Provider Site Resources associated: {getProvResources.ToList().Count}");
        //                    //Loop through all of the Provider Site resources
        //                    foreach (var provSiteResource in getProvResources)
        //                    {
        //                        Logger.WriteDebugMessage("Starting loop for " + provSiteResource.cvt_name);

        //                        //Verify that the Resource is typed, not required, but should be filled in
        //                        if (provSiteResource.cvt_Type != null)
        //                        {
        //                            //Logger.WriteDebugMessage("cvt_Type SWITCH");
        //                            switch (provSiteResource.cvt_Type.Value)
        //                            {
        //                                case 100000000: //Telepresenter/Imager
        //                                case 99999999: //Provider

        //                                    if (provSiteResource.cvt_TSAResourceType == null)
        //                                    {
        //                                        Logger.WriteDebugMessage(
        //                                            String.Format(
        //                                                "Provider Site Resource is invalid, please remove and re-add the Provider(s) to this TSA: {0}.",
        //                                                provSiteResource.cvt_name));
        //                                    }
        //                                    else
        //                                    {
        //                                        if (provSiteResource.cvt_TSAResourceType.Value == 0)
        //                                            //Group of Providers
        //                                        {
        //                                            if (provSiteResource.cvt_RelatedResourceGroupid == null)
        //                                            {
        //                                                Logger.WriteDebugMessage(
        //                                                    String.Format(
        //                                                        "Provider Site Resource is invalid, please remove and re-add the Provider Group to this TSA: {0}.",
        //                                                        provSiteResource.cvt_name));
        //                                            }
        //                                            else
        //                                            {
        //                                                //Query for child names
        //                                                var groupResourceRecords =
        //                                                    srv.mcs_groupresourceSet.Where(
        //                                                        g =>
        //                                                            g.mcs_relatedResourceGroupId.Id ==
        //                                                            provSiteResource.cvt_RelatedResourceGroupid.Id &&
        //                                                            g.statecode == 0);
        //                                                foreach (var child in groupResourceRecords)
        //                                                {
        //                                                    if (child.mcs_RelatedUserId != null && !prositeUsers.Contains(child.mcs_RelatedUserId.Name + " ; "))
        //                                                        prositeUsers += child.mcs_RelatedUserId.Name + " ; ";
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (provSiteResource.cvt_RelatedUserId == null)
        //                                            {
        //                                                Logger.WriteDebugMessage(
        //                                                    String.Format(
        //                                                        "Provider Site Resource is invalid, please remove and re-add the Provider to this TSA: {0}.",
        //                                                        provSiteResource.cvt_name));
        //                                                break;
        //                                            }
        //                                            if (provSiteResource.cvt_RelatedUserId != null && !prositeUsers.Contains(provSiteResource.cvt_RelatedUserId.Name + " ; "))
        //                                                prositeUsers += provSiteResource.cvt_RelatedUserId.Name + " ; ";
        //                                        }
        //                                    }
        //                                    break;
        //                                case 917290000: //Paired
        //                                    if (provSiteResource.cvt_RelatedResourceGroupid == null)
        //                                    {
        //                                        Logger.WriteDebugMessage(String.Format("Provider Site Paired Resources record is invalid, please remove and re-add the Paired Resources record to this TSA: {0}.", provSiteResource.cvt_name));
        //                                        break;
        //                                    }
        //                                    //Query for child names
        //                                    var childgroupResourceRecords = srv.mcs_groupresourceSet.Where(g => g.mcs_relatedResourceGroupId.Id == provSiteResource.cvt_RelatedResourceGroupid.Id && g.statecode == 0);
        //                                    foreach (var child in childgroupResourceRecords)
        //                                    {
        //                                        if (child.mcs_RelatedUserId != null && !prositeUsers.Contains(child.mcs_RelatedUserId.Name + " ; "))
        //                                            prositeUsers += child.mcs_RelatedUserId.Name + " ; ";
        //                                    }
        //                                    break;
        //                                    //default: No Default Required - Room or Technology (or unknown type), do nothing
        //                                    //    break;
        //                            }
        //                        }
        //                        else //Probably Single Provider, but check.
        //                        {
        //                            if (provSiteResource.cvt_TSAResourceType == null)
        //                            {
        //                                Logger.WriteDebugMessage(String.Format("Provider Site Resource is invalid, please remove and re-add the single provider to this TSA: {0}.", provSiteResource.cvt_name));
        //                                break;
        //                            }
        //                            //Provider or Telepresenter
        //                            if ((provSiteResource.cvt_TSAResourceType.Value == 2) || (provSiteResource.cvt_TSAResourceType.Value == 3))
        //                            {
        //                                if (provSiteResource.cvt_RelatedUserId != null && !prositeUsers.Contains(provSiteResource.cvt_RelatedUserId.Name + " ; "))
        //                                {
        //                                    prositeUsers += provSiteResource.cvt_RelatedUserId.Name + " ; ";

        //                                }
        //                                else
        //                                    Logger.WriteDebugMessage(String.Format("User not listed in Provider Group {0}.", provSiteResource.cvt_name));
        //                            }
        //                            else
        //                                Logger.WriteDebugMessage(String.Format("Type is not null and provider site resource is not a user {0}", provSiteResource.cvt_name));
        //                        }
        //                    }
        //                    //Buildout Group Prov Only Paired here
        //                    Logger.WriteDebugMessage("Finished Sorting through Provider Site Resources");
        //                    #endregion

        //                    #region PATIENT SITE RESOURCES

        //                    var getPatResources = from patGroups in srv.cvt_patientresourcegroupSet
        //                        where patGroups.cvt_RelatedTSAid.Id == tsa.Id
        //                        where patGroups.statecode == 0
        //                        select new
        //                        {
        //                            patGroups.Id,
        //                            patGroups.cvt_RelatedResourceId,
        //                            patGroups.cvt_resourcespecguid,
        //                            patGroups.cvt_TSAResourceType,
        //                            patGroups.cvt_type,
        //                            patGroups.cvt_RelatedResourceGroupid,
        //                            patGroups.cvt_RelatedUserId,
        //                            patGroups.cvt_name
        //                        };

        //                    Logger.WriteDebugMessage($"Generate the value for the Patient Site users (cvt_patsiteusers) field for the TSA: {tsa.mcs_name} with Id {tsa.Id} update - Started\n# of Patient Site Resources associated: {getPatResources.ToList().Count}");

        //                    //Loop through all of the Patient Site resources
        //                    foreach (var patSiteResource in getPatResources)
        //                    {
        //                        if (patSiteResource.cvt_RelatedUserId != null &&
        //                            !patsiteUsers.Contains(patSiteResource.cvt_RelatedUserId.Name + " ; "))
        //                        {
        //                            patsiteUsers += patSiteResource.cvt_RelatedUserId.Name + " ; ";
        //                        }

        //                        //Verify that the Resource is typed
        //                        if (patSiteResource.cvt_type != null)
        //                        {
        //                            switch (patSiteResource.cvt_type.Value)
        //                            {
        //                                case 917290000: //Paired Resource Group
        //                                    //Query for child names, if Vista Clinics then write into the string
        //                                    var childgroupResourceRecords =
        //                                        srv.mcs_groupresourceSet.Where(
        //                                            g =>
        //                                                g.mcs_relatedResourceGroupId.Id ==
        //                                                patSiteResource.cvt_RelatedResourceGroupid.Id &&
        //                                                g.statecode == 0);
        //                                    foreach (var child in childgroupResourceRecords)
        //                                    {
        //                                        if (child.mcs_RelatedUserId != null &&
        //                                            !patsiteUsers.Contains(child.mcs_RelatedUserId?.Name + " ; "))
        //                                            //User for Group
        //                                        {
        //                                            patsiteUsers += child.mcs_RelatedUserId?.Name + " ; ";
        //                                        }
        //                                    }
        //                                    break;
        //                                case 99999999: //Provider
        //                                case 100000000: //Telepresenter/Imager
        //                                    var childgrpResourceRecords =   from gr in srv.mcs_groupresourceSet
        //                                                                    join rg in srv.mcs_resourcegroupSet on gr.mcs_relatedResourceGroupId.Id equals rg.mcs_resourcegroupId.Value
        //                                                                    join prg in srv.cvt_patientresourcegroupSet on rg.mcs_resourcegroupId.Value equals prg.cvt_RelatedResourceGroupid.Id
        //                                                                    where gr.mcs_RelatedUserId != null && prg.cvt_patientresourcegroupId.Value == patSiteResource.Id
        //                                                                    select gr.mcs_RelatedUserId;

        //                                    foreach (var child in childgrpResourceRecords)
        //                                    {
        //                                        if (child != null && !patsiteUsers.Contains(child.Name + " ; "))
        //                                            patsiteUsers += child.Name + " ; ";
        //                                    }
        //                                    break;
        //                            }
        //                        }
        //                    }

        //                    Logger.WriteDebugMessage(
        //                        $"Generate the value for the Patient Site users (cvt_patsiteusers) field for the TSA: {tsa.mcs_name} with Id {tsa.Id} update - Complete.\nOld Value: {tsa.cvt_patsiteusers}, Generated Value: {patsiteUsers}");
        //                    #endregion

        //                    #region TSA UPDATE
        //                    char[] charsToTrim = { ' ', ';' };
        //                    var patsiteUsersToUpdate = CvtHelper.ValidateLength(patsiteUsers, 2500).TrimEnd(charsToTrim);
        //                    var prositeUsersToUpdate = CvtHelper.ValidateLength(prositeUsers, 2500).TrimEnd(charsToTrim);
        //                    bool shouldUpdate = false;
        //                    var updateTsa = new mcs_services
        //                    {
        //                        Id = tsa.Id,
        //                    };

        //                    if (tsa.cvt_patsiteusers != patsiteUsersToUpdate)
        //                    {
        //                        updateTsa.cvt_patsiteusers = patsiteUsersToUpdate;
        //                        shouldUpdate = true;
        //                    }
        //                    else
        //                        Logger.WriteDebugMessage(
        //                            $"Generated value for the Patient Site users (cvt_patsiteusers) field for the TSA: {tsa.mcs_name} with Id {tsa.Id} are identical. Hence skipping the update\nValue: {patsiteUsersToUpdate}");

        //                    if (tsa.cvt_providers != prositeUsersToUpdate)
        //                    {
        //                        updateTsa.cvt_providers = prositeUsersToUpdate; 
        //                        shouldUpdate = true;
        //                    }
        //                    else
        //                        Logger.WriteDebugMessage(
        //                            $"Generated value for the providers (cvt_providers) field for the TSA: {tsa.mcs_name} with Id {tsa.Id} are identical. Hence skipping the update\nValue: {prositeUsersToUpdate}");

        //                    try
        //                    {
        //                        if(shouldUpdate)
        //                            OrganizationService.Update(updateTsa);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Logger.WriteToFile(
        //                            $"Failed to update the value for the TSA Name: {tsa.mcs_name} with Id:{tsa.Id}. Exception: {ex.Message}");
        //                    }

        //                    counter++;
        //                    Logger.WriteDebugMessage($"UpdateTsaSiteUserNames: {counter} of {totalCount} updated");
        //                    #endregion

        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.WriteToFile(
        //                        $"Failed to update the value for the Patient Site users (cvt_patsiteusers) field for the TSA Name: {tsa.mcs_name} with Id:{tsa.Id}.\nException: {ex.Message}\n{ex.StackTrace}");
        //                }
        //            }
        //        }
        //        Logger.WriteTxnTimingMessage("UpdateTsaSiteUserNames Ended");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteToFile($"Failed to update the value for the Patient Site users (cvt_patsiteusers). Exception: {ex.Message}");
        //    }
        //}

        public void UpdateBUTeams()
        {
            Logger.setMethod = "UpdateBUTeams";
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                EntityReferenceCollection RolesToAssociate = new EntityReferenceCollection();
                var fsProfile = srv.FieldSecurityProfileSet.FirstOrDefault(fsp => fsp.Name == "Read Secure Fields");
                if (fsProfile == null)
                    throw new InvalidPluginExecutionException("Failed to find Field Security Profile: Read Secure Fields");

                Logger.WriteDebugMessage("Found Field Security Profile: Read Secure Fields");
                RolesToAssociate.Add(new EntityReference(Role.EntityLogicalName, fsProfile.Id));
                //get BU Teams
                var BUs = srv.BusinessUnitSet;
                foreach (BusinessUnit record in BUs)
                {
                    var findBUTeam = srv.TeamSet.FirstOrDefault(t => t.Name == record.Name);
                    if (findBUTeam != null)
                    {
                        try
                        {
                            OrganizationService.Associate(Team.EntityLogicalName, findBUTeam.Id, new Relationship("teamprofiles_association"), RolesToAssociate);
                            Logger.WriteDebugMessage("Successfully associated the Field Security Profile with Team: " + record.Name);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteDebugMessage("Failed to associate Field Security Profile with Team: " + record.Name + ". Ex: " + ex.Message);
                        }
                    }
                }
            }
        }

        //Set up all of the Scheduling Packages
        public void SetupSP()
        {
            Logger.WriteDebugMessage("starting");
            int created = 0;
            int existed = 0;
            int failed = 0;

            using (var srv = new Xrm(OrganizationService))
            {
                var specialtyList = srv.mcs_servicetypeSet.Where(s =>
                s.statecode.Value == (int)mcs_servicetypeState.Active);

                Logger.WriteDebugMessage("Retrieved " + specialtyList.ToList().Count + " specialties.");

                foreach (mcs_servicetype record in specialtyList)
                {
                    var subtypeList = srv.mcs_servicesubtypeSet.Where(s => s.statecode.Value == (int)mcs_servicesubtypeState.Active && s.cvt_relatedServiceTypeId.Id == record.Id);
                    Logger.WriteDebugMessage("Retrieved " + subtypeList.ToList().Count + " sub-specialties.");
                    int tempExist = 0;
                    int tempFail = 0;

                    created += CreateSchedulingPackages(record, null, out tempExist, out tempFail);

                    existed += tempExist;
                    failed += tempFail;

                    if (subtypeList != null)
                    {
                        foreach (mcs_servicesubtype subRecord in subtypeList)
                        {
                            tempExist = 0;
                            tempFail = 0;
                            created += CreateSchedulingPackages(record, subRecord, out tempExist, out tempFail);

                            existed += tempExist;
                            failed += tempFail;
                        }
                    }
                }

                Logger.WriteToFile("Finished running the Create Schedule Packages. Skipped existing: " + existed + ". Successfully created: " + created + ". Failed: " + failed);
                var allSubtypeList = srv.mcs_servicesubtypeSet.Where(s => s.statecode.Value == (int)mcs_servicesubtypeState.Active);
                Logger.WriteToFile(specialtyList.ToList().Count + " specialties. " + allSubtypeList.ToList().Count + " sub-specialties.  Should have created: " + ((specialtyList.ToList().Count + allSubtypeList.ToList().Count) * 7) + " scheduling packages.");
            }
        }

        public int CreateSchedulingPackages(mcs_servicetype specialty, mcs_servicesubtype subspecialty, out int exist, out int fail)
        {
            Logger.setMethod = "CreateSchedulingPackages";
            Logger.WriteDebugMessage("starting");
            int created = 0;
            int existed = 0;
            int failed = 0;
            #region Permutations
            int[] providerLocationList = new int[7];
            providerLocationList[0] = (int)cvt_resourcepackagecvt_providerlocationtype.ClinicBased;
            providerLocationList[1] = (int)cvt_resourcepackagecvt_providerlocationtype.ClinicBased;
            providerLocationList[2] = (int)cvt_resourcepackagecvt_providerlocationtype.ClinicBased;
            providerLocationList[3] = (int)cvt_resourcepackagecvt_providerlocationtype.ClinicBased;
            providerLocationList[4] = (int)cvt_resourcepackagecvt_providerlocationtype.Telework;
            providerLocationList[5] = (int)cvt_resourcepackagecvt_providerlocationtype.Telework;
            providerLocationList[6] = (int)cvt_resourcepackagecvt_providerlocationtype.ClinicBased;
            //providerLocationList[7] = (int)cvt_resourcepackagecvt_providerlocationtype.ClinicBased;
            //providerLocationList[8] = (int)cvt_resourcepackagecvt_providerlocationtype.Telework;

            int[] patientLocationList = new int[7];
            patientLocationList[0] = (int)cvt_resourcepackagecvt_patientlocationtype.ClinicBased;
            patientLocationList[1] = (int)cvt_resourcepackagecvt_patientlocationtype.ClinicBased;
            patientLocationList[2] = (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone;
            patientLocationList[3] = (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone;
            patientLocationList[4] = (int)cvt_resourcepackagecvt_patientlocationtype.ClinicBased;
            patientLocationList[5] = (int)cvt_resourcepackagecvt_patientlocationtype.ClinicBased;
            patientLocationList[6] = (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone;
            //patientLocationList[7] = (int)cvt_resourcepackagecvt_patientlocationtype.ClinicBased;
            //patientLocationList[8] = (int)cvt_resourcepackagecvt_patientlocationtype.ClinicBased;

            int[] availableTelehealthModalityList = new int[9];
            availableTelehealthModalityList[0] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.ClinicalVideoTelehealth;
            availableTelehealthModalityList[1] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward;
            availableTelehealthModalityList[2] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.Telephone;
            availableTelehealthModalityList[3] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.ClinicalVideoTelehealth;
            availableTelehealthModalityList[4] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward;
            availableTelehealthModalityList[5] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.Telephone;
            availableTelehealthModalityList[6] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.ClinicalVideoTelehealth;
            availableTelehealthModalityList[7] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward;
            availableTelehealthModalityList[8] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.Telephone;
            //availableTelehealthModalityList[7] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.ClinicalVideoTelehealth;
            //availableTelehealthModalityList[8] = (int)cvt_resourcepackagecvt_availabletelehealthmodality.ClinicalVideoTelehealth;

            bool[] groupList = new bool[7];
            groupList[0] = false;
            groupList[1] = false;
            groupList[2] = false;
            groupList[3] = false;
            groupList[4] = false;
            groupList[5] = false;
            groupList[6] = true;
            //groupList[7] = true;
            //groupList[8] = true;

            #endregion
            using (var srv = new Xrm(OrganizationService))
            {
                for (int i = 0; i <= 6; i++) //Not creating ClinicPatient -Group in 4.4
                {
                    cvt_resourcepackage newSP = new cvt_resourcepackage()
                    {
                        cvt_StartAppointmentsEvery = 15,
                        cvt_AppointmentLength = 30,
                        cvt_providerlocationtype = new OptionSetValue(providerLocationList[i]),
                        cvt_patientlocationtype = new OptionSetValue(patientLocationList[i]),
                        cvt_availabletelehealthmodality = new OptionSetValue(availableTelehealthModalityList[i]),
                        cvt_groupappointment = groupList[i],
                        cvt_specialty = new EntityReference(specialty.LogicalName, specialty.Id),
                        cvt_name = "Autocreated"  //Do I need to stub out the name? It should automatically create?
                    };

                    if (subspecialty != null)
                    {
                        newSP.cvt_specialtysubtype = new EntityReference(subspecialty.LogicalName, subspecialty.Id);
                    }
                    try
                    {
                        if (subspecialty != null)
                        {
                            var existingSP = srv.cvt_resourcepackageSet.FirstOrDefault(rp =>
                                rp.cvt_providerlocationtype.Value == newSP.cvt_providerlocationtype.Value &&
                                rp.cvt_patientlocationtype.Value == newSP.cvt_patientlocationtype.Value &&
                                rp.cvt_availabletelehealthmodality.Value == newSP.cvt_availabletelehealthmodality.Value &&
                                rp.cvt_groupappointment.Value == newSP.cvt_groupappointment.Value &&
                                rp.cvt_specialty.Id == specialty.Id &&
                                rp.cvt_specialtysubtype.Id == subspecialty.Id &&
                                rp.cvt_hub == null &&
                                rp.statuscode.Value == (int)cvt_resourcepackage_statuscode.Active);

                            if (existingSP == null)
                            {
                                OrganizationService.Create(newSP);
                                created++;
                            }
                            else
                            {
                                Logger.WriteToFile(String.Format("Skipped creating a duplicate SP with specialty subtype for pro: {0}, pat: {1}, modality: {2}, group: {3}.", newSP.cvt_providerlocationtype.Value, newSP.cvt_patientlocationtype.Value, newSP.cvt_availabletelehealthmodality.Value, newSP.cvt_groupappointment.Value));
                                existed++;
                            }
                        }
                        else //subspecialty == null
                        {
                            var existingSP = srv.cvt_resourcepackageSet.FirstOrDefault(rp =>
                                rp.cvt_providerlocationtype.Value == newSP.cvt_providerlocationtype.Value &&
                                rp.cvt_patientlocationtype.Value == newSP.cvt_patientlocationtype.Value &&
                                rp.cvt_availabletelehealthmodality.Value == newSP.cvt_availabletelehealthmodality.Value &&
                                rp.cvt_groupappointment.Value == newSP.cvt_groupappointment.Value &&
                                rp.cvt_specialty.Id == specialty.Id &&
                                rp.cvt_hub == null &&
                                rp.statuscode.Value == (int)cvt_resourcepackage_statuscode.Active);

                            if (existingSP == null)
                            {
                                OrganizationService.Create(newSP);
                                created++;
                            }
                            else
                            {
                                Logger.WriteToFile(String.Format("Skipped creating a duplicate SP without specialty subtype for pro: {0}, pat: {1}, modality: {2}, group: {3}.", newSP.cvt_providerlocationtype.Value, newSP.cvt_patientlocationtype.Value, newSP.cvt_availabletelehealthmodality.Value, newSP.cvt_groupappointment.Value));
                                existed++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        Logger.WriteToFile(String.Format("Failed to create SP for pro: {0}, pat: {1}, modality: {2}, group: {3}. Error: {4}", newSP.cvt_providerlocationtype.Value, newSP.cvt_patientlocationtype.Value, newSP.cvt_availabletelehealthmodality.Value, newSP.cvt_groupappointment.Value, ex.Message));
                    }
                }
                exist = existed;
                fail = failed;
                return created;
            }
        }

        #endregion

        #region Team/Ownership

        /// <summary>
        /// Update BU on teams for site
        /// </summary>
        public void UpdateSiteTeamBU()
        {
            Logger.setMethod = "UpdateSiteTeamBU";
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("About to get all of the TSS Sites.");
                var sites = srv.mcs_siteSet.Where(r => r.statecode.Value == 0);
                Logger.WriteDebugMessage("Successfully retrieved tss site set. Starting data check/assign.");
                var numberUpdated = 0;
                var numberFound = 0;

                foreach (var site in sites)
                {
                    numberFound++;
                    //Get site team
                    var siteTeam = srv.TeamSet.FirstOrDefault(t => t.Name == site.mcs_name);
                    if (siteTeam != null)
                    {
                        //Do we need to verify owner and make sure it is the TSS Site, VISN BU or maybe the Facility BU?

                        //Team found, now compare BU
                        if (siteTeam.BusinessUnitId.Id != site.mcs_BusinessUnitId.Id)
                        {
                            CvtHelper.UpdateSiteTeam(siteTeam.Id, site.mcs_BusinessUnitId.Id, Logger, OrganizationService);
                            numberUpdated++;
                        }
                    }
                }
                Logger.WriteDebugMessage("Found " + numberFound.ToString() + " TSS Sites.");
                Logger.WriteDebugMessage("Updated BU on " + numberUpdated.ToString() + " TSS Site Teams to match their respective TSS Site.");
            }
        }

        public void UpdateChiefofStaffTeamNames()
        {
            Logger.setMethod = "UpdateChiefofStaffTeamNames";
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("About to get all the Chief of Staff Teams.");
                var cosTeams = srv.TeamSet.Where(t => t.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff);
                Logger.WriteDebugMessage("Successfully retrieved Chief of Staff Teams. Starting the rename process");
                var numberUpdated = 0;
                var numberFound = 0;

                foreach (var cosTeam in cosTeams)
                {
                    numberFound++;
                    if (!string.IsNullOrWhiteSpace(cosTeam.Name) && cosTeam.Name.Contains("Chiefs of Staff"))
                    {
                        var cosTeamUpdate = new Team
                        {
                            Id = cosTeam.Id,
                            Name = cosTeam.Name.Replace("Chiefs of Staff", "Chief of Staff")
                        };
                        OrganizationService.Update(cosTeamUpdate);
                        numberUpdated++;
                    }
                }
                Logger.WriteDebugMessage($"Found {numberFound} Chief of Staff Teams.\nUpdated {numberUpdated} Teams.");
            }
        }

        /// <summary>
        /// Update Team Ownership for MTSA, etc
        /// </summary>
        /// <remarks>Called by the F2BU function - step 6</remarks>
        public void FixRecordOwnership()
        {
            Logger.setMethod = "FixRecordOwnership";
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                var message = string.Empty;
                //MTSA
                //message += CvtHelper.FixOwnershipForEntity(srv.cvt_mastertsaSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                //TSSResource
                message += CvtHelper.FixOwnershipForEntity(srv.mcs_resourceSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                //Component
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_componentSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                //TSS Resource Group
                message += CvtHelper.FixOwnershipForEntity(srv.mcs_resourcegroupSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                // Group Resource
                message += CvtHelper.FixOwnershipForEntity(srv.mcs_groupresourceSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                //Telehealth Privileging
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_tssprivilegingSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                //FPPEOPPE
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_qualitycheckSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                //TSA
                //message += CvtHelper.FixOwnershipForEntity(srv.mcs_servicesSet.Where(x => x.statecode.Value == 0), Logger, OrganizationService);

                //PatResources
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_patientresourcegroupSet.Where(prg => prg.statecode.Value == 0), Logger, OrganizationService);

                //ProResources
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_providerresourcegroupSet.Where(prg => prg.statecode.Value == 0), Logger, OrganizationService);

                //TSS Sites
                message += CvtHelper.FixOwnershipForEntity(srv.mcs_siteSet.Where(s => s.statecode.Value == 0), Logger, OrganizationService);

                //Equipment

                Logger.WriteDebugMessage("Finished running the Assign All Job.  Summary: " + message);
            }
        }

        public void FixSiteOwnership()
        {
            Logger.setMethod = "FixSiteOwnership";
            Logger.WriteDebugMessage("starting");

            using (var srv = new Xrm(OrganizationService))
            {
                //TSSResource
                var message = CvtHelper.FixOwnershipForEntity(srv.mcs_resourceSet.Where(x => x.statecode.Value == 0 && x.OwningTeam == null), Logger, OrganizationService);

                //Component
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_componentSet.Where(x => x.statecode.Value == 0 && x.OwningTeam == null), Logger, OrganizationService);

                //TSS Resource Group
                message += CvtHelper.FixOwnershipForEntity(srv.mcs_resourcegroupSet.Where(x => x.statecode.Value == 0 && x.OwningTeam == null), Logger, OrganizationService);

                //Group Resource
                message += CvtHelper.FixOwnershipForEntity(srv.mcs_groupresourceSet.Where(x => x.statecode.Value == 0 && x.OwningTeam == null), Logger, OrganizationService);

                //PatResources
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_patientresourcegroupSet.Where(x => x.statecode.Value == 0 && x.OwningTeam == null), Logger, OrganizationService);

                //ProResources
                message += CvtHelper.FixOwnershipForEntity(srv.cvt_providerresourcegroupSet.Where(x => x.statecode.Value == 0 && x.OwningTeam == null), Logger, OrganizationService);

                //TSS Sites
                message += CvtHelper.FixOwnershipForEntity(srv.mcs_siteSet.Where(x => x.statecode.Value == 0 && x.OwningTeam == null), Logger, OrganizationService);

                Logger.WriteDebugMessage("Finished running the Assign All Job.  Summary: " + message);
            }
        }
        /// <summary>
        /// Gets list of all TYPED teams and appropriately assigns predetermined roles
        /// </summary>
        public void UpdateTeamSecurityRoles()
        {
            Logger.setMethod = "UpdateTeamSecurityRoles";
            Logger.WriteDebugMessage("starting UpdateTeamSecurityRoles");

            using (var srv = new Xrm(OrganizationService))
            {
                var teams = srv.TeamSet.Where(t => t.TeamType.Value != (int)TeamTeamType.Access).ToList();
                var numberAdded = 0;
                var numberRemoved = 0;
                var numberUpdated = 0;
                Logger.WriteDebugMessage("Teams Found: " + teams.Count());
                foreach (var team in teams)
                {
                    try
                    {
                        int addedRoles = 0;
                        int removedRoles = 0;

                        CvtHelper.UpdateSecurityRoles(team.Id, Team.EntityLogicalName, OrganizationService, Logger, out addedRoles, out removedRoles);
                        if (removedRoles + addedRoles > 0)
                        {
                            Logger.WriteDebugMessage(String.Format("Team Security Role maintenance: {0}, Added {1}, Removed {2}", team.Name, addedRoles, removedRoles));
                            numberUpdated++;

                            if (removedRoles > 0)
                                numberRemoved++;
                            if (addedRoles > 0)
                                numberAdded++;
                        }
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
                            Logger.WriteDebugMessage(ex.Message.Substring(6) + team.Name);
                            throw new InvalidPluginExecutionException(ex.Message.Substring(6));
                        }
                        else
                        {
                            Logger.WriteToFile(ex.Message + team.Name);
                            throw new InvalidPluginExecutionException(ex.Message);
                        }
                    }
                }
                Logger.WriteDebugMessage("Found " + teams.Count().ToString() + " Teams");
                Logger.WriteDebugMessage("Number of Teams Updated " + numberUpdated.ToString());
                Logger.WriteDebugMessage("Added roles to " + numberAdded.ToString() + " Teams");
                Logger.WriteDebugMessage("Removed roles from " + numberRemoved.ToString() + " Teams");

                Logger.WriteDebugMessage("ending UpdateTeamSecurityRoles");
            }
        }

        /// <summary>
        /// Updates the User's Primary Team
        /// </summary>
        public void UpdateUserPrimaryTeam()
        {
            Logger.setMethod = "UpdateUserPrimaryTeam";
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("About to get all of the Active System Users.");
                var allUsers = srv.SystemUserSet.Where(u => u.IsDisabled == false);

                foreach (var user in allUsers)
                {
                    CvtHelper.UpdatePrimaryTeam(user.Id, OrganizationService, Logger);
                }
                Logger.setMethod = "UpdateUserPrimaryTeam";
                Logger.WriteDebugMessage("Finished Updating User Primary Teams.");
            }
        }

        /// <summary>
        /// Update all the Team's names
        /// </summary>
        public void UpdateTeamNames()
        {
            Logger.setMethod = "UpdateTeamNames";
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.WriteDebugMessage("About to get all of the Active System Users.");
                var allTeams = srv.TeamSet.Where(t => t.TeamType.Value != (int)TeamTeamType.Access);

                foreach (var team in allTeams)
                {
                    var createName = CvtHelper.DeriveName(team, false, Logger, OrganizationService);
                    if (createName != "")
                    {
                        Team updateTeam = new Team()
                        {
                            Id = team.Id,
                            Name = createName
                        };
                        try
                        {
                            OrganizationService.Update(updateTeam);
                            Logger.WriteDebugMessage("Updated Team Name. From: " + team.Name + "; To: " + updateTeam.Name);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteDebugMessage("Failed to update Team Name. From: " + team.Name + "; To: " + updateTeam.Name + ". Exception: " + ex.Message);
                        }
                    }

                }
                Logger.setMethod = "UpdateTeamNames";
                Logger.WriteDebugMessage("Finished Updating Team Names.");
            }
        }

        public void SetupTeams()
        {
            Logger.setMethod = "SetupTeams";
            Logger.WriteGranularTimingMessage("Starting SetupTeams");
            using (var srv = new Xrm(OrganizationService))
            {
                var facilities = (from f in srv.mcs_facilitySet
                                  select f.Id).ToList<Guid>();
                var facWithFTCTeams = (from f in srv.mcs_facilitySet
                                       join t in srv.TeamSet on f.Id equals t.cvt_Facility.Id
                                       where t.cvt_Type.Value == (int)Teamcvt_Type.FTC
                                       where t.cvt_Facility != null
                                       select f.Id).ToList<Guid>();
                var facWithCPTeams = (from f in srv.mcs_facilitySet
                                      join t in srv.TeamSet on f.Id equals t.cvt_Facility.Id
                                      where t.cvt_Type.Value == (int)Teamcvt_Type.CredentialingandPrivileging
                                      where t.cvt_Facility != null
                                      select f.Id).ToList<Guid>();
                int FTCNeeded = 0;
                int FTCCreated = 0;
                int CPNeeded = 0;
                int CPCreated = 0;

                IEnumerable<Guid> differenceFTC = facilities.Except(facWithFTCTeams);
                IEnumerable<Guid> differenceCP = facilities.Except(facWithCPTeams);

                foreach (Guid g in differenceFTC)
                {
                    FTCNeeded++;
                    var thisFac = (mcs_facility)OrganizationService.Retrieve(mcs_facility.EntityLogicalName, g, new ColumnSet(true));
                    var team = new Team()
                    {
                        Name = String.Format("FTC Approval Group @ {0}", thisFac.mcs_name),
                        cvt_Type = new OptionSetValue((int)Teamcvt_Type.FTC),
                        BusinessUnitId = thisFac.mcs_BusinessUnitId,
                        cvt_Facility = new EntityReference() { Id = thisFac.Id, Name = thisFac.mcs_name, LogicalName = thisFac.LogicalName }
                    };
                    try
                    {
                        OrganizationService.Create(team);
                        FTCCreated++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteDebugMessage("Failed to create Team: " + team.Name + ".Exception: " + ex.Message);
                    }
                }

                foreach (Guid g in differenceCP)
                {
                    CPNeeded++;
                    var thisFac = (mcs_facility)OrganizationService.Retrieve(mcs_facility.EntityLogicalName, g, new ColumnSet(true));
                    var team = new Team()
                    {
                        Name = String.Format("Credentialing and Privileging Officer Approval Group @ {0}", thisFac.mcs_name),
                        cvt_Type = new OptionSetValue((int)Teamcvt_Type.CredentialingandPrivileging),
                        BusinessUnitId = thisFac.mcs_BusinessUnitId,
                        cvt_Facility = new EntityReference() { Id = thisFac.Id, Name = thisFac.mcs_name, LogicalName = thisFac.LogicalName }
                    };
                    try
                    {
                        OrganizationService.Create(team);
                        CPCreated++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteDebugMessage("Failed to create Team: " + team.Name + ".Exception: " + ex.Message);
                    }
                }

                Logger.WriteDebugMessage(String.Format("Created {0}/{1} FTC teams.", FTCCreated, FTCNeeded));
                Logger.WriteDebugMessage(String.Format("Created {0}/{1} CP teams.", CPCreated, CPNeeded));
            }
        }

        public void DeleteAllInventoryImportRecords()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                Logger.setMethod = "DeleteAllInventoryImportRecords";
                var fieldMismatch = from f in srv.cvt_fieldmismatchSet
                                    select f.Id;
                foreach (var id in fieldMismatch)
                    DeleteRecord(cvt_fieldmismatch.EntityLogicalName, id);

                var stagingComponents = from s in srv.cvt_stagingcomponentSet
                                        select s.Id;
                foreach (var id in stagingComponents)
                    DeleteRecord(cvt_stagingcomponent.EntityLogicalName, id);

                var stagingResources = from f in srv.cvt_stagingresourceSet
                                       select f.Id;
                foreach (var id in stagingResources)
                    DeleteRecord(cvt_stagingresource.EntityLogicalName, id);

                Logger.WriteDebugMessage("DeleteAllInventoryImportRecords Complete");
            }
        }

        private void DeleteRecord(string entityLogicalName, Guid id)
        {
            Logger.setMethod = "DeleteRecord";
            try
            {
                OrganizationService.Delete(entityLogicalName, id);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"Error occured while deleting the {entityLogicalName} record with id {id}\n{ex.Message}");
            }
        }

        public void UpdateInventoryUniqueIdwhenEmpty()
        {
            UpdateUniqueIdwhenNotSet(Logger, OrganizationService);
        }

        internal static void UpdateUniqueIdwhenNotSet(MCSLogger logger, IOrganizationService organizationService)
        {
            logger.setMethod = "UpdateUniqueIdwhenNotSet";
            using (var srv = new Xrm(organizationService))
            {
                var stagingResources = from f in srv.cvt_stagingresourceSet
                                       where f.statecode.Value == (int)cvt_stagingresourceState.Active && f.cvt_uniqueid == null
                                       select f.Id;

                var fieldMatches = from f in srv.cvt_fieldmismatchSet
                                   where f.statecode.Value == (int)cvt_fieldmismatchState.Active && f.cvt_fieldschemaname == "cvt_uniqueid" && f.cvt_importdisplayvalue == null
                                   select f.Id;
                foreach (var resource in stagingResources)
                {
                    var updateResource = new cvt_stagingresource { Id = resource, cvt_uniqueid = "Not Registered" };

                    try
                    {
                        organizationService.Update(updateResource);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteToFile(
                            $"Error occured while updating the Unique Id field of {cvt_stagingresource.EntityLogicalName} record with id {resource}\n{ex.Message}");
                    }
                }

                foreach (var fieldMismatch in fieldMatches)
                {
                    var updateFieldMismatch = new cvt_fieldmismatch { Id = fieldMismatch, cvt_importdisplayvalue = "Not Registered", cvt_importinternalvalue = "Not Registered" };

                    try
                    {
                        organizationService.Update(updateFieldMismatch);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteToFile(
                            $"Error occured while updating the import value field of {cvt_fieldmismatch.EntityLogicalName} record with id {fieldMismatch}\n{ex.Message}");
                    }
                }
            }
        }

        public void AssignInventoryToTeam()
        {
            AssignInventoryToNtthd(Logger, OrganizationService);
        }

        internal static void AssignInventoryToNtthd(MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.setMethod = "AssignInventoryToTeam";
            using (var srv = new Xrm(OrganizationService))
            {
                var findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == "NTTHD");
                if (findTeam != null)
                {
                    var stagingResources = from f in srv.cvt_stagingresourceSet
                                           where f.statecode.Value == (int)cvt_stagingresourceState.Active && f.mcs_Facility == null
                                           select f.Id;

                    EntityReference assignOwner = new EntityReference()
                    {
                        LogicalName = Team.EntityLogicalName,
                    };

                    foreach (var resource in stagingResources)
                    {
                        assignOwner.Id = findTeam.Id;

                        AssignRequest assignRequest = new AssignRequest()
                        {
                            Assignee = assignOwner,
                            Target = new EntityReference(cvt_stagingresource.EntityLogicalName, resource)
                        };

                        try
                        {
                            OrganizationService.Execute(assignRequest);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile($"Error occured while assigning the {cvt_stagingresource.EntityLogicalName} record with id {resource} to NTTHD Team\n{ex.Message}");
                        }
                    }
                }
                else
                    Logger.WriteToFile("Could not find team with the name 'NTTHD'. Please create the team, add appropriate roles and Run AssignInventoryToTeam again");
            }

            Logger.WriteDebugMessage("AssignInventoryToTeam Complete");
        }

        #endregion

        #region Align Locations
        /// <summary>
        /// Goes through all Facilities and attempts to Align the Locations for the Facility, Site, TSS Resource
        /// </summary>
        public void UpdateFacilitySiteTSSResource()
        {
            Logger.WriteDebugMessage("Starting method UpdateFacilitySiteTSSResource");

            Logger.setMethod = "UpdateFacilitySiteTSSResource";
            Logger.WriteGranularTimingMessage("Starting UpdateFacilitySiteTSSResource");
            using (var srv = new Xrm(OrganizationService))
            {
                //Query for all Facilities
                var allFacilities = srv.mcs_facilitySet;

                //Logger.WriteDebugMessage(string.Format("Invoking the method CvtHelper.AlignLocations. Facility Count {0}", allFacilities.Count<mcs_facility>()));

                foreach (var facility in allFacilities)
                {
                    try
                    {
                        CvtHelper.AlignLocations(facility, OrganizationService, Logger);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile("Failed CvtHelper.AlignLocations. " + CvtHelper.BuildExceptionMessage(ex) + ex.StackTrace);
                    }
                }
            }
            Logger.setMethod = "UpdateFacilitySiteTSSResource";
            Logger.WriteGranularTimingMessage("Ending UpdateFacilitySiteTSSResource");
        }

        /// <summary>
        /// Update Facility and VISN on all TSS Resources
        /// </summary>
        public void UpdateTSSResourceData()
        {
            Logger.setMethod = "UpdateTSSResourceData";
            Logger.WriteGranularTimingMessage("Starting UpdateTSSResourceData");
            using (var srv = new Xrm(OrganizationService))
            {
                var resources = srv.mcs_resourceSet;
                var numberFound = 0;
                var numberUpdated = 0;

                Logger.WriteDebugMessage("Starting loop for Resources");

                foreach (var resource in resources)
                {
                    numberFound++;
                    if (resource.mcs_RelatedSiteId != null) //Site is not null
                    {
                        //Get Facility of Site
                        mcs_site resourceSite = (mcs_site)OrganizationService.Retrieve(mcs_site.EntityLogicalName, resource.mcs_RelatedSiteId.Id, new ColumnSet(true));
                        Guid resourceFacility = (resource.mcs_Facility != null) ? resource.mcs_Facility.Id : Guid.Empty;
                        Guid siteFacility = (resourceSite.mcs_FacilityId != null) ? resourceSite.mcs_FacilityId.Id : Guid.Empty;
                        Guid resourceVISN = (resource.mcs_BusinessUnitId != null) ? resource.mcs_BusinessUnitId.Id : Guid.Empty;

                        mcs_facility resourceFacilityEntity = (mcs_facility)OrganizationService.Retrieve(mcs_facility.EntityLogicalName, resource.mcs_Facility.Id, new ColumnSet(true));
                        Guid facilityVisn = (resourceFacilityEntity.mcs_VISN != null) ? resourceFacilityEntity.mcs_VISN.Id : Guid.Empty;
                        var TSSResourceUpdate = false;
                        var update = new mcs_resource();

                        //check if need to update facility
                        if (resourceFacility != siteFacility)
                        {
                            update.mcs_Facility = resourceSite.mcs_FacilityId;
                            TSSResourceUpdate = true;
                        }

                        //check if need to update visn
                        if (resourceVISN != facilityVisn)
                        {
                            update.mcs_BusinessUnitId = resourceFacilityEntity.mcs_VISN;
                            TSSResourceUpdate = true;
                        }

                        if (TSSResourceUpdate == true)
                        {
                            update.Id = resource.Id;
                            Logger.WriteDebugMessage(string.Format("Starting the Resource update \nId:{0}, BU: {1}, Facility: {2}", update.Id, update.mcs_Facility?.Id ?? Guid.Empty, update.mcs_BusinessUnitId?.Id ?? Guid.Empty));
                            try
                            {
                                OrganizationService.Update(update);
                            }
                            catch (FaultException ex)
                            {
                                Logger.WriteDebugMessage("Failed to update TSS Resource :" + CvtHelper.BuildExceptionMessage(ex) + ex.StackTrace);
                            }
                            numberUpdated++;
                        }
                    }
                }
                Logger.WriteDebugMessage("Found " + numberFound.ToString() + " TSS Resources");
                Logger.WriteDebugMessage("Updated " + numberUpdated.ToString() + " TSS Resources");
            }
        }

        //Update all Sites
        public void UpdateSiteTeamLookup()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var sites = srv.mcs_siteSet;
                var numberFound = 0;
                var numberUpdated = 0;

                foreach (var site in sites)
                {
                    try
                    {
                        numberFound++;

                        //compare team in lookup
                        if (site.cvt_TSSSiteTeam != null && site.cvt_TSSSiteTeam.Name == site.mcs_name)
                            continue;

                        //Find Site Team
                        var siteTeam = srv.TeamSet.FirstOrDefault(t => t.Name == site.mcs_name);

                        if (siteTeam == null) //Site Team is not found
                        {
                            Logger.WriteToFile("Need to create Site Team for Site: " + site.mcs_name);
                            continue;
                        }
                        var update = new mcs_site()
                        {
                            Id = site.Id,
                            cvt_TSSSiteTeam = new EntityReference(Team.EntityLogicalName, siteTeam.Id)
                        };
                        OrganizationService.Update(update);
                        numberUpdated++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteDebugMessage(ex.Message);
                    }

                }
                Logger.WriteDebugMessage("Found " + numberFound.ToString() + " TSS Sites");
                Logger.WriteDebugMessage("Updated " + numberUpdated.ToString() + " TSS Sites with correct Site Team");
            }
        }
        #endregion

        #region ArchivedFunctions
        /// <summary>
        /// Update all user option time zones based on user records
        /// </summary>
        public void UpdateUserTimeZones()
        {
            Logger.setMethod = "UpdateUserTimeZones";
            Logger.WriteDebugMessage("Update User Time Zones Begin");
            using (var srv = new Xrm(OrganizationService))
            {

                //var orphService = from service in srv.ServiceSet
                //  join tsa in srv.mcs_servicesSet on service.Id equals tsa.mcs_RelatedServiceId.Id into JoinedSvcTsa
                //  from tsa in JoinedSvcTsa.DefaultIfEmpty()

                var users = (from u in srv.SystemUserSet
                             join s in srv.UserSettingsSet on u.Id equals s.SystemUserId.Value
                             select new
                             {
                                 u.Id,
                                 u.cvt_TimeZone,
                                 s.TimeZoneCode
                             });
                var numberFound = 0;
                var numberUpdated = 0;
                foreach (var user in users)
                {
                    numberFound++;
                    if (user.cvt_TimeZone != user.TimeZoneCode && user.cvt_TimeZone != null)
                    {
                        var updatedSetting = new UserSettings()
                        {
                            Id = user.Id,
                            TimeZoneCode = user.cvt_TimeZone
                        };
                        OrganizationService.Update(updatedSetting);
                        numberUpdated++;
                    }
                    else
                    {


                    }
                }
                Logger.WriteGranularTimingMessage("Found " + numberFound.ToString() + " Resources");
                Logger.WriteGranularTimingMessage("Updated " + numberUpdated.ToString() + " Resources");
            }
            Logger.WriteDebugMessage("Update User Time Zones End");
        }

        /// <summary>
        /// This utility method queries for all Resources, then copies their system generated ID into an exposed field that can be searched and used by regular users
        /// </summary>
        public void UpdateResourceIDs()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var resources = (from r in srv.mcs_resourceSet
                                 where (r.cvt_systemtype.Value == 1)
                                 select new
                                 {
                                     r.Id,
                                     r.cvt_Identifier
                                 });
                var numberFound = 0;
                var numberUpdated = 0;
                foreach (var resource in resources)
                {
                    numberFound++;
                    if (resource.cvt_Identifier == null)
                    {
                        var update = new mcs_resource()
                        {
                            cvt_Identifier = resource.Id.ToString(),
                            Id = resource.Id
                        };
                        OrganizationService.Update(update);
                        numberUpdated++;
                    }
                }
                Logger.WriteGranularTimingMessage("Found " + numberFound.ToString() + " Resources");
                Logger.WriteGranularTimingMessage("Updated " + numberUpdated.ToString() + " Resources");
            }
        }

        /// <summary>
        /// This method updates component record with the resource ID
        /// </summary>
        public void UpdateComponentResourceIDs()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var components = srv.cvt_componentSet;
                var numberUpdated = 0;
                var numberFound = 0;
                foreach (var component in components)
                {
                    numberFound++;
                    if (component.cvt_ParentResourceIdentifier != null)
                    {
                        var update = new cvt_component()
                        {
                            Id = component.Id,
                            cvt_ParentResourceIdentifier = component.cvt_relatedresourceid.Id.ToString()
                        };
                        OrganizationService.Update(update);
                        numberUpdated++;
                    }
                }
                Logger.WriteGranularTimingMessage("Found " + numberFound.ToString() + " Components");
                Logger.WriteGranularTimingMessage("Updated " + numberUpdated.ToString() + " Components");
            }
        }
        #endregion

        #region SchedulingRedesign

        public void DeleteServices()
        {
            using (var srv = new Xrm(OrganizationService))
            {

                Logger.WriteDebugMessage(String.Format("Before: Number of ResourceSpec: {0}. Number of CBG: {1}.", srv.ResourceSpecSet.ToList().Count, srv.ConstraintBasedGroupSet.ToList().Count));



                var serviceSet = srv.ServiceSet.ToList();
                var count = 0;


                foreach (var ser in serviceSet)
                {
                    try
                    {
                        OrganizationService.Delete(Service.EntityLogicalName, ser.Id);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(ex.Message);
                        ResourceSpec rs = new ResourceSpec()
                        {
                            Id = ser.ResourceSpecId.Id,
                            GroupObjectId = new Guid("CAEE5E35-ED33-E611-80C8-00155D175D02")
                        };
                        OrganizationService.Update(rs);
                    }
                }
                Logger.WriteDebugMessage(String.Format("Finished Deleting Services. {0}/{1}.", count, serviceSet.Count));
                Logger.WriteDebugMessage(String.Format("Remaining {0} Services.", srv.ServiceSet.ToList().Count));
                Logger.WriteDebugMessage(String.Format("After: Number of ResourceSpec: {0}. Number of CBG: {1}.", srv.ResourceSpecSet.ToList().Count, srv.ConstraintBasedGroupSet.ToList().Count));
            }
        }

        public void UpdateSAService()
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var SA = srv.ServiceAppointmentSet;

                foreach (var s in SA)
                {
                    var editSA = new ServiceAppointment()
                    {
                        Id = s.Id,
                        ServiceId = null
                    };

                    if (s.StateCode.Value == ServiceAppointmentState.Canceled || s.StateCode.Value == ServiceAppointmentState.Closed)
                    {
                        SetStateRequest changeSAState = new SetStateRequest()
                        {
                            EntityMoniker = new EntityReference(ServiceAppointment.EntityLogicalName, s.Id),
                            State = new OptionSetValue((int)ServiceAppointmentState.Open),
                            Status = new OptionSetValue((int)serviceappointment_statuscode.RequestedOpen)
                        };
                        OrganizationService.Execute(changeSAState);

                        OrganizationService.Update(editSA);

                        changeSAState.State = new OptionSetValue((int)s.StateCode.Value);
                        changeSAState.Status = new OptionSetValue((int)s.StatusCode.Value);
                        OrganizationService.Execute(changeSAState);
                    }
                    else
                        OrganizationService.Update(editSA);
                }
            }
            var sa = new ServiceAppointment()
            {
                ServiceId = null,
                Id = new Guid("EE3FFCA4-BF4A-E511-9C2D-00155D5575E0")
            };
            try
            {
                OrganizationService.Update(sa);
                Logger.WriteDebugMessage("Success.");
            }
            catch (Exception ex)
            {
                Logger.WriteDebugMessage(ex.Message);
            }
        }
        #endregion

        #region AbstractClassRequiredMethods
        public override string McsSettingsDebugField
        {
            get { return "mcs_serviceplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}