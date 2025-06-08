using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class UserCreatePostStageRunner : PluginRunner
    {
        #region Constructor
        public UserCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            UpdateVISN(PrimaryEntity.Id, OrganizationService, Logger);

            //Assign to TCT Team
            UserUpdatePostStageRunner.ifTCTAssignToTeam(PrimaryEntity.Id, OrganizationService, Logger);
        }

        /// <summary>
        /// Gets the current VISN and the VISN that the user should be in based on their BU, and updates the VISN field if it is not correct
        /// </summary>
        /// <param name="UserId">used to get the full User Record</param>
        /// <param name="OrganizationService">The Org Service for connecting to CRM</param>
        /// <param name="Logger">The Logger to write logging statements</param>
        internal static void UpdateVISN(Guid UserId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.setMethod = "UpdateVISN";
            Logger.WriteDebugMessage("starting");
            var thisUser = OrganizationService.Retrieve(SystemUser.EntityLogicalName, UserId, new ColumnSet(true)).ToEntity<SystemUser>();
            var properVISN = GetUserVISN(thisUser, OrganizationService, Logger);
            if (properVISN != Guid.Empty && (thisUser.mcs_VISN == null || thisUser.mcs_VISN.Id != properVISN))
            {
                var updateUser = new SystemUser()
                {
                    Id = UserId,
                    mcs_VISN = new EntityReference(BusinessUnit.EntityLogicalName, properVISN)
                };
                OrganizationService.Update(updateUser);
            }
            else
                Logger.WriteDebugMessage(String.Format("No need to update User {0}'s VISN", thisUser.FullName));
            Logger.WriteDebugMessage("Ending method");
        }

        /// <summary>
        /// Retrieves the VISN BU that corresponds to the User's BU whether the user is in a Facility BU or VISN BU
        /// </summary>
        /// <param name="thisUser"></param>
        /// <param name="OrganizationService"></param>
        /// <param name="Logger"></param>
        /// <returns>the VISN BU EntityReference or a blank id</returns>
        internal static Guid GetUserVISN(SystemUser thisUser, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Guid VISN = Guid.Empty;
            using (var srv = new Xrm(OrganizationService))
            {
                if (thisUser.BusinessUnitId == null)
                    throw new InvalidPluginExecutionException("A Business Unit must be selected for all users");
                if (thisUser.BusinessUnitId.Name == "VHA")
                    return VISN; //No VISN is applicable because this user operates Organization Wide
                else if (thisUser.BusinessUnitId.Name.Contains("VISN"))
                    VISN = thisUser.BusinessUnitId.Id;
                else
                {
                    var BU = srv.BusinessUnitSet.FirstOrDefault(bu => bu.Id == thisUser.BusinessUnitId.Id);
                    VISN = BU.ParentBusinessUnitId.Id;
                }
            }
            return VISN;
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