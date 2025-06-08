using System;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;

namespace VA.TMP.Integration.VirtualMeetingRoom.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class VirtualMeetingRoomDeleteStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="virtualMeetingRoomDeleteRequestMessage">VirtualMeetingRoomCreateRequestMessage instance.</param>
        public VirtualMeetingRoomDeleteStateObject(VirtualMeetingRoomDeleteRequestMessage virtualMeetingRoomDeleteRequestMessage)
        {
            OrganizationName = virtualMeetingRoomDeleteRequestMessage.OrganizationName;
            UserId = virtualMeetingRoomDeleteRequestMessage.UserId;
            LogRequest = virtualMeetingRoomDeleteRequestMessage.LogRequest;
            AppointmentId = virtualMeetingRoomDeleteRequestMessage.AppointmentId;
            MiscDataForRequest = virtualMeetingRoomDeleteRequestMessage.MiscData;
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
        /// Gets or sets the Schema Path.
        /// </summary>
        public string SchemaPath { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public bool UseFakeResponse { get; set; }

        /// <summary>
        /// Gets or sets the CorrelationId.
        /// </summary>
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets the Misc data for Request.
        /// </summary>
        public string MiscDataForRequest { get; set; }

        /// <summary>
        /// Gets or sets the Misc data for Response.
        /// </summary>
        public string MiscDataForResponse { get; set; }

        /// <summary>
        /// Gets or Sets a Virtual Meeting Room instance.
        /// </summary>
        public Schema.VirtualMeetingRoom.VirtualMeetingRoomDeleteType VirtualMeetingRoomDelete { get; set; }

        /// <summary>
        /// Gets or Sets an instance of the VirtualMeetingRoom class as a string.
        /// </summary>
        public string SerializedVirtualMeetingRoomDelete { get; set; }

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
        public VirtualMeetingRoomDeleteResponseMessage VirtualMeetingRoomDeleteResponseMessage { get; set; }

        public int EcProcessingTimeMs { get; set; }
    }
}
