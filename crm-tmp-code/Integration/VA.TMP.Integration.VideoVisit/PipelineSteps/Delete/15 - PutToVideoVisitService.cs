using System.Diagnostics;
using System.Linq;
using Ec.VideoVisit.Messages;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    /// <summary>
    /// Post to Video Visit Service step.
    /// </summary>
    public class PutToVideoVisitServiceStep : IFilter<VideoVisitDeleteStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcCancelUri => _settings.Items.First(x => x.Key == "EcCancelUri").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public PutToVideoVisitServiceStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VideoVisitDeleteStateObject state)
        {
            if (state.ExceptionOccured) return;
            if (!string.IsNullOrEmpty(state.VistaFakeResponseType)) return;

            var timer = new Stopwatch();
            timer.Start();
            state.CancelAppointmentRequest.SamlToken = state.SamlToken;

            var response = _servicePost.PostToEc<EcTmpCancelAppointmentRequest, EcTmpCancelAppointmentResponse>(
                "VVS Delete", EcCancelUri, _settings, state.CancelAppointmentRequest).ConfigureAwait(false).GetAwaiter().GetResult();

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
                    if (response.HttpStatusCode == "OK")
                    {
                        state.EcResponse = response;
                        return;
                    }
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = string.Format("The Video Visit Cancel Appointment message failed with HTTP status code {0}", response.HttpStatusCode);
                }
                else
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "The EcTmpCancelAppointmentResponse response value is null";
                }
            }
        }
    }
}
