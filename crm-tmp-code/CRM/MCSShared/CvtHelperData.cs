using MCS.ApplicationInsights;
using MCSUtilities2011;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace MCSShared
{
    public static partial class CvtHelper
    {
        #region Team
        /// <summary>
        /// Passing in either a SystemUser or a Team, associate security roles
        /// </summary>
        /// <param name="thisId">Accepts either Team or SystemUser Guids</param>
        /// <param name="EntityLogicalName"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void UpdateSecurityRoles(Guid thisId, string EntityLogicalName, IOrganizationService OrganizationService, MCSLogger Logger, out int addRoles, out int removeRoles)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                //Vars to Track Counts
                bool addedRoles = false;
                bool removedRoles = false;
                addRoles = 0;
                removeRoles = 0;

                IQueryable currentRoleResults = null;
                string rolesToAdd = "";
                int totalRoles = 0;
                Guid businessUnitId = Guid.Empty;
                string thisRecordName = "";
                string displayMessage = "";
                EntityReferenceCollection RolesToAssociate = new EntityReferenceCollection();
                EntityReferenceCollection RolesToDisassociate = new EntityReferenceCollection();

                if (EntityLogicalName == Team.EntityLogicalName)
                {
                    var thisTeam = srv.TeamSet.First(t => t.Id == thisId);
                    if (thisTeam.BusinessUnitId == null)
                    {
                        Logger.WriteDebugMessage("No BU set on Team, so no roles were added.");
                        //Maybe check For Facility value and align to that BU.
                        return; //no BU, so exit                      
                    }
                    businessUnitId = thisTeam.BusinessUnitId.Id;
                    thisRecordName = thisTeam.Name;
                    rolesToAdd = "TMP User";
                    if (thisTeam.cvt_Type != null)
                    {
                        switch (thisTeam.cvt_Type.Value)
                        {
                            case (int)Teamcvt_Type.FTC:
                                //User + Approver + FTC
                                rolesToAdd += "|TMP TSA Approver|TMP Scheduling Package Manager";
                                break;
                            case (int)Teamcvt_Type.ServiceChief:
                                rolesToAdd += "|TMP TSA Approver|TMP PPE Feedback";
                                break;
                            case (int)Teamcvt_Type.ChiefofStaff:
                                rolesToAdd += "|TMP TSA Approver";
                                break;
                            case (int)Teamcvt_Type.CredentialingandPrivileging:
                                rolesToAdd += "|TMP TSA Approver|TMP Privileging";
                                break;
                            //case "917290004": //TSA Notification
                            //    break;
                            case (int)Teamcvt_Type.Scheduler:
                                //User + Scheduler
                                rolesToAdd += "|TMP Scheduler";
                                break;
                            case (int)Teamcvt_Type.DataAdministrator:
                                rolesToAdd += "|TMP Field Application Administrator";
                                break;
                            case (int)Teamcvt_Type.Staff:
                                rolesToAdd += "|TMP Resource Manager|TMP Scheduling Package Manager";
                                break;
                            case (int)Teamcvt_Type.HubDirector:
                                rolesToAdd += "|TMP Resource Manager|TMP Scheduling Package Manager|TMP Hub TSA Approver";
                                break;
                            case (int)Teamcvt_Type.HubTSAManager:
                                rolesToAdd += "|TMP Resource Manager|TMP Scheduling Package Manager|TMP Hub SP Manager|TMP Hub TSA Approver";
                                break;
                        }
                    }
                    else
                    {
                        if (thisTeam.Name.Contains("("))
                            rolesToAdd += "|TMP Site Team";
                        else if (thisTeam.Name.Contains("Operations Manual"))
                            rolesToAdd += "|TMP Manual Managers";
                        else
                            rolesToAdd += "|TMP BU Team";
                    }

                    displayMessage = "Missing Default Role(s) systematically associated (based on Type) to Team: ";
                    currentRoleResults = srv.TeamRolesSet.Where(tr => tr.TeamId == thisId);
                }
                else if (EntityLogicalName == SystemUser.EntityLogicalName)
                {
                    var thisUser = srv.SystemUserSet.First(u => u.Id == thisId);
                    if (thisUser.cvt_SecurityRolesString == null)
                        return; //Future: Auto add Roles based on Type field on User Record (like Team)
                    rolesToAdd = thisUser.cvt_SecurityRolesString;
                    businessUnitId = thisUser.BusinessUnitId.Id;
                    thisRecordName = thisUser.FullName;
                    displayMessage = " Former Roles re-associated in new Business Unit for user: ";
                    currentRoleResults = srv.SystemUserRolesSet.Where(u => u.SystemUserId == thisId);
                }

                //Common Code
                if (rolesToAdd == "TMP User")
                {
                    //Check for Site Team, Facility Team, VISN Team.
                    //Could query for site or facility or visn for exact match of name and if so, add team depending on which result?
                    Logger.WriteDebugMessage(string.Format("No roles to assign for {0}: {1}", EntityLogicalName, thisRecordName));
                    return;
                }
                var SecRoles = rolesToAdd.Contains('|') ? rolesToAdd.Split('|') : new string[] { rolesToAdd }; //split or only one value
                totalRoles = SecRoles.Count();

                try
                {
                    //Get the Roles to Add
                    foreach (var defaultRole in SecRoles)
                    {
                        bool matchToIgnoreAdd = false;
                        //If no current roles, do not try to match
                        if (currentRoleResults != null)
                        {
                            foreach (Entity role in currentRoleResults)
                            {
                                if (matchToIgnoreAdd)
                                    break;

                                Guid roleGuid = Guid.Empty;
                                if (role != null && role.Attributes != null && role.Attributes["roleid"] != null)
                                    roleGuid = (Guid)(role.Attributes["roleid"]);
                                var roleRecord = srv.RoleSet.FirstOrDefault(r => r.Id == roleGuid);
                                if ((roleRecord != null) && (roleRecord.Name != null))
                                    matchToIgnoreAdd = (defaultRole == roleRecord.Name) ? true : false;
                            }
                        }
                        if (!matchToIgnoreAdd) //No Match, so add
                        {
                            var newRole = srv.RoleSet.FirstOrDefault(sr => sr.BusinessUnitId.Id == businessUnitId && sr.Name == defaultRole.ToString());
                            RolesToAssociate.Add(new EntityReference(Role.EntityLogicalName, newRole.Id));
                            addRoles++;
                            addedRoles = true;
                        }
                    }

                    //If no current roles, do not try to match
                    if (currentRoleResults != null)
                    {
                        //Get the Roles to Remove
                        foreach (Entity role in currentRoleResults)
                        {
                            Guid roleGuid = Guid.Empty;
                            if (role != null && role.Attributes != null && role.Attributes["roleid"] != null)
                                roleGuid = (Guid)(role.Attributes["roleid"]);
                            var roleRecord = srv.RoleSet.FirstOrDefault(r => r.Id == roleGuid);
                            bool matchToRemove = SecRoles.FirstOrDefault(r => r == roleRecord.Name) == null;
                            if (matchToRemove)
                            {
                                RolesToDisassociate.Add(new EntityReference(Role.EntityLogicalName, roleGuid));
                                removeRoles++;
                                removedRoles = true;
                            }
                        }
                    }

                    if (addedRoles)
                    {
                        OrganizationService.Associate(EntityLogicalName, thisId, new Relationship(EntityLogicalName.ToLower() + "roles_association"), RolesToAssociate);
                        Logger.WriteDebugMessage(string.Format("{0}/{1} {2}{3}", addRoles, totalRoles, displayMessage, thisRecordName));
                    }

                    if (removedRoles)
                    {
                        OrganizationService.Disassociate(EntityLogicalName, thisId, new Relationship(EntityLogicalName.ToLower() + "roles_association"), RolesToDisassociate);
                        Logger.WriteDebugMessage(string.Format("{0} role(s) removed from {1}", removeRoles, thisRecordName));
                    }
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
        }

        /// <summary>
        /// Passing in either a SystemUser or a Team, associate security roles
        /// </summary>
        /// <param name="thisId">Accepts either Team or SystemUser Guids</param>
        /// <param name="EntityLogicalName"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        internal static void UpdateSecurityRoles(Guid thisId, string EntityLogicalName, IOrganizationService OrganizationService, PluginLogger Logger, out int addRoles, out int removeRoles)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                //Vars to Track Counts
                bool addedRoles = false;
                bool removedRoles = false;
                addRoles = 0;
                removeRoles = 0;

                IQueryable currentRoleResults = null;
                string rolesToAdd = "";
                int totalRoles = 0;
                Guid businessUnitId = Guid.Empty;
                string thisRecordName = "";
                string displayMessage = "";
                EntityReferenceCollection RolesToAssociate = new EntityReferenceCollection();
                EntityReferenceCollection RolesToDisassociate = new EntityReferenceCollection();

                if (EntityLogicalName == Team.EntityLogicalName)
                {
                    var thisTeam = srv.TeamSet.First(t => t.Id == thisId);
                    if (thisTeam.BusinessUnitId == null)
                    {
                        Logger.Trace("No BU set on Team, so no roles were added.");
                        //Maybe check For Facility value and align to that BU.
                        return; //no BU, so exit                      
                    }
                    businessUnitId = thisTeam.BusinessUnitId.Id;
                    thisRecordName = thisTeam.Name;
                    rolesToAdd = "TMP User";
                    if (thisTeam.cvt_Type != null)
                    {
                        switch (thisTeam.cvt_Type.Value)
                        {
                            case (int)Teamcvt_Type.FTC:
                                //User + Approver + FTC
                                rolesToAdd += "|TMP TSA Approver|TMP Scheduling Package Manager";
                                break;
                            case (int)Teamcvt_Type.ServiceChief:
                                rolesToAdd += "|TMP TSA Approver|TMP PPE Feedback";
                                break;
                            case (int)Teamcvt_Type.ChiefofStaff:
                                rolesToAdd += "|TMP TSA Approver";
                                break;
                            case (int)Teamcvt_Type.CredentialingandPrivileging:
                                rolesToAdd += "|TMP TSA Approver|TMP Privileging";
                                break;
                            //case "917290004": //TSA Notification
                            //    break;
                            case (int)Teamcvt_Type.Scheduler:
                                //User + Scheduler
                                rolesToAdd += "|TMP Scheduler";
                                break;
                            case (int)Teamcvt_Type.DataAdministrator:
                                rolesToAdd += "|TMP Field Application Administrator";
                                break;
                            case (int)Teamcvt_Type.Staff:
                                rolesToAdd += "|TMP Resource Manager|TMP Scheduling Package Manager";
                                break;
                            case (int)Teamcvt_Type.HubDirector:
                                rolesToAdd += "|TMP Resource Manager|TMP Scheduling Package Manager|TMP Hub TSA Approver";
                                break;
                            case (int)Teamcvt_Type.HubTSAManager:
                                rolesToAdd += "|TMP Resource Manager|TMP Scheduling Package Manager|TMP Hub SP Manager|TMP Hub TSA Approver";
                                break;
                        }
                    }
                    else
                    {
                        if (thisTeam.Name.Contains("("))
                            rolesToAdd += "|TMP Site Team";
                        else if (thisTeam.Name.Contains("Operations Manual"))
                            rolesToAdd += "|TMP Manual Managers";
                        else
                            rolesToAdd += "|TMP BU Team";
                    }

                    displayMessage = "Missing Default Role(s) systematically associated (based on Type) to Team: ";
                    currentRoleResults = srv.TeamRolesSet.Where(tr => tr.TeamId == thisId);
                }
                else if (EntityLogicalName == SystemUser.EntityLogicalName)
                {
                    var thisUser = srv.SystemUserSet.First(u => u.Id == thisId);
                    if (thisUser.cvt_SecurityRolesString == null)
                        return; //Future: Auto add Roles based on Type field on User Record (like Team)
                    rolesToAdd = thisUser.cvt_SecurityRolesString;
                    businessUnitId = thisUser.BusinessUnitId.Id;
                    thisRecordName = thisUser.FullName;
                    displayMessage = " Former Roles re-associated in new Business Unit for user: ";
                    currentRoleResults = srv.SystemUserRolesSet.Where(u => u.SystemUserId == thisId);
                }

                //Common Code
                if (rolesToAdd == "TMP User")
                {
                    //Check for Site Team, Facility Team, VISN Team.
                    //Could query for site or facility or visn for exact match of name and if so, add team depending on which result?
                    Logger.Trace(string.Format("No roles to assign for {0}: {1}", EntityLogicalName, thisRecordName));
                    return;
                }
                var SecRoles = rolesToAdd.Contains('|') ? rolesToAdd.Split('|') : new string[] { rolesToAdd }; //split or only one value
                totalRoles = SecRoles.Count();

                try
                {
                    //Get the Roles to Add
                    foreach (var defaultRole in SecRoles)
                    {
                        bool matchToIgnoreAdd = false;
                        //If no current roles, do not try to match
                        if (currentRoleResults != null)
                        {
                            foreach (Entity role in currentRoleResults)
                            {
                                if (matchToIgnoreAdd)
                                    break;

                                Guid roleGuid = Guid.Empty;
                                if (role != null && role.Attributes != null && role.Attributes["roleid"] != null)
                                    roleGuid = (Guid)(role.Attributes["roleid"]);
                                var roleRecord = srv.RoleSet.FirstOrDefault(r => r.Id == roleGuid);
                                if ((roleRecord != null) && (roleRecord.Name != null))
                                    matchToIgnoreAdd = (defaultRole == roleRecord.Name) ? true : false;
                            }
                        }
                        if (!matchToIgnoreAdd) //No Match, so add
                        {
                            var newRole = srv.RoleSet.FirstOrDefault(sr => sr.BusinessUnitId.Id == businessUnitId && sr.Name == defaultRole.ToString());
                            RolesToAssociate.Add(new EntityReference(Role.EntityLogicalName, newRole.Id));
                            addRoles++;
                            addedRoles = true;
                        }
                    }

                    //If no current roles, do not try to match
                    if (currentRoleResults != null)
                    {
                        //Get the Roles to Remove
                        foreach (Entity role in currentRoleResults)
                        {
                            Guid roleGuid = Guid.Empty;
                            if (role != null && role.Attributes != null && role.Attributes["roleid"] != null)
                                roleGuid = (Guid)(role.Attributes["roleid"]);
                            var roleRecord = srv.RoleSet.FirstOrDefault(r => r.Id == roleGuid);
                            bool matchToRemove = SecRoles.FirstOrDefault(r => r == roleRecord.Name) == null;
                            if (matchToRemove)
                            {
                                RolesToDisassociate.Add(new EntityReference(Role.EntityLogicalName, roleGuid));
                                removeRoles++;
                                removedRoles = true;
                            }
                        }
                    }

                    if (addedRoles)
                    {
                        OrganizationService.Associate(EntityLogicalName, thisId, new Relationship(EntityLogicalName.ToLower() + "roles_association"), RolesToAssociate);
                        Logger.Trace(string.Format("{0}/{1} {2}{3}", addRoles, totalRoles, displayMessage, thisRecordName));
                    }

                    if (removedRoles)
                    {
                        OrganizationService.Disassociate(EntityLogicalName, thisId, new Relationship(EntityLogicalName.ToLower() + "roles_association"), RolesToDisassociate);
                        Logger.Trace(string.Format("{0} role(s) removed from {1}", removeRoles, thisRecordName));
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.Trace(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.Trace(ex.Message);
                }
            }
        }

        /// <summary>
        /// Update Site Team to the specified Business Unit and associate the TMP Site Team role from that new Business Unit
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="buId"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        internal static void UpdateSiteTeam(Guid teamId, Guid buId, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.setMethod = "CvtHelper.UpdateSiteTeam";
            try
            {
                var req = new SetParentTeamRequest()
                {
                    TeamId = teamId,
                    BusinessId = buId
                };

                OrganizationService.Execute(req);
                Logger.WriteDebugMessage("Site team record updated to new BU");

                //Need to reassign the TMP Site Team security role
                //Logger.WriteDebugMessage("About to query for appropriate security role.");
                var siteTeamrole = new Role();
                using (var srv = new Xrm(OrganizationService))
                {
                    siteTeamrole = srv.RoleSet.FirstOrDefault(r => r.Name == "TMP Site Team" && r.BusinessUnitId.Id == buId);
                }
                if (siteTeamrole != null)
                {
                    // Associate the user with the role.
                    OrganizationService.Associate(Team.EntityLogicalName, teamId, new Relationship("teamroles_association"),
                        new EntityReferenceCollection() { new EntityReference(Role.EntityLogicalName, siteTeamrole.Id) });
                    Logger.WriteDebugMessage("Associated TMP Site Team role to team.");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToFile("Exception with UpdateSiteTeam for " + teamId + ". Message: " + ex.Message);
            }
        }

        /// <summary>
        /// Update the Primary Team field to match the VISN on the user record.
        /// </summary>
        /// <param name="thisId"></param>
        internal static void UpdatePrimaryTeam(Guid thisId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.setMethod = "UpdatePrimaryTeam";
            Logger.WriteDebugMessage("Starting UpdatePrimaryTeam");
            using (var srv = new Xrm(OrganizationService))
            {
                var thisUser = srv.SystemUserSet.FirstOrDefault(su => su.Id == thisId);
                Logger.WriteDebugMessage("Retrieved User Set.");
                if (thisUser.cvt_PrimaryTeam == null || (thisUser.cvt_PrimaryTeam.Name != thisUser.BusinessUnitId.Name))
                {
                    Logger.WriteDebugMessage("Mismatched Primary Team and VISN name, update team.");
                    //Need to update the team to match the VISN
                    var VISNTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisUser.BusinessUnitId.Name);
                    if (VISNTeam != null)
                    {
                        Logger.WriteDebugMessage("Found Team of user's current VISN. About to Update.");
                        SystemUser userUpdate = new SystemUser()
                        {
                            Id = thisUser.Id,
                            cvt_PrimaryTeam = new EntityReference() { LogicalName = Team.EntityLogicalName, Id = VISNTeam.Id, Name = VISNTeam.Name }
                        };
                        OrganizationService.Update(userUpdate);
                        Logger.WriteDebugMessage("Update successful.");
                    }
                }
            }
        }

        #endregion

        #region SA Permissions
        //Moved here to resolve Deadlock issues
        /// <summary>
        /// This method initiates the Assignment and Sharing processes on Service Appointment Creation
        /// </summary>
        public static void SetServiceAppointmentPermissions(IOrganizationService OrganizationService, Entity PrimaryEntity, MCSLogger Logger)
        {
            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == PrimaryEntity.Id);
                    if (sa != null)
                    {
                        //Initiate the ShareServiceAppointment to share the Service appointment record with the associated Provider Site's Scheduler Team & createdby user
                        ShareServiceAppointment(sa, srv, Logger, OrganizationService, PrimaryEntity);
                        // This Assign owner method assigns the Service Appointment to the associated Patient Site's Scheduler Team
                        // This provides Patient Site's Scheduler Team the ability to view, modify, and close out service activities so that only the creator does not have to perform these activities each time. 
                        AssignOwner(sa, Logger, OrganizationService);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteDebugMessage(ex.Message);
                Logger.WriteDebugMessage(ex.StackTrace);
            }
        }

        /// <summary>
        /// This method initiates the Assignment and Sharing processes on Service Appointment Creation
        /// </summary>
        public static void SetServiceAppointmentPermissions(IOrganizationService OrganizationService, Entity PrimaryEntity, PluginLogger Logger)
        {
            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == PrimaryEntity.Id);
                    if (sa != null)
                    {
                        //Initiate the ShareServiceAppointment to share the Service appointment record with the associated Provider Site's Scheduler Team & createdby user
                        ShareServiceAppointment(sa, srv, Logger, OrganizationService, PrimaryEntity);
                        // This Assign owner method assigns the Service Appointment to the associated Patient Site's Scheduler Team
                        // This provides Patient Site's Scheduler Team the ability to view, modify, and close out service activities so that only the creator does not have to perform these activities each time. 
                        AssignOwner(sa, Logger, OrganizationService);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Trace(ex.Message);
                Logger.Trace(ex.StackTrace);
            }
        }

        internal static void ShareRecord(Entity record, MCSLogger logger, IOrganizationService organizationService, EntityReference principalReference, AccessRights accessRights)
        {
            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = accessRights,
                    Principal = principalReference
                },
                Target = new EntityReference(record.LogicalName, record.Id)
            };
            try
            {
                organizationService.Execute(grantAccessRequest);
                logger.WriteDebugMessage($"{record.LogicalName} shared to a {principalReference.LogicalName}. Ending ShareRecord.");
            }
            catch (Exception ex)
            {
                logger.WriteDebugMessage($"Failed to share {record.Id} to a {principalReference.LogicalName} with name {principalReference.Name} and Id:{principalReference.Id}.  Error: {CvtHelper.BuildExceptionMessage(ex)}");
                throw;
            }
        }

        internal static void ShareRecord(Entity record, PluginLogger logger, IOrganizationService organizationService, EntityReference principalReference, AccessRights accessRights)
        {
            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = accessRights,
                    Principal = principalReference
                },
                Target = new EntityReference(record.LogicalName, record.Id)
            };
            try
            {
                organizationService.Execute(grantAccessRequest);
                logger.Trace($"{record.LogicalName} shared to a {principalReference.LogicalName}. Ending ShareRecord.");
            }
            catch (Exception ex)
            {
                logger.Trace($"Failed to share {record.Id} to a {principalReference.LogicalName} with name {principalReference.Name} and Id:{principalReference.Id}.  Error: {CvtHelper.BuildExceptionMessage(ex)}");
                throw;
            }
        }

        /// <summary>
        /// This method shares the Service Appointment with the associated Provider Site's Scheduler Team
        /// This provides Provider Site's Scheduler Team the ability to view, modify, and close out service activities so that the creator/Patient Site Scheduler Team does not have perform these activities always. 
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="srv"></param>
        internal static void ShareServiceAppointment(ServiceAppointment sa, Xrm srv, MCSLogger Logger, IOrganizationService OrganizationService, Entity PrimaryEntity)
        {
            Logger.setMethod = "ShareServiceAppointment";
            Logger.WriteDebugMessage("starting ShareServiceAppointment");

            // If the Service Appointment Type is VA Video Connect provider site scheduler team would be the owner otherwise share the service appointment record with provider team 
            if (sa != null && sa.cvt_Type != null && !sa.cvt_Type.Value && sa.mcs_relatedprovidersite != null)
            {
                if (sa.mcs_relatedsite == null || sa.mcs_relatedprovidersite.Id != sa.mcs_relatedsite.Id) //Group || InterFacility
                {
                    var findTeam = (from t in srv.TeamSet
                                    join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                    join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                    where s.mcs_siteId.Value == sa.mcs_relatedprovidersite.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                    select t).FirstOrDefault();

                    if (findTeam == null || findTeam.Id == Guid.Empty)
                    {
                        Logger.WriteDebugMessage("No Team associated to provider site found. Hence not sharing the Service Activity to the provider site scheduling Team.");
                    }
                    else if (sa.OwnerId.Id == findTeam.Id)
                    {
                        Logger.WriteDebugMessage("Sharing action was not performed because the owner team is already the provider facility scheduling team.");
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Sharing the Service Activity to the provider site service Team.");

                        // Grant the team read/write access
                        var teamReference = new EntityReference(Team.EntityLogicalName, findTeam.Id);
                        var grantAccessRequest = new GrantAccessRequest
                        {
                            PrincipalAccess = new PrincipalAccess
                            {
                                AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess,
                                Principal = teamReference
                            },
                            Target = new EntityReference(sa.LogicalName, sa.Id)
                        };
                        try
                        {
                            OrganizationService.Execute(grantAccessRequest);
                            Logger.WriteDebugMessage(sa.LogicalName + " shared to a Team. Ending ShareServiceAppointment.");
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile(string.Format("Failed to share {0} to a Team.  Error: {1}", sa.Subject, CvtHelper.BuildExceptionMessage(ex)));
                            throw;
                        }
                    }
                }
                else
                    Logger.WriteDebugMessage("SA should be a Group or intrafacility, so no need to share.");
            }
            else
            {
                Logger.WriteDebugMessage("Provider site on the Service Activity is null or empty. Hence not shared with the provider site scheduling Team.");
            }

            //Shares record to creator so that they don't lose access once it is reassigned to the team.  
            if (sa.Contains("createdby"))
            {

                var shareToSelf = new GrantAccessRequest
                {
                    PrincipalAccess = new PrincipalAccess
                    {
                        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.ShareAccess,
                        Principal = sa.Contains("createdby") ? (EntityReference)sa["createdby"] : null
                    },
                    Target = new EntityReference(sa.LogicalName, sa.Id)
                };
                try
                {
                    OrganizationService.Execute(shareToSelf);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile("Unable to share to self, the will no longer have access to this record if they are on pro side of IFC and not on scheduler team.  Error: " + ex.Message);
                }
            }
            else
                Logger.WriteToFile("SA Create is missing createdby, unable to share to self");

            Logger.WriteDebugMessage("finished ShareServiceAppointment");
        }

        /// <summary>
        /// This method shares the Service Appointment with the associated Provider Site's Scheduler Team
        /// This provides Provider Site's Scheduler Team the ability to view, modify, and close out service activities so that the creator/Patient Site Scheduler Team does not have perform these activities always. 
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="srv"></param>
        internal static void ShareServiceAppointment(ServiceAppointment sa, Xrm srv, PluginLogger Logger, IOrganizationService OrganizationService, Entity PrimaryEntity)
        {
            //Logger.setMethod = "ShareServiceAppointment";
            Logger.Trace("starting ShareServiceAppointment");

            // If the Service Appointment Type is VA Video Connect provider site scheduler team would be the owner otherwise share the service appointment record with provider team 
            if (sa != null && sa.cvt_Type != null && !sa.cvt_Type.Value && sa.mcs_relatedprovidersite != null)
            {
                if (sa.mcs_relatedsite == null || sa.mcs_relatedprovidersite.Id != sa.mcs_relatedsite.Id) //Group || InterFacility
                {
                    var findTeam = (from t in srv.TeamSet
                                    join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                    join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                    where s.mcs_siteId.Value == sa.mcs_relatedprovidersite.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                    select t).FirstOrDefault();

                    if (findTeam == null || findTeam.Id == Guid.Empty)
                    {
                        Logger.Trace("No Team associated to provider site found. Hence not sharing the Service Activity to the provider site scheduling Team.");
                    }
                    else if (sa.OwnerId.Id == findTeam.Id)
                    {
                        Logger.Trace("Sharing action was not performed because the owner team is already the provider facility scheduling team.");
                    }
                    else
                    {
                        Logger.Trace("Sharing the Service Activity to the provider site service Team.");

                        // Grant the team read/write access
                        var teamReference = new EntityReference(Team.EntityLogicalName, findTeam.Id);
                        var grantAccessRequest = new GrantAccessRequest
                        {
                            PrincipalAccess = new PrincipalAccess
                            {
                                AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess,
                                Principal = teamReference
                            },
                            Target = new EntityReference(sa.LogicalName, sa.Id)
                        };
                        try
                        {
                            OrganizationService.Execute(grantAccessRequest);
                            Logger.Trace(sa.LogicalName + " shared to a Team. Ending ShareServiceAppointment.");
                        }
                        catch (Exception ex)
                        {
                            Logger.Trace(string.Format("Failed to share {0} to a Team.  Error: {1}", sa.Subject, CvtHelper.BuildExceptionMessage(ex)));
                            throw;
                        }
                    }
                }
                else
                    Logger.Trace("SA should be a Group or intrafacility, so no need to share.");
            }
            else
            {
                Logger.Trace("Provider site on the Service Activity is null or empty. Hence not shared with the provider site scheduling Team.");
            }

            //Shares record to creator so that they don't lose access once it is reassigned to the team.  
            if (sa.Contains("createdby"))
            {

                var shareToSelf = new GrantAccessRequest
                {
                    PrincipalAccess = new PrincipalAccess
                    {
                        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.ShareAccess,
                        Principal = sa.Contains("createdby") ? (EntityReference)sa["createdby"] : null
                    },
                    Target = new EntityReference(sa.LogicalName, sa.Id)
                };
                try
                {
                    OrganizationService.Execute(shareToSelf);
                }
                catch (Exception ex)
                {
                    Logger.Trace("Unable to share to self, the will no longer have access to this record if they are on pro side of IFC and not on scheduler team.  Error: " + ex.Message);
                }
            }
            else
                Logger.Trace("SA Create is missing createdby, unable to share to self");

            Logger.Trace("finished ShareServiceAppointment");
        }

        internal static void ShareReserveResource(Guid reserveResourceId, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("starting ShareReserveResource");
            using (var srv = new Xrm(OrganizationService))
            {
                var reserveResource = srv.AppointmentSet.FirstOrDefault(a => a.Id == reserveResourceId);
                // If the RR has a related Appt, then need to share it with the other side, pat or pro.
                if (reserveResource != null && reserveResource.cvt_serviceactivityid != null && reserveResource.cvt_serviceactivityid.Id != Guid.Empty)
                {
                    //get the SA for the SP
                    var sa = srv.ServiceAppointmentSet.FirstOrDefault(appt => appt.Id == reserveResource.cvt_serviceactivityid.Id);
                    if (sa != null && sa.mcs_groupappointment != null)
                    {
                        var proSite = sa.mcs_relatedprovidersite;
                        //Share RR with pro scheduler team
                        if (proSite.Id == reserveResource.cvt_Site.Id)
                        {
                            Logger.WriteDebugMessage("No need to share, this Patient Reserve Resource is Intrafacility.");
                            return;
                        }
                        else
                        {
                            var findTeam = (from t in srv.TeamSet
                                            join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                            join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                            where s.mcs_siteId.Value == sa.mcs_relatedprovidersite.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                            select t).FirstOrDefault();

                            if (findTeam == null || findTeam.Id == Guid.Empty)
                            {
                                Logger.WriteDebugMessage("No Team associated to provider site found. Hence not sharing the RR to the provider site scheduling team.");
                            }
                            else if (reserveResource.OwnerId.Id == findTeam.Id)
                            {
                                Logger.WriteDebugMessage("Sharing action was not performed because the owner team of the RR is already the provider site scheduling team.");
                            }
                            else
                            {
                                Logger.WriteDebugMessage("Sharing the RR to the provider site scheduling team.");

                                // Grant the team read/write access
                                var teamReference = new EntityReference(Team.EntityLogicalName, findTeam.Id);
                                var grantAccessRequest = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess,
                                        Principal = teamReference
                                    },
                                    Target = new EntityReference(reserveResource.LogicalName, reserveResource.Id)
                                };
                                try
                                {
                                    OrganizationService.Execute(grantAccessRequest);
                                    Logger.WriteDebugMessage(reserveResource.LogicalName + " shared to a Team..");
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteToFile(string.Format("Failed to share {0} to a Team.  Error: {1}", reserveResource.Subject, CvtHelper.BuildExceptionMessage(ex)));
                                    throw;
                                }
                            }
                            //Shares record of the RR to creator of the SA so that they don't lose access once it is reassigned to the team.  
                            if (sa.Contains("createdby"))
                            {

                                var shareToSelf = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.ShareAccess,
                                        Principal = sa.Contains("createdby") ? (EntityReference)sa["createdby"] : null
                                    },
                                    Target = new EntityReference(reserveResource.LogicalName, reserveResource.Id)
                                };
                                try
                                {
                                    OrganizationService.Execute(shareToSelf);
                                }
                                catch (Exception ex)
                                {
                                    Logger.WriteToFile("Unable to share to self, the will no longer have access to this record if they are on pro side of IFC and not on scheduler team.  Error: " + ex.Message);
                                }
                            }
                            else
                                Logger.WriteToFile("Appt is missing createdby, unable to share to self.");
                        }
                    }
                }
            }
            Logger.WriteDebugMessage("Finished");
        }

        internal static void ShareReserveResource(Guid reserveResourceId, PluginLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.Trace("starting ShareReserveResource");
            using (var srv = new Xrm(OrganizationService))
            {
                var reserveResource = srv.AppointmentSet.FirstOrDefault(a => a.Id == reserveResourceId);
                // If the RR has a related Appt, then need to share it with the other side, pat or pro.
                if (reserveResource != null && reserveResource.cvt_serviceactivityid != null && reserveResource.cvt_serviceactivityid.Id != Guid.Empty)
                {
                    //get the SA for the SP
                    var sa = srv.ServiceAppointmentSet.FirstOrDefault(appt => appt.Id == reserveResource.cvt_serviceactivityid.Id);
                    if (sa != null && sa.mcs_groupappointment != null)
                    {
                        var proSite = sa.mcs_relatedprovidersite;
                        //Share RR with pro scheduler team
                        if (proSite.Id == reserveResource.cvt_Site.Id)
                        {
                            Logger.Trace("No need to share, this Patient Reserve Resource is Intrafacility.");
                            return;
                        }
                        else
                        {
                            var findTeam = (from t in srv.TeamSet
                                            join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                            join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                            where s.mcs_siteId.Value == sa.mcs_relatedprovidersite.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                            select t).FirstOrDefault();

                            if (findTeam == null || findTeam.Id == Guid.Empty)
                            {
                                Logger.Trace("No Team associated to provider site found. Hence not sharing the RR to the provider site scheduling team.");
                            }
                            else if (reserveResource.OwnerId.Id == findTeam.Id)
                            {
                                Logger.Trace("Sharing action was not performed because the owner team of the RR is already the provider site scheduling team.");
                            }
                            else
                            {
                                Logger.Trace("Sharing the RR to the provider site scheduling team.");

                                // Grant the team read/write access
                                var teamReference = new EntityReference(Team.EntityLogicalName, findTeam.Id);
                                var grantAccessRequest = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess,
                                        Principal = teamReference
                                    },
                                    Target = new EntityReference(reserveResource.LogicalName, reserveResource.Id)
                                };
                                try
                                {
                                    OrganizationService.Execute(grantAccessRequest);
                                    Logger.Trace(reserveResource.LogicalName + " shared to a Team..");
                                }
                                catch (Exception ex)
                                {
                                    Logger.Trace(string.Format("Failed to share {0} to a Team.  Error: {1}", reserveResource.Subject, CvtHelper.BuildExceptionMessage(ex)));
                                    throw;
                                }
                            }
                            //Shares record of the RR to creator of the SA so that they don't lose access once it is reassigned to the team.  
                            if (sa.Contains("createdby"))
                            {

                                var shareToSelf = new GrantAccessRequest
                                {
                                    PrincipalAccess = new PrincipalAccess
                                    {
                                        AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.ShareAccess,
                                        Principal = sa.Contains("createdby") ? (EntityReference)sa["createdby"] : null
                                    },
                                    Target = new EntityReference(reserveResource.LogicalName, reserveResource.Id)
                                };
                                try
                                {
                                    OrganizationService.Execute(shareToSelf);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Trace("Unable to share to self, the will no longer have access to this record if they are on pro side of IFC and not on scheduler team.  Error: " + ex.Message);
                                }
                            }
                            else
                                Logger.Trace("Appt is missing createdby, unable to share to self.");
                        }
                    }
                }
            }
            Logger.Trace("Finished");
        }
        #endregion

        #region AssignOwner
        /// <summary>
        /// Assigns Ownership of the record to the correct team. 
        /// </summary>
        /// <param name="thisRecord"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <returns>Assigned - true/false</returns>
        internal static bool AssignOwner(Entity thisRecord, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            #region Setup
            Logger.setMethod = "AssignOwner";
            Logger.WriteDebugMessage("Starting AssignOwner");

            EntityReference assignOwner = new EntityReference()
            {
                LogicalName = Team.EntityLogicalName,
            };
            Entity findTeam = new Entity();
            #endregion

            try
            {
                switch (thisRecord.LogicalName)
                {
                    #region Deprecated M/TSA
                    case cvt_providerresourcegroup.EntityLogicalName:
                    case cvt_patientresourcegroup.EntityLogicalName:
                    case mcs_services.EntityLogicalName:
                    case cvt_mastertsa.EntityLogicalName:
                        Logger.WriteDebugMessage("attempted operation on deprecated (M/TSA) entity in AssignOwner method.");
                        return false;
                    #endregion
                    #region TSS Site
                    case mcs_site.EntityLogicalName:
                        var thisSite = (mcs_site)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisSite.mcs_name);
                        break;
                    #endregion
                    #region Resource
                    case mcs_resource.EntityLogicalName:
                        Logger.WriteDebugMessage(thisRecord.LogicalName + ". In resource switch case");
                        var thisResource = (mcs_resource)thisRecord;
                        Logger.WriteDebugMessage("Cast Complete");

                        //Check that the TSS Resource's owner is the same name as the site
                        if (thisResource.mcs_RelatedSiteId == null || thisResource.OwnerId.Name == thisResource.mcs_RelatedSiteId.Name)
                            return false;

                        //Get the correct team
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisResource.mcs_RelatedSiteId.Name);
                        Logger.WriteDebugMessage("Case Complete");
                        break;
                    #endregion
                    #region Component
                    case cvt_component.EntityLogicalName:

                        var thisComponent = (cvt_component)thisRecord;
                        if (thisComponent.cvt_relatedresourceid == null)
                            return false;

                        mcs_resource parentResource = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName, thisComponent.cvt_relatedresourceid.Id, new ColumnSet(true));
                        if (parentResource.mcs_RelatedSiteId == null)
                            return false;

                        //Check that the TSS Resource's owner is the same name as the site, if so, pass along or query for correct one.
                        if (parentResource.OwnerId.Name == parentResource.mcs_RelatedSiteId.Name)
                            findTeam.Id = parentResource.OwnerId.Id;
                        else
                        {
                            //Get the correct team
                            using (var srv = new Xrm(OrganizationService))
                                findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == parentResource.mcs_RelatedSiteId.Name);
                            if (findTeam == null)
                                return false;

                            //Also Reassign TSS Resource
                            AssignRequest assignTSSResource = new AssignRequest()
                            {
                                Assignee = findTeam.ToEntityReference(),
                                Target = new EntityReference(mcs_resource.EntityLogicalName, parentResource.Id)
                            };

                            OrganizationService.Execute(assignTSSResource);
                            Logger.WriteDebugMessage("TSS Resource reassigned to Site Team.");
                        }
                        //No need to reassign, corrrect owner
                        if (thisComponent.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                    #endregion
                    #region Resource Group
                    case mcs_resourcegroup.EntityLogicalName:

                        var thisResourceGroup = (mcs_resourcegroup)thisRecord;
                        if (thisResourceGroup.mcs_relatedSiteId == null || thisResourceGroup.OwnerId.Name == thisResourceGroup.mcs_relatedSiteId.Name)
                            return false;

                        //Get the correct team
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisResourceGroup.mcs_relatedSiteId.Name);
                        break;
                    #endregion
                    #region Group Resource
                    case mcs_groupresource.EntityLogicalName:

                        var thisGroupResource = (mcs_groupresource)thisRecord;
                        if (thisGroupResource.mcs_relatedSiteId == null || thisGroupResource.OwnerId.Name == thisGroupResource.mcs_relatedSiteId.Name)
                            return false;

                        //Get the correct team
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisGroupResource.mcs_relatedSiteId.Name);
                        break;
                    #endregion
                    #region Staging Resources
                    case cvt_stagingresource.EntityLogicalName:
                        if (!AssignOwnerStagingResource(thisRecord, Logger, OrganizationService, ref findTeam)) return false;
                        break;
                    #endregion
                    #region Telehealth Privileging
                    case cvt_tssprivileging.EntityLogicalName:

                        var thisPrivileging = (cvt_tssprivileging)thisRecord;

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == thisPrivileging.cvt_PrivilegedAtId.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.CredentialingandPrivileging);

                        //Can't find team or already corrrect owner
                        if (findTeam == null)
                        {
                            Logger.WriteToFile("C&P Team Not Found for Facility: " + thisPrivileging.cvt_PrivilegedAtId.Name);
                            return false;
                        }
                        else
                        {
                            if (thisPrivileging.OwnerId.Id == findTeam.Id)
                                return false;
                        }

                        break;
                    #endregion
                    #region FPPE/OPPE
                    case cvt_qualitycheck.EntityLogicalName:

                        var thisPPE = (cvt_qualitycheck)thisRecord;
                        Logger.WriteDebugMessage("Querying for the parent priv.");
                        var privilegeParent = (cvt_tssprivileging)OrganizationService.Retrieve(cvt_tssprivileging.EntityLogicalName, thisPPE.cvt_TSSPrivilegingId.Id, new ColumnSet(true));
                        Logger.WriteDebugMessage("Querying for the Service Chief Team for the facility.");

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == privilegeParent.cvt_PrivilegedAtId.Id
                                && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief
                                && t.cvt_ServiceType != null && t.cvt_ServiceType.Id == privilegeParent.cvt_ServiceTypeId.Id);

                        //Can't find team or already correct owner
                        if (findTeam == null)
                        {
                            Logger.WriteDebugMessage("Can't find Service Chief team for Facility: " + privilegeParent.cvt_PrivilegedAtId.Name + " with Specialty: " + privilegeParent.cvt_ServiceTypeId.Name + ". Creating team.");
                            Team newTeam = new Team()
                            {
                                cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, privilegeParent.cvt_PrivilegedAtId.Id),
                                cvt_Type = new OptionSetValue((int)Teamcvt_Type.ServiceChief),
                                Name = privilegeParent.cvt_ServiceTypeId.Name + " Service Chief Approval Group @ " + privilegeParent.cvt_PrivilegedAtId.Name,
                                TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                cvt_ServiceType = privilegeParent.cvt_ServiceTypeId,
                                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, privilegeParent.OwningBusinessUnit.Id)
                            };
                            findTeam = new Team
                            {
                                Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                            };
                        }
                        else
                        {
                            if (thisPPE.OwnerId.Id == findTeam.Id)
                                return false;
                        }
                        break;

                    #endregion
                    #region Appointment
                    case ServiceAppointment.EntityLogicalName:
                        var thisServiceAppointment = (ServiceAppointment)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisServiceAppointment.cvt_Type.HasValue)
                            {
                                var site = (thisServiceAppointment.cvt_Type.Value || thisServiceAppointment.mcs_groupappointment.Value) ? thisServiceAppointment.mcs_relatedprovidersite : thisServiceAppointment.mcs_relatedsite;

                                if (site != null && site.Id != Guid.Empty)
                                {
                                    findTeam = (from t in srv.TeamSet
                                                join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                                join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                                where s.mcs_siteId.Value == site.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                                select t).FirstOrDefault();

                                    if (findTeam == null || findTeam.Id == Guid.Empty)
                                        Logger.WriteDebugMessage(string.Format("Scheduler Team associated to the site on the Service Activity {0} with Id {1} is null or empty. Hence the Service appointment is not assigned to the Patient Facility Scheduler Team.", site.Name, site.Id));
                                    else
                                        Logger.WriteDebugMessage("Initiating the assignment to the Patient Facility Scheduler Team: " + findTeam.Id + ((Team)findTeam).Name);
                                }
                                else
                                {
                                    Logger.WriteDebugMessage(string.Format("Associated site on the Service Activity is null or empty. Service Activity Type: {0}", (thisServiceAppointment.cvt_Type.Value) ? "VA Video Connect" : "Clinic Based"));
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Reserve Resource
                    case Appointment.EntityLogicalName:
                        var thisAppointment = (Appointment)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisAppointment.cvt_serviceactivityid == null)
                                return false;
                            if (thisAppointment.cvt_Site == null)
                                return false;

                            var site = thisAppointment.cvt_Site;

                            if (site != null && site.Id != Guid.Empty)
                            {
                                findTeam = (from t in srv.TeamSet
                                            join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                            join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                            where s.mcs_siteId.Value == site.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                            select t).FirstOrDefault();

                                if (findTeam == null || findTeam.Id == Guid.Empty)
                                    Logger.WriteDebugMessage(string.Format("Scheduler Team associated to the site on the Appointment {0} with Id {1} is null or empty. Hence the Appointment is not assigned to the Patient Facility Scheduler Team.", site.Name, site.Id));
                                else
                                    Logger.WriteDebugMessage("Initiating the assignment to the Patient Facility Scheduler Team: " + findTeam.Id + ((Team)findTeam).Name);
                            }
                        }
                        break;
                    #endregion
                    #region PPEReview
                    case cvt_ppereview.EntityLogicalName:
                        var thisReview = (cvt_ppereview)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisReview.cvt_telehealthprivileging == null)
                                return false;

                            var thisPriv = srv.cvt_tssprivilegingSet.FirstOrDefault(t => t.Id == thisReview.cvt_telehealthprivileging.Id);

                            if (thisPriv == null)
                                return false;

                            var Facility = thisPriv.cvt_PrivilegedAtId;

                            if (Facility != null && Facility.Id != Guid.Empty)
                            {
                                findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == thisPriv.cvt_PrivilegedAtId.Id
                                    && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief
                                    && t.cvt_ServiceType != null && t.cvt_ServiceType.Id == thisPriv.cvt_ServiceTypeId.Id);

                                if (findTeam == null || findTeam.Id == Guid.Empty)
                                {
                                    Logger.WriteDebugMessage("Can't find Service Chief team for Facility: " + thisPriv.cvt_PrivilegedAtId.Name + " with Specialty: " + thisPriv.cvt_ServiceTypeId.Name + ". Creating team.");
                                    Team newTeam = new Team()
                                    {
                                        cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, thisPriv.cvt_PrivilegedAtId.Id),
                                        cvt_Type = new OptionSetValue((int)Teamcvt_Type.ServiceChief),
                                        Name = thisPriv.cvt_ServiceTypeId.Name + " Service Chief Approval Group @ " + thisPriv.cvt_PrivilegedAtId.Name,
                                        TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                        cvt_ServiceType = thisPriv.cvt_ServiceTypeId,
                                        BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisPriv.OwningBusinessUnit.Id)
                                    };
                                    findTeam = new Team
                                    {
                                        Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                                    };
                                }
                                else
                                {
                                    if (thisReview.OwnerId.Id == findTeam.Id)
                                        return false;
                                }
                                Logger.WriteDebugMessage("Initiating the assignment to the Home Facility SC Team: " + findTeam.Id + ((Team)findTeam).Name);
                            }
                        }
                        break;
                    #endregion
                    #region Resource Package - Assign to VHA BU
                    case cvt_resourcepackage.EntityLogicalName:

                        var thisRP = (cvt_resourcepackage)thisRecord;

                        //Check the OwnerId
                        if (thisRP.OwnerId.Name.Contains("VHA"))
                            return false;

                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisRP.cvt_hub != null)
                            {
                                findTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.HubTSAManager && T.cvt_Facility.Id == thisRP.cvt_hub.Id);
                            }
                            else
                            {
                                var findRootBU = srv.BusinessUnitSet.FirstOrDefault(bu => bu.ParentBusinessUnitId == null);
                                if (findRootBU == null)
                                {
                                    Logger.WriteToFile("Failed to find root BU. Not setting owner");
                                }

                                findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == findRootBU.Name);
                            }

                            if (findTeam == null)
                                Logger.WriteToFile("Failed to find Team. Not setting owner for SP");
                            else
                                Logger.WriteToFile("Found root BU Team. Name: " + findTeam.ToEntity<Team>().Name);
                        }

                        if (thisRP.OwnerId != null && thisRP.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                    #endregion
                    #region Participating Site - Facility FTC Team
                    case cvt_participatingsite.EntityLogicalName:

                        var thisPS = (cvt_participatingsite)thisRecord;

                        //Check the Provider Facility vs OwnerId
                        if ((thisPS.OwnerId.Name.Contains("FTC") && (thisPS.OwnerId.Name.Contains(thisPS.cvt_facility.Name))))
                            return false;

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility == thisPS.cvt_facility && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);

                        if (findTeam == null)
                        {
                            string name = (thisPS.cvt_facility != null) ? thisPS.cvt_facility.Name : "No Facility Listed";
                            Logger.WriteDebugMessage("FTC Team not found for Facility: " + name + " Creating team.");
                            Team newTeam = new Team()
                            {
                                cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, thisPS.cvt_facility.Id),
                                cvt_Type = new OptionSetValue((int)Teamcvt_Type.FTC),
                                TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisPS.OwningBusinessUnit.Id),
                                Name = "FTC Approval Group @ " + thisPS.cvt_facility.Name
                            };
                            findTeam = new Team
                            {
                                Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                            };
                        }

                        if (thisPS.OwnerId != null && thisPS.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                    #endregion
                    #region Scheduling Resource - TCT Team
                    case cvt_schedulingresource.EntityLogicalName:
                        using (var srv = new Xrm(OrganizationService))
                        {
                            var thisSR = (cvt_schedulingresource)thisRecord;
                            var relatedPS = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == thisSR.cvt_participatingsite.Id);
                            //Check the Provider Facility vs OwnerId
                            if ((thisSR.OwnerId.Name.Contains("TCT") && (thisSR.OwnerId.Name.Contains(relatedPS.cvt_site.Name))))
                                return false;


                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite == relatedPS.cvt_site && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.Staff);

                            if (findTeam == null)
                            {
                                string name = (relatedPS.cvt_site != null) ? relatedPS.cvt_site.Name : "No TMP Site Listed";
                                Logger.WriteDebugMessage("TCT Team not found for TMP Site: " + name + ". Creating team.");
                                Team newTeam = new Team()
                                {
                                    cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, relatedPS.cvt_site.Id),
                                    cvt_Type = new OptionSetValue((int)Teamcvt_Type.Staff),
                                    TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                    BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisSR.OwningBusinessUnit.Id),
                                    Name = "Staff @ " + relatedPS.cvt_site.Name
                                };
                                findTeam = new Team
                                {
                                    Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                                };
                            }

                            if (thisSR.OwnerId != null && thisSR.OwnerId.Id == findTeam.Id)
                                return false;
                            break;
                        }
                    #endregion
                    #region Facility Approval - Provider Facility FTC Team
                    case cvt_facilityapproval.EntityLogicalName:

                        var thisFA = (cvt_facilityapproval)thisRecord;

                        //Check the Provider Facility vs OwnerId
                        if ((thisFA.OwnerId.Name.Contains("FTC") && (thisFA.OwnerId.Name.Contains(thisFA.cvt_providerfacility.Name))))
                            return false;

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility == thisFA.cvt_providerfacility && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);

                        if (findTeam == null)
                        {
                            string name = (thisFA.cvt_providerfacility != null) ? thisFA.cvt_providerfacility.Name : "No Facility Listed";
                            Logger.WriteDebugMessage("FTC Team not found for Facility: " + name + " Creating team.");
                            Team newTeam = new Team()
                            {
                                cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, thisFA.cvt_providerfacility.Id),
                                cvt_Type = new OptionSetValue((int)Teamcvt_Type.FTC),
                                TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisFA.OwningBusinessUnit.Id),
                                Name = "FTC Approval Group @ " + thisFA.cvt_providerfacility.Name
                            };
                            findTeam = new Team
                            {
                                Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                            };
                        }

                        if (thisFA.OwnerId != null && thisFA.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                        #endregion

                }
                #region Common Code for the Assign
                if (findTeam == null || findTeam.Id == Guid.Empty)
                    return false;
                Logger.WriteDebugMessage("Team found. Assignment started");
                assignOwner.Id = findTeam.Id;

                AssignRequest assignRequest = new AssignRequest()
                {
                    Assignee = assignOwner,
                    Target = new EntityReference(thisRecord.LogicalName, thisRecord.Id)
                };

                OrganizationService.Execute(assignRequest);
                Logger.WriteDebugMessage(thisRecord.LogicalName + " assigned to a Team. Ending AssignOwner.");
                return true;
                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteToFile($"Error occured while assigning the {thisRecord.LogicalName} entity record with Id {thisRecord.Id} to the team {findTeam?.Id} \nError: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Assigns Ownership of the record to the correct team. 
        /// </summary>
        /// <param name="thisRecord"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <returns>Assigned - true/false</returns>
        internal static bool AssignOwner(Entity thisRecord, PluginLogger Logger, IOrganizationService OrganizationService)
        {
            #region Setup
            //Logger.setMethod = "AssignOwner";
            Logger.Trace("Starting AssignOwner");

            EntityReference assignOwner = new EntityReference()
            {
                LogicalName = Team.EntityLogicalName,
            };
            Entity findTeam = new Entity();
            #endregion

            try
            {
                switch (thisRecord.LogicalName)
                {
                    #region Deprecated M/TSA
                    case cvt_providerresourcegroup.EntityLogicalName:
                    case cvt_patientresourcegroup.EntityLogicalName:
                    case mcs_services.EntityLogicalName:
                    case cvt_mastertsa.EntityLogicalName:
                        Logger.Trace("attempted operation on deprecated (M/TSA) entity in AssignOwner method.");
                        return false;
                    #endregion
                    #region TSS Site
                    case mcs_site.EntityLogicalName:
                        var thisSite = (mcs_site)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisSite.mcs_name);
                        break;
                    #endregion
                    #region Resource
                    case mcs_resource.EntityLogicalName:
                        Logger.Trace(thisRecord.LogicalName + ". In resource switch case");
                        var thisResource = (mcs_resource)thisRecord;
                        Logger.Trace("Cast Complete");

                        //Check that the TSS Resource's owner is the same name as the site
                        if (thisResource.mcs_RelatedSiteId == null || thisResource.OwnerId.Name == thisResource.mcs_RelatedSiteId.Name)
                            return false;

                        //Get the correct team
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisResource.mcs_RelatedSiteId.Name);
                        Logger.Trace("Case Complete");
                        break;
                    #endregion
                    #region Component
                    case cvt_component.EntityLogicalName:

                        var thisComponent = (cvt_component)thisRecord;
                        if (thisComponent.cvt_relatedresourceid == null)
                            return false;

                        mcs_resource parentResource = (mcs_resource)OrganizationService.Retrieve(mcs_resource.EntityLogicalName, thisComponent.cvt_relatedresourceid.Id, new ColumnSet(true));
                        if (parentResource.mcs_RelatedSiteId == null)
                            return false;

                        //Check that the TSS Resource's owner is the same name as the site, if so, pass along or query for correct one.
                        if (parentResource.OwnerId.Name == parentResource.mcs_RelatedSiteId.Name)
                            findTeam.Id = parentResource.OwnerId.Id;
                        else
                        {
                            //Get the correct team
                            using (var srv = new Xrm(OrganizationService))
                                findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == parentResource.mcs_RelatedSiteId.Name);
                            if (findTeam == null)
                                return false;

                            //Also Reassign TSS Resource
                            AssignRequest assignTSSResource = new AssignRequest()
                            {
                                Assignee = findTeam.ToEntityReference(),
                                Target = new EntityReference(mcs_resource.EntityLogicalName, parentResource.Id)
                            };

                            OrganizationService.Execute(assignTSSResource);
                            Logger.Trace("TSS Resource reassigned to Site Team.");
                        }
                        //No need to reassign, corrrect owner
                        if (thisComponent.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                    #endregion
                    #region Resource Group
                    case mcs_resourcegroup.EntityLogicalName:

                        var thisResourceGroup = (mcs_resourcegroup)thisRecord;
                        if (thisResourceGroup.mcs_relatedSiteId == null || thisResourceGroup.OwnerId.Name == thisResourceGroup.mcs_relatedSiteId.Name)
                            return false;

                        //Get the correct team
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisResourceGroup.mcs_relatedSiteId.Name);
                        break;
                    #endregion
                    #region Group Resource
                    case mcs_groupresource.EntityLogicalName:

                        var thisGroupResource = (mcs_groupresource)thisRecord;
                        if (thisGroupResource.mcs_relatedSiteId == null || thisGroupResource.OwnerId.Name == thisGroupResource.mcs_relatedSiteId.Name)
                            return false;

                        //Get the correct team
                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == thisGroupResource.mcs_relatedSiteId.Name);
                        break;
                    #endregion
                    #region Staging Resources
                    case cvt_stagingresource.EntityLogicalName:
                        if (!AssignOwnerStagingResource(thisRecord, Logger, OrganizationService, ref findTeam)) return false;
                        break;
                    #endregion
                    #region Telehealth Privileging
                    case cvt_tssprivileging.EntityLogicalName:

                        var thisPrivileging = (cvt_tssprivileging)thisRecord;

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == thisPrivileging.cvt_PrivilegedAtId.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.CredentialingandPrivileging);

                        //Can't find team or already corrrect owner
                        if (findTeam == null)
                        {
                            Logger.Trace("C&P Team Not Found for Facility: " + thisPrivileging.cvt_PrivilegedAtId.Name);
                            return false;
                        }
                        else
                        {
                            if (thisPrivileging.OwnerId.Id == findTeam.Id)
                                return false;
                        }

                        break;
                    #endregion
                    #region FPPE/OPPE
                    case cvt_qualitycheck.EntityLogicalName:

                        var thisPPE = (cvt_qualitycheck)thisRecord;
                        Logger.Trace("Querying for the parent priv.");
                        var privilegeParent = (cvt_tssprivileging)OrganizationService.Retrieve(cvt_tssprivileging.EntityLogicalName, thisPPE.cvt_TSSPrivilegingId.Id, new ColumnSet(true));
                        Logger.Trace("Querying for the Service Chief Team for the facility.");

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == privilegeParent.cvt_PrivilegedAtId.Id
                                && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief
                                && t.cvt_ServiceType != null && t.cvt_ServiceType.Id == privilegeParent.cvt_ServiceTypeId.Id);

                        //Can't find team or already correct owner
                        if (findTeam == null)
                        {
                            Logger.Trace("Can't find Service Chief team for Facility: " + privilegeParent.cvt_PrivilegedAtId.Name + " with Specialty: " + privilegeParent.cvt_ServiceTypeId.Name + ". Creating team.");
                            Team newTeam = new Team()
                            {
                                cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, privilegeParent.cvt_PrivilegedAtId.Id),
                                cvt_Type = new OptionSetValue((int)Teamcvt_Type.ServiceChief),
                                Name = privilegeParent.cvt_ServiceTypeId.Name + " Service Chief Approval Group @ " + privilegeParent.cvt_PrivilegedAtId.Name,
                                TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                cvt_ServiceType = privilegeParent.cvt_ServiceTypeId,
                                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, privilegeParent.OwningBusinessUnit.Id)
                            };
                            findTeam = new Team
                            {
                                Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                            };
                        }
                        else
                        {
                            if (thisPPE.OwnerId.Id == findTeam.Id)
                                return false;
                        }
                        break;

                    #endregion
                    #region Appointment
                    case ServiceAppointment.EntityLogicalName:
                        var thisServiceAppointment = (ServiceAppointment)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisServiceAppointment.cvt_Type.HasValue)
                            {
                                var site = (thisServiceAppointment.cvt_Type.Value || thisServiceAppointment.mcs_groupappointment.Value) ? thisServiceAppointment.mcs_relatedprovidersite : thisServiceAppointment.mcs_relatedsite;

                                if (site != null && site.Id != Guid.Empty)
                                {
                                    findTeam = (from t in srv.TeamSet
                                                join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                                join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                                where s.mcs_siteId.Value == site.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                                select t).FirstOrDefault();

                                    if (findTeam == null || findTeam.Id == Guid.Empty)
                                        Logger.Trace(string.Format("Scheduler Team associated to the site on the Service Activity {0} with Id {1} is null or empty. Hence the Service appointment is not assigned to the Patient Facility Scheduler Team.", site.Name, site.Id));
                                    else
                                        Logger.Trace("Initiating the assignment to the Patient Facility Scheduler Team: " + findTeam.Id + ((Team)findTeam).Name);
                                }
                                else
                                {
                                    Logger.Trace(string.Format("Associated site on the Service Activity is null or empty. Service Activity Type: {0}", (thisServiceAppointment.cvt_Type.Value) ? "VA Video Connect" : "Clinic Based"));
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Reserve Resource
                    case Appointment.EntityLogicalName:
                        var thisAppointment = (Appointment)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisAppointment.cvt_serviceactivityid == null)
                                return false;
                            if (thisAppointment.cvt_Site == null)
                                return false;

                            var site = thisAppointment.cvt_Site;

                            if (site != null && site.Id != Guid.Empty)
                            {
                                findTeam = (from t in srv.TeamSet
                                            join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                                            join s in srv.mcs_siteSet on f.mcs_facilityId.Value equals s.mcs_FacilityId.Id
                                            where s.mcs_siteId.Value == site.Id && t.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode()
                                            select t).FirstOrDefault();

                                if (findTeam == null || findTeam.Id == Guid.Empty)
                                    Logger.Trace(string.Format("Scheduler Team associated to the site on the Appointment {0} with Id {1} is null or empty. Hence the Appointment is not assigned to the Patient Facility Scheduler Team.", site.Name, site.Id));
                                else
                                    Logger.Trace("Initiating the assignment to the Patient Facility Scheduler Team: " + findTeam.Id + ((Team)findTeam).Name);
                            }
                        }
                        break;
                    #endregion
                    #region PPEReview
                    case cvt_ppereview.EntityLogicalName:
                        var thisReview = (cvt_ppereview)thisRecord;
                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisReview.cvt_telehealthprivileging == null)
                                return false;

                            var thisPriv = srv.cvt_tssprivilegingSet.FirstOrDefault(t => t.Id == thisReview.cvt_telehealthprivileging.Id);

                            if (thisPriv == null)
                                return false;

                            var Facility = thisPriv.cvt_PrivilegedAtId;

                            if (Facility != null && Facility.Id != Guid.Empty)
                            {
                                findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility.Id == thisPriv.cvt_PrivilegedAtId.Id
                                    && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief
                                    && t.cvt_ServiceType != null && t.cvt_ServiceType.Id == thisPriv.cvt_ServiceTypeId.Id);

                                if (findTeam == null || findTeam.Id == Guid.Empty)
                                {
                                    Logger.Trace("Can't find Service Chief team for Facility: " + thisPriv.cvt_PrivilegedAtId.Name + " with Specialty: " + thisPriv.cvt_ServiceTypeId.Name + ". Creating team.");
                                    Team newTeam = new Team()
                                    {
                                        cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, thisPriv.cvt_PrivilegedAtId.Id),
                                        cvt_Type = new OptionSetValue((int)Teamcvt_Type.ServiceChief),
                                        Name = thisPriv.cvt_ServiceTypeId.Name + " Service Chief Approval Group @ " + thisPriv.cvt_PrivilegedAtId.Name,
                                        TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                        cvt_ServiceType = thisPriv.cvt_ServiceTypeId,
                                        BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisPriv.OwningBusinessUnit.Id)
                                    };
                                    findTeam = new Team
                                    {
                                        Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                                    };
                                }
                                else
                                {
                                    if (thisReview.OwnerId.Id == findTeam.Id)
                                        return false;
                                }
                                Logger.Trace("Initiating the assignment to the Home Facility SC Team: " + findTeam.Id + ((Team)findTeam).Name);
                            }
                        }
                        break;
                    #endregion
                    #region Resource Package - Assign to VHA BU
                    case cvt_resourcepackage.EntityLogicalName:

                        var thisRP = (cvt_resourcepackage)thisRecord;

                        //Check the OwnerId
                        if (thisRP.OwnerId.Name.Contains("VHA"))
                            return false;

                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (thisRP.cvt_hub != null)
                            {
                                findTeam = srv.TeamSet.FirstOrDefault(T => T.cvt_Type.Value == (int)Teamcvt_Type.HubTSAManager && T.cvt_Facility.Id == thisRP.cvt_hub.Id);
                            }
                            else
                            {
                                var findRootBU = srv.BusinessUnitSet.FirstOrDefault(bu => bu.ParentBusinessUnitId == null);
                                if (findRootBU == null)
                                {
                                    Logger.Trace("Failed to find root BU. Not setting owner");
                                }

                                findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == findRootBU.Name);
                            }

                            if (findTeam == null)
                                Logger.Trace("Failed to find Team. Not setting owner for SP");
                            else
                                Logger.Trace("Found root BU Team. Name: " + findTeam.ToEntity<Team>().Name);
                        }

                        if (thisRP.OwnerId != null && thisRP.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                    #endregion
                    #region Participating Site - Facility FTC Team
                    case cvt_participatingsite.EntityLogicalName:

                        var thisPS = (cvt_participatingsite)thisRecord;

                        //Check the Provider Facility vs OwnerId
                        if ((thisPS.OwnerId.Name.Contains("FTC") && (thisPS.OwnerId.Name.Contains(thisPS.cvt_facility.Name))))
                            return false;

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility == thisPS.cvt_facility && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);

                        if (findTeam == null)
                        {
                            string name = (thisPS.cvt_facility != null) ? thisPS.cvt_facility.Name : "No Facility Listed";
                            Logger.Trace("FTC Team not found for Facility: " + name + " Creating team.");
                            Team newTeam = new Team()
                            {
                                cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, thisPS.cvt_facility.Id),
                                cvt_Type = new OptionSetValue((int)Teamcvt_Type.FTC),
                                TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisPS.OwningBusinessUnit.Id),
                                Name = "FTC Approval Group @ " + thisPS.cvt_facility.Name
                            };
                            findTeam = new Team
                            {
                                Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                            };
                        }

                        if (thisPS.OwnerId != null && thisPS.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                    #endregion
                    #region Scheduling Resource - TCT Team
                    case cvt_schedulingresource.EntityLogicalName:
                        using (var srv = new Xrm(OrganizationService))
                        {
                            var thisSR = (cvt_schedulingresource)thisRecord;
                            var relatedPS = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.Id == thisSR.cvt_participatingsite.Id);
                            //Check the Provider Facility vs OwnerId
                            if ((thisSR.OwnerId.Name.Contains("TCT") && (thisSR.OwnerId.Name.Contains(relatedPS.cvt_site.Name))))
                                return false;


                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite == relatedPS.cvt_site && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.Staff);

                            if (findTeam == null)
                            {
                                string name = (relatedPS.cvt_site != null) ? relatedPS.cvt_site.Name : "No TMP Site Listed";
                                Logger.Trace("TCT Team not found for TMP Site: " + name + ". Creating team.");
                                Team newTeam = new Team()
                                {
                                    cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, relatedPS.cvt_site.Id),
                                    cvt_Type = new OptionSetValue((int)Teamcvt_Type.Staff),
                                    TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                    BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisSR.OwningBusinessUnit.Id),
                                    Name = "Staff @ " + relatedPS.cvt_site.Name
                                };
                                findTeam = new Team
                                {
                                    Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                                };
                            }

                            if (thisSR.OwnerId != null && thisSR.OwnerId.Id == findTeam.Id)
                                return false;
                            break;
                        }
                    #endregion
                    #region Facility Approval - Provider Facility FTC Team
                    case cvt_facilityapproval.EntityLogicalName:

                        var thisFA = (cvt_facilityapproval)thisRecord;

                        //Check the Provider Facility vs OwnerId
                        if ((thisFA.OwnerId.Name.Contains("FTC") && (thisFA.OwnerId.Name.Contains(thisFA.cvt_providerfacility.Name))))
                            return false;

                        using (var srv = new Xrm(OrganizationService))
                            findTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_Facility == thisFA.cvt_providerfacility && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.FTC);

                        if (findTeam == null)
                        {
                            string name = (thisFA.cvt_providerfacility != null) ? thisFA.cvt_providerfacility.Name : "No Facility Listed";
                            Logger.Trace("FTC Team not found for Facility: " + name + " Creating team.");
                            Team newTeam = new Team()
                            {
                                cvt_Facility = new EntityReference(mcs_facility.EntityLogicalName, thisFA.cvt_providerfacility.Id),
                                cvt_Type = new OptionSetValue((int)Teamcvt_Type.FTC),
                                TeamType = new OptionSetValue((int)TeamTeamType.Owner),
                                BusinessUnitId = new EntityReference(BusinessUnit.EntityLogicalName, thisFA.OwningBusinessUnit.Id),
                                Name = "FTC Approval Group @ " + thisFA.cvt_providerfacility.Name
                            };
                            findTeam = new Team
                            {
                                Id = CreateTeamForAssign(newTeam, Logger, OrganizationService)
                            };
                        }

                        if (thisFA.OwnerId != null && thisFA.OwnerId.Id == findTeam.Id)
                            return false;
                        break;
                        #endregion

                }
                #region Common Code for the Assign
                if (findTeam == null || findTeam.Id == Guid.Empty)
                    return false;
                Logger.Trace("Team found. Assignment started");
                assignOwner.Id = findTeam.Id;

                AssignRequest assignRequest = new AssignRequest()
                {
                    Assignee = assignOwner,
                    Target = new EntityReference(thisRecord.LogicalName, thisRecord.Id)
                };

                OrganizationService.Execute(assignRequest);
                Logger.Trace(thisRecord.LogicalName + " assigned to a Team. Ending AssignOwner.");
                return true;
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Trace($"Error occured while assigning the {thisRecord.LogicalName} entity record with Id {thisRecord.Id} to the team {findTeam?.Id} \nError: {ex.Message}");
            }

            return false;
        }

        internal static bool AssignOwnerStagingResource(Entity thisRecord, MCSLogger logger,
            IOrganizationService organizationService, ref Entity findTeam)
        {
            logger.setMethod = "AssignOwnerStagingResource";
            logger.WriteToFile("Start - Assign Owner AssignOwnerStagingResource");

            var thisStagingResource = (cvt_stagingresource)thisRecord;
            if (thisStagingResource.mcs_Facility == null)
            {
                logger.WriteDebugMessage("Facility information not available.");
                using (var srv = new Xrm(organizationService))
                    findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == "NTTHD");
                return (findTeam == null) ? false : true;
            }

            //Get the FTC team for the facility
            using (var srv = new Xrm(organizationService))
                findTeam =
                    srv.TeamSet.FirstOrDefault(
                        t =>
                            t.cvt_Facility.Id == thisStagingResource.mcs_Facility.Id && t.cvt_Type != null &&
                            t.cvt_Type.Value == (int)Teamcvt_Type.FTC);

            if (thisStagingResource.mcs_RelatedSiteId == null)
            {
                logger.WriteDebugMessage(
                    $"Facility information available. Looking for the FTC team for the Facility {thisStagingResource.mcs_Facility.Name}");
                //Check the Provider Facility vs OwnerId
                if ((thisStagingResource.OwnerId != null && thisStagingResource.OwnerId.Name.Contains("FTC") &&
                     (thisStagingResource.OwnerId.Name.Contains(thisStagingResource.mcs_Facility.Name))))
                    return false;

                string name = thisStagingResource.mcs_Facility.Name;

                if (findTeam == null)
                {
                    logger.WriteToFile(
                        $"The Inventory Staging Resource with Id: {thisStagingResource?.Id} could not assigned to the Facility FTC Team.\nFTC Team not found for Facility: {name}\nPlease create the Team and associate necessary security roles and update the Inventory staging resource record");
                    return false;
                }


                //Check that the Staging Resource's owner is the same name as the site, if so, pass along or query for correct one.
                if (thisStagingResource.OwnerId != null && thisStagingResource.OwnerId.Id == findTeam.Id)
                {
                    logger.WriteDebugMessage(
                        $"FTC Team is available for Facility: {name} and record is already assigned to the Team. Hence skipping the assignment process");
                    return false;
                }

                logger.WriteDebugMessage($"FTC Team is available for Facility: {name}. Assigning record to the Team");
            }
            else
            {
                if (findTeam != null && findTeam.Id != Guid.Empty && thisRecord.Id != Guid.Empty)
                {
                    var principalReference = new EntityReference(findTeam.LogicalName, findTeam.Id);
                    var accessRights = AccessRights.ReadAccess | AccessRights.WriteAccess |
                                       AccessRights.AppendAccess | AccessRights.AppendToAccess;
                    ShareRecord(thisRecord, logger, organizationService, principalReference, accessRights);
                }

                logger.WriteDebugMessage(
                    $"Site information available. Looking for the TCT team for the site {thisStagingResource.mcs_RelatedSiteId.Name}");

                //Check the Provider Facility vs OwnerId
                if ((thisStagingResource.OwnerId.Name.Contains("TCT") &&
                     (thisStagingResource.OwnerId.Name.Contains(thisStagingResource.mcs_RelatedSiteId.Name))))
                {
                    logger.WriteDebugMessage($"Owner {thisStagingResource.OwnerId.Name} appears to have set correctly");
                    return false;
                }

                //Get the correct team
                using (var srv = new Xrm(organizationService))
                    findTeam =
                        srv.TeamSet.FirstOrDefault(
                            t =>
                                t.cvt_TMPSite.Id == thisStagingResource.mcs_RelatedSiteId.Id && t.cvt_Type != null &&
                                t.cvt_Type.Value == (int)Teamcvt_Type.Staff);

                string name = thisStagingResource.mcs_RelatedSiteId.Name;

                if (findTeam == null)
                {
                    logger.WriteToFile(
                        $"The Inventory Staging Resource with Id: {thisStagingResource?.Id} could not assigned to the Site TCT Team.\nTCT Team not found for Site: {name}" +
                        $"\nPlease create the Team and associate necessary security roles and update the Inventory staging resource record");
                    return false;
                }
                else
                {
                    logger.WriteDebugMessage($"TCT Team is available for Site: {name}. Assigning record to the Team");

                    //Check that the Staging Resource's owner is the same name as the site, if so, pass along or query for correct one.
                    if (thisStagingResource.OwnerId != null && thisStagingResource.OwnerId.Id == findTeam.Id)
                        return false;
                }
            }
            return true;
        }

        internal static bool AssignOwnerStagingResource(Entity thisRecord, PluginLogger logger,
            IOrganizationService organizationService, ref Entity findTeam)
        {
            //logger.setMethod = "AssignOwnerStagingResource";
            logger.Trace("Start - Assign Owner AssignOwnerStagingResource");

            var thisStagingResource = (cvt_stagingresource)thisRecord;
            if (thisStagingResource.mcs_Facility == null)
            {
                logger.Trace("Facility information not available.");
                using (var srv = new Xrm(organizationService))
                    findTeam = srv.TeamSet.FirstOrDefault(t => t.Name == "NTTHD");
                return (findTeam == null) ? false : true;
            }

            //Get the FTC team for the facility
            using (var srv = new Xrm(organizationService))
                findTeam =
                    srv.TeamSet.FirstOrDefault(
                        t =>
                            t.cvt_Facility.Id == thisStagingResource.mcs_Facility.Id && t.cvt_Type != null &&
                            t.cvt_Type.Value == (int)Teamcvt_Type.FTC);

            if (thisStagingResource.mcs_RelatedSiteId == null)
            {
                logger.Trace(
                    $"Facility information available. Looking for the FTC team for the Facility {thisStagingResource.mcs_Facility.Name}");
                //Check the Provider Facility vs OwnerId
                if ((thisStagingResource.OwnerId != null && thisStagingResource.OwnerId.Name.Contains("FTC") &&
                     (thisStagingResource.OwnerId.Name.Contains(thisStagingResource.mcs_Facility.Name))))
                    return false;

                string name = thisStagingResource.mcs_Facility.Name;

                if (findTeam == null)
                {
                    logger.Trace(
                        $"The Inventory Staging Resource with Id: {thisStagingResource?.Id} could not assigned to the Facility FTC Team.\nFTC Team not found for Facility: {name}\nPlease create the Team and associate necessary security roles and update the Inventory staging resource record");
                    return false;
                }


                //Check that the Staging Resource's owner is the same name as the site, if so, pass along or query for correct one.
                if (thisStagingResource.OwnerId != null && thisStagingResource.OwnerId.Id == findTeam.Id)
                {
                    logger.Trace(
                        $"FTC Team is available for Facility: {name} and record is already assigned to the Team. Hence skipping the assignment process");
                    return false;
                }

                logger.Trace($"FTC Team is available for Facility: {name}. Assigning record to the Team");
            }
            else
            {
                if (findTeam != null && findTeam.Id != Guid.Empty && thisRecord.Id != Guid.Empty)
                {
                    var principalReference = new EntityReference(findTeam.LogicalName, findTeam.Id);
                    var accessRights = AccessRights.ReadAccess | AccessRights.WriteAccess |
                                       AccessRights.AppendAccess | AccessRights.AppendToAccess;
                    ShareRecord(thisRecord, logger, organizationService, principalReference, accessRights);
                }

                logger.Trace(
                    $"Site information available. Looking for the TCT team for the site {thisStagingResource.mcs_RelatedSiteId.Name}");

                //Check the Provider Facility vs OwnerId
                if ((thisStagingResource.OwnerId.Name.Contains("TCT") &&
                     (thisStagingResource.OwnerId.Name.Contains(thisStagingResource.mcs_RelatedSiteId.Name))))
                {
                    logger.Trace($"Owner {thisStagingResource.OwnerId.Name} appears to have set correctly");
                    return false;
                }

                //Get the correct team
                using (var srv = new Xrm(organizationService))
                    findTeam =
                        srv.TeamSet.FirstOrDefault(
                            t =>
                                t.cvt_TMPSite.Id == thisStagingResource.mcs_RelatedSiteId.Id && t.cvt_Type != null &&
                                t.cvt_Type.Value == (int)Teamcvt_Type.Staff);

                string name = thisStagingResource.mcs_RelatedSiteId.Name;

                if (findTeam == null)
                {
                    logger.Trace(
                        $"The Inventory Staging Resource with Id: {thisStagingResource?.Id} could not assigned to the Site TCT Team.\nTCT Team not found for Site: {name}" +
                        $"\nPlease create the Team and associate necessary security roles and update the Inventory staging resource record");
                    return false;
                }
                else
                {
                    logger.Trace($"TCT Team is available for Site: {name}. Assigning record to the Team");

                    //Check that the Staging Resource's owner is the same name as the site, if so, pass along or query for correct one.
                    if (thisStagingResource.OwnerId != null && thisStagingResource.OwnerId.Id == findTeam.Id)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Create Team in the assign (add the security roles)
        /// </summary>
        /// <param name="newTeam"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <returns></returns>
        internal static Guid CreateTeamForAssign(Team newTeam, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            var newTeamId = OrganizationService.Create(newTeam);
            Logger.WriteDebugMessage("Team created.");

            //Rush the adding of security roles
            if (SetTeamRoles(newTeamId, Team.EntityLogicalName, OrganizationService, Logger))
            {
                Logger.WriteDebugMessage("Allowing time to process security changes.");
                System.Threading.Thread.Sleep(15000);
                Logger.WriteDebugMessage("Resuming assign.");
            }
            return newTeamId;
        }

        /// <summary>
        /// Create Team in the assign (add the security roles)
        /// </summary>
        /// <param name="newTeam"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <returns></returns>
        internal static Guid CreateTeamForAssign(Team newTeam, PluginLogger Logger, IOrganizationService OrganizationService)
        {
            var newTeamId = OrganizationService.Create(newTeam);
            Logger.Trace("Team created.");

            //Rush the adding of security roles
            if (SetTeamRoles(newTeamId, Team.EntityLogicalName, OrganizationService, Logger))
            {
                Logger.Trace("Allowing time to process security changes.");
                System.Threading.Thread.Sleep(15000);
                Logger.Trace("Resuming assign.");
            }
            return newTeamId;
        }

        public static bool SetTeamRoles(Guid Id, string EntityLogicalName, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.setMethod = "SetTeamRoles";
            //Add the Security Roles:
            int addedRoles;
            int removedRoles;
            CvtHelper.UpdateSecurityRoles(Id, Team.EntityLogicalName, OrganizationService, Logger, out addedRoles, out removedRoles);
            if (addedRoles + removedRoles > 0)
            {
                Logger.WriteDebugMessage(string.Format("Added {0} roles, Removed {1} roles", addedRoles, removedRoles));
                return true;
            }
            return false;
        }

        public static bool SetTeamRoles(Guid Id, string EntityLogicalName, IOrganizationService OrganizationService, PluginLogger Logger)
        {
            //Logger.setMethod = "SetTeamRoles";
            //Add the Security Roles:
            int addedRoles;
            int removedRoles;
            CvtHelper.UpdateSecurityRoles(Id, Team.EntityLogicalName, OrganizationService, Logger, out addedRoles, out removedRoles);
            if (addedRoles + removedRoles > 0)
            {
                Logger.Trace(string.Format("Added {0} roles, Removed {1} roles", addedRoles, removedRoles));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Runs assignowner for entire dataset passed in
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <returns></returns>
        internal static string FixOwnershipForEntity(IQueryable<Entity> entitySet, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.setMethod = "FixOwnershipForEntity";
            Logger.WriteDebugMessage("About to update Entity record ownership.");

            var count = 0;
            var entityName = "";
            foreach (var thisRecord in entitySet)
            {
                try
                {
                    bool update = CvtHelper.AssignOwner(thisRecord, Logger, OrganizationService);
                    entityName = thisRecord.LogicalName;
                    if (update)
                        count++;
                }
                catch (Exception ex)
                {
                    Logger.WriteDebugMessage("error message: " + ex.Message);
                }
            }
            var message = string.Format("Assigned ownership for {0}: {1}/{2}. ", entityName, count, entitySet.ToList().Count.ToString());
            Logger.WriteDebugMessage(message);
            return message;
        }
        #endregion

        #region Record Names
        /// <summary>
        /// Returns the record name if it needs to be changed.
        /// </summary>
        /// <param name="EntityObj"></param>
        /// <param name="RecordId"></param>
        /// <param name="Logger"></param>
        /// <param name="OrganizationService"></param>
        /// <returns>Returns "" if no name change, or the name that should be changed to, if derived</returns>
        internal static string ReturnRecordNameIfChanged(Entity Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            string NameIfChanged = "";
            Logger.WriteGranularTimingMessage("Deriving name for " + Record.LogicalName);
            switch (Record.LogicalName)
            {
                //Legacy TSA
                case cvt_mastertsa.EntityLogicalName:
                    Logger.WriteDebugMessage("Attempted operation on deprecated entity Master TSA (cvt_mastertsa) in ReturnRecordIfChanged method of CvtHelperData.cs ");
                    //NameIfChanged = DeriveName((cvt_mastertsa)Record, OnCreate, Logger, OrganizationService);
                    break;
                case mcs_services.EntityLogicalName:
                    Logger.WriteDebugMessage("Attempted operation on deprecated entity TSA (mcs_services) in ReturnRecordIfChanged method of CvtHelperData.cs ");
                    //NameIfChanged = DeriveName((mcs_services)Record, OnCreate, Logger, OrganizationService);
                    break;

                //Resource/Groups
                case mcs_resource.EntityLogicalName:
                    NameIfChanged = DeriveName((mcs_resource)Record, OnCreate, Logger, OrganizationService, string.Empty);
                    break;
                case mcs_resourcegroup.EntityLogicalName:
                    NameIfChanged = DeriveName((mcs_resourcegroup)Record, OnCreate, Logger, OrganizationService, string.Empty);
                    break;

                //Team            
                case Team.EntityLogicalName:
                    NameIfChanged = DeriveName((Team)Record, OnCreate, Logger, OrganizationService);
                    break;

                //Privileging
                case cvt_tssprivileging.EntityLogicalName:
                    NameIfChanged = DeriveName((cvt_tssprivileging)Record, OnCreate, Logger, OrganizationService);
                    break;

                //TSA 2.0
                case cvt_resourcepackage.EntityLogicalName:
                    NameIfChanged = DeriveName((cvt_resourcepackage)Record, OnCreate, Logger, OrganizationService);
                    break;
                case cvt_participatingsite.EntityLogicalName:
                    NameIfChanged = DeriveName((cvt_participatingsite)Record, OnCreate, Logger, OrganizationService, string.Empty);
                    break;
                case cvt_schedulingresource.EntityLogicalName:
                    NameIfChanged = DeriveName((cvt_schedulingresource)Record, OnCreate, Logger, OrganizationService);
                    break;
                case cvt_facilityapproval.EntityLogicalName:
                    NameIfChanged = DeriveName((cvt_facilityapproval)Record, OnCreate, Logger, OrganizationService);
                    break;

                default:
                    Logger.WriteDebugMessage("Entity Name not found. No name generated.");
                    break;
            }

            return NameIfChanged;
        }

        internal static string DeriveName(Team Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Team thisRecord = new Team();
            if (OnCreate) //use record
                thisRecord = Record.ToEntity<Team>();
            else //find record
            {
                using (var srv = new Xrm(OrganizationService))
                    thisRecord = srv.TeamSet.First(t => t.Id == Record.Id);
            }
            string derivedResultField = "";
            var teamServiceLine = new mcs_servicetype();
            if (thisRecord.cvt_ServiceType != null && thisRecord.cvt_ServiceType.Id != null)
            {
                using (var srv = new Xrm(OrganizationService))
                    teamServiceLine = srv.mcs_servicetypeSet.FirstOrDefault(st => st.Id == thisRecord.cvt_ServiceType.Id);
            }

            string serviceLineName = (teamServiceLine != null && teamServiceLine.mcs_name != null) ? teamServiceLine.mcs_name + " " : "";
            var role = (thisRecord.cvt_Type != null) ? (thisRecord.cvt_Type.Value) : -1;

            var teamFacility = new mcs_facility();
            if (thisRecord.cvt_Facility != null && thisRecord.cvt_Facility.Id != null)
            {
                using (var srv = new Xrm(OrganizationService))
                    teamFacility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == thisRecord.cvt_Facility.Id);
            }

            string facilityName = (teamFacility != null && teamFacility.mcs_name != null) ? " @ " + teamFacility.mcs_name : "";
            string roleName = "";

            var teamSite = new mcs_site();
            if (thisRecord.cvt_TMPSite != null && thisRecord.cvt_TMPSite.Id != null)
            {
                using (var srv = new Xrm(OrganizationService))
                    teamSite = srv.mcs_siteSet.FirstOrDefault(f => f.Id == thisRecord.cvt_TMPSite.Id);
            }
            string siteName = (teamSite != null && teamSite.mcs_name != null) ? " @ " + teamSite.mcs_name : "";

            switch (role)
            {
                case 917290000:
                    roleName = "FTC Approval Group";
                    break;
                case 917290001:
                    roleName = "Service Chief Approval Group";
                    break;
                case 917290002:
                    roleName = "Chief of Staff Approval Group";
                    break;
                case 917290003:
                    roleName = "Credentialing and Privileging Officer Approval Group";
                    break;
                case 917290004:
                    roleName = "TSA Notification Group";
                    break;
                case 917290005:
                    roleName = "Scheduler Group";
                    break;
                case 917290006:
                    roleName = "Data Administrators";
                    break;
                case 917290007:
                    roleName = "Staff" + siteName;
                    break;
                case 917290008:
                    roleName = " ER On-Call Providers";
                    facilityName = "";
                    break;
                case 917290009:
                    roleName = "Hub Director";
                    break;
                case 917290010:
                    roleName = "Hub TSA Manager";
                    break;
            }
            derivedResultField = string.Format("{0}{1}{2}", serviceLineName, roleName, facilityName);

            if (string.IsNullOrEmpty(serviceLineName + roleName) || thisRecord.Name == derivedResultField.Trim())
                derivedResultField = "";
            return derivedResultField;
        }

        internal static string DeriveName(cvt_tssprivileging Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            cvt_tssprivileging thisRecord = new cvt_tssprivileging();
            if (OnCreate) //use record
                thisRecord = Record.ToEntity<cvt_tssprivileging>();
            else //find record
            {
                using (var srv = new Xrm(OrganizationService))
                    thisRecord = srv.cvt_tssprivilegingSet.First(p => p.Id == Record.Id);
            }
            string derivedResultField = "";

            var providerName = (thisRecord.cvt_ProviderId != null) ? thisRecord.cvt_ProviderId.Name : "";
            var serviceType = (thisRecord.cvt_ServiceTypeId != null) ? " - " + thisRecord.cvt_ServiceTypeId.Name : "";
            var facilityName = (thisRecord.cvt_PrivilegedAtId != null) ? " @ " + thisRecord.cvt_PrivilegedAtId.Name : "";
            var typeOfPrivileging = (thisRecord.cvt_TypeofPrivileging != null) ? thisRecord.cvt_TypeofPrivileging.Value : 0;
            var privText = "";
            switch (typeOfPrivileging)
            {
                case 917290000:
                    privText = " (Home/Parent)";
                    break;

                case 917290001:
                    privText = " (Proxy/Secondary)";
                    break;
            }

            derivedResultField = string.Format("{0}{1}{2}{3}", providerName, serviceType, facilityName, privText);

            if (thisRecord.cvt_name == derivedResultField)
                derivedResultField = "";
            return derivedResultField;
        }

        internal static string DeriveName(mcs_resource Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService, string siteValue)
        {
            mcs_resource thisRecord;
            if (OnCreate) //use record
                thisRecord = Record.ToEntity<mcs_resource>();
            else //find record
            {
                using (var srv = new Xrm(OrganizationService))
                    thisRecord = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == Record.Id);

                if (thisRecord == null)
                    throw new InvalidPluginExecutionException("TMP Resource Record wasn't found for ID:" + Record.Id);

                if (Record.cvt_CartTypeId != null) thisRecord.cvt_CartTypeId = Record.cvt_CartTypeId;
                if (Record.mcs_UserNameInput != null) thisRecord.mcs_UserNameInput = Record.mcs_UserNameInput;
                if (Record.cvt_building != null) thisRecord.cvt_building = Record.cvt_building;
                if (Record.mcs_room != null) thisRecord.mcs_room = Record.mcs_room;
                if (Record.cvt_systemtype != null) thisRecord.cvt_systemtype = Record.cvt_systemtype;
                if (Record.cvt_room != null) thisRecord.cvt_room = Record.cvt_room;
                if (Record.cvt_uniqueid != null) thisRecord.cvt_uniqueid = Record.cvt_uniqueid;
            }

            string derivedResultField = "";

            //TMP Resource Name
            var mcs_usernameinput = thisRecord.mcs_UserNameInput;

            switch (thisRecord.mcs_Type.Value)
            {
                case (int)mcs_resourcetype.UnspecifiedTctTelepresenter:
                    var cernerUniqueId = thisRecord.mcs_CernerUniqueID;

                    derivedResultField = (cernerUniqueId != null) ? cernerUniqueId : "Default Unspecified TCT/Telepresenter";
                    break;
                case (int)mcs_resourcetype.Room:
                    var building = thisRecord.cvt_building;
                    var room = thisRecord.mcs_room;

                    derivedResultField = (building != null) ? "Bldg. " + building : "";
                    derivedResultField += (derivedResultField != "" && room != null) ? ", " : "";
                    derivedResultField += (room != null) ? "Room " + room : "";

                    if ((derivedResultField == "") && (mcs_usernameinput != null))
                        derivedResultField = mcs_usernameinput;
                    break;
                case (int)mcs_resourcetype.Technology:
                    if (thisRecord.cvt_CartTypeId != null)
                    {
                        var cartType = OrganizationService.Retrieve(cvt_carttype.EntityLogicalName,
                            thisRecord.cvt_CartTypeId.Id, new ColumnSet("cvt_abbreviation"));
                        derivedResultField += cartType.GetAttributeValue<string>("cvt_abbreviation") + " ";
                        if (thisRecord.cvt_systemtype.Value ==
                            (int)cvt_carttypecvt_ResourceSystemType.TelehealthPatientCartSystem)
                            derivedResultField += "Cart ";
                    }
                    else if (thisRecord.cvt_systemtype != null &&
                             thisRecord.cvt_systemtype.Value ==
                             (int)cvt_carttypecvt_ResourceSystemType.TelehealthPatientCartSystem)
                    {
                        derivedResultField += "PC Cart ";
                    }
                    else if (thisRecord.FormattedValues.Contains("cvt_systemtype"))
                        derivedResultField += thisRecord.FormattedValues["cvt_systemtype"]; //SystemType

                    //derivedResultField += thisRecord.cvt_room + " "; //Room Number
                    derivedResultField += (thisRecord.cvt_uniqueid != null) ? ": " + thisRecord.cvt_uniqueid : "";
                    break;
                default:
                    derivedResultField += mcs_usernameinput ?? "";
                    break;
            }

            if (siteValue != string.Empty)
            {
                derivedResultField += " @ " + siteValue;
            }
            else if (thisRecord.mcs_RelatedSiteId != null)
            {
                var thisSite = (mcs_site)OrganizationService.Retrieve(mcs_site.EntityLogicalName, thisRecord.mcs_RelatedSiteId.Id, new ColumnSet("mcs_stationnumber"));
                //using (var srv = new Xrm(OrganizationService))
                //{
                //var site = srv.mcs_siteSet.FirstOrDefault(s => s.Id == thisRecord.mcs_RelatedSiteId.Id);
                if (thisSite != null)
                    derivedResultField += " @ " + thisSite.mcs_StationNumber;
                //}
            }

            if (thisRecord.mcs_Type.Value == (int)mcs_resourcetype.Technology)
            {
                derivedResultField += " Rm. " + thisRecord.cvt_room; //Room Number
            }
            //if (thisRecord.mcs_name == derivedResultField)
            //    derivedResultField = "";
            return derivedResultField;
        }

        internal static string DeriveName(mcs_resourcegroup Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService, string siteValue)
        {
            #region Getting/Validating the record
            mcs_resourcegroup thisRecord = new mcs_resourcegroup();
            if (OnCreate) //use record
                thisRecord = Record.ToEntity<mcs_resourcegroup>();
            else //find record
            {
                Logger.WriteDebugMessage("Naming: Not on create, querying for record.");
                using (var srv = new Xrm(OrganizationService))
                    thisRecord = srv.mcs_resourcegroupSet.FirstOrDefault(r => r.Id == Record.Id);
            }

            if (thisRecord == null)
                throw new InvalidPluginExecutionException("TSS Resource Group Record wasn't found for GUID:" + thisRecord.Id);
            #endregion

            //New fields
            var locationText = "";
            var typeOfAppointmentText = "";
            var specialtyText = "";
            var specialtySubTypeText = "";
            var siteText = "";

            var resourceGroupTypeText = "";
            var allRequiredTech = "";
            var allRequiredRoom = "";

            #region Setting Resource Type
            var resourceType = -1;
            if (Record.Contains("mcs_type") && Record.mcs_Type != null)
            {
                resourceType = Record.mcs_Type.Value;
                Logger.WriteDebugMessage(string.Format("Updated mcs_Type from the context with {0}", Record.mcs_Type));
            }
            else if (thisRecord.Contains("mcs_type") && thisRecord.mcs_Type != null)
            {
                resourceType = thisRecord.mcs_Type.Value;
            }

            #endregion

            #region Location PRO or PAT

            var LocationOption = -1;
            if (Record.Contains("cvt_location") && Record.cvt_location != null)
            {
                LocationOption = Record.cvt_location.Value;
            }
            else if (thisRecord.Contains("cvt_location") && thisRecord.cvt_location != null)
            {
                LocationOption = thisRecord.cvt_location.Value;
            }

            switch (LocationOption)
            {
                case 917290000:
                    locationText = "PRO";
                    break;
                case 917290001:
                    locationText = "PAT";
                    break;

            }
            #endregion

            #region Type of Appointment IND or GRP
            var typeOfAppointmentOption = -1;
            if (Record.Contains("cvt_typeofappointment") && Record.cvt_typeofappointment != null)
            {
                typeOfAppointmentOption = Record.cvt_typeofappointment.Value;
            }
            else if (thisRecord.Contains("cvt_typeofappointment") && thisRecord.cvt_typeofappointment != null)
            {
                typeOfAppointmentOption = thisRecord.cvt_typeofappointment.Value;
            }

            switch (typeOfAppointmentOption)
            {
                case 917290000:
                    typeOfAppointmentText = "IND";
                    break;
                case 917290001:
                    typeOfAppointmentText = "GRP";
                    break;
            }
            #endregion

            #region Specialty and/or Specialty Sub-Type Name or Abbreviation
            //Get Specialty/Sub if PRO or PAT&GRP&AllRequired
            if ((locationText == "PRO") || (locationText == "PAT" && typeOfAppointmentText == "GRP" && resourceType == (int)mcs_resourcetype.PairedResourceGroup))
            {
                Guid specialtyGUID = Guid.Empty;
                //Use Spec from Context first if exists
                if (Record.Contains("cvt_specialty") && Record.cvt_specialty != null)
                {
                    specialtyGUID = Record.cvt_specialty.Id;
                    Logger.WriteDebugMessage(string.Format("Updated cvt_specialty from the context with {0}", Record.cvt_specialty.Name));
                }
                else if (thisRecord.Contains("cvt_specialty") && thisRecord.cvt_specialty != null)
                {
                    specialtyGUID = thisRecord.cvt_specialty.Id;
                }

                //Need to get the abbreviation off of the record
                if (specialtyGUID != Guid.Empty)
                {
                    var specRecord = (mcs_servicetype)OrganizationService.Retrieve(mcs_servicetype.EntityLogicalName, specialtyGUID, new ColumnSet("cvt_abbreviation", "mcs_name"));
                    if (specRecord != null)
                    {
                        specialtyText = !string.IsNullOrEmpty(specRecord.cvt_abbreviation) ? specRecord.cvt_abbreviation : specRecord.mcs_name;
                    }
                }

                Guid specialtySubTypeGUID = Guid.Empty;
                if (Record.Contains("cvt_specialtysubtype") && Record.cvt_specialtysubtype != null)
                {
                    specialtySubTypeGUID = Record.cvt_specialtysubtype.Id;
                    Logger.WriteDebugMessage(string.Format("Updated cvt_specialtysubtype from the context with {0}", Record.cvt_specialtysubtype.Name));
                }
                else if (thisRecord.Contains("cvt_specialtysubtype") && thisRecord.cvt_specialtysubtype != null)
                {
                    specialtySubTypeGUID = thisRecord.cvt_specialtysubtype.Id;
                }

                //Need to get the abbreviation off of the record
                if (specialtySubTypeGUID != Guid.Empty)
                {
                    var specSubTypeRecord = (mcs_servicesubtype)OrganizationService.Retrieve(mcs_servicesubtype.EntityLogicalName, specialtySubTypeGUID, new ColumnSet("cvt_abbreviation", "mcs_name"));
                    if (specSubTypeRecord != null)
                    {
                        specialtySubTypeText = !string.IsNullOrEmpty(specSubTypeRecord.cvt_abbreviation) ? specSubTypeRecord.cvt_abbreviation : specSubTypeRecord.mcs_name;
                    }
                }
            }
            else
                Logger.WriteDebugMessage("Not using the Specialty or Sub-Type for TSS Resource Group Naming.");
            #endregion

            #region TMP Site Name or Abbreviation
            Guid siteGUID = Guid.Empty;
            if (Record.Contains("mcs_relatedsiteid") && Record.mcs_relatedSiteId != null)
            {
                siteGUID = Record.mcs_relatedSiteId.Id;
                Logger.WriteDebugMessage(string.Format("Updated mcs_relatedSiteId from the context with {0}", Record.mcs_relatedSiteId.Name));
            }
            else if (thisRecord.Contains("mcs_relatedsiteid") && thisRecord.mcs_relatedSiteId != null)
            {
                siteGUID = thisRecord.mcs_relatedSiteId.Id;
            }

            //Need to get the abbreviation off of the record
            if (siteValue != string.Empty)
            {
                siteText = " @ " + siteValue;
            }
            else if (siteGUID != Guid.Empty)
            {
                var siteRecord = (mcs_site)OrganizationService.Retrieve(mcs_site.EntityLogicalName, siteGUID, new ColumnSet("mcs_stationnumber", "mcs_name"));
                if (siteRecord != null)
                {
                    siteText = siteRecord.mcs_StationNumber != null ? siteRecord.mcs_StationNumber : siteRecord.mcs_name;
                    siteText = " @ " + siteText;
                }
            }
            else
                Logger.WriteToFile("This TSS Resource Group should have a TMP Site and does not: " + thisRecord.mcs_name + ". GUID: " + thisRecord.Id);
            #endregion

            #region UserName
            var userUniqueID = "";
            if (Record.Contains("mcs_usernameinput") && Record.mcs_UserNameInput != null)
            {
                userUniqueID = (!string.IsNullOrEmpty(Record.mcs_UserNameInput)) ? Record.mcs_UserNameInput : "";
                Logger.WriteDebugMessage(string.Format("Updated mcs_UserNameInput from the context with {0}", Record.mcs_UserNameInput));
            }
            else if (thisRecord.Contains("mcs_usernameinput") && thisRecord.mcs_UserNameInput != null)
            {
                userUniqueID = (!string.IsNullOrEmpty(thisRecord.mcs_UserNameInput)) ? thisRecord.mcs_UserNameInput : "";
            }
            #endregion

            #region Resource Type, Paired Tech/Room
            Logger.WriteDebugMessage("Checking for Resource Type.");
            //Check Resource Type
            switch (resourceType)
            {
                //If Paired
                case (int)mcs_resourcetype.PairedResourceGroup:
                case (int)mcs_resourcetype.Technology:
                    Logger.WriteDebugMessage("Resource Type is 'Paired' or 'Technology.'");
                    resourceGroupTypeText = thisRecord.FormattedValues["mcs_type"].ToString();
                    using (var srv = new Xrm(OrganizationService))
                    {
                        var groupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == thisRecord.Id);

                        foreach (mcs_groupresource gr in groupResources)
                        {
                            if (gr.mcs_RelatedResourceId != null)
                            {
                                var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == gr.mcs_RelatedResourceId.Id);
                                if (resource != null)
                                {
                                    Logger.WriteDebugMessage("Found the TSS Resource.");
                                    if (allRequiredTech == "" && resource.mcs_Type?.Value == (int)mcs_resourcetype.Technology)
                                    {
                                        allRequiredTech = resource.FormattedValues["cvt_systemtype"]?.ToString();
                                        Logger.WriteDebugMessage("resource.cvt_systemtype.ToString() = " + resource.FormattedValues["cvt_systemtype"]?.ToString());
                                    }
                                    if (resourceType != (int)mcs_resourcetype.Technology && allRequiredRoom == "" && resource.mcs_Type?.Value == (int)mcs_resourcetype.Room && resource.mcs_room != null)
                                        allRequiredRoom = "Room " + resource.mcs_room;
                                }
                            }
                        }
                    }
                    break;
                case -1:
                    resourceGroupTypeText = "";
                    break;
                default:
                    Logger.WriteDebugMessage("Before mcs_type formattedValues");
                    resourceGroupTypeText = thisRecord.FormattedValues["mcs_type"].ToString() + "s";
                    break;
            }
            #endregion

            #region Constructing the Name
            Logger.WriteDebugMessage("Before derivedResultField");
            //Specialty & Specialty Sub-Type - if PRO or PAT&GRP&AllRequired
            var derivedResultField = !string.IsNullOrEmpty(specialtyText) ? specialtyText + "." : "";
            derivedResultField += !string.IsNullOrEmpty(specialtySubTypeText) ? specialtySubTypeText + "." : "";

            Logger.WriteDebugMessage("Before PAT check");
            //Tech Type - Pat&AR&Ind or Pat&Tech
            if ((locationText == "PAT" && resourceType == (int)mcs_resourcetype.PairedResourceGroup && typeOfAppointmentText == "IND") || (locationText == "PAT" && resourceType == (int)mcs_resourcetype.Technology))
            {
                derivedResultField += !string.IsNullOrEmpty(allRequiredTech) ? allRequiredTech + "." : "";
                Logger.WriteDebugMessage("Included Tech in the name");
            }
            else
                Logger.WriteDebugMessage("Did not include Tech in the name");

            //Room Number
            if (locationText == "PAT" && resourceType == (int)mcs_resourcetype.PairedResourceGroup)
                derivedResultField += !string.IsNullOrEmpty(allRequiredRoom) ? allRequiredRoom + "." : "";

            //Unique Id
            derivedResultField += !string.IsNullOrEmpty(userUniqueID) ? userUniqueID : "";

            //Type of Appt
            if (locationText == "PAT" && resourceType == (int)mcs_resourcetype.PairedResourceGroup)
            {
                //Trim leading or trailing "." from derivedResultField
                derivedResultField = derivedResultField.Trim('.');
                derivedResultField += !string.IsNullOrEmpty(typeOfAppointmentText) ? "." + typeOfAppointmentText : "";
            }
            //Trim leading or trailing "." from derivedResultField
            derivedResultField = derivedResultField.Trim('.');

            //: Resource Group Type
            derivedResultField += " : " + resourceGroupTypeText + siteText;

            //Location
            derivedResultField += !string.IsNullOrEmpty(locationText) ? "." + locationText : "";

            //In common:
            //{unique id} : {Resource Group Type} @ {TSS site}.{PAT/PRO}
            #endregion

            Logger.WriteDebugMessage("Automatically constructed name: " + derivedResultField);
            Logger.WriteDebugMessage("Name from the record: " + thisRecord.mcs_name.Trim());

            if (string.IsNullOrEmpty(derivedResultField) || thisRecord.mcs_name.Trim() == derivedResultField.Trim())
                derivedResultField = "";
            return derivedResultField;
        }

        internal static string DeriveName(cvt_facilityapproval Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                #region Getting/Validating the record
                cvt_facilityapproval thisRecord = new cvt_facilityapproval();
                if (OnCreate) //use record
                    thisRecord = Record.ToEntity<cvt_facilityapproval>();
                else //find record
                {
                    Logger.WriteDebugMessage("Naming: Not on create, querying for record.");

                    thisRecord = srv.cvt_facilityapprovalSet.FirstOrDefault(r => r.Id == Record.Id);
                }

                if (thisRecord == null)
                    throw new InvalidPluginExecutionException("Facility Approval Record wasn't found for GUID:" + thisRecord.Id);
                #endregion

                //New fields
                var derivedResultField = "";

                #region  Constructing the Name
                if (Record.Contains("cvt_resourcepackage") && Record.cvt_resourcepackage != null)
                {
                    var resPackage = srv.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == Record.cvt_resourcepackage.Id);
                    if (resPackage != null)
                        derivedResultField += resPackage.cvt_name + ": ";
                }

                if (Record.Contains("cvt_providerfacility") && Record.cvt_providerfacility != null)
                {
                    var proFacility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == Record.cvt_providerfacility.Id);
                    if (proFacility != null)
                        derivedResultField += proFacility.mcs_name + " to ";
                }

                if (Record.Contains("cvt_patientfacility") && Record.cvt_patientfacility != null)
                {
                    var patFacility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == Record.cvt_patientfacility.Id);
                    if (patFacility != null)
                        derivedResultField += " to " + patFacility.mcs_name;
                }
                #endregion

                Logger.WriteDebugMessage("Automatically constructed name: " + derivedResultField);
                Logger.WriteDebugMessage("Name from the record: " + thisRecord.cvt_name.Trim());

                if (string.IsNullOrEmpty(derivedResultField) || thisRecord.cvt_name.Trim() == derivedResultField.Trim())
                    derivedResultField = "";
                return derivedResultField;
            }
        }

        internal static string DeriveName(cvt_participatingsite Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService, string siteName)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                #region Getting/Validating the record
                cvt_participatingsite thisRecord = new cvt_participatingsite();
                if (OnCreate) //use record
                    thisRecord = Record.ToEntity<cvt_participatingsite>();
                else //find record
                {
                    Logger.WriteDebugMessage("Naming: Not on create, querying for record.");

                    thisRecord = srv.cvt_participatingsiteSet.FirstOrDefault(r => r.Id == Record.Id);
                }

                if (thisRecord == null)
                    throw new InvalidPluginExecutionException("Participating Site Record wasn't found for GUID:" + thisRecord.Id);
                #endregion

                //New fields
                var derivedResultField = "";

                #region  Constructing the Name

                if (Record.Contains("cvt_locationtype") && Record.cvt_locationtype != null)
                {
                    switch (Record.cvt_locationtype.Value)
                    {
                        case 917290000:
                            derivedResultField = "Pro - ";
                            break;
                        case 917290001:
                            derivedResultField = "Pat - ";
                            break;
                    }
                }

                if (siteName != string.Empty)
                {
                    derivedResultField += siteName;
                }
                else if (Record.Contains("cvt_site") && Record.cvt_site != null)
                {
                    //var site = srv.mcs_siteSet.FirstOrDefault(s => s.Id == Record.cvt_site.Id);
                    var site = (mcs_site)OrganizationService.Retrieve(mcs_site.EntityLogicalName, Record.cvt_site.Id, new ColumnSet("mcs_name"));

                    if (site != null)
                    {
                        derivedResultField += site.mcs_name;
                        if (string.IsNullOrEmpty(thisRecord.cvt_name)) thisRecord.cvt_name = derivedResultField;
                    }
                }
                #endregion

                Logger.WriteTxnTimingMessage($"Automatically constructed name: {derivedResultField}");
                Logger.WriteDebugMessage("Automatically constructed name: " + derivedResultField);
                Logger.WriteDebugMessage("Name from the record: " + thisRecord.cvt_name.Trim());

                if (string.IsNullOrEmpty(derivedResultField) || thisRecord.cvt_name.Trim() == derivedResultField.Trim())
                    derivedResultField = "";
                return derivedResultField;
            }
        }

        internal static string DeriveName(cvt_schedulingresource Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                #region Getting/Validating the record
                cvt_schedulingresource thisRecord = new cvt_schedulingresource();
                if (OnCreate) //use record
                    thisRecord = Record.ToEntity<cvt_schedulingresource>();
                else //find record
                {
                    Logger.WriteDebugMessage("Naming: Not on create, querying for record.");

                    thisRecord = srv.cvt_schedulingresourceSet.FirstOrDefault(r => r.Id == Record.Id);
                }

                if (thisRecord == null)
                    throw new InvalidPluginExecutionException("Scheduling Resource Record wasn't found for GUID:" + thisRecord.Id);
                #endregion

                //New fields
                var derivedResultField = "";

                #region  Constructing the Name


                if (Record.Contains("cvt_tmpresourcegroup") && Record.cvt_tmpresourcegroup != null)
                {
                    var resGr = srv.mcs_resourcegroupSet.FirstOrDefault(rg => rg.Id == Record.cvt_tmpresourcegroup.Id);
                    if (resGr != null)
                        derivedResultField = resGr.mcs_name;
                }

                if (Record.Contains("cvt_tmpresource") && Record.cvt_tmpresource != null)
                {
                    var res = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == Record.cvt_tmpresource.Id);
                    if (res != null)
                        derivedResultField = res.mcs_name;
                }

                if (Record.Contains("cvt_user") && Record.cvt_user != null)
                {
                    var user = srv.SystemUserSet.FirstOrDefault(su => su.Id == Record.cvt_user.Id);
                    if (user != null)
                        derivedResultField = user.FullName;
                }
                #endregion

                Logger.WriteDebugMessage("Automatically constructed name: " + derivedResultField);
                Logger.WriteDebugMessage("Name from the record: " + thisRecord.cvt_name.Trim());

                if (string.IsNullOrEmpty(derivedResultField) || thisRecord.cvt_name.Trim() == derivedResultField.Trim())
                    derivedResultField = "";
                return derivedResultField;
            }
        }

        internal static string DeriveName(cvt_resourcepackage Record, bool OnCreate, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            #region Getting/Validating the record
            cvt_resourcepackage thisRecord = new cvt_resourcepackage();
            if (OnCreate) //use record
                thisRecord = Record.ToEntity<cvt_resourcepackage>();
            else //find record
            {
                Logger.WriteDebugMessage("Naming: Not on create, querying for record.");
                using (var srv = new Xrm(OrganizationService))
                    thisRecord = srv.cvt_resourcepackageSet.FirstOrDefault(r => r.Id == Record.Id);
            }

            if (thisRecord == null)
                throw new InvalidPluginExecutionException("Scheduling Package Record wasn't found for GUID:" + thisRecord.Id);
            #endregion

            //New fields
            var specialtyId = (Record.Contains("cvt_specialty") && Record.cvt_specialty != null) ? Record.cvt_specialty.Id : Guid.Empty;
            var specialtySubtypeId = (Record.Contains("cvt_specialtysubtype") && Record.cvt_specialtysubtype != null) ? Record.cvt_specialtysubtype.Id : Guid.Empty;
            var providerLocation = (Record.Contains("cvt_providerlocationtype") && Record.cvt_providerlocationtype != null) ? Record.cvt_providerlocationtype.Value : -1;
            var patientLocation = (Record.Contains("cvt_patientlocationtype") && Record.cvt_patientlocationtype != null) ? Record.cvt_patientlocationtype.Value : -1;
            var telehealthModality = (Record.Contains("cvt_availabletelehealthmodality") && Record.cvt_availabletelehealthmodality != null) ? Record.cvt_availabletelehealthmodality.Value : -1;
            var isGroup = (Record.Contains("cvt_groupappointment") && Record.cvt_groupappointment != null) ? Record.cvt_groupappointment.Value : false;
            var intraOrInter = (Record.Contains("cvt_intraorinterfacility") && Record.cvt_intraorinterfacility != null) ? Record.cvt_intraorinterfacility.Value : -1;

            var specialtyName = "";
            var providerLocationText = "";
            var patientLocationText = "";
            var telehealthModalityText = "";
            var isGroupText = "";
            var isIntraText = "";

            #region Getting Specialty
            if (specialtyId != Guid.Empty)
            {
                specialtyName = Record.cvt_specialty.Name;
                //query for potential abbreviation
                var specialtyRecord = (mcs_servicetype)OrganizationService.Retrieve(mcs_servicetype.EntityLogicalName, specialtyId, new ColumnSet(true));
                if (specialtyRecord != null)
                {
                    specialtyName = specialtyRecord.cvt_abbreviation != null ? specialtyRecord.cvt_abbreviation : specialtyRecord.mcs_name;
                }

                if (specialtySubtypeId != Guid.Empty)
                {
                    //query for potential abbreviation
                    var specialtySubTypeRecord = (mcs_servicesubtype)OrganizationService.Retrieve(mcs_servicesubtype.EntityLogicalName, specialtySubtypeId, new ColumnSet(true));
                    if (specialtySubTypeRecord != null)
                    {
                        specialtyName += (specialtySubTypeRecord.cvt_abbreviation != null) ? ":" + specialtySubTypeRecord.cvt_abbreviation : ":" + specialtySubTypeRecord.mcs_name;
                    }
                }
            }
            #endregion

            #region Location

            if (providerLocation != -1)
            {
                switch (providerLocation)
                {
                    case (int)cvt_resourcepackagecvt_providerlocationtype.ClinicBased:
                        providerLocationText += " CB";
                        break;
                    case (int)cvt_resourcepackagecvt_providerlocationtype.Telework:
                        providerLocationText += " TW";
                        break;
                }
            }

            if (patientLocation != -1)
            {
                switch (patientLocation)
                {
                    case (int)cvt_resourcepackagecvt_patientlocationtype.ClinicBased:
                        patientLocationText = " CB";
                        break;
                    case (int)cvt_resourcepackagecvt_patientlocationtype.VAVideoConnectTelephone:
                        patientLocationText = " VVC";
                        break;
                }
            }

            if (telehealthModality != -1)
            {
                switch (telehealthModality)
                {
                    case (int)cvt_resourcepackagecvt_availabletelehealthmodality.ClinicalVideoTelehealth:
                        telehealthModalityText = " CVT ";
                        break;
                    case (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward:
                        telehealthModalityText = " SFT ";
                        break;
                    case (int)cvt_resourcepackagecvt_availabletelehealthmodality.Telephone:
                        telehealthModalityText = " PHONE ";
                        break;
                }
            }
            if (isGroup)
                isGroupText = ", Grp";
            else
                isGroupText = ", Ind";

            //Logger.WriteDebugMessage($"intraOrInter: {intraOrInter}");
            if (intraOrInter != -1)
            {
                switch (intraOrInter)
                {
                    case (int)cvt_resourcepackagecvt_intraorinterfacility.Intrafacility:
                        isIntraText = ", Intra";
                        break;
                    case (int)cvt_resourcepackagecvt_intraorinterfacility.Interfacility:
                        isIntraText = ", Inter";
                        break;
                }
            }
            //Logger.WriteDebugMessage($"isIntraText: {isIntraText}");
            #endregion

            #region Constructing the Name
            Logger.WriteDebugMessage("Before derivedResultField");
            var derivedResultField = $"{specialtyName}{telehealthModalityText}- Pat{patientLocationText}, Pro{providerLocationText}{isGroupText}{isIntraText}";
            #endregion
            var name = string.IsNullOrEmpty(thisRecord.cvt_name) ? string.Empty : thisRecord.cvt_name.Trim();
            Logger.WriteDebugMessage("Automatically constructed name: " + derivedResultField);
            Logger.WriteDebugMessage("Name from the record: " + name);

            if (string.IsNullOrEmpty(derivedResultField) || name == derivedResultField.Trim())
                derivedResultField = "";
            return derivedResultField;
        }

        #endregion

        #region Assessing Resources - PS, SR, PRG
        internal static bool ValidatePS(Guid ParticipatingSiteId, cvt_schedulingresource NewSchedulingResource, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("starting");

            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    #region Validation
                    if (ParticipatingSiteId == Guid.Empty)
                    {
                        Logger.WriteDebugMessage("ParticipatingSiteId has no value, exiting function.");
                        return false;
                    }

                    var thisParticipatingSite = srv.cvt_participatingsiteSet.FirstOrDefault(rg => rg.Id == ParticipatingSiteId);
                    if (thisParticipatingSite == null)
                    {
                        Logger.WriteDebugMessage("Participating Site record cannot be retrieved, exiting function.");
                        return false;
                    }

                    if (thisParticipatingSite.cvt_locationtype == null)
                    {
                        Logger.WriteDebugMessage("Participating Site record has no location type, exiting function.");
                        return false;
                    }


                    if (thisParticipatingSite.cvt_resourcepackage == null)
                    {
                        Logger.WriteDebugMessage("Participating Site record has no related scheduling package, exiting function.");
                        return false;
                    }

                    var parentSchedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(rp => rp.Id == thisParticipatingSite.cvt_resourcepackage.Id);
                    if (parentSchedulingPackage == null)
                    {
                        Logger.WriteDebugMessage("Scheduling Package record cannot be retrieved, exiting function.");
                        return false;
                    }

                    if (parentSchedulingPackage.cvt_availabletelehealthmodality == null)
                    {
                        Logger.WriteDebugMessage("Scheduling Package record has no Available Telehealth Modality, exiting function.");
                        return false;
                    }
                    bool isSFT = parentSchedulingPackage.cvt_availabletelehealthmodality.Value == (int)cvt_resourcepackagecvt_availabletelehealthmodality.StoreandForward ? true : false;
                    bool isProviderPS = thisParticipatingSite.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider ? true : false;

                    int newvcDirect = 0;
                    int newvcPRG = 0;
                    int newproviderDirect = 0;
                    int newproviderPRG = 0;
                    int newprgProviderNoVC = 0;
                    if (NewSchedulingResource.Id != Guid.Empty)
                    {
                        Logger.WriteDebugMessage("New Resource detected: " + NewSchedulingResource.cvt_name);
                        verifyRGHasRelated(NewSchedulingResource, Logger, OrganizationService);
                        var srType = AssessSchedulingResource(NewSchedulingResource, Logger, OrganizationService, out newvcDirect, out newvcPRG, out newproviderDirect, out newproviderPRG, out newprgProviderNoVC);

                        Logger.WriteDebugMessage("srType returned is: " + srType);
                        if (srType != "directvc" && srType != "prg")
                        {
                            Logger.WriteDebugMessage("No need to continue assessing record, it is not a PRG or a standalone VC.");
                            return true;
                        }

                        if (newvcPRG == 0)
                            Logger.WriteDebugMessage("Did not find a PRG VC on the newly added SR.");
                        Logger.WriteDebugMessage($"New VC/Provider Counts. newvcDirect: ({newvcDirect}); newvcPRG: ({newvcPRG}); newproviderDirect: ({newproviderDirect}); newproviderPRG: ({newproviderPRG}); newprgProviderNoVC: ({newprgProviderNoVC}) ");
                    }
                    else
                        Logger.WriteDebugMessage("New Resource not detected.");
                    #endregion

                    int totalvcDirect = 0;
                    int totalvcPRG = 0;
                    int totalproviderDirect = 0;
                    int totalproviderPRG = 0;
                    int totalprgProviderNoVC = 0;

                    var schedulingResources = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_participatingsite.Id == ParticipatingSiteId);

                    //Validate the number of standalone VC and PRGs in VC
                    foreach (cvt_schedulingresource sr in schedulingResources)
                    {
                        if (sr.Id != NewSchedulingResource.Id)
                        {
                            verifyRGHasRelated(sr, Logger, OrganizationService);
                            //Assess the existing scheduling resources, not the new one
                            AssessSchedulingResource(sr, Logger, OrganizationService, out int vcDirect, out int vcPRG, out int providerDirect, out int providerPRG, out int prgProviderNoVC);
                            totalvcDirect += vcDirect;
                            totalvcPRG += vcPRG;
                            totalproviderDirect += providerDirect;
                            totalproviderPRG += providerPRG;
                            totalprgProviderNoVC += prgProviderNoVC;
                        }
                    }
                    Logger.WriteDebugMessage($"VC/Provider Counts. totalvcDirect: ({totalvcDirect}); totalvcPRG: ({totalvcPRG}); totalproviderDirect: ({totalproviderDirect}); totalproviderPRG: ({totalproviderPRG}); totalprgProviderNoVC: ({totalprgProviderNoVC}) ");

                    if (!isProviderPS)
                    {
                        Logger.WriteDebugMessage("Patient PS.");
                        if (NewSchedulingResource.Id != Guid.Empty)
                        {
                            Logger.WriteDebugMessage("Assessing newly added Patient Scheduling Resource: " + NewSchedulingResource.cvt_name);

                            if (newvcDirect > 0 && totalvcPRG > 0)
                            {
                                Logger.WriteDebugMessage("Patient PS, cannot have an existing PRG with VC and try to add a direct VC.");
                                throw new InvalidPluginExecutionException("customYou cannot add a VistA Clinic if there is already a Paired Resource Group with a VistA Clinic. Please use an appropriate Paired Resource Group or remove the existing Paired Resource Group with VistA Clinic before adding a new one.");
                            }
                            if (newvcDirect > 0 && totalvcPRG > 0)
                            {
                                Logger.WriteDebugMessage("Patient PS, cannot have an existing PRG with VC and try to add a direct VC.");
                                throw new InvalidPluginExecutionException("customYou cannot add a Paired Resource Group without a VistA Clinic if there is already a Paired Resource Group with a VistA Clinic on a Participating Site. Please use an appropriate Paired Resource Group in order to schedule.​");
                            }
                        }
                        //When adding a Single Resource of Type=VistA Clinic to a Patient Participating Site, and a Scheduling Resource of Type=VistA Clinic already exists at the Participating Site, the user is thrown an error message that states:
                        if ((newvcDirect > 0 && totalvcDirect > 0) || (totalvcDirect + newvcDirect > 1))
                            throw new InvalidPluginExecutionException("customA Patient-side VistA Clinic already exists for this Patient Site on this Scheduling Package. Please remove the existing VistA Clinic before adding a new one.");
                        //When adding a Paired Resource Group that includes a VistA Clinic, and a Scheduling Resource of Type=VistA Clinic already exists at the Participating Site, the user is thrown an error message that states: ""
                        if (newvcPRG > 0 && totalvcDirect > 0)
                            throw new InvalidPluginExecutionException("customA Patient-side VistA Clinic already exists for this Patient Site on this Scheduling Package. Please remove before adding a Paired Resource Group with a VistA Clinic.");

                        if (newvcPRG + newvcDirect + totalvcPRG + totalvcDirect == 0)
                        {
                            var ps = srv.cvt_participatingsiteSet.FirstOrDefault(p => p.Id == ParticipatingSiteId);
                            if (ps == null)
                            {
                                Logger.WriteDebugMessage($"Participating Site with id: {ParticipatingSiteId} does not exist");
                                throw new InvalidPluginExecutionException($"customParticipating Site with id: {ParticipatingSiteId} does not exist");
                            }
                        }
                    }
                    else
                    {
                        Logger.WriteDebugMessage("Provider PS.");
                        if (NewSchedulingResource.Id != Guid.Empty)
                        {
                            Logger.WriteDebugMessage("Assessing newly added Provider Scheduling Resource: " + NewSchedulingResource.cvt_name);

                            if (newvcDirect > 0 && totalvcPRG > 0 && !isSFT)
                            {
                                Logger.WriteDebugMessage("Provider but not SFT, cannot have an existing PRG with VC and try to add a direct VC.");
                                throw new InvalidPluginExecutionException("customYou cannot have both a VistA Clinic resource type and a Paired Resource Group with a VistA Clinic on a Participating Site. Please remove one in order to schedule.​​");
                            }
                            if (newvcPRG == 0 && totalvcPRG > 0 && !isSFT)
                            {
                                Logger.WriteDebugMessage("Provider but not SFT, cannot have an existing PRG with VC and try to add a PRG without a VC.");
                                throw new InvalidPluginExecutionException("customYou cannot add a Paired Resource Group without a VistA Clinic if there is already a Paired Resource Group with a VistA Clinic on a Participating Site. Please use an appropriate Paired Resource Group in order to schedule.​");
                            }

                        }
                        if (newvcDirect + totalvcDirect > 1)
                            throw new InvalidPluginExecutionException("customYou cannot have more than one standalone VistA Clinic resource on a Participating Site. Please remove one in order to schedule.");

                        //If a Scheduling Resource of Type= VistA Clinic exists on the PS but also one in a Paired Resource Group (PRG), throw error with message:  ​
                        if (newvcDirect + totalvcDirect > 0 && newvcPRG + totalvcPRG > 0)
                            throw new InvalidPluginExecutionException("customYou cannot have both a VistA Clinic resource type and a Paired Resource Group with a VistA Clinic on a Participating Site. Please remove one in order to schedule.");
                        //If a SR of Type=Provider exists on the PS but not in a PRG and a standalone VistA Clinic is not included, throw error with message:
                        //Removing the two following checks to remedy DevOps Bug 8960 ----  WMC 20190723
                        //if (newproviderDirect + totalproviderDirect > 0 && newproviderPRG + totalproviderPRG == 1 && newvcDirect + totalvcDirect == 0)
                        //    throw new InvalidPluginExecutionException("customYou cannot have a standalone Provider without adding a VistA Clinic too. Please add a VistA clinic or use an appropriate Paired Resource Group in order to schedule.​");
                        //if (newproviderDirect + totalproviderDirect > 0 && newvcDirect + totalvcDirect == 0)
                        //    throw new InvalidPluginExecutionException("customYou cannot have a standalone Provider without adding a VistA Clinic too. Please add a VistA clinic or use an appropriate Paired Resource Group in order to schedule.​");

                        if (newvcPRG + newvcDirect + totalvcPRG + totalvcDirect == 0)
                        {
                            Logger.WriteDebugMessage("Only SFT Provider PS can have 0 VCs.");
                            if (isSFT)
                                Logger.WriteDebugMessage("Only SFT Provider PS can have 0 VCs.");
                        }
                    }

                    Logger.WriteDebugMessage("ending");
                    return true;
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

        internal static void CheckPairedResourceGroup(Guid ResourceGroupId, Guid ResourceId, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("starting");
            string errorMessage = "A VistA Clinic already exists as a TMP Resource in this Paired Resource Group. Please remove the existing VistA Clinic from the Paired Resource Group before adding a new one.";

            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    if (ResourceId != Guid.Empty)
                    {
                        //This is specific to a newly added Group Resource
                        var thisResource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == ResourceId);

                        if (thisResource != null && thisResource.mcs_Type != null && thisResource.mcs_Type.Value != (int)mcs_resourcetype.VistaClinic)
                        {
                            //This is not a VC, we can exit
                            return;
                        }
                        else
                            Logger.WriteDebugMessage("This is a VC, evaluating if it is the only one.");
                    }
                    else
                    {
                        Logger.WriteDebugMessage("No Resource added, no reason to evaluate.");
                        return;
                    }
                    if (ResourceGroupId == Guid.Empty)
                        return;
                    else
                    {
                        Logger.WriteDebugMessage("ResourceGroupId not empty.");
                        mcs_resourcegroup thisResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(rg => rg.Id == ResourceGroupId);
                        if (thisResourceGroup != null && thisResourceGroup.mcs_Type != null && thisResourceGroup.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                        {
                            Logger.WriteDebugMessage("This is a Paired Resource Group.");
                            //Make sure another VC does not exist
                            var otherGRs = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == ResourceGroupId); //&& gr.Id != ResourceId
                            Logger.WriteDebugMessage("Group Resources found related to this PRG: " + otherGRs.ToList().Count.ToString());
                            foreach (mcs_groupresource item in otherGRs)
                            {
                                Logger.WriteDebugMessage("Assessing GR: " + item.mcs_name);
                                if (item.mcs_RelatedResourceId != null)
                                {
                                    //Found Related Resource, could be VC, Room, Tech
                                    if (item.mcs_RelatedResourceId.Id != ResourceId)
                                    {
                                        var childResource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == item.mcs_RelatedResourceId.Id);
                                        if (childResource != null && childResource.mcs_Type != null && childResource.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic)
                                        {
                                            throw new InvalidPluginExecutionException(errorMessage);
                                        }
                                    }
                                    else
                                        Logger.WriteDebugMessage("Skipping evaluation of the Resources, it is the one being added.");
                                }
                            }
                        }
                        else
                        {
                            Logger.WriteDebugMessage("Not a Paired Resource Group. Exiting check.");
                            return;
                        }
                    }
                    Logger.WriteDebugMessage("ending");
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

        internal static string AssessSchedulingResource(cvt_schedulingresource sr, MCSLogger Logger, IOrganizationService OrganizationService, out int vcDirect, out int vcPRG, out int providerDirect, out int providerPRG, out int prgProviderNoVC)
        {
            Logger.WriteDebugMessage("starting");
            string returnedValue = "";
            vcDirect = 0;
            vcPRG = 0;
            providerDirect = 0;
            providerPRG = 0;
            prgProviderNoVC = 0;

            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    if (sr.cvt_tmpresource != null)
                    {
                        var srResource = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == sr.cvt_tmpresource.Id);
                        if (srResource != null && srResource.mcs_Type != null && srResource.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic)
                        {
                            Logger.WriteDebugMessage("Found Direct VC.");
                            vcDirect++;
                            returnedValue = "directvc";
                        }
                    }
                    if (sr.cvt_user != null)
                    {
                        var srUser = srv.SystemUserSet.FirstOrDefault(su => su.Id == sr.cvt_user.Id);
                        if (srUser != null && srUser.cvt_type != null && srUser.cvt_type.Value == (int)SystemUsercvt_type.ClinicianProvider)
                        {
                            Logger.WriteDebugMessage("Found Direct Provider.");
                            providerDirect++;
                            returnedValue = "directprovider";
                        }
                    }
                    if (sr.cvt_tmpresourcegroup != null)
                    {
                        var srResouceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(r => r.Id == sr.cvt_tmpresourcegroup.Id);
                        if (srResouceGroup != null && srResouceGroup.mcs_Type != null && srResouceGroup.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                        {
                            //Query for Group Resource linker records
                            var linkerGR = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == srResouceGroup.Id);
                            int vcInThisPRG = 0;
                            int providerInThisPRG = 0;
                            returnedValue = "prg";
                            foreach (mcs_groupresource linker in linkerGR)
                            {
                                if (linker != null && linker.mcs_RelatedResourceId != null)
                                {
                                    var relatedR = srv.mcs_resourceSet.FirstOrDefault(r => r.Id == linker.mcs_RelatedResourceId.Id);
                                    if (relatedR != null && relatedR.mcs_Type != null && relatedR.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic)
                                    {
                                        Logger.WriteDebugMessage("Found PRG VC.");
                                        vcPRG++;
                                        vcInThisPRG++;
                                    }

                                }
                                else if (linker != null && linker.mcs_RelatedUserId != null)
                                {
                                    var relatedU = srv.SystemUserSet.FirstOrDefault(u => u.Id == linker.mcs_RelatedUserId.Id);
                                    if (relatedU != null && relatedU.cvt_type != null && relatedU.cvt_type.Value == (int)SystemUsercvt_type.ClinicianProvider)
                                    {
                                        Logger.WriteDebugMessage("Found PRG Provider.");
                                        providerPRG++;
                                        providerInThisPRG++;
                                    }
                                }
                            }
                            if (providerInThisPRG == 1 && vcInThisPRG == 0)
                            {
                                Logger.WriteDebugMessage("PRG has Provider but no VC.");
                                prgProviderNoVC++;
                            }
                        }
                    }
                    Logger.WriteDebugMessage("ending");
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
            Logger.WriteDebugMessage("Returning value: " + returnedValue);
            return returnedValue;
        }

        internal static void verifyRGHasRelated(cvt_schedulingresource sr, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.WriteDebugMessage("starting");
            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    if (sr.cvt_tmpresourcegroup != null)
                    {
                        var srResouceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(r => r.Id == sr.cvt_tmpresourcegroup.Id);
                        if (srResouceGroup != null && srResouceGroup.mcs_Type != null && srResouceGroup.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup)
                        {
                            //Query for Group Resource linker records
                            var linkerGR = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == srResouceGroup.Id).ToList();
                            if (linkerGR.Count == 0)
                            {
                                throw new InvalidPluginExecutionException($"TMP Resource Group ({srResouceGroup.mcs_name}) has no related Resources.  It cannot be empty, please go back and add Resources to the Resource Group. ");
                            }
                        }
                    }
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
            Logger.WriteDebugMessage("ending");
        }
        #endregion
    }
}