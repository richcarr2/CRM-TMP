using System;
using System.Collections.Generic;
using Ec.VideoVisit.Messages;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;

namespace VA.TMP.Integration.VideoVisit.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class VideoVisitCreateStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="videoVisitCreateRequestMessage">VideoVisitCreateRequestMessage instance.</param>
        public VideoVisitCreateStateObject(VideoVisitCreateRequestMessage videoVisitCreateRequestMessage)
        {
            OrganizationName = videoVisitCreateRequestMessage.OrganizationName;
            UserId = videoVisitCreateRequestMessage.UserId;
            LogRequest = videoVisitCreateRequestMessage.LogRequest;
            AppointmentId = videoVisitCreateRequestMessage.AppointmentId;
            SystemUsers = new List<SystemUser>();
            ContactIds = new List<Guid>();
            ContactIds.AddRange(videoVisitCreateRequestMessage.AddedPatients);
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
        /// Gets or sets the UserId.
        /// </summary>
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public string VistaFakeResponseType { get; set; }

        /// <summary>
        /// Get or set the Service Appointment.
        /// </summary>
        public ServiceAppointment ServiceAppointment { get; set; }

        /// <summary>
        /// Get or set the Service Appointment.
        /// </summary>
        public Appointment CrmAppointment { get; set; }

        /// <summary>
        /// Get or set the Service Appointment.
        /// </summary>
        public List<Guid> ContactIds { get; set; }

        /// <summary>
        /// Get or set the Service Appointment.
        /// </summary>
        public List<SystemUser> SystemUsers { get; set; }

        /// <summary>
        /// Gets or sets an Appointment.
        /// </summary>
        public EcTmpCreateAppointmentRequestData Appointment { get; set; }

        /// <summary>
        /// Gets or sets a serialized Appointment.
        /// </summary>
        public string SerializedAppointment { get; set; }

        /// <summary>
        /// Gets or Sets whether an exception occured.
        /// </summary>
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets or Sets the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the Video Visit Response Message.
        /// </summary>
        public VideoVisitCreateResponseMessage VideoVisitCreateResponseMessage { get; set; }

        public EcTmpCreateAppointmentResponse EcResponse { get; set; }

        public int EcProcessingTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the SamlToken
        /// </summary>
        public string SamlToken { get; set; }
    }
}
