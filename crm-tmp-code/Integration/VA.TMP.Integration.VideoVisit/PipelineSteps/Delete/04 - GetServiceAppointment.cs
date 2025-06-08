using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Delete
{
    /// <summary>
    /// Get Service Appointment step.
    /// </summary>
    public class GetServiceAppointmentStep : IFilter<VideoVisitDeleteStateObject>
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
        public void Execute(VideoVisitDeleteStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var appt = srv.AppointmentSet.FirstOrDefault(a => a.Id == state.AppointmentId);
                state.IsGroup = appt != null;
                if (state.IsGroup)
                {
                    state.CrmAppointment = appt;
                    if (state.CrmAppointment == null) throw new MissingAppointmentException(string.Format("Unable to retrieve Group Appointment - {0}", state.AppointmentId));
                    state.ServiceAppointment = srv.ServiceAppointmentSet.FirstOrDefault(sa => sa.Id == state.CrmAppointment.cvt_serviceactivityid.Id);
                    if (state.ServiceAppointment == null) throw new MissingAppointmentException(string.Format("Unable to retrieve Group Service Appointment - {0}", state.AppointmentId));
                }
                else
                {
                    state.ServiceAppointment = srv.ServiceAppointmentSet.FirstOrDefault(sa => sa.Id == state.AppointmentId);
                    if (state.ServiceAppointment == null) throw new MissingAppointmentException(string.Format("Unable to retrieve Individual Service Appointment - {0}", state.AppointmentId));
                }
            }
        }
    }
}
