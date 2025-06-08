using MCSShared;
using MCSUtilities2011;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class UserUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public UserUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }       
        #endregion

        #region Functions
        public override void Execute()
        {
            if (PluginExecutionContext.Depth > 2) { return; }
            var entity = (Entity)PluginExecutionContext.InputParameters["Target"];
            var thisUser = entity.ToEntity<SystemUser>();

            bool updateUserConnections = thisUser.cvt_updateuserconnections == null ? false : thisUser.cvt_updateuserconnections.Value;
            bool deleteUserConnections = thisUser.cvt_deleteuserconnections == null ? false : thisUser.cvt_deleteuserconnections.Value;
            bool saveUserRoles = thisUser.cvt_SaveSecurityRoles ?? false;
            bool disableUserfromPriv = thisUser.cvt_disable == null ? false : thisUser.cvt_disable.Value;

            var ReplacementUser = thisUser.cvt_replacementuser;

            if (ReplacementUser != null && updateUserConnections == true) 
            {
                var ReplacementResourceId = McsHelper.getEntRefID("cvt_replacementuser");
                Logger.WriteGranularTimingMessage("Starting UpdateResourceConnections");
                UpdateUserConnections(PluginExecutionContext.PrimaryEntityId, ReplacementResourceId);
                Logger.WriteGranularTimingMessage("Ending UpdateResourceConnections");
            }
            if (deleteUserConnections == true)
            {
                Logger.WriteGranularTimingMessage("Starting DeleteResourceConnections");
                DeleteUserConnections(PluginExecutionContext.PrimaryEntityId);
                Logger.WriteGranularTimingMessage("Ending DeleteResourceConnections");
            }    
                    
            //disable user
            if (disableUserfromPriv == true)
                DisableUser(PluginExecutionContext.PrimaryEntityId);       

            if (thisUser.cvt_TimeZone != null)
                SyncTimeZones(thisUser);

            //If the workflow to save user roles is fired, then copy the roles into 
            if (saveUserRoles)
                PersistUserRoles(thisUser);

            //Transfer the security role if there are saved roles and if the BU is changing
            if (thisUser.BusinessUnitId != null)
            {
                CvtHelper.UpdateSecurityRoles(thisUser.Id, SystemUser.EntityLogicalName, OrganizationService, Logger, out int addedRoles, out int removedRoles);
            }
            //Assign to TCT Team
            Logger.WriteDebugMessage("Check if user is TCT.");
            ifTCTAssignToTeam(thisUser.Id, OrganizationService, Logger);

            //Update Primary Team
            CvtHelper.UpdatePrimaryTeam(thisUser.Id, OrganizationService, Logger);

            if (PrimaryEntity.Attributes.Contains("businessunitid"))
                UserCreatePostStageRunner.UpdateVISN(PrimaryEntity.Id, OrganizationService, Logger);
        }

        /// <summary>
        /// Saves the User's Security Roles into a string field called cvt_SecurityRolesString - to be used later in the UpdateSecurityRoles method which associates the new security roles for a user after their BU has changed
        /// </summary>
        /// <param name="thisUser">user who is moving BUs</param>
        internal void PersistUserRoles(SystemUser thisUser)
        {
            var rolesString = "";
            using (var srv = new Xrm(OrganizationService))
            {
                var userRoles = srv.SystemUserRolesSet.Where(ur => ur.SystemUserId == thisUser.Id);
                foreach (var userRole in userRoles)
                {
                    var role = srv.RoleSet.FirstOrDefault(r => r.Id == userRole.RoleId);
                    rolesString += role.Name + "|";
                }
                rolesString = rolesString.Length > 0 ? rolesString.Substring(0, rolesString.Length - 1) : rolesString;
            }
            var updateUser = new SystemUser()
            {
                Id = thisUser.Id,
                cvt_SecurityRolesString = rolesString
            };
            OrganizationService.Update(updateUser);
        }

        /// <summary>
        /// Get the timezone value from the personal options and sync up the user time zone field and the personal options time zone field
        /// </summary>
        /// <param name="user"></param>
        internal void SyncTimeZones(SystemUser user)
        {
            var options = new UserSettings() { Id = user.Id }
                ;
            using (var srv = new Xrm(OrganizationService))
            {
                var userOptions = srv.UserSettingsSet.FirstOrDefault(u => u.SystemUserId.Value == user.Id);
                if (userOptions.TimeZoneCode.Value != user.cvt_TimeZone.Value && user.cvt_TimeZone.Value != 35)//if the user timezone is not the default (EST), and not equal to the personal options - a user has tried to update it on the user record, so push the change to the personal options
                    options.TimeZoneCode = user.cvt_TimeZone;
            }
            if (options.TimeZoneCode != null) //Only set the personal option timezone if the TimeZoneCode on the user record does not match the time zone on the personal options (a.k.a. UserSettings)
                OrganizationService.Update(options);
        }

        internal void DisableUser(Guid thisId)
        {
            //Remvoves the value from the field
            SystemUser provUpdate = new SystemUser()
            {
                Id = thisId,
                cvt_disable = null
            };

            //Disable the provider's user record here
            SetStateRequest requestDisable = new SetStateRequest()
            {
                EntityMoniker = new EntityReference(SystemUser.EntityLogicalName, thisId),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(-1)
            };

            OrganizationService.Update(provUpdate);
            OrganizationService.Execute(requestDisable);
        }

        internal static void ifTCTAssignToTeam(Guid thisId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.setMethod = "ifTCTAssignToTeam";
            Logger.WriteDebugMessage("Starting method");
            using (var srv = new Xrm(OrganizationService))
            {
                var thisUser = srv.SystemUserSet.FirstOrDefault(su => su.Id == thisId);
                if (thisUser != null && thisUser.cvt_type != null)
                {
                    if (thisUser.cvt_type.Value == (int)SystemUsercvt_type.TCTStaff && thisUser.cvt_site != null)
                    {
                        Logger.WriteDebugMessage("User is TCT, attempting to assign to team.");
                        //Find TCT Team
                        var existingTeam = srv.TeamSet.FirstOrDefault(t => t.cvt_TMPSite.Id == thisUser.cvt_site.Id && t.cvt_Type != null && t.cvt_Type.Value == (int)Teamcvt_Type.Staff);
                        if (existingTeam != null)
                        {
                            //Add this User to that TCT Team
                            try
                            {
                                AddMembersTeamRequest req = new AddMembersTeamRequest()
                                {
                                    TeamId = existingTeam.Id,
                                    MemberIds = new Guid[1] { thisId }
                                };
                                OrganizationService.Execute(req);
                                Logger.WriteDebugMessage("Added individual Member to TCT Team for " + thisUser.cvt_site.Name);
                            }
                            catch (Exception ex2)
                            {
                                Logger.WriteToFile(String.Format("Failed to add individual member {0} : {1}", thisUser.FullName, ex2.Message));
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Update other records
        internal void UpdateUserConnections(Guid thisId, Guid ReplacementUserId)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    Logger.setMethod = "Update User Connections";
                   
                    var replacementUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == ReplacementUserId);
                    if (replacementUser == null) { return; }
                    Logger.WriteDebugMessage("Got Replacement");

                    var thisUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == thisId);

                    Logger.setMethod = "CreateSystemRecords";
                    //build the top half of the constraints xml
                    var builder = new System.Text.StringBuilder("<Constraints>");
                    builder.Append("<Constraint>");
                    builder.Append("<Expression>");
                    builder.Append("<Body>resource[\"Id\"] == ");
                    builder.Append(ReplacementUserId.ToString("B"));
                    builder.Append("</Body>");
                    builder.Append("<Parameters>");
                    builder.Append("<Parameter name=\"resource\" />");
                    builder.Append("</Parameters>");
                    builder.Append("</Expression>");
                    builder.Append("</Constraint>");
                    builder.Append("</Constraints>");
                    // Define an anonymous type to define the possible constraint based group type code values.
                    var constraintBasedGroupTypeCode = new
                    {
                        Static = 0,
                        Dynamic = 1,
                        Implicit = 2
                    };
                    //we need the user for the business unit
                    Logger.WriteDebugMessage("About to get User");
                    var systemUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == PluginExecutionContext.InitiatingUserId);

                    if (systemUser == null) return;
                    Logger.WriteDebugMessage("got user");

                    var group = new ConstraintBasedGroup
                    {
                        BusinessUnitId = systemUser.BusinessUnitId,
                        Constraints = builder.ToString(),
                        Name = "Selection Rule:" + McsHelper.getStringValue("cvt_name"),
                        GroupTypeCode = new OptionSetValue(constraintBasedGroupTypeCode.Implicit)
                    };

                    var newSysResource = OrganizationService.Create(group);
                    
                    //now create the resource spec record
                    var spec = new ResourceSpec
                    {
                        BusinessUnitId = systemUser.BusinessUnitId,
                        ObjectiveExpression = @"<Expression><Body>udf ""Random""(factory,resource,appointment,request,leftoffset,rightoffset)</Body><Parameters><Parameter name=""factory"" /><Parameter name=""resource"" /><Parameter name=""appointment"" /><Parameter name=""request"" /><Parameter name=""leftoffset"" /><Parameter name=""rightoffset"" /></Parameters><Properties EvaluationInterval=""P0D"" evaluationcost=""0"" /></Expression>",
                        RequiredCount = 1,
                        Name = "Selection Rule:" + McsHelper.getStringValue("cvt_name"),
                        GroupObjectId = newSysResource,
                        SameSite = true
                    };
                    var _specId = OrganizationService.Create(spec);

                    //Going through Pat Site Resources that need to be updated. 
                    var getSchResources = from schResources in srv.cvt_schedulingresourceSet
                                          where schResources.cvt_user.Id == thisId
                                          where schResources.statecode == 0
                                          select new
                                          {
                                              schResources.Id
                                              //patGroup.cvt_RelatedTSAid
                                          };
                    foreach (var sr in getSchResources)
                    {
                        var updateSR = new Entity("cvt_schedulingresource") { Id = sr.Id };
                        updateSR["cvt_relateduserid"] = new EntityReference("systemuser", ReplacementUserId);
                        updateSR["cvt_name"] = replacementUser.FullName;
                        if (replacementUser.cvt_site == null)
                            throw new InvalidPluginExecutionException("Replacement User has no TMP Site selected:.");
                        updateSR["cvt_relatedsiteid"] = new EntityReference("mcs_site", replacementUser.cvt_site.Id);
                        updateSR["cvt_resourcespecguid"] = _specId.ToString();
                        updateSR["cvt_constraintgroupguid"] = newSysResource.ToString();                     
                        OrganizationService.Update(updateSR);
                        Logger.WriteDebugMessage("Scheduling Resource Updated");

                        //if (sr.cvt_RelatedTSAid != null)
                        //    updateTSAfields(sr.cvt_RelatedTSAid.Id);
                    }

                    //Going through Group Resources that need to be updated
                    var getGroupResources = from groupResource in srv.mcs_groupresourceSet
                                            where groupResource.mcs_RelatedUserId.Id == thisId
                                            where groupResource.statecode == 0
                                           select new
                                           {
                                               groupResource.Id,
                                               groupResource.mcs_relatedResourceGroupId
                                           };
                    foreach (var groupResource in getGroupResources)
                    {                                                                       
                        var updateGroupResource = new Entity("mcs_groupresource") { Id = groupResource.Id };
                        var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == groupResource.mcs_relatedResourceGroupId.Id);
                        updateGroupResource["mcs_relateduserid"] = new EntityReference("systemuser", ReplacementUserId);
                        updateGroupResource["mcs_name"] = replacementUser.FullName;
                        if (replacementUser.cvt_site == null)
                            throw new InvalidPluginExecutionException("Replacement User has no TSS Site selected: group resource");
                        updateGroupResource["mcs_relatedsiteid"] = new EntityReference("mcs_site", replacementUser.cvt_site.Id);                                         
                        OrganizationService.Update(updateGroupResource);
                        Logger.WriteDebugMessage("Group Resource Updated");

                        //Now we need to update the Resource Group
                        UpdateRG(relatedResourceGroup.Id);
                        Logger.WriteDebugMessage("Resource Group Updated");

                        ////now we need to look for Pat / Pro Site Resources related to this Resource Group. And update the TSA's related to them. 
                        ////Going through Pat Site Resources that need to be deleted. 
                        //var getRGSchResources = from RGschRes in srv.cvt_schedulingresourceSet
                        //                        where RGschRes.cvt_tmpresourcegroup.Id == relatedResourceGroup.Id
                        //                        where RGschRes.statecode == 0
                        //                        select new
                        //                        {
                        //                            RGschRes.Id
                        //                            //RGpatGroup.cvt_RelatedTSAid
                        //                        };
                        //foreach (var RGpatGroup in getRGSchResources)
                        //{
                        //    if (RGpatGroup.cvt_RelatedTSAid != null)
                        //        updateTSAfields(RGpatGroup.cvt_RelatedTSAid.Id);
                        //}
                    }
                    //After we have made all the updates to Pat/Pro Site Resources, Group Resources / Resource Groups, and TSA's, we will set this User's "update connnections" field back to No so that it can be triggered again if necessary. 
                    var updateUser = new Entity("systemuser") { Id = thisId };
                    updateUser["cvt_updateuserconnections"] = false;
                    OrganizationService.Update(updateUser);
                    Logger.WriteDebugMessage("cvt_updateuserconnections Field updated");
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
        }

        internal void DeleteUserConnections(Guid thisId)
        {
            Logger.setMethod = "Delete User Connections";
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    var thisUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == thisId);

                    //Going through Pat Site Resources that need to be deleted. 
                    var getSchResources = from schResource in srv.cvt_schedulingresourceSet
                                          where schResource.cvt_user.Id == thisId
                                          where schResource.statecode == 0
                                          select new
                                          {
                                              schResource.Id
                                              //patGroup.cvt_RelatedTSAid
                                          };
                    foreach (var sr in getSchResources)
                    {
                        var deletePatGroup = new Entity("cvt_schedulingresource") { Id = sr.Id };
                        OrganizationService.Delete(deletePatGroup.LogicalName, sr.Id);
                        Logger.WriteDebugMessage("Scheduling Resource Deleted");

                        //if (patGroup.cvt_RelatedTSAid != null)
                        //    updateTSAfields(patGroup.cvt_RelatedTSAid.Id);                     
                    }

                    //Going through Group Resources that need to be deleted
                    var getGroupResources = from groupResource in srv.mcs_groupresourceSet
                                            where groupResource.mcs_RelatedUserId.Id == thisId
                                            where groupResource.statecode == 0
                                            select new
                                            {
                                                groupResource.Id,
                                                groupResource.mcs_relatedResourceGroupId

                                            };
                    foreach (var groupResource in getGroupResources)
                    {
                        var deleteGroupResource = new Entity("mcs_groupresource") { Id = groupResource.Id };
                        var relatedResourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == groupResource.mcs_relatedResourceGroupId.Id);
                        OrganizationService.Delete(deleteGroupResource.LogicalName, groupResource.Id);
                        Logger.WriteDebugMessage("Group Resource Deleted");

                        //Now we need to update the Resource Group
                        UpdateRG(relatedResourceGroup.Id);
                        Logger.WriteDebugMessage("Resource Group Updated");

                        //now we need to look for Pat / Pro Site Resources related to this Resource Group. And update the TSA's related to them. 
                        //Going through Pat Site Resources that need to be deleted. 
                        //var getRGPatResources = from RGpatGroup in srv.cvt_patientresourcegroupSet
                        //                      where RGpatGroup.cvt_RelatedResourceGroupid.Id == relatedResourceGroup.Id
                        //                      where RGpatGroup.statecode == 0
                        //                      select new
                        //                      {
                        //                          RGpatGroup.Id,
                        //                          RGpatGroup.cvt_RelatedTSAid
                        //                      };
                        //foreach (var RGpatGroup in getRGPatResources)
                        //{
                        //    if (RGpatGroup.cvt_RelatedTSAid != null)
                        //        updateTSAfields(RGpatGroup.cvt_RelatedTSAid.Id);
                        //}
                    }

                    //After we have removed the connections to Pat/Pro Site Resources, Group Resources / Resource Groups, and TSA's, we will set this User's "delete connnections" field back to No so that it can be triggered again if necessary. 
                    var updateUser = new Entity("systemuser") { Id = thisId };
                    updateUser["cvt_deleteuserconnections"] = false;
                    OrganizationService.Update(updateUser);
                    Logger.WriteDebugMessage("cvt_deleteduserconnectionse Field updated");
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
        }

        internal void UpdateRG(Guid thisId)
        {
            Logger.setMethod = "Update System Group Resource";
            Logger.WriteDebugMessage("starting UpdateResourceGroup");
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    var RG = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == thisId);
                    var resourcesString = String.Empty;
                    var count = 0;
                    var resources = CvtHelper.GetResources(thisId, srv, out resourcesString, out count);

                    var builder = CvtHelper.BuildConstraintsXML(resources);

                    var group = new ConstraintBasedGroup
                    {
                        Id = RG.mcs_RelatedResourceGroupId.Id,
                        Constraints = builder.ToString(),
                        Name = RG.mcs_name
                    };

                    OrganizationService.Update(group);
                    Logger.WriteDebugMessage("System Resource Updated with " + count + " resources");
                    var resGroup = new mcs_resourcegroup() { Id = RG.Id, cvt_resources = resourcesString };
                    OrganizationService.Update(resGroup);
                    Logger.WriteDebugMessage("Updated Resource Group String");
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
        }

        internal void updateTSAfields(Guid tsaGuid)
        {
            CvtHelper.CreateUpdateService(tsaGuid, Logger, OrganizationService, McsSettings);
        }

        internal void updateMTSAfields(Guid mtsaGuid)
        {
            CvtHelper.UpdateMTSA(mtsaGuid, Guid.Empty, Logger, OrganizationService);
        }

        internal bool CheckSystemUserUpdatesForService(SystemUser preImage, SystemUser postImage,string fieldName)
        {
            Logger.WriteDebugMessage("Beginning CheckUserUpdatesForService for '" + fieldName + "'");
            bool doUpdate = false;
            EntityReference preVC = new EntityReference("mcs_resource", Guid.Empty);
            EntityReference postVC = new EntityReference("mcs_resource", Guid.Empty);
            Guid preVCId = Guid.Empty;
            Guid postVCId = Guid.Empty;
            bool isDisabledPre = false;
            bool isDisabled = false;

            Logger.WriteDebugMessage("Checking for Enabled User");
            if (preImage.Contains("isdisabled"))
            {
                var accessor = preImage["isdisabled"];
                isDisabledPre = (accessor.ToString() == "True") ? true : false;
            }
            Logger.WriteDebugMessage("Checked for Enabled User - Pre");

            if (postImage.Contains("isdisabled"))
            {
                var accessor = postImage["isdisabled"];
                isDisabled = (accessor.ToString() == "True") ? true : false;

            }
            Logger.WriteDebugMessage("Checked for Enabled User - Post");
            
            if (preImage.Contains(fieldName) && (preImage[fieldName]!=null))
            {
                preVC = (EntityReference)preImage[fieldName];
                preVCId = preVC.Id;
            }
            Logger.WriteDebugMessage("Checked for '" + fieldName + "' Id value - Pre");

            if (postImage.Contains(fieldName)&&(postImage[fieldName]!=null))
            {
                postVC = (EntityReference)postImage[fieldName];
                postVCId = postVC.Id;
            }
            Logger.WriteDebugMessage("Checked for '" + fieldName + "' Id value - Post");

            //we want to call the service if:
            //A - the status of the user has changed, or
            //B - if the clinic has changed on an active user

            Logger.WriteDebugMessage("Evaluate doUpdate");
            if (isDisabledPre != isDisabled) //status of the user has changed
            {
                doUpdate = true;
            }
            else if (!isDisabled) //we have an enabled provider, but
            {
                if (preVCId != postVCId) //the XXXX  Clinic has changed 
                {
                    doUpdate = true;
                }
            }
            Logger.WriteDebugMessage("doUpdate evaluation complete.");


            // Check the statu

            return doUpdate;
            
        }

        #endregion

        #region Additional Interface Settings
        public override string McsSettingsDebugField
        {
            get { return "mcs_userplugin"; }
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