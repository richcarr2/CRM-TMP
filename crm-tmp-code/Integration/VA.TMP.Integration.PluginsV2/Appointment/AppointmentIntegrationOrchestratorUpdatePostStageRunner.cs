using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MCS.ApplicationInsights;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentIntegrationOrchestratorUpdatePostStageRunner : AILogicBase
    {
        public AppointmentIntegrationOrchestratorUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "mcs_appointmentplugin";

        public Dictionary<Guid, bool> VeteransDelta { get; set; }

        public override void ExecuteLogic()
        {

            LogEntry();
            //Logger.setDebug = true;
            //Logger.WriteDebugMessage("AppointmentIntegrationOrchestratorUpdatePostStageRunner -Execute() Initiated.");
            Trace("AppointmentIntegrationOrchestratorUpdatePostStageRunner -Execute() Initiated.", LogLevel.Debug);
            var apptImage = PrimaryEntity.ToEntity<DataModel.Appointment>();
            if (apptImage == null) throw new InvalidPluginExecutionException("Invalid Plugin Registration");

            var entityCollection = PrimaryEntity.GetAttributeValue<EntityCollection>("optionalattendees");

            var patientsChanged = entityCollection?.Entities != null && entityCollection.Entities.Count() > 0;
            var statusChange = apptImage.cvt_IntegrationBookingStatus != null;
            var retryInitiated = apptImage.cvt_RetryIntegration;

            //Logger.WriteDebugMessage($"+=+=+=+=+= Patients Changed: {patientsChanged} +=+=+=+=+=");
            Trace($"+=+=+=+=+= Patients Changed: {patientsChanged} +=+=+=+=+=", LogLevel.Debug);
            //Logger.WriteDebugMessage($"+=+=+=+=+= Status Changed: {statusChange} +=+=+=+=+=");
            Trace($"+=+=+=+=+= Status Changed: {statusChange} +=+=+=+=+=", LogLevel.Debug);
            //Logger.WriteDebugMessage($"+=+=+=+=+= Retry Initiated: {retryInitiated.HasValue && retryInitiated.Value} +=+=+=+=+=");
            Trace($"+=+=+=+=+= Retry Initiated: {retryInitiated.HasValue && retryInitiated.Value} +=+=+=+=+=", LogLevel.Debug);

            if (retryInitiated.HasValue && retryInitiated.Value)
            {
                //Logger.WriteDebugMessage("About to ExecuteRetry");
                Trace("About to ExecuteRetry.", LogLevel.Debug);
                ExecuteRetry();
            }
            else if (patientsChanged) ExecuteBook();
            else if (statusChange)
            {
                switch (apptImage.cvt_IntegrationBookingStatus.Value)
                {
                    case (int)Appointmentcvt_IntegrationBookingStatus.PartialVistaFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.VistaFailure:
                        //Logger.WriteDebugMessage("Vista Integration Failures don't trigger cancellations, ending plugin now");
                        Trace("Vista Integration Failures don't trigger cancellations, ending plugin now.", LogLevel.Debug);
                        break;
                    case (int)Appointmentcvt_IntegrationBookingStatus.TechnologyFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.SchedulingError:
                    case (int)Appointmentcvt_IntegrationBookingStatus.PatientNoShow:
                    case (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled:
                    case (int)Appointmentcvt_IntegrationBookingStatus.ClinicCancelled:
                        //Logger.WriteDebugMessage("About to cancel");
                        Trace("About to cancel.", LogLevel.Debug);
                        ExecuteCancel();
                        break;
                    default:
                        //Logger.WriteDebugMessage("Vista Integration not triggered from status " + ((Appointmentcvt_IntegrationBookingStatus)apptImage.cvt_IntegrationBookingStatus.Value).ToString());
                        Trace($"Vista Integration not triggered from status {((Appointmentcvt_IntegrationBookingStatus)apptImage.cvt_IntegrationBookingStatus.Value)}.", LogLevel.Debug);
                        break;
                }
            }

            LogExit(1);
        }

        public void ExecuteBook()
        {
            LogEntry();

            if (RunProxyAdd() && RunVista() && RunVvs())
            {
                //Logger.WriteDebugMessage("Successfully Completed all integration pipeline");
                Trace("Successfully Completed all integration pipeline.", LogLevel.Debug);
            }

            LogExit(1);
        }

        public void ExecuteCancel()
        {
            LogEntry();

            //Logger.WriteDebugMessage("Going to RunVista");
            Trace("Going to RunVista.", LogLevel.Debug);
            if (RunVista())
            {
                //Logger.WriteDebugMessage("Going to RunVVS");
                Trace("Going to RunVVS.", LogLevel.Debug);
                if (RunVvs())
                {
                    //Logger.WriteDebugMessage("Successfully Completed all integration pipeline");
                    Trace("Successfully Completed all integration pipeline.", LogLevel.Debug);
                }
                else
                {
                    //Logger.WriteDebugMessage("RunVista RunVVS");
                    Trace("RunVista RunVVS.", LogLevel.Debug);
                }
            }
            else
            {
                //Logger.WriteDebugMessage("RunVista failed");
                Trace("RunVista failed.", LogLevel.Debug);
            }
            LogExit(1);
        }

        private void ExecuteRetry()
        {
            LogEntry();

            List<mcs_integrationresult> failedIrs;
            using (var srv = new Xrm(OrganizationService)) failedIrs = srv.mcs_integrationresultSet.Where(ir =>
                ir.mcs_appointmentid.Id == PrimaryEntity.Id &&
                ir.mcs_status.Value == (int)mcs_integrationresultmcs_status.Error).ToList();

            var retryIr = IntegrationPluginHelpers.FindEarliestFailure(failedIrs, Logger);

            if (retryIr == null)
            {
                //Logger.WriteDebugMessage("Skipping Retry as Integration Result with error status could not be found.");
                Trace("Skipping Retry as Integration Result with error status could not be found.", LogLevel.Debug);
            }
            else RunRetry(retryIr);

            // Reset the Retry integration field post run retry execution
            //Logger.WriteDebugMessage("BEGIN Update Retry Flag to False");
            Trace("BEGIN Update Retry Flag to False.", LogLevel.Debug);
            var apt = new DataModel.Appointment { Id = PrimaryEntity.Id, cvt_RetryIntegration = false };
            OrganizationService.Update(apt);
            //Logger.WriteDebugMessage("END Update Retry Flag to False");
            Trace("END Update Retry Flag to False.", LogLevel.Debug);

            LogExit(1);
        }

        private void RunRetry(mcs_integrationresult integrationResult)
        {
            LogEntry();

            var typeOfIntegration = integrationResult.mcs_VimtMessageRegistryName;
            var successfulRetry = false;
            var finishedIntegrationPipeline = false;
            switch (typeOfIntegration)
            {
                case MessageRegistry.ProxyAddRequestMessage:
                    successfulRetry = RunProxyAdd();
                    if (successfulRetry && RunVista() && RunVvs()) finishedIntegrationPipeline = true;
                    break;
                case MessageRegistry.VideoVisitCreateRequestMessage:
                    successfulRetry = RunVvs();
                    finishedIntegrationPipeline = successfulRetry;
                    break;
                case MessageRegistry.HealthShareMakeCancelOutboundRequestMessage:
                    successfulRetry = RunVista();
                    if (successfulRetry) finishedIntegrationPipeline = RunVvs();
                    break;
                case MessageRegistry.VideoVisitDeleteRequestMessage:
                    successfulRetry = RunVvs();
                    finishedIntegrationPipeline = successfulRetry;
                    break;
                default:
                    //Logger.WriteDebugMessage("Unknown Message Type, skipping retry");
                    Trace("Unknown Message Type, skipping retry.", LogLevel.Debug);
                    break;
            }

            if (successfulRetry)
            {
                //Logger.WriteDebugMessage("BEGIN Update Failed IR Status");
                Trace("BEGIN Update Failed IR Status.", LogLevel.Debug);
                ChangeIrStatusToRetrySuccess(integrationResult);
                //Logger.WriteDebugMessage("BEGIN Update Failed IR Status");
                Trace("END Update Failed IR Status.", LogLevel.Debug);
            }
            else
            {
                //Logger.WriteDebugMessage("IR Status is not updated because successfulRetry is false");
                Trace("IR Status is not updated because successfulRetry is false.", LogLevel.Debug);
            }

            var integrationStatus = finishedIntegrationPipeline ? "Succeeded" : "Did Not Succeed";
            //Logger.WriteToFile(string.Format("Remaining Integrations All {0}.", finishedIntegrationPipeline ? "Succeeded" : "Did Not Succeed"));
            Trace($"Remaining Integrations All {integrationStatus}.", LogLevel.Debug);

            LogExit(1);
        }

        private void ChangeIrStatusToRetrySuccess(mcs_integrationresult ir)
        {
            LogEntry();

            //Logger.WriteDebugMessage($"Updating IR {ir.mcs_VimtMessageRegistryName} to retry success");
            Trace($"Updating IR {ir.mcs_VimtMessageRegistryName} to retry success.", LogLevel.Debug);

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

            //Logger.WriteToFile("About to run ProxyAdd");
            Trace("About to run ProxyAdd.", LogLevel.Debug);
            var proxyAddRunner = new AppointmentUpdatePostStageRunner(ServiceProvider);
            proxyAddRunner.Execute();
            //proxyAddRunner.RunPlugin(ServiceProvider);
            VeteransDelta = proxyAddRunner.VeteransDelta;

            if (!proxyAddRunner.Success)
            {
                //Logger.WriteToFile("Proxy Add did not succeed, Ending Integration Series");
                Trace("Proxy Add did not succeed, Ending Integration Series.", LogLevel.Debug);
            }

            LogExit(1);
            return proxyAddRunner.Success;

        }

        private bool RunVista()
        {
            LogEntry();

            //Logger.WriteToFile("About to run RunVista");
            Trace("About to run RunVista.", LogLevel.Debug);

            var vistaRunner = new AppointmentUpdateVistaHealthSharePostStageRunner(ServiceProvider);
            vistaRunner.Execute();
            //vistaRunner.RunPlugin(ServiceProvider);

            if (!vistaRunner.Success)
            {
                //Logger.WriteToFile("Vista did not succeed, ending integrations");
                Trace("Vista did not succeed, ending integrations.", LogLevel.Debug);
            }

            LogExit(1);
            return vistaRunner.Success;
        }

        private bool RunVvs()
        {
            LogEntry();

            //Logger.WriteToFile("About to run RunVVS");
            Trace("About to run RunVVS.", LogLevel.Debug);
            var vvsRunner = new AppointmentUpdateVVSPostStageRunner(ServiceProvider, VeteransDelta);
            vvsRunner.Execute();
            //vvsRunner.RunPlugin(ServiceProvider);

            if (!vvsRunner.Success)
            {
                //Logger.WriteToFile("Vvs did not succeed");
                Trace("Vvs did not succeed.", LogLevel.Debug);
            }

            LogExit(1);
            return vvsRunner.Success;
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
