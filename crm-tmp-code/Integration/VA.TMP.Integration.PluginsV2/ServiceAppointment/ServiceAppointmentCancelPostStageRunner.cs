using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using MCS.ApplicationInsights;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.Cerner;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Plugins.Helpers;
using VA.TMP.Integration.Plugins.Messages;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    /// <summary>
    ///  CRM Plugin Runner class to handle canceling a ServiceAppointment.
    /// </summary>
    public class ServiceAppointmentCancelPostStageRunner : AILogicBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public ServiceAppointmentCancelPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        public bool Success { get; set; }

        ///// <summary>
        ///// Gets the primary entity.
        ///// </summary>
        ///// <returns>Returns the primary entity.</returns>
        //public override Entity GetPrimaryEntity()
        //{
        //    var primaryReference = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];

        //    return new Entity(primaryReference.LogicalName) { Id = primaryReference.Id };
        //}

        /// <summary>
        /// Executes the plugin runner.
        /// </summary>
        public override void ExecuteLogic()
        {
            LogEntry();

            Success = false;
            bool VistaTypeFacility = true;
            try
            {
                using (var context = new Xrm(OrganizationService))
                {
                    var settings = context.mcs_settingSet.FirstOrDefault(x => x.mcs_name == "Active Settings");

                    if (settings == null)
                    {
                        LogExit(1);
                        throw new InvalidPluginExecutionException("Active Settings Cannot be Null");
                    }

                    var serviceAppointment = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == PrimaryEntity.Id);

                    if (serviceAppointment == null) throw new InvalidPluginExecutionException("Service Appointment cannot be null.");

                    if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                    {
                        VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVista"];
                        //Logger.WriteDebugMessage($"INFO: isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                        Trace($"INFO: isVistaFacility exists in shared variables; set to: {VistaTypeFacility}", LogLevel.Debug, true);
                    }
                    else
                    {
                        VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(serviceAppointment, context, Logger);
                        PluginExecutionContext.SharedVariables.Add("isVistaFacility", VistaTypeFacility);
                    }

                    if (serviceAppointment.StateCode.Value != ServiceAppointmentState.Canceled)
                    {
                        //Logger.WriteDebugMessage("Service Activity Not in Canceled status, exiting Cancel Integrations");
                        Trace("Service Activity Not in Canceled status, exiting Cancel Integrations", LogLevel.Debug, true);
                        LogExit(2);
                        return;
                    }

                    // Ensure this is NOT a Telephone Call VVC Service Appointment.
                    if (serviceAppointment.cvt_TelephoneCall.HasValue && serviceAppointment.cvt_TelephoneCall.Value)
                    {
                        //Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VVS cancel");
                        Trace("This is a Telephone Call Appointment, hence skipping VVS cancel", LogLevel.Debug, true);
                        Success = true;
                        LogExit(3);
                        return;
                    }

                    // Call Service to cancel the Video Visit.
                    if (!(!serviceAppointment.cvt_Type.Value && serviceAppointment.mcs_groupappointment.Value) && VistaPluginHelpers.RunVvs(serviceAppointment, context, Logger, PluginExecutionContext))
                    {
                        if (VistaTypeFacility)
                        {
                            var videoVisitDeleteResponseMessage = VistaPluginHelpers.CancelAndSendVideoVisitServiceSa(serviceAppointment, PluginExecutionContext.InitiatingUserId, PluginExecutionContext.OrganizationName, OrganizationService, Logger);
                            if (videoVisitDeleteResponseMessage == null)
                            {
                                LogExit(4);
                                return;
                            }

                            ProcessVideoVisitDeleteResponseMessage(videoVisitDeleteResponseMessage);
                        }
                        else
                        {
                            //Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as Canceling VVS is not applicable to Cerner integration.");
                            Trace("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as Canceling VVS is not applicable to Cerner integration.", LogLevel.Debug, true);
                        }
                    }
                    else
                    {
                        //Logger.WriteDebugMessage("Bypassed VVS Cancel. Either the VVS Switch is OFF or the SA is a CVT Group appointment (Cancellation are triggered from individual RRs in case of group)");
                        Trace("Bypassed VVS Cancel. Either the VVS Switch is OFF or the SA is a CVT Group appointment (Cancellation are triggered from individual RRs in case of group)", LogLevel.Debug, true);
                    }

                    Success = true;
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error, true);
                LogExit(5);
                throw new InvalidPluginExecutionException($"ERROR in ServiceAppointmentCancelPostStageRunner: {IntegrationPluginHelpers.BuildErrorMessage(ex)}");
            }
            catch (InvalidPluginExecutionException ex)
            {
                //Logger.WriteDebugMessage(ex.Message);
                Trace(ex.Message, LogLevel.Error, true);
                LogExit(6);
                throw;
            }
            catch (Exception ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error, true);
                LogExit(7);
                throw;
            }

            LogExit(8);
        }

        /// <summary>
        /// Create an Integration Result.
        /// </summary>
        /// <param name="videoVisitDeleteResponseMessage">Video Visit Delete Response Message.</param>
        private void ProcessVideoVisitDeleteResponseMessage(VideoVisitDeleteResponseMessage videoVisitDeleteResponseMessage)
        {
            LogEntry();

            Logger.WriteDebugMessage("Processing VVS Cancel Response");
            var errorMessage = videoVisitDeleteResponseMessage.ExceptionOccured ? videoVisitDeleteResponseMessage.ExceptionMessage : string.Empty;
            IntegrationPluginHelpers.CreateIntegrationResult("Cancel VVS", videoVisitDeleteResponseMessage.ExceptionOccured, errorMessage,
                videoVisitDeleteResponseMessage.VimtRequest, videoVisitDeleteResponseMessage.SerializedInstance, videoVisitDeleteResponseMessage.VimtResponse,
                typeof(VideoVisitDeleteRequestMessage).FullName, typeof(VideoVisitDeleteResponseMessage).FullName, MessageRegistry.VideoVisitDeleteRequestMessage,
                PrimaryEntity.Id, OrganizationService, videoVisitDeleteResponseMessage.VimtLagMs, videoVisitDeleteResponseMessage.EcProcessingMs, videoVisitDeleteResponseMessage.VimtProcessingMs, null, false);

            Logger.WriteDebugMessage("Finished Processing VVS Cancel Response");

            LogExit(1);
        }

        private void ProcessCernerVideoVisitDeleteResponseMessage(TmpCernerOutboundResponseMessage response)
        {
            LogEntry();

            Logger.WriteDebugMessage("Processing Cerner VVS Cancel Response");

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            IntegrationPluginHelpers.CreateIntegrationResult("Cancel Cerner Service Appointment", response.ExceptionOccured, errorMessage,
                response.RequestMessage, response.RequestMessage, response.ResponseMessage,
                typeof(VideoVisitDeleteRequestMessage).FullName, typeof(TmpCernerOutboundResponseMessage).FullName, MessageRegistry.CernerOutboundResponseMessage,
                PrimaryEntity.Id, OrganizationService, 0, 0, response.MessageProcessingTime, null, false, controlId: response.ControlId);

            Logger.WriteDebugMessage("Finished Cerner VVS Cancel Response");

            LogExit(1);
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
