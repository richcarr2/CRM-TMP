using System;
using System.Collections.Generic;
using System.Linq;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Helpers
{
    public static class VistaPluginHelpers
    {
        private static List<Guid> GetPatients(VA.TMP.DataModel.ServiceAppointment serviceAppointment)
        {
            return serviceAppointment.Customers.Select(c => c.PartyId.Id).ToList();
        }

        private static List<Guid> GetPatients(DataModel.Appointment appointment)
        {
            return appointment.OptionalAttendees.Select(c => c.PartyId.Id).ToList();
        }

        private static List<Guid> GetPatients(Entity record)
        {
            if (record.LogicalName == DataModel.Appointment.EntityLogicalName)
                return GetPatients(record.ToEntity<DataModel.Appointment>());
            if (record.LogicalName == DataModel.ServiceAppointment.EntityLogicalName)
                return GetPatients(record.ToEntity<DataModel.ServiceAppointment>());
            return new List<Guid>();
        }

        private static List<Guid> GetPreviouslyBookedPatients(IOrganizationService OrganizationService, EntityReference bookingRecord, MCSLogger logger)
        {

        
            List<Guid> previouslyBookedPatients = new List<Guid>();
            List<cvt_vistaintegrationresult> pastBookings;
            using (var srv = new Xrm(OrganizationService))
            {
                if (bookingRecord.LogicalName == DataModel.Appointment.EntityLogicalName)
                    pastBookings = srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_Appointment.Id == bookingRecord.Id && vir.cvt_VistAStatus != Common.VistaStatus.CANCELED.ToString() && vir.cvt_VistAStatus != Common.VistaStatus.FAILED_TO_SCHEDULE.ToString()).ToList();
                else if (bookingRecord.LogicalName == DataModel.ServiceAppointment.EntityLogicalName)
                    pastBookings = srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_ServiceActivity.Id == bookingRecord.Id && vir.cvt_VistAStatus != Common.VistaStatus.CANCELED.ToString() && vir.cvt_VistAStatus != Common.VistaStatus.FAILED_TO_SCHEDULE.ToString()).ToList();
                else
                    throw new InvalidPluginExecutionException("Invalid Entity Type: Unable to retrieve patient changes.");
                foreach (var book in pastBookings)
                {
                    var contactId = book.cvt_Veteran?.Id ?? IntegrationPluginHelpers.GetPatIdFromIcn(book.cvt_PersonId, OrganizationService);
                    //Only want distinct patients since WriteResults (aka vista integration results) will return 2 copies for a clinic based (1 for pat side and 1 for pro side with the same patient)
                    if (!previouslyBookedPatients.Contains(contactId))
                        previouslyBookedPatients.Add(contactId);
                }
            }

            logger.WriteDebugMessage("Exiting GetPreviouslyBookedPatients()");

            return previouslyBookedPatients;
        }

        ///// <summary>
        ///// Overload used when getting the answer as to whether this is a book or cancel is irrelevant
        ///// </summary>
        ///// <param name="currentRecord"></param>
        ///// <param name="OrganizationService"></param>
        ///// <param name="Logger"></param>
        ///// <returns></returns>
        //internal static List<Guid> GetChangedPatients(Entity currentRecord, IOrganizationService OrganizationService, MCSLogger Logger)
        //{
        //    bool throwAwayValue;
        //    return GetChangedPatients(currentRecord, OrganizationService, Logger, out throwAwayValue);
        //}

        internal static List<Guid> GetChangedPatients(Entity currentRecord, IOrganizationService organizationService, MCSLogger logger, out bool isAdd)
        {
            isAdd = true;
            List<Guid> currentPatients;

            switch (currentRecord.LogicalName)
            {
                case DataModel.Appointment.EntityLogicalName:
                    currentPatients = currentRecord.ToEntity<DataModel.Appointment>().OptionalAttendees?.Select(ap => ap.PartyId.Id).ToList();
                    break;
                case DataModel.ServiceAppointment.EntityLogicalName:
                    currentPatients = currentRecord.ToEntity<DataModel.ServiceAppointment>().Customers?.Select(ap => ap.PartyId.Id).ToList();
                    break;
                default:
                    throw new InvalidPluginExecutionException("Invalid Entity: Cannot retrieve Patient changes");
            }

            var previousPatients = GetPreviouslyBookedPatients(organizationService, currentRecord.ToEntityReference(), logger);
            var newPatients = currentPatients.Except(previousPatients).ToList();

            logger.WriteDebugMessage($"Found {newPatients.Count} new patients");

            if (newPatients.Count > 0) return newPatients;

            isAdd = false;
            var removedPatients = previousPatients; //.Except(currentPatients).ToList();
            logger.WriteDebugMessage($"Found {removedPatients.Count} patients that are in the process of being canceled");
            return removedPatients;
        }

        internal static string GetDuzFromStationCode(Guid userId, MCSLogger logger, IOrganizationService organizationService, string stationNumber)
        {
            string userDuz = string.Empty;
            if (!string.IsNullOrWhiteSpace(stationNumber))
            {
                using (var context = new Xrm(organizationService))
                {
                    var duz = context.cvt_userduzSet.FirstOrDefault(u => u.cvt_StationNumber == stationNumber && u.cvt_User.Id == userId && u.statecode.Value == (int)cvt_userduzState.Active);
                    if (duz != null && !string.IsNullOrWhiteSpace(duz.cvt_name))
                    {
                        userDuz = duz.cvt_name;
                    }
                    else
                    {
                        logger.WriteToFile($"The user duz for the facility with the station number {stationNumber} is either not available or empty for the user with Id: {userId}");
                    }
                }
            }
            else
            {
                logger.WriteToFile("The facility is either not associated to the Service Appointment or the facility do not have a station number");
            }

            return userDuz;
        }

        internal static bool FullAppointmentCanceled(DataModel.Appointment appointment)
        {
            var isCanceled = StatusIsCanceled(appointment.cvt_IntegrationBookingStatus?.Value ?? -1);

            if (isCanceled) return true;
            if (appointment.StateCode == null) return false;
            if (appointment.StateCode.Value == AppointmentState.Canceled) return true;

            return false;
        }

        private static bool StatusIsCanceled(int statusCode)
        {
            switch (statusCode)
            {
                case (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled:
                case (int)Appointmentcvt_IntegrationBookingStatus.PatientNoShow:
                case (int)Appointmentcvt_IntegrationBookingStatus.ClinicCancelled:
                case (int)Appointmentcvt_IntegrationBookingStatus.TechnologyFailure:
                case (int)Appointmentcvt_IntegrationBookingStatus.SchedulingError:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Create an instance of the Video Vist Service request and send to VIMT.
        /// </summary>
        /// <returns>VideoVisitDeleteResponseMessage.</returns>
        internal static VideoVisitDeleteResponseMessage CancelAndSendVideoVisitServiceSa(DataModel.ServiceAppointment serviceAppointment, Guid userId, string organizationName, IOrganizationService organizationService, MCSLogger logger)
        {
            var cancels = IntegrationPluginHelpers.GetPatientsFromActivityPartyList(serviceAppointment.Customers.ToList());

            var request = new VideoVisitDeleteRequestMessage
            {
                LogRequest = true,
                UserId = userId,
                OrganizationName = organizationName,
                AppointmentId = serviceAppointment.Id,
                CanceledPatients = cancels,
                WholeAppointmentCanceled = true
            };

            var vimtRequest = Serialization.DataContractSerialize(request);
            VideoVisitDeleteResponseMessage response = null;
            try
            {
                logger.WriteDebugMessage("Sending Cancel to VIMT");

                using (var context = new Xrm(organizationService))
                {
                    var apiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "VvsCancelUri");

                    var baseUrl = apiIntegrationSettings.BaseUrl;
                    var uri = apiIntegrationSettings.Uri;
                    var resource = apiIntegrationSettings.Resource;
                    var appId = apiIntegrationSettings.AppId;
                    var secret = apiIntegrationSettings.Secret;
                    var authority = apiIntegrationSettings.Authority;
                    var tenantId = apiIntegrationSettings.TenantId;
                    var subscriptionId = apiIntegrationSettings.SubscriptionId;
                    var isProdApi = apiIntegrationSettings.IsProdApi;
                    var subscriptionIdEast = apiIntegrationSettings.SubscriptionIdEast;
                    var subscriptionIdSouth = apiIntegrationSettings.SubscriptionIdSouth;

                    response = RestPoster.Post<VideoVisitDeleteRequestMessage, VideoVisitDeleteResponseMessage>("VVS Delete", baseUrl, uri, request, resource, appId, secret, authority, tenantId,
                        subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                    logger.WriteDebugMessage("Cancel successfully sent to VIMT");
                    return response;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Cancel VVS", errorMessage, vimtRequest, typeof(VideoVisitDeleteRequestMessage).FullName,
                    typeof(VideoVisitDeleteResponseMessage).FullName, MessageRegistry.VideoVisitDeleteRequestMessage, serviceAppointment.Id, organizationService,
                    response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs, true,false);
                logger.WriteToFile(errorMessage);

                return null;
            }
        }

        //Check for switches
        //MAIN VA Video Connect
        //MAIN SFT
        //Record Specialty
        //Record Specialty SubType
        //Record Provider Facility
        //MAIN Interfacility
        //Record Patient Facility
        public static bool RunVistaIntegrationForThisAppointment(mcs_setting settings, DataModel.ServiceAppointment sa, Xrm context, MCSLogger logger, string pathToVista)
        {
            mcs_site patSite = null;
            mcs_site proSite = null;
            mcs_facility proFacility = null;
            mcs_facility patFacility = null;
            logger.setMethod = "RunVistaIntegrationForThisAppointment";
            logger.WriteDebugMessage("starting RunVistaIntegrationForThisAppointment");

            logger.WriteDebugMessage("Checking Facility Type!");
            var VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(sa, context, logger);

            var isHm = sa.cvt_Type ?? false;

            if (isHm)
            {
                var hmSwitchIsOn = settings.cvt_UseVVSHomeMobile.HasValue && settings.cvt_UseVVSHomeMobile.Value;
                LogSwitchOutput(logger, hmSwitchIsOn, "Main VA Video Connect Encounter", pathToVista);

                if (!hmSwitchIsOn) return false;
            }

            var isSft = sa.cvt_TelehealthModality ?? false;

            if (isSft)
            {
                var sftSwitchIsOn = settings.cvt_UseVVSSingleEncounterNonHomeMobile.HasValue && settings.cvt_UseVVSSingleEncounterNonHomeMobile.Value;
                LogSwitchOutput(logger, sftSwitchIsOn, "Main Non VA Video Connect Single Encounter", pathToVista);

                if (!sftSwitchIsOn) return false;
            }

            var specialtyRecord = context.mcs_servicetypeSet.FirstOrDefault(s => s.Id == sa.mcs_servicetype.Id);
            var specialtySwitch = specialtyRecord != null && (specialtyRecord.cvt_UseVVS ?? false);

            // Checking Specialty
            LogSwitchOutput(logger, specialtySwitch, "Specialty", pathToVista);

            if (!specialtySwitch) return false;

            var subSpecialtyRecord = sa.mcs_servicesubtype != null ? context.mcs_servicesubtypeSet.FirstOrDefault(s => s.Id == sa.mcs_servicesubtype.Id) : null;

            // Checking Specialty Sub-type
            if (subSpecialtyRecord == null) logger.WriteDebugMessage("Sub-Specialty not specified on Service Activity.");
            else
            {
                var subSpecialtySwitch = subSpecialtyRecord.cvt_UseVVS ?? false;
                LogSwitchOutput(logger, subSpecialtySwitch, "Specialty Sub-Type", pathToVista);

                if (!subSpecialtySwitch)
                {
                    logger.WriteDebugMessage("Specialty Sub-Type switch is off.");
                    return false;
                }
            }

            // Provider Facility
            //TSA (mcs_services) is deprecated.  Have to get to Facility via a different path

            //var tsa = context.mcs_servicesSet.FirstOrDefault(s => s.Id == sa.mcs_relatedtsa.Id);
            //var proFacility = context.mcs_facilitySet.FirstOrDefault(f => f.Id == tsa.cvt_ProviderFacility.Id);
            if (sa.mcs_relatedprovidersite != null)
            {
                proSite = context.mcs_siteSet.FirstOrDefault(pro => pro.Id == sa.mcs_relatedprovidersite.Id);
            }

            if ((proSite?.Id != null) && (proSite?.Id != Guid.Empty))
            {
                proFacility = context.mcs_facilitySet.FirstOrDefault(p => p.Id == proSite.mcs_FacilityId.Id);
            }

            if (VistaTypeFacility)
            {
                var proFacilitySwitchIsOn = proFacility != null && (proFacility.cvt_UseVistaIntrafacility ?? false);

                if (proFacility != null) LogSwitchOutput(logger, proFacilitySwitchIsOn, "(" + proFacility.mcs_name + ") Provider Facility", pathToVista);

                if (proSite != null)
                {
                    logger.WriteDebugMessage("Provider Site is listed, so checking Provider Facility switch.");
                    if (!proFacilitySwitchIsOn)
                    {
                        logger.WriteDebugMessage("Provider Facility switch is off.");
                        return false;
                    }
                }
                else
                    logger.WriteDebugMessage("Provider Site is not listed.");
            }
         

            //get patFacility to compare
            if (sa.mcs_relatedsite != null)
            {
                patSite = context.mcs_siteSet.FirstOrDefault(pat => pat.Id == sa.mcs_relatedsite.Id);
            }
            if ((patSite?.Id != null) && (patSite?.Id != Guid.Empty))
            {
                patFacility = context.mcs_facilitySet.FirstOrDefault(f => f.Id == patSite.mcs_FacilityId.Id);
            }

            //var isTsaInterfacility = tsa?.cvt_ServiceScope != null && tsa.cvt_ServiceScope.Value == (int)mcs_servicescvt_ServiceScope.InterFacility;
            bool isTsaInterfacility = false;

            if ((proFacility != null) && (proFacility.Id != Guid.Empty))
            {
                if ((patFacility != null) && (patFacility.Id != Guid.Empty))
                {
                    if (patFacility.Id != proFacility.Id)
                    {
                        isTsaInterfacility = true;
                    }
                }
            }

            if (VistaTypeFacility)
            {
                if (isTsaInterfacility)
                {
                    // Need to check for the actual Interfacility switch
                    var ifcSwitchIsOn = settings.cvt_UseVVSInterfacility ?? false;
                    LogSwitchOutput(logger, ifcSwitchIsOn, "Main Interfacility", pathToVista);

                    if (!ifcSwitchIsOn)
                    {
                        logger.WriteDebugMessage("IFC switch is off.");
                        return false;
                    }
                    // Check Patient Facility
                    //already have patFacility
                    //var patFacility = context.mcs_facilitySet.FirstOrDefault(f => f.Id == tsa.cvt_PatientFacility.Id);

                    var patFacilitySwitchIsOn = patFacility != null && (patFacility.cvt_UseVistaIntrafacility ?? false);

                    if (patFacility != null) LogSwitchOutput(logger, patFacilitySwitchIsOn, "(" + patFacility.mcs_name + ") Patient Facility", pathToVista);

                    if (!patFacilitySwitchIsOn)
                    {
                        logger.WriteDebugMessage("Patient Facility switch is off.");
                        return false;
                    }
                }
                else if (proFacility == null && patFacility != null)
                {

                    logger.WriteDebugMessage("Checking Patient Facility Switch");
                    var patFacilitySwitchIsOn = patFacility != null && (patFacility.cvt_UseVistaIntrafacility ?? false);
                    LogSwitchOutput(logger, patFacilitySwitchIsOn, "(" + patFacility.mcs_name + ") Patient Facility", pathToVista);

                    if (!patFacilitySwitchIsOn)
                    {
                        logger.WriteDebugMessage("Patient Facility switch is off.");
                        return false;
                    }
                }

            }

            logger.WriteDebugMessage("Returning true for RunVistaIntegrationForThisAppointment.");

            return true;
        }

        public static bool RunVistaIntegration(DataModel.ServiceAppointment sa, Xrm context, MCSLogger Logger)
        {
            Logger.WriteDebugMessage($"VistaPluginHelpers.RunVistaIntegration(): Enter");

            var settings = context.mcs_settingSet.FirstOrDefault(x => x.mcs_name == "Active Settings");
            if (settings == null) throw new InvalidPluginExecutionException("Active Settings Cannot be Null");

            Logger.WriteDebugMessage("Checking Facility Type!");
            var VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(sa, context, Logger);

            if (!VistaTypeFacility) // i.e. Cerner
            {
                return true;
            }
            else
            {
                Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
            }

            if (settings.cvt_UseVistaIntegration.HasValue && settings.cvt_UseVistaIntegration.Value)
            {
                return RunVistaIntegrationForThisAppointment(settings, sa, context, Logger, "VIA/HealthShare");
            }

            Logger.WriteDebugMessage("Vista Integration (through VIA/HealthShare) is turned off system wide, skipping VVS integration");

            Logger.WriteDebugMessage($"VistaPluginHelpers.RunVistaIntegration(): Exit");

            return false;
        }

        public static bool RunVvs(DataModel.ServiceAppointment sa, Xrm context, MCSLogger Logger)
        {
            // Ensure this is NOT a Telephone Call VVC Service Appointment.
            if (sa.cvt_TelephoneCall.HasValue && sa.cvt_TelephoneCall.Value)
            {
                Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VVS integration");
                return false;
            }

            var settings = context.mcs_settingSet.FirstOrDefault(x => x.mcs_name == "Active Settings");
            if (settings == null) throw new InvalidPluginExecutionException("Active Settings Cannot be Null");
            if (settings.cvt_UseVVS != null && settings.cvt_UseVVS.Value)
            {
                return RunVistaIntegrationForThisAppointment(settings, sa, context, Logger, "VVS");
            }

            Logger.WriteDebugMessage("VVS is turned off system wide, skipping VVS integration");
            return false;
        }

        public static void LogSwitchOutput(MCSLogger logger, bool switchIsOn, string appointmentType, string pathToVista)
        {
            logger.WriteDebugMessage(string.Format("{0} Vista Booking switch is {1}", appointmentType, switchIsOn ? "turned on." : $"turned off, skipping {pathToVista} Integration."));
        }
    }
}
