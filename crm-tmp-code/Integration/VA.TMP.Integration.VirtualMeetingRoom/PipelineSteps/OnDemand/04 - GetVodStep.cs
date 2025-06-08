using System;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.OnDemand
{
    /// <summary>
    /// Get Service Appointment step.
    /// </summary>
    public class GetVodStep : IFilter<VmrOnDemandCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetVodStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VmrOnDemandCreateStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                state.VideoOnDemand = context.cvt_vodSet.FirstOrDefault(x => x.Id == state.VideoOnDemandId);
                if (state.VideoOnDemand == null) throw new MissingVodException(string.Format("Unable to retrieve Video On Demand - {0}", state.VideoOnDemandId));

                if (state.VideoOnDemand.cvt_starttime == null) throw new MissingVodException(string.Format("Video On Demand Start Date is null - {0}", state.VideoOnDemandId));
                if (state.VideoOnDemand.cvt_endtime == null) throw new MissingVodException(string.Format("Video On Demand End Date is null - {0}", state.VideoOnDemandId));

                state.AppointmentStartDate = (DateTime)state.VideoOnDemand.cvt_starttime;
                state.AppointmentEndDate = (DateTime)state.VideoOnDemand.cvt_endtime;
            }
        }
    }
}
