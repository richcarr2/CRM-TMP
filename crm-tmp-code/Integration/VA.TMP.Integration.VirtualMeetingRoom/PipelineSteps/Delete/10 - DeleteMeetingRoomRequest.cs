using System.Diagnostics;
using System.Linq;
using Ec.VirtualMeetingRoom.Messages;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Delete
{
    /// <summary>
    /// Send to Video Visit Service step.
    /// </summary>
    public class DeleteMeetingRoomRequestStep : IFilter<VirtualMeetingRoomDeleteStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcDeleteUri => _settings.Items.First(x => x.Key == "EcDeleteUri").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public DeleteMeetingRoomRequestStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomDeleteStateObject state)
        {
            if (state.UseFakeResponse) return;

            var ecVmrRequest = new EcVirtualDeleteMeetingRequest
            {
                OrganizationName = state.OrganizationName,
                UserId = state.UserId,
                mcs_EncounterId = state.AppointmentId.ToString(),
                mcs_MiscData = state.MiscDataForRequest
            };

            // Call the Enterprise Component to send the meeting info to Vyopta
            var timer = new Stopwatch();
            timer.Start();

            var response = _servicePost.PostToEc<EcVirtualDeleteMeetingRequest, EcVirtualDeleteMeetingResponse>(
                "VMR Delete", EcDeleteUri, _settings, ecVmrRequest).ConfigureAwait(false).GetAwaiter().GetResult();

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
                    state.MiscDataForResponse = response.mcs_MiscData;
                }
                else
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "The EcVirtualDeleteMeetingResponseDataInfo response value is null";
                }
            }
        }
    }
}
