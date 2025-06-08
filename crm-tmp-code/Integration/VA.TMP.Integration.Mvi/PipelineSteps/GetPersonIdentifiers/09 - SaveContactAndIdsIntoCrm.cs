using System;
using System.Diagnostics;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.Mappers;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Mvi.PipelineSteps.GetPersonIdentifiers
{
    /// <summary>
    /// Save Contact and Ids step.
    /// </summary>
    public class SaveContactAndIdsIntoCrmStep : IFilter<GetPersonIdentifiersStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SaveContactAndIdsIntoCrmStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetPersonIdentifiersStateObject state)
        {
            var stopWatch = Stopwatch.StartNew();

            _logger.Info(string.Format("Calling TMP.SaveContactsAndIdsIntoCrmStep."));
            var rawMviValues = state.RawMviValue?.Split('^');
            var identifier = rawMviValues.FirstOrDefault();
            var identifierStatus = rawMviValues.LastOrDefault();
            var identifierType = rawMviValues.First(v => v.Equals("NI"));

            _logger.Debug($"Indentifier Type: {identifierType}");

            if (string.IsNullOrEmpty(identifier))
            {
                _logger.Error("No RawMviValue in state object, unable to retrieve identifier");
            }
            else
            {
                using (var context = new Xrm(state.OrganizationServiceProxy))
                {
                    var contact = (from c in context.ContactSet
                                   join i in context.mcs_personidentifiersSet
                                   on c.Id equals i.mcs_patient.Id
                                   where
                                   i.mcs_identifier == identifier &&
                                   i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI &&
                                   i.mcs_assigningauthority == "USVHA"
                                   select c).FirstOrDefault();
                    _logger.Info(string.Format("***************Progress.{0}. Took {1} ms************", "Retrieved Contact", stopWatch.ElapsedMilliseconds));

                    if (contact == null || contact.Id.Equals(Guid.Empty))
                    {
                        identifier = state.Ss;
                        contact = (from c in context.ContactSet
                                   join i in context.mcs_personidentifiersSet
                                   on c.Id equals i.mcs_patient.Id
                                   where
                                   i.mcs_identifier == identifier &&
                                   i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.SocialSecurity_SS &&
                                   i.mcs_assigningauthority == "USSSA"
                                   select c).FirstOrDefault();
                    }

                    var serializedSelectedPersonRequest = Serialization.DataContractSerialize(state.SelectedPersonRequest);
                    _logger.Info($"Selected Person Request: {serializedSelectedPersonRequest}");
                    _logger.Info("***********************************************************************************");
                    var serializedIds = Serialization.DataContractSerialize(state.CorrespondingIds);
                    _logger.Info($"Corresponding Ids: {serializedIds}");

                    //Only create the Contact if the Identifier Status is either A or C, or P and the Identifier Type is NI
                    if (contact == null && (identifierStatus.Trim().Equals("A") || identifierStatus.Trim().Equals("C") || (identifierStatus.Trim().Equals("P") && !string.IsNullOrEmpty(identifierType))))
                    {
                        state.Contact = MapGetPersonIdentifiersRequestToContact.Create(state.SelectedPersonRequest, state);
                        state.Contact.Id = state.OrganizationServiceProxy.Create(state.Contact);
                        state.Contact.ContactId = state.Contact.Id;
                    }
                    else
                    {
                        state.Contact = MapGetPersonIdentifiersRequestToContact.Update(state.SelectedPersonRequest, state, contact, state.OrganizationServiceProxy);
                        state.OrganizationServiceProxy.Update(state.Contact);
                    }
                }
                _logger.Info(string.Format("***************Progress.{0}. Took {1} ms************", "Set Contact Update/Create", stopWatch.ElapsedMilliseconds));

                _logger.Info(string.Format("***************Progress.{0}. Took {1} ms************", "Completed Step", stopWatch.ElapsedMilliseconds));
            }
        }
    }
}
