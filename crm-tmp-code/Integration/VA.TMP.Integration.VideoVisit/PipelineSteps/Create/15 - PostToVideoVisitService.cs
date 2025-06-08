using System.Diagnostics;
using System.Linq;
using Ec.VideoVisit.Messages;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Create
{
    /// <summary>
    /// Post to Video Visit Service step.
    /// </summary>
    public class PostToVideoVisitServiceStep : IFilter<VideoVisitCreateStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcCreateUri => _settings.Items.First(x => x.Key == "EcCreateUri").Value;

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
        public void Execute(VideoVisitCreateStateObject state)
        {
            if (state.ExceptionOccured) return;
            if (!string.IsNullOrEmpty(state.VistaFakeResponseType)) return;

            var ecTmpCreateAppointmentRequest = new EcTmpCreateAppointmentRequest
            {
                OrganizationName = state.OrganizationName,
                UserId = state.UserId,
                EcTmpCreateAppointmentRequestDataInfo = state.Appointment
            };

            ecTmpCreateAppointmentRequest.EcTmpCreateAppointmentRequestDataInfo.SamlToken = state.SamlToken;

            // Call the Enterprise Component to send the meeting info to Vyopta
            var timer = new Stopwatch();
            timer.Start();

            var response = _servicePost.PostToEc<EcTmpCreateAppointmentRequest, EcTmpCreateAppointmentResponse>(
                "VVS Create", EcCreateUri, _settings, ecTmpCreateAppointmentRequest).ConfigureAwait(false).GetAwaiter().GetResult();


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

                    if (response.HttpStatusCode == "OK") return;
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = string.Format("The Video Visit Create Appointment message failed with HTTP status code {0}", response.HttpStatusCode);
                }
                else
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "The EcTmpCreateAppointmentResponse response value is null";
                }
            }
        }
    }
}
