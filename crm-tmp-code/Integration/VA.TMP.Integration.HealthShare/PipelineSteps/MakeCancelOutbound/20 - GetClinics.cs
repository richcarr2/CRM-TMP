using System.Linq;
using System.Threading;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Get Clinics step.
    /// </summary>
    public class GetClinicsStep : IFilter<MakeCancelOutboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetClinicsStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelOutboundStateObject state)
        {
            if (state.AppointmentType != AppointmentType.HOME_MOBILE) state.PatientClinic = GetClinic(state, Side.Patient, _logger);
            if (state.AppointmentType != AppointmentType.STORE_FORWARD) state.ProviderClinic = GetClinic(state, Side.Provider, _logger);
        }

        /// <summary>
        /// Gets the Resource for the given Patient or Provider.
        /// </summary>
        /// <param name="state">State object.</param>
        /// <param name="side">Patient or Provider.</param>
        /// <returns>Resource.</returns>
        private static mcs_resource GetClinic(MakeCancelOutboundStateObject state, Side side, ILog logger)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var bookedResources = state.ServiceAppointment.Resources.Where(r => r.PartyId.LogicalName == "equipment").ToList();

                if (state.Appointment?.RequiredAttendees != null && side == Side.Patient && state.IsGroupAppointment)
                {
                    bookedResources.AddRange(state.Appointment.RequiredAttendees.Where(r => r.PartyId.LogicalName == "equipment").ToList());
                }

                if (bookedResources.Count < 1) throw new MissingClinicException($"No Valid Vista Clinics were found for the {side.ToString()} side");

                foreach (var equipmentParty in bookedResources)
                {
                    //var resource = srv.mcs_resourceSet.FirstOrDefault(r => r.mcs_relatedResourceId != null && r.mcs_relatedResourceId.Id == equipmentParty.PartyId.Id);
                    var resources = srv.mcs_resourceSet.Where(r => r.mcs_relatedResourceId != null && r.mcs_relatedResourceId.Id == equipmentParty.PartyId.Id).ToList();
                    foreach (var resource in resources)
                    {
                        if (resource == null) throw new MissingClinicException("The resource cannot be null");

                        if (resource.mcs_Type.Value != (int)mcs_resourcetype.VistaClinic)
                        {
                            logger.Info($"Equipment record is not a Vista clinic: {resource.mcs_name}. Continuing.");
                            continue;
                        }
                        if (side != Side.Patient)
                        {
                            logger.Info($"***** Side = Provider, AppointmentType: {state.AppointmentType}, IsGroup: {state.IsGroupAppointment}, RelatedProviderSite Null? {state.ServiceAppointment.mcs_relatedprovidersite == null}");

                            if (state.ServiceAppointment.mcs_relatedprovidersite != null)
                            {
                                // Find Participating Site
                                var providerPs = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.cvt_site.Id == state.ServiceAppointment.mcs_relatedprovidersite.Id &&
                                    ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider &&
                                    ps.cvt_resourcepackage.Id == state.ServiceAppointment.cvt_relatedschedulingpackage.Id);

                                if (providerPs != null)
                                {
                                    // Match VC as a standalone or part of a TMP Resource Group
                                    var standaloneVc = srv.cvt_schedulingresourceSet.FirstOrDefault(sr => sr.cvt_tmpresource.Id == resource.Id && sr.cvt_participatingsite.Id == providerPs.Id);

                                    if (standaloneVc != null)
                                    {
                                        logger.Info($"Vista Clinic matched a standalone VC on the Provider PS: {resource.mcs_name}");
                                        return resource;
                                    }

                                    logger.Info($"Vista Clinic didn't match a standalone VC on the Provider PS, checking TMP Resource Groups: {resource.mcs_name}");

                                    // Get all TMP Resource Groups
                                    var rgSet = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_tmpresourcegroup != null && sr.cvt_participatingsite.Id == providerPs.Id).ToList();

                                    foreach (var record in rgSet)
                                    {
                                        // Loop through Resource Groups and get all Group Resources
                                        var groupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == record.cvt_tmpresourcegroup.Id).ToList();

                                        foreach (var item in groupResources)
                                        {
                                            // Loop through all Group Resources and check for this particular VistA Clinic TMP Resource
                                            if (item.mcs_RelatedResourceId?.Id == resource.Id)
                                            {
                                                logger.Info($"Vista Clinic matched a VC within a TMP Resource Group on the Provider PS. Resource: {resource.mcs_name}");
                                                return resource;
                                            }
                                            else
                                                logger.Info($"Vista Clinic did not match this group resource: {item.mcs_name}. Continuing.");
                                        }
                                        logger.Info($"Vista Clinic did not match any group resources within this TMP Resource Group: {record.cvt_name}. Continuing.");
                                    }
                                }
                                else
                                {
                                    logger.Error("No Provider Particpating Site was found.");
                                    throw new MissingSiteException("No Provider Particpating Site was found.");
                                }
                            }
                            else
                            {
                                logger.Error("No Provider Site is listed");
                                throw new MissingSiteException("No Provider Site is listed");
                            }
                        }
                        else
                        {
                            logger.Info($"***** Side = Patient, AppointmentType: {state.AppointmentType}, IsGroup: {state.IsGroupAppointment}, RelatedSite Null? {state.ServiceAppointment.mcs_relatedsite == null}");
                            if (!state.IsGroupAppointment && state.ServiceAppointment.mcs_relatedsite != null)
                            {
                                // Find Participating Site
                                var patientPs = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.cvt_site.Id == state.ServiceAppointment.mcs_relatedsite.Id &&
                                    ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient &&
                                    ps.cvt_resourcepackage.Id == state.ServiceAppointment.cvt_relatedschedulingpackage.Id);

                                if (patientPs != null)
                                {
                                    // Match VC as a standalone or part of a TMP Resource Group
                                    var standaloneVc = srv.cvt_schedulingresourceSet.FirstOrDefault(sr => sr.cvt_tmpresource.Id == resource.Id && sr.cvt_participatingsite.Id == patientPs.Id);

                                    if (standaloneVc != null)
                                    {
                                        logger.Info($"Vista Clinic matched a standalone VC on the Patient PS: {resource.mcs_name}");
                                        return resource;
                                    }

                                    logger.Info($"Vista Clinic didn't match a standalone VC on the Patient PS, checking TMP Resource Groups for this VC: {resource.mcs_name}");

                                    // Get all TMP Resource Groups
                                    var rgSet = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_tmpresourcegroup != null && sr.cvt_participatingsite.Id == patientPs.Id).ToList();

                                    foreach (var record in rgSet)
                                    {
                                        // Loop through Resource Groups and get all Group Resources
                                        var groupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == record.cvt_tmpresourcegroup.Id).ToList();

                                        foreach (var item in groupResources)
                                        {
                                            // Loop through all Group Resources and check for this particular VistA Clinic TMP Resource
                                            if (item.mcs_RelatedResourceId?.Id == resource.Id)
                                            {
                                                logger.Info($"Vista Clinic matched a VC within a TMP Resource Group on the Patient PS. Resource: {resource.mcs_name}");
                                                return resource;
                                            }
                                            else
                                                logger.Info($"Vista Clinic did not match this group resource: {item.mcs_name}. Continuing.");
                                        }
                                        logger.Info($"Vista Clinic did not match any group resources within this TMP Resource Group: {record.cvt_name}. Continuing.");
                                    }
                                }
                                else
                                {
                                    logger.Error("No Patient Particpating Site was found.");
                                    throw new MissingSiteException("No Patient Particpating Site was found.");
                                }
                            }
                            else if (state.IsGroupAppointment && state.Appointment?.cvt_Site != null)
                            {
                                // Find Participating Site
                                var patientPs = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_scheduleable.Value &&
                                    ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient &&
                                    ps.cvt_resourcepackage.Id == state.ServiceAppointment.cvt_relatedschedulingpackage.Id &&
                                    ps.cvt_site.Id == state.Appointment.cvt_Site.Id).ToList();

                                if (patientPs.Any())
                                {
                                    foreach (var ps in patientPs)
                                    {
                                        // Match VC as a standalone or part of a TMP Resource Group
                                        var standaloneVc = srv.cvt_schedulingresourceSet.FirstOrDefault(sr => sr.cvt_tmpresource.Id == resource.Id && sr.cvt_participatingsite.Id == ps.Id);

                                        if (standaloneVc != null)
                                        {
                                            logger.Info($"Group Vista Clinic matched a standalone VC on the Patient PS: {resource.mcs_name}");
                                            return resource;
                                        }

                                        logger.Info($"Group Vista Clinic didn't match a standalone VC on the Patient PS, checking TMP Resource Groups for this VC: {resource.mcs_name}");

                                        // Get all TMP Resource Groups
                                        var rgSet = srv.cvt_schedulingresourceSet.Where(sr => sr.cvt_tmpresourcegroup != null && sr.cvt_participatingsite.Id == ps.Id).ToList();

                                        foreach (var record in rgSet)
                                        {
                                            // Loop through Resource Groups and get all Group Resources
                                            var groupResources = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == record.cvt_tmpresourcegroup.Id).ToList();

                                            foreach (var item in groupResources)
                                            {
                                                // Loop through all Group Resources and check for this particular VistA Clinic TMP Resource
                                                if (item.mcs_RelatedResourceId?.Id == resource.Id)
                                                {
                                                    logger.Info($"Group Vista Clinic matched a VC within a TMP Resource Group on the Patient PS. Resource: {resource.mcs_name}");
                                                    return resource;
                                                }
                                                else
                                                    logger.Info($"Group Vista Clinic did not match this group resource: {item.mcs_name}. Continuing.");
                                            }
                                            logger.Info($"Group Vista Clinic did not match any group resources within this TMP Resource Group: {record.cvt_name}. Continuing.");
                                        }
                                    }
                                }
                                else
                                {
                                    logger.Error("No Patient Particpating Site was found.");
                                    throw new MissingSiteException("No Patient Particpating Site was found.");
                                }
                            }
                            else
                            {
                                logger.Error("No Patient Site is listed");
                                throw new MissingSiteException("No Patient Site is listed");
                            }
                        }
                    }
                }
                logger.Error($"No Valid Vista Clinics were found for the {side.ToString()} side");
                throw new MissingClinicException($"No Valid Vista Clinics were found for the {side.ToString()} side");
            }
        }
    }
}