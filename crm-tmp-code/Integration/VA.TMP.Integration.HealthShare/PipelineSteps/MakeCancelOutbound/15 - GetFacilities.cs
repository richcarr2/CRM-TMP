using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Get Facilities Step.
    /// </summary>
    public class GetFacilitiesStep : IFilter<MakeCancelOutboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetFacilitiesStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelOutboundStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                mcs_site patSite = null;
                mcs_site proSite = null;

                var scheduleingPackage = srv.cvt_resourcepackageSet.FirstOrDefault(x => x.Id == state.ServiceAppointment.cvt_relatedschedulingpackage.Id);
                if (scheduleingPackage == null) throw new MissingSchedulingPackageException($"The Scheduling Package cannot be null for appointment id {state.ServiceAppointmentId}");

                _logger.Info($"Related Site Null? {state.ServiceAppointment.mcs_relatedsite == null}");
                if (!state.IsGroupAppointment && state.ServiceAppointment.mcs_relatedsite != null)
                {
                    patSite = srv.mcs_siteSet.FirstOrDefault(pat => pat.Id == state.ServiceAppointment.mcs_relatedsite.Id);
                    _logger.Info($"patSite Null? {patSite == null}");
                }
                else if (state.IsGroupAppointment)
                {
                    patSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == state.Appointment.cvt_Site.Id);
                    if (patSite?.mcs_FacilityId == null) throw new MissingFacilityException("The Patient Site is missing or Facility is missing");
                }

                _logger.Info($"Related Provider Site Null? {state.ServiceAppointment.mcs_relatedprovidersite == null}");
                if (state.ServiceAppointment.mcs_relatedprovidersite != null)
                {
                    proSite = srv.mcs_siteSet.FirstOrDefault(pro => pro.Id == state.ServiceAppointment.mcs_relatedprovidersite.Id);
                    _logger.Info($"proSite Null? {proSite == null}");
                }

                if (patSite != null)
                {
                    state.PatientFacility = srv.mcs_facilitySet.FirstOrDefault(x => x.Id == patSite.mcs_FacilityId.Id);
                    if (state.PatientFacility == null) throw new MissingFacilityException("The Patient Facility is missing");
                }

                if (proSite == null) return;

                state.ProviderFacility = srv.mcs_facilitySet.FirstOrDefault(x => x.Id == proSite.mcs_FacilityId.Id);
                if (state.ProviderFacility == null) throw new MissingFacilityException("The Provider Facility is missing");
            }
        }
    }
}
