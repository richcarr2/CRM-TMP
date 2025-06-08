using System;
using Ec.HealthShare.Messages;
using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class GetConsultsStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="requestMessage">Request message.</param>
        public GetConsultsStateObject(TmpHealthShareGetConsultsRequest requestMessage)
        {
            OrganizationName = requestMessage.OrganizationName;
            UserId = requestMessage.UserId;
            RequestMessage = requestMessage;
            LogRequest = requestMessage.LogRequest;
            IsHomeMobile = requestMessage.IsHomeMobile;
            IsStoreForward = requestMessage.IsStoreForward;
            EcProcessingTimeMs = 0;
        }

        /// <summary>
        /// Get or set the Request message.
        /// </summary>
        public TmpHealthShareGetConsultsRequest RequestMessage { get; set; }

        /// <summary>
        /// Get or set EC Request message.
        /// </summary>
        public EcHealthShareGetConsultsRequest PatEcRequestMessage { get; set; }

        /// <summary>
        /// Get or set EC Request message.
        /// </summary>
        public EcHealthShareGetConsultsRequest ProEcRequestMessage { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

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
        /// Flag sent from plugin to determine if the consults from patient side is retrieved (false = retrieved, true = not retrieved)
        /// </summary>
        public bool IsHomeMobile { get; set; }

        /// <summary>
        /// Flag sent from plugin to determine if the consults from provider side is retrieved (false = retrieved, true = not retrieved)
        /// </summary>
        public bool IsStoreForward { get; set; }

        /// <summary>
        /// Get or set the Response message.
        /// </summary>
        public TmpHealthShareGetConsultsResponse ResponseMessage { get; set; }

        /// <summary>
        /// Get or set the EC Response message.
        /// </summary>
        public EcHealthShareGetConsultsResponse PatEcResponseMessage { get; set; }

        /// <summary>
        /// Get or set the EC Response message.
        /// </summary>
        public EcHealthShareGetConsultsResponse ProEcResponseMessage { get; set; }

        /// <summary>
        /// Get or set the Patient Request UniqueId
        /// </summary>
        public string PatientRequestUniqueId { get; set; }

        /// <summary>
        /// Get or set the Provider Request UniqueId
        /// </summary>
        public string ProviderRequestUniqueId { get; set; }

        /// <summary>
        /// Get or set string representation of the Request message.
        /// </summary>
        public string SerializedRequestMessage { get; set; }

        /// <summary>
        /// Get or set whether an exception occured.
        /// </summary>
        public bool ExceptionOccured { get; set; }

        /// <summary>
        /// Get or set the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Get or set whether to fake the EC Call/Response for local testing.
        /// </summary>
        public string ConsultsFakeResponseType { get; set; }

        /// <summary>
        /// Get or set EC Processing Time.
        /// </summary>
        public int EcProcessingTimeMs { get; set; }
    }
}
