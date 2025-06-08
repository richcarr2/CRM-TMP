using System;
using System.Text;
using Ec.VideoVisit.Messages;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VideoVisit.Helpers;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.PipelineSteps.Update
{
    public class ValidateBusinessRulesStep : IFilter<VideoVisitUpdateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ValidateBusinessRulesStep(ILog logger)
        {
            _logger = logger;
        }

        public void Execute(VideoVisitUpdateStateObject state)
        {
            var errors = ValidateAllBusinessRules(state.Appointment, state.ServiceAppointment);
            if (string.IsNullOrEmpty(errors)) return;

            errors += "\n\n" + state.SerializedAppointment;
            throw new VvsBusinessRulesException(errors);
        }

        private static string ValidateAllBusinessRules(EcTmpUpdateAppointmentRequest ecRequest, ServiceAppointment serviceAppointment)
        {
            var validationErrors = new StringBuilder();
            var validator = new BusinessRuleValidator();

            validationErrors
                .Append(validator.CheckPatientCount(ecRequest.Patients))
                .Append(validator.CheckAppointmentKind(ecRequest))
                .Append(validator.CheckLocationTypes(ecRequest))
                .Append(validator.CheckPatientAssigningAuthorityAndName(ecRequest.Patients))
                .Append(validator.CheckPatientEmail(ecRequest.Patients, (EcTmpAppointmentKind)Enum.Parse(typeof(EcTmpAppointmentKind), ecRequest.AppointmentKind, true)))
                .Append(validator.CheckProviderEmail(ecRequest.Providers))
                .Append(validator.CheckVmrs(ecRequest, serviceAppointment));

            return validationErrors.ToString().Trim('|');
        }
    }
}
