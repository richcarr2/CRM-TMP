using System;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create
{
    /// <summary>
    /// Get Service Appointment step.
    /// </summary>
    public class GetServiceAppointmentStep : IFilter<VirtualMeetingRoomCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetServiceAppointmentStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomCreateStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                state.ServiceAppointment = context.ServiceAppointmentSet.FirstOrDefault(x => x.Id == state.AppointmentId);
                if (state.ServiceAppointment == null) throw new MissingAppointmentException(string.Format("Unable to retrieve Service Appointment - {0}", state.AppointmentId));

                if (state.ServiceAppointment.ScheduledStart == null) throw new MissingAppointmentException(string.Format("Service Appointment Start Date is null - {0}", state.AppointmentId));
                if (state.ServiceAppointment.ScheduledEnd == null) throw new MissingAppointmentException(string.Format("Service Appointment End Date is null - {0}", state.AppointmentId));

                state.AppointmentStartDate = (DateTime)state.ServiceAppointment.ScheduledStart;
                state.AppointmentEndDate = (DateTime)state.ServiceAppointment.ScheduledEnd;
            }
        }
    }
}
