using System;
using Ec.HealthShare.Messages;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class MakeCancelInboundStateObject : PipeState
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <param name="requestMessage"></param>
        public MakeCancelInboundStateObject(TmpHealthShareMakeCancelInboundRequestMessage requestMessage)
        {
            RequestMessage = requestMessage;
            OrganizationName = requestMessage.OrganizationName;
            UserId = requestMessage.UserId;
            LogRequest = requestMessage.LogRequest;
            IsGroupAppointment = false;
            ErrorMakeCancelToVista = requestMessage.Status.ToUpper().Trim() != "AA";
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
        /// Get or set the Request message.
        /// </summary>
        public TmpHealthShareMakeCancelInboundRequestMessage RequestMessage { get; set; }

        /// <summary>
        /// Get or set the Response message.
        /// </summary>
        public TmpHealthShareMakeCancelInboundResponseMessage ResponseMessage { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Get or set whether HealthShare got an error making or canceling an Appointment in Vista.
        /// </summary>
        public bool ErrorMakeCancelToVista { get; }

        /// <summary>
        /// Get or set string representation of the Request message.
        /// </summary>
        public string SerializedRequestMessage { get; set; }

        /// <summary>
        /// Get or set Integration Result.
        /// </summary>
        public mcs_integrationresult IntegrationResult { get; set; }

        /// <summary>
        /// Get or set the Outbound EC Request Message.
        /// </summary>
        public EcHealthShareMakeCancelOutboundRequestMessage OutboundEcRequestMessage { get; set; }

        /// <summary>
        /// Get or set the Outbound Request Message.
        /// </summary>
        public TmpHealthShareMakeCancelOutboundRequestMessage OutboundRequestMessage { get; set; }

        /// <summary>
        /// Get or set the Patient Id.
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Get or set the Service Appointment.
        /// </summary>
        public ServiceAppointment ServiceAppointment { get; set; }

        /// <summary>
        /// Get or set the Appointment Type.
        /// </summary>
        public AppointmentType AppointmentType { get; set; }

        /// <summary>
        /// Get or set whether this is a Group Appointment.
        /// </summary>
        public bool IsGroupAppointment { get; set; }

        /// <summary>
        /// Get or set the Appointment.
        /// </summary>
        public Appointment Appointment { get; set; }

        /// <summary>
        /// Get or set whether an exception occured.
        /// </summary>
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Get or set the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }
    }
}
