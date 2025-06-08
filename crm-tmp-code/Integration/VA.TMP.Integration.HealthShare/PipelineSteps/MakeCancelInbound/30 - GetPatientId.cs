using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Get Patient Id step.
    /// </summary>
    public class GetPatientIdStep : IFilter<MakeCancelInboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetPatientIdStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelInboundStateObject state)
        {
            using (var svc = new Xrm(state.OrganizationServiceProxy))
            {
                var patientIdentifier = svc.mcs_personidentifiersSet.FirstOrDefault(x =>
                    x.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI &&
                    x.mcs_assigningauthority == "USVHA" &&
                    x.mcs_identifier == state.OutboundEcRequestMessage.PatientIcn);

                if (patientIdentifier == null) throw new PatientIcnException($"Patient ICN not found for {state.OutboundEcRequestMessage.PatientIcn}");

                state.PatientId = patientIdentifier.mcs_patient.Id;
            }
        }
    }
}
