using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.OptionSets;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class ProxyAddStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="proxyAddRequestMessage">ProxyAddRequestMessage instance.</param>
        public ProxyAddStateObject(ProxyAddRequestMessage proxyAddRequestMessage)
        {
            OrganizationName = proxyAddRequestMessage.OrganizationName;
            UserId = proxyAddRequestMessage.UserId;
            UserFirstName = proxyAddRequestMessage.UserFirstName;
            UserLastName = proxyAddRequestMessage.UserLastName;
            LogRequest = proxyAddRequestMessage.LogRequest;
            ServiceAppointId = proxyAddRequestMessage.ServiceAppointmentId;
            AppointmentId = proxyAddRequestMessage.AppointmentId;
            FakeResponseType = proxyAddRequestMessage.FakeResponseType;
            ProcessingCode = (ProcessingType)Enum.Parse(typeof(ProcessingType), proxyAddRequestMessage.ProcessingCode, true);
            ReturnMviMessagesInResponse = proxyAddRequestMessage.ReturnMviMessagesInResponse;
            PatientVeteran = proxyAddRequestMessage.PatientVeteran;
            PatientServiceConnected = proxyAddRequestMessage.PatientServiceConnected;
            PatientType = proxyAddRequestMessage.PatientType;
            VeteranPartyId = proxyAddRequestMessage.VeteranPartyId;
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
        /// Gets or sets User's first name.
        /// </summary>
        public string UserFirstName { get; set; }

        /// <summary>
        /// Gets or sets User's last name.
        /// </summary>
        public string UserLastName { get; set; }

        /// <summary>
        /// Gets or sets whether to log the request.
        /// </summary>
        public bool LogRequest { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Gets or sets the Service Appointment Id.
        /// </summary>
        public Guid? ServiceAppointId { get; set; }

        /// <summary>
        /// Gets or Sets the Appointment Id for Groups.
        /// </summary>
        public Guid? AppointmentId { get; set; }

        /// <summary>
        /// Gets or sets the ServiceAppointment.
        /// </summary>
        public ServiceAppointment ServiceAppointment { get; set; }

        /// <summary>
        /// Gets or sets the Appointment
        /// </summary>
        public Appointment Appointment { get; set; }

        /// <summary>
        /// Gets or sets Patient's VistA site.
        /// </summary>
        public string PatientSite { get; set; }

        /// <summary>
        /// Gets or sets the Service related to the Service Appointment.
        /// </summary>
        public mcs_services Service { get; set; }

        ///<summary>
        ///Gets or sets the Scheduling Package related to the Service Appointment
        ///</summary>

        public cvt_resourcepackage ResourcePackage { get; set; }

        /// <summary>
        /// Gets or sets Provider's VistA site.
        /// </summary>
        public string ProviderSite { get; set; }

        /// <summary>
        /// Gets or sets the status reason.
        /// </summary>
        public serviceappointment_statuscode? StatusReason { get; set; }

        /// <summary>
        /// Gets or sets the Veteran.
        /// </summary>
        public Contact Veteran { get; set; }

        /// <summary>
        /// Gets or sets list of identifiers for the veteran.
        /// </summary>
        public IEnumerable<mcs_personidentifiers> VeteranIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets Veteran ICN identifier.
        /// </summary>
        public mcs_personidentifiers VeteranIcn { get; set; }

        /// <summary>
        /// Gets or sets Veteran SS identifier.
        /// </summary>
        public mcs_personidentifiers VeteranSs { get; set; }

        /// <summary>
        /// Gets or sets the Patient Identifer to add.
        /// </summary>
        public mcs_personidentifiers PatientSideIdentifierToAdd { get; set; }

        /// <summary>
        /// Gets or sets the Provider site identifier to add
        /// </summary>
        public mcs_personidentifiers ProviderSideIdentifierToAdd { get; set; }

        /// <summary>
        /// Gets or sets the check for both sites having same station code
        /// </summary>
        public bool PatientAndProviderSitesAreEqual { get; set; }

        /// <summary>
        /// Gets or sets the Proxy Add to Vista Request.
        /// </summary>
        public ProxyAddToVistaRequest ProxyAddToVistaRequest { get; set; }

        /// <summary>
        /// Gets or sets whether to use a fake response.
        /// </summary>
        public string FakeResponseType { get; set; }

        /// <summary>
        /// Gets or sets ProcessingCode.
        /// </summary>
        public ProcessingType ProcessingCode { get; set; }

        /// <summary>
        /// Gets or sets ReturnMviMessagesInResponse.
        /// </summary>
        public bool ReturnMviMessagesInResponse { get; set; }

        /// <summary>
        /// Gets or sets PatientVeteran.
        /// </summary>
        public bool PatientVeteran { get; set; }

        /// <summary>
        /// Gets or sets PatientServiceConnected.
        /// </summary>
        public bool PatientServiceConnected { get; set; }

        /// <summary>
        /// Gets or sets PatientType.
        /// </summary>
        public int PatientType { get; set; }

        public Guid VeteranPartyId { get; set; }

        /// <summary>
        /// Gets or sets an instance of the request sent to the EC.
        /// </summary>
        public string SerializedInstance { get; set; }

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
        public ProxyAddResponseMessage ProxyAddResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets EC Processing Time
        /// </summary>
        public int EcProcessingTimeMs { get; set; }
    }
}
