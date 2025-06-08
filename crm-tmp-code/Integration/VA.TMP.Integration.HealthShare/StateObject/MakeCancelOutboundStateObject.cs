using System;
using System.Collections.Generic;
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
    public class MakeCancelOutboundStateObject : PipeState
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        /// <param name="requestMessage"></param>
        public MakeCancelOutboundStateObject(TmpHealthShareMakeCancelOutboundRequestMessage requestMessage)
        {
            ControlIds = new List<string>();
            EcRequestMessages = new List<EcHealthShareMakeCancelOutboundRequestMessage>();
            ResponseMessage = new TmpHealthShareMakeCancelOutboundResponseMessage();
            RequestMessage = requestMessage;
            OrganizationName = requestMessage.OrganizationName;
            UserId = requestMessage.UserId;
            LogRequest = requestMessage.LogRequest;
            IsCancelAppointment = requestMessage.VisitStatus.ToUpper() == VistaStatus.CANCELED.ToString();
            IsMakeAppointment = requestMessage.VisitStatus.ToUpper() == VistaStatus.SCHEDULED.ToString();
            ServiceAppointmentId = requestMessage.ServiceAppointmentId;
            AppointmentId = requestMessage.AppointmentId;
            VistaIntegrationResultId = requestMessage.VistaIntegrationResultId;
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
        public TmpHealthShareMakeCancelOutboundRequestMessage RequestMessage { get; set; }

        /// <summary>
        /// Get or set the Response message.
        /// </summary>
        public TmpHealthShareMakeCancelOutboundResponseMessage ResponseMessage { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Get or set whether to use a fake response.
        /// </summary>
        public bool UseFakeResponse { get; set; }

        /// <summary>
        /// Get the Service Appointment Id.
        /// </summary>
        public Guid ServiceAppointmentId { get; }

        public bool IsCancelAppointment { get; }

        /// <summary>
        /// Get whether this is a Scheduled Appointment.
        /// </summary>
        public bool IsMakeAppointment { get; }

        /// <summary>
        /// Get or set the Service Appointment.
        /// </summary>
        public ServiceAppointment ServiceAppointment { get; set; }

        /// <summary>
        /// Get or set the ControlIds.
        /// </summary>
        public List<string> ControlIds { get; set; }

        /// <summary>
        /// Get the Appointment Id.
        /// </summary>
        public Guid? AppointmentId { get; }

        /// <summary>
        /// Get or set the Appointment.
        /// </summary>
        public Appointment Appointment { get; set; }

        /// <summary>
        /// Get or set Patient Facility.
        /// </summary>
        public mcs_facility PatientFacility { get; set; }

        /// <summary>
        /// Get or set Provider Facility.
        /// </summary>
        public mcs_facility ProviderFacility { get; set; }

        /// <summary>
        /// Get or set Appointment Type.
        /// </summary>
        public AppointmentType AppointmentType { get; set; }

        /// <summary>
        /// Get or set whether is Group Appointment.
        /// </summary>
        public bool IsGroupAppointment { get; set; }

        /// <summary>
        /// Get or set whether is Group Appointment.
        /// </summary>
        public bool GroupAppointment { get; set; }

        /// <summary>
        /// Get or set Patient Clinic.
        /// </summary>
        public mcs_resource PatientClinic { get; set; }

        /// <summary>
        /// Get or set Provider Clinic.
        /// </summary>
        public mcs_resource ProviderClinic { get; set; }

        /// <summary>
        /// Gets or Sets Vista Integration Result Id in the case of Group Cancel.
        /// </summary>
        public Guid? VistaIntegrationResultId { get; }

        /// <summary>
        /// Gets or Sets the Vista Integration Result for Group Cancel.
        /// </summary>
        public cvt_vistaintegrationresult VistaIntegrationResult { get; set; }

        /// <summary>
        /// Get or set EC Request Messages.
        /// </summary>
        public List<EcHealthShareMakeCancelOutboundRequestMessage> EcRequestMessages { get; set; }

        /// <summary>
        /// Get or set string representation of the Request message.
        /// </summary>
        public string SerializedRequestMessage { get; set; }
    }
}
