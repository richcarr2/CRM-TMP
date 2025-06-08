using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Get Service Appointment step.
    /// </summary>
    public class GetServiceAppointmentStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetServiceAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                if (state.AppointmentId != null)
                {
                    state.Appointment = context.AppointmentSet.FirstOrDefault(x => x.Id == state.AppointmentId);
                    if (state.Appointment == null) throw new MissingAppointmentException("Appointment cannot be null");

                    if (state.Appointment != null && state.Appointment.cvt_serviceactivityid != null) state.ServiceAppointment = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == state.Appointment.cvt_serviceactivityid.Id);
                    if (state.ServiceAppointment == null) throw new MissingAppointmentException(string.Format("Unable to retrieve Service Appointment for group with AppointmentId - {0}", state.AppointmentId));
                    if (state.Appointment.cvt_Site == null) throw new MissingSiteException(string.Format("No Patient Site Listed on Group Appointment with Id {0}", state.AppointmentId));

                    var patSite = context.mcs_siteSet.FirstOrDefault(x => x.Id == state.Appointment.cvt_Site.Id);

                    if (patSite != null) state.PatientSite = patSite.mcs_ParentStationNumber;
                    else throw new MissingSiteException(string.Format("Unable to find Site with Id {0}", state.Appointment.cvt_Site.Id));
                }
                else
                {
                    state.ServiceAppointment = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == state.ServiceAppointId);
                    if (state.ServiceAppointment == null) throw new MissingAppointmentException(string.Format("Unable to retrieve Service Appointment - {0}", state.ServiceAppointId));

                    // Get Patient Site
                    if (state.ServiceAppointment.mcs_relatedsite == null)
                    {
                        state.PatientSite = string.Empty;
                        _logger.Info("Patient Site is empty.");
                    }
                    else
                    {
                        var relatedSiteId = context.mcs_siteSet.FirstOrDefault(x => x.mcs_siteId == state.ServiceAppointment.mcs_relatedsite.Id);
                        state.PatientSite = relatedSiteId != null ? relatedSiteId.mcs_ParentStationNumber : string.Empty;
                        if (string.IsNullOrEmpty(state.PatientSite)) _logger.Info("No Patient Site Found.");
                    }
                }

                // Get Service (TSA)
                // TSA is Deprecated, use Scheduling Package
                //state.Service = state.ServiceAppointment.mcs_relatedtsa == null ? null : context.mcs_servicesSet.FirstOrDefault(x => x.mcs_servicesId == state.ServiceAppointment.mcs_relatedtsa.Id);

                state.ResourcePackage = state.ServiceAppointment.cvt_relatedschedulingpackage == null ? null : context.cvt_resourcepackageSet.FirstOrDefault(x => x.cvt_resourcepackageId == state.ServiceAppointment.cvt_relatedschedulingpackage.Id);

                if (state.ResourcePackage == null) throw new MissingSchedulingPackageException("Unable to retrieve the Scheduling Package");

                // Get Provider Site
                if (state.ServiceAppointment.mcs_relatedprovidersite == null)
                {
                    state.ProviderSite = string.Empty;
                    _logger.Info("Provider Site is empty.");
                }
                else
                {
                    var relatedProviderSiteId = context.mcs_siteSet.FirstOrDefault(x => x.mcs_siteId == state.ServiceAppointment.mcs_relatedprovidersite.Id);
                    state.ProviderSite = relatedProviderSiteId != null ? relatedProviderSiteId.mcs_ParentStationNumber : string.Empty;
                    if (string.IsNullOrEmpty(state.ProviderSite)) _logger.Info("No Provider Site Found.");
                }
            }
        }
    }
}
