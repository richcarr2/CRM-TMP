using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.CRM;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Messages.Cerner;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentVistaHealthShareCancelPostStageRunner : PluginRunner
    {
        public ServiceAppointmentVistaHealthShareCancelPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        /// <summary>
        /// Gets the primary entity.
        /// </summary>
        /// <returns>Returns the primary entity.</returns>
        public override Entity GetPrimaryEntity()
        {
            LogEntry();

            var primaryReference = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];

            LogExit(1);

            return new Entity(primaryReference.LogicalName) { Id = primaryReference.Id };
        }

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

        public bool Success { get; set; }

        public override void Execute()
        {
            LogEntry();

            var saRecord = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();
            bool filter;
            bool VistaTypeFacility = true;

            using (var context = new Xrm(OrganizationService))
            {
                filter = VistaPluginHelpers.RunVistaIntegration(saRecord, context, Logger);
                Logger.WriteDebugMessage($"Run HealthShare Cancel after checking switches is set to {filter}");
                Logger.WriteDebugMessage("Checking Facility Type!");
                VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(saRecord, context, Logger);
            }

            if (saRecord.cvt_Type != null && (saRecord.mcs_groupappointment == null || !saRecord.mcs_groupappointment.Value || saRecord.cvt_Type.Value))
            {
                if (saRecord.StateCode != null && saRecord.StateCode.Value == ServiceAppointmentState.Canceled)
                {
                    if (filter)
                    {
                        var orgName = PluginExecutionContext.OrganizationName;
                        var user = OrganizationService.Retrieve(SystemUser.EntityLogicalName, PluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();

                        using (var context = new Xrm(OrganizationService))
                        {
                            Logger.WriteDebugMessage("Starting GetAndSetIntegrationSettings");
                            GetAndSetIntegrationSettings(context);
                            Logger.WriteDebugMessage("Finished GetAndSetIntegrationSettings");

                            var anyFailures = RunCancelVistaBooking(saRecord, orgName, user, VistaTypeFacility);

                            if (!anyFailures)
                            {
                                Success = true;
                            }
                            else //something went wrong
                            {
                                Success = false;
                                IntegrationPluginHelpers.ChangeEntityStatus(OrganizationService, saRecord, (int)serviceappointment_statuscode.CancelFailure);
                            }
                        }
                    }
                    else // VIA integrations are turned off
                    {
                        Logger.WriteDebugMessage("Vista Integration (through VIA/HealthShare) is turned off system wide, skipping Vista Cancel integration");
                        Success = true;
                    }
                }
                else
                {
                    Logger.WriteDebugMessage("Vista Cancel Integration skipped - Appointment not in 'Canceled' state");
                    Success = true;
                }
            }
            else
            {
                Logger.WriteDebugMessage("Cancel Request invalid for Clinic Based Group Appointments. ");
                Success = true;
            }

            LogExit(1);

        }

        private void GetAndSetIntegrationSettings(Xrm context)
        {
            LogEntry();
            ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "HsMakeCancelOutboundUri");
            LogExit(1);
        }

        private bool RunCancelVistaBooking(DataModel.ServiceAppointment sa, string orgName, SystemUser user, bool VistaTypeFacility)
        {
            LogEntry();

            var anyfailures = false;
            var veterans = GetPatients(sa);

            var request = new TmpHealthShareMakeCancelOutboundRequestMessage
            {
                Patients = veterans,
                ServiceAppointmentId = PrimaryEntity.Id,
                LogRequest = true,
                OrganizationName = orgName,
                UserId = user.Id,
                VisitStatus = VistaStatus.CANCELED.ToString()
            };

            var vimtRequest = Serialization.DataContractSerialize(request);
            TmpHealthShareMakeCancelOutboundResponseMessage response = null;

            try
            {
                if (VistaTypeFacility)
                {
                    Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
                    Logger.WriteDebugMessage($"Sending HealthShare Vista Cancel Appointment Request Message to VIMT: {vimtRequest}.");

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

                    Logger.WriteDebugMessage($"Finished Sending HealthShare Vista Cancel Appointment Request Message to VIMT for Service Appointment with Id: {PrimaryEntity.Id}");
                    var status = ProcessVistaResponse(response, sa);

                    Logger.WriteDebugMessage($"SA status for HealthShare Cancel is {sa.StatusCode.Value}");

                    if (status != sa.StatusCode.Value) anyfailures = true;
                }
                else
                {
                    Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control Passed to Logic App for Cerner Processing.");

                    using (var context = new Xrm(OrganizationService))
                    {
                        var cernerResponse = CernerHelper.FireLogicApp(PrimaryEntity.ToEntityReference(), context, OrganizationService, Logger, veterans, "Cancel|ServiceAppointment");
                        var status = ProcessCernerResponse(cernerResponse, sa);

                        if(cernerResponse.ExceptionOccured) 
                            anyfailures = true;
                    }
                }
              
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Cancel Vista Appointment", errorMessage, vimtRequest,
                    typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                    MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse,
                    response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                Logger.WriteToFile(errorMessage);
                anyfailures = true;
            }

            Logger.WriteDebugMessage($"For HealthShare Cancel anyfailures is set to {anyfailures}");

            LogExit(1);

            return anyfailures;
        }

        private List<Guid> GetPatients(DataModel.ServiceAppointment sa)
        {
            LogEntry();

            List<Guid> patients;
            using (var srv = new Xrm(OrganizationService))
            {
                var vistaBookings = srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_ServiceActivity.Id == sa.Id && vir.cvt_VistAStatus != VistaStatus.CANCELED.ToString() && vir.cvt_VistAStatus != VistaStatus.FAILED_TO_SCHEDULE.ToString() && vir.cvt_Veteran != null).ToList();
                patients = vistaBookings.Select(vir => vir.cvt_Veteran.Id).Distinct().ToList();
            }

            LogExit(1);

            return patients;
        }


        private int ProcessCernerResponse(TmpCernerOutboundResponseMessage response, DataModel.ServiceAppointment sa)
        {
            LogEntry();

            if (response == null) return (int)serviceappointment_statuscode.CancelFailure;

            var errMessage = string.Empty;
            var exceptionOccured = false;
            
            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;

            IntegrationPluginHelpers.CreateCernerIntegrationResult("Cancel Cerner Appointment", response.ExceptionOccured, errorMessage,
                response.RequestMessage, "", response.ResponseMessage,
                typeof(TmpCernerOutboundResponseMessage).FullName, typeof(TmpCernerOutboundResponseMessage).FullName,
                MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, 0,
                0, response.MessageProcessingTime, controlId: response.ControlId);

            errMessage += errorMessage;
            exceptionOccured = !string.IsNullOrEmpty(errMessage);
            
            if (exceptionOccured) Logger.WriteDebugMessage($"The was an error in the response for Cerner Cancel: {errMessage}");

            var status = (serviceappointment_statuscode)sa.StatusCode.Value;
            if (response.ExceptionOccured || exceptionOccured) status = serviceappointment_statuscode.CancelFailure;

            Logger.WriteDebugMessage($"Status returned after Cerner Cancel process response is {status}");

            LogExit(1);

            return (int)status;
        }
        private int ProcessVistaResponse(TmpHealthShareMakeCancelOutboundResponseMessage response, DataModel.ServiceAppointment sa)
        {
            LogEntry();

            if (response == null) return (int)serviceappointment_statuscode.CancelFailure;

            var errMessage = string.Empty;
            var exceptionOccured = false;

            foreach (var patientIntegrationResultInformation in response.PatientIntegrationResultInformation)
            {
                var errorMessage = patientIntegrationResultInformation.ExceptionOccured ? patientIntegrationResultInformation.ExceptionMessage : string.Empty;

                IntegrationPluginHelpers.CreateIntegrationResult("Cancel Vista Appointment", patientIntegrationResultInformation.ExceptionOccured, errorMessage,
                    patientIntegrationResultInformation.VimtRequest, response.SerializedInstance, patientIntegrationResultInformation.VimtResponse,
                    typeof(TmpHealthShareMakeCancelOutboundRequestMessage).FullName, typeof(TmpHealthShareMakeCancelOutboundResponseMessage).FullName,
                    MessageRegistry.HealthShareMakeCancelOutboundRequestMessage, PrimaryEntity.Id, OrganizationService, response.VimtLagMs,
                    patientIntegrationResultInformation.EcProcessingMs, response.VimtProcessingMs, patientIntegrationResultInformation);

                errMessage += errorMessage;
                exceptionOccured = !string.IsNullOrEmpty(errMessage);
            }

            if (exceptionOccured) Logger.WriteDebugMessage($"The was an error in the response for HealthShare Cancel: {errMessage}");

            var status = (serviceappointment_statuscode)sa.StatusCode.Value;
            if (response.ExceptionOccured || exceptionOccured) status = serviceappointment_statuscode.CancelFailure;

            Logger.WriteDebugMessage($"Status returned after HealthShare Cancel process response is {status}");

            LogExit(1);

            return (int)status;
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
