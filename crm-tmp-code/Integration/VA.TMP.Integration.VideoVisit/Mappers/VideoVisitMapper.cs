using System;
using System.Collections.Generic;
using Ec.VideoVisit.Messages;
using log4net;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;

namespace VA.TMP.Integration.VideoVisit.Mappers
{
    /// <summary>
    /// Class to map a Service Appointment to VVS Appointment.
    /// </summary>
    internal class VideoVisitMapper
    {
        private readonly IOrganizationService _organizationService;
        private readonly ServiceAppointment _serviceAppointment;
        private readonly List<Guid> _contacts;
        private readonly List<SystemUser> _systemUser;
        private readonly Appointment _appointment;
        private readonly bool _isGroup;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="organizationService">CRM Organization Service.</param>
        /// <param name="serviceAppointment">CRM Service Appointment.</param>
        /// <param name="contacts">CRM Contacts.</param>
        /// <param name="systemUsers">CRM Sysem Users.</param>
        /// <param name="isGroup">Is group appointment.</param>
        /// <param name="appointment">Optional appointment.</param>
        public VideoVisitMapper(IOrganizationService organizationService, ServiceAppointment serviceAppointment, List<Guid> contacts, List<SystemUser> systemUsers, bool isGroup, Appointment appointment = null)
        {
            _organizationService = organizationService;
            _serviceAppointment = serviceAppointment;
            _contacts = contacts;
            _systemUser = systemUsers;
            _appointment = appointment;
            _isGroup = isGroup;
        }

        /// <summary>
        /// Map a schema Apppointment Initiation data to TMP entities.
        /// </summary>
        /// <param name="logger">The Logger Object.</param>
        /// <returns>Appointment.</returns>
        internal EcTmpCreateAppointmentRequestData Map(ILog logger)
        {
            var ecAppointmentKind = MappingResolvers.GetAppointmentKind(_serviceAppointment, _organizationService, logger);
            var createAppointmentRequest = new EcTmpCreateAppointmentRequestData
            {
                Version = "1.0",
                Id = _appointment?.Id.ToString() ?? _serviceAppointment.Id.ToString(), //1 appointment
                Duration = _serviceAppointment.ScheduledDurationMinutes ?? 0,
                DateTime = _serviceAppointment.ScheduledStart.ToString(),
                SchedulingRequestType = EcTmpSchedulingRequestType.OTHER_THAN_NEXT_AVAILABLE_PATIENT_REQUESTED,
                SchedulingRequestTypeSpecified = true,
                Type = EcTmpAppointmentType.REGULAR,
                TypeSpecified = true,
                DesiredDate = _serviceAppointment.ScheduledStart.ToString(),
                DesiredDateSpecified = true,
                AppointmentKind = ecAppointmentKind.ToString(),
                SourceSystem = "TMP",
                SamlToken = ""
            };

            if (HasBookingNotes(_serviceAppointment)) createAppointmentRequest.BookingNotes = _serviceAppointment.cvt_SchedulingInstructions.Length > 150 ? _serviceAppointment.cvt_SchedulingInstructions.Substring(0, 150) : _serviceAppointment.cvt_SchedulingInstructions;

            createAppointmentRequest.Patients = MappingResolvers.ResolveContacts(_contacts, _organizationService, _serviceAppointment, ecAppointmentKind,
                _isGroup, _isGroup && (ecAppointmentKind != EcTmpAppointmentKind.MOBILE_ANY && ecAppointmentKind != EcTmpAppointmentKind.MOBILE_GFE) ? _appointment.Id : Guid.Empty, logger).ToArray();
            createAppointmentRequest.Providers = MappingResolvers.MapProviders(_systemUser, _organizationService, _serviceAppointment, ecAppointmentKind, logger).ToArray();
            return createAppointmentRequest;
        }

        internal EcTmpUpdateAppointmentRequest MapUpdate(EcTmpCreateAppointmentRequestData source)
        {
            return new EcTmpUpdateAppointmentRequest
            {
                AppointmentKind = source.AppointmentKind,
                BookingNotes = source.BookingNotes,
                DateTime = source.DateTime,
                DesiredDate = source.DesiredDate,
                DesiredDateSpecified = source.DesiredDateSpecified,
                Duration = source.Duration,
                Id = source.Id,
                Patients = source.Patients,
                Providers = source.Providers,
                SchedulingRequestType = source.SchedulingRequestType,
                SchedulingRequestTypeSpecified = source.SchedulingRequestTypeSpecified,
                SourceSystem = source.SourceSystem,
                Status = source.Status,
                Type = source.Type,
                TypeSpecified = source.TypeSpecified,
                Version = source.Version,
                SamlToken = ""
            };
        }

        internal EcTmpUpdateAppointmentRequest MapUpdate(ILog logger)
        {
            logger.Debug("Map update begin");

            var ecAppointmentKind = MappingResolvers.GetAppointmentKind(_serviceAppointment, _organizationService, logger);
            logger.Debug("after apppoinment kind");

            var updateAppointmentRequest = new EcTmpUpdateAppointmentRequest
            {
                Version = "1.0",
                Id = _appointment?.Id.ToString() ?? _serviceAppointment.Id.ToString(), //1 appointment
                Duration = _serviceAppointment.ScheduledDurationMinutes ?? 0,
                DateTime = _serviceAppointment.ScheduledStart.ToString(),
                SchedulingRequestType = EcTmpSchedulingRequestType.OTHER_THAN_NEXT_AVAILABLE_PATIENT_REQUESTED,
                SchedulingRequestTypeSpecified = true,
                Type = EcTmpAppointmentType.REGULAR,
                TypeSpecified = true,
                DesiredDate = _serviceAppointment.ScheduledStart.ToString(),
                DesiredDateSpecified = true,
                AppointmentKind = ecAppointmentKind.ToString(),
                SourceSystem = "TMP",
                SamlToken = ""
            };

            if (HasBookingNotes(_serviceAppointment)) updateAppointmentRequest.BookingNotes = _serviceAppointment.cvt_SchedulingInstructions;
            if (_contacts == null)
                logger.Debug("Contact is null ");
            else
                logger.Debug("Contact is not null");

            if (_systemUser == null)
                logger.Debug("Systemuser is null ");
            else
                logger.Debug("Systemuser is not null");

            if (_appointment == null)
                logger.Debug("Appointment is null ");
            else
                logger.Debug("Appointment is not null");

            if (updateAppointmentRequest.Id == null)
                logger.Debug("update appointment is  null");

            else
                logger.Debug($"update appointment is not  null -  {updateAppointmentRequest.Id}");

            var resolveContact = MappingResolvers.ResolveContacts(_contacts, _organizationService, _serviceAppointment, ecAppointmentKind,
                _isGroup, Guid.Empty, logger);


            if (resolveContact == null)
                logger.Debug("REsolve contact is null ");
            else
                logger.Debug("REsolve contact is not null");

            updateAppointmentRequest.Patients = resolveContact.ToArray();

            var resolveProvider = MappingResolvers.MapProviders(_systemUser, _organizationService, _serviceAppointment, ecAppointmentKind, logger);


            if (resolveProvider == null)
                logger.Debug("REsolve Provider is null ");
            else
                logger.Debug("REsolve Provider is not null");

            updateAppointmentRequest.Providers = resolveProvider.ToArray();
            return updateAppointmentRequest;
        }

        /// <summary>
        /// Determine whether the Service Appointment has booking notes.
        /// </summary>
        /// <param name="serviceAppointment">Service Appointment.</param>
        /// <returns>Whether the Service Appointment has booking notes.</returns>
        private static bool HasBookingNotes(ServiceAppointment serviceAppointment)
        {
            return !string.IsNullOrEmpty(serviceAppointment.cvt_SchedulingInstructions);
        }
    }
}
