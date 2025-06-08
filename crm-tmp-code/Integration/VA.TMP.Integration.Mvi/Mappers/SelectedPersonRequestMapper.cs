using VA.TMP.Integration.Mvi.StateObject;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.Mappers
{
    /// <summary>
    /// Class to Map State to Selected Person Request.
    /// </summary>
    public class SelectedPersonRequestMapper
    {
        /// <summary>
        /// Map State to Selected Person Request.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal SelectedPersonRequest Map(GetPersonIdentifiersStateObject state)
        {
            return new SelectedPersonRequest
            {
                Edipi = state.Edipi,
                AssigningAuthority = state.AssigningAuthority,
                AssigningFacility = state.AssigningFacility,
                IdentifierClassCode = state.IdentifierClassCode,
                FirstName = state.FirstName,
                FamilyName = state.FamilyName,
                MiddleName = state.MiddleName,
                OrganizationName = state.OrganizationName,
                SocialSecurityNumber = state.Ss,
                PatientSearchIdentifier = state.PatientSearchIdentifier,
                IdentifierType = state.IdentifierType,
                RecordSource = state.RecordSource,
                RawValueFromMvi = state.RawMviValue,
                FullAddress = state.FullAddress,
                DateofBirth = state.DateOfBirth,
                UseRawMviValue = state.UseRawMviValue,
                UserFirstName = state.UserFirstName,
                UserId = state.UserId,
                UserLastName = state.UserLastName,
                FullName = state.FullName
            };
        }
    }
}