using System;
using System.Linq;
using System.ServiceModel;
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
//hi Rusty

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentVmrCancelPostStageRunner : AILogicBase
    {
        public ServiceAppointmentVmrCancelPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField => "cvt_serviceactivityplugin";

        private ApiIntegrationSettings ApiIntegrationSettings { get; set; }

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
            Success = false;

            bool VistaTypeFacility = true;

            try
            {
                using (var context = new Xrm(OrganizationService))
                {

                    ApiIntegrationSettings = IntegrationPluginHelpers.GetApiSettings(context, "VmrDeleteUri");

                    var settings = context.mcs_settingSet.FirstOrDefault(x => x.mcs_name == "Active Settings");
                    if (settings == null) throw new InvalidPluginExecutionException("Active Settings Cannot be Null");

                    var serviceAppointment = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == PrimaryEntity.Id);

                    if (serviceAppointment == null) throw new InvalidPluginExecutionException("Service Appointment cannot be null.");

                    if (serviceAppointment.StateCode != null && serviceAppointment.StateCode.Value != ServiceAppointmentState.Canceled)
                    {
                        //Logger.WriteDebugMessage("Service Activity Not in Canceled status, exiting Cancel Integrations");
                        Trace("Service Activity Not in Canceled status, exiting Cancel Integrations.", LogLevel.Debug);
                        return;
                    }

                    //Logger.WriteDebugMessage("Checking Facility Type!");
                    Trace("Checking Facility Type!", LogLevel.Debug);

                    if (PluginExecutionContext.SharedVariables.Contains("isVistaFacility"))
                    {
                        VistaTypeFacility = (bool)PluginExecutionContext.SharedVariables["isVistaFacility"];
                        //Logger.WriteDebugMessage($"INFO: isVistaFacility exists in shared variables; set to: " + VistaTypeFacility.ToString());
                        Trace($"INFO: isVistaFacility exists in shared variables; set to: {VistaTypeFacility}", LogLevel.Debug);
                    }
                    else
                    {
                        VistaTypeFacility = CernerHelper.CheckIfRelatedCernerFacility(serviceAppointment, context, pluginLogger);
                        PluginExecutionContext.SharedVariables.Add("isVistaFacility", VistaTypeFacility);
                    }

                    if (VistaTypeFacility)
                    {
                        //Logger.WriteDebugMessage("The Current Facility IS a Vista Facility.  Continue Processing.");
                        Trace("The Current Facility IS a Vista Facility.  Continue Processing.", LogLevel.Debug);
                        // Ensure this is NOT a Telephone Call VVC Service Appointment.
                        if (serviceAppointment.cvt_TelephoneCall.HasValue && serviceAppointment.cvt_TelephoneCall.Value)
                        {
                            //Logger.WriteDebugMessage("This is a Telephone Call Appointment, hence skipping VMR cancel");
                            Trace("This is a Telephone Call Appointment, hence skipping VMR cancel.", LogLevel.Debug);
                            Success = true;
                            return;
                        }

                        if (settings.cvt_accenturevyopta != null && settings.cvt_accenturevyopta.Value)
                        {
                            // Call Service to cancel the Virtual Meeting Room.
                            if (serviceAppointment.cvt_Type != null && serviceAppointment.cvt_Type.Value)
                            {
                                ////Figure out if the patient has a COTS or CVT Tablet then we have to bypass Cancel VMR
                                var patientAP = serviceAppointment.Customers.FirstOrDefault();
                                if (patientAP != null && patientAP.PartyId != null)
                                {
                                    //Logger.WriteDebugMessage(serviceAppointment.Customers.ToList().Count().ToString() + " patients " + patientAP.PartyId.Name.ToString());
                                    Trace(serviceAppointment.Customers.ToList().Count().ToString() + " patients " + patientAP.PartyId.Name.ToString(), LogLevel.Debug);
                                    var patient = (Contact)OrganizationService.Retrieve(Contact.EntityLogicalName, patientAP.PartyId.Id, new ColumnSet(true));

                                    if (patient != null && patient.cvt_TabletType != null)
                                    {
                                        if (patient.cvt_TabletType.Value == (int)Contactcvt_TabletType.SIPDevice)
                                        {
                                            //Logger.WriteDebugMessage("Patient has SIP Device (CVT or COTS Tablet). Bypassed VMR Cancel.");
                                            Trace("Patient has SIP Device (CVT or COTS Tablet). Bypassed VMR Cancel.", LogLevel.Debug);
                                            Success = true;
                                            return;
                                        }
                                        else if (patient.cvt_TabletType.Value == (int)Contactcvt_TabletType.VALoanedDevice && patient.DoNotEMail.Value)
                                        {
                                            //Logger.WriteDebugMessage("Patient has a static VMR. Bypassed VMR Cancel.");
                                            Trace("Patient has a static VMR. Bypassed VMR Cancel.", LogLevel.Debug);
                                            Success = true;
                                            return;
                                        }
                                    }
                                }
                                //
                                var virtualMeetingRoomDeleteResponseMessage = CancelAndSendVirtualMeetingRoom(serviceAppointment.Id, PluginExecutionContext.InitiatingUserId, PluginExecutionContext.OrganizationName);
                                if (virtualMeetingRoomDeleteResponseMessage == null) return;
                                ProcessVirtualMeetingRoomCreateResponseMessage(virtualMeetingRoomDeleteResponseMessage);

                                // If we fail canceling a virtual meeting room do not call the video visit service.
                                if (virtualMeetingRoomDeleteResponseMessage.ExceptionOccured) return;
                            }
                        }
                        else
                        {
                            //Logger.WriteDebugMessage("Bypassed VMR Cancel");
                            Trace("Bypassed VMR Cancel.", LogLevel.Debug);
                        }
                    }
                    else
                    {
                        //Logger.WriteDebugMessage("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as VMR is not applicable to Cerner integration.");
                        Trace("The Current Facility IS NOT a Vista Facility. Vista Processing halted; Control NOT Passed to Logic App for Cerner Processing as VMR is not applicable to Cerner integration.", LogLevel.Debug);
                        Success = true;
                    }
                }
                Success = true;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                //Logger.WriteToFile(ex.Message);
                Trace(ex.Message, LogLevel.Error);
                throw new InvalidPluginExecutionException(string.Format("ERROR in ServiceAppointmentCancelPostStageRunner: {0}", IntegrationPluginHelpers.BuildErrorMessage(ex)));
            }
            catch (InvalidPluginExecutionException ex)
            {
                //Logger.WriteDebugMessage(ex.Message);
                Trace(ex.Message, LogLevel.Error);
                throw;
            }
            catch (Exception ex)
            {
                //Logger.WriteToFile(CvtHelper.BuildExceptionMessage(ex));
                Trace(CvtHelper.BuildExceptionMessage(ex), LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Create an instance of the Virtual Meeting Room request and send to VIMT.
        /// </summary>
        /// <returns>VirtualMeetingRoomDeleteResponseMessage.</returns>
        private VirtualMeetingRoomDeleteResponseMessage CancelAndSendVirtualMeetingRoom(Guid appointmentId, Guid userId, string organizationName)
        {
            var request = new VirtualMeetingRoomDeleteRequestMessage
            {
                LogRequest = true,
                UserId = userId,
                OrganizationName = organizationName,
                AppointmentId = appointmentId,
                MiscData = string.Empty
            };

            var vimtRequest = Serialization.DataContractSerialize(request);
            VirtualMeetingRoomDeleteResponseMessage response = new VirtualMeetingRoomDeleteResponseMessage();
            try
            {
                //Logger.WriteDebugMessage("Begin: Sending Cancel to VMR");
                Trace("Begin: Sending Cancel to VMR.", LogLevel.Debug);

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

                //var context = new Xrm(OrganizationService);
                response.ExceptionOccured = false;
                response.ExceptionMessage = string.Empty;
                response.SerializedInstance = string.Empty;
                response.VimtRequest = string.Empty;
                // response = RestPoster.Post<VirtualMeetingRoomDeleteRequestMessage, VirtualMeetingRoomDeleteResponseMessage>("VMR Delete", baseUrl, uri, request, resource, appId, secret, authority,
                //tenantId, subscriptionId, isProdApi, subscriptionIdEast, subscriptionIdSouth, out int lag);
                //response.VimtLagMs = lag;

                //Logger.WriteDebugMessage("End: Sending Cancel to VMR");
                Trace("End: Sending Cancel to VMR", LogLevel.Debug);
                return response;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Cancel Virtual Meeting Room", errorMessage, vimtRequest, typeof(VirtualMeetingRoomDeleteRequestMessage).FullName, typeof(VirtualMeetingRoomDeleteResponseMessage).FullName, MessageRegistry.VirtualMeetingRoomDeleteRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs, true);
                //Logger.WriteToFile(errorMessage);
                Trace(errorMessage, LogLevel.Error);

                return null;
            }
        }

        /// <summary>
        /// Create an Integration Result.
        /// </summary>
        /// <param name="virtualMeetingRoomDeleteResponseMessage">Virtual Meeting Room Delete Response Message.</param>
        private void ProcessVirtualMeetingRoomCreateResponseMessage(VirtualMeetingRoomDeleteResponseMessage virtualMeetingRoomDeleteResponseMessage)
        {
            var errorMessage = virtualMeetingRoomDeleteResponseMessage.ExceptionOccured ? virtualMeetingRoomDeleteResponseMessage.ExceptionMessage : null;
            IntegrationPluginHelpers.CreateIntegrationResult("Cancel Virtual Meeting Room", virtualMeetingRoomDeleteResponseMessage.ExceptionOccured, errorMessage,
                virtualMeetingRoomDeleteResponseMessage.VimtRequest, virtualMeetingRoomDeleteResponseMessage.SerializedInstance, virtualMeetingRoomDeleteResponseMessage.VimtResponse,
                typeof(VirtualMeetingRoomDeleteRequestMessage).FullName, typeof(VirtualMeetingRoomDeleteResponseMessage).FullName, MessageRegistry.VirtualMeetingRoomDeleteRequestMessage,
                PrimaryEntity.Id, OrganizationService, virtualMeetingRoomDeleteResponseMessage.VimtLagMs, virtualMeetingRoomDeleteResponseMessage.EcProcessingMs, virtualMeetingRoomDeleteResponseMessage.VimtProcessingMs);
        }
    }
}
