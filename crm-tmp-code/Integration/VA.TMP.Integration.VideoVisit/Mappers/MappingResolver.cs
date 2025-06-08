using System;
using System.Collections.Generic;
using System.Linq;
using Ec.VideoVisit.Messages;
using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.VideoVisit.Mappers
{
    /// <summary>
    /// Default Mapping Resolvers class to hold all Resolver functions. 
    /// </summary>
    internal static class MappingResolvers
    {
        /// <summary>
        /// Person Identifier Resolver Function
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static EcTmpPersonIdentifier PersonIdentifierResolver(IOrganizationService organizationService, Contact source)
        {
            using (var srv = new Xrm(organizationService))
            {
                var identifiers = srv.mcs_personidentifiersSet.Where(x => x.mcs_patient.Id == source.Id).ToList();
                var icn = identifiers.FirstOrDefault(x => x.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI && x.mcs_assigningauthority == "USVHA");
                if (icn == null) throw new PatientIcnException("Patient has no ICN: " + source.FullName);
                if (string.IsNullOrEmpty(icn.mcs_identifier)) throw new PatientIcnException("ICN is empty");
                var icnString = icn.mcs_identifier;
                return new EcTmpPersonIdentifier { AssigningAuthority = "ICN", UniqueId = icnString };
            }
        }

        /// <summary>
        /// Person Name Resolver Function
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static EcTmpPersonName PersonNameResolver(Contact source)
        {
            return new EcTmpPersonName { FirstName = source.FirstName, LastName = source.LastName };
        }

        /// <summary>
        /// Patient Contact Info Resolver Function
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static EcTmpContactInformation PatientContactInformationResolver(Contact source)
        {
            var contactInformation = new EcTmpContactInformation
            {
                Mobile = source.Telephone1,
                PreferredEmail = source.EMailAddress1
            };

            if (source.cvt_TimeZone == null) return contactInformation;

            contactInformation.TimeZoneSpecified = true;
            contactInformation.TimeZone = Convert.ToString(source.cvt_TimeZone.Value);

            return contactInformation;
        }

        /// <summary>
        /// Patient Virutal Meeting Room Resolver Function
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static EcTmpVirtualMeetingRoom PatientVirtualMeetingRoomResolver(ServiceAppointment source)
        {
            return string.IsNullOrEmpty(source.mcs_meetingroomname) && string.IsNullOrEmpty(source.mcs_patientpin) && string.IsNullOrEmpty(source.mcs_PatientUrl)
                ? null
                : new EcTmpVirtualMeetingRoom
                {
                    Conference = source.mcs_meetingroomname,
                    Pin = source.mcs_patientpin,
                    Url = source.mcs_PatientUrl
                };
        }

        /// <summary>
        /// Provider Name Resolver Function
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static EcTmpPersonName ProviderNameResolver(SystemUser source)
        {
            return new EcTmpPersonName { FirstName = source.FirstName, LastName = source.LastName };
        }

        /// <summary>
        /// Provider Contact Info Resolver Function 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static EcTmpContactInformation ProviderContactInformationResolver(SystemUser source)
        {
            var contactInformation = new EcTmpContactInformation
            {
                Mobile = source.MobilePhone,
                PreferredEmail = source.InternalEMailAddress
            };

            if (source.cvt_TimeZone == null) return contactInformation;

            contactInformation.TimeZoneSpecified = true;
            contactInformation.TimeZone = Convert.ToString(source.cvt_TimeZone.Value);


            return contactInformation;
        }

        /// <summary>
        /// Provider Location Resolver Function 
        /// </summary>
        /// <param name="organizationService">Organization Service.</param>
        /// <param name="serviceAppointment">Service Appointment.</param>
        /// <param name="source">System User.</param>
        /// <param name="ecTmpAppointmentKind">Appointment Kind.</param>
        /// <param name="logger">The Logger Object.</param>
        /// <returns>Location</returns>
        internal static EcTmpLocation ProviderLocationResolver(IOrganizationService organizationService, ServiceAppointment serviceAppointment, SystemUser source, EcTmpAppointmentKind ecTmpAppointmentKind, ILog logger)
        {
            using (var srv = new Xrm(organizationService))
            {
                // To get to facility, get the related TMP Site from the SA, then get facility from there
                mcs_site proSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == serviceAppointment.mcs_relatedprovidersite.Id);
                mcs_facility facility = null;
                if (proSite != null)
                {
                    facility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == proSite.mcs_FacilityId.Id);
                }

                if ((facility.Id == null) || (facility.Id == Guid.Empty))
                {
                    throw new MissingFacilityException("Unable to retrieve Provider Facility");
                }

                var siteCode = facility.mcs_StationNumber;
                var facilityName = facility.mcs_name;
                var timeZone = facility.mcs_Timezone ?? 0;

                var clinic = ClinicResolver(serviceAppointment, serviceAppointment.mcs_relatedprovidersite.Id, organizationService, Guid.Empty, Side.Provider, logger);

                if (string.IsNullOrEmpty(clinic.Ien) || string.IsNullOrEmpty(clinic.Name)) throw new MissingClinicException("No VistA Clinic is getting booked on the provider side (or Vista Clinic IEN is empty).  Unable to send Appointment to VistA.");

                return new EcTmpLocation
                {
                    Type = EcTmpLocationType.VA,
                    Facility = new EcTmpFacility { SiteCode = siteCode, Name = facilityName, TimeZone = Convert.ToString(timeZone) },
                    Clinic = new EcTmpClinic { Name = clinic.Name, Ien = clinic.Ien }
                };
            }
        }

        /// <summary>
        /// Provider Virutal Meeting Room Resolver Function
        /// </summary>
        /// <param name="source">Service Appointment.</param>
        /// <returns>Virtual Meeting Room.</returns>
        internal static EcTmpVirtualMeetingRoom ProviderVirtualMeetingRoomResolver(ServiceAppointment source)
        {
            return string.IsNullOrEmpty(source.mcs_meetingroomname) && string.IsNullOrEmpty(source.mcs_providerpin) && string.IsNullOrEmpty(source.mcs_providerurl)
                ? null
                : new EcTmpVirtualMeetingRoom
                {
                    Conference = source.mcs_meetingroomname,
                    Pin = source.mcs_providerpin,
                    Url = source.mcs_providerurl
                };
        }

        /// <summary>
        /// Reads List of Users and maps to List of EcTmpProviders
        /// </summary>
        /// <param name="users">List of users.</param>
        /// <param name="orgService">Organization Service for retrieving other data.</param>
        /// <param name="serviceAppointment">Service Appointment for getting location and VMR info.</param>
        /// <param name="ecTmpAppointmentKind">Appointment Kind.</param>
        /// <param name="logger">The Logger Object.</param>
        /// <returns>List of Providers.</returns>
        internal static List<EcTmpProviders> MapProviders(List<SystemUser> users, IOrganizationService orgService, ServiceAppointment serviceAppointment, EcTmpAppointmentKind ecTmpAppointmentKind, ILog logger)
        {
            if (ecTmpAppointmentKind == EcTmpAppointmentKind.STORE_FORWARD) return new List<EcTmpProviders>();

            var providerList = new List<EcTmpProviders>();

            foreach (var user in users)
            {
                var provider = new EcTmpProviders
                {
                    Name = ProviderNameResolver(user),
                    ContactInformation = ProviderContactInformationResolver(user),
                    Location = ProviderLocationResolver(orgService, serviceAppointment, user, ecTmpAppointmentKind, logger),
                    VirtualMeetingRoom = ProviderVirtualMeetingRoomResolver(serviceAppointment)
                };
                providerList.Add(provider);
            }
            return providerList;
        }

        /// <summary>
        /// Maps Contacts to Patients.
        /// </summary>
        /// <param name="contacts">List of Contacts.</param>
        /// <param name="organizationService">Organization Service.</param>
        /// <param name="serviceAppointment">Service Appointment.</param>
        /// <param name="ecTmpAppointmentKind">Appointment Kind.</param>
        /// <param name="isGroup">Is Group.</param>
        /// <param name="apptId">Appointment Id.</param>
        /// <param name="logger">The Logger Object.</param>
        /// <returns>List of Patients.</returns>
        internal static List<EcTmpPatients> ResolveContacts(List<Guid> contacts, IOrganizationService organizationService, ServiceAppointment serviceAppointment, EcTmpAppointmentKind ecTmpAppointmentKind, bool isGroup, Guid apptId, ILog logger)
        {
            var patients = new List<EcTmpPatients>();

            foreach (var contactId in contacts)
            {
                var contact = (Contact)organizationService.Retrieve(Contact.EntityLogicalName, contactId, new ColumnSet(true));
                var pat = new EcTmpPatients
                {
                    Id = PersonIdentifierResolver(organizationService, contact),
                    Name = PersonNameResolver(contact),
                    ContactInformation = PatientContactInformationResolver(contact),
                };
                if (!string.IsNullOrEmpty(serviceAppointment.mcs_meetingroomname)) pat.VirtualMeetingRoom = PatientVirtualMeetingRoomResolver(serviceAppointment);
                pat.Location = PatientLocationResolver(ecTmpAppointmentKind, serviceAppointment, isGroup, apptId, organizationService, logger);
                pat.HasMobileGFE = contact.Contains("cvt_tablettype") && contact.cvt_TabletType.Value.Equals(917290002);

                // Add in Facility and Clinic as optional
                patients.Add(pat);
            }
            return patients;
        }

        /// <summary>
        /// Maps Patient facility.
        /// </summary>
        /// <param name="ecTmpAppointmentKind">Appointment Kind.</param>
        /// <param name="serviceAppointment">Service Appointment.</param>
        /// <param name="isGroup">Is Group.</param>
        /// <param name="apptId">Appointment Id.</param>
        /// <param name="organizationService">Organization Service.</param>
        /// <param name="logger">The Logger Object.</param>
        /// <returns>Location.</returns>
        internal static EcTmpLocation PatientLocationResolver(EcTmpAppointmentKind ecTmpAppointmentKind, ServiceAppointment serviceAppointment, bool isGroup, Guid apptId, IOrganizationService organizationService, ILog logger)
        {
            if (ecTmpAppointmentKind == EcTmpAppointmentKind.MOBILE_GFE || ecTmpAppointmentKind == EcTmpAppointmentKind.MOBILE_ANY) return new EcTmpLocation { Type = EcTmpLocationType.NonVA };
            Guid siteId = Guid.Empty;

            using (var srv = new Xrm(organizationService))
            {
                mcs_site patSite = null;
                mcs_facility patFacility = null;
                Side side = Side.Patient;

                if (isGroup)
                {
                    var appointment = organizationService.Retrieve(Appointment.EntityLogicalName, apptId, new ColumnSet(true)).ToEntity<Appointment>();
                    siteId = appointment.cvt_Site?.Id ?? Guid.Empty;
                    var site = srv.mcs_siteSet.FirstOrDefault(s => s.Id == siteId);
                    patFacility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == site.mcs_FacilityId.Id);
                }
                else if (serviceAppointment.mcs_relatedsite != null)
                {
                    // Now we have to get the TMP_Site before we can get to Facility
                    patSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == serviceAppointment.mcs_relatedsite.Id);
                    patFacility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == patSite.mcs_FacilityId.Id);

                    siteId = serviceAppointment.mcs_relatedsite?.Id ?? Guid.Empty;
                }
                else if (ecTmpAppointmentKind == EcTmpAppointmentKind.CLINIC_BASED)
                {
                    var participantSite = srv.cvt_participatingsiteSet.FirstOrDefault(x => x.cvt_locationtype != null && x.cvt_resourcepackage != null
                                                                                        && x.cvt_resourcepackage.Id == serviceAppointment.cvt_relatedschedulingpackage.Id
                                                                                        && x.cvt_locationtype.Value == 917290001); //Patient
                    if (participantSite != null)
                    {
                        patSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == participantSite.cvt_site.Id);
                        patFacility = srv.mcs_facilitySet.FirstOrDefault(f => f.Id == patSite.mcs_FacilityId.Id);
                        siteId = patSite.Id;
                        side = Side.Provider;
                    }
                }

                if (patFacility == null) throw new MissingFacilityException("Patient Facility cannot be null");

                var location = new EcTmpLocation
                {
                    Type = EcTmpLocationType.VA,
                    Clinic = ClinicResolver(serviceAppointment, siteId, organizationService, apptId, side, logger),
                    Facility = new EcTmpFacility
                    {
                        Name = patFacility.mcs_name,
                        SiteCode = patFacility.mcs_StationNumber,
                        TimeZone = Convert.ToString(patFacility.mcs_Timezone) // ?? 0
                    }
                };

                return location;
            }
        }

        /// <summary>
        /// Maps Clinic.
        /// </summary>
        /// <param name="serviceAppointment">Service Appointment.</param>
        /// <param name="organizationService">Organization Service.</param>
        /// <param name="apptId">Appointment Id.</param>
        /// <param name="side">The side</param>
        /// <param name="logger">The Logger Object.</param>
        /// <returns>Clinic.</returns>
        internal static EcTmpClinic ClinicResolver(ServiceAppointment serviceAppointment, Guid siteId, IOrganizationService organizationService, Guid apptId, Side side, ILog logger)
        {
            var clinicName = string.Empty;
            var clinicIen = string.Empty;

            var resource = GetClinic(serviceAppointment, organizationService, apptId, side, logger);
            if (resource != null)
            {
                clinicName = resource.mcs_UserNameInput;
                clinicIen = resource.cvt_ien;
            }

            return new EcTmpClinic { Name = clinicName, Ien = clinicIen };
        }

        /// <summary>
        /// Gets the Resource for the given Patient or Provider.
        /// </summary>
        /// <param name="serviceAppointment">The Service Appointment object.</param>
        /// <param name="organizationService">The Org Service object</param>
        /// <param name="apptId">The appointment (reserve resource) record Id in case associated</param>
        /// <param name="side">The Patient or Provider side</param>
        /// <param name="logger">The Logger Object.</param>
        /// <returns>Resource.</returns>
        internal static mcs_resource GetClinic(ServiceAppointment serviceAppointment, IOrganizationService organizationService, Guid apptId, Side side, ILog logger)
        {
            //The logic here is same as mapping Get clinic in Make/Cancel appointments defined under VA.TMP.Integration.HealthShare\PipelineSteps\MakeCancelOutbound\20 - GetClinics.cs
            //Todo: Move this logic to a common helper function which can be invoked from here as well as from Make/Cancel appointment
            using (var srv = new Xrm(organizationService))
            {
                Appointment appointment = null;
                var apptType = serviceAppointment.cvt_Type.Value ? "VA Video Connect" : "Clinic Based";

                var bookedResources = serviceAppointment.Resources.Where(r => r.PartyId.LogicalName == "equipment").ToList();

                if (apptId != Guid.Empty)
                {
                    appointment = srv.AppointmentSet.FirstOrDefault(a => a.Id == apptId);
                    if (appointment?.RequiredAttendees != null && side == Side.Patient && serviceAppointment.mcs_groupappointment.HasValue && serviceAppointment.mcs_groupappointment.Value)
                    {
                        bookedResources.AddRange(appointment.RequiredAttendees.Where(r => r.PartyId.LogicalName == "equipment").ToList());
                    }
                }

                if (bookedResources.Count < 1) throw new MissingClinicException($"No Valid Vista Clinics were found for the {side.ToString()} side");

                foreach (var equipmentParty in bookedResources)
                {
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
                            logger.Info($"***** Side = Provider, AppointmentType: {apptType}, IsGroup: {serviceAppointment.mcs_groupappointment}, RelatedProviderSite Null? {serviceAppointment.mcs_relatedprovidersite == null}");

                            if (serviceAppointment.mcs_relatedprovidersite != null)
                            {
                                // Find Participating Site
                                var providerPs = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.cvt_site.Id == serviceAppointment.mcs_relatedprovidersite.Id &&
                                    ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Provider &&
                                    ps.cvt_resourcepackage.Id == serviceAppointment.cvt_relatedschedulingpackage.Id);

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
                            logger.Info($"***** Side = Patient, AppointmentType: {apptType}, IsGroup: {serviceAppointment.mcs_groupappointment}, RelatedSite Null? {serviceAppointment.mcs_relatedsite == null}");
                            if (!serviceAppointment.mcs_groupappointment.Value && serviceAppointment.mcs_relatedsite != null)
                            {
                                // Find Participating Site
                                var patientPs = srv.cvt_participatingsiteSet.FirstOrDefault(ps => ps.cvt_site.Id == serviceAppointment.mcs_relatedsite.Id &&
                                                                                            ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient &&
                                                                                            ps.cvt_resourcepackage.Id == serviceAppointment.cvt_relatedschedulingpackage.Id);

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
                            else if (serviceAppointment.mcs_groupappointment.HasValue && serviceAppointment.mcs_groupappointment.Value && appointment?.cvt_Site != null)
                            {
                                // Find Participating Site
                                var patientPs = srv.cvt_participatingsiteSet.Where(ps => ps.cvt_scheduleable.Value &&
                                    ps.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient &&
                                    ps.cvt_resourcepackage.Id == serviceAppointment.cvt_relatedschedulingpackage.Id).ToList();

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

        /// <summary>
        /// Gets Appointment Kind.
        /// </summary>
        /// <param name="serviceAppointment">Service Appointment.</param>
        /// <param name="organizationService">Organization Service.</param>
        /// <returns>Appointment Kind.</returns>
        internal static EcTmpAppointmentKind GetAppointmentKind(ServiceAppointment serviceAppointment, IOrganizationService organizationService, ILog logger)
        {
            var apptModality = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
            if (apptModality.Value == 178970008) return EcTmpAppointmentKind.MOBILE_ANY;

            if (serviceAppointment.cvt_TelehealthModality != null && serviceAppointment.cvt_TelehealthModality.Value) return EcTmpAppointmentKind.STORE_FORWARD;
            if (serviceAppointment.cvt_Type != null && !serviceAppointment.cvt_Type.Value) return EcTmpAppointmentKind.CLINIC_BASED;

            //Default to MOBILE_ANY for all others
            //VVS has deprecated the MOBILE_GFE AppointmentKind
            if (serviceAppointment.Customers == null || 
                serviceAppointment.Customers.Count().Equals(0) || 
                serviceAppointment.Customers?.FirstOrDefault() == null) 
                    throw new MissingPatientException($"No Patients are listed on the service activity {serviceAppointment.Id} named {serviceAppointment.Subject}");

            return EcTmpAppointmentKind.MOBILE_ANY;
        }

        private static bool IsMobileAnyOrGFE(ServiceAppointment sa, IOrganizationService organizationService, ILog logger)
        {
            if (sa.Customers == null || sa.Customers.Count().Equals(0) || sa.Customers?.FirstOrDefault() == null) throw new MissingPatientException(string.Format("No Patients are listed on the service activity {0} named {1}", sa.Id, sa.Subject));

            //ALWAYS takes the First Customer in the list that is automatically sorted alphabetically, by first name.  This can potentially affect Group Appointments.
            var patientAp = sa.Customers.FirstOrDefault();
            if (patientAp == null || patientAp.PartyId == null || patientAp.PartyId.Id.Equals(Guid.Empty)) throw new MissingPatientException("Unable to find Customer.");

            var p = organizationService.Retrieve("contact", patientAp.PartyId.Id, new ColumnSet(new string[] { "cvt_tablettype", "cvt_bltablet", "donotemail", "cvt_staticvmrlink" }));

            if (p == null) throw new MissingPatientException(string.Format("No patient was found with Id: {0}, AP.ActivityId: {1}", patientAp.PartyId.Id, patientAp.ActivityId.Id));

            if (p.Contains("cvt_tablettype")) logger.Debug($"Tablet Type: {p.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value}");
            logger.Debug($"Do Not Email: {p.GetAttributeValue<bool>("donotemail")}");
            logger.Debug($"Static VMR Link: {p.GetAttributeValue<string>("cvt_staticvmrlink")}");
            logger.Debug($"SIP Address: {p.GetAttributeValue<string>("cvt_bltablet")}");

            var hasTablet = ((p.Contains("cvt_tablettype") &&
                            p.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == (int)Contactcvt_TabletType.VALoanedDevice) &&
                            p.GetAttributeValue<bool>("donotemail") && !string.IsNullOrEmpty(p.GetAttributeValue<string>("cvt_staticvmrlink").Trim())) ||
                            ((p.Contains("cvt_tablettype") &&
                            p.GetAttributeValue<OptionSetValue>("cvt_tablettype").Value == (int)Contactcvt_TabletType.SIPDevice) &&
                            p.GetAttributeValue<bool>("donotemail") && !string.IsNullOrEmpty(p.GetAttributeValue<string>("cvt_bltablet").Trim()));

            logger.Debug($"Has Tablet: {hasTablet}");

            return hasTablet;
        }

        private static bool IsGfeServiceActivity(ServiceAppointment sa, IOrganizationService organizationService)
        {
            if (sa.Customers?.FirstOrDefault() == null) throw new MissingPatientException(string.Format("No Patients are listed on the service activity {0} named {1}", sa.Id, sa.Subject));

            var patientAp = sa.Customers.FirstOrDefault();
            if (patientAp == null) throw new MissingPatientException("Unable to find Customer.");

            Contact patient;
            using (var srv = new Xrm(organizationService))
                patient = srv.ContactSet.FirstOrDefault(c => c.Id == patientAp.PartyId.Id);

            if (patient == null) throw new MissingPatientException(string.Format("No patient was found with Id: {0}, AP.ActivityId: {1}", patientAp.PartyId.Id, patientAp.ActivityId.Id));
            var hasTablet = !string.IsNullOrEmpty(patient.cvt_bltablet);

            return hasTablet;
        }
    }
}
