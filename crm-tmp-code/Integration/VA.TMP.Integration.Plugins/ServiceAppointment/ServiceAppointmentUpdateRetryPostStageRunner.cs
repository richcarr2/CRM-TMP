using MCSPlugins;
using mcsScheduling;
using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.Xrm;
using VRMRest;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    /// <summary>
    ///  CRM Plugin Runner class to handle creating a ServiceAppointment.
    /// </summary>
    public class ServiceAppointmentUpdateRetryPostStageRunner : PluginRunner
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public ServiceAppointmentUpdateRetryPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }

        /// <summary>
        /// Gets or sets the VIMT URL.
        /// </summary>
        private string VimtUrl { get; set; }

        /// <summary>
        /// Executes the plugin runner.
        /// </summary>
        public override void Execute()
        {
            try
            {
                var serviceAppointmentEntity = PrimaryEntity.ToEntity<mcsScheduling.ServiceAppointment>();
                using (var context = new mcsScheduling.Xrm(OrganizationService))
                {
                    // Retrieve the Service Appointment and ensure it is in the correct state before proceeding.
                    var serviceAppointment = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == serviceAppointmentEntity.Id);
                    if (serviceAppointment == null) throw new InvalidPluginExecutionException("Service Appointment cannot be null.");
                    // Ensure this is a Home/Mobile Service Appointment.
                    if (serviceAppointment.cvt_Type == null || !serviceAppointment.cvt_Type.Value) return;
               
                    VimtUrl = context.mcs_integrationsettingSet.First(x => x.mcs_name == "VIMT URL").mcs_value;

                    if (serviceAppointment.StateCode == null || serviceAppointment.StateCode != ServiceAppointmentState.Scheduled) throw new InvalidPluginExecutionException("Service Appointment is not in Scheduled Status.");

                    var virtualMeetingRoomCreateResponseMessage = CreateAndSendVirtualMeetingRoom(serviceAppointment, PluginExecutionContext.InitiatingUserId, PluginExecutionContext.OrganizationName);
                    ProcessVirtualMeetingRoomCreateResponseMessage(virtualMeetingRoomCreateResponseMessage);

                    // If we fail reserving a virtual meeting room do not call the video visit service.
                    if (virtualMeetingRoomCreateResponseMessage.ExceptionOccured)
                    {
                        UpdateServiceAppointmentToFailStatus();
                        return;
                    }
                    
                    var videoVisitCreateResponseMessage = CreateAndSendVideoVisitService(serviceAppointment.Id, PluginExecutionContext.InitiatingUserId, PluginExecutionContext.OrganizationName);
                    ProcessVideoVisitCreateResponseMessage(videoVisitCreateResponseMessage);

                    if (videoVisitCreateResponseMessage.ExceptionOccured)
                    {
                        UpdateServiceAppointmentToFailStatus();
                        return;
                    }
                    //If both VMR & VVC are successful, update the Service Appt to Scheduled. 
                    else
                    {
                        UpdateServiceAppointmentToScheduledStatus();
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(ex.Message);
                throw new InvalidPluginExecutionException(string.Format("ERROR in ServiceAppointmentCreatePostStageRunner: {0}", CvtHelper.BuildErrorMessage(ex)));
            }
            catch (InvalidPluginExecutionException ex)
            {
                Logger.WriteDebugMessage(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Create an instance of the Virtual Meeting Room request and send to VIMT.
        /// </summary>
        /// <returns>VirtualMeetingRoomCreateResponseMessage.</returns>
        private VirtualMeetingRoomCreateResponseMessage CreateAndSendVirtualMeetingRoom(mcsScheduling.ServiceAppointment serviceAppointment, Guid userId, string organizationName)
        {
            // Get the PatientId and ProviderId. Guard verbosely here against null references/objects.
            var bookedPatient = serviceAppointment.Customers.FirstOrDefault(p => p.PartyId.LogicalName == "contact");
            if (bookedPatient == null) throw new InvalidPluginExecutionException("Patient cannot be null.");

            if (bookedPatient.PartyId == null) throw new InvalidPluginExecutionException("The Patient PartyId cannot be null.");
            var patientId = bookedPatient.PartyId.Id;

            var bookedSysUser = serviceAppointment.Resources.FirstOrDefault(r => r.PartyId.LogicalName == "systemuser");
            if (bookedSysUser == null) throw new InvalidPluginExecutionException("Provider cannot be null.");

            if (bookedSysUser.PartyId == null) throw new InvalidPluginExecutionException("The Provider PartyId cannot be null.");
            var providerId = bookedSysUser.PartyId.Id;

            var request = new VirtualMeetingRoomCreateRequestMessage
            {
                LogRequest = true,
                UserId = userId,
                OrganizationName = organizationName,
                AppointmentId = serviceAppointment.Id,
                PatientId = patientId,
                ProviderId = providerId,
                MiscData = string.Empty
            };

            return Utility.SendReceive<VirtualMeetingRoomCreateResponseMessage>(new Uri(VimtUrl), MessageRegistry.VirtualMeetingRoomCreateRequestMessage, request, null);
        }

        /// <summary>
        /// Create an instance of the Video Vist Service request and send to VIMT.
        /// </summary>
        /// <returns>VideoVisitCreateResponseMessage.</returns>
        private VideoVisitCreateResponseMessage CreateAndSendVideoVisitService(Guid appointmentId, Guid userId, string organizationName)
        {
            var request = new VideoVisitCreateRequestMessage
            {
                LogRequest = true,
                UserId = userId,
                OrganizationName = organizationName,
                AppointmentId = appointmentId
            };

            return Utility.SendReceive<VideoVisitCreateResponseMessage>(new Uri(VimtUrl), MessageRegistry.VideoVisitCreateRequestMessage, request, null);
        }

        /// <summary>
        /// Update the Service Activity and create an Integration Result.
        /// </summary>
        /// <param name="virtualMeetingRoomCreateResponseMessage">Virtual Meeting Room Create Response Message.</param>
        private void ProcessVirtualMeetingRoomCreateResponseMessage(VirtualMeetingRoomCreateResponseMessage virtualMeetingRoomCreateResponseMessage)
        {
            var integrationResult = new mcs_integrationresult
            {
                mcs_name = "Create Virtual Meeting Room",
                mcs_error = virtualMeetingRoomCreateResponseMessage.ExceptionOccured ? virtualMeetingRoomCreateResponseMessage.ExceptionMessage : null,
                mcs_vimtrequest = virtualMeetingRoomCreateResponseMessage.VimtRequest,
                mcs_integrationrequest = virtualMeetingRoomCreateResponseMessage.SerializedInstance,
                mcs_vimtresponse = virtualMeetingRoomCreateResponseMessage.VimtResponse,
                mcs_status = virtualMeetingRoomCreateResponseMessage.ExceptionOccured ? new OptionSetValue((int)mcs_integrationresultmcs_status.Error) : new OptionSetValue((int)mcs_integrationresultmcs_status.Complete),
                mcs_VimtRequestMessageType = typeof(VirtualMeetingRoomCreateRequestMessage).AssemblyQualifiedName,
                mcs_VimtResponseMessageType = typeof(VirtualMeetingRoomCreateResponseMessage).AssemblyQualifiedName,
                mcs_VimtMessageRegistryName = MessageRegistry.VirtualMeetingRoomCreateRequestMessage,
                mcs_serviceappointmentid = new EntityReference(mcsScheduling.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id)
            };
            OrganizationService.Create(integrationResult);

            if (virtualMeetingRoomCreateResponseMessage.ExceptionOccured) return;
            var serviceAppointment = new mcsScheduling.ServiceAppointment
            {
                Id = PrimaryEntity.Id,
                mcs_meetingroomname = virtualMeetingRoomCreateResponseMessage.MeetingRoomName,
                mcs_PatientUrl = virtualMeetingRoomCreateResponseMessage.PatientUrl,
                mcs_providerurl = virtualMeetingRoomCreateResponseMessage.ProviderUrl,
                mcs_patientpin = virtualMeetingRoomCreateResponseMessage.PatientPin,
                mcs_providerpin = virtualMeetingRoomCreateResponseMessage.ProviderPin,
                mcs_dialingalias = virtualMeetingRoomCreateResponseMessage.DialingAlias,
                mcs_miscdata = virtualMeetingRoomCreateResponseMessage.MiscData
            };
            OrganizationService.Update(serviceAppointment);
        }

        /// <summary>
        /// Create an Integration Result.
        /// </summary>
        /// <param name="videoVisitCreateResponseMessage">Video Visit Create Response Message.</param>
        private void ProcessVideoVisitCreateResponseMessage(VideoVisitCreateResponseMessage videoVisitCreateResponseMessage)
        {
            var integrationResult = new mcs_integrationresult
            {
                mcs_name = "Create Video Visit",
                mcs_error = videoVisitCreateResponseMessage.ExceptionOccured ? videoVisitCreateResponseMessage.ExceptionMessage : null,
                mcs_vimtrequest = videoVisitCreateResponseMessage.VimtRequest,
                mcs_integrationrequest = videoVisitCreateResponseMessage.SerializedInstance,
                mcs_vimtresponse = videoVisitCreateResponseMessage.VimtResponse,
                mcs_status = videoVisitCreateResponseMessage.ExceptionOccured ? new OptionSetValue((int)mcs_integrationresultmcs_status.Error) : new OptionSetValue((int)mcs_integrationresultmcs_status.Complete),
                mcs_VimtRequestMessageType = typeof(VideoVisitCreateRequestMessage).AssemblyQualifiedName,
                mcs_VimtResponseMessageType = typeof(VideoVisitCreateResponseMessage).AssemblyQualifiedName,
                mcs_VimtMessageRegistryName = MessageRegistry.VideoVisitCreateRequestMessage,
                mcs_serviceappointmentid = new EntityReference(mcsScheduling.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id)
            };
            OrganizationService.Create(integrationResult);
        }

        /// <summary>
        /// Update Service Appointment to Fail Sttus
        /// </summary>
        private void UpdateServiceAppointmentToFailStatus()
        {
            var serviceAppointment = new mcsScheduling.ServiceAppointment
            {
                Id = PrimaryEntity.Id,
                StatusCode = new OptionSetValue((int)serviceappointment_statuscode.InterfaceVIMTFailure)
            };
            OrganizationService.Update(serviceAppointment);
        }
        private void UpdateServiceAppointmentToScheduledStatus()
        {
            var serviceAppointment = new mcsScheduling.ServiceAppointment
            {
                Id = PrimaryEntity.Id,
                StatusCode = new OptionSetValue((int)serviceappointment_statuscode.ReservedScheduled)
            };
            OrganizationService.Update(serviceAppointment);
        }
    }
}