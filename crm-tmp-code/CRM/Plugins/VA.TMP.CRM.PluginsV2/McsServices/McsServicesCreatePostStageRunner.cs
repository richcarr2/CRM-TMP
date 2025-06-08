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
    public class McsServicesCreatePostStageRunner : PluginRunner
    {
        //Instantiate McsServicesCreatePostStageRunner object for thread safety purposes
        public McsServicesCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }
        #region Internal Methods/Properties
        /// <summary>
        /// Runs primary business logic - 1 copy provider site resources from Master TSA, 2 - share TSA with patient facility if it is InterVISN
        /// </summary>
        public override void Execute()
        {
            //if (PrimaryEntity.LogicalName != mcs_services.EntityLogicalName)
            //    throw new Exception("Target entity is not of type mcs_services, Plugin Registered Incorrectly");
            if (PrimaryEntity.LogicalName != cvt_facilityapproval.EntityLogicalName)
                throw new Exception("Target entity is not of type cvt_facilityapproval, Plugin Registered Incorrectly");

            var tsa = PrimaryEntity.ToEntity<cvt_facilityapproval>();
            if (tsa != null && tsa.cvt_resourcepackage == null)
            {
                Logger.WriteDebugMessage("not doing copy Scheduling Package Prov Resources");
                return;
            }

            Logger.WriteDebugMessage("Copying Provider Resources from Scheduling Package onto this TSA");
            // Logger.WriteGranularTimingMessage("Starting CreateMasterProviderResourceGroups");
            // CreateMasterProviderResourceGroups(tsa); 
            // Logger.WriteGranularTimingMessage("Ending CreateMasterProviderResourceGroups");

            // new var for servicescope 
            string servicescope = string.Empty;
            if ((tsa.cvt_patientfacility != null) && (tsa.cvt_providerfacility != null) && (tsa.cvt_patientfacility != tsa.cvt_providerfacility))
            {
                servicescope = "InterFacility";
            }
            else {
                servicescope = "IntraFacility";
            }
                
            //Run enable InterVISN TSA - the method itself will exit execution if criteria are not met
            var shared = EnableInterFacilityTSA(tsa, Logger, OrganizationService);
            Logger.WriteDebugMessage(String.Format("TSA is listed as: {0}. It was Shared: {1} ", servicescope, shared));
        }

        internal static bool EnableInterFacilityTSA(cvt_facilityapproval tsa, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            Logger.setMethod = "Enable InterFacility TSA";
            Logger.WriteDebugMessage("Starting InterFacility TSA");
            using (var srv = new Xrm(OrganizationService))
            {
                //var proFacility = tsa.cvt_ProviderFacility;
                //var patFacility = tsa.cvt_PatientFacility;

                var proFacility = tsa.cvt_providerfacility;
                var patFacility = tsa.cvt_providerfacility;


                // new var for servicescope 
                string servicescope = string.Empty;
                if ((tsa.cvt_patientfacility != null) && (tsa.cvt_providerfacility != null) && (tsa.cvt_patientfacility != tsa.cvt_providerfacility))
                {
                    servicescope = "InterFacility";
                }
                else
                {
                    servicescope = "IntraFacility";
                }

                ///this may break when optionsets is regenerated
                var scope = (servicescope != string.Empty) ? (servicescope=="InterFacility")?(int)mcs_servicescvt_ServiceScope.InterFacility: (int)mcs_servicescvt_ServiceScope.IntraFacility : (int)mcs_servicescvt_ServiceScope.IntraFacility;

                if ( patFacility == null || scope == (int)mcs_servicescvt_ServiceScope.IntraFacility)
                    return false;

                //get Scheduling Package for specialty
                /*------------------------------------------------------------------*/
                cvt_resourcepackage resPkg = srv.cvt_resourcepackageSet.FirstOrDefault(r => r.Id == tsa.cvt_resourcepackage.Id);

                /*------------------------------------------------------------------*/

                //var FacilityTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == patFacility.Id && 
                //    (t.cvt_ServiceType == null || t.cvt_ServiceType.Id == tsa.cvt_servicetype.Id)).ToList();
                var FacilityTeams = srv.TeamSet.Where(t => t.cvt_Facility.Id == patFacility.Id &&
                    (t.cvt_ServiceType == null || t.cvt_ServiceType.Id == resPkg.cvt_specialty.Id)).ToList(); 

                AccessRights approverRights = AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.WriteAccess | AccessRights.ReadAccess;

                //set the following teams so they can be shared with
                Team patFTCTeam = new Team(), 
                    patCPTeam = new Team(), 
                    patSCTeam = new Team(), 
                    patCoSTeam = new Team(), 
                    patSchedTeam = new Team();

                foreach (var team in FacilityTeams)
                {
                    if (team.cvt_Type != null && team.cvt_Type.Value == (int)Teamcvt_Type.FTC)
                        patFTCTeam = team;
                    if (team.cvt_Type != null && team.cvt_Type.Value == (int)Teamcvt_Type.CredentialingandPrivileging)
                        patCPTeam = team;
                    if (team.cvt_Type != null && team.cvt_Type.Value == (int)Teamcvt_Type.ServiceChief)
                        patSCTeam = team;
                    if (team.cvt_Type != null && team.cvt_Type.Value == (int)Teamcvt_Type.ChiefofStaff)
                        patCoSTeam = team;
                    if (team.cvt_Type != null && team.cvt_Type.Value == (int)Teamcvt_Type.Scheduler)
                        patSchedTeam = team;

                }

                Logger.WriteDebugMessage(String.Format("Sharing Interfacility TSA from PROV({0}) to PAT({1})", proFacility.Id, patFacility.Id));
                //Give Access to all of the proper teams
                var missingTeams = "";
                if (patFTCTeam == null ||  patFTCTeam.Id == new Guid())
                    missingTeams += "FTC, ";
                else
                    GrantAccess(patFTCTeam, tsa, approverRights, Logger, OrganizationService);

                if (patSCTeam == null || patSCTeam.Id == new Guid())
                    missingTeams += "Service Chief, ";
                else
                    GrantAccess(patSCTeam, tsa, approverRights, Logger, OrganizationService);

                if (patCPTeam == null || patCPTeam.Id == new Guid())
                    missingTeams += "Cred/Priv, ";
                else
                    GrantAccess(patCPTeam, tsa, approverRights, Logger, OrganizationService);

                if (patCoSTeam == null || patCoSTeam.Id == new Guid())
                    missingTeams += "Chief of Staff, "; 
                else
                    GrantAccess(patCoSTeam, tsa, approverRights, Logger, OrganizationService);

                if (patSchedTeam == null || patSchedTeam.Id == new Guid())
                    missingTeams += "Scheduling, ";
                else
                    GrantAccess(patSchedTeam, tsa, AccessRights.AppendToAccess | AccessRights.AppendAccess | AccessRights.ReadAccess, Logger, OrganizationService);

                if (missingTeams != "")
                    Logger.WriteDebugMessage(String.Format("Unable to find the following teams to share an Interfacility TSA for {0}: {1}. Please create those teams and then run the 'InterFacilityTSA' Plugin Settings job.", patFacility.Id, missingTeams.Trim().Substring(0, missingTeams.Length-2)));
                return true;
            }
        }

        /// <summary>
        /// Shares the privilege specified in "accessType" for the record in "Target" with the team in "Team"
        /// </summary>
        /// <param name="team">Team who will be shared with</param>
        /// <param name="target">Record that will be shared</param>
        /// <param name="accessType">privilege that will be shared</param>
        //internal static void GrantAccess(Team team, mcs_services target, AccessRights accessType, MCSLogger Logger, IOrganizationService OrganizationService)
        internal static void GrantAccess(Team team, cvt_facilityapproval target, AccessRights accessType, MCSLogger Logger, IOrganizationService OrganizationService)
        {
            if (team == null || team.Id == new Guid())
            {
                Logger.WriteDebugMessage("Team not found.");
                return;
            }
            Logger.setMethod = "Grant Access " + accessType.ToString();
            Logger.WriteDebugMessage(String.Format("Granting {0} privileges to {1}", accessType.ToString(), team.Name));
            // Grant the Team read access to the created TSA.
            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = accessType,
                    Principal = new EntityReference()
                    {
                        Id = team.Id,
                        Name = team.Name,
                        LogicalName = team.LogicalName
                    }
                },
                Target = new EntityReference()
                {
                    Id = target.Id,
                    Name = target.cvt_name,
                    LogicalName = target.LogicalName
                }
            };

            OrganizationService.Execute(grantAccessRequest);
            Logger.WriteDebugMessage("Privilege Granted");
        }

        //internal static cvt_providerresourcegroup GetNewProviderResourceGroup(mcs_services tsa, 
        //    cvt_providerresourcegroup providerResourceGroup)
        internal static cvt_providerresourcegroup GetNewProviderResourceGroup(cvt_facilityapproval tsa,
            cvt_providerresourcegroup providerResourceGroup)
        {
            
                var newProviderResourceGroup = new cvt_providerresourcegroup()
                    {
                        cvt_name = providerResourceGroup.cvt_name,
                        cvt_RelatedResourceGroupid = providerResourceGroup.cvt_RelatedResourceGroupid,
                        cvt_RelatedResourceId = providerResourceGroup.cvt_RelatedResourceId,
                        cvt_relatedsiteid = providerResourceGroup.cvt_relatedsiteid,
                        cvt_RelatedTSAid = new EntityReference(cvt_facilityapproval.EntityLogicalName, tsa.Id),
                        cvt_RelatedUserId = providerResourceGroup.cvt_RelatedUserId,
                        cvt_Type = providerResourceGroup.cvt_Type,
                        cvt_TSAResourceType = providerResourceGroup.cvt_TSAResourceType,
                        cvt_constraintgroupguid = providerResourceGroup.cvt_constraintgroupguid,
                        cvt_resourcespecguid = providerResourceGroup.cvt_resourcespecguid
                    };


                return newProviderResourceGroup;
            
        }

        //internal void CreateMasterProviderResourceGroups(cvt_facilityapproval tsa)
        //{
        //    try
        //    {
        //        Logger.setMethod = "QuickCreatePatientGroupResource";
        //        using (var context = new Xrm(OrganizationService))
        //        {
        //            //var masterTsa = context.CreateQuery<cvt_mastertsa>().FirstOrDefault(c => 
        //            //    c.Id == tsa.cvt_resourcepackage.Id);
        //            var masterTsa = context.CreateQuery<cvt_resourcepackage>().FirstOrDefault(c =>
        //                             c.Id == tsa.cvt_resourcepackage.Id);

        //            if (masterTsa == null)
        //                return;

        //            //context.LoadProperty(masterTsa, "cvt_cvt_mastertsa_cvt_providerresourcegroup_RelatedTSAId");
        //            context.LoadProperty(masterTsa, "cvt_cvt_resourcepackage_cvt_participatingsite_resourcepackage");

        //            if (masterTsa.cvt_cvt_resourcepackage_cvt_participatingsite_resourcepackage == null || 
        //                !masterTsa.cvt_cvt_resourcepackage_cvt_participatingsite_resourcepackage.Any())
        //                return;

        //            foreach (var newProviderResourceGroup in
        //               masterTsa.cvt_cvt_resourcepackage_cvt_participatingsite_resourcepackage.Select(providerResourceGroup =>
        //                    GetNewProviderResourceGroup(tsa, providerResourceGroup)))
        //            {
        //                Logger.WriteDebugMessage("Adding Resource");
        //                context.AddObject(newProviderResourceGroup);
        //            }
        //            Logger.WriteDebugMessage("Saving Changes");

        //            context.SaveChanges();
        //        }
        //    }
        //    catch (FaultException<OrganizationServiceFault> ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //        throw new InvalidPluginExecutionException(ex.Message);

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //        throw new InvalidPluginExecutionException(ex.Message);

        //    }
        //}

        #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_tsaplugin"; }
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }
        #endregion
    }
}