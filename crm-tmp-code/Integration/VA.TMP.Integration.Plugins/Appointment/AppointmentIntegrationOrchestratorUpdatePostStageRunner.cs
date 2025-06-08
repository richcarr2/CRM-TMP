using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentIntegrationOrchestratorUpdatePostStageRunner : PluginRunner
    {
        public AppointmentIntegrationOrchestratorUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "mcs_appointmentplugin";

        public Dictionary<Guid, bool> VeteransDelta { get; set; }

        public override void Execute()
        {

            LogEntry();

            Logger.WriteDebugMessage("AppointmentIntegrationOrchestratorUpdatePostStageRunner -Execute() Initiated.");
            var apptImage = PrimaryEntity.ToEntity<DataModel.Appointment>();
            if (apptImage == null) throw new InvalidPluginExecutionException("Invalid Plugin Registration");
           
            var entityCollection = PrimaryEntity.GetAttributeValue<EntityCollection>("optionalattendees");

            var patientsChanged = entityCollection?.Entities != null && entityCollection.Entities.Count() > 0;
            var statusChange = apptImage.cvt_IntegrationBookingStatus != null;
            var retryInitiated = apptImage.cvt_RetryIntegration;

            Logger.WriteDebugMessage($"+=+=+=+=+= Patients Changed: {patientsChanged} +=+=+=+=+=");
            Logger.WriteDebugMessage($"+=+=+=+=+= Status Changed: {statusChange} +=+=+=+=+=");
            Logger.WriteDebugMessage($"+=+=+=+=+= Retry Initiated: {retryInitiated.HasValue && retryInitiated.Value} +=+=+=+=+=");

            if (retryInitiated.HasValue && retryInitiated.Value) ExecuteRetry();
            else if (patientsChanged) ExecuteBook();
            else if (statusChange)
            {
                switch (apptImage.cvt_IntegrationBookingStatus.Value)
                {
                    case (int)Appointmentcvt_IntegrationBookingStatus.PartialVistaFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.CancelFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.VistaFailure:
                        Logger.WriteDebugMessage("Vista Integration Failures don't trigger cancellations, ending plugin now");
                        break;
                    case (int)Appointmentcvt_IntegrationBookingStatus.TechnologyFailure:
                    case (int)Appointmentcvt_IntegrationBookingStatus.SchedulingError:
                    case (int)Appointmentcvt_IntegrationBookingStatus.PatientNoShow:
                    case (int)Appointmentcvt_IntegrationBookingStatus.PatientCanceled:
                    case (int)Appointmentcvt_IntegrationBookingStatus.ClinicCancelled:
                        ExecuteCancel();
                        break;
                    default:
                        Logger.WriteDebugMessage("Vista Integration not triggered from status " + ((Appointmentcvt_IntegrationBookingStatus)apptImage.cvt_IntegrationBookingStatus.Value).ToString());
                        break;
                }
            }

            LogExit(1);
        }

        public void ExecuteBook()
        {
            LogEntry();

            if (RunProxyAdd() && RunVista() && RunVvs()) Logger.WriteDebugMessage("Successfully Completed all integration pipeline");

            LogExit(1);
        }

        public void ExecuteCancel()
        {
            LogEntry();

            if (RunVista() && RunVvs()) Logger.WriteDebugMessage("Successfully Completed all integration pipeline");

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

            if (retryIr == null) Logger.WriteDebugMessage("Skipping Retry as Integration Result with error status could not be found.");
            else RunRetry(retryIr);

            // Reset the Retry integration field post run retry execution
            Logger.WriteDebugMessage("BEGIN Update Retry Flag to False");
            var apt = new DataModel.Appointment { Id = PrimaryEntity.Id, cvt_RetryIntegration = false };
            OrganizationService.Update(apt);
            Logger.WriteDebugMessage("END Update Retry Flag to False");

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
                    Logger.WriteDebugMessage("Unknown Message Type, skipping retry");
                    break;
            }

            if (successfulRetry)
            {
                Logger.WriteDebugMessage("BEGIN Update Failed IR Status");
                ChangeIrStatusToRetrySuccess(integrationResult);
                Logger.WriteDebugMessage("BEGIN Update Failed IR Status");
            }
            else Logger.WriteDebugMessage("IR Status is not updated because successfulRetry is false");

            Logger.WriteToFile(string.Format("Remaining Integrations All {0}.", finishedIntegrationPipeline ? "Succeeded" : "Did Not Succeed"));

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

            var proxyAddRunner = new AppointmentUpdatePostStageRunner(ServiceProvider);
            proxyAddRunner.RunPlugin(ServiceProvider);
            VeteransDelta = proxyAddRunner.VeteransDelta;

            if (!proxyAddRunner.Success) Logger.WriteToFile("Proxy Add did not succeed, Ending Integration Series");

            LogExit(1);
            return proxyAddRunner.Success;

        }

        private bool RunVista()
        {
            LogEntry();

            var vistaRunner = new AppointmentUpdateVistaHealthSharePostStageRunner(ServiceProvider);
            vistaRunner.RunPlugin(ServiceProvider);

            if (!vistaRunner.Success) Logger.WriteToFile("Vista did not succeed, ending integrations");

            LogExit(1);
            return vistaRunner.Success;
        }

        private bool RunVvs()
        {
            LogEntry();

            var vvsRunner = new AppointmentUpdateVVSPostStageRunner(ServiceProvider, VeteransDelta);
            vvsRunner.RunPlugin(ServiceProvider);

            if (!vvsRunner.Success) Logger.WriteToFile("Vvs did not succeed");

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
