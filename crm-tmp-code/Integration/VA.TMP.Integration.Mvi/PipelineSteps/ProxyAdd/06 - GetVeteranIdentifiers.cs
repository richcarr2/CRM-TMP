using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd
{
    /// <summary>
    /// Get Veteran Identifiers step.
    /// </summary>
    public class GetVeteranIdentifiersStep : IFilter<ProxyAddStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetVeteranIdentifiersStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(ProxyAddStateObject state)
        {
            if (state.Veteran == null) return;

            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                state.VeteranIdentifiers = context.mcs_personidentifiersSet.Where(x => x.mcs_patient.Id == state.Veteran.Id).ToList();

                if (state.VeteranIdentifiers == null || !state.VeteranIdentifiers.Any())
                {
                    throw new MissingIdentifiersException("The Veteran does not have any MVI Identifiers");
                }

                state.VeteranIcn = state.VeteranIdentifiers.FirstOrDefault(x => x.mcs_assigningauthority == "USVHA" && x.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI);
                if (state.VeteranIcn == null) throw new MissingIdentifiersException("Veteran does not have an National Identifier");

                state.VeteranSs = state.VeteranIdentifiers.FirstOrDefault(x => x.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS);
                if (state.VeteranSs == null) throw new MissingIdentifiersException("Veteran does not have an SS Identifier");
            }
        }
    }
}
