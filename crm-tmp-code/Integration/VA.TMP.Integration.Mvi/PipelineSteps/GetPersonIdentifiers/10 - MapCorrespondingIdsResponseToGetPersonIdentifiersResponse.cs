using System;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.PipelineSteps.GetPersonIdentifiers
{
    /// <summary>
    /// Map Corresponding Ids Response to Get Person Identifiers Response step.
    /// </summary>
    public class MapCorrespondingIdsResponseToGetPersonIdentifiersResponseStep : IFilter<GetPersonIdentifiersStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapCorrespondingIdsResponseToGetPersonIdentifiersResponseStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(GetPersonIdentifiersStateObject state)
        {
            state.GetPersonIdentifiersResponseMessage = new Messages.Mvi.GetPersonIdentifiersResponseMessage
            {
                DateofBirth = state.CorrespondingIdsResponse?.DateofBirth ?? state.DateOfBirth,
                FullAddress = state.CorrespondingIdsResponse?.FullAddress ?? state.FullAddress,
                Edipi = state.CorrespondingIdsResponse?.Edipi ?? state.Edipi,
                ExceptionMessage = state.CorrespondingIdsResponse?.RawMviExceptionMessage,
                ExceptionOccured = state.CorrespondingIdsResponse?.ExceptionOccured ?? false,
                FamilyName = state.CorrespondingIdsResponse?.FamilyName ?? state.FamilyName,
                FirstName = state.CorrespondingIdsResponse?.FirstName ?? state.FirstName,
                FullName = state.CorrespondingIdsResponse?.FullName ?? state.FullName,
                MiddleName = state.CorrespondingIdsResponse?.MiddleName ?? state.MiddleName,
                //MessageId = state.CorrespondingIdsResponse?.MessageId,
                ParticipantId = state.CorrespondingIdsResponse?.ParticipantId,
                Ss = state.CorrespondingIdsResponse?.SocialSecurityNumber,
                UserId = state.CorrespondingIdsResponse?.UserId ?? state.UserId,
                Url = string.Format("https://{0}/main.aspx?etn=contact&pagetype=entityrecord&id=%7B{1}%7D", state.ServerName.Substring(state.ServerName.IndexOf("//", StringComparison.Ordinal) + 2), state.Contact.ContactId),
                CorrespondingIdList = state.CorrespondingIds,
                ContactId = state.Contact == null ? Guid.Empty : state.Contact.Id
            };
        }
    }
}
