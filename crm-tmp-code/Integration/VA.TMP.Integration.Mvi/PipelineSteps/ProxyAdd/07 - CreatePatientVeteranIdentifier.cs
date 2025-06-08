using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Create Patient Veteran Identifier step.
    /// </summary>
    public class CreatePatientVeteranIdentifierStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CreatePatientVeteranIdentifierStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            if (string.IsNullOrEmpty(state.PatientSite) || state.Veteran == null) return;

            // Query to determine if the Patient Site is already in the Identifier list
            var veteranIdentifier = state.VeteranIdentifiers.FirstOrDefault(x => x.mcs_assigningfacility == state.PatientSite);

            if (veteranIdentifier == null)
            {
                state.PatientSideIdentifierToAdd = new mcs_personidentifiers()
                {
                    mcs_assigningauthority = "USVHA",
                    mcs_assigningfacility = state.PatientSite,
                    mcs_identifier = "PROXY_VISTA",
                    mcs_identifiertype = new Microsoft.Xrm.Sdk.OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.ParticipantIdentifier_PI),
                    mcs_name = string.Format("VistA Station {0} IEN", state.PatientSite),
                    mcs_patient = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, state.Veteran.Id)
                };
            }
            else
            {
                _logger.Info("Patient Site already in the Identifier list");
            }
        }
    }
}
