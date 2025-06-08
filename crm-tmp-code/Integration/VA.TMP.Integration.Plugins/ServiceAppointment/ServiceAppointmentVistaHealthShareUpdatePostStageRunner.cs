using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.Cerner;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentVistaHealthShareUpdatePostStageRunner : PluginRunner
    {
        public ServiceAppointmentVistaHealthShareUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        public bool Success { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        public override void Execute()
        {
            LogEntry();

            var saRecord = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();
            bool VistaTypeFacility = true;

            if (saRecord.cvt_Type != null && (saRecord.mcs_groupappointment == null || !saRecord.mcs_groupappointment.Value || saRecord.cvt_Type.Value)) //VA Video Connect
            {
                using (var context = new Xrm(OrganizationService))
                {
                    Logger.WriteDebugMessage("Checking Facility Type!");
                    VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(saRecord, context, Logger);
                
                    var runVista = VistaPluginHelpers.RunVistaIntegration(saRecord, context, Logger);
                    if (runVista)
                    {
                        if (saRecord.StateCode != null && (saRecord.StateCode.Value == ServiceAppointmentState.Scheduled || saRecord.StateCode.Value == ServiceAppointmentState.Open))
                        {
                            var orgName = PluginExecutionContext.OrganizationName;
                            var user = OrganizationService.Retrieve(SystemUser.EntityLogicalName, PluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();

                            Logger.WriteDebugMessage("Starting GetAndSetIntegrationSettings");
                            GetAndSetIntegrationSettings(context);
                            Logger.WriteDebugMessage("Finished GetAndSetIntegrationSettings");

                            var anyFailures = RunVistaBooking(saRecord, orgName, user, VistaTypeFacility);

                            if (!anyFailures) TriggerNextIntegration();
                        }
                        else
                        {
                            Success = true;
                            Logger.WriteDebugMessage("Service Activity is not in a proper status to run Vista Integration");
                        }
                    }
                    else
                    {
                        TriggerNextIntegration();
                        Logger.WriteDebugMessage("Vista Integration Switched Off, moving on to next integration.");
                    }
                }
            }
            else Logger.WriteDebugMessage("Clinic Based Groups do not run Vista Integration");

            LogExit(1);
        }

        private void GetAndSetIntegrationSettings(Xrm context)
        {
            LogEntry();

            ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "HsMakeCancelOutboundUri");

            LogExit(1);

        }

        private bool RunVistaBooking(DataModel.ServiceAppointment sa, string orgName, SystemUser user, bool IsVistaTypeFacility)
        {
            LogEntry();

            var anyfailures = false;
            var veterans = VistaPluginHelpers.GetChangedPatients(sa, OrganizationService, Logger, out var isBooking);

            // Checks to ensure new patients were added, and only sends those new patients
            if (!isBooking)
            {
                Logger.WriteDebugMessage("No new patients detected, exiting Vista Booking");

                LogExit(1);

                return false;
            }
            var request = new TmpHealthShareMakeCancelOutboundRequestMessage
            {
                Patients = veterans,
                ServiceAppointmentId = PrimaryEntity.Id,
                LogRequest = true,
                OrganizationName = orgName,
                UserId = user.Id,
                VisitStatus = VistaStatus.SCHEDULED.ToString()
            };

            Logger.WriteDebugMessage("Set up HealthShareMakeCancelOutboundRequestMessage request object.");

            TmpHealthShareMakeCancelOutboundResponseMessage response = null;
            TmpCernerOutboundResponseMessage cernerResponse = null;

            var vimtRequest = Serialization.DataContractSerialize(request);
            try
            {

                if (IsVistaTypeFacility)
                {
                    Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
                    Logger.WriteDebugMessage($"Sending HealthShare Make Appointment Request Message to VIMT: {vimtRequest}.");

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

                    response = RestPoster.Post<TmpHealthShareMakeCancelOutboundRequestMessage, TmpHealthShareMakeCancelOutboundResponseMessage>(
                        "HealthShare MakeCancel Outbound", baseUrl, uri, request, resource, appId, secret, authority, tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                    response.VimtLagMs = lag;

                    Logger.WriteDebugMessage($"Finished Sending HealthShare Make Appointment Request Message to VIMT for Service Appointment with Id: {PrimaryEntity.Id}");
                    ProcessVistaResponse(response);
                    anyfailures = response.ExceptionOccured || (response.PatientIntegrationResultInformation != null && response.PatientIntegrationResultInformation.Any(x => x.ExceptionOccured));
                }
                else
                {
                    Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control Passed to Logic App for Cerner Processing.");

                    using (var context = new Xrm(OrganizationService))
                    {
                        cernerResponse = CernerHelper.FireLogicApp(PrimaryEntity.ToEntityReference(), context, OrganizationService, Logger, veterans, "Book|ServiceAppointment");
                        ProcessCernerResponse(cernerResponse);
                        if (cernerResponse.ExceptionOccured) anyfailures = true;
                    }
                }

            }
            catch (Exception ex)
            {
                if (IsVistaTypeFacility)
                {
                    var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                    IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Make Vista Appointment", errorMessage, vimtRequest,
                        typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                        MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse,
                        response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                    Logger.WriteToFile(errorMessage);
                    anyfailures = true;
                }
                else
                {
                    var errorMessage = string.Format("Cerner communication issue", ex);
                    IntegrationPluginHelpers.CreateIntegrationResultOnCernerFailure("Make Cerner Appointment", errorMessage, vimtRequest,
                        typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                        MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse,
                        response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                    Logger.WriteToFile(errorMessage);
                    anyfailures = true;
                }
              
            }

            LogExit(1);

            return anyfailures;
        }

        private void ProcessVistaResponse(TmpHealthShareMakeCancelOutboundResponseMessage response)
        {
            LogEntry();

            foreach (var patientIntegrationResultInformation in response.PatientIntegrationResultInformation)
            {
                var errorMessage = patientIntegrationResultInformation.ExceptionOccured ? patientIntegrationResultInformation.ExceptionMessage : string.Empty;
                IntegrationPluginHelpers.CreateIntegrationResult("Make Vista Appointment", patientIntegrationResultInformation.ExceptionOccured, errorMessage,
                    patientIntegrationResultInformation.VimtRequest, response.SerializedInstance, patientIntegrationResultInformation.VimtResponse,
                    typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName, MessageRegistry.HealthShareMakeCancelOutboundRequestMessage,
                    PrimaryEntity.Id, OrganizationService, response.VimtLagMs, patientIntegrationResultInformation.EcProcessingMs, response.VimtProcessingMs, patientIntegrationResultInformation);
            }

            LogExit(1);
        }
        private void ProcessCernerResponse(TmpCernerOutboundResponseMessage response)
        {
            LogEntry();

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            IntegrationPluginHelpers.CreateCernerIntegrationResult("Make Cerner Appointment", response.ExceptionOccured, errorMessage,
                response.RequestMessage, response.RequestMessage, response.ResponseMessage,
                typeof(TmpCernerOutboundResponseMessage).FullName, typeof(TmpCernerOutboundResponseMessage).FullName, MessageRegistry.CernerOutboundResponseMessage,
                PrimaryEntity.Id, OrganizationService, 0, 0, response.MessageProcessingTime, controlId: response.ControlId);

            LogExit(1);
        }

        private void TriggerNextIntegration()
        {
            Success = true;
        }

        private void LogEntry([CallerMemberName] string memberName = "",
                             [CallerFilePath] string sourceFilePath = "",
                             [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = this.GetType().Name;
                Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");
            }
            catch (Exception e)
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
                Logger.WriteDebugMessage($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}");
            }
            catch (Exception e)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }
        }
    }
}
