using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Get the Patients step.
    /// </summary>
    public class GetVeteranStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetVeteranStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            var vet = state.OrganizationServiceProxy.Retrieve(Contact.EntityLogicalName, state.VeteranPartyId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            if (vet == null) throw new MissingPatientException("Veteran could not be found: " + state.VeteranPartyId);
            state.Veteran = vet.ToEntity<Contact>();
        }
    }
}