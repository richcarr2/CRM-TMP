using System.Diagnostics;
using System.Linq;
using Ec.HealthShare.Messages;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Rest.Interface;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Send Appointment to EC step.
    /// </summary>
    public class SendAppointmentToEcStep : IFilter<MakeCancelOutboundStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcMakeCancelOutboundUri => _settings.Items.First(x => x.Key == "EcMakeCancelOutboundUri").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SendAppointmentToEcStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelOutboundStateObject state)
        {
            foreach (var ecRequest in state.EcRequestMessages)
            {
                var request = Serialization.DataContractSerialize(ecRequest);
                _logger.Info($"INFO: HealthShare Make Cancel Outbound EC Request: {request}");

                var timer = new Stopwatch();
                timer.Start();

                var returnResponse = state.ResponseMessage.PatientIntegrationResultInformation.FirstOrDefault(x => x.ControlId == ecRequest.ControlId);
                if (returnResponse == null) throw new MissingIntegrationResultException($"Cannot find Response for ControlId: {ecRequest.ControlId}");
                returnResponse.VimtRequest = request;

                if (!state.UseFakeResponse)
                {
                    var ecResponseMessage = _servicePost.PostToEc<EcHealthShareMakeCancelOutboundRequestMessage, EcHealthShareMakeCancelOutboundResponseMessage>(
                        "HealthShare Make Cancel Outbound", EcMakeCancelOutboundUri, _settings, ecRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                    var serializedResponse = Serialization.DataContractSerialize(ecResponseMessage);
                    _logger.Info($"INFO: HealthShare Make Cancel Outbound EC Response: {serializedResponse}");

                    returnResponse.VimtResponse = Serialization.DataContractSerialize(ecResponseMessage);
                    returnResponse.ExceptionOccured = ecResponseMessage.ExceptionOccured;
                    returnResponse.ExceptionMessage = ecResponseMessage.ExceptionMessage;
                }
                else
                {
                    _logger.Info("**************** HealthShare MakeCancel Outbound using FAKES ****************");
                    returnResponse.VimtResponse = string.Empty;
                    returnResponse.ExceptionOccured = false;
                    returnResponse.ExceptionMessage = "********* FAKE RESPONSE *********";
                    state.ResponseMessage.VimtResponse = string.Empty;
                    state.ResponseMessage.ExceptionOccured = false;
                    state.ResponseMessage.ExceptionMessage = "********* FAKE RESPONSE *********";
                }

                timer.Stop();
                returnResponse.EcProcessingMs += (int)timer.ElapsedMilliseconds;
            }
        }
    }
}
