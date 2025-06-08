using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using MCS.ApplicationInsights;
using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    /// <summary>
    ///  CRM Plugin Runner class to handle updating a ServiceAppointment.
    /// </summary>
    public class ServiceAppointmentVmrUpdatePostStageRunner : AILogicBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public ServiceAppointmentVmrUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        public bool Success { get; set; }

        /// <summary>
        /// Executes the plugin runner.
        /// </summary>
        public override void ExecuteLogic()
        {
            Success = false;
            bool VistaTypeFacility;

            try
            {
                LogEntry();

                //Logger.WriteDebugMessage("Proxy Add Completed, beginning VMR Generation");
                Trace("Proxy Add Completed, beginning VMR Generation.", LogLevel.Debug);

                using (var context = new Xrm(OrganizationService))
                {
                    // Retrieve the Service Appointment.
                    var serviceAppointment = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == PrimaryEntity.Id);
                    if (serviceAppointment == null) throw new InvalidPluginExecutionException("Service Appointment cannot be null.");
                    //Retrieve the Appointment Modality
                    OptionSetValue apptmodality = null;
                    if (serviceAppointment.Contains("tmp_appointmentmodality"))
                    {
                        //Logger.WriteDebugMessage("Getting Appointment Modality from SA");
                        Trace("Getting Appointment Modality from SA.", LogLevel.Debug);
                        apptmodality = serviceAppointment.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
                    }
                    //Logger.WriteDebugMessage(apptmodality.Value.ToString());
                    Trace(apptmodality.Value.ToString(), LogLevel.Debug);

                    //Logger.WriteDebugMessage("Checking Facility Type!");
                    Trace("Checking Facility Type!", LogLevel.Debug);
                    if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                    {
                        VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                        //Logger.WriteDebugMessage($"INFO: isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                        Trace($"INFO: isVistaFacility exists in shared variables; set to: {VistaTypeFacility}.", LogLevel.Debug);
                    }
                    //Check if Appointment Modality is not null and is not set to VVC Test Call
                    else if (apptmodality != null && apptmodality.Value != 178970008)
                    {
                        VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(serviceAppointment, context, pluginLogger);
                        PluginExecutionContext.SharedVariables.Add("isVistaFacility", VistaTypeFacility);
                    }
                    else 
                    {
                        VistaTypeFacility = true;
                    }

                    if (VistaTypeFacility)
                    {
                        //Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
                        Trace("The Current Facility IS a Vista Facility.  Continue Processing.", LogLevel.Debug);
                        // Check the settings record to make sure we want to Call the Accenture & Vyopta Services
                        var settings = context.mcs_settingSet.FirstOrDefault(x => x.mcs_name == "Active Settings");
                        if (settings == null) throw new InvalidPluginExecutionException("Active Settings cannot be null.");

                        if (settings.cvt_accenturevyopta != null && settings.cvt_accenturevyopta.Value &&
                            !CvtHelper.isGfeServiceActivity(serviceAppointment, OrganizationService, pluginLogger))
                        {
                            ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "VmrCreateUri");

                            //Check if Appointment Modality is set to VVC Test Call
                            if (apptmodality != null && apptmodality.Value != 178970008)
                            {
                                // Ensure this is a Home/Mobile Service Appointment.
                                if (serviceAppointment.cvt_Type == null || !serviceAppointment.cvt_Type.Value)
                                {
                                    //Logger.WriteDebugMessage("Not Home/Mobile, skipping VMR");
                                    Trace("Not Home/Mobile, skipping VMR.", LogLevel.Debug);
                                    //Wait for 5 sec incase
                                    //System.Threading.Thread.Sleep(5000);                            
                                    Success = true;
                                    LogExit(1);
                                    return;
                                }
                                // Ensure this is NOT a Telephone Call VVC Service Appointment.
                                if (serviceAppointment.cvt_TelephoneCall.HasValue && serviceAppointment.cvt_TelephoneCall.Value)
                                {
                                    //Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VMR");
                                    Trace("This is a Telephone Call Appointment, hence skipping VMR.", LogLevel.Debug);
                                    Success = true;
                                    LogExit(2);
                                    return;
                                }
                            }

                            //Logger.WriteDebugMessage($"**** Service Appointment Status Code is: {serviceAppointment.StatusCode.Value}");
                            Trace("**** Service Appointment Status Code is: {serviceAppointment.StatusCode.Value}", LogLevel.Debug);
                            //Logger.WriteDebugMessage($"**** Service Appointment Status Hash Code is: {serviceAppointment.StatusCode.GetHashCode()}");
                            Trace("**** Service Appointment Status Hash Code is: {serviceAppointment.StatusCode.GetHashCode()}", LogLevel.Debug);

                            if (apptmodality != null && apptmodality.Value != 178970008)
                            {
                                if (serviceAppointment.StateCode == null || serviceAppointment.StateCode != ServiceAppointmentState.Scheduled) throw new InvalidPluginExecutionException("Service Appointment is not in Scheduled State.");
                            }

                            if (serviceAppointment.StatusCode == null) throw new InvalidPluginExecutionException("Service Appointment Status Code cannot be null");

                            if ((serviceAppointment.StatusCode.Value == (int)serviceappointment_statuscode.Pending) || (serviceAppointment.StatusCode.Value == (int)serviceappointment_statuscode.InterfaceVIMTFailure) || apptmodality.Value == 178970008)
                            {
                                var ProviderVirtualMeetingSpace = "";
                                //Figure out if there is a static VMR or if we need to create a VMR
                                if (serviceAppointment.Customers.Count() == 1)
                                {
                                    //
                                    var patientAP = serviceAppointment.Customers.FirstOrDefault();
                                    if (patientAP == null || patientAP.PartyId == null)
                                    {
                                        LogExit(3);
                                        return;
                                    }

                                    var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, patientAP.PartyId.Id, new ColumnSet(true));
                                    //Logger.WriteDebugMessage("Contact: " + patient.FullName + " VMR: " + patient.cvt_PatientVirtualMeetingSpace + " and Tablet: " + patient.cvt_bltablet);
                                    Trace("Contact: " + patient.FullName + " VMR: " + patient.cvt_PatientVirtualMeetingSpace + " and Tablet: " + patient.cvt_bltablet, LogLevel.Debug);

                                    if (patient != null && patient.cvt_TabletType != null)
                                    {
                                        //Patient has a Technology Type “VAIssuediOSDevice” and “Do Not Allow Emails” is marked “Do Not Allow”
                                        if (patient.cvt_TabletType.Value == (int)Contactcvt_TabletType.VALoanedDevice &&
                                            patient.DoNotEMail.Value &&
                                            !string.IsNullOrEmpty(patient.cvt_staticvmrlink))
                                        {
                                            ProviderVirtualMeetingSpace = patient.cvt_staticvmrlink;

                                            var pin = string.Empty;
                                            var meetingRoomName = string.Empty;
                                            var dialAlias = string.Empty;
                                            var miscData = string.Empty;

                                            var vmrValues = ProviderVirtualMeetingSpace.Split('&');  //split the URL into its base & parameters

                                            for (var x = 0; x < vmrValues.Length; x++)
                                            {
                                                var paramVal = vmrValues[x].Split('=');
                                                switch (paramVal[0].ToString())
                                                {
                                                    case "conference":
                                                        dialAlias = paramVal[1].ToString();
                                                        var mrn = dialAlias.Split('@');
                                                        meetingRoomName = mrn[0];
                                                        break;
                                                    case "pin":
                                                        pin = paramVal[1].ToString();
                                                        break;
                                                }
                                            }

                                            //concatenate for miscData
                                            miscData = "hostDialUrl=" + ProviderVirtualMeetingSpace + ";guestDialUrl=" + ProviderVirtualMeetingSpace;

                                            var updateSa = new VA.TMP.DataModel.ServiceAppointment()
                                            {
                                                Id = serviceAppointment.Id,
                                                mcs_providerurl = ProviderVirtualMeetingSpace,
                                                mcs_PatientUrl = ProviderVirtualMeetingSpace,
                                                mcs_patientpin = pin,
                                                mcs_providerpin = pin,
                                                mcs_meetingroomname = meetingRoomName,
                                                mcs_dialingalias = dialAlias,
                                                mcs_miscdata = miscData
                                            };
                                            OrganizationService.Update(updateSa);
                                            //Logger.WriteDebugMessage($"Updated the SA with the patient's static VMR link. {ProviderVirtualMeetingSpace}");
                                            Trace($"Updated the SA with the patient's static VMR link. {ProviderVirtualMeetingSpace}", LogLevel.Debug);
                                            Success = true;
                                            LogExit(4);
                                            return;
                                        }
                                    }
                                }

                                //Continue - Call Service to create the Virtual Meeting Room
                                //Logger.WriteDebugMessage("Beginning CreateAndSendVirtualMeetingRoom");
                                Trace("Beginning CreateAndSendVirtualMeetingRoom.", LogLevel.Debug);
                                var virtualMeetingRoomCreateResponseMessage = CreateAndSendVirtualMeetingRoom(context, serviceAppointment, PluginExecutionContext.InitiatingUserId, PluginExecutionContext.OrganizationName);
                                //Logger.WriteDebugMessage("Finished CreateAndSendVirtualMeetingRoom");
                                Trace("Finished CreateAndSendVirtualMeetingRoom.", LogLevel.Debug);
                                if (virtualMeetingRoomCreateResponseMessage == null)
                                {
                                    LogExit(5);
                                    return;
                                }


                                //Logger.WriteDebugMessage("Beginning ProcessVirtualMeetingRoomCreateResponseMessage");
                                Trace("Beginning ProcessVirtualMeetingRoomCreateResponseMessage.", LogLevel.Debug);
                                ProcessVirtualMeetingRoomCreateResponseMessage(virtualMeetingRoomCreateResponseMessage);
                                //Logger.WriteDebugMessage("Finished ProcessVirtualMeetingRoomCreateResponseMessage");
                                Trace("Finished ProcessVirtualMeetingRoomCreateResponseMessage.", LogLevel.Debug);
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("Service Appointment Status must be Pending or Interface VIMT Failure!");
                            }
                        }
                        else
                        {
                            // Just Set the Status to Scheduled
                            if (settings.cvt_accenturevyopta == null || !settings.cvt_accenturevyopta.Value)
                            {
                                //Logger.WriteDebugMessage("Vyopta Integration Bypassed");
                                Trace("Vyopta Integration Bypassed.", LogLevel.Debug);
                            }
                            else if (CvtHelper.isGfeServiceActivity(serviceAppointment, OrganizationService, pluginLogger))
                            {
                                //Logger.WriteDebugMessage("Patient Has GFE, No VMR Generated");
                                Trace("Patient Has GFE, No VMR Generated.", LogLevel.Debug);
                            }
                            Success = true;
                        }
                    }
                    else
                    {
                        Success = true;
                        //Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as VMR is not a concept applicable to Cerner integration.");
                        Trace("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as VMR is not a concept applicable to Cerner integration.", LogLevel.Debug);
                    }


                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error);
                throw new InvalidPluginExecutionException(string.Format("ERROR in ServiceAppointmentCreatePostStageRunner: {0}", IntegrationPluginHelpers.BuildErrorMessage(ex)));
            }
            catch (InvalidPluginExecutionException ex)
            {
                //Logger.WriteToFile("Invalid Plugin Exception: " + CvtHelper.BuildExceptionMessage(ex));
                Trace($"Invalid Plugin Exception: {CvtHelper.BuildExceptionMessage(ex)}", LogLevel.Error);
                throw;
            }
            catch (Exception ex)
            {
                //Logger.WriteToFile("Generic Exception: " + CvtHelper.BuildExceptionMessage(ex));
                Trace($"Generic Exception: {CvtHelper.BuildExceptionMessage(ex)}", LogLevel.Error);
                throw;
            }

            LogExit(6);

        }

        /// <summary>
        /// Create an instance of the Virtual Meeting Room request and send to VIMT.
        /// </summary>
        /// <returns>VirtualMeetingRoomCreateResponseMessage.</returns>
        private VirtualMeetingRoomCreateResponseMessage CreateAndSendVirtualMeetingRoom(Xrm context, DataModel.ServiceAppointment serviceAppointment, Guid userId, string organizationName)
        {
            // Get the PatientId and ProviderId. Guard verbosely here against null references/objects.
            var bookedPatient = serviceAppointment.Customers.FirstOrDefault(p => p.PartyId.LogicalName == "contact");
            if (bookedPatient == null) throw new InvalidPluginExecutionException("Patient cannot be null.");

            if (bookedPatient.PartyId == null) throw new InvalidPluginExecutionException("The Patient PartyId cannot be null.");
            var patientId = bookedPatient.PartyId.Id;

            var bookedSysUser = serviceAppointment.Resources.FirstOrDefault(r => r.PartyId.LogicalName == "systemuser");
            if (bookedSysUser == null) throw new InvalidPluginExecutionException("ERROR: No provider found in the Appointment resource field. Possibly the provider associated to the SP is disabled in the System");

            if (bookedSysUser.PartyId == null) throw new InvalidPluginExecutionException("The Provider PartyId cannot be null.");
            var providerId = bookedSysUser.PartyId.Id;

            var request = new VirtualMeetingRoomCreateRequestMessage
            {
                LogRequest = true,
                UserId = userId,
                OrganizationName = organizationName,
                AppointmentId = serviceAppointment.Id,
                PatientId = patientId,
                ProviderId = providerId,
            };
            VirtualMeetingRoomCreateResponseMessage response = null;
            var vimtRequest = Serialization.DataContractSerialize(request);
            try
            {
                var baseUrl = ApiIntegrationSettings.BaseUrl;
                var uri = ApiIntegrationSettings.Uri;
                var resource = ApiIntegrationSettings.Resource;
                var appId = ApiIntegrationSettings.AppId;
                var secret = ApiIntegrationSettings.Secret;
                var authority = ApiIntegrationSettings.Authority;
                var tenantId = ApiIntegrationSettings.TenantId;
                var subscriptionId = ApiIntegrationSettings.SubscriptionId;
                var isProdApi = ApiIntegrationSettings.IsProdApi;
                var subscriptionIdEast = ApiIntegrationSettings.SubscriptionIdEast;
                var subscriptionIdSouth = ApiIntegrationSettings.SubscriptionIdSouth;

                response = GetVMRResponse(context, serviceAppointment);
                //this.Logger.WriteDebugMessage($"Meeting room {response.MeetingRoomName}");
                Trace($"Meeting room {response.MeetingRoomName}.", LogLevel.Debug);
                //this.Logger.WriteDebugMessage($"Patient URL {response.PatientUrl}");
                Trace($"Patient URL {response.PatientUrl}.", LogLevel.Debug);

                //this.Logger.WriteDebugMessage($"Provider URL {response.ProviderUrl}");
                Trace($"Provider URL {response.ProviderUrl}.", LogLevel.Debug);


                //  RestPoster.Post<VirtualMeetingRoomCreateRequestMessage, VirtualMeetingRoomCreateResponseMessage>("VMR", baseUrl, uri, request, resource, appId, secret, authority, tenantId,
                //subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                response.VimtLagMs = 0;

                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Create Virtual Meeting Room", errorMessage, vimtRequest,
                    typeof(VirtualMeetingRoomCreateRequestMessage).FullName, typeof(VirtualMeetingRoomCreateResponseMessage).FullName,
                    MessageRegistry.VirtualMeetingRoomCreateRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse,
                    response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                //Logger.WriteToFile(errorMessage);
                Trace(errorMessage, LogLevel.Error);

                return null;
            }
        }

        /// <summary>
        /// Update the Service Activity and create an Integration Result.
        /// </summary>
        /// <param name="virtualMeetingRoomCreateResponseMessage">Virtual Meeting Room Create Response Message.</param>
        private void ProcessVirtualMeetingRoomCreateResponseMessage(VirtualMeetingRoomCreateResponseMessage virtualMeetingRoomCreateResponseMessage)
        {
            var errorMessage = virtualMeetingRoomCreateResponseMessage.ExceptionOccured ? virtualMeetingRoomCreateResponseMessage.ExceptionMessage : null;
            IntegrationPluginHelpers.CreateIntegrationResult("Create Virtual Meeting Room", virtualMeetingRoomCreateResponseMessage.ExceptionOccured, errorMessage,
                virtualMeetingRoomCreateResponseMessage.VimtRequest, virtualMeetingRoomCreateResponseMessage.SerializedInstance, virtualMeetingRoomCreateResponseMessage.VimtResponse,
                typeof(VirtualMeetingRoomCreateRequestMessage).FullName, typeof(VirtualMeetingRoomCreateResponseMessage).FullName, MessageRegistry.VirtualMeetingRoomCreateRequestMessage,
                PrimaryEntity.Id, OrganizationService, 0, virtualMeetingRoomCreateResponseMessage.EcProcessingMs, virtualMeetingRoomCreateResponseMessage.VimtProcessingMs);

            if (virtualMeetingRoomCreateResponseMessage.ExceptionOccured)
            {
                IntegrationPluginHelpers.UpdateServiceAppointmentStatus(OrganizationService, PrimaryEntity.Id, serviceappointment_statuscode.InterfaceVIMTFailure);
                return;
            }
            var serviceAppointment = new DataModel.ServiceAppointment
            {
                Id = PrimaryEntity.Id,
                mcs_meetingroomname = virtualMeetingRoomCreateResponseMessage.MeetingRoomName,
                mcs_PatientUrl = virtualMeetingRoomCreateResponseMessage.PatientUrl,
                mcs_providerurl = virtualMeetingRoomCreateResponseMessage.ProviderUrl,
                mcs_patientpin = virtualMeetingRoomCreateResponseMessage.PatientPin,
                mcs_providerpin = virtualMeetingRoomCreateResponseMessage.ProviderPin,
                mcs_dialingalias = virtualMeetingRoomCreateResponseMessage.DialingAlias,
                mcs_miscdata = virtualMeetingRoomCreateResponseMessage.MiscData,
                cvt_VMRCompleted = true
            };
            if(string.IsNullOrEmpty(serviceAppointment.mcs_dialingalias))
            {
                // retrive details from URL TMPX2600599@care2.evn.va.gov
                // removing "conference=" from the substring by adding 11 characters
                string urlSubString = serviceAppointment.mcs_PatientUrl.Substring(serviceAppointment.mcs_PatientUrl.IndexOf("conference=")+11);
                serviceAppointment.mcs_dialingalias = urlSubString.Substring(0, urlSubString.IndexOf('&'));

            }
            OrganizationService.Update(serviceAppointment);
            Success = true;
        }

        private VirtualMeetingRoomCreateResponseMessage GetVMRResponse(Xrm context, DataModel.ServiceAppointment serviceAppointment)
        {


            // var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, patientAP.PartyId.Id, new ColumnSet(true));
            var integrationSettings = context.mcs_integrationsettingSet.Select(x => new IntegrationSetting { Name = x.mcs_name, Value = x.mcs_value }).ToList();

            var VirtualMeetingRoomDigitLength = Convert.ToInt32(integrationSettings.First(x => x.Name == "Virtual Meeting Room Digit Length").Value);
            var VirtualMeetingRoomPrefix = integrationSettings.First(x => x.Name == "Virtual Meeting Room Prefix").Value;
            var VirtualMeetingRoomSuffix = integrationSettings.First(x => x.Name == "Virtual Meeting Room Suffix").Value;

            var meetingroom = VA.TMP.Integration.Common.RandomDigits.GetRandomDigitString(VirtualMeetingRoomDigitLength);
            var meetingroomWithPrefix = VirtualMeetingRoomPrefix + meetingroom;

            #region host & guest key

            var sharedKey = integrationSettings.First(x => x.Name == "VMRSharedKey").Value;

            string host_key = string.Format("{0}{1}", sharedKey, meetingroom);
            string guest_key = string.Format("{0}{1}", meetingroom, sharedKey);

            // it should retun integer/string pin numbers. we can get first 7 or 10 based on link type
            var host_pin = VA.TMP.Integration.Common.RandomDigits.SHA256HexHashString(host_key).Substring(0, 7) + "#"; // Provider
            var guest_pin = VA.TMP.Integration.Common.RandomDigits.SHA256HexHashString(guest_key).Substring(0, 10) + "#"; // Patient

            #endregion

            // Provider
            var ProviderVmrFormatUrl = integrationSettings.First(x => x.Name == "Provider VMR Format URL").Value;
            var PatientVmrFormatUrl = integrationSettings.First(x => x.Name == "Patient VMR Format URL").Value;
            var VmrBaseUrl = integrationSettings.First(x => x.Name == "VMR Base URL").Value;
            var VmrBaseUrlExtension = integrationSettings.First(x => x.Name == "VMR Base URL Extension").Value;
            var name = string.Empty;
            if (serviceAppointment.mcs_relatedprovidersite != null)
            {
                name = serviceAppointment.mcs_relatedprovidersite.Id.ToString().Replace("{", "").Replace("}", "");
            }

            var patientUrl = string.Format(PatientVmrFormatUrl, VmrBaseUrl, VmrBaseUrlExtension, meetingroomWithPrefix, VirtualMeetingRoomSuffix, guest_pin);

            var providerUrl = string.Format(ProviderVmrFormatUrl, VmrBaseUrl, VmrBaseUrlExtension, name,
                    meetingroomWithPrefix, VirtualMeetingRoomSuffix, host_pin);

            //concatenate for miscData
            var miscData = "hostDialUrl=" + providerUrl + ";guestDialUrl=" + patientUrl;



            VirtualMeetingRoomCreateResponseMessage response = new VirtualMeetingRoomCreateResponseMessage()
            {
                ProviderUrl = providerUrl,
                PatientUrl = patientUrl,
                MeetingRoomName = meetingroom,
                PatientPin = guest_pin,
                ProviderPin = host_pin,
                AppointmentId = serviceAppointment.Id.ToString(),
                MiscData = miscData,
                VimtLagMs = 0
            };

            return response;


        }


        private void LogEntry([CallerMemberName] string memberName = "",
                       [CallerFilePath] string sourceFilePath = "",
                       [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = this.GetType().Name;
                //Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");
                Trace($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}.", LogLevel.Debug);
            }
            catch (Exception)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }

        private void LogExit(int exitPoint,
                               [CallerMemberName] string memberName = "",
                               [CallerFilePath] string sourceFilePath = "",
                               [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = this.GetType().Name;
                //Logger.WriteDebugMessage($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}");
                Trace($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}.", LogLevel.Debug);
            }
            catch (Exception)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }

    }
}
