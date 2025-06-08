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
    public class VideoVisitDeleteStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="videoVisitDeleteRequestMessage">VideoVisitDeleteRequestMessage instance.</param>
        public VideoVisitDeleteStateObject(VideoVisitDeleteRequestMessage videoVisitDeleteRequestMessage)
        {
            OrganizationName = videoVisitDeleteRequestMessage.OrganizationName;
            UserId = videoVisitDeleteRequestMessage.UserId;
            LogRequest = videoVisitDeleteRequestMessage.LogRequest;
            AppointmentId = videoVisitDeleteRequestMessage.AppointmentId;
            CanceledPatients = new List<Guid>();
            CanceledPatients.AddRange(videoVisitDeleteRequestMessage.CanceledPatients);
            WholeApptCanceled = videoVisitDeleteRequestMessage.WholeAppointmentCanceled;
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
        /// Gets or sets an Cancel Appointment Request.
        /// </summary>
        public EcTmpCancelAppointmentRequest CancelAppointmentRequest { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public string VistaFakeResponseType { get; set; }

        /// <summary>
        /// Gets or sets the Appointment Id.
        /// </summary>
        public Guid AppointmentId { get; set; }

        /// <summary>
        /// Get or set the Service Appointment.
        /// </summary>
        public ServiceAppointment ServiceAppointment { get; set; }

        /// <summary>
        /// Gets the Appointment for Groups
        /// </summary>
        public Appointment CrmAppointment { get; set; }

        /// <summary>
        /// Gets or sets a serialized Appointment.
        /// </summary>
        public string SerializedAppointment { get; set; }

        /// <summary>
        /// Indicates whether the delete request is for a group appointment (or individual)
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// Indicates if the entire appointment was canceled or if it is just an individual removed from a group
        /// </summary>
        public bool WholeApptCanceled { get; set; }

        /// <summary>
        /// Gets or Sets whether an exception occured.
        /// </summary>
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Gets or Sets the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// List of Patients being removed from appointment
        /// </summary>
        public List<Guid> CanceledPatients { get; set; }

        /// <summary>
        /// Gets or sets the Video Visit Response Message to send back to the plugin.
        /// </summary>
        public VideoVisitDeleteResponseMessage VideoVisitDeleteResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets the Cancel Response returned by the EC
        /// </summary>
        public EcTmpCancelAppointmentResponse EcResponse { get; set; }

        public int EcProcessingTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the SamlToken
        /// </summary>
        public string SamlToken { get; set; }
    }
}
