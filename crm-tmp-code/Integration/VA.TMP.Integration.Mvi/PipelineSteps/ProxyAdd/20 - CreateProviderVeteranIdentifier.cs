using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Create Provider Veteran Identifier step.
    /// </summary>
    public class CreateProviderVeteranIdentifierStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CreateProviderVeteranIdentifierStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            if (string.IsNullOrEmpty(state.ProviderSite) || state.ExceptionOccured || state.Veteran == null) return;

            state.PatientAndProviderSitesAreEqual = state.PatientSite == state.ProviderSite;

            if (state.PatientAndProviderSitesAreEqual)
            {
                _logger.Info("Provider and Patient Station Codes are the same, skip creating provider veteran identifier");
                return;
            }

            var veteranIdentifier = state.VeteranIdentifiers.FirstOrDefault(x => x.mcs_assigningfacility == state.ProviderSite);

            if (veteranIdentifier == null)
            {
                state.ProviderSideIdentifierToAdd = new mcs_personidentifiers()
                {
                    mcs_assigningauthority = "USVHA",
                    mcs_assigningfacility = state.ProviderSite,
                    mcs_identifier = "PROXY_VISTA",
                    mcs_identifiertype = new Microsoft.Xrm.Sdk.OptionSetValue((int)mcs_personidentifiersmcs_identifiertype.ParticipantIdentifier_PI),
                    mcs_name = string.Format("VistA Station {0} IEN", state.ProviderSite),
                    mcs_patient = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, state.Veteran.Id)
                };
            }
            else
            {
                _logger.Info("Provider Site already in the Identifer list");
            }
        }
    }
}
