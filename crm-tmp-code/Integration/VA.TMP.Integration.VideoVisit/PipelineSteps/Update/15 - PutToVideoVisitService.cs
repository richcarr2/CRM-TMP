using System.Diagnostics;
using System.Linq;
using Ec.VideoVisit.Messages;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    public class PutToVideoVisitServiceStep : IFilter<VideoVisitUpdateStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcUpdateUri => _settings.Items.First(x => x.Key == "EcUpdateUri").Value;

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

        public void Execute(VideoVisitUpdateStateObject state)
        {
            if (state.ExceptionOccured) return;
            if (!string.IsNullOrEmpty(state.FakeResponseType)) return;

            state.Appointment.OrganizationName = state.OrganizationName;
            state.UserId = state.UserId;
            state.Appointment.SamlToken = state.SamlToken;

            // Call the Enterprise Component to send the meeting info to Vyopta
            var timer = new Stopwatch();
            timer.Start();

            var response = _servicePost.PostToEc<EcTmpUpdateAppointmentRequest, EcTmpUpdateAppointmentResponse>(
                "VVS Update", EcUpdateUri, _settings, state.Appointment).ConfigureAwait(false).GetAwaiter().GetResult();

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
                    state.ExceptionMessage = "The EcTmpUpdateAppointmentResponse response value is null";
                }
            }
        }
    }
}
