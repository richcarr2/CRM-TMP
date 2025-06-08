using System;
using Ec.VideoVisit.Messages;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;

namespace VA.TMP.Integration.VideoVisit.StateObject
{
    public class VideoVisitGetLoanedDevicesStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="videoVisitCreateRequestMessage">VideoVisitCreateRequestMessage instance.</param>
        public VideoVisitGetLoanedDevicesStateObject(VideoVisitGetLoanedDevicesRequestMessage videoVisitGetLoanedDevicesRequestMessage)
        {
            OrganizationName = videoVisitGetLoanedDevicesRequestMessage.OrganizationName;
            UserId = videoVisitGetLoanedDevicesRequestMessage.UserId;
            LogRequest = videoVisitGetLoanedDevicesRequestMessage.LogRequest;
            ICN = videoVisitGetLoanedDevicesRequestMessage.ICN;
            SamlToken = videoVisitGetLoanedDevicesRequestMessage.SamlToken;
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
        /// Gets or set the ICN
        /// </summary>
        public string ICN { get; set; }

        /// <summary>
        /// Gets or sets whether to log the request.
        /// </summary>
        public bool LogRequest { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public string VistaFakeResponseType { get; set; }

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
        public VideoVisitGetLoanedDevicesResponseMessage VideoVisitGetLoanedDevicesResponseMessage { get; set; }

        public EcTmpGetLoanedDevicesResponse EcResponse { get; set; }

        public int EcProcessingTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the SamlToken
        /// </summary>
        public string SamlToken { get; set; }
    }
}
