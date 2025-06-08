using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MCSHelperClass;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentIntegrationOrchestratorPostStageRunner : PluginRunner
    {
        public ServiceAppointmentIntegrationOrchestratorPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        private Dictionary<Guid, bool> Veterans { get; set; }

        public override void Execute()
        {
            LogEntry();

            var isStatusChange = PluginExecutionContext.InputParameters.Contains("EntityMoniker");

            if (isStatusChange) ExecuteCancel();
            else
            {
                var saImage = PrimaryEntity.ToEntity<DataModel.ServiceAppointment>();

                Logger.WriteDebugMessage("Checking Facility Type!");

                Logger.WriteDebugMessage("The Current Facility IS a Vistar Facility.  Continue Processing.");
                var patientsChanged = saImage.Customers != null;

                var preImage = (GetSecondaryEntity()?.ToEntity<DataModel.ServiceAppointment>() ?? null);

                if (patientsChanged) ExecuteBook(saImage, preImage);

                var isRetry = saImage.cvt_RetryIntegration;

                if (isRetry.HasValue && isRetry.Value) ExecuteRetry();

            }

            LogExit(1);
        }

        private void ExecuteBook(DataModel.ServiceAppointment sa, DataModel.ServiceAppointment preImage)
        {

            LogEntry();

            OptionSetValue statusCode = null;

            if (sa.StatusCode != null)
            {
                statusCode = sa.StatusCode;
                Logger.WriteDebugMessage("StatusCode is null");
            }
            else
            {
                Logger.WriteDebugMessage("StatusCode on SA is null, getting status code from pre-image");

                if (preImage != null)
                {
                    statusCode = preImage.StatusCode;
                }
                else
                {
                    throw new InvalidPluginExecutionException("ExecuteBook(DataModel.ServiceAppointment sa, DataModel.ServiceAppointment preImage) : StatusCode is null");
                }

            }

            Logger.WriteDebugMessage("Enter ExecuteBook(DataModel.ServiceAppointment sa)");
            //Skip the Integration when the  user updates teh SA status to Open >> Requested: Open. 
            //The status reason is set to Requested: Open to free up the resources associated to appointment when there is an integration error
            if (PluginExecutionContext.MessageName.ToLower() == "update" && statusCode.Value == (int)serviceappointment_statuscode.RequestedOpen)
            {
                Logger.WriteDebugMessage("ServiceAppointmentIntegrationOrchestratorPostStageRunner - Skipping ExecuteBook() as the record is being updated to status reason Request Open.");
                LogExit(1);
                return;
            }

            Logger.WriteDebugMessage("About to run to run the integration pipeline for a service appointment book");

            // Runs left to right and stops as soon as one fails, so ordering determines order of integrations
            //if (RunProxyAdd() && RunVmr() && RunVista(true) && RunVvs()) Logger.WriteDebugMessage("Successfully completed booking integration pipeline.");
            //Logic for RunVVS was moved to the IntegrationResultUpdatePostStage Plugin
            if (RunProxyAdd() && RunVmr() && RunVista(true)) Logger.WriteDebugMessage("Successfully completed booking integration pipeline.");

            Logger.WriteDebugMessage("Finished the integration pipeline for a service appointment book");

            LogExit(2);
        }

        private void ExecuteCancel()
        {
            LogEntry();

            // Runs left to right and stops as soon as one fails, so ordering determines order of integrations
            //if (RunCancelVmr() && RunVista(false) && RunCancelVvs()) Logger.WriteDebugMessage("Successfully completed cancel integration pipeline.");
            //Logic for RunCancelVVS was moved to the IntegrationResultUpdatePostStage Plugin
            if (RunCancelVmr() && RunVista(false)) Logger.WriteDebugMessage("Successfully completed cancel integration pipeline.");

            LogExit(1);

        }

        /// <summary>
        /// Logic to execute a Retry
        /// </summary>
        private void ExecuteRetry()
        {
            LogEntry();

            if (!(bool)PrimaryEntity.Attributes["cvt_retryintegration"]) return;

            Logger.WriteDebugMessage("Retry Initiated");

            using (var srv = new Xrm(OrganizationService))
            {
                var failedIRs = srv.mcs_integrationresultSet.Where(ir => ir.mcs_serviceappointmentid.Id == PrimaryEntity.Id && ir.mcs_status.Value == (int)mcs_integrationresultmcs_status.Error).ToList();
                var retryIr = IntegrationPluginHelpers.FindEarliestFailure(failedIRs, Logger);

                if (retryIr == null) Logger.WriteDebugMessage("Skipping Retry as Integration Result with error status could not be found.");
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
            Logger.WriteDebugMessage($"Executing RetryIntegrationResult. Type of Integration: {typeOfIntegration}");

            var successfulRetry = false;
            var finishedIntegrationPipeline = false;
            var isMake = !PluginExecutionContext.InputParameters.Contains("EntityMoniker");

            switch (typeOfIntegration)
            {
                case MessageRegistry.ProxyAddRequestMessage:
                    successfulRetry = RunProxyAdd();
                    //if (successfulRetry && RunVmr() && RunVista(true) && RunVvs()) finishedIntegrationPipeline = true;
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
                    Logger.WriteDebugMessage($"Unknown Message Type - {typeOfIntegration}, skipping retry");
                    break;
            }

            if (successfulRetry) ChangeIrStatusToRetrySuccess(integrationResult);

            Logger.WriteToFile(string.Format("Remaining Integrations {0}.", finishedIntegrationPipeline ? "Succeeded" : "Did Not Succeed"));

            LogExit(1);

        }

        private void ChangeIrStatusToRetrySuccess(mcs_integrationresult ir)
        {
            LogEntry();

            Logger.WriteDebugMessage($"Updating IR {ir.mcs_VimtMessageRegistryName} to retry success");
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
                proxyAddRunner.RunPlugin(ServiceProvider);

                if (!proxyAddRunner.Success)
                {
                    Logger.WriteToFile("Proxy Add did not Succeed, ending integration pipeline");
                    LogExit(1);
                    return false;
                }
                Logger.WriteDebugMessage($"Proxy add wait start time {System.DateTime.Now.ToString()}");

                var integrationSettings = context.mcs_integrationsettingSet.Select(x => new IntegrationSetting { Name = x.mcs_name, Value = x.mcs_value }).ToList();
                var WaitValue = Convert.ToInt32(integrationSettings.First(x => x.Name == "Adding wait value in seconds").Value);
                System.Threading.Thread.Sleep(WaitValue * 1000);

                Logger.WriteDebugMessage($"Proxy add wait complete time {System.DateTime.Now.ToString()}");
                Veterans = proxyAddRunner.VeteransChanged;


                Logger.WriteDebugMessage("Exit RunProxyAdd()");
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

            Logger.WriteDebugMessage("Running VMR");

            // Only run this on create or retry - for subsequent adding of individual person to H/M group, skip this
            if (PluginExecutionContext.MessageName == "Create" || PrimaryEntity?.Attributes?.Contains("cvt_retryintegration") == true)
            {
                if (PrimaryEntity?.Attributes?.Contains("cvt_retryintegration") == true && (bool)PrimaryEntity["cvt_retryintegration"])
                {
                    Logger.WriteDebugMessage("Checking to see if VMR retry is necessary");

                    using (var srv = new Xrm(OrganizationService))
                    {
                        var failedVmr = srv.mcs_integrationresultSet.FirstOrDefault(x =>
                            x.mcs_serviceappointmentid.Id == PrimaryEntity.Id &&
                            x.mcs_status.Value == (int)mcs_integrationresultmcs_status.Error &&
                            x.mcs_name == "Create Virtual Meeting Room");

                        // If running retry, make sure that VMR wasn't already done.  If so, skip this integration and move on to the next one
                        if (failedVmr == null)
                        {
                            Logger.WriteDebugMessage("No need to retry VMR");
                            LogExit(1);
                            return true;
                        }
                        else Logger.WriteDebugMessage("VMR retry will execute");
                    }
                }

                var vmrRunner = new ServiceAppointmentVmrUpdatePostStageRunner(ServiceProvider);
                vmrRunner.RunPlugin(ServiceProvider);

                if (!vmrRunner.Success)
                {
                    Logger.WriteToFile("Create VMR did not succeed, ending integration pipeline");
                    LogExit(2);
                    return false;
                }
            }
            else Logger.WriteDebugMessage("Skipping VMR for non-retry SA Update");

            LogExit(3);

            return true;
        }

        private bool RunVista(bool isMake)
        {
            LogEntry();

            if (isMake)
            {
                var vistaRunner = new ServiceAppointmentVistaHealthShareUpdatePostStageRunner(ServiceProvider);
                vistaRunner.RunPlugin(ServiceProvider);

                if (!vistaRunner.Success) Logger.WriteToFile("Vista Make Appointment did not succeed, ending integration pipeline");
                LogExit(1);
                return vistaRunner.Success;
            }
            else
            {
                var vistaRunner = new ServiceAppointmentVistaHealthShareCancelPostStageRunner(ServiceProvider);
                vistaRunner.RunPlugin(ServiceProvider);

                if (!vistaRunner.Success) Logger.WriteToFile("Vista Cancel Appointment did not succeed, ending integration pipeline");
                LogExit(2);
                return vistaRunner.Success;
            }


        }

        //Logic for RunVVS was moved to the IntegrationResultUpdatePostStage Plugin
        //private bool RunVvs()
        //{
        //    LogEntry();

        //    var vvsRunner = new ServiceAppointmentVvsUpdatePostStageRunner(ServiceProvider, Veterans);
        //    vvsRunner.RunPlugin(ServiceProvider);

        //    if (!vvsRunner.Success) Logger.WriteToFile("VVS did not succeed, ending integration pipeline");

        //    LogExit(1);

        //    return vvsRunner.Success;
        //}

        //Logic for RunCancelVVS was moved to the IntegrationResultUpdatePostStage Plugin
        //private bool RunCancelVvs()
        //{
        //    LogEntry();

        //    var cancelRunner = new ServiceAppointmentCancelPostStageRunner(ServiceProvider);
        //    cancelRunner.RunPlugin(ServiceProvider);

        //    if (!cancelRunner.Success) Logger.WriteToFile("Cancel VVS failed, ending integration pipeline");

        //    LogExit(1);

        //    return cancelRunner.Success;
        //}

        private bool RunCancelVmr()
        {
            LogEntry();

            var cancelRunner = new ServiceAppointmentVmrCancelPostStageRunner(ServiceProvider);
            cancelRunner.RunPlugin(ServiceProvider);

            if (!cancelRunner.Success) Logger.WriteToFile("Cancel VMR failed, ending integration pipeline");

            LogExit(1);

            return cancelRunner.Success;
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
