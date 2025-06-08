using System.Diagnostics;
using System.Linq;
using Ec.VideoVisit.Messages;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.VideoVisit.Helpers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.GetLoanedDevices
{
    /// <summary>
    /// Post to Video Visit Service step.
    /// </summary>
    public class PostToVideoVisitServiceStep : IFilter<VideoVisitGetLoanedDevicesStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcGetLoanedDevicesUri => _settings.Items.First(x => x.Key == "EcGetLoanedDevicesUri").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public PostToVideoVisitServiceStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitGetLoanedDevicesStateObject state)
        {
            if (state.ExceptionOccured) return;
            if (!string.IsNullOrEmpty(state.VistaFakeResponseType)) return;

            var ecTmpGetLoandedDevicesRequest = new EcTmpGetLoanedDevicesRequest
            {
                ICN = state.ICN,
                SamlToken = state.SamlToken
            };

            _logger.Debug($"Request payload: {Serialization.DataContractSerialize(ecTmpGetLoandedDevicesRequest)}");


            // Call the Enterprise Component to send the meeting info to Vyopta
            var timer = new Stopwatch();
            timer.Start();

            var response = _servicePost.PostToEc<EcTmpGetLoanedDevicesRequest, EcTmpGetLoanedDevicesResponse>(
                "VVS GetLoanedDevices", EcGetLoanedDevicesUri, _settings, ecTmpGetLoandedDevicesRequest).ConfigureAwait(false).GetAwaiter().GetResult();

            _logger.Debug($"Response from EC: {Serialization.DataContractSerialize(response)}");


            timer.Stop();
            state.EcProcessingTimeMs += (int)timer.ElapsedMilliseconds;
            if (response.ExceptionOccured)
            {
                state.ExceptionOccured = true;
                state.ExceptionMessage = response.ExceptionMessage;
            }
            else
            {
                if (response != null)
                {
                    state.EcResponse = response;
                    state.VideoVisitGetLoanedDevicesResponseMessage = LoanedDevicesHelper.ConvertLoanedDevices(response, _logger);
                }
                else
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "The EcTmpGetLoandedDevicesResponse response value is null";
                }
            }
        }
    }
}
