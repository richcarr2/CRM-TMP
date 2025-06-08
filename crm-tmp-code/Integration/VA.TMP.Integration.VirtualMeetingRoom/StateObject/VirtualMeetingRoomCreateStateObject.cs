using System;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;

namespace VA.TMP.Integration.VirtualMeetingRoom.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class VirtualMeetingRoomCreateStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="virtualMeetingRoomCreateRequestMessage">VirtualMeetingRoomCreateRequestMessage instance.</param>
        public VirtualMeetingRoomCreateStateObject(VirtualMeetingRoomCreateRequestMessage virtualMeetingRoomCreateRequestMessage)
        {
            OrganizationName = virtualMeetingRoomCreateRequestMessage.OrganizationName;
            UserId = virtualMeetingRoomCreateRequestMessage.UserId;
            LogRequest = virtualMeetingRoomCreateRequestMessage.LogRequest;
            AppointmentId = virtualMeetingRoomCreateRequestMessage.AppointmentId;
            PatientId = virtualMeetingRoomCreateRequestMessage.PatientId;
            ProviderId = virtualMeetingRoomCreateRequestMessage.ProviderId;
        }

        /// <summary>
        /// Gets or sets the CRM organization name.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the UserId.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets whether to log the request.
        /// </summary>
        public bool LogRequest { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Gets or sets the Appointment Id.
        /// </summary>
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets the Virtual Meeting Room Digit Length.
        /// </summary>
        public int VirtualMeetingRoomDigitLength { get; set; }

        /// <summary>
        /// Gets or sets the Virtual Meeting Room Prefix.
        /// </summary>
        public string VirtualMeetingRoomPrefix { get; set; }

        /// <summary>
        /// Gets or sets the Virtual Meeting Room Suffix.
        /// </summary>
        public string VirtualMeetingRoomSuffix { get; set; }

        /// <summary>
        /// Gets or sets the Patient PIN length.
        /// </summary>
        public int PatientPinLength { get; set; }

        /// <summary>
        /// Gets or sets the Provider PIN length.
        /// </summary>
        public int ProviderPinLength { get; set; }

        /// <summary>
        /// Gets or sets the Schema Path.
        /// </summary>
        public string SchemaPath { get; set; }

        /// <summary>
        /// Gets or sets the Provider Virtual Meeting Room Format URL.
        /// </summary>
        public string ProviderVmrFormatUrl { get; set; }

        /// <summary>
        /// Gets or sets the Patient Virtual Meeting Room Format URL.
        /// </summary>
        public string PatientVmrFormatUrl { get; set; }

        /// <summary>
        /// Gets or sets the Virtual Meeting Room Base URL.
        /// </summary>
        public string VmrBaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the Virtual Meeting Room Base URL Extension.
        /// </summary>
        public string VmrBaseUrlExtension { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public bool UseFakeResponse { get; set; }

        /// <summary>
        /// Gets or sets the Vyopta Guest Url Prefix.
        /// </summary>
        public string VyoptaGuestUrlPrefix { get; set; }

        /// <summary>
        ///  Gets or sets the Vyopta Host Url Prefix.
        /// </summary>
        public string VyoptaHostUrlPrefix { get; set; }

        /// <summary>
        /// Gets or sets the Patient Id.
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Gets or sets the Provider Id.
        /// </summary>
        public Guid ProviderId { get; set; }

        /// <summary>
        /// Gets or sets the Service Appointment.
        /// </summary>
        public ServiceAppointment ServiceAppointment { get; set; }

        /// <summary>
        /// Gets or Sets the Meeting Room Name. 
        /// </summary>
        public string MeetingRoomName { get; set; }

        /// <summary>
        /// Get or Sets the Patient Pin. 
        /// </summary>
        public string PatientPin { get; set; }

        /// <summary>
        /// Get or Sets the Provider Pin. 
        /// </summary>
        public string ProviderPin { get; set; }

        /// <summary>
        /// Gets or Sets the Appt Start Date. 
        /// </summary>
        public DateTime AppointmentStartDate { get; set; }

        /// <summary>
        /// Gets or Sets the Appt End Date. 
        /// </summary>
        public DateTime AppointmentEndDate { get; set; }

        /// <summary>
        /// Gets or Sets the Misc Flag for Request. 
        /// </summary>
        public string MiscDataForRequest { get; set; }

        /// <summary>
        /// Gets or Sets a Virtual Meeting Room instance.
        /// </summary>
        public Schema.VirtualMeetingRoom.VirtualMeetingRoomType VirtualMeetingRoom { get; set; }

        /// <summary>
        /// Gets or Sets an instance of the VirtualMeetingRoom class as a string.
        /// </summary>
        public string SerializedVirtualMeetingRoom { get; set; }

        /// <summary>
        /// Gets or Sets a Patient URL.
        /// </summary>
        public string PatientUrl { get; set; }

        /// <summary>
        /// Gets or Sets a Provider URL.
        /// </summary>
        public string ProviderUrl { get; set; }

        /// <summary>
        /// Gets or Sets the CorrelationId.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or Sets the Dialing Alias.
        /// </summary>
        public string DialingAlias { get; set; }

        /// <summary>
        /// Gets or Sets the Misc data for Response.
        /// </summary>
        public string MiscDataForResponse { get; set; }

        /// <summary>
        /// Gets or Sets whether an exception occured.
        /// </summary>
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets or Sets the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }

        public bool LogTimingEc { get; set; }

        public bool LogSoapEc { get; set; }

        public bool LogDebugEc { get; set; }
        /// <summary>
        /// Gets or sets the Virtual Meeting Room Response Message.
        /// </summary>       
        public VirtualMeetingRoomCreateResponseMessage VirtualMeetingRoomCreateResponseMessage { get; set; }

        public int EcProcessingTimeMs { get; set; }
    }
}
