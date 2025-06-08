using System.Diagnostics;
using System.Linq;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.Integration.Rest.Interface;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.Services
{
    public class ProxyAddService : IProxyAddService
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcProxyAddUri => _settings.Items.First(x => x.Key == "EcProxyAddUri").Value;

        private string MviOrgName => _settings.Items.First(x => x.Key == "MviOrgName").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ProxyAddService(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Call the Proxy Add Enterprise Component.
        /// </summary>
        /// <param name="state">State object.</param>
        /// <param name="isPatient">Whether a Patient or Provider.</param>
        public void SendIdentifierToProxyAddEc(ref ProxyAddStateObject state, bool isPatient)
        {
            state.ProxyAddToVistaRequest.OrganizationName = MviOrgName;

            AddPersonResponse response = null;

            switch (state.FakeResponseType)
            {
                // Fake Success
                case "0":
                    response = isPatient ? Common.Mvi.CreateFakeSuccessResponse(state.PatientSite) : Common.Mvi.CreateFakeSuccessResponse(state.ProviderSite);
                    break;

                // DB Down
                case "1":
                    response = Common.Mvi.CreateFakeDbDownResponse();
                    break;

                // Failure to Proxy Add
                case "2":
                    response = Common.Mvi.CreateFakeFailureResponse();
                    break;

                // Server Down
                case "3":
                    response = Common.Mvi.CreateFakeServerDownResponse();
                    break;

                // Default
                default:
                    var timer = new Stopwatch();
                    timer.Start();

                    response = _servicePost.PostToEc<ProxyAddToVistaRequest, AddPersonResponse>(
                        "MVI Proxy Add", EcProxyAddUri, _settings, state.ProxyAddToVistaRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                    timer.Stop();
                    state.EcProcessingTimeMs += (int)timer.ElapsedMilliseconds;
                    break;
            }

            if (response == null) _logger.Error(string.Format("Proxy Add to Vista Response for {0} is null", isPatient ? "Patient" : "Provider"));
            else _logger.Info(string.Format("Proxy Add to Vista Response for {0} is {1}", isPatient ? "Patient" : "Provider", Serialization.DataContractSerialize(response)));

            if (response != null && response.Acknowledgement != null && response.Acknowledgement.TypeCode != null)
            {
                switch (response.Acknowledgement.TypeCode)
                {
                    case "AA":
                        if (response.Acknowledgement.AcknowledgementDetails == null || response.Acknowledgement.AcknowledgementDetails.Length == 0)
                        {
                            state.ExceptionOccured = true;
                            state.ExceptionMessage = "No Acknowledgment Details Listed in Proxy Add to Vista Success Response";
                            _logger.Error("No Acknowledgment Details Listed in Proxy Add to Vista Success Response");
                        }
                        else
                        {
                            var ack = response.Acknowledgement.AcknowledgementDetails.FirstOrDefault();
                            if (ack != null && ack.Code != null)
                            {
                                if (!string.IsNullOrEmpty(ack.Code.Code))
                                {
                                    var parts = ack.Code.Code.Split('^');
                                    if (parts.Length < 3)
                                    {
                                        state.ExceptionOccured = true;
                                        state.ExceptionMessage = "Unexpected Acknowledgment Code in Success, expecting at least 3 parts separated by ^";
                                        _logger.Error("Unexpected Acknowledgment Code in Success, expecting at least 3 parts separated by ^");
                                    }
                                    else
                                    {
                                        var identiferId = parts[0];

                                        _logger.Info(string.Format("{0} - {1}", identiferId, ack.Text));

                                        if (isPatient)
                                        {
                                            state.PatientSideIdentifierToAdd.mcs_identifier = identiferId;
                                            state.OrganizationServiceProxy.Create(state.PatientSideIdentifierToAdd);
                                            _logger.Info(string.Format("VistA Station {0} successfully added to TMP", identiferId));
                                        }
                                        else
                                        {
                                            state.ProviderSideIdentifierToAdd.mcs_identifier = identiferId;
                                            state.OrganizationServiceProxy.Create(state.ProviderSideIdentifierToAdd);
                                            _logger.Info(string.Format("VistA Station {0} successfully added to TMP", identiferId));
                                        }
                                    }
                                }
                                else
                                {
                                    state.ExceptionOccured = true;
                                    state.ExceptionMessage = "No Acknowledgment Code Listed in Proxy Add to Vista Success Response";
                                    _logger.Error("No Acknowledgment Code Listed in Proxy Add to Vista Success Response");
                                }
                            }
                            else
                            {
                                state.ExceptionOccured = true;
                                state.ExceptionMessage = "Acknowledgment or Acknowledgment Code Success is null";
                                _logger.Error("Acknowledgment or Acknowledgment Code Success is null");
                            }
                        }
                        break;
                    case "AR":
                    case "AE":
                        if (response.Acknowledgement.AcknowledgementDetails == null || response.Acknowledgement.AcknowledgementDetails.Length == 0)
                        {
                            state.ExceptionOccured = true;
                            state.ExceptionMessage = "No Acknowledgment Details Listed in Proxy Add to Vista Fail Response";
                            _logger.Error("No Acknowledgment Details Listed in Proxy Add to Vista Fail Response");
                        }
                        else
                        {
                            var ack2 = response.Acknowledgement.AcknowledgementDetails.FirstOrDefault();
                            if (ack2 != null && ack2.Code != null)
                            {
                                if (!string.IsNullOrEmpty(ack2.Code.Code))
                                {
                                    var exceptionMessage = string.Format("Proxy Add to Vista failed with the following: {0} - {1} - {2}", ack2.Code.Code, ack2.Code.DisplayName, ack2.Text);
                                    state.ExceptionOccured = true;
                                    state.ExceptionMessage = exceptionMessage;
                                    _logger.Error(exceptionMessage);

                                }
                                else
                                {
                                    state.ExceptionOccured = true;
                                    state.ExceptionMessage = "No Acknowledgment Code Listed in Proxy Add to Vista Fail Response";
                                    _logger.Error("No Acknowledgment Code Listed in Proxy Add to Vista Fail Response");
                                }
                            }
                            else
                            {
                                state.ExceptionOccured = true;
                                state.ExceptionMessage = "Acknowledgment or Acknowledgment Code Fail is null";
                                _logger.Error("Acknowledgment or Acknowledgment Code Fail is null");
                            }
                        }
                        break;
                }
            }
            else
            {
                if (response == null)
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "No Response from Proxy Add to Vista Enterprise Component";
                    _logger.Error("Response was null");
                }
                else if (response.Acknowledgement == null)
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = string.Format("Unexpected Response from Proxy Add to Vista Enterprise Component.  The following message was returned: {0}.  RawMVIExceptionMessage: {1}.  Possibly MVI Server Down.", response.Message, response.RawMviExceptionMessage);
                    _logger.Error(state.ExceptionMessage);
                }
                else if (response.Acknowledgement.TypeCode == null)
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "Unexpected Response from Proxy Add to Vista Enterprise Component, no Acknowledgment Type Code returned from Enterprise Component";
                    _logger.Error("TypeCode was null");
                }
                else
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "Unexpected Response from Proxy Add to Vista Enterprise Component";
                    _logger.Error(state.ExceptionMessage);
                }
            }
        }
    }
}
