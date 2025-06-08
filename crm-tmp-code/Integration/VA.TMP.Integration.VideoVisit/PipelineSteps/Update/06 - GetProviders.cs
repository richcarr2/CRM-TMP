using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VideoVisit.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    public class GetProvidersStep : IFilter<VideoVisitUpdateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetProvidersStep(ILog logger)
        {
            _logger = logger;
        }

        public void Execute(VideoVisitUpdateStateObject state)
        {
            if (state.ServiceAppointment.Resources == null || state.ServiceAppointment.Resources.ToList().Count == 0) throw new MissingResourceException("Resources must be specified for all appointments");

            var bookedSysUsers = state.ServiceAppointment.Resources.Where(r => r.PartyId.LogicalName == "systemuser").ToList();

            var isStoreForward = state.ServiceAppointment.cvt_TelehealthModality ?? false;
            if (bookedSysUsers.Count == 0 && !isStoreForward) throw new MissingProviderException("Unable to retrieve Provider Data");

            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var schedulingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(r => r.Id == state.ServiceAppointment.cvt_relatedschedulingpackage.Id);
                if (schedulingPackage == null) throw new MissingSchedulingPackageException("No Scheduling Package is associated with the service activity");

                var parties = bookedSysUsers.Select(ap => ap.PartyId.Id);

                var provSite = srv.cvt_participatingsiteSet.FirstOrDefault(x =>
                    x.cvt_resourcepackage.Id == schedulingPackage.Id &&
                    x.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider &&
                    x.cvt_site.Id == state.ServiceAppointment.mcs_relatedprovidersite.Id);

                if (provSite == null) throw new MissingSiteException("The Provider Site is missing");

                // Query for Scheduling Resource Listing the Provider as a user directly
                var schedulingResource = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_participatingsite.Id == provSite.Id && sr.cvt_user != null).Select(sr => sr.cvt_user.Id).ToList();

                // Query for PRGs Listing the Provider as a user through a group
                var groupSchedulingResources = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_participatingsite.Id == provSite.Id && sr.cvt_tmpresourcegroup != null).Select(sr => sr.cvt_tmpresourcegroup.Id).ToList();
                var groupResourceUserIds = new List<Guid>();

                foreach (var sr in groupSchedulingResources)
                {
                    groupResourceUserIds.AddRange(srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == sr && gr.mcs_RelatedUserId != null).Select(gr => gr.mcs_RelatedUserId.Id).ToList());
                }

                // Match Group PRGs with booked resources
                var matches = new List<Guid>();

                foreach (var party in parties)
                {
                    if (groupResourceUserIds.Contains(party)) matches.Add(party);
                    if (schedulingResource.Contains(party)) matches.Add(party);
                }

                foreach (var match in matches)
                {
                    var user = srv.SystemUserSet.FirstOrDefault(u => u.Id == match);
                    state.SystemUsers.Add(user);
                }
            }
        }
    }
}
