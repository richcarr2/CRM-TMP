using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Ec.HealthShare.Messages;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.Mappers
{
    /// <summary>
    /// Make Cancel Outbound Mapper.
    /// </summary>
    internal class MakeCancelOutboundEcRequestMapper
    {
        private readonly MakeCancelOutboundStateObject _state;
        private readonly EcHealthShareMakeCancelOutboundRequestMessage _ecRequest;
        private readonly Guid _patientId;
        private readonly ILog _logger;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <param name="state">State object.</param>
        /// <param name="patientId">PatientId.</param>
        internal MakeCancelOutboundEcRequestMapper(MakeCancelOutboundStateObject state, Guid patientId, ILog logger)
        {
            _state = state;
            _patientId = patientId;
            _ecRequest = new EcHealthShareMakeCancelOutboundRequestMessage();
            _logger = logger;
        }

        /// <summary>
        /// Maps the State object to EC Request Message.
        /// </summary>
        /// <returns>EcHealthShareMakeCancelOutboundRequestMessage.</returns>
        internal EcHealthShareMakeCancelOutboundRequestMessage Map()
        {
            _logger.Info("***Mapping - Begin Initial Map");
            _ecRequest.ControlId = RandomDigits.GetRandomDigitString(20);
            _ecRequest.OrganizationName = _state.OrganizationName;
            _ecRequest.SignOnFacility = GetSignOnFacility();
            _ecRequest.SecondFacility = GetSecondFacility();
            _ecRequest.PatientIcn = GetPatientIcn();
            _ecRequest.PatientName = GetPatientName();
            _ecRequest.Duration = _state.ServiceAppointment.ScheduledDurationMinutes.ToString();
            _ecRequest.StartTime = GetStartTime();
            _ecRequest.VisitStatus = _state.RequestMessage.VisitStatus.ToUpper();

            //Refer to VistaIntegrationResult.cvt_CancelReason when a individual patient is cancelled. In case of cancellation whole appointment IntegrationBookingStatus field on appointment has the cancel reson updated by the dialog process. 
            if (_state.IsGroupAppointment && !_state.IsMakeAppointment && (_state.VistaIntegrationResult?.cvt_CancelReason == null && _state.Appointment.cvt_IntegrationBookingStatus == null))
                throw new GroupCancelMappingException($"Group Cancel: Cannot find Cancel Reason information from appointment : {_state.Appointment.Id} or Vista Integration Result: {_state.VistaIntegrationResult.Id}");

            _logger.Info("***Mapping - Before CancelReason Logic.");
            _logger.Info("***Mapping - _state.ServiceAppointmentId: " + _state.ServiceAppointmentId.ToString());
            _logger.Info("***Mapping - _state.IsMakeAppointment: " + _state.IsMakeAppointment.ToString());
            _logger.Info("***Mapping - _state.IsGroupAppointment: " + _state.IsGroupAppointment.ToString());
            _logger.Info("***Mapping - _state.GroupAppointment: " + _state.GroupAppointment.ToString());
            //if (_state.ServiceAppointmentId != null)
            //    _ecRequest.CancelReason = _state.IsMakeAppointment ? string.Empty : _state.ServiceAppointment.cvt_ReasonforCancelation;
            //else

            _ecRequest.CancelReason = _state.IsMakeAppointment ? string.Empty : _state.IsCancelAppointment || _state.IsGroupAppointment ? _state.ServiceAppointment.Contains("tmp_reasonforcancelation") ? _state.ServiceAppointment.GetAttributeValue<string>("tmp_reasonforcancelation") : (MapCancelReasonToVista(_state.VistaIntegrationResult.cvt_CancelReason?.Value ?? _state.Appointment.cvt_IntegrationBookingStatus.Value, _logger)) : MapCancelReasonToVista(_state.ServiceAppointment.StatusCode.Value, _logger);
            _logger.Info("***Mapping - After CancelReason Logic.");

            _ecRequest.CancelCode = _state.IsMakeAppointment ? string.Empty : _state.IsGroupAppointment || _state.GroupAppointment ? MapCancelCodeStringToVista(_state.ServiceAppointment.cvt_cancelreason ?? MapCancelCodeToVista(_state.Appointment.cvt_IntegrationBookingStatus.Value, _logger), _logger) : MapCancelCodeToVista(_state.ServiceAppointment.StatusCode.Value, _logger);
            //_ecRequest.CancelRemarks = _state.IsMakeAppointment ? string.Empty : _state.IsGroupAppointment ? ((cvt_vistaintegrationresultcvt_CancelReason)(_state.VistaIntegrationResult.cvt_CancelReason?.Value ?? _state.Appointment.cvt_IntegrationBookingStatus.Value)).ToString() : ((serviceappointment_statuscode)_state.ServiceAppointment.StatusCode.Value).ToString();
            _ecRequest.CancelRemarks = _state.IsMakeAppointment ? string.Empty : _state.IsGroupAppointment ? _state.Appointment.cvt_CancelRemarks ?? _state.ServiceAppointment.cvt_cancelremarks : _state.ServiceAppointment.cvt_cancelremarks;

            _ecRequest.SchedulerSecId = GetSchedulerSecId();
            _ecRequest.SchedulerEmail = GetUserEmail(_state.RequestMessage.UserId);
            _ecRequest.Comments = _state.ServiceAppointment.cvt_comments;
            _ecRequest.VvdUrl = _state.ServiceAppointment.mcs_providerurl;
            _ecRequest.ClinicallyIndicatedDate = _state.ServiceAppointment.cvt_clinicallyindicateddate;
            _logger.Info("***Mapping - End Initial Map");

            if (_state.AppointmentType != AppointmentType.HOME_MOBILE) _ecRequest.Patient.Add(AddPatients());
            if (_state.AppointmentType != AppointmentType.STORE_FORWARD) _ecRequest.Provider.Add(AddProviders());

            // In this case where both sides aren't being mapped HealthShare needs an empty object for its HL7 mapping.
            if (_state.AppointmentType == AppointmentType.HOME_MOBILE) _ecRequest.Patient.Add(new Patient());
            if (_state.AppointmentType == AppointmentType.STORE_FORWARD) _ecRequest.Provider.Add(new Provider());

            // In the case of group or clinic based appointments if provider and patient station number are the same then only populate patient side or provider side.
            if (_state.AppointmentType == AppointmentType.CLINIC_BASED || _state.AppointmentType == AppointmentType.GROUP)
            {
                var patientFacility = _ecRequest.Patient.Any() ? _ecRequest.Patient.First().Facility : string.Empty;
                var providerFacility = _ecRequest.Provider.Any() ? _ecRequest.Provider.First().Facility : string.Empty;

                _logger.Info($"***Patient Facility {patientFacility}. Provider Facility {providerFacility}");

                if (string.IsNullOrEmpty(patientFacility) && string.IsNullOrEmpty(providerFacility)) throw new MissingFacilityException("Patient and Provider Clinic cannot both be null");

                if (patientFacility == providerFacility)
                {
                    _ecRequest.SecondFacility = string.Empty;
                    _ecRequest.Patient.First().Facility = $"{_ecRequest.Patient.First().Facility}A";
                    _ecRequest.Provider.First().Facility = $"{_ecRequest.Provider.First().Facility}B";
                }
            }

            return _ecRequest;
        }

        /// <summary>
        /// Get Scheduler's SignOn Facility.
        /// </summary>
        /// <returns>Scheduler's Station Number.</returns>
        private string GetSignOnFacility()
        {
            _logger.Info("***Mapping - GetSignOnFacility");
            using (var srv = new Xrm(_state.OrganizationServiceProxy))
            {
                //var loginSites = srv.cvt_userduzSet.Where(x =>
                //    x.cvt_User.Id == _state.RequestMessage.UserId &&
                //    x.statecode == cvt_userduzState.Active).Select(x => x.cvt_StationNumber).ToList();

                //var hasPatientDuz = _state.PatientFacility != null ?
                //    loginSites.Any(x => x == _state.PatientFacility.mcs_StationNumber) : false;

                //var hasProviderDuz = _state.ProviderFacility != null ?
                //    loginSites.Any(x => x == _state.ProviderFacility.mcs_StationNumber) : false;

                //if (_state.AppointmentType == AppointmentType.HOME_MOBILE && hasProviderDuz) return _state.ProviderFacility.mcs_StationNumber;
                //else if (_state.AppointmentType == AppointmentType.STORE_FORWARD && hasPatientDuz) return _state.PatientFacility.mcs_StationNumber;
                //else if (hasPatientDuz || hasProviderDuz) return _state.PatientFacility.mcs_StationNumber;

                if (_state.AppointmentType == AppointmentType.HOME_MOBILE) return _state.ProviderFacility.mcs_StationNumber;
                else if (_state.AppointmentType == AppointmentType.STORE_FORWARD) return _state.PatientFacility.mcs_StationNumber;
                else return _state.PatientFacility.mcs_StationNumber;

                throw new SignOnFacilityMappingException($"Unable to get SignOn Facility. User {_state.RequestMessage.UserId} is not eligible to login to the Vista Site");
            }
        }

        /// <summary>
        /// Get Scheduler's Second Facility.
        /// </summary>
        /// <returns>Scheduler's second Station Number.</returns>
        private string GetSecondFacility()
        {
            _logger.Info("***Mapping - GetSecondFacility");

            if (_state.AppointmentType == AppointmentType.CLINIC_BASED || _state.AppointmentType == AppointmentType.GROUP) return _state.ProviderFacility.mcs_StationNumber;
            else return string.Empty;
        }

        /// <summary>
        /// Get Patient ICN.
        /// </summary>
        /// <returns>Patient ICN.</returns>
        private string GetPatientIcn()
        {
            _logger.Info("***Mapping - GetPatientIcn");
            using (var srv = new Xrm(_state.OrganizationServiceProxy))
            {
                var personIdentifier = srv.mcs_personidentifiersSet.FirstOrDefault(x =>
                    x.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI &&
                    x.mcs_assigningauthority == "USVHA" &&
                    x.mcs_patient.Id == _patientId);

                if (personIdentifier == null) throw new PatientIcnException($"The Patient {_patientId} does not have an ICN");

                return personIdentifier.mcs_identifier;
            }
        }

        /// <summary>
        /// Get Patient Name.
        /// </summary>
        /// <returns>Patient Name.</returns>
        private string GetPatientName()
        {
            _logger.Info("***Mapping - GetPatientName");
            using (var srv = new Xrm(_state.OrganizationServiceProxy))
            {
                var veteran = srv.ContactSet.FirstOrDefault(x => x.Id == _patientId);
                if (veteran == null) throw new MissingPatientException($"Unable to find Patient {_patientId}");

                var firstName = veteran.FirstName;
                var lastName = veteran.LastName;
                var middleName = veteran.MiddleName;
                var middleInitial = string.IsNullOrEmpty(middleName) ? string.Empty : middleName.Substring(0, 1);

                return $"{lastName},{firstName} {middleInitial}".TrimEnd();
            }
        }

        /// <summary>
        /// Get Start Time.
        /// </summary>
        /// <returns>Appointment Start Time.</returns>
        private string GetStartTime()
        {
            _logger.Info("***Mapping - GetStartTime");
            if (!_state.ServiceAppointment.ScheduledStart.HasValue) throw new StartTimeMappingException($"Appointment {_state.ServiceAppointment.Id} does not have a start time.");

            return _state.ServiceAppointment.ScheduledStart.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        /// <summary>
        /// Maps the TMP Status Reason to the corresponding Vista Cancel Code
        /// </summary>
        /// <param name="tmpStatusReason"></param>
        /// <returns></returns>
        public static string MapCancelCodeToVista(int tmpStatusReason, ILog logger)
        {
            logger.Info("***Mapping - MapCancelCodeToVista");
            string cancelCode;
            switch (tmpStatusReason)
            {
                case 917290001: // Technology Failure
                case 917290008: // Scheduling Error
                case 917290000: // Clinic Canceled
                    cancelCode = "C";
                    break;
                case 9: // Patient Canceled
                    cancelCode = "PC";
                    break;
                case 10: // Patient No Show
                    cancelCode = "N";
                    break;
                default:
                    cancelCode = "C";
                    break;
            }
            return cancelCode;
        }

        public static string MapCancelCodeStringToVista(string tmpStatusReason, ILog logger)
        {
            logger.Info("***Mapping - MapCancelCodeToVista");
            string cancelCode;
            switch (tmpStatusReason)
            {
                case "Patient Canceled":
                    cancelCode = "PC";
                    break;
                case "Clinic Canceled":
                    cancelCode = "C";
                    break;
                default:
                    cancelCode = "C";
                    break;
            }
            return cancelCode;
        }

        /// <summary>
        /// Maps the TMP Status Reason to the corresponding Vista Cancel Reason
        /// </summary>
        /// <param name="tmpStatusReason"></param>
        /// <returns></returns>
        public static string MapCancelReasonToVista(int tmpStatusReason, ILog logger)
        {
            logger.Info("***Mapping - MapCancelReasonToVista");
            string cancelReason;
            switch (tmpStatusReason)
            {
                case 917290000:
                    cancelReason = "CLINIC CANCELLED";
                    break;
                case 917290008:
                    cancelReason = "SCHEDULING CONFLICT/ERROR";
                    break;
                case 917290001:
                case 9:
                case 10:
                    cancelReason = "OTHER";
                    break;
                default:
                    cancelReason = "OTHER";
                    break;
            }
            return cancelReason;
        }

        /// <summary>
        /// Get Scheduler Name.
        /// </summary>
        /// <returns>Scheduler Name.</returns>
        private string GetSchedulerSecId()
        {
            _logger.Info("***Mapping - GetSchedulerSecId");
            using (var srv = new Xrm(_state.OrganizationServiceProxy))
            {
                var scheduler = srv.SystemUserSet.FirstOrDefault(x => x.Id == _state.RequestMessage.UserId);
                if (scheduler == null) throw new SchedulerNameMappingException($"Scheduler {_state.RequestMessage.UserId} cannot be null");

                var firstName = scheduler.FirstName;
                var lastName = scheduler.LastName;
                var middleName = scheduler.MiddleName;
                var middleInitial = string.IsNullOrEmpty(middleName) ? string.Empty : middleName.Substring(0, 1);

                return $"{lastName}, {firstName} {middleInitial}".TrimEnd();
            }
        }

        /// <summary>
        /// Add Patients.
        /// </summary>
        /// <returns>Patient(s).</returns>
        private Patient AddPatients()
        {
            _logger.Info("***Mapping - AddPatients");
            _logger.Info($"Patient Facility Null? {_state.PatientFacility == null}");
            _logger.Info($"Patient Clinic Null? {_state.PatientClinic == null}");
            return new Patient
            {
                Facility = _state.PatientFacility.mcs_StationNumber,
                ClinicIen = _state.PatientClinic.cvt_ien,
                ClinicDefaultProviderDuz = _state.PatientClinic.cvt_defaultproviderduz,
                ProviderEmail = GetUserEmail(_state.PatientClinic.cvt_defaultprovider?.Id),
                ConsultId = _state.ServiceAppointment.cvt_PatConsultIen,
                ConsultTitle = _state.ServiceAppointment.cvt_patconsulttitle,
                RtcId = _state.ServiceAppointment.cvt_patrtcid,
                RtcParent = _state.ServiceAppointment.cvt_rtcparentpatient
            };
        }

        /// <summary>
        /// Add Providers.
        /// </summary>
        private Provider AddProviders()
        {
            _logger.Info("***Mapping - AddProviders");
            _logger.Info($"Provider Facility Null? {_state.ProviderFacility == null}");
            _logger.Info($"Provider Clinic Null? {_state.ProviderClinic == null}");
            return new Provider
            {
                Facility = _state.ProviderFacility.mcs_StationNumber,
                ClinicIen = _state.ProviderClinic.cvt_ien,
                ClinicDefaultProviderDuz = _state.ProviderClinic.cvt_defaultproviderduz,
                ProviderEmail = GetUserEmail(_state.ProviderClinic.cvt_defaultprovider?.Id),
                ConsultId = _state.ServiceAppointment.cvt_ProConsultIen,
                ConsultTitle = _state.ServiceAppointment.cvt_proconsulttitle,
                RtcId = _state.ServiceAppointment.cvt_prortcid,
                RtcParent = _state.ServiceAppointment.cvt_rtcparentprovider
            };
        }

        /// <summary>
        /// Get User Email.
        /// </summary>
        /// <param name="userId">Default Provider Id.</param>
        /// <returns>User Email.</returns>
        private string GetUserEmail(Guid? userId)
        {
            _logger.Info("***Mapping - GetUserEmail");
            var email = string.Empty;
            if (userId == null) return email;

            using (var srv = new Xrm(_state.OrganizationServiceProxy))
            {
                var provider = srv.SystemUserSet.FirstOrDefault(x => x.Id == userId);
                if (provider == null) throw new SchedulerNameMappingException($"User with Id {userId} cannot be null");

                email = provider.DomainName.Contains("@") ? provider.DomainName : provider.InternalEMailAddress;
            }

            return email;
        }
    }
}
