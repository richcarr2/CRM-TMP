using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Create
{
    /// <summary>
    /// Gets SA/Appt and Providers
    /// </summary>
    public class GetServiceAppointmentStep : IFilter<VideoVisitCreateStateObject>
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
        public void Execute(VideoVisitCreateStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                state.CrmAppointment = srv.AppointmentSet.FirstOrDefault(a => a.Id == state.AppointmentId);
                if (state.CrmAppointment != null)
                {
                    if (state.CrmAppointment.cvt_serviceactivityid == null) throw new MissingAppointmentException("Group Appointment has no parent Service Activity");
                    state.ServiceAppointment = srv.ServiceAppointmentSet.FirstOrDefault(sa => sa.Id == state.CrmAppointment.cvt_serviceactivityid.Id);
                }
                else
                {
                    state.ServiceAppointment = srv.ServiceAppointmentSet.FirstOrDefault(x => x.Id == state.AppointmentId);
                }

                // Look up the booked system users in the resources party list and verify they are not from the patient side              
                if (state.ServiceAppointment == null) throw new MissingAppointmentException("Service Appointment cannot be null");
            }
        }
    }
}
