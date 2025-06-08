using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using MCS.ApplicationInsights;
using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    /// <summary>
    ///  CRM Plugin Runner class to handle creating a ServiceAppointment.
    /// </summary>
    public class ServiceAppointmentCreatePostStageRunner : AILogicBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public ServiceAppointmentCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the ProxyAddFakeResponseType.
        /// </summary>
        private string ProxyAddFakeResponseType { get; set; }

        /// <summary>
        /// Gets or sets ProcessingCode.
        /// </summary>
        private string ProcessingCode { get; set; }

        /// <summary>
        /// Gets or sets ReturnMviMessagesInResponse.
        /// </summary>
        private bool ReturnMviMessagesInResponse { get; set; }

        /// <summary>
        /// Gets or sets PatientVeteran.
        /// </summary>
        private bool PatientVeteran { get; set; }

        /// <summary>
        /// Gets or sets PatientServiceConnected.
        /// </summary>
        private bool PatientServiceConnected { get; set; }

        /// <summary>
        /// Gets or sets PatientType.
        /// </summary>
        private int PatientType { get; set; }

        /// <summary>
        /// The Guids of Veterans that were added (bool = true) or removed (bool = false)
        /// </summary>
        public Dictionary<Guid, bool> VeteransChanged { get; set; }

        /// <summary>
        /// This indicates whether there was a proxy add failure - if not, then the next integration will be called
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Executes the plugin runner.
        /// </summary>
        public override void ExecuteLogic()
        {
            Success = false;
            bool VistaTypeFacility = true;

            try
            {
                var saRecord = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();

                using (var context = new Xrm(OrganizationService))
                {
                    Trace("Checking Facility Type!", LogLevel.Debug, true);
                    //Logger.WriteDebugMessage("Checking Facility Type!");
                    if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                    {
                        VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                        //Logger.WriteDebugMessage($"INFO: isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                        Trace($"INFO: isVistaFacility exists in shared variables; set to: {VistaTypeFacility}", LogLevel.Debug, true);
                    }
                    else
                    {
                        VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(saRecord, context, pluginLogger);
                        PluginExecutionContext.SharedVariables.Add("isVistaFacility", VistaTypeFacility);
                    }
                }

                // Not Group or is H/M
                if (saRecord.mcs_groupappointment == null || !saRecord.mcs_groupappointment.Value || saRecord.cvt_Type.Value)
                {
                    if (saRecord.StateCode.Value == ServiceAppointmentState.Scheduled)
                    {
                        var orgName = PluginExecutionContext.OrganizationName;
                        var user = OrganizationService.Retrieve(SystemUser.EntityLogicalName, PluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();

                        using (var context = new Xrm(OrganizationService))
                        {

                            //Logger.WriteDebugMessage("Starting GetAndSetIntegrationSettings");
                            Trace("Starting GetAndSetIntegrationSettings", LogLevel.Debug, true);
                            GetAndSetIntegrationSettings(context);
                            //Logger.WriteDebugMessage("Finished GetAndSetIntegrationSettings");
                            Trace("Finished GetAndSetIntegrationSettings", LogLevel.Debug, true);

                            var anyFailures = RunProxyAdd(orgName, user, saRecord, VistaTypeFacility);

                            if (!anyFailures) Success = true;
                        }
                    }
                    else
                    {
                        //Logger.WriteDebugMessage(string.Format("ServiceAppointment is not scheduled (status is {1}), Proxy Add Canceled: {0}", PrimaryEntity.Id, saRecord.StatusCode.Value));
                        Trace($"ServiceAppointment is not scheduled (status is {saRecord.StatusCode.Value}), Proxy Add Canceled: {PrimaryEntity.Id}", LogLevel.Debug, true);
                        Success = true;
                    }

                    // Moved here to resolve deadlocking issue between this function and the "Trigger Next Integration" function - when a group appointment is selected, none of the integrations run
                    CvtHelper.SetServiceAppointmentPermissions(OrganizationService, PrimaryEntity, pluginLogger);
                    //Logger.WriteDebugMessage("Set the SA Permissions (and assign).");
                    Trace("Set the SA Permissions (and assign).", LogLevel.Debug, true);
                }
                else
                {
                    //Logger.WriteDebugMessage("Proxy Add is executed on the Appointment, not the Service Activity for Group Appointments that are Clinic Based.");
                    Trace("Proxy Add is executed on the Appointment, not the Service Activity for Group Appointments that are Clinic Based.", LogLevel.Debug, true);
                    Success = true;
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error, true);
                throw new InvalidPluginExecutionException(string.Format("ERROR in ServiceAppointmentCreatePostStageRunner: {0}", IntegrationPluginHelpers.BuildErrorMessage(ex)));
            }
            catch (InvalidPluginExecutionException ex)
            {
                //Logger.WriteDebugMessage(ex.Message);
                Trace(ex.Message, LogLevel.Error, true);
                throw;
            }
            catch (Exception ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error, true);
                throw;
            }
        }

        /// <summary>
        /// Gets and sets the Integration Settings needed for Proxy Add.
        /// </summary>
        /// <param name="context">CRM context.</param>
        private void GetAndSetIntegrationSettings(Xrm context)
        {
            ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "MviProxyAddUri");

            var proxyAddFakeResponseType = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "MVI ProxyAdd Fake Response Type");
            if (proxyAddFakeResponseType != null) ProxyAddFakeResponseType = proxyAddFakeResponseType.mcs_value;
            else throw new InvalidPluginExecutionException("No ProxyAddFakeResponseType listed in Integration Settings.  Please contact the Help Desk to add ProxyAddFakeResponseType.  Proxy Add Canceled.");

            var processingCode = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "ProcessingCode");
            if (processingCode != null) ProcessingCode = processingCode.mcs_value;
            else throw new InvalidPluginExecutionException("No ProcessingCode listed in Integration Settings.  Please contact the Help Desk to add ProcessingCode.  Proxy Add Canceled.");

            var returnMviMessagesInResponse = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "ReturnMviMessagesInResponse");
            if (returnMviMessagesInResponse != null) ReturnMviMessagesInResponse = Convert.ToBoolean(returnMviMessagesInResponse.mcs_value);
            else throw new InvalidPluginExecutionException("No ReturnMviMessagesInResponse listed in Integration Settings.  Please contact the Help Desk to add ReturnMviMessagesInResponse.  Proxy Add Canceled.");

            var patientVeteran = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "PatientVeteran");
            if (patientVeteran != null) PatientVeteran = Convert.ToBoolean(patientVeteran.mcs_value);
            else throw new InvalidPluginExecutionException("No PatientVeteran listed in Integration Settings.  Please contact the Help Desk to add PatientVeteran.  Proxy Add Canceled.");

            var patientServiceConnected = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "PatientServiceConnected");
            if (patientServiceConnected != null) PatientServiceConnected = Convert.ToBoolean(patientServiceConnected.mcs_value);
            else throw new InvalidPluginExecutionException("No PatientServiceConnected listed in Integration Settings.  Please contact the Help Desk to add PatientServiceConnected.  Proxy Add Canceled.");

            var patientType = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "PatientType");
            if (patientType != null) PatientType = Convert.ToInt32(patientType.mcs_value);
            else throw new InvalidPluginExecutionException("No PatientType listed in Integration Settings.  Please contact the Help Desk to add PatientType.  Proxy Add Canceled.");
        }

        /// <summary>
        /// Sends the Proxy Add Request to VIMT.
        /// </summary>
        /// <param name="orgName">Organization Service.</param>
        /// <param name="user">CRM User.</param>
        /// <param name="sa">Service Appointment</param>
        /// <returns>ProxyAddResponseMessage.</returns>
        private bool RunProxyAdd(string orgName, SystemUser user, DataModel.ServiceAppointment sa, bool VistaTypeFacility)
        {
            var anyfailures = false;
            var failureCount = 0;

            // Use the PrimaryEntity so that you can only send Customers that were modified (added or removed)
            VeteransChanged = IntegrationPluginHelpers.GetListOfNewPatients(OrganizationService, sa, pluginLogger, PluginExecutionContext);

            var veteransAdded = VeteransChanged.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

            foreach (var veteranParty in veteransAdded)
            {
                if (failureCount > 0) return true;

                //Logger.WriteDebugMessage("VeteranParty: " + veteranParty);
                Trace($"VeteranParty: {veteranParty}", LogLevel.Debug, true);

                var request = new ProxyAddRequestMessage
                {
                    LogRequest = true,
                    ServiceAppointmentId = PrimaryEntity.Id,
                    OrganizationName = orgName,
                    FakeResponseType = ProxyAddFakeResponseType,
                    UserFirstName = user.FirstName,
                    UserLastName = user.LastName,
                    UserId = user.Id,
                    ProcessingCode = ProcessingCode,
                    ReturnMviMessagesInResponse = ReturnMviMessagesInResponse,
                    PatientVeteran = PatientVeteran,
                    PatientServiceConnected = PatientServiceConnected,
                    PatientType = PatientType,
                    VeteranPartyId = veteranParty
                };

                ProxyAddResponseMessage response = null;
                var vimtRequest = Serialization.DataContractSerialize(request);

                try
                {
                    if (VistaTypeFacility)
                    {
                        //Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
                        Trace("The Current Facility IS a Vista Facility.  Continue Processing.", LogLevel.Debug, true);

                        //Logger.WriteDebugMessage(string.Format("Sending Proxy Add Message to VIMT: {0}.", vimtRequest));
                        Trace($"Sending Proxy Add Message to VIMT: {vimtRequest}.", LogLevel.Debug, true);

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

                        response = RestPoster.Post<ProxyAddRequestMessage, ProxyAddResponseMessage>("MVI Proxy Add", baseUrl, uri, request, resource, appId, secret, authority, tenantId, subscriptionId,
                        isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                        response.VimtLagMs = lag;

                        //Logger.WriteDebugMessage(string.Format("Finished Sending Proxy Add Message to VIMT for {0}", veteranParty));
                        Trace($"Finished Sending Proxy Add Message to VIMT for {veteranParty}.", LogLevel.Debug, true);
                        ProcessProxyAddToVistaResponse(response);
                        if (response.ExceptionOccured) failureCount++;
                    }
                    else
                    {
                        //Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility.  Continue Processing.");
                        Trace("The Current Facility IS NOT a Vista Facility.  Continue Processing.", LogLevel.Debug, true);
                    }
                }

                catch (Exception ex)
                {
                    var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                    IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Proxy Add to Vista", errorMessage, vimtRequest, typeof(ProxyAddRequestMessage).FullName,
                        typeof(ProxyAddResponseMessage).FullName, MessageRegistry.ProxyAddRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                    //Logger.WriteToFile(errorMessage);
                    Trace(errorMessage, LogLevel.Error, true);
                    anyfailures = true;
                }
            }

            return failureCount > 0 ? true : anyfailures;
        }

        /// <summary>
        /// Process Proxy Add to VistA response.
        /// </summary>
        /// <param name="response">Proxy Add Response.</param>
        private void ProcessProxyAddToVistaResponse(ProxyAddResponseMessage response)
        {
            if (response == null) throw new Exception("No Proxy Add Response was returned from VIMT");

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            IntegrationPluginHelpers.CreateIntegrationResult("Proxy Add to VistA", response.ExceptionOccured, errorMessage,
                response.VimtRequest, response.SerializedInstance, response.VimtResponse,
                typeof(ProxyAddRequestMessage).FullName, typeof(ProxyAddResponseMessage).FullName, MessageRegistry.ProxyAddRequestMessage,
                PrimaryEntity.Id, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs);

            if (response.ExceptionOccured) IntegrationPluginHelpers.UpdateServiceAppointmentStatus(OrganizationService, PrimaryEntity.Id, serviceappointment_statuscode.InterfaceVIMTFailure);
        }
    }
}
