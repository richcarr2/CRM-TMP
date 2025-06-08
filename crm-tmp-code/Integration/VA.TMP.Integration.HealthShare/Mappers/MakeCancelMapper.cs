using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.Mappers
{
    internal class MakeCancelMapper
    {
        private readonly TmpHealthShareMakeAndCancelAppointmentRequestMessage _request;
        private readonly OrganizationWebProxyClient _organizationServiceProxy;
        private readonly ILog _logger;
        private readonly Core.Settings _settings;
        private readonly Rest.Interface.IServicePost _servicePost;

        private string MviOrgName => _settings.Items.First(x => x.Key == "MviOrgName").Value;
        private string MviPersonSearchUri => _settings.Items.First(x => x.Key == "MviPersonSearchUri").Value;

        public MakeCancelMapper(TmpHealthShareMakeAndCancelAppointmentRequestMessage request, OrganizationWebProxyClient organizationServiceProxy, ILog logger, Rest.Interface.IServicePost servicePost, Core.Settings settings)
        {
            _request = request;
            _organizationServiceProxy = organizationServiceProxy;
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        internal Appointment Map()
        {
            var appointment = new Appointment
            {
                cvt_PatientICN = _request.PatientIcn,
                ScheduledDurationMinutes = _request.Duration,
                cvt_VisitStatus = _request.VisitStatus,
                cvt_FacilityText = _request.Facility,
                cvt_stationnumber = _request.StationNumber,
                cvt_ClinicIEN = _request.ClinicIen,
                cvt_ClinicName = _request.ClinicName,
                cvt_ProviderEmail = _request.ProviderEmail,
                cvt_ConsultId = _request.ConsultId,
                cvt_ConsultName = _request.ConsultName,
                cvt_CancelReason = _request.CancelReason,
                cvt_CancelCode = _request.CancelCode,
                cvt_CancelRemarks = _request.CancelRemarks,
                cvt_SchedulerName = _request.SchedulerName,
                cvt_SchedulerEmail = _request.SchedulerEmail,
                Subject = $"VISTA - {_request.ClinicName ?? _request.ConsultName} "
            };

            if (_request.Duration <= 0)
            {
                _logger.Error("ERROR: Duration in the Request is Invalid");
                throw new InvalidDurationException("Duration in the HealthShare Request is NULL/INVALID");
            }
            using (var service = new Xrm(_organizationServiceProxy))
            {
                switch (appointment.cvt_CancelCode)
                {
                    // For Non-Clinic Days - Patient and Provider details are not required, hence skipping validation methods
                    case "NCD":
                    case "RCD":
                        MapFacilitySiteDetails(appointment, service);
                        MapSchedulerDetails(appointment, service);
                        MapClinicDetails(_organizationServiceProxy, appointment);
                        //MapClinicDetails(service, appointment);
                        break;
                    default:
                        MapProviderDetails(service, appointment);
                        MapSchedulerDetails(appointment, service);
                        MapFacilitySiteDetails(appointment, service);
                        MapPatientDetails(service, appointment);
                        MapClinicDetails(_organizationServiceProxy, appointment);
                        //MapClinicDetails(service, appointment);
                        break;
                }

            }

            //_logger.Debug($"Mapped Appointment: {Serialization.DataContractSerialize(appointment)}");

            return appointment;
        }

        private void MapScheduledStart(Appointment appointment)
        {
            var isParseSuccessful = DateTime.TryParse(_request.StartTime, out var startTime);
            if (!string.IsNullOrWhiteSpace(_request.StartTime) && isParseSuccessful)
            {
                appointment.ScheduledStart = startTime;
            }
            else
            {
                _logger.Error("ERROR: StartTime in the Request is null/Invalid");
                throw new StartTimeMappingException("StartTime in the Request Json is NULL/INVALID");
            }
        }

        private void MapScheduledStart(Xrm service, Appointment appointment, mcs_resource firstClinic)
        {
            //var isParseSuccessful = DateTime.TryParse(_request.StartTime, out var startTime);
            if (!string.IsNullOrWhiteSpace(_request.StartTime))
            {
                if (DateTime.TryParse(_request.StartTime, out var startTime))
                {
                    if (!_request.StartTime.ToUpper().EndsWith("Z"))
                    {
                        var clinic = firstClinic?.mcs_relatedResourceId != null
                            ? service.EquipmentSet.Where(c => c.Id == firstClinic.mcs_relatedResourceId.Id)
                            : new List<Equipment>().AsQueryable();

                        if (clinic.ToList().Count().Equals(0)) return;

                        _logger.Debug("Selected clinic: " + clinic.FirstOrDefault().Name);

                        var timeZonerecord = service.TimeZoneDefinitionSet.FirstOrDefault(t => t.TimeZoneCode != null && t.TimeZoneCode.Value == clinic.FirstOrDefault().TimeZoneCode);
                        var clinicTimezone = TimeZoneInfo.FindSystemTimeZoneById(timeZonerecord.StandardName);

                        var clinicUtcOffset = clinicTimezone.GetUtcOffset(startTime);

                        _logger.Debug("startime before " + startTime);
                        startTime = startTime.AddHours(-(clinicUtcOffset.TotalHours));
                        _logger.Debug("startime after " + startTime);

                        appointment.ScheduledStart = startTime;
                    }

                    _logger.Debug($"Setting Scheduled Start Time to: {startTime}");
                    appointment.ScheduledStart = startTime;
                }
                else
                {
                    _logger.Error("ERROR: StartTime in the Request is Invalid");
                    throw new StartTimeMappingException("StartTime in the Request Json is INVALID");
                }
            }
            else
            {
                _logger.Error("ERROR: StartTime in the Request is null/Invalid");
                throw new StartTimeMappingException("StartTime in the Request Json is NULL/INVALID");
            }
        }

        private void MapPatientDetails(Xrm service, Appointment appointment)
        {
            if (string.IsNullOrWhiteSpace(_request.PatientIcn))
            {
                _logger.Info("INFO: Patient ICN is missing in the request");
            }
            else
            {
                var pId = service.mcs_personidentifiersSet.FirstOrDefault(i => i.mcs_identifier == _request.PatientIcn &&
                                                                                  i.mcs_assigningauthority == "USVHA");

                if (pId != null)
                {
                    var optParty = new ActivityParty { ["partyid"] = pId.mcs_patient };
                    var op = new List<ActivityParty> { optParty };
                    appointment.OptionalAttendees = op;
                }
                else
                {
                    pId = SearchMviByIcn(service, _request.PatientIcn);

                    if (pId != null && !pId.Id.Equals(Guid.Empty))
                    {
                        var optParty = new ActivityParty { ["partyid"] = pId.mcs_patient };
                        var op = new List<ActivityParty> { optParty };
                        appointment.OptionalAttendees = op;
                    }
                    else
                    {
                        _logger.Info($"Cannot find patient with IEN matching {_request.PatientIcn}");
                    }
                }
            }
        }

        private void MapFacilitySiteDetails(Appointment appointment, Xrm service)
        {
            if (string.IsNullOrWhiteSpace(_request.Facility))
            {
                _logger.Info("INFO: Facility is missing in the request");
            }
            else
            {
                mcs_facility facility = null;
                mcs_site site = null;

                var facilities = service.mcs_facilitySet.Where(i => i.mcs_StationNumber == _request.Facility.Substring(0, 3)).ToList();

                if (!facilities.Count().Equals(0))
                {
                    facilities.ForEach((f) =>
                    {
                        var sites = service.mcs_siteSet.Where(s => s.mcs_StationNumber == _request.StationNumber && s.mcs_FacilityId.Id == f.Id).ToList();
                        if (sites != null && !sites.Count().Equals(0))
                        {
                            facility = f;
                            site = sites.FirstOrDefault();
                        }
                    });

                    if (facility != null)
                    {
                        appointment.cvt_Facility = new EntityReference("mcs_facility", facility.Id);
                        appointment.cvt_FacilityText = $"{facility.mcs_name} ({facility.mcs_StationNumber})";
                        appointment.OwnerId = GetSchedulerGroup(facility, service);
                    }
                    else
                    {
                        appointment.cvt_FacilityText = "";

                        // Log it when we can't find a facility
                        _logger.Info($"Cannot find Facility with Station Number = {_request.Facility}");
                    }

                    // Moved Site block after Facility to update Subject correctly with Site name and Station number
                    if (site != null)
                    {
                        appointment.cvt_FacilityText = $"{site.mcs_UserNameInput} {site.FormattedValues["mcs_type"]} ({site.mcs_StationNumber})";
                        appointment.cvt_Site = new EntityReference(mcs_site.EntityLogicalName, site.Id);
                    }
                }
                else
                {
                    // Log it when we can't find a facility
                    _logger.Info($"Cannot find Facility with Station Number = {_request.Facility}");
                }

                appointment.Subject = $"VISTA - {_request.ClinicName ?? _request.ConsultName} @ {appointment.cvt_FacilityText}";
            }
        }

        private void MapSchedulerDetails(Appointment appointment, Xrm service)
        {
            if (string.IsNullOrWhiteSpace(_request.SchedulerEmail))
            {
                _logger.Info("INFO: SchedulerEmail is missing in the request");
            }
            else
            {
                var scheduler =
                    service.SystemUserSet.FirstOrDefault(i => i.InternalEMailAddress == _request.SchedulerEmail);

                if (scheduler != null)
                {
                    appointment.cvt_SchedulerUser = new EntityReference("systemuser", scheduler.Id);
                }
                else
                {
                    // Log it when we can't find a scheduler
                    _logger.Info($"Cannot find systemuser with email matching {_request.SchedulerEmail} for Scheduler {_request.SchedulerName}");
                }
            }
        }

        private void MapProviderDetails(Xrm service, Appointment appointment)
        {
            // PROVIDER
            if (string.IsNullOrEmpty(_request.ProviderEmail))
            {
                _logger.Info("INFO: ProviderEmail is missing in the request");
            }
            else
            {
                var provider = service.SystemUserSet.FirstOrDefault(i => i.InternalEMailAddress == _request.ProviderEmail);

                if (provider != null)
                {
                    appointment.cvt_Provider = new EntityReference("systemuser", provider.Id);

                    var reqParty = new ActivityParty { ["partyid"] = new EntityReference("systemuser", provider.Id) };

                    var ap = new List<ActivityParty> { reqParty };
                    appointment.RequiredAttendees = ap;
                }
                else
                {
                    // Log it when we can't find a provider
                    _logger.Info($"Cannot find systemuser with email matching {_request.ProviderEmail} for Provider");
                }
            }
        }

        private void MapClinicDetails(OrganizationWebProxyClient organizationService, Appointment appointment)
        {
            //mcs_facility facility = null;
            mcs_resource firstClinic = null;
            mcs_site site = null;

            // Clinic
            if (string.IsNullOrEmpty(_request.ClinicIen) || string.IsNullOrEmpty(_request.Facility))
            {
                _logger.Info($"INFO: ClinicIen: {_request.ClinicIen} or Facility: {_request.Facility} is missing in the request");
            }
            else
            {
                (firstClinic, site) = MappingResolver.ClinicSiteResolver(organizationService, _request.Facility, _request.ClinicIen, _request.StationNumber, _logger, appointment?.cvt_Site?.Id);

                if (firstClinic != null && firstClinic.mcs_relatedResourceId != null)
                {
                    _logger.Debug("Clinic Id: " + firstClinic.mcs_relatedResourceId.Id);
                    var reqParty = new ActivityParty { ["partyid"] = new EntityReference(Equipment.EntityLogicalName, firstClinic.mcs_relatedResourceId.Id) };

                    var ap = new List<ActivityParty> { reqParty };
                    if (appointment.RequiredAttendees != null)
                        ap.AddRange(appointment.RequiredAttendees.ToList());
                    appointment.RequiredAttendees = ap;

                    using (var service = new Xrm(organizationService))
                    {
                        if (site != null && appointment.cvt_Site == null)
                        {
                            appointment.cvt_Site = new EntityReference(mcs_site.EntityLogicalName, site.Id);

                            // Appended Subject with TMP Site name and Station Number
                            appointment.cvt_FacilityText = $"{site.mcs_UserNameInput} {site.FormattedValues["mcs_type"]} ({site.mcs_StationNumber})";
                        }

                        // Set Scheduled Start Date 
                        MapScheduledStart(service, appointment, firstClinic);
                    }
                }
            }
        }

        //private void MapClinicDetails(Xrm service, Appointment appointment)
        //{
        //    mcs_facility facility = null;
        //    mcs_resource firstClinic = null;
        //    mcs_site site = null;

        //    // Clinic
        //    if (string.IsNullOrEmpty(_request.ClinicIen) || string.IsNullOrEmpty(_request.Facility))
        //    {
        //        _logger.Info($"INFO: ClinicIen: {_request.ClinicIen} or Facility: {_request.Facility} is missing in the request");
        //    }
        //    else
        //    {
        //        var facilities = service.mcs_facilitySet.Where(i => i.mcs_StationNumber == _request.Facility.Substring(0, 3)).ToList();

        //        if (!facilities.Count().Equals(0))
        //        {
        //            if (facilities.Count() > 1)
        //            {
        //                facilities.ForEach((f) =>
        //                {
        //                    //if (firstClinic != null) return;
        //                    facility = f;
        //                    var clinics = from c in service.mcs_resourceSet
        //                                  where c.mcs_Facility.Id == f.Id && c.cvt_ien == _request.ClinicIen
        //                                  && c.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic
        //                                  orderby c.CreatedOn
        //                                  select c;

        //                    if (clinics != null)
        //                    {

        //                        if (clinics.ToList().Count == 0)
        //                        {
        //                            _logger.Info($"Cannot find clinic with mcs_Facility.Id == {facility.Id} && cvt_ien == {_request.ClinicIen}");
        //                            return;
        //                        }

        //                        var clinicCount = clinics.ToList().Count;
        //                        if (clinicCount > 1)
        //                        {
        //                            var sites = appointment.cvt_Site != null
        //                                ? new List<mcs_site>() { service.mcs_siteSet.FirstOrDefault(s => s.Id == appointment.cvt_Site.Id) }
        //                                : string.IsNullOrEmpty(_request.StationNumber)
        //                                    ? service.mcs_siteSet.Where(s => s.mcs_FacilityId.Id == f.Id).ToList()
        //                                    : service.mcs_siteSet.Where(s => s.mcs_StationNumber == _request.StationNumber && s.mcs_FacilityId.Id == f.Id).ToList();

        //                            if (sites != null)
        //                            {
        //                                if (sites.Count().Equals(1))
        //                                {
        //                                    var clinic = clinics.FirstOrDefault(c => c.mcs_RelatedSiteId.Id == sites.First().Id);
        //                                    if (clinic != null) firstClinic = clinic;
        //                                    if (appointment.cvt_Site == null) site = sites.First();
        //                                }
        //                                else
        //                                {
        //                                    sites.ForEach((s) =>
        //                                    {
        //                                        var clinic = clinics.FirstOrDefault(c => c.mcs_RelatedSiteId.Id == s.Id);
        //                                        if (clinic != null && !clinic.Id.Equals(Guid.Empty))
        //                                        {
        //                                            _logger.Debug("Clinic: " + firstClinic.Id);
        //                                            if (firstClinic == null) firstClinic = clinic;
        //                                            if (appointment.cvt_Site == null) site = s;
        //                                        }
        //                                    });
        //                                }
        //                            }
        //                            _logger.Info($"Duplicates - If : Found {clinicCount} clinics with mcs_Facility.Id == {facility.Id} && cvt_ien == {_request.ClinicIen}");
        //                        }
        //                        else
        //                        {
        //                            if (firstClinic == null) firstClinic = clinics.First();
        //                            if (appointment.cvt_Site == null) site = service.mcs_siteSet.FirstOrDefault(s => s.Id == firstClinic.mcs_RelatedSiteId.Id);
        //                        }
        //                    }
        //                });
        //            }
        //            else
        //            {
        //                facility = facilities.First();
        //                var clinics = from c in service.mcs_resourceSet
        //                              where c.mcs_Facility.Id == facility.Id && c.cvt_ien == _request.ClinicIen
        //                              && c.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic
        //                              orderby c.CreatedOn
        //                              select c;

        //                if (clinics != null)
        //                {
        //                    if (clinics.ToList().Count == 0)
        //                    {
        //                        _logger.Info($"Cannot find clinic with mcs_Facility.Id == {facility.Id} && cvt_ien == {_request.ClinicIen}");
        //                        return;
        //                    }

        //                    var clinicCount = clinics.ToList().Count;
        //                    if (clinicCount > 1)
        //                    {
        //                        var sites = appointment.cvt_Site != null
        //                            ? new List<mcs_site>() { service.mcs_siteSet.FirstOrDefault(s => s.Id == appointment.cvt_Site.Id) }
        //                            : string.IsNullOrEmpty(_request.StationNumber)
        //                                ? service.mcs_siteSet.Where(s => s.mcs_FacilityId.Id == facility.Id).ToList()
        //                                : service.mcs_siteSet.Where(s => s.mcs_StationNumber == _request.StationNumber && s.mcs_FacilityId.Id == facility.Id).ToList();

        //                        if (sites != null)
        //                        {
        //                            if (sites.Count().Equals(1))
        //                            {
        //                                var clinic = clinics.FirstOrDefault(c => c.mcs_RelatedSiteId.Id == sites.First().Id);
        //                                if (clinic != null) firstClinic = clinic;
        //                                if (appointment.cvt_Site == null) site = sites.First();
        //                            }
        //                            else
        //                            {
        //                                sites.ForEach((s) =>
        //                                {
        //                                    var clinic = clinics.FirstOrDefault(c => c.mcs_RelatedSiteId.Id == s.Id);
        //                                    if (clinic != null && !clinic.Id.Equals(Guid.Empty))
        //                                    {
        //                                        _logger.Debug("Clinic: " + firstClinic.Id);
        //                                        if (firstClinic == null) firstClinic = clinic;
        //                                        if (appointment.cvt_Site == null) site = s;
        //                                    }
        //                                });
        //                            }
        //                        }
        //                        _logger.Info($"Duplicates - Else: Found {clinicCount} clinics with mcs_Facility.Id == {facility.Id} && cvt_ien == {_request.ClinicIen}");
        //                    }
        //                    else
        //                    {
        //                        if (firstClinic == null) firstClinic = clinics.First();
        //                        if (appointment.cvt_Site == null) site = service.mcs_siteSet.FirstOrDefault(s => s.Id == firstClinic.mcs_RelatedSiteId.Id);
        //                    }
        //                }
        //            }
        //        }

        //        if (firstClinic != null && firstClinic.mcs_relatedResourceId != null)
        //        {
        //            _logger.Debug("Clinic Id: " + firstClinic.mcs_relatedResourceId.Id);
        //            var reqParty = new ActivityParty { ["partyid"] = new EntityReference(Equipment.EntityLogicalName, firstClinic.mcs_relatedResourceId.Id) };

        //            var ap = new List<ActivityParty> { reqParty };
        //            if (appointment.RequiredAttendees != null)
        //                ap.AddRange(appointment.RequiredAttendees.ToList());
        //            appointment.RequiredAttendees = ap;
        //        }

        //        if (site != null && appointment.cvt_Site == null)
        //        {
        //            appointment.cvt_Site = new EntityReference(mcs_site.EntityLogicalName, site.Id);

        //            // Appended Subject with TMP Site name and Station Number
        //            appointment.cvt_FacilityText = $"{site.mcs_UserNameInput} {site.FormattedValues["mcs_type"]} ({site.mcs_StationNumber})";
        //        }

        //        // Set Scheduled Start Date 
        //        MapScheduledStart(service, appointment, firstClinic);
        //    }
        //}

        private EntityReference GetSchedulerGroup(mcs_facility facility, Xrm srv)
        {
            EntityReference schedulerTeam = null;
            if (facility != null)
            {
                var teams = (from t in srv.TeamSet
                             join f in srv.mcs_facilitySet on t.cvt_Facility.Id equals f.mcs_facilityId.Value
                             where f.mcs_facilityId.Value == facility.Id
                             select new Team { Id = t.Id, cvt_Type = t.cvt_Type });

                foreach (var team in teams)
                {
                    if (team.cvt_Type != null && team.cvt_Type.Value == Teamcvt_Type.Scheduler.GetHashCode())
                    {
                        schedulerTeam = new EntityReference(Team.EntityLogicalName, team.Id);
                        break;
                    }
                }

                if (schedulerTeam == null)
                {
                    // Log it when we can't find a scheduler team
                    _logger.Info($"INFO: Scheduler Team for the facility {facility.mcs_name} with Station Number {_request.Facility} is not found in TMP");
                }
            }

            return schedulerTeam;
        }

        private mcs_personidentifiers SearchMviByIcn(Xrm service, string patientIdentifier)
        {
            var assigningAuthority = "USVHA";
            var assigningFacility = "200M";
            var identifierType = "NI";
            mcs_personidentifiers personIdentifier = null;

            var unattendedSearchRequest = new TmpHealthShareUnattendedSearchRequest
            {
                IsAttended = false,
                MessageId = Guid.NewGuid().ToString(),
                OrganizationName = MviOrgName,
                PatientIdentifier = string.Format("{0}^{1}^{2}^{3}", patientIdentifier, identifierType, assigningFacility, assigningAuthority),
                UserId = _request.UserId
            };

            try
            {
                var response = _servicePost.PostToEc<TmpHealthShareUnattendedSearchRequest, TmpHealthSharePersonSearchResponseMessage>(
                    "MVI Unattended Person Search", MviPersonSearchUri, _settings, unattendedSearchRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                if (response != null && response.RetrieveOrSearchPersonResponse != null)
                {
                    if (!response.RetrieveOrSearchPersonResponse.ExceptionOccured)
                    {
                        //CreateContact(service, response.RetrieveOrSearchPersonResponse.Person[0]);
                        //personIdentifier = service.mcs_personidentifiersSet.OrderByDescending(pi => pi.CreatedOn).FirstOrDefault(i => i.mcs_identifier == _request.PatientIcn &&
                        //                                                          i.mcs_assigningauthority == "USVHA");
                    }
                    else
                    {
                        _logger.Error($"Failed to retrieve the Patient with ICN {patientIdentifier} from MVI for the following reason: {response.ExceptionMessage}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to retrieve Patient with ICN {patientIdentifier} from MVI for the following reason: {e}");
                throw;
            }

            return personIdentifier;
        }

        private void CreateContact(Xrm service, PatientPerson person)
        {
            var dob = GetDate(person.BirthDate);
            var dod = GetDate(person.DeceasedDate);
            var idTheft = GetIdTheftIndicator(person.IdentifyTheft);

            var contact = new Contact();

            person.NameList.ToList().ForEach((name) =>
            {
                if (name.NameType.ToLower().Equals("legal"))
                {
                    contact.FirstName = name.GivenName;
                    contact.LastName = name.FamilyName;
                    contact.MiddleName = name.MiddleName;
                    contact.Salutation = name.NamePrefix;
                    contact.Suffix = name.NameSuffix;
                }
                else
                {
                    contact.mcs_othernames = string.IsNullOrEmpty(contact.mcs_othernames) || contact.mcs_othernames.Length.Equals(0)
                        ? name.ToString()
                        : "; " + name.ToString();
                }
            });

            if (person.Address != null)
            {
                contact.Address1_Line1 = person.Address.StreetAddressLine;
                contact.Address1_City = person.Address.City;
                contact.Address1_Country = person.Address.Country;
                contact.Address1_PostalCode = person.Address.PostalCode;
                contact.Address1_StateOrProvince = person.Address.State;
                contact.Address1_Name = Enum.GetName(typeof(AddressUse), person.Address.Use);
            }

            if (!string.IsNullOrEmpty(person.GenderCode))
            {
                int genderCode;
                switch (person.GenderCode.ToLower())
                {
                    case "m":
                        genderCode = 1;
                        break;
                    case "f":
                        genderCode = 2;
                        break;
                    default:
                        genderCode = -1;
                        break;
                }

                if (genderCode > 0) contact.GenderCode = new OptionSetValue(genderCode);
            }

            contact.mcs_contact_mcs_personidentifiers_patient = GetPersonIdentifiers(person.CorrespondingIdList.ToList(), person.EdiPi, person.Ss, out var lastFour);
            if (!string.IsNullOrEmpty(lastFour)) contact.mcs_Last4 = lastFour;

            contact.mcs_deceased = dod.HasValue;
            contact.mcs_deceaseddate = dod;
            contact.mcs_identitytheft = idTheft;
            contact.Telephone2 = person.PhoneNumber;
            contact.BirthDate = dob;

            contact.EntityState = EntityState.Created;

            service.AddObject(contact);
            service.SaveChanges();
        }

        /// <summary>
        /// Get Date. Adjust it by 17 hours upward so that regardless of user time zone, the date is correct - assumes CRM Server is in East Coast (subtract 5 hours for UTC offset + 12 hours to put the time to noon UTC)
        /// </summary>
        /// <param name="dateString">Date string.</param>
        /// <returns>Date.</returns>
        private DateTime? GetDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return null;
            if (dateString.Length < 8) return null;

            var sYear = dateString.Substring(0, 4);
            var sMonth = dateString.Substring(4, 2);
            var sDay = dateString.Substring(6, 2);
            var sDate = string.Format("{0}/{1}/{2}", sMonth, sDay, sYear);

            DateTime date;
            if (DateTime.TryParse(sDate, out date)) return date.AddHours(7);

            return null;
        }

        /// <summary>
        /// Get Theft Indicator.
        /// </summary>
        /// <param name="idTheftIndicator">Theft Indicator Id</param>
        /// <returns>Theft Indicator.</returns>
        private bool? GetIdTheftIndicator(string idTheftIndicator)
        {
            return !string.IsNullOrEmpty(idTheftIndicator) && idTheftIndicator.ToLower() == "yes";
        }

        /// <summary>
        /// Get Person Identifiers.
        /// </summary>
        /// <param name="ids">Ids.</param>
        /// <param name="edipi">Edipi.</param>
        /// <param name="ss">Ss.</param>
        /// <param name="lastFour">Last Four.</param>
        /// <returns>List of Person Identifiers.</returns>
        public List<mcs_personidentifiers> GetPersonIdentifiers(List<UnattendedSearchRequest> ids, string edipi, string ss, out string lastFour)
        {
            var personIds = new List<mcs_personidentifiers>();
            lastFour = string.Empty;

            foreach (var id in ids)
            {
                var personId = new mcs_personidentifiers
                {
                    mcs_identifier = id.PatientIdentifier,
                    mcs_assigningfacility = id.AssigningFacility == "200M" ? "" : id.AssigningFacility.Replace("200", ""),
                    mcs_assigningauthority = id.AssigningAuthority.Replace("200", ""),
                    mcs_authorityorganizationid = !string.IsNullOrEmpty(id.AuthorityOid) ? id.AuthorityOid : id.AssigningAuthority
                };

                int idType;
                switch (id.IdentifierType)
                {
                    case "NI":
                        idType = 125150000;
                        break;
                    case "PI":
                        idType = 125150001;
                        break;
                    case "EI":
                        idType = 125150002;
                        break;
                    case "PN":
                        idType = 125150003;
                        break;
                    case "SS":
                        idType = 125150004;
                        break;
                    default:
                        idType = 0;
                        break;
                }
                personId.mcs_identifiertype = new OptionSetValue(idType);

                personIds.Add(personId);

            }

            if (personIds.Any(i => i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS))
            {
                ss = personIds.First(i => i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS).mcs_identifier;

                if (!string.IsNullOrEmpty(ss) && ss.Length >= 4) lastFour = ss.Substring(ss.Length - 4);
            }
            else
            {
                if (!string.IsNullOrEmpty(ss))
                {
                    personIds.Add(new mcs_personidentifiers
                    {
                        mcs_identifier = ss,
                        mcs_assigningauthority = "USSSA",
                        mcs_identifiertype = new OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS)
                    });

                    if (ss.Length >= 4) lastFour = ss.Substring(ss.Length - 4);
                }
            }

            if (!string.IsNullOrEmpty(edipi) && personIds.All(i => i.mcs_identifier != edipi))
                personIds.Add(new mcs_personidentifiers
                {
                    mcs_identifier = edipi,
                    mcs_assigningauthority = "USDOD",
                    mcs_identifiertype = new OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI)
                });

            return personIds;
        }
    }
}
