using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.CRM
{
    public class UserTeamActionPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service =
                factory.CreateOrganizationService(context.UserId);

            var teamId = (string)context.InputParameters["TeamID"];
            Guid[] userIDs = new[] { context.PrimaryEntityId };
            List<Guid> teamIDs = new List<Guid>();
            foreach (string team in teamId.Split(','))
            {
                teamIDs.Add(new Guid(team));
            }
           
            RemoveMembersFromTeam(teamIDs.ToArray(), userIDs, service);
        }

        public static void RemoveMembersFromTeam(Guid[] teamIds, Guid[] membersId, IOrganizationService service)
        {
            foreach (Guid teamId in teamIds)
            {
                RemoveMembersTeamRequest removeRequest = new RemoveMembersTeamRequest();

                removeRequest.TeamId = teamId;
                removeRequest.MemberIds = membersId;
                service.Execute(removeRequest);
            }
        }
    }
}
