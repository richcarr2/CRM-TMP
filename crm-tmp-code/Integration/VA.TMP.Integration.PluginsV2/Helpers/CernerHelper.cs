using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Plugins.Helpers;
using MCSUtilities2011;
using System.Net.Http.Headers;
using VA.TMP.Integration.Messages.Cerner;
using System.Diagnostics;
using System.Web;
using System.Web.Script.Serialization;
using VA.TMP.OptionSets;
using System.Reflection;
using System.Runtime.CompilerServices;
using MCS.ApplicationInsights;

namespace VA.TMP.Integration.Plugins.Helpers
{
    public static class CernerHelper
    {
        /// <summary>
        /// Determines if the facilities associated with a given Service Appointment (or Appointment, if the optional last parameter is specified)
        /// is related to a site that is related to a Facility that has a Facility Type of "Cerner Millennium".
        /// </summary>
        /// 
        /// <returns>
        /// True if either the Patient or Provider side is NOT related to a VistA Facility (i.e. Cerner)
        /// False if (both the Patient and Provider side are related to a VistA Facility)
        /// </returns>
        public static bool CheckIfRelatedCernerFacility(VA.TMP.DataModel.ServiceAppointment ServiceAppointment, Xrm context, MCSLogger Logger, VA.TMP.DataModel.Appointment Appointment = null)
        {

            LogEntry(Logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Enter");
            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Service Appointment id passed in = {ServiceAppointment?.Id.ToString() ?? ""}");
            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Reserve Resource id passed in = {Appointment?.Id.ToString() ?? ""}");

            bool isVista = true;

            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: isVista = true by default");

            try
            {
                var osvVista = new OptionSetValue(917290001);

                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: osvVista for comparision has value {osvVista.Value}");

                mcs_site patSite = null;
                mcs_site proSite = null;
                mcs_facility profacility = null;
                mcs_facility patfacility = null;

                /*
                 *  Get Patient Site
                 *  
                 *  There are two ways to get the patient site, depending on the conditions:
                 *  
                 *  (1) If the SA is NOT a group appointment AND there is a Patient Site (mcs_relatedsite) on the SA, use the Patient Site (mcs_relatedsite) on the SA.
                 *  (2) If the SA is a group appointment, get the Appointment record associated with the SA and then get the Patient Site (cvt_Site) associated with the Appointment. 
                 */

                // If this is not a group appointment and there is a Patient Site (mcs_relatedsite)

                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: GroupAppointment = " + ServiceAppointment.mcs_groupappointment);

                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: mcs_relatedsite = " + ServiceAppointment?.mcs_relatedsite?.Id);

                if (!(bool)ServiceAppointment.mcs_groupappointment && ServiceAppointment.mcs_relatedsite != null)
                {
                    patSite = context.mcs_siteSet.FirstOrDefault(pat => pat.Id == ServiceAppointment.mcs_relatedsite.Id);
                    Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: This is not a group appt, so got patSite from sa.mcs_relatedsite. The id is = " + patSite.Id);
                }

                // Else, if this is a group appointment

                else if ((bool)ServiceAppointment.mcs_groupappointment)
                {
                    Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Group Appt: ServiceAppointment.mcs_groupappointment == true");

                    // Get the appointment passed in, if it exists, otherwise, iterate the list of reserve resources associated with the SA
                    // on the record to see if there is anything (pick the first one).

                    var apptIdFromReserveResources = GetRelatedReserveResourcesAssociatedWithServiceAppointmentAndPatientSide(ServiceAppointment, context, Logger);

                    Guid first_apptIdFromReserveResources = Guid.Empty;

                    if (apptIdFromReserveResources.Count > 0)
                    {
                        first_apptIdFromReserveResources = apptIdFromReserveResources.First().Id;
                    }

                    var apptId = (Appointment != null ? Appointment.Id : first_apptIdFromReserveResources);

                    // If there is an Appointment record associated with the service appointment

                    if (apptId != null && apptId != Guid.Empty)
                    {
                        Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Group Appt: apptId = {apptId}");

                        var appt = context.AppointmentSet.FirstOrDefault(x => x.Id == apptId);

                        // If the appointment identified exists in TMP

                        if (appt != null)
                        {
                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Group Appt: appt = " + appt.Id);

                            // Get the patient site from the Appointment

                            var patientSiteId = appt.cvt_Site?.Id;

                            if (patientSiteId != null)
                            {
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Group Appt: patientSiteId = " + patientSiteId);

                                patSite = context.mcs_siteSet.FirstOrDefault(s => s.Id == patientSiteId);

                                if (patSite != null)
                                {
                                    Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Group Appt: Got Patient Site from Appointment linked to SA. patientSite Id = " + patSite.Id);
                                }
                                else
                                {
                                    Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: Could not retrieve patient site for Appointment {appt.Id}, Site id = {patientSiteId}, this is not expected");
                                }

                            }
                            else
                            {
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: No site (cvt_site) associated with Appointment {appt.Id}");
                            }

                        }
                        else
                        {
                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: Appointment with id {apptId} is null");
                        }

                    }
                    else
                    {
                        Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: (Patient side) : Could not find reserve resources associated with service appointment with id = {ServiceAppointment.Id}");
                    }

                }

                /*
                 *  Get Patient Facility
                 */


                // If the Patient Site exists

                if (patSite != null)
                {
                    // If the Facility on the Patient Site is not null

                    if (patSite.mcs_FacilityId != null)
                    {
                        patfacility = context.mcs_facilitySet.FirstOrDefault(x => x.Id == patSite.mcs_FacilityId.Id);

                        if (patfacility == null)
                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: The Patient Facility is null");
                    }
                    else
                    {
                        Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: The Patient Facility is missing on Patient Site = {patSite.Id}");
                    }
                }
                else
                {
                    Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: The Patient Site is null");
                }

                /*
                 *  Get Provider Site 
                 *  There is only one place to get this - from the SA (field = mcs_relatedprovidersite)
                 */

                // If there is a Provider Site on the Service Appointment

                if (ServiceAppointment.mcs_relatedprovidersite != null)
                {
                    // Get the Provider Site record

                    Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: ServiceAppointment.mcs_relatedprovidersite != null");

                    proSite = context.mcs_siteSet.FirstOrDefault(pro => pro.Id == ServiceAppointment.mcs_relatedprovidersite.Id);

                    if (proSite != null)
                    {
                        /*
                        *  Get Provider Facility 
                        */

                        if (proSite.mcs_FacilityId != null)
                        {
                            profacility = context.mcs_facilitySet.FirstOrDefault(x => x.Id == proSite.mcs_FacilityId.Id);
                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Id = {profacility.Id}");
                        }

                    }
                    else
                    {
                        Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: The Provider Site is null");
                    }

                }
                else
                {
                    Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): WARNING: ServiceAppointment.mcs_relatedprovidersite is null");
                }


                /*
                * 
                *  Determine if this is Vista vs Cerner situation.
                * 
                */

                // If both the patient and provider facilities are null, throw an exception. We can't make a decision if both facilities are null.

                if (patfacility == null && profacility == null)
                {
                    throw new Exception("CheckIfRelatedCernerFacility(): ERROR: Both the Patient Facility and Provider Facilities are null. This condition is not expected!");
                }

                else
                {
                    // If there is a patient facility

                    if (patfacility != null)
                    {

                        if (patfacility.Contains("cvt_facilitytype") && patfacility["cvt_facilitytype"] != null)
                        {
                            var osv1 = (OptionSetValue)patfacility["cvt_facilitytype"];

                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type - Option Set Value for Facility {patfacility.Id} is {osv1.Value}");

                            // NOT vista (i.e. Cerner)

                            if (osv1.Value != osvVista.Value)
                            {
                                isVista = false;
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type OptionSet Value = {osv1.Value}");
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type for mcs_facility = {patfacility.Id} != Vista, setting isVista = False");
                            }

                            //  Vista

                            else
                            {
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type for mcs_facility = {patfacility.Id} == {osvVista.Value} [Vista], isVista not changed here");
                            }

                        }
                        else
                        {
                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type (cvt_facilitytype) for mcs_facility = {patfacility.Id} is null");
                        }
                    }
                    else
                    {
                        Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: The Patient Facility is null on this service appt");
                    }

                    // If there is a provider facility

                    if (profacility != null)
                    {
                        if (profacility.Contains("cvt_facilitytype") && profacility["cvt_facilitytype"] != null)
                        {
                            var osv2 = (OptionSetValue)profacility["cvt_facilitytype"];

                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type - Option Set Value for Facility {profacility.Id} is {osv2.Value}");

                            // NOT vista (i.e. Cerner)

                            if (osv2.Value != osvVista.Value)
                            {
                                isVista = false;
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type OptionSet Value = {osv2.Value}");
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type for mcs_facility = {profacility.Id} != Vista, setting isVista = False");
                            }

                            //  Vista

                            else
                            {
                                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type for mcs_facility = {profacility.Id} == {osvVista.Value} [Vista], isVista not changed here.");
                            }

                        }
                        else
                        {
                            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type (cvt_facilitytype) for mcs_facility = {profacility.Id} is null");
                        }
                    }
                    else
                    {
                        Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: The Provider Facility is null on this service appt");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): ERROR: " + e.ToString());
                LogExit(Logger, 1);
                throw e;
            }

            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Final conclusion is that ServiceAppointment with ID = {ServiceAppointment.Id} {(isVista ? " *is* " : " *is NOT* ")} associated with a VistA Facility");

            Logger.WriteDebugMessage($"CheckIfRelatedCernerFacility(): INFO: Exit");

            LogExit(Logger, 2);
            return isVista;
        }

        /// <summary>
        /// Determines if the facilities associated with a given Service Appointment (or Appointment, if the optional last parameter is specified)
        /// is related to a site that is related to a Facility that has a Facility Type of "Cerner Millennium".
        /// </summary>
        /// 
        /// <returns>
        /// True if either the Patient or Provider side is NOT related to a VistA Facility (i.e. Cerner)
        /// False if (both the Patient and Provider side are related to a VistA Facility)
        /// </returns>
        public static bool CheckIfRelatedCernerFacility(VA.TMP.DataModel.ServiceAppointment ServiceAppointment, Xrm context, PluginLogger Logger, VA.TMP.DataModel.Appointment Appointment = null)
        {

            LogEntry(Logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Enter");
            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Service Appointment id passed in = {ServiceAppointment?.Id.ToString() ?? ""}");
            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Reserve Resource id passed in = {Appointment?.Id.ToString() ?? ""}");

            bool isVista = true;

            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: isVista = true by default");

            try
            {
                var osvVista = new OptionSetValue(917290001);

                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: osvVista for comparision has value {osvVista.Value}");

                mcs_site patSite = null;
                mcs_site proSite = null;
                mcs_facility profacility = null;
                mcs_facility patfacility = null;

                /*
                 *  Get Patient Site
                 *  
                 *  There are two ways to get the patient site, depending on the conditions:
                 *  
                 *  (1) If the SA is NOT a group appointment AND there is a Patient Site (mcs_relatedsite) on the SA, use the Patient Site (mcs_relatedsite) on the SA.
                 *  (2) If the SA is a group appointment, get the Appointment record associated with the SA and then get the Patient Site (cvt_Site) associated with the Appointment. 
                 */

                // If this is not a group appointment and there is a Patient Site (mcs_relatedsite)

                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: GroupAppointment = " + ServiceAppointment.mcs_groupappointment);

                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: mcs_relatedsite = " + ServiceAppointment?.mcs_relatedsite?.Id);

                if (!(bool)ServiceAppointment.mcs_groupappointment && ServiceAppointment.mcs_relatedsite != null)
                {
                    patSite = context.mcs_siteSet.FirstOrDefault(pat => pat.Id == ServiceAppointment.mcs_relatedsite.Id);
                    Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: This is not a group appt, so got patSite from sa.mcs_relatedsite. The id is = " + patSite.Id);
                }

                // Else, if this is a group appointment

                else if ((bool)ServiceAppointment.mcs_groupappointment)
                {
                    Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Group Appt: ServiceAppointment.mcs_groupappointment == true");

                    // Get the appointment passed in, if it exists, otherwise, iterate the list of reserve resources associated with the SA
                    // on the record to see if there is anything (pick the first one).

                    var apptIdFromReserveResources = GetRelatedReserveResourcesAssociatedWithServiceAppointmentAndPatientSide(ServiceAppointment, context, Logger);

                    Guid first_apptIdFromReserveResources = Guid.Empty;

                    if (apptIdFromReserveResources.Count > 0)
                    {
                        first_apptIdFromReserveResources = apptIdFromReserveResources.First().Id;
                    }

                    var apptId = (Appointment != null ? Appointment.Id : first_apptIdFromReserveResources);

                    // If there is an Appointment record associated with the service appointment

                    if (apptId != null && apptId != Guid.Empty)
                    {
                        Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Group Appt: apptId = {apptId}");

                        var appt = context.AppointmentSet.FirstOrDefault(x => x.Id == apptId);

                        // If the appointment identified exists in TMP

                        if (appt != null)
                        {
                            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Group Appt: appt = " + appt.Id);

                            // Get the patient site from the Appointment

                            var patientSiteId = appt.cvt_Site?.Id;

                            if (patientSiteId != null)
                            {
                                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Group Appt: patientSiteId = " + patientSiteId);

                                patSite = context.mcs_siteSet.FirstOrDefault(s => s.Id == patientSiteId);

                                if (patSite != null)
                                {
                                    Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Group Appt: Got Patient Site from Appointment linked to SA. patientSite Id = " + patSite.Id);
                                }
                                else
                                {
                                    Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: Could not retrieve patient site for Appointment {appt.Id}, Site id = {patientSiteId}, this is not expected");
                                }

                            }
                            else
                            {
                                Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: No site (cvt_site) associated with Appointment {appt.Id}");
                            }

                        }
                        else
                        {
                            Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: Appointment with id {apptId} is null");
                        }

                    }
                    else
                    {
                        Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: Group Appt: (Patient side) : Could not find reserve resources associated with service appointment with id = {ServiceAppointment.Id}");
                    }

                }

                /*
                 *  Get Patient Facility
                 */


                // If the Patient Site exists

                if (patSite != null)
                {
                    // If the Facility on the Patient Site is not null

                    if (patSite.mcs_FacilityId != null)
                    {
                        patfacility = context.mcs_facilitySet.FirstOrDefault(x => x.Id == patSite.mcs_FacilityId.Id);

                        if (patfacility == null)
                            Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: The Patient Facility is null");
                    }
                    else
                    {
                        Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: The Patient Facility is missing on Patient Site = {patSite.Id}");
                    }
                }
                else
                {
                    Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: The Patient Site is null");
                }

                /*
                 *  Get Provider Site 
                 *  There is only one place to get this - from the SA (field = mcs_relatedprovidersite)
                 */

                // If there is a Provider Site on the Service Appointment

                if (ServiceAppointment.mcs_relatedprovidersite != null)
                {
                    // Get the Provider Site record

                    Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: ServiceAppointment.mcs_relatedprovidersite != null");

                    proSite = context.mcs_siteSet.FirstOrDefault(pro => pro.Id == ServiceAppointment.mcs_relatedprovidersite.Id);

                    if (proSite != null)
                    {
                        /*
                        *  Get Provider Facility 
                        */

                        if (proSite.mcs_FacilityId != null)
                        {
                            profacility = context.mcs_facilitySet.FirstOrDefault(x => x.Id == proSite.mcs_FacilityId.Id);
                            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Id = {profacility.Id}");
                        }

                    }
                    else
                    {
                        Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: The Provider Site is null");
                    }

                }
                else
                {
                    Logger.Trace($"CheckIfRelatedCernerFacility(): WARNING: ServiceAppointment.mcs_relatedprovidersite is null");
                }


                /*
                * 
                *  Determine if this is Vista vs Cerner situation.
                * 
                */

                // If both the patient and provider facilities are null, throw an exception. We can't make a decision if both facilities are null.

                if (patfacility == null && profacility == null)
                {
                    throw new Exception("CheckIfRelatedCernerFacility(): ERROR: Both the Patient Facility and Provider Facilities are null. This condition is not expected!");
                }

                else
                {
                    // If there is a patient facility

                    if (patfacility != null)
                    {

                        if (patfacility.Contains("cvt_facilitytype") && patfacility["cvt_facilitytype"] != null)
                        {
                            var osv1 = (OptionSetValue)patfacility["cvt_facilitytype"];

                            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type - Option Set Value for Facility {patfacility.Id} is {osv1.Value}");

                            // NOT vista (i.e. Cerner)

                            if (osv1.Value != osvVista.Value)
                            {
                                isVista = false;
                                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type OptionSet Value = {osv1.Value}");
                                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type for mcs_facility = {patfacility.Id} != Vista, setting isVista = False");
                            }

                            //  Vista

                            else
                            {
                                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type for mcs_facility = {patfacility.Id} == {osvVista.Value} [Vista], isVista not changed here");
                            }

                        }
                        else
                        {
                            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Patient Facility Type (cvt_facilitytype) for mcs_facility = {patfacility.Id} is null");
                        }
                    }
                    else
                    {
                        Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: The Patient Facility is null on this service appt");
                    }

                    // If there is a provider facility

                    if (profacility != null)
                    {
                        if (profacility.Contains("cvt_facilitytype") && profacility["cvt_facilitytype"] != null)
                        {
                            var osv2 = (OptionSetValue)profacility["cvt_facilitytype"];

                            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type - Option Set Value for Facility {profacility.Id} is {osv2.Value}");

                            // NOT vista (i.e. Cerner)

                            if (osv2.Value != osvVista.Value)
                            {
                                isVista = false;
                                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type OptionSet Value = {osv2.Value}");
                                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type for mcs_facility = {profacility.Id} != Vista, setting isVista = False");
                            }

                            //  Vista

                            else
                            {
                                Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type for mcs_facility = {profacility.Id} == {osvVista.Value} [Vista], isVista not changed here.");
                            }

                        }
                        else
                        {
                            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Provider Facility Type (cvt_facilitytype) for mcs_facility = {profacility.Id} is null");
                        }
                    }
                    else
                    {
                        Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: The Provider Facility is null on this service appt");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Trace($"CheckIfRelatedCernerFacility(): ERROR: " + e.ToString());
                LogExit(Logger, 1);
                throw e;
            }

            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Final conclusion is that ServiceAppointment with ID = {ServiceAppointment.Id} {(isVista ? " *is* " : " *is NOT* ")} associated with a VistA Facility");

            Logger.Trace($"CheckIfRelatedCernerFacility(): INFO: Exit");

            LogExit(Logger, 2);
            return isVista;
        }

        /// <summary>
        /// Determines if the facilities associated with a given Reserve Resource 
        /// is related to a site that is related to a Facility that has a Facility Type of "Cerner Millennium".
        /// </summary>
        /// 
        /// <returns>
        /// True if either the Patient or Provider side is NOT related to a VistA Facility (i.e. Cerner)
        /// False if (both the Patient and Provider side are related to a VistA Facility)
        /// </returns>
        public static bool CheckIfRelatedCernerFacility(VA.TMP.DataModel.Appointment appt, Xrm context, MCSLogger Logger)
        {

            LogEntry(Logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}(Appointment)";

            Logger.WriteDebugMessage($"{classAndMethod} : INFO: Appointment id = {appt?.Id.ToString() ?? ""}");
            Logger.WriteDebugMessage($"{classAndMethod} : INFO: Getting Service Appointment for Appointment id {appt?.Id.ToString() ?? ""}");

            VA.TMP.DataModel.ServiceAppointment serviceAppt = null;

            if (appt.cvt_serviceactivityid != null)
            {
                Entity ServiceAppointmentEntity = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == appt.cvt_serviceactivityid.Id);

                if (ServiceAppointmentEntity != null)
                {
                    serviceAppt = (VA.TMP.DataModel.ServiceAppointment)ServiceAppointmentEntity;

                    Logger.WriteDebugMessage($"{classAndMethod} : INFO: Calling overloaded method CheckIfRelatedCernerFacility(ServiceAppointment)");

                    var isVista = CheckIfRelatedCernerFacility(serviceAppt, context, Logger, appt);

                    Logger.WriteDebugMessage($"{classAndMethod} : INFO : Final conclusion is that Reserve Resource with ID = {appt.Id} {(isVista ? " *is* " : " *is NOT* ")} associated with a VistA Facility");

                    LogExit(Logger, 1);

                    return isVista;
                }
                else
                {
                    LogExit(Logger, 2);
                    throw new Exception($"{classAndMethod} : ERROR : Can't find a service appointment associated with appointment (id = {appt.Id})");
                }
            }
            else
            {
                LogExit(Logger, 3);
                throw new Exception($"{classAndMethod} : ERROR : cvt_serviceactivityid on service appointment is null");
            }

            LogExit(Logger, 4);
        }

        /// <summary>
        /// Determines if the facilities associated with a given Reserve Resource 
        /// is related to a site that is related to a Facility that has a Facility Type of "Cerner Millennium".
        /// </summary>
        /// 
        /// <returns>
        /// True if either the Patient or Provider side is NOT related to a VistA Facility (i.e. Cerner)
        /// False if (both the Patient and Provider side are related to a VistA Facility)
        /// </returns>
        public static bool CheckIfRelatedCernerFacility(VA.TMP.DataModel.Appointment appt, Xrm context, PluginLogger Logger)
        {

            LogEntry(Logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}(Appointment)";

            Logger.Trace($"{classAndMethod} : INFO: Appointment id = {appt?.Id.ToString() ?? ""}");
            Logger.Trace($"{classAndMethod} : INFO: Getting Service Appointment for Appointment id {appt?.Id.ToString() ?? ""}");

            VA.TMP.DataModel.ServiceAppointment serviceAppt = null;

            if (appt.cvt_serviceactivityid != null)
            {
                Entity ServiceAppointmentEntity = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == appt.cvt_serviceactivityid.Id);

                if (ServiceAppointmentEntity != null)
                {
                    serviceAppt = (VA.TMP.DataModel.ServiceAppointment)ServiceAppointmentEntity;

                    Logger.Trace($"{classAndMethod} : INFO: Calling overloaded method CheckIfRelatedCernerFacility(ServiceAppointment)");

                    var isVista = CheckIfRelatedCernerFacility(serviceAppt, context, Logger, appt);

                    Logger.Trace($"{classAndMethod} : INFO : Final conclusion is that Reserve Resource with ID = {appt.Id} {(isVista ? " *is* " : " *is NOT* ")} associated with a VistA Facility");

                    LogExit(Logger, 1);

                    return isVista;
                }
                else
                {
                    LogExit(Logger, 2);
                    throw new Exception($"{classAndMethod} : ERROR : Can't find a service appointment associated with appointment (id = {appt.Id})");
                }
            }
            else
            {
                LogExit(Logger, 3);
                throw new Exception($"{classAndMethod} : ERROR : cvt_serviceactivityid on service appointment is null");
            }

            LogExit(Logger, 4);
        }

        public static TmpCernerOutboundResponseMessage FireLogicApp(EntityReference erEntity, OrganizationServiceContext _svcContext, IOrganizationService svc, MCSLogger logger, List<Guid> patients, string operation, string entityType = "")
        {

            LogEntry(logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            Stopwatch timer = new Stopwatch();
            timer.Start();

            var responseMessage = new TmpCernerOutboundResponseMessage();

            bool failed = true; // Assume failure until proven otherwise. 

            string _apiCallResult = string.Empty;
            //string _appAddress = string.Empty;
            string _errMsg = string.Empty;
            //string _cernerKey = string.Empty;
            string _entityId = erEntity.Id.ToString().Replace("{", "").Replace("}", "");
            string _strJSON = string.Empty;
            string _strToday = string.Empty;
            string _serviceAppointmentId = string.Empty;
            string _appointmentId = string.Empty;
            var integrationSettings = default(ApiIntegrationSettings);

            string PluginCorrelationId = new Random().Next(10000000, 99999999).ToString() + new Random().Next(10000000, 99999999).ToString() + new Random().Next(1000, 9999).ToString();

            try
            {
                logger.WriteDebugMessage($"{classAndMethod} : DEBUG : In {classAndMethod}");
                //String serviceApptId = "";

                // Get the service appointment ID from the Appointment

                if (entityType == "ReserveResource")
                {
                    _appointmentId = erEntity.Id.ToString();

                    logger.WriteDebugMessage($"{classAndMethod} : DEBUG : In {classAndMethod} - {_appointmentId}");

                    using (var sv = new Xrm(svc))
                    {
                        var apptReference = sv.AppointmentSet.FirstOrDefault(x => x.Id == erEntity.Id);

                        if (apptReference != null)
                        {
                            var serviceAppt = apptReference.cvt_serviceactivityid;

                            if (serviceAppt != null)
                            {
                                var msg = $"{classAndMethod} : DEBUG : Got service appointment id {_serviceAppointmentId} from reserve resource {erEntity.Id}";
                                _serviceAppointmentId = serviceAppt.Id.ToString();
                                logger.WriteDebugMessage(msg);

                            }
                            else
                            {
                                var msg = $"{classAndMethod} : DEBUG : There was no service appointment associated with this reserve resources ({serviceAppt.Id}). This is not expected. Terminating.";
                                logger.WriteDebugMessage(msg);
                                LogExit(logger, 1);
                                throw new InvalidPluginExecutionException(msg);
                            }
                        }
                        else
                        {
                            var msg = $"{classAndMethod} : WARNING : Could not find reserve resource in CRM for appointment id = {erEntity.Id}";
                            logger.WriteDebugMessage(msg);
                            LogExit(logger, 2);
                            throw new InvalidPluginExecutionException(msg);
                        }

                    }

                }

                // Otherwise, this is a service appointment already.

                else
                {
                    _serviceAppointmentId = erEntity.Id.ToString();
                }

                logger.WriteDebugMessage($"{classAndMethod} : DEBUG : Create JavaScriptSerializer");

                _strToday = DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");

                var patientInfo = new PatientInfo(patients);
                var serializedPatientInfoList = patientInfo.GetSeralizedInstance();


                _strJSON = $"{{\"DateTimeMessageSent\":\"{_strToday}\",\"action\":\"{operation}\",\"Patients\":{serializedPatientInfoList},\"ServiceAppointmentId\":\"{_serviceAppointmentId}\",\"AppointmentId\":\"{_appointmentId}\",\"MessageSource\":\"Plugin\",\"PluginCorrelationId\":\"{PluginCorrelationId}\"}}";

                logger.WriteDebugMessage($"{classAndMethod} : DEBUG : _strJSON to post is \r\n" + _strJSON);
                logger.WriteDebugMessage($"{classAndMethod} : DEBUG : Create XRM service");

                using (var srv = new Xrm(svc))
                {
                    logger.WriteDebugMessage($"{classAndMethod} : Getting integration settings");

                    var settings = srv.mcs_integrationsettingSet.Select(x => new ApiIntegrationSettingsNameValuePair { Name = x.mcs_name, Value = x.mcs_value }).ToList();

                    integrationSettings = new ApiIntegrationSettings
                    {
                        LogicAppUri = settings.FirstOrDefault(x => x.Name == "CernerOutboundProcessHTTPEndpoint").Value,
                        IsProdApi = Convert.ToBoolean(settings.FirstOrDefault(x => x.Name == "IsProdApi").Value),
                        SubscriptionId = settings.FirstOrDefault(x => x.Name == "SubscriptionId").Value,
                        SubscriptionIdEast = settings.FirstOrDefault(x => x.Name == "SubscriptionIdEast").Value,
                        SubscriptionIdSouth = settings.FirstOrDefault(x => x.Name == "SubscriptionIdSouth").Value
                    };

                    //_appAddress = integrationSettings.LogicAppUri.ToString();
                    //_cernerKey = integrationSettings.CernerKey.ToString();

                    logger.WriteDebugMessage($"CernerHelper.FireLogicApp() : DEBUG : LogicAppUri is \r\n{integrationSettings.LogicAppUri}");
                    logger.WriteDebugMessage($"CernerHelper.FireLogicApp() : DEBUG : IsProdApi is \r\n{integrationSettings.IsProdApi}");
                    logger.WriteDebugMessage($"CernerHelper.FireLogicApp() : DEBUG : SubscriptionId is \r\n{integrationSettings.SubscriptionId}");
                    logger.WriteDebugMessage($"CernerHelper.FireLogicApp() : DEBUG : SubscriptionIdEast is \r\n{integrationSettings.SubscriptionIdEast}");
                    logger.WriteDebugMessage($"CernerHelper.FireLogicApp() : DEBUG : SubscriptionIdSouth is \r\n{integrationSettings.SubscriptionIdSouth}");

                }

                /******************************************************************************************************************
                 The HTTP request will be a POST request to the URL in the “CernerOutboundProcessHTTPEndpoint” Integration Setting. 
                 The HTTP request will need two headers added to it: 
                         Content-Type: application/json
                         Ocp-Apim-Subscription-Key 
                 ******************************************************************************************************************/

                logger.WriteDebugMessage($"{classAndMethod} : DEBUG : content to send to client is \r\n" + _strJSON);

                try
                {
                    responseMessage.RequestMessage = $"URL: {integrationSettings.LogicAppUri}\r\n\r\nRequest Body:\r\n{_strJSON}";

                    try
                    {
                        var responseDictionary = Retry.Do(async () => await GetHttpResponseAsync(integrationSettings, _strJSON, logger), TimeSpan.FromSeconds(10), 3);

                        logger.WriteDebugMessage($"{classAndMethod} : INFO : Set _apiCallResult");
                        _apiCallResult = responseDictionary.Result.First().Value;

                        logger.WriteDebugMessage($"{classAndMethod} : SUCCESS : Cerner outbound HTTP endpoint call successful. Response is " + responseDictionary.Result.First().Value);

                        logger.WriteDebugMessage($"{classAndMethod} : INFO : Set ResponseMessage");
                        responseMessage.ResponseMessage = _apiCallResult;
                        responseMessage.ExceptionOccured = false;
                        failed = false;

                    }
                    catch (AggregateException ex)
                    {
                        failed = true;
                        responseMessage.ExceptionOccured = true;

                        foreach (var excep in ex.InnerExceptions)
                        {
                            // Check for JSON in the body of the error. This would be the case for a 402
                            // exception from the logic app where there is more diagnostic information.

                            if (excep.Data.Contains("HTTPRequestException") && excep.Data["HTTPRequestException"] != null)
                            {
                                var httpRequestException = (HttpRequestException)excep.Data["HTTPRequestException"];
                                _errMsg += "HTTPRequestException : " + httpRequestException.ToString() + "\r\n\r\n";
                            }

                            if (excep.Data.Contains("HTTPResponseMessage") && excep.Data["HTTPResponseMessage"] != null && (excep.Data["HTTPResponseMessage"] as HttpResponseMessage).Content.ToString().Contains("{ "))
                            {
                                try
                                {
                                    var httpResp = (HttpResponseMessage)excep.Data["HTTPResponseMessage"];

                                    var responseString = httpResp.Content.ReadAsStringAsync().Result;

                                    var json = new JavaScriptSerializer().Deserialize<dynamic>(httpResp.Content.ToString());

                                    if (((IDictionary<string, object>)json).ContainsKey("Errors") && json.Errors.Length > 0)
                                        _errMsg += "Error from Logic app : " + json.Errors[0].ToString() + "\r\n\r\n";
                                }
                                catch (Exception e)
                                {
                                    _errMsg += "Error deserializing error response : " + e.ToString() + "\r\n\r\n";
                                }

                            }

                            if (!excep.Data.Contains("HTTPResponseMessage") && !excep.Data.Contains("HTTPRequestException"))
                            {
                                _errMsg += "Generic exception : " + excep.ToString() + "\r\n\r\n";
                            }

                        }

                        logger.WriteDebugMessage($"{classAndMethod} : ERROR : A fatal error occured posting to Cerner outbound endpoint. There were 3 attempts. Errors were:" + _errMsg);
                    }

                }
                catch (Exception mainExcep)
                {
                    _errMsg += mainExcep.ToString();
                    logger.WriteDebugMessage($"{classAndMethod} : ERROR : A fatal error occured posting to HC endpoint for Cerner outbound process. There were 3 attempts. Errors were:" + _errMsg);
                    responseMessage.ExceptionOccured = true;
                    responseMessage.ResponseMessage = _apiCallResult;
                }

                responseMessage.ExceptionMessage = _errMsg;

                writeIntegrationResult(erEntity, _strJSON, _errMsg, _apiCallResult, svc, _svcContext, logger);

            }
            catch (Exception e)
            {
                responseMessage.ExceptionMessage = $"{classAndMethod} : ERROR : Fatal error : " + e.ToString();
                responseMessage.ExceptionOccured = true;
            }

            timer.Stop();

            responseMessage.MessageProcessingTime = (int)timer.ElapsedMilliseconds;
            responseMessage.ControlId = PluginCorrelationId; // User by Phil Buckhalter in HC

            logger.WriteDebugMessage($"{classAndMethod} : DEBUG : Exit method");

            LogExit(logger, 3);

            return responseMessage;

        }

        public static TmpCernerOutboundResponseMessage FireLogicApp(EntityReference erEntity, OrganizationServiceContext _svcContext, IOrganizationService svc, PluginLogger logger, List<Guid> patients, string operation, string entityType = "")
        {

            LogEntry(logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            Stopwatch timer = new Stopwatch();
            timer.Start();

            var responseMessage = new TmpCernerOutboundResponseMessage();

            bool failed = true; // Assume failure until proven otherwise. 

            string _apiCallResult = string.Empty;
            //string _appAddress = string.Empty;
            string _errMsg = string.Empty;
            //string _cernerKey = string.Empty;
            string _entityId = erEntity.Id.ToString().Replace("{", "").Replace("}", "");
            string _strJSON = string.Empty;
            string _strToday = string.Empty;
            string _serviceAppointmentId = string.Empty;
            string _appointmentId = string.Empty;
            var integrationSettings = default(ApiIntegrationSettings);

            string PluginCorrelationId = new Random().Next(10000000, 99999999).ToString() + new Random().Next(10000000, 99999999).ToString() + new Random().Next(1000, 9999).ToString();

            try
            {
                logger.Trace($"{classAndMethod} : DEBUG : In {classAndMethod}");
                //String serviceApptId = "";

                // Get the service appointment ID from the Appointment

                if (entityType == "ReserveResource")
                {
                    _appointmentId = erEntity.Id.ToString();

                    logger.Trace($"{classAndMethod} : DEBUG : In {classAndMethod} - {_appointmentId}");

                    using (var sv = new Xrm(svc))
                    {
                        var apptReference = sv.AppointmentSet.FirstOrDefault(x => x.Id == erEntity.Id);

                        if (apptReference != null)
                        {
                            var serviceAppt = apptReference.cvt_serviceactivityid;

                            if (serviceAppt != null)
                            {
                                var msg = $"{classAndMethod} : DEBUG : Got service appointment id {_serviceAppointmentId} from reserve resource {erEntity.Id}";
                                _serviceAppointmentId = serviceAppt.Id.ToString();
                                logger.Trace(msg);

                            }
                            else
                            {
                                var msg = $"{classAndMethod} : DEBUG : There was no service appointment associated with this reserve resources ({serviceAppt.Id}). This is not expected. Terminating.";
                                logger.Trace(msg);
                                LogExit(logger, 1);
                                throw new InvalidPluginExecutionException(msg);
                            }
                        }
                        else
                        {
                            var msg = $"{classAndMethod} : WARNING : Could not find reserve resource in CRM for appointment id = {erEntity.Id}";
                            logger.Trace(msg);
                            LogExit(logger, 2);
                            throw new InvalidPluginExecutionException(msg);
                        }

                    }

                }

                // Otherwise, this is a service appointment already.

                else
                {
                    _serviceAppointmentId = erEntity.Id.ToString();
                }

                logger.Trace($"{classAndMethod} : DEBUG : Create JavaScriptSerializer");

                _strToday = DateTime.Now.ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss");

                var patientInfo = new PatientInfo(patients);
                var serializedPatientInfoList = patientInfo.GetSeralizedInstance();


                _strJSON = $"{{\"DateTimeMessageSent\":\"{_strToday}\",\"action\":\"{operation}\",\"Patients\":{serializedPatientInfoList},\"ServiceAppointmentId\":\"{_serviceAppointmentId}\",\"AppointmentId\":\"{_appointmentId}\",\"MessageSource\":\"Plugin\",\"PluginCorrelationId\":\"{PluginCorrelationId}\"}}";

                logger.Trace($"{classAndMethod} : DEBUG : _strJSON to post is \r\n" + _strJSON);
                logger.Trace($"{classAndMethod} : DEBUG : Create XRM service");

                using (var srv = new Xrm(svc))
                {
                    logger.Trace($"{classAndMethod} : Getting integration settings");

                    var settings = srv.mcs_integrationsettingSet.Select(x => new ApiIntegrationSettingsNameValuePair { Name = x.mcs_name, Value = x.mcs_value }).ToList();

                    integrationSettings = new ApiIntegrationSettings
                    {
                        LogicAppUri = settings.FirstOrDefault(x => x.Name == "CernerOutboundProcessHTTPEndpoint").Value,
                        IsProdApi = Convert.ToBoolean(settings.FirstOrDefault(x => x.Name == "IsProdApi").Value),
                        SubscriptionId = settings.FirstOrDefault(x => x.Name == "SubscriptionId").Value,
                        SubscriptionIdEast = settings.FirstOrDefault(x => x.Name == "SubscriptionIdEast").Value,
                        SubscriptionIdSouth = settings.FirstOrDefault(x => x.Name == "SubscriptionIdSouth").Value
                    };

                    //_appAddress = integrationSettings.LogicAppUri.ToString();
                    //_cernerKey = integrationSettings.CernerKey.ToString();

                    logger.Trace($"CernerHelper.FireLogicApp() : DEBUG : LogicAppUri is \r\n{integrationSettings.LogicAppUri}");
                    logger.Trace($"CernerHelper.FireLogicApp() : DEBUG : IsProdApi is \r\n{integrationSettings.IsProdApi}");
                    logger.Trace($"CernerHelper.FireLogicApp() : DEBUG : SubscriptionId is \r\n{integrationSettings.SubscriptionId}");
                    logger.Trace($"CernerHelper.FireLogicApp() : DEBUG : SubscriptionIdEast is \r\n{integrationSettings.SubscriptionIdEast}");
                    logger.Trace($"CernerHelper.FireLogicApp() : DEBUG : SubscriptionIdSouth is \r\n{integrationSettings.SubscriptionIdSouth}");

                }

                /******************************************************************************************************************
                 The HTTP request will be a POST request to the URL in the “CernerOutboundProcessHTTPEndpoint” Integration Setting. 
                 The HTTP request will need two headers added to it: 
                         Content-Type: application/json
                         Ocp-Apim-Subscription-Key 
                 ******************************************************************************************************************/

                logger.Trace($"{classAndMethod} : DEBUG : content to send to client is \r\n" + _strJSON);

                try
                {
                    responseMessage.RequestMessage = $"URL: {integrationSettings.LogicAppUri}\r\n\r\nRequest Body:\r\n{_strJSON}";

                    try
                    {
                        var responseDictionary = Retry.Do(async () => await GetHttpResponseAsync(integrationSettings, _strJSON, logger), TimeSpan.FromSeconds(10), 3);

                        logger.Trace($"{classAndMethod} : INFO : Set _apiCallResult");
                        _apiCallResult = responseDictionary.Result.First().Value;

                        logger.Trace($"{classAndMethod} : SUCCESS : Cerner outbound HTTP endpoint call successful. Response is " + responseDictionary.Result.First().Value);

                        logger.Trace($"{classAndMethod} : INFO : Set ResponseMessage");
                        responseMessage.ResponseMessage = _apiCallResult;
                        responseMessage.ExceptionOccured = false;
                        failed = false;

                    }
                    catch (AggregateException ex)
                    {
                        failed = true;
                        responseMessage.ExceptionOccured = true;

                        foreach (var excep in ex.InnerExceptions)
                        {
                            // Check for JSON in the body of the error. This would be the case for a 402
                            // exception from the logic app where there is more diagnostic information.

                            if (excep.Data.Contains("HTTPRequestException") && excep.Data["HTTPRequestException"] != null)
                            {
                                var httpRequestException = (HttpRequestException)excep.Data["HTTPRequestException"];
                                _errMsg += "HTTPRequestException : " + httpRequestException.ToString() + "\r\n\r\n";
                            }

                            if (excep.Data.Contains("HTTPResponseMessage") && excep.Data["HTTPResponseMessage"] != null && (excep.Data["HTTPResponseMessage"] as HttpResponseMessage).Content.ToString().Contains("{ "))
                            {
                                try
                                {
                                    var httpResp = (HttpResponseMessage)excep.Data["HTTPResponseMessage"];

                                    var responseString = httpResp.Content.ReadAsStringAsync().Result;

                                    var json = new JavaScriptSerializer().Deserialize<dynamic>(httpResp.Content.ToString());

                                    if (((IDictionary<string, object>)json).ContainsKey("Errors") && json.Errors.Length > 0)
                                        _errMsg += "Error from Logic app : " + json.Errors[0].ToString() + "\r\n\r\n";
                                }
                                catch (Exception e)
                                {
                                    _errMsg += "Error deserializing error response : " + e.ToString() + "\r\n\r\n";
                                }

                            }

                            if (!excep.Data.Contains("HTTPResponseMessage") && !excep.Data.Contains("HTTPRequestException"))
                            {
                                _errMsg += "Generic exception : " + excep.ToString() + "\r\n\r\n";
                            }

                        }

                        logger.Trace($"{classAndMethod} : ERROR : A fatal error occured posting to Cerner outbound endpoint. There were 3 attempts. Errors were:" + _errMsg, LogLevel.Error);
                    }

                }
                catch (Exception mainExcep)
                {
                    _errMsg += mainExcep.ToString();
                    logger.Trace($"{classAndMethod} : ERROR : A fatal error occured posting to HC endpoint for Cerner outbound process. There were 3 attempts. Errors were:" + _errMsg, LogLevel.Error);
                    responseMessage.ExceptionOccured = true;
                    responseMessage.ResponseMessage = _apiCallResult;
                }

                responseMessage.ExceptionMessage = _errMsg;

                writeIntegrationResult(erEntity, _strJSON, _errMsg, _apiCallResult, svc, _svcContext, logger);

            }
            catch (Exception e)
            {
                responseMessage.ExceptionMessage = $"{classAndMethod} : ERROR : Fatal error : " + e.ToString();
                responseMessage.ExceptionOccured = true;
            }

            timer.Stop();

            responseMessage.MessageProcessingTime = (int)timer.ElapsedMilliseconds;
            responseMessage.ControlId = PluginCorrelationId; // User by Phil Buckhalter in HC

            logger.Trace($"{classAndMethod} : DEBUG : Exit method");

            LogExit(logger, 3);

            return responseMessage;

        }

        private static async Task<Dictionary<HttpResponseMessage, string>> GetHttpResponseAsync(ApiIntegrationSettings integrationSettings, string json, MCSLogger logger)
        {
            LogEntry(logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            logger.WriteDebugMessage($"{classAndMethod} : DEBUG : Constructing Http client");

            // Construct the HTTP client

            using (var client = new HttpClient())
            {
                // Construct the HTTPRequestMessage

                var _reqMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(json, Encoding.ASCII, "application/json")
                };

                // Add headers
                if (integrationSettings.IsProdApi)
                {
                    _reqMessage.Headers.Add("Ocp-Apim-Subscription-Key-E", integrationSettings.SubscriptionIdEast);
                    _reqMessage.Headers.Add("Ocp-Apim-Subscription-Key-S", integrationSettings.SubscriptionIdSouth);
                }
                else
                {
                    _reqMessage.Headers.Add("Ocp-Apim-Subscription-Key", integrationSettings.SubscriptionId);
                }

                //_reqMessage.Headers.Add("Ocp-Apim-Subscription-Key", cernerKey);

                // Set the request URI

                _reqMessage.RequestUri = new Uri(integrationSettings.LogicAppUri);

                // Post the JSON

                logger.WriteDebugMessage($"{classAndMethod} :  DEBUG : Posting JSON");

                var httpResponseMessage = await client.SendAsync(_reqMessage);

                var responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;

                logger.WriteDebugMessage($"{classAndMethod} : DEBUG : ResponseString = " + responseString);

                try
                {
                    var retDict = new Dictionary<HttpResponseMessage, string>();
                    retDict.Add(httpResponseMessage.EnsureSuccessStatusCode(), responseString); // EnsureSuccessStatusCode() throws exception if non-http success code.
                    LogExit(logger, 2);
                    return retDict;
                }
                catch (Exception e)
                {
                    if (e is HttpRequestException)
                    {
                        // Throw exception with more information in it.

                        var exception = new Exception();
                        exception.Data.Add("HTTPResponseMessage", httpResponseMessage);
                        exception.Data.Add("HttpRequestException", e);
                        throw exception;
                    }
                    else
                    {
                        LogExit(logger, 3);
                        // Rethrow otherwise
                        throw;
                    }

                }
            }

            LogExit(logger, 4);

        }

        private static async Task<Dictionary<HttpResponseMessage, string>> GetHttpResponseAsync(ApiIntegrationSettings integrationSettings, string json, PluginLogger logger)
        {
            LogEntry(logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            logger.Trace($"{classAndMethod} : DEBUG : Constructing Http client");

            // Construct the HTTP client

            using (var client = new HttpClient())
            {
                // Construct the HTTPRequestMessage

                var _reqMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(json, Encoding.ASCII, "application/json")
                };

                // Add headers
                if (integrationSettings.IsProdApi)
                {
                    _reqMessage.Headers.Add("Ocp-Apim-Subscription-Key-E", integrationSettings.SubscriptionIdEast);
                    _reqMessage.Headers.Add("Ocp-Apim-Subscription-Key-S", integrationSettings.SubscriptionIdSouth);
                }
                else
                {
                    _reqMessage.Headers.Add("Ocp-Apim-Subscription-Key", integrationSettings.SubscriptionId);
                }

                //_reqMessage.Headers.Add("Ocp-Apim-Subscription-Key", cernerKey);

                // Set the request URI

                _reqMessage.RequestUri = new Uri(integrationSettings.LogicAppUri);

                // Post the JSON

                logger.Trace($"{classAndMethod} :  DEBUG : Posting JSON");

                var httpResponseMessage = await client.SendAsync(_reqMessage);

                var responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;

                logger.Trace($"{classAndMethod} : DEBUG : ResponseString = " + responseString);

                try
                {
                    var retDict = new Dictionary<HttpResponseMessage, string>();
                    retDict.Add(httpResponseMessage.EnsureSuccessStatusCode(), responseString); // EnsureSuccessStatusCode() throws exception if non-http success code.
                    LogExit(logger, 2);
                    return retDict;
                }
                catch (Exception e)
                {
                    if (e is HttpRequestException)
                    {
                        // Throw exception with more information in it.

                        var exception = new Exception();
                        exception.Data.Add("HTTPResponseMessage", httpResponseMessage);
                        exception.Data.Add("HttpRequestException", e);
                        throw exception;
                    }
                    else
                    {
                        LogExit(logger, 3);
                        // Rethrow otherwise
                        throw;
                    }

                }
            }

            LogExit(logger, 4);

        }

        public static void writeIntegrationResult(EntityReference erEntity, string payload, string errMsg, string apiCallResult, IOrganizationService svc, OrganizationServiceContext orgContext, MCSLogger logger)
        {
            LogEntry(logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            try
            {
                Entity enEntity = svc.Retrieve(erEntity.LogicalName, erEntity.Id, new ColumnSet(true));

                if (errMsg == string.Empty)
                {
                    errMsg = "No Reported Errors";
                }

                var integrationResult = new mcs_integrationresult
                {
                    mcs_name = $"{classAndMethod} : Cerner Outbound Integration Result (CernerHelper) - {enEntity.LogicalName}",
                    mcs_error = errMsg,
                    mcs_integrationrequest = payload,
                    mcs_serviceappointmentid = erEntity,
                    mcs_result = apiCallResult,
                    mcs_status = new OptionSetValue(803750002)
                };

                if (enEntity.Id != null && enEntity.Id != Guid.Empty)
                {
                    integrationResult.mcs_serviceappointmentid = new EntityReference(enEntity.LogicalName, enEntity.Id);
                }

                orgContext.AddObject(integrationResult);
                orgContext.SaveChanges();
            }
            catch (Exception e)
            {
                logger.WriteDebugMessage($"{classAndMethod} : WARNING : Cannot log to Integration Results. " + e.ToString());
            }

            LogExit(logger, 1);
        }

        public static void writeIntegrationResult(EntityReference erEntity, string payload, string errMsg, string apiCallResult, IOrganizationService svc, OrganizationServiceContext orgContext, PluginLogger logger)
        {
            LogEntry(logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            try
            {
                Entity enEntity = svc.Retrieve(erEntity.LogicalName, erEntity.Id, new ColumnSet(true));

                if (errMsg == string.Empty)
                {
                    errMsg = "No Reported Errors";
                }

                var integrationResult = new mcs_integrationresult
                {
                    mcs_name = $"{classAndMethod} : Cerner Outbound Integration Result (CernerHelper) - {enEntity.LogicalName}",
                    mcs_error = errMsg,
                    mcs_integrationrequest = payload,
                    mcs_serviceappointmentid = erEntity,
                    mcs_result = apiCallResult,
                    mcs_status = new OptionSetValue(803750002)
                };

                if (enEntity.Id != null && enEntity.Id != Guid.Empty)
                {
                    integrationResult.mcs_serviceappointmentid = new EntityReference(enEntity.LogicalName, enEntity.Id);
                }

                orgContext.AddObject(integrationResult);
                orgContext.SaveChanges();
            }
            catch (Exception e)
            {
                logger.Trace($"{classAndMethod} : WARNING : Cannot log to Integration Results. " + e.ToString(), LogLevel.Error);
            }

            LogExit(logger, 1);
        }

        /// <summary>
        /// Helper method to get all Reserve Resources associated with a Service Appointment.
        /// Normally, this should not be needed, but if the CheckIfCernerRelatedFacility method is called 
        /// in a group appointment scenario and a Reserve Resource is not passed in as a parameter, then this is a 
        /// fallback.
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="context"></param>
        /// <param name="Logger"></param>
        /// <returns></returns>
        public static List<VA.TMP.DataModel.Appointment> GetRelatedReserveResourcesAssociatedWithServiceAppointmentAndPatientSide(VA.TMP.DataModel.ServiceAppointment sa, Xrm context, MCSLogger Logger)
        {
            LogEntry(Logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            var reserveResourceList = new List<VA.TMP.DataModel.Appointment>();

            try
            {
                var reserveResources = context.AppointmentSet.Where(x => x.cvt_serviceactivityid != null && x.cvt_serviceactivityid.Id == sa.Id)?.ToList();

                if (reserveResources != null)
                {
                    foreach (var reserveResource in reserveResources)
                    {
                        var siteId = reserveResource?.cvt_Site?.Id;

                        if (siteId != null)
                        {
                            var site = context.mcs_siteSet.FirstOrDefault(x => x.Id == siteId);

                            if (site != null)
                            {
                                Logger.WriteDebugMessage($"{classAndMethod} : DEBUG : Got site with id = {siteId}");

                                var facilityId = site?.mcs_FacilityId?.Id;

                                if (facilityId != null)
                                {
                                    Logger.WriteDebugMessage($"{classAndMethod} : DEBUG : Facility id = {facilityId}");

                                    var associatedPatientParticipatingSite = context.cvt_participatingsiteSet.FirstOrDefault(x => x.cvt_facility != null && x.cvt_facility.Id == facilityId && x.cvt_locationtype != null && x.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient);

                                    if (associatedPatientParticipatingSite != null)
                                    {
                                        Logger.WriteDebugMessage($"{classAndMethod} : DEBUG : Found one reserve resource with id = {reserveResource.Id} to add from Patient side. Participating site id was = {associatedPatientParticipatingSite.Id}");
                                        reserveResourceList.Add(reserveResource);
                                    }
                                }
                                else
                                {
                                    Logger.WriteDebugMessage($"{classAndMethod} : DEBUG : facility == null");
                                }
                            }
                            else
                            {
                                Logger.WriteDebugMessage($"{classAndMethod} : DEBUG : site == null");
                            }

                        }
                        else
                        {
                            Logger.WriteDebugMessage($"{classAndMethod} : DEBUG : siteId == null");
                        }
                    }
                }
                else
                {
                    Logger.WriteDebugMessage($"{classAndMethod} : ERROR: There are no reserve resources associated with service appointment({sa.Id})");
                }

                if (reserveResourceList.Count == 0)
                {
                    Logger.WriteDebugMessage($"{classAndMethod} : ERROR: Did not find any reserve resources associated with this service appointment that were associated with participating sites on the Patient side.");
                }

            }
            catch (Exception e)
            {
                Logger.WriteDebugMessage($"{classAndMethod} : ERROR : {e}");
            }

            LogExit(Logger, 1);

            return reserveResourceList;

        }
        /// <summary>
        /// Helper method to get all Reserve Resources associated with a Service Appointment.
        /// Normally, this should not be needed, but if the CheckIfCernerRelatedFacility method is called 
        /// in a group appointment scenario and a Reserve Resource is not passed in as a parameter, then this is a 
        /// fallback.
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="context"></param>
        /// <param name="Logger"></param>
        /// <returns></returns>
        public static List<VA.TMP.DataModel.Appointment> GetRelatedReserveResourcesAssociatedWithServiceAppointmentAndPatientSide(VA.TMP.DataModel.ServiceAppointment sa, Xrm context, PluginLogger Logger)
        {
            LogEntry(Logger);

            var classAndMethod = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}.{MethodBase.GetCurrentMethod().Name}()";

            var reserveResourceList = new List<VA.TMP.DataModel.Appointment>();

            try
            {
                var reserveResources = context.AppointmentSet.Where(x => x.cvt_serviceactivityid != null && x.cvt_serviceactivityid.Id == sa.Id)?.ToList();

                if (reserveResources != null)
                {
                    foreach (var reserveResource in reserveResources)
                    {
                        var siteId = reserveResource?.cvt_Site?.Id;

                        if (siteId != null)
                        {
                            var site = context.mcs_siteSet.FirstOrDefault(x => x.Id == siteId);

                            if (site != null)
                            {
                                Logger.Trace($"{classAndMethod} : DEBUG : Got site with id = {siteId}");

                                var facilityId = site?.mcs_FacilityId?.Id;

                                if (facilityId != null)
                                {
                                    Logger.Trace($"{classAndMethod} : DEBUG : Facility id = {facilityId}");

                                    var associatedPatientParticipatingSite = context.cvt_participatingsiteSet.FirstOrDefault(x => x.cvt_facility != null && x.cvt_facility.Id == facilityId && x.cvt_locationtype != null && x.cvt_locationtype.Value == (int)cvt_participatingsitecvt_locationtype.Patient);

                                    if (associatedPatientParticipatingSite != null)
                                    {
                                        Logger.Trace($"{classAndMethod} : DEBUG : Found one reserve resource with id = {reserveResource.Id} to add from Patient side. Participating site id was = {associatedPatientParticipatingSite.Id}");
                                        reserveResourceList.Add(reserveResource);
                                    }
                                }
                                else
                                {
                                    Logger.Trace($"{classAndMethod} : DEBUG : facility == null");
                                }
                            }
                            else
                            {
                                Logger.Trace($"{classAndMethod} : DEBUG : site == null");
                            }

                        }
                        else
                        {
                            Logger.Trace($"{classAndMethod} : DEBUG : siteId == null");
                        }
                    }
                }
                else
                {
                    Logger.Trace($"{classAndMethod} : ERROR: There are no reserve resources associated with service appointment({sa.Id})");
                }

                if (reserveResourceList.Count == 0)
                {
                    Logger.Trace($"{classAndMethod} : ERROR: Did not find any reserve resources associated with this service appointment that were associated with participating sites on the Patient side.");
                }

            }
            catch (Exception e)
            {
                Logger.Trace($"{classAndMethod} : ERROR : {e}");
            }

            LogExit(Logger, 1);

            return reserveResourceList;

        }
        public class Patient
        {
            public string PatientId { get; set; }

        }
        public class PatientInfo
        {
            public List<Patient> PatientsList;
            public PatientInfo(List<Guid> patientsGuidList)
            {

                PatientsList = new List<Patient>();

                foreach (var item in patientsGuidList)
                {
                    PatientsList.Add(new Patient
                    {
                        PatientId = item.ToString()
                    });
                }

            }

            public String GetSeralizedInstance()
            {
                string seralizedResult = "";

                var serializer = new JavaScriptSerializer();

                if (PatientsList != null)
                {
                    seralizedResult = serializer.Serialize(PatientsList);
                }

                return seralizedResult;

            }
        }

        private static void LogEntry(MCSLogger Logger, [CallerMemberName] string memberName = "",
                        [CallerFilePath] string sourceFilePath = "",
                        [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}";
                Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");

            }
            catch (Exception e)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }
        }

        private static void LogEntry(PluginLogger Logger, [CallerMemberName] string memberName = "",
                        [CallerFilePath] string sourceFilePath = "",
                        [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}";
                Logger.Trace($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");

            }
            catch (Exception e)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }
        }

        private static void LogExit(MCSLogger Logger, int exitPoint,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        {

            try
            {

                string className = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}";
                Logger.WriteDebugMessage($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}");
            }
            catch (Exception e)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }
        }

        private static void LogExit(PluginLogger Logger, int exitPoint,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        {

            try
            {

                string className = $"{MethodBase.GetCurrentMethod().ReflectedType.Name}";
                Logger.Trace($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}");
            }
            catch (Exception e)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }
        }
    }
}
