using System;
using System.Diagnostics;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.OnDemand
{
    /// <summary>
    /// Set the Meeting Room Name Step. 
    /// </summary>
    public class SetMeetingRoomNameStep : IFilter<VmrOnDemandCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SetMeetingRoomNameStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VmrOnDemandCreateStateObject state)
        {
            var continueFindingNumber = true;
            string proposedName;
            var timer = Stopwatch.StartNew();
            var depth = 0;

            do
            {
                var randomDigit = RandomDigits.GetRandomDigitString(7);
                depth++;

                proposedName = $"{state.VirtualMeetingRoomPrefix}{randomDigit}";

                using (var context = new Xrm(state.OrganizationServiceProxy))
                {
                    _logger.Debug($"Progress: Beginning VOD Step. Took {timer.ElapsedMilliseconds} ms");

                    var dateHundredDaysAgo = state.AppointmentStartDate.Subtract(new TimeSpan(100, 0, 0, 0));

                    var match = context.cvt_vodSet.FirstOrDefault(x =>
                        x.Id != state.VideoOnDemandId &&
                        x.cvt_meetingroomname != null &&
                        x.cvt_starttime >= dateHundredDaysAgo &&
                        x.cvt_starttime <= state.AppointmentStartDate &&
                        x.cvt_meetingroomname == proposedName);

                    _logger.Debug($"Progress: Retrieved All VOD Appointments. Took {timer.ElapsedMilliseconds} ms");

                    if (match == null) continueFindingNumber = false;
                }
            } while (continueFindingNumber);

            if (string.IsNullOrEmpty(proposedName)) throw new VmrNameException("Unable to generate VOD name");

            state.MeetingRoomName = proposedName;

            _logger.Debug($"Progress: Generated unique Meeting Room Name - {state.MeetingRoomName}. Took {timer.ElapsedMilliseconds} ms with {depth} try(s)");
            timer.Stop();
        }
    }
}
