using MCS.ApplicationInsights;
using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentCreatePostStageRunner : AILogicBase
    {
        #region Constructor
        public ServiceAppointmentCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {}

        #endregion

        #region Execute
        public override void ExecuteLogic()
        {
            if (PrimaryEntity.LogicalName != ServiceAppointment.EntityLogicalName)
                throw new Exception("Target entity is not of type ServiceAppointment");
            RunGroupSpecific(PrimaryEntity.Id);
        }

        #endregion

        #region Group specific
        public void RunGroupSpecific(Guid saId) 
        {
            //Logger.setMethod = "RunGroupSpecific";
            //Logger.WriteDebugMessage("starting Method");
            Trace("starting Method", LogLevel.Debug);
            using (var srv = new Xrm(OrganizationService))
            {
                var sa = srv.ServiceAppointmentSet.FirstOrDefault(s => s.Id == saId);
                if (sa != null)
                {
                    if (sa.mcs_groupappointment != null)
                    {
                        if (sa.mcs_groupappointment.Value && !sa.cvt_Type.Value)
                        {
                            //Logger.WriteDebugMessage("Setting SA Permissions");
                            Trace("Setting SA Permissions", LogLevel.Debug);
                            //Run SA Permissions function here if the otehr group logic is running, otherwise run it in the Integration Plugin
                            CvtHelper.SetServiceAppointmentPermissions(OrganizationService, sa, pluginLogger);
                            //Logger.WriteDebugMessage("Service Activity is Clinic-Based Group, so running RescheduleGroup and SetupAppointments.");
                            Trace("Service Activity is Clinic-Based Group, so running RescheduleGroup and SetupAppointments.", LogLevel.Debug);
                            //Group
                            RescheduleGroup(sa);
                            SetupAppointments(sa);
                        }
                        else
                        {
                            //Logger.WriteDebugMessage(string.Format("SA is not a group ({0}) or is VA Video Connect ({1})", sa.mcs_groupappointment.Value, sa.cvt_Type.Value));
                            Trace($"SA is not a group ({sa.mcs_groupappointment.Value}) or is VA Video Connect ({sa.cvt_Type.Value})", LogLevel.Debug);
                        }
                    }
                    else
                    {
                        //Logger.WriteDebugMessage("Group is null");
                        Trace("Group is null", LogLevel.Debug);
                    }
                }
                else
                {
                    //Logger.WriteDebugMessage("No SA Found");
                    Trace("No SA Found", LogLevel.Debug);
                }
            }
            //Logger.WriteDebugMessage("finished Method");
            Trace("finished Method", LogLevel.Debug);
        }

        /// <summary>
        /// This method looks at the service activity and reschedules it if it is a group such that only the provider site resources are booked on the service activity.  The patient paired groups get pushed onto child appointments so that they can be individually booked or cancelled without cancelling the entire meeting
        /// </summary>
        /// <param name="saId">The ID of the service activity that is to be rescheduled</param>
        public void RescheduleGroup(ServiceAppointment sa)
        {
            //Logger.WriteDebugMessage("Attempting Reschedule");
            Trace("Attempting Reschedule", LogLevel.Debug);
            using (var srv = new Xrm(OrganizationService))
            {
                if (sa.ServiceId == null)
                {
                    //Logger.WriteDebugMessage("Missing service on ServiceAppointment.");
                    Trace("Missing service on ServiceAppointment.", LogLevel.Debug);
                    return;
                }
                var serviceAppointmentService = srv.ServiceSet.FirstOrDefault(s => s.Id == sa.ServiceId.Id);
                if (serviceAppointmentService == null)
                {
                    //Logger.WriteDebugMessage("Could not find service record.");
                    Trace("Could not find service record.", LogLevel.Debug);
                    return;
                }
                //get provider SITE from the SA
                if (sa.mcs_relatedprovidersite == null)
                {
                    //Logger.WriteDebugMessage("Missing Provider Site on ServiceAppointment.");
                    Trace("Missing Provider Site on ServiceAppointment.", LogLevel.Debug);
                    return;
                }
                mcs_site proSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == sa.mcs_relatedprovidersite.Id);

                if (proSite == null)
                {
                    //Logger.WriteDebugMessage("Could not find provider site record.");
                    Trace("Could not find provider site record.", LogLevel.Debug);
                    return;
                }

                //Logger.WriteDebugMessage("Getting the Provider participating site.");
                Trace("Getting the Provider participating site.", LogLevel.Debug);
                cvt_participatingsite proPS = srv.cvt_participatingsiteSet.FirstOrDefault(p => p.cvt_site.Id == proSite.Id && p.cvt_resourcepackage.Id==sa.cvt_relatedschedulingpackage.Id && p.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider);

                if (proPS == null)
                {
                    //Logger.WriteDebugMessage("Could not find provider participating site record.");
                    Trace("Could not find provider participating site record.", LogLevel.Debug);
                    return;
                }

                int matched = 0;
                List<ActivityParty> originalResourceList = sa.Resources.ToList();
                //Logger.WriteDebugMessage("Number of originally scheduled resources = " + originalResourceList.Count.ToString());
                Trace($"Number of originally scheduled resources = {originalResourceList.Count}.", LogLevel.Debug);

                //Check that the Service Activity has the resource listed in the patient site ID field (from the provider participating site) - if that resource cant be found, then the resource list is correct and rescheduling is not needed
                if (proPS.cvt_grouppatientbranch == null || proPS.cvt_grouppatientbranch == "")
                {
                    //Logger.WriteDebugMessage("Missing Group Patient Branch id for reschedule.");
                    Trace("Missing Group Patient Branch id for reschedule.", LogLevel.Debug);
                    return;
                }

                var hasBothPatAndProvResources = false;
                foreach (var oldResource in originalResourceList)
                {
                    if (oldResource.PartyId != null)
                    {
                        if (proPS.cvt_grouppatientbranch.Contains(oldResource.PartyId.Id.ToString()))
                        {
                            hasBothPatAndProvResources = true;
                            //Logger.WriteDebugMessage("The Existing Resource List has a Patient Site Resource, this ServiceAppointment needs to be rescheduled to only have Provider Resources.");
                            Trace("The Existing Resource List has a Patient Site Resource, this ServiceAppointment needs to be rescheduled to only have Provider Resources.", LogLevel.Debug);
                            break;
                        }
                    }
                }
                if (!hasBothPatAndProvResources)
                {
                    //Logger.WriteDebugMessage("The Existing Resource List does not have any patient site resources, so skip rescheduling the patient site resources into appts.");
                    Trace("The Existing Resource List does not have any patient site resources, so skip rescheduling the patient site resources into appts.", LogLevel.Debug);
                    return; //If it doesnt have pat resources, no need to reschedule this to be prov only resources (this should be the case for recurring SAs since this function was run on the initial SA)
                }

                //Logger.WriteDebugMessage("Start assessing the service.");
                Trace("Start assessing the service.", LogLevel.Debug);
                //get Root ResourceSpec Choose 1
                var level1ResourceSpecs = returnResourceIdsFromRS(serviceAppointmentService.ResourceSpecId.Id);

                //Just split into both possible branches - ProvOnly, Pat/Prov
                if (level1ResourceSpecs.Count() == 2)
                {
                    //Logger.WriteDebugMessage("Should be 2. Count = " + level1ResourceSpecs.Count() + ". Specs are: " + level1ResourceSpecs[0] + ";" + level1ResourceSpecs[1]);
                    Trace($"Should be 2. Count = {level1ResourceSpecs.Count()}. Specs are: {level1ResourceSpecs[0]}; {level1ResourceSpecs[1]}.", LogLevel.Debug);
                    var branch0 = returnResourceIdsFromRS(level1ResourceSpecs[0]);
                    var branch1 = returnResourceIdsFromRS(level1ResourceSpecs[1]);

                    int ProvOnlyBranch = (branch0.Count < branch1.Count) ? 0 : 1;
                    //Attempting to find the shorter branch, which should indicate it is the ProvOnlyBranch
                    //Logger.WriteDebugMessage($"Branch0: {branch0.Count}. Branch1: {branch1.Count}.");
                    Trace($"Branch0: {branch0.Count}. Branch1: {branch1.Count}.", LogLevel.Debug);
                    //Logger.WriteDebugMessage("The smaller branch is object : " + ProvOnlyBranch + ". About to split out the provBranch's CBG's resourceSpecIds.");
                    Trace($"The smaller branch is object: {ProvOnlyBranch}. About to split out the provBranch's CBG's resourceSpecIds.", LogLevel.Debug);

                    /*level2ResourceIds gives us: (list of ResourceGuids)
                     * 1) actual individual equipment/users. 
                     * 2) Choose 1 of non AR ResGroup [need 1 level]. 
                     * 3) Choose 1 of Choose All AR group(s) [need two levels] */
                    var level2ResourceIds = ProvOnlyBranch == 0 ? branch0 : branch1;

                    //Create a Activity Party with same objects, but other constraint groups
                    List<ActivityParty> provOnlyBranchAPL = new List<ActivityParty>();

                    //Staging Level1 ResourceSpec EntityReference
                    //This will be used for Individual Equipment/Users found on Level 2.
                    EntityReference Level1ResSpec = new EntityReference()
                    {
                        Id = level1ResourceSpecs[ProvOnlyBranch],
                        LogicalName = ResourceSpec.EntityLogicalName
                    };

                    //Looping through all of the nodes within the ProvOnly Branch
                    //Logger.WriteDebugMessage($"Count for Guid List for the ProvBranch: {level2ResourceIds.Count()}");
                    Trace($"Count for Guid List for the ProvBranch: {level2ResourceIds.Count()}.", LogLevel.Debug);
                    foreach (var itemResource in level2ResourceIds)
                    {
                        //itemResource is either the Indivdual Resource or a ResSpec of a Group
                        //Logger.WriteDebugMessage("Item: " + itemResource.ToString("B"));
                        Trace($"Item: {itemResource.ToString("B")}.", LogLevel.Debug);
                        var returnedAP = returnActivityPartytoAddfromResource(itemResource, originalResourceList, Level1ResSpec);
                        //This is a Resource, otherwise it will return null
                        if (returnedAP != null && returnedAP.PartyId != null)
                        {
                            provOnlyBranchAPL.Add(returnedAP);
                            matched += 1;
                            //Logger.WriteDebugMessage("Populated ProvOnlyBranchAPL with Individual");
                            Trace("Populated ProvOnlyBranchAPL with Individual.", LogLevel.Debug);
                        }
                        else
                        {
                            //Logger.WriteDebugMessage("This is not a resource.");
                            Trace("This is not a resource.", LogLevel.Debug);
                            /*This item is not a Resource, it is a ResourceSpec
                            * Need to figure out if this ID is:
                            * 1) non Paired ResourceGroup
                            * 2) another ResourceSpec, which would mean it is an Paired ResourceGroup [need to go another level]                             
                            */
                            EntityReference Level2ResSpec = new EntityReference()
                            {
                                Id = itemResource,
                                LogicalName = ResourceSpec.EntityLogicalName
                            };

                            var resGroup = srv.ResourceSpecSet.FirstOrDefault(rg => rg.Id == itemResource);
                            if (resGroup != null) //Resource Group (either AR or Ind)
                            {
                                var getIndGroupIdsAndRootARResSpec = returnResourceIdsFromRS(itemResource);
                                //Logger.WriteDebugMessage($"getIndGroupIdsAndRootARResSpec Count: {getIndGroupIdsAndRootARResSpec.Count()}");
                                Trace($"getIndGroupIdsAndRootARResSpec Count: {getIndGroupIdsAndRootARResSpec.Count()}", LogLevel.Debug);
                                foreach (var member in getIndGroupIdsAndRootARResSpec)
                                {
                                    var level2returnedAP = returnActivityPartytoAddfromResource(member, originalResourceList, Level2ResSpec);
                                    //This is a Resource, otherwise it will return null
                                    if (level2returnedAP != null && level2returnedAP.PartyId != null)
                                    {
                                        provOnlyBranchAPL.Add(level2returnedAP);
                                        matched += 1;
                                        //Logger.WriteDebugMessage("Populated ProvOnlyBranchAPL with Individual");
                                        Trace("Populated ProvOnlyBranchAPL with Individual.", LogLevel.Debug);
                                    }
                                    else
                                    {


                                        EntityReference Level3ResSpec = new EntityReference()
                                        {
                                            Id = member,
                                            LogicalName = ResourceSpec.EntityLogicalName
                                        };

                                        var Level4ResSpec = srv.ResourceSpecSet.FirstOrDefault(trgar => trgar.Id == member);
                                        if (Level4ResSpec == null) //If this is a group and does not return a ResourceSpec, it is an individual Group
                                        {
                                            //Logger.WriteDebugMessage("Did not find ResourceSpec, so this is an individual Group: " + member);
                                            Trace($"Did not find ResourceSpec, so this is an individual Group: {member}.", LogLevel.Debug);
                                            //ResourceSpec doesnt exist, so the "member" aligns with the ConstraintBasedGroup, so call that function directly
                                            var indGroups = returnResourceIdsFromCBG(member);
                                            //Logger.WriteDebugMessage("Number of members of the group: " + indGroups.Count());
                                            Trace($"Number of members of the group: {indGroups.Count()} ", LogLevel.Debug);
                                            foreach (var indMember in indGroups)
                                            {
                                                //Logger.WriteDebugMessage(String.Format("Looping through members of group {0} -- {1}", indGroups, indMember));
                                                Trace($"Looping through members of group {indGroups} -- {indMember}.", LogLevel.Debug);
                                                var returnedIndAPgroup = returnActivityPartytoAddfromResource(indMember, originalResourceList, Level2ResSpec);
                                                if (returnedIndAPgroup != null && returnedIndAPgroup.PartyId != null)
                                                {
                                                    provOnlyBranchAPL.Add(returnedIndAPgroup);
                                                    matched += 1;
                                                    //Logger.WriteDebugMessage("Populated ProvOnlyBranchAPL with non-AR Resource Group");
                                                    Trace("Populated ProvOnlyBranchAPL with non-AR Resource Group.", LogLevel.Debug);
                                                }
                                            }
                                        }
                                        else //ResourceSpec was found, so we need to dig one more level down to get the AR Group specified in each Choose All
                                        {
                                            //Logger.WriteDebugMessage("Found ResourceSpec, so this is a Paired Group: " + member);
                                            Trace($"Found ResourceSpec, so this is a Paired Group: {member} ", LogLevel.Debug);
                                            var ResGroupAR = returnResourceIdsFromRS(member); //Should be one Choose All
                                            //Logger.WriteDebugMessage("ResGroupAR count should be 1, since it is the choose all of the ResGr. Count= " + ResGroupAR.Count);
                                            Trace($"ResGroupAR count should be 1, since it is the choose all of the ResGr. Count={ResGroupAR.Count}.", LogLevel.Debug);
                                            if (ResGroupAR.Count > 0)
                                            {
                                                //Logger.WriteDebugMessage(ResGroupAR[0].ToString());
                                                Trace(ResGroupAR[0].ToString(), LogLevel.Debug);
                                                var Level5ResSpec = srv.ResourceSpecSet.FirstOrDefault(r => r.GroupObjectId.Value == ResGroupAR[0]);
                                                var membersOfResGroupAR = returnResourceIdsFromRS(Level5ResSpec.Id);
                                                //Logger.WriteDebugMessage("Count should equal the number of resources within the ResGroup. Count= " + membersOfResGroupAR.Count);
                                                Trace($"Count should equal the number of resources within the ResGroup. Count={membersOfResGroupAR.Count}.", LogLevel.Debug);
                                                foreach (var m in membersOfResGroupAR)
                                                {
                                                    var returnedAPgroupAR = returnActivityPartytoAddfromResource(m, originalResourceList, Level3ResSpec, ResGroupAR[0]);
                                                    if (returnedAPgroupAR != null && returnedAPgroupAR.PartyId != null)
                                                    {
                                                        provOnlyBranchAPL.Add(returnedAPgroupAR);
                                                        matched += 1;
                                                        //Logger.WriteDebugMessage("Populated ProvOnlyBranchAPL with AR Resource Group");
                                                        Trace("Populated ProvOnlyBranchAPL with AR Resource Group.", LogLevel.Debug);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //Logger.WriteDebugMessage("Finished matching objects. Matched: " + matched);
                    Trace($"Finished matching objects. Matched: {matched}.", LogLevel.Debug);

                    if (matched > 0)
                    {
                        //Logger.WriteDebugMessage("Creating the new servAppt obj");
                        Trace("Creating the new servAppt obj.", LogLevel.Debug);
                        //set Resources = new Party List
                        var servAppt = new ServiceAppointment()
                        {
                            Id = sa.Id,
                            Resources = new List<ActivityParty>()
                        };

                        //Logger.WriteDebugMessage("Update to clear our Resources");
                        Trace("Update to clear our Resources.", LogLevel.Debug);
                        OrganizationService.Update(servAppt);

                        //Logger.WriteDebugMessage("Actually Adding the Prov Resources.");
                        Trace("Actually Adding the Prov Resources.", LogLevel.Debug);
                        servAppt.Resources = provOnlyBranchAPL;

                        foreach (var res in servAppt.Resources)
                        {
                            //Logger.WriteDebugMessage("PartyId " + res.PartyId.Id.ToString() + "; PartyLogicalName = " + res.PartyId.LogicalName + "; RSid = " + res.ResourceSpecId.Id.ToString());
                            Trace($"PartyId {res.PartyId.Id.ToString()}; PartyLogicalName = {res.PartyId.LogicalName}; RSid = {res.ResourceSpecId.Id}.", LogLevel.Debug);
                        }

                        RescheduleRequest rr = new RescheduleRequest()
                        {
                            Target = servAppt
                        };
                        //Logger.WriteDebugMessage("Executing Reschedule request");
                        Trace("Executing Reschedule request.", LogLevel.Debug);
                        RescheduleResponse resp = (RescheduleResponse)OrganizationService.Execute(rr);
                        //Logger.WriteDebugMessage("Reschedule Request completed");
                        Trace("Reschedule Request completed.", LogLevel.Debug);
                        if (resp.ValidationResult.ValidationSuccess)
                        {
                            //Logger.WriteDebugMessage("Re-added Provider Site Resources to Service Activity \"Resources\"");
                            Trace("Re-added Provider Site Resources to Service Activity \"Resources\".", LogLevel.Debug);
                        }
                        else
                        {   //"ErrorCode.RequiredResourceMisMatch"
                            //Logger.WriteDebugMessage("Failed to Re-Book Service Activity with only Provider Site Resources");
                            Trace("Failed to Re-Book Service Activity with only Provider Site Resources.", LogLevel.Debug);
                            string errorstring = "";
                            foreach (var error in resp.ValidationResult.TraceInfo.ErrorInfoList)
                            {
                                errorstring += error.ErrorCode.ToString() + " || ";
                            }
                            //Logger.WriteDebugMessage(errorstring);
                            Trace(errorstring, LogLevel.Debug);
                        }
                    }
                    else
                    {
                        //Logger.WriteDebugMessage("Didn't match any SA.resources with the ProvOnly branch.  No RescheduleRequest attempt.");
                        Trace("Didn't match any SA.resources with the ProvOnly branch.  No RescheduleRequest attempt.", LogLevel.Debug);
                    }
                }
                else
                {
                    //Logger.WriteDebugMessage("Group Service is not set up correctly, there should be 2 branches and " + level1ResourceSpecs.Count() + " were found.  Please Reset this TSA to draft and then into Production again.");
                    Trace($"Group Service is not set up correctly, there should be 2 branches and {level1ResourceSpecs.Count()} were found.  Please Reset this TSA to draft and then into Production again.", LogLevel.Debug);
                }
            }
        }

        /// <summary>
        /// Read TSA listed on Service Activity, get the Paired groups, then set up an appointment for each one
        /// </summary>
        /// <param name="SA"></param>
        /// <returns>List of activity parties that correspond to the patient site resources</returns>
        internal void SetupAppointments(ServiceAppointment SA)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                //Logger.WriteToFile("Needs to be refactored for SP, this will break.");
                var proPS = srv.cvt_participatingsiteSet.FirstOrDefault(pro => pro.cvt_resourcepackage.Id == SA.cvt_relatedschedulingpackage.Id && pro.cvt_site.Id == SA.mcs_relatedprovidersite.Id && pro.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider);

                if (proPS == null)
                {
                    //Logger.WriteToFile("No Provider Site found, exiting SetupAppointments function.");
                    Trace("No Provider Site found, exiting SetupAppointments function.", LogLevel.Debug);
                    return;
                }

                var pats = (from resGr in srv.mcs_resourcegroupSet join schRes in srv.cvt_schedulingresourceSet on resGr.Id equals schRes.cvt_tmpresourcegroup.Id
                            join ps in srv.cvt_participatingsiteSet on schRes.cvt_participatingsite.Id equals ps.Id
                            where ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient
                            && resGr.mcs_Type.Value == (int)mcs_resourcetype.PairedResourceGroup

                            && ps.cvt_resourcepackage.Id == SA.cvt_relatedschedulingpackage.Id
                            && ps.cvt_scheduleable.Value == true
                            select resGr).ToList();

                //Logger.WriteDebugMessage("Number of patient paired resources found: " + pats.Count);
                Trace($"Number of patient paired resources found: {pats.Count}.", LogLevel.Debug);
                foreach (var pat in pats)
                {
                    //Logger.WriteDebugMessage(String.Format("Setting up appointment for {0}.", pat.mcs_name));
                    Trace($"Setting up appointment for {pat.mcs_name}.", LogLevel.Debug);
                    GenerateGroupAppointment(pat, SA);
                }
            }
        }

        /// <summary>
        /// accepts the Service Activity and 1 group as parameters.  Called 1 time per patient side resource group.  Creates an Appointment record for the group.
        /// </summary>
        /// <param name="group">The resource group (Patient Paired Group from TSA) that will be parsed and added to the Appointment</param>
        /// <param name="SA">Service Activity that will be the parent of the Appointment</param>
        /// <remarks>Appointment has to be saved in Open:Requested Status, and then State Change is initiated to get it to Scheduled Status</remarks>
        internal void GenerateGroupAppointment(mcs_resourcegroup group, ServiceAppointment SA)
        {
            var participants = new List<ActivityParty>();
            var site = group.mcs_relatedSiteId;
            using (var context = new Xrm(OrganizationService))
            {
                var GroupResources = context.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == group.Id);
                foreach (var gr in GroupResources)
                {
                    var participant = new Entity();
                    if (gr.mcs_RelatedResourceId != null)
                        participant = CvtHelper.getPartyParticipantFromResource(gr.mcs_RelatedResourceId.Id, context);
                    else if (gr.mcs_RelatedUserId != null)
                        participant = CvtHelper.getPartyParticipantFromResource(gr.mcs_RelatedUserId.Id, context);
                    if (participant != null)
                    {
                        var party = new ActivityParty()
                        {
                            PartyId = new EntityReference(participant.LogicalName, participant.Id),
                            ParticipationTypeMask = new OptionSetValue(5)
                        };
                        participants.Add(party);
                    }
                    else
                    {
                        //Logger.WriteToFile(String.Format("When creating Group Appt, no participant found for Group Resource: {0}, Resource Group: {1}, Service Activity ID: {2}", gr.mcs_name, group.mcs_name, SA.Id));
                        Trace($"When creating Group Appt, no participant found for Group Resource: {gr.mcs_name}, Resource Group: {group.mcs_name}, Service Activity ID: {SA.Id}.", LogLevel.Debug);
                    }
                }

                //Logger.WriteDebugMessage("About to look for Patient VC to add.");
                Trace("About to look for Patient VC to add.", LogLevel.Debug);
                //Add the Patient Side VC to the participants list.

                if (SA.cvt_relatedproviderid != null)
                {
                    var patVC = context.mcs_resourceSet.FirstOrDefault(vc => vc.cvt_primarystopcode == "690" && vc.mcs_RelatedSiteId.Id == group.mcs_relatedSiteId.Id && vc.cvt_defaultprovider.Id == SA.cvt_relatedproviderid.Id && vc.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic);
                    if (patVC != null)
                    {
                        var participant = new Entity();
                        participant = CvtHelper.getPartyParticipantFromResource(patVC.Id, context);

                        if (participant != null)
                        {
                            var party = new ActivityParty()
                            {
                                PartyId = new EntityReference(participant.LogicalName, participant.Id),
                                ParticipationTypeMask = new OptionSetValue(5)
                            };
                            //Logger.WriteDebugMessage("Adding Patient VC participants list.");
                            Trace("Adding Patient VC participants list.", LogLevel.Debug);
                            participants.Add(party);
                        }
                        else
                        {
                            //Logger.WriteToFile(String.Format("When creating Group Appt, not able to automatically add Pat VC."));
                            Trace("When creating Group Appt, not able to automatically add Pat VC.", LogLevel.Debug);
                        }
                    }
                    else
                    {
                        //Logger.WriteToFile(String.Format("When creating Group Appt, no Pat VC found."));
                        Trace("When creating Group Appt, no Pat VC found.", LogLevel.Debug);
                    }
                }
                else
                {
                    //Logger.WriteToFile(String.Format("Group Appt: no Provider listed on Appointment form, could not look for Patient VC."));
                    Trace("Group Appt: no Provider listed on Appointment form, could not look for Patient VC.", LogLevel.Debug);
                }

            }
            var appt = new Appointment()
            {
                ScheduledStart = SA.ScheduledStart,
                ScheduledEnd = SA.ScheduledEnd,
                ScheduledDurationMinutes = SA.ScheduledDurationMinutes,
                cvt_serviceactivityid = new EntityReference(ServiceAppointment.EntityLogicalName, SA.Id),
                RequiredAttendees = participants,
                cvt_ResourceGroup = new EntityReference(mcs_resourcegroup.EntityLogicalName, group.Id),
                Subject = SA.Subject + " for " + group.mcs_name,
                cvt_Site = site
            };
            //Logger.WriteDebugMessage(String.Format("Creating Appointment for {0}", appt.Subject));
            Trace($"Creating Appointment for {appt.Subject}.", LogLevel.Debug);
            Guid apptId = OrganizationService.Create(appt);
            //Logger.WriteDebugMessage(String.Format("Appointment Created for {0}, ID = {1}", appt.Subject, apptId));
            Trace($"Appointment Created for {appt.Subject}, ID = {apptId}.", LogLevel.Debug);

            //You have to create the appointment with open status and then call SetState to mark it as booked
            //SetStateRequest ssr = new SetStateRequest()
            //{
            //    State = new OptionSetValue(3),
            //    Status = new OptionSetValue(5),
            //    EntityMoniker = new EntityReference(Appointment.EntityLogicalName, apptId)
            //};
            var apptBookingStatus = new Appointment
            {
                Id = apptId,
                cvt_IntegrationBookingStatus = new OptionSetValue((int)Appointmentcvt_IntegrationBookingStatus.ReservedScheduled)
            };

            try
            { //Add this into Try Catch, so failure in blocking out appointment doesnt roll back rest of plugin.  Also allows for proper error logging if it fails (it becomes searchable)
                OrganizationService.Update(apptBookingStatus);
                //Logger.WriteDebugMessage("Appointment State Changed - Marked as Busy");
                Trace("Appointment State Changed - Marked as Busy", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                //Logger.WriteToFile("Appointment Set State Request Failed " + ex.Message);
                Trace($"Appointment Set State Request Failed {ex.Message}.", LogLevel.Error);
            }
        }

        #endregion

        #region Helper Functions
        /// <summary>
        /// This method creates the activity party which is later used to populate the new list of Resources for the Service Activity
        /// </summary>
        /// <param name="ResourceId">ID of System Resource Record which will become the ID of the PartyId EntityReference</param>
        /// <param name="oldList">Original List of System Resources (Both Pat and Prov Resources)</param>
        /// <param name="parentResourceSpec">Resource Spec Entity Reference to be used in the returned ActivityParty</param>
        /// <param name="ARresGrGuid">optional: used to compare resources in multiple Patient groups to ensure the correct group was retrieved</param>
        /// <returns>The activity Party which is to be used to create the new "Resources" List in the for the Reschedule Request</returns>
        internal ActivityParty returnActivityPartytoAddfromResource(Guid ResourceId, List<ActivityParty> oldList, EntityReference parentResourceSpec, Guid ARresGrGuid = new Guid())
        {
            ActivityParty result = new ActivityParty();
            using (var srv = new Xrm(OrganizationService))
            {
                //Does this check need to be here?
                var individualResource = srv.ResourceSet.FirstOrDefault(r => r.Id == ResourceId);
                if (individualResource != null && individualResource.ObjectTypeCode != ResourceSpec.EntityLogicalName)
                {
                    //Logger.WriteDebugMessage("provResource returned results, so it is Equip or User.  ObjectTypeCode = " + individualResource.ObjectTypeCode);
                    Trace($"provResource returned results, so it is Equip or User.  ObjectTypeCode = {individualResource.ObjectTypeCode}.", LogLevel.Debug);
                    //Setting up AP EntityRef
                    EntityReference actualPartyToAdd = new EntityReference()
                    {
                        Id = individualResource.Id,
                        LogicalName = individualResource.ObjectTypeCode
                    };

                    //Logger.WriteDebugMessage("Item is a(n) " + individualResource.ObjectTypeCode);
                    Trace($"Item is a(n) {individualResource.ObjectTypeCode}.", LogLevel.Debug);
                    //Logger.WriteDebugMessage("Individual.Id = " + individualResource.Id);
                    Trace($"Individual.Id = {individualResource.Id}.", LogLevel.Debug);

                    foreach (var oldParty in oldList)
                    {
                        //Loop through the list of resources currently listed in the SA.Resources field and verify that the new resource is in this list
                        //Logger.WriteDebugMessage("Looping through existing SA.resources. Old Id = " + oldParty.PartyId.Id.ToString());
                        Trace($"Looping through existing SA.resources. Old Id = {oldParty.PartyId.Id}.", LogLevel.Debug);
                        if (oldParty.PartyId.Id == individualResource.Id) //match, need to add it
                        {
                            var matched = matchAPFromOldListToNewGroupId(oldParty, ARresGrGuid); //returns true either if no Guid is passed in (automatically) or if the guid matches the Resource Group Listed (check for Paired groups)
                            if (matched)
                            {
                                //Logger.WriteDebugMessage("These matched. Printing RSid = " + parentResourceSpec.Id.ToString());
                                Trace($"These matched. Printing RSid = {parentResourceSpec.Id}.", LogLevel.Debug);
                                ActivityParty correctProvAP = new ActivityParty()
                                {
                                    PartyId = actualPartyToAdd,
                                    ResourceSpecId = parentResourceSpec,
                                    Effort = 1,
                                    ParticipationTypeMask = new OptionSetValue(10)
                                };
                                result = correctProvAP;
                                //Logger.WriteDebugMessage("Matched resource to oldList. Name = " + individualResource.Name);
                                Trace($"Matched resource to oldList. Name = {individualResource.Name}.", LogLevel.Debug);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    //Logger.WriteDebugMessage("Query to Resource table resulted in either no record, or the objectypecode = ResourceSpec.");
                    Trace("Query to Resource table resulted in either no record, or the objectypecode = ResourceSpec.", LogLevel.Debug);
                }
            }
            return result;
        }

        /// <summary>
        /// This method checks that the Resource from the Old List that corresponds to the item from the new list are using the same resource group - only used for Paired Groups, any other groups should pass in null for the newGroupId, which results in a return value of true
        /// </summary>
        /// <param name="oldListItem">The old Activity Party item, from which we retrieve the ResourceSpec and its ConstraintBasedGroup</param>
        /// <param name="newGroupId">The Id of the Resource Group that corresponds to the System Resource to be added</param>
        /// <returns>true if the check passes (or no check is needed), or false if the check fails</returns>
        /// <remarks>For anything other than an paired group, newGroupId should be null, which results in an automatic return of true - aka pass the check</remarks>
        internal bool matchAPFromOldListToNewGroupId(ActivityParty oldListItem, Guid newGroupId)
        {
            //Logger.WriteDebugMessage("Passed in param of ARresGrGuid " + newGroupId.ToString());
            Trace($"Passed in param of ARresGrGuid {newGroupId}.", LogLevel.Debug);
            if (newGroupId != Guid.Empty)
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    var rootSpec = srv.ResourceSpecSet.FirstOrDefault(rs => rs.Id == oldListItem.ResourceSpecId.Id);
                    if (rootSpec != null)
                    {
                        var OldCBG = srv.ConstraintBasedGroupSet.FirstOrDefault(cbg => cbg.Id == rootSpec.GroupObjectId.Value);
                        //Logger.WriteDebugMessage(String.Format("Checking if Base CBG {0} matched passed RG {1}, then this is the correct branch", OldCBG.Id, newGroupId.ToString()));
                        Trace($"Checking if Base CBG {OldCBG.Id} matched passed RG {newGroupId}, then this is the correct branch", LogLevel.Debug);
                        if (OldCBG != null && OldCBG.Id == newGroupId)
                            return true;
                        else
                        {
                            //Logger.WriteDebugMessage("Constraint Based Group for the Resource Spec did not align with Resource Group for " + oldListItem.PartyId.Name);
                            Trace($"Constraint Based Group for the Resource Spec did not align with Resource Group for {oldListItem.PartyId.Name}.", LogLevel.Debug);
                        }
                    }
                    else
                    {
                        //Logger.WriteDebugMessage("ActivityParty in Old List did not have valid Resource Spec");
                        Trace("ActivityParty in Old List did not have valid Resource Spec.", LogLevel.Debug);
                    }
                }
                return false;
            }
            else
            {
                //Logger.WriteDebugMessage("No Group Id passed, no need to match, so return true");
                Trace("No Group Id passed, no need to match, so return true.", LogLevel.Debug);
                return true;
            }
        }

        /// <summary>
        /// This method accepts the Id of the resource spec, then gets the Constraint based group, and returns the list of resource Specs/ResourceIds underneath
        /// </summary>
        /// <param name="ResourceSpecId">The ID of the resource Spec to be used to get the constraint based group (which contains the list of IDs)</param>
        /// <returns></returns>
        internal List<Guid> returnResourceIdsFromRS(Guid ResourceSpecId)
        {
            //Logger.WriteDebugMessage("Starting returnResourceIdsFromRS. RSid = " + ResourceSpecId);
            Trace($"Starting returnResourceIdsFromRS. RSid = {ResourceSpecId}.", LogLevel.Debug);
            List<Guid> results = new List<Guid>();
            using (var srv = new Xrm(OrganizationService))
            {
                //Get the actual ResSpec record
                var resSpec = srv.ResourceSpecSet.FirstOrDefault(rs => rs.Id == ResourceSpecId);
                if (resSpec != null && resSpec.GroupObjectId != null)
                {
                    //Logger.WriteDebugMessage("resSpec name = " + resSpec.Name + " GroupObjectId = " + resSpec.GroupObjectId.Value);
                    Trace($"resSpec name = {resSpec.Name} GroupObjectId = {resSpec.GroupObjectId.Value}.", LogLevel.Debug);
                    //Get the related Constraint Based Group
                    results = returnResourceIdsFromCBG(resSpec.GroupObjectId.Value);
                }
            }
            return results;
        }

        /// <summary>
        /// Takes a constraint based group and parses out the Resource IDs listed within (Resource IDs can be either child resource Specs, or System Resource IDs)
        /// </summary>
        /// <param name="RGId">The ID of the Constraint Based Group/Resource Group (They are the same ID)</param>
        /// <returns>The resource IDs (or ResourceSpec IDs)</returns>
        internal List<Guid> returnResourceIdsFromCBG(Guid RGId)
        {
            //Logger.WriteDebugMessage("Starting returnResourceIdsFromCBG. CBGid = " + RGId);
            Trace($"Starting returnResourceIdsFromCBG. CBGid = {RGId}.", LogLevel.Debug);
            List<Guid> results = new List<Guid>();
            using (var srv = new Xrm(OrganizationService))
            {
                //Get the related Constraint Based Group for this Resource Group ID
                var cbg = srv.ConstraintBasedGroupSet.FirstOrDefault(c => c.Id == RGId);
                if (cbg != null)
                {
                    //Logger.WriteDebugMessage("Found CBG. Constraints = " + cbg.Constraints);
                    Trace($"Found CBG. Constraints = {cbg.Constraints}.", LogLevel.Debug);
                    var cbgResIds = cbg.Constraints.Split(new[] { "||" }, StringSplitOptions.None);
                    //Loop through strings
                    for (int i = 0; i < cbgResIds.Count(); i++)
                    {
                        var begin = cbgResIds[i].IndexOf("{");
                        var end = cbgResIds[i].IndexOf("}");
                        if (begin != -1 && end != -1)
                            results.Add(new Guid(cbgResIds[i].Substring(begin + 1, end - begin - 1)));
                    }
                    //Logger.WriteDebugMessage("Finished parsing through RS> CBG> constraint string for ResourceIds.");
                    Trace("Finished parsing through RS> CBG> constraint string for ResourceIds.", LogLevel.Debug);
                }
            }
            return results;
        }

        #endregion


        #region Additional Interface Methods/Properties
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["post"];
        }

        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }
        #endregion
    }
}
