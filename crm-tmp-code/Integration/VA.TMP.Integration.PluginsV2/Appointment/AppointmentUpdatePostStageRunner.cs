using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using MCS.ApplicationInsights;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Appointment
{
    /// <summary>
    ///  CRM Plugin Runner class to handle updating an Appointment.
    /// </summary>
    public class AppointmentUpdatePostStageRunner : AILogicBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public AppointmentUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "mcs_appointmentplugin";

        /// <summary>
        /// This field returns false if there were any proxy add failures.  If so, the subsequent integrations won't run
        /// </summary>
        public bool Success;

        /// <summary>
        /// This is the list of Veterans that were added or removed
        /// </summary>
        public Dictionary<Guid, bool> VeteransDelta;

        /// <summary>
        /// Gets or sets the Api Settings.
        /// </summary>
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
        /// Executes the plugin runner.
        /// </summary>
        public override void ExecuteLogic()
        {
            try
            {
                var appt = OrganizationService.Retrieve(DataModel.Appointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.Appointment>();

                var orgName = PluginExecutionContext.OrganizationName;
                var user = OrganizationService.Retrieve(SystemUser.EntityLogicalName, PluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();
                var hasVeterans = appt.OptionalAttendees != null && appt.OptionalAttendees.Count() > 0;
                var hasRelatedSa = appt.cvt_serviceactivityid != null;
                var apptIsBusy = appt.StatusCode.Value == (int)appointment_statuscode.Busy;
                bool VistaTypeFacility = true;

                if (apptIsBusy && hasVeterans && hasRelatedSa)
                {
                    using (var context = new Xrm(OrganizationService))
                    {
                        //Logger.WriteDebugMessage("Checking Facility Type!");
                        Trace("Checking Facility Type!", LogLevel.Debug);
                        if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                        {
                            VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                            //Logger.WriteDebugMessage($"isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                            Trace($"isVistaFacility exists in shared variables; set to: {VistaTypeFacility}", LogLevel.Debug);
                        }
                        else
                        {
                            VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(appt, context, Logger);
                        }
                        //////if (VistaTypeFacility) // Vista Facility. Proceed
                        //////{
                        //    ////Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
                        //Logger.WriteDebugMessage("Starting GetAndSetIntegrationSettings");
                        Trace("Starting GetAndSetIntegrationSettings.", LogLevel.Debug);
                        GetAndSetIntegrationSettings(context);
                        //Logger.WriteDebugMessage("Finished GetAndSetIntegrationSettings");
                        Trace("Finished GetAndSetIntegrationSettings.", LogLevel.Debug);

                        //Logger.WriteDebugMessage("Sending Proxy Add request to VIMT");
                        Trace("Sending Proxy Add request to VIMT.", LogLevel.Debug);
                        var responses = CreateAndSendProxyAddToVista(orgName, user, appt);
                        //Logger.WriteDebugMessage("Finished sending Proxy Add request to VIMT");
                        Trace("Finished sending Proxy Add request to VIMT.", LogLevel.Debug);

                        if (responses == null)
                        {
                            //Logger.WriteDebugMessage("The Proxy Add response is null");
                            Trace("The Proxy Add response is null.", LogLevel.Debug);
                            Success = false;
                            return;
                        }
                        foreach (var response in responses)
                        {
                            ProcessProxyAddToVistaResponse(response);
                            if (response == null || response.ExceptionOccured)
                            {
                                //Logger.WriteDebugMessage("Proxy Add failed, do not trigger VVS");
                                Trace("Proxy Add failed, do not trigger VVS.", LogLevel.Debug);
                                Success = false;
                                IntegrationPluginHelpers.UpdateAppointment(OrganizationService, PrimaryEntity.Id, Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure);
                                //Logger.WriteDebugMessage("Appointment status updated to VIMT Failure");
                                Trace("Appointment status updated to VIMT Failure.", LogLevel.Debug);
                                return;
                            }
                        }
                        //Logger.WriteDebugMessage("Finished Processing Proxy Add");
                        Trace("Finished Processing Proxy Add.", LogLevel.Debug);
                        ////////}
                        ////////else
                        ////////{
                        //// ////   Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted. Control NOT passed to logic app as Proxy Adds are not an applicable concept in Cerner.");
                        ////////}
                    }
                }
                else
                {
                    //Logger.WriteDebugMessage($"Proxy Add did not fire because either appt did not have veterans ({hasVeterans}), a parent Service Activity ({hasRelatedSa}), or was not in the correct status ({apptIsBusy})");
                    Trace($"Proxy Add did not fire because either appt did not have veterans ({hasVeterans}), a parent Service Activity ({hasRelatedSa}), or was not in the correct status ({apptIsBusy})", LogLevel.Debug);
                }
                Success = true;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error);
                throw new InvalidPluginExecutionException($"ERROR in AppointmentCreatePostStageRunner: {IntegrationPluginHelpers.BuildErrorMessage(ex)}");
            }
            catch (InvalidPluginExecutionException ex)
            {
                //Logger.WriteDebugMessage(ex.Message);
                Trace(ex.Message, LogLevel.Error);
                throw;
            }
            catch (Exception ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error);
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

        #region Proxy Add to Vista

        /// <summary>
        /// Create Proxy Add request and send to VIMT.
        /// </summary>
        /// <param name="orgName">Organization name.</param>
        /// <param name="user">CRM User.</param>
        /// <param name="appointment">Appointment.</param>
        /// <returns>ProxyAddResponseMessage.</returns>
        private List<ProxyAddResponseMessage> CreateAndSendProxyAddToVista(string orgName, SystemUser user, DataModel.Appointment appointment)
        {
            var responses = new List<ProxyAddResponseMessage>();
            VeteransDelta = IntegrationPluginHelpers.GetListOfNewPatients(OrganizationService, appointment, Logger, PluginExecutionContext);

            // Only get the veteran ids listed as "new" from the dictionary (this is indicated by the value of the Key Value Pair being true)
            var newVets = VeteransDelta.Where(kvp => kvp.Value).Select(kvp => kvp.Key)?.ToList() ?? new List<Guid>();

            foreach (var vet in newVets)
            {
                var request = new ProxyAddRequestMessage
                {
                    LogRequest = true,
                    AppointmentId = PrimaryEntity.Id,
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
                    VeteranPartyId = vet
                };

                var vimtRequest = Serialization.DataContractSerialize(request);

                //Logger.WriteDebugMessage($"Sending Group Proxy Add Message to VIMT: {vimtRequest}.");
                Trace("Sending Group Proxy Add Message to VIMT: {vimtRequest}.", LogLevel.Debug);
                ProxyAddResponseMessage response = null;
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

                    response = RestPoster.Post<ProxyAddRequestMessage, ProxyAddResponseMessage>("MVI Group Proxy Add", baseUrl, uri, request, resource, appId, secret, authority, tenantId, subscriptionId,
                        isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                    responses.Add(response);
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                    IntegrationPluginHelpers.CreateAppointmentIntegrationResultOnVimtFailure("Group Proxy Add to Vista", errorMessage, vimtRequest, typeof(ProxyAddRequestMessage).FullName,
                        typeof(ProxyAddResponseMessage).FullName, MessageRegistry.ProxyAddRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest,
                        response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                    //Logger.WriteToFile(errorMessage);
                    Trace(errorMessage, LogLevel.Error);
                    responses.Add(null);
                }
            }
            return responses;
        }

        /// <summary>
        /// Process Proxy Add response.
        /// </summary>
        /// <param name="response">ProxyAddResponseMessage.</param>
        private void ProcessProxyAddToVistaResponse(ProxyAddResponseMessage response)
        {
            //Logger.WriteDebugMessage("processing response: " + response?.MessageId);
            Trace($"processing response: {response?.MessageId}.", LogLevel.Debug);
            if (response == null) return;

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            IntegrationPluginHelpers.CreateAppointmentIntegrationResult("Group Proxy Add To Vista", response.ExceptionOccured, errorMessage, response.VimtRequest,
                response.SerializedInstance, response.VimtResponse, typeof(ProxyAddRequestMessage).FullName, typeof(ProxyAddResponseMessage).FullName,
                MessageRegistry.ProxyAddRequestMessage, PrimaryEntity.Id, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs, false);
        }

        #endregion
    }
}
