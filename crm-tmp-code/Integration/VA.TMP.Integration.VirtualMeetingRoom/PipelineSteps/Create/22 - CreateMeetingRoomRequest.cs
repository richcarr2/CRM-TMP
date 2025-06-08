using System.Diagnostics;
using System.Linq;
using Ec.VirtualMeetingRoom.Messages;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create
{
    /// <summary>
    /// Post to Vyopta step.
    /// </summary>
    public class CreateMeetingRoomeRequestStep : IFilter<VirtualMeetingRoomCreateStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcCreateUri => _settings.Items.First(x => x.Key == "EcCreateUri").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CreateMeetingRoomeRequestStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomCreateStateObject state)
        {
            if (state.UseFakeResponse || state.ExceptionOccured) return;

            var ecVmrRequest = new EcVyoptaSMScheduleMeetingRequest
            {
                OrganizationName = state.OrganizationName,
                UserId = state.UserId,
                mcs_EncounterId = state.AppointmentId.ToString(),
                mcs_EndTime = state.AppointmentEndDate,
                mcs_GuestName = state.PatientId.ToString(),
                mcs_GuestPin = state.PatientPin,
                mcs_HostName = state.ProviderId.ToString(),
                mcs_HostPin = state.ProviderPin,
                mcs_MeetingRoomName = state.MeetingRoomName,
                mcs_MiscData = state.MiscDataForRequest,
                mcs_StartTime = state.AppointmentStartDate
            };

            // Call the Enterprise Component to send the meeting info to Vyopta
            var timer = new Stopwatch();
            timer.Start();

            var response = _servicePost.PostToEc<EcVyoptaSMScheduleMeetingRequest, EcVyoptaSMScheduleMeetingResponse>(
                "VMR Schedule", EcCreateUri, _settings, ecVmrRequest).ConfigureAwait(false).GetAwaiter().GetResult();

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
                    if (response.mcs_EncounterId != state.AppointmentId.ToString())
                    {
                        state.ExceptionOccured = true;
                        state.ExceptionMessage = string.Format("The EncounterId {0} returned from Vyopta does not match the AppointmentId {1} from TMP", response.mcs_EncounterId, state.AppointmentId);
                    }
                    else
                    {
                        state.CorrelationId = response.mcs_EncounterId;
                        state.DialingAlias = response.mcs_DialingAlias;
                        state.MiscDataForResponse = response.mcs_MiscData;
                    }
                }
                else
                {
                    state.ExceptionOccured = true;
                    state.ExceptionMessage = "The EcVyoptaSMScheduleMeetingResponseDataInfo response value is null";
                }
            }
        }
    }
}
