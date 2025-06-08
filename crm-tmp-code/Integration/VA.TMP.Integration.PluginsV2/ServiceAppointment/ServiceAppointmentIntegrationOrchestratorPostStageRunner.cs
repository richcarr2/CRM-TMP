using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MCS.ApplicationInsights;
using MCSHelperClass;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentIntegrationOrchestratorPostStageRunner : AILogicBase
    {
        private ApiIntegrationSettings ApiIntegrationSettingsCreate { get; set; }

        private ApiIntegrationSettings ApiIntegrationSettingsUpdate { get; set; }

        public bool Success { get; set; }

        public ServiceAppointmentIntegrationOrchestratorPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        private Dictionary<Guid, bool> Veterans { get; set; }

        public override void ExecuteLogic()
        {
            LogEntry();

            var isStatusChange = PluginExecutionContext.InputParameters.Contains("EntityMoniker");
            //Logger.WriteDebugMessage($"isStatusChage: {isStatusChange}");
            Trace($"isStatusChage: {isStatusChange}", LogLevel.Debug, true);

            if (isStatusChange) ExecuteCancel();
            else
            {
                using (var service = new Xrm(OrganizationService))
                {

                    //var saImage = PrimaryEntity.ToEntity<DataModel.ServiceAppointment>();
                    var saImage = service.ServiceAppointmentSet.FirstOrDefault(sa => sa.Id == PrimaryEntity.Id);

                    //Logger.WriteDebugMessage("Checking Facility Type!");
                    Trace("Checking Facility Type!", LogLevel.Debug, true);

                    //Logger.WriteDebugMessage("The Current Facility IS a Vistar Facility.  Continue Processing.");
                    Trace("The Current Facility IS a Vistar Facility.  Continue Processing.", LogLevel.Debug, true);
                    var patientsChanged = saImage.Customers != null;
                    var cancelReason = saImage.cvt_cancelreason;
                    //Logger.WriteDebugMessage("cancel Reason " + cancelReason);
                    Trace($"Cancel Reason {cancelReason}", LogLevel.Debug, true);

                    var preImage = (GetSecondaryEntity()?.ToEntity<DataModel.ServiceAppointment>() ?? null);

                    if (patientsChanged && string.IsNullOrEmpty(cancelReason)) ExecuteBook(saImage, preImage);

                    var isRetry = saImage.cvt_RetryIntegration;

                    if (isRetry.HasValue && isRetry.Value) ExecuteRetry();
                }

            }

            LogExit(1);
        }

        private void ExecuteBook(DataModel.ServiceAppointment sa, DataModel.ServiceAppointment preImage)
        {

            LogEntry();

            OptionSetValue statusCode = null;
            OptionSetValue apptmodality = null;

            if (sa.StatusCode != null)
            {
                statusCode = sa.StatusCode;
                //Logger.WriteDebugMessage("StatusCode is null");
                Trace("StatusCode is null.", LogLevel.Debug, true);
            }
            else
            {
                //Logger.WriteDebugMessage("StatusCode on SA is null, getting status code from pre-image");
                Trace("StatusCode on SA is null, getting status code from pre-image.", LogLevel.Debug, true);

                if (preImage != null)
                {
                    statusCode = preImage.StatusCode;
                }
                else
                {
                    throw new InvalidPluginExecutionException("ExecuteBook(DataModel.ServiceAppointment sa, DataModel.ServiceAppointment preImage) : StatusCode is null");
                }

            }

            //Logger.WriteDebugMessage("Enter ExecuteBook(DataModel.ServiceAppointment sa)");
            Trace("Enter ExecuteBook(DataModel.ServiceAppointment sa).", LogLevel.Debug, true);
            //Skip the Integration when the  user updates teh SA status to Open >> Requested: Open. 
            //The status reason is set to Requested: Open to free up the resources associated to appointment when there is an integration error
            if (PluginExecutionContext.MessageName.ToLower() == "update" && statusCode.Value == (int)serviceappointment_statuscode.RequestedOpen)
            {
                //Logger.WriteDebugMessage("ServiceAppointmentIntegrationOrchestratorPostStageRunner - Skipping ExecuteBook() as the record is being updated to status reason Request Open.");
                Trace("ServiceAppointmentIntegrationOrchestratorPostStageRunner - Skipping ExecuteBook() as the record is being updated to status reason Request Open.", LogLevel.Debug, true);
                LogExit(1);
                return;
            }

            //Logger.WriteDebugMessage("About to run to run the integration pipeline for a service appointment book");
            Trace("About to run to run the integration pipeline for a service appointment book.", LogLevel.Debug, true);

            //Retrieve Appointment Modality
            if (sa.Contains("tmp_appointmentmodality"))
            {
                //Logger.WriteDebugMessage("Getting Appointment Modality from SA");
                Trace("Getting Appointment Modality from SA", LogLevel.Debug, true);
                apptmodality = sa.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
            }
            else
            {
                //Logger.WriteDebugMessage("Appointment Modality on SA is null, getting modality from pre-image");
                Trace("Appointment Modality on SA is null, getting modality from pre-image.", LogLevel.Debug, true);
                if (preImage != null && preImage.Contains("tmp_appointmentmodality"))
                {
                    apptmodality = preImage.GetAttributeValue<OptionSetValue>("tmp_appointmentmodality");
                }
                else
                {
                    throw new InvalidPluginExecutionException("ExecuteBook(DataModel.ServiceAppointment sa, DataModel.ServiceAppointment preImage) : apptmodality is null");
                }
            }
            //Logger.WriteDebugMessage(apptmodality.Value.ToString());
            Trace(apptmodality.Value.ToString(), LogLevel.Debug, true);

            //Check if Appointment Modality is set to VVC Test Call
            if (apptmodality.Value == 178970008)
            {
                if (RunVmr() && RunVvs(sa))
                {
                    //Logger.WriteDebugMessage("Successfully completed booking integration pipeline.");
                    Trace("Successfully completed booking integration pipeline.", LogLevel.Debug, true);
                }
            }
            else
            {
                if (RunProxyAdd() && RunVmr() && RunVista(true))
                {
                    //Logger.WriteDebugMessage("Successfully completed booking integration pipeline.");
                    Trace("Successfully completed booking integration pipeline.", LogLevel.Debug, true);
                }
            }

            //Logger.WriteDebugMessage("Finished the integration pipeline for a service appointment book");
            Trace("Finished the integration pipeline for a service appointment book.", LogLevel.Debug, true);

            LogExit(2);
        }

        private void ExecuteCancel()
        {
            LogEntry();
            //Logger.WriteDebugMessage($"inside ExecuteCancel");
            Trace("Inside ExecuteCancel.", LogLevel.Debug, true);

            //Logic for RunCancelVVS was moved to the IntegrationResultUpdatePostStage Plugin
            if (RunCancelVmr() && RunVista(false))
            {
                //Logger.WriteDebugMessage("Successfully completed cancel integration pipeline.");
                Trace("Successfully completed cancel integration pipeline.", LogLevel.Debug, true);
            }

            LogExit(1);

        }

        /// <summary>
        /// Logic to execute a Retry
        /// </summary>
        private void ExecuteRetry()
        {
            LogEntry();

            if (!(bool)PrimaryEntity.Attributes["cvt_retryintegration"]) return;

            //Logger.WriteDebugMessage("Retry Initiated");
            Trace("Retry Initiated", LogLevel.Debug, true);

            using (var srv = new Xrm(OrganizationService))
            {
                var failedIRs = srv.mcs_integrationresultSet.Where(ir => ir.mcs_serviceappointmentid.Id == PrimaryEntity.Id && ir.mcs_status.Value == (int)mcs_integrationresultmcs_status.Error).ToList();
                var retryIr = IntegrationPluginHelpers.FindEarliestFailure(failedIRs, pluginLogger);

                if (retryIr == null)
                {
                    //Logger.WriteDebugMessage("Skipping Retry as Integration Result with error status could not be found.");
                    Trace("Skipping Retry as Integration Result with error status could not be found.", LogLevel.Debug, true);
                }
                else RetryIntegrationResult(retryIr);
            }

            // Reset the Retry integration field post run retry execution
            var apt = new DataModel.ServiceAppointment { Id = PrimaryEntity.Id, cvt_RetryIntegration = false };
            OrganizationService.Update(apt);

            LogExit(1);
        }

        private void RetryIntegrationResult(mcs_integrationresult integrationResult)
        {
            LogEntry();

            var typeOfIntegration = integrationResult.mcs_VimtMessageRegistryName;
            //Logger.WriteDebugMessage($"Executing RetryIntegrationResult. Type of Integration: {typeOfIntegration}");
            Trace($"Executing RetryIntegrationResult. Type of Integration: {typeOfIntegration}.", LogLevel.Debug, true);

            var successfulRetry = false;
            var finishedIntegrationPipeline = false;
            var isMake = !PluginExecutionContext.InputParameters.Contains("EntityMoniker");

            switch (typeOfIntegration)
            {
                case MessageRegistry.ProxyAddRequestMessage:
                    successfulRetry = RunProxyAdd();
                    if (successfulRetry && RunVmr() && RunVista(true)) finishedIntegrationPipeline = true;
                    break;
                case MessageRegistry.HealthShareMakeCancelOutboundRequestMessage:
                    successfulRetry = RunVmr();
                    //if (successfulRetry && RunVista(isMake) && RunVvs()) finishedIntegrationPipeline = true;
                    if (successfulRetry && RunVista(isMake)) finishedIntegrationPipeline = true;
                    break;
                case MessageRegistry.VirtualMeetingRoomCreateRequestMessage:
                    successfulRetry = RunVmr();
                    //if (successfulRetry && RunVista(true)) finishedIntegrationPipeline = RunVvs();
                    break;
                case MessageRegistry.VideoVisitCreateRequestMessage:
                    //successfulRetry = RunVvs();
                    if (successfulRetry) finishedIntegrationPipeline = true;
                    break;
                case MessageRegistry.VirtualMeetingRoomDeleteRequestMessage:
                    successfulRetry = RunCancelVmr();
                    //if (successfulRetry && RunVista(false)) finishedIntegrationPipeline = RunCancelVvs();
                    break;
                case MessageRegistry.VideoVisitDeleteRequestMessage:
                    //successfulRetry = RunCancelVvs();
                    finishedIntegrationPipeline = successfulRetry;
                    break;
                default:
                    //Logger.WriteDebugMessage($"Unknown Message Type - {typeOfIntegration}, skipping retry");
                    Trace($"Unknown Message Type - {typeOfIntegration}, skipping retry.", LogLevel.Debug, true);
                    break;
            }

            if (successfulRetry) ChangeIrStatusToRetrySuccess(integrationResult);

            var integrationSucceeded = finishedIntegrationPipeline ? "Succeeded" : "Did Not Succeed";
            //Logger.WriteToFile(string.Format("Remaining Integrations {0}.", finishedIntegrationPipeline ? "Succeeded" : "Did Not Succeed"));
            Trace($"Remaining Integrations {integrationSucceeded}.", LogLevel.Debug, true);

            LogExit(1);

        }

        private void ChangeIrStatusToRetrySuccess(mcs_integrationresult ir)
        {
            LogEntry();

            //Logger.WriteDebugMessage($"Updating IR {ir.mcs_VimtMessageRegistryName} to retry success");
            Trace($"Updating IR {ir.mcs_VimtMessageRegistryName} to retry success.", LogLevel.Debug, true);
            var updateIr = new mcs_integrationresult
            {
                Id = ir.Id,
                mcs_status = new OptionSetValue((int)mcs_integrationresultmcs_status.ErrorRetrySuccess)
            };
            OrganizationService.Update(updateIr);

            LogExit(1);
        }

        private bool RunProxyAdd()
        {
            LogEntry();

            using (var context = new Xrm(OrganizationService))
            {

                var proxyAddRunner = new ServiceAppointmentCreatePostStageRunner(ServiceProvider);
                proxyAddRunner.Execute();
                //proxyAddRunner.RunPlugin(ServiceProvider);

                if (!proxyAddRunner.Success)
                {
                    //Logger.WriteToFile("Proxy Add did not Succeed, ending integration pipeline");
                    Trace("Proxy Add did not Succeed, ending integration pipeline.", LogLevel.Debug, true);
                    LogExit(1);
                    return false;
                }
                //Logger.WriteDebugMessage($"Proxy add wait start time {System.DateTime.Now.ToString()}");
                Trace($"Proxy add wait start time {System.DateTime.Now}.", LogLevel.Debug, true);

                var integrationSettings = context.mcs_integrationsettingSet.Select(x => new IntegrationSetting { Name = x.mcs_name, Value = x.mcs_value }).ToList();
                var WaitValue = Convert.ToInt32(integrationSettings.First(x => x.Name == "Adding wait value in seconds").Value);
                System.Threading.Thread.Sleep(WaitValue * 1000);

                //Logger.WriteDebugMessage($"Proxy add wait complete time {System.DateTime.Now.ToString()}");
                Trace($"Proxy add wait complete time {System.DateTime.Now}.", LogLevel.Debug, true);
                Veterans = proxyAddRunner.VeteransChanged;


                //Logger.WriteDebugMessage("Exit RunProxyAdd()");
                Trace("Exit RunProxyAdd().", LogLevel.Debug, true);
            }

            LogExit(2);

            return true;
        }

        /// <summary>
        /// VMR entry point
        /// </summary>
        /// <returns></returns>
        private bool RunVmr()
        {
            LogEntry();

            //Logger.WriteDebugMessage("Running VMR");
            Trace("Running VMR.", LogLevel.Debug, true);

            // Only run this on create or retry - for subsequent adding of individual person to H/M group, skip this
            if (PluginExecutionContext.MessageName == "Create" || PrimaryEntity?.Attributes?.Contains("cvt_retryintegration") == true)
            {
                if (PrimaryEntity?.Attributes?.Contains("cvt_retryintegration") == true && (bool)PrimaryEntity["cvt_retryintegration"])
                {
                    //Logger.WriteDebugMessage("Checking to see if VMR retry is necessary");
                    Trace("Checking to see if VMR retry is necessary.", LogLevel.Debug, true);

                    using (var srv = new Xrm(OrganizationService))
                    {
                        var failedVmr = srv.mcs_integrationresultSet.FirstOrDefault(x =>
                            x.mcs_serviceappointmentid.Id == PrimaryEntity.Id &&
                            x.mcs_status.Value == (int)mcs_integrationresultmcs_status.Error &&
                            x.mcs_name == "Create Virtual Meeting Room");

                        // If running retry, make sure that VMR wasn't already done.  If so, skip this integration and move on to the next one
                        if (failedVmr == null)
                        {
                            //Logger.WriteDebugMessage("No need to retry VMR");
                            Trace("No need to retry VMR.", LogLevel.Debug, true);
                            LogExit(1);
                            return true;
                        }
                        else
                        {
                            //Logger.WriteDebugMessage("VMR retry will execute");
                            Trace("VMR retry will execute.", LogLevel.Debug, true);
                        }
                    }
                }

                var vmrRunner = new ServiceAppointmentVmrUpdatePostStageRunner(ServiceProvider);
                vmrRunner.Execute();
                //vmrRunner.RunPlugin(ServiceProvider);

                if (!vmrRunner.Success)
                {
                    //Logger.WriteToFile("Create VMR did not succeed, ending integration pipeline");
                    Trace("Create VMR did not succeed, ending integration pipeline.", LogLevel.Debug, true);
                    LogExit(2);
                    return false;
                }
            }
            else
            {
                //Logger.WriteDebugMessage("Skipping VMR for non-retry SA Update");
                Trace("Skipping VMR for non-retry SA Update.", LogLevel.Debug, true);
            }

            LogExit(3);

            return true;
        }

        private bool RunVista(bool isMake)
        {
            LogEntry();

            if (isMake)
            {
                var vistaRunner = new ServiceAppointmentVistaHealthShareUpdatePostStageRunner(ServiceProvider);
                vistaRunner.Execute();
                //vistaRunner.RunPlugin(ServiceProvider);

                if (!vistaRunner.Success)
                {
                    //Logger.WriteToFile("Vista Make Appointment did not succeed, ending integration pipeline");
                    Trace("Vista Make Appointment did not succeed, ending integration pipeline.", LogLevel.Debug, true);
                }
                LogExit(1);
                return vistaRunner.Success;
            }
            else
            {
                var vistaRunner = new ServiceAppointmentVistaHealthShareCancelPostStageRunner(ServiceProvider);
                vistaRunner.Execute();
                //vistaRunner.RunPlugin(ServiceProvider);

                if (!vistaRunner.Success)
                {
                    //Logger.WriteToFile("Vista Cancel Appointment did not succeed, ending integration pipeline");
                    Trace("Vista Cancel Appointment did not succeed, ending integration pipeline.", LogLevel.Debug, true);
                }
                LogExit(2);
                return vistaRunner.Success;
            }
        }

        private bool RunVvs(DataModel.ServiceAppointment sa)
        {
            LogEntry();

            //bool isisVistaTypeFacility;

            if (sa == null) throw new InvalidPluginExecutionException($"Unable to Find Service Appointment with id {PrimaryEntity.Id}");

            // Ensure this is NOT a Telephone Call VVC Service Appointment.
            if (sa.cvt_TelephoneCall.HasValue && sa.cvt_TelephoneCall.Value)
            {
                //Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VVS integration");
                Trace("This is a Telephone Call Appointment, hence skipping VVS integration.", LogLevel.Debug, true);
                Success = true;
                return Success;
            }

            using (var context = new Xrm(OrganizationService))
            {
                //if (isisVistaTypeFacility)
                //{
                sa = context.ServiceAppointmentSet.FirstOrDefault(appt => appt.Id == sa.Id);

                //Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
                Trace("The Current Facility IS a Vista Facility.  Continue Processing.", LogLevel.Debug, true);

                if (VistaPluginHelpers.RunVvs(sa, context, pluginLogger, PluginExecutionContext))
                {
                    ApiIntegrationSettingsCreate = IntegrationPluginHelpers.GetApiSettings(context, "VvsCreateUri");
                    ApiIntegrationSettingsUpdate = IntegrationPluginHelpers.GetApiSettings(context, "VvsUpdateUri");

                    SendVistaMessage(sa);
                    Success = true;
                }
                else
                {
                    //Logger.WriteDebugMessage("VVS Integration Bypassed, not updating Service Activity Status");
                    Trace("VVS Integration Bypassed, not updating Service Activity Status.", LogLevel.Debug, true);
                    Success = true;
                }
            }

            LogExit(1);

            return Success;
        }

        private void SendVistaMessage(DataModel.ServiceAppointment serviceAppointment)
        {
            // Default the success flag to false so that it is false until it successfully reaches a completed Vista Booking
            var changedPatients = new List<Guid>();

            //Logger.WriteDebugMessage("Beginning VistA Book/Cancel");
            Trace("Beginning VistA Book/Cancel.", LogLevel.Debug, true);

            //Logger.WriteDebugMessage("Using Vista Integration Results to determine Patient Delta");
            Trace("Using Vista Integration Results to determine Patient Delta.", LogLevel.Debug, true);
            if (serviceAppointment.Customers != null) changedPatients.Add(serviceAppointment.Customers.Select(c => c.PartyId.Id).First());
            var createRequest = new VideoVisitCreateRequestMessage
            {
                AppointmentId = serviceAppointment.Id,
                LogRequest = true,
                OrganizationName = PluginExecutionContext.OrganizationName,
                UserId = serviceAppointment.CreatedBy.Id,
                AddedPatients = changedPatients
            };

            //Logger.WriteDebugMessage("Sending Create Booking to Vista");
            Trace("Sending Create Booking to Vista.", LogLevel.Debug, true);

            var baseUrl = ApiIntegrationSettingsCreate.BaseUrl;
            var uri = ApiIntegrationSettingsCreate.Uri;
            var resource = ApiIntegrationSettingsCreate.Resource;
            var appId = ApiIntegrationSettingsCreate.AppId;
            var secret = ApiIntegrationSettingsCreate.Secret;
            var authority = ApiIntegrationSettingsCreate.Authority;
            var tenantId = ApiIntegrationSettingsCreate.TenantId;
            var subscriptionId = ApiIntegrationSettingsCreate.SubscriptionId;
            var isProdApi = ApiIntegrationSettingsCreate.IsProdApi;
            var subscriptionIdEast = ApiIntegrationSettingsCreate.SubscriptionIdEast;
            var subscriptionIdSouth = ApiIntegrationSettingsCreate.SubscriptionIdSouth;

            var request = VA.TMP.Integration.Common.Serialization.DataContractSerialize(createRequest);
            //Logger.WriteDebugMessage($"Create request {request}");
            Trace($"Create request {request}", LogLevel.Debug, true);

            var response = RestPoster.Post<VideoVisitCreateRequestMessage, VideoVisitCreateResponseMessage>("VVS Create", baseUrl, uri, createRequest, resource, appId, secret, authority,
                tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag, pluginLogger);
            response.VimtLagMs = lag;

            ProcessVistaCreateResponse(response, typeof(VideoVisitCreateRequestMessage), typeof(VideoVisitCreateResponseMessage), serviceAppointment);
        }

        private bool RunCancelVmr()
        {
            LogEntry();

            var cancelRunner = new ServiceAppointmentVmrCancelPostStageRunner(ServiceProvider);
            cancelRunner.Execute();
            //cancelRunner.RunPlugin(ServiceProvider);

            if (!cancelRunner.Success)
            {
                //Logger.WriteToFile("Cancel VMR failed, ending integration pipeline");
                Trace("Cancel VMR failed, ending integration pipeline.", LogLevel.Debug, true);
            }

            LogExit(1);

            return cancelRunner.Success;
        }

        private void ProcessVistaBookResponse(bool isCreate, DataModel.ServiceAppointment serviceAppointment, string errorMessage, bool exceptionOccured, string vimtRequest, string serializedInstance, string vimtResponse, int EcProcessingTime, int vimtProcessingTime, int lagTime, WriteResults writeResults)
        {
            var name = isCreate ? "Create VVS" : "Update VVS";
            var reqType = isCreate ? typeof(VideoVisitCreateRequestMessage).FullName : typeof(VideoVisitUpdateRequestMessage).FullName;
            var respType = isCreate ? typeof(VideoVisitCreateResponseMessage).FullName : typeof(VideoVisitUpdateResponseMessage).FullName;
            var regName = isCreate ? MessageRegistry.VideoVisitCreateRequestMessage : MessageRegistry.VideoVisitUpdateRequestMessage;
            IntegrationPluginHelpers.CreateIntegrationResult(name, exceptionOccured, errorMessage, vimtRequest, serializedInstance, vimtResponse, reqType, respType, regName,
                serviceAppointment.Id, OrganizationService, lagTime, EcProcessingTime, vimtProcessingTime, null, false);

            if (exceptionOccured)
            {
                //Logger.WriteDebugMessage("VVS Booking Failed, not updating Service Activity Status");
                Trace("VVS Booking Failed, not updating Service Activity Status.", LogLevel.Debug, true);
                return;
            }

            Success = !exceptionOccured;
        }

        private void ProcessVistaCreateResponse(VideoVisitCreateResponseMessage response, Type requestType, Type responseType, DataModel.ServiceAppointment appointment)
        {
            if (response == null) return;
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            ProcessVistaBookResponse(true, appointment, errorMessage, response.ExceptionOccured, response.VimtRequest, response.SerializedInstance, response.VimtResponse, response.EcProcessingMs, response.VimtProcessingMs, response.VimtLagMs, response.WriteResults);
        }

        private void LogEntry([CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                string className = this.GetType().Name;
                //Logger.WriteDebugMessage($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}");
                Trace($"Entered {className}.{memberName}() | LineNumber = {sourceLineNumber}", LogLevel.Debug, true);

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
                Trace($"Exiting {className}.{memberName}() | Exit Point = {exitPoint} | LineNumber = {sourceLineNumber}", LogLevel.Debug, true);
            }
            catch (Exception)
            {
                // Fail silently - this diagnostic code should never interrupt the integration process 
            }

        }
    }
}
