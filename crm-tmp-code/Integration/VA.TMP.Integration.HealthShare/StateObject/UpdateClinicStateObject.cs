using Microsoft.Xrm.Sdk.WebServiceClient;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.StateObject
{
    /// <summary>
    /// Class used to hold state between pipeline steps.
    /// </summary>
    public class UpdateClinicStateObject : PipeState
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="requestMessage">TmpHealthShareUpdateClinicRequestMessage instance.</param>
        public UpdateClinicStateObject(TmpHealthShareUpdateClinicRequestMessage requestMessage)
        {
            RequestMessage = requestMessage;
        }

        /// <summary>
        /// Get or set the Request message.
        /// </summary>
        public TmpHealthShareUpdateClinicRequestMessage RequestMessage { get; set; }

        /// <summary>
        /// Get or set the Clinic.
        /// </summary>
        public mcs_resource Clinic { get; set; }

        /// <summary>
        /// Get or set the CRM Organization Service Proxy.
        /// </summary>
        public OrganizationWebProxyClient OrganizationServiceProxy { get; set; }

        /// <summary>
        /// Get or set TmpHealthShareUpdateClinicResponseMessage.
        /// </summary>
        public TmpHealthShareUpdateClinicResponseMessage ResponseMessage { get; set; }

        /// <summary>
        /// Get or set string respresentation of the Request message.
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
    }
}