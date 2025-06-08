using log4net;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Mvi.StateObject;
using VEIS.Mvi.Messages;

namespace VA.TMP.Integration.Mvi.Mappers
{
    /// <summary>
    /// Maps Patient/Provider data to a Proxy Add to Vista request.
    /// </summary>
    public class ProxyAddMapper
    {
        private readonly ILog _logger;
        private readonly ProxyAddStateObject _state;
        private readonly bool _isPatient;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="state">State.</param>
        /// <param name="isPatient">Whether Patient or Provider.</param>
        public ProxyAddMapper(ILog logger, ProxyAddStateObject state, bool isPatient)
        {
            _logger = logger;
            _state = state;
            _isPatient = isPatient;
        }

        /// <summary>
        /// Map Patient/Provider data to Proxy Add to Vista request.
        /// </summary>
        /// <returns>ProxyAddToVistaRequest</returns>
        public ProxyAddToVistaRequest Map()
        {
            if (_state.Veteran.BirthDate == null) throw new MissingVeteranBirthdayException("Veteran's birth date cannot be null");

            var proxyAddToVistaRequest = new ProxyAddToVistaRequest
            {
                Requestor = new CrmUser { UserId = _state.UserId, LastName = _state.UserLastName, FirstName = _state.UserFirstName },
                ProcessingCode = _state.ProcessingCode,
                MviIdentifier = new Identifier
                {
                    Id = _state.VeteranIcn.mcs_identifier,
                    Type = IdentifierType.NationalIdentifier,
                    AssigningFacility = "200M",
                    AssigningAuthority = "USVHA"
                },
                SocialSecurityNumber = new Identifier
                {
                    Id = _state.VeteranSs.mcs_identifier,
                    Type = IdentifierType.SocialSecurityNumber,
                    AssigningFacility = _state.VeteranSs.mcs_assigningfacility,
                    AssigningAuthority = _state.VeteranSs.mcs_assigningauthority
                },
                VistaIdentifier = new Identifier
                {
                    Id = _isPatient ? _state.PatientSideIdentifierToAdd.mcs_identifier : _state.ProviderSideIdentifierToAdd.mcs_identifier,
                    Type = IdentifierType.PatientIdentifier,
                    AssigningFacility = _isPatient ? _state.PatientSideIdentifierToAdd.mcs_assigningfacility : _state.ProviderSideIdentifierToAdd.mcs_assigningfacility,
                    AssigningAuthority = _isPatient ? _state.PatientSideIdentifierToAdd.mcs_assigningauthority : _state.ProviderSideIdentifierToAdd.mcs_assigningauthority
                },
                FamilyName = _state.Veteran.LastName,
                GivenName = _state.Veteran.FirstName,
                GenderCode = _state.Veteran.GenderCode != null ? _state.Veteran.GenderCode.Value == 1 ? GenderCode.M : GenderCode.F : GenderCode.NotSpecified,
                BirthDate = _state.Veteran.BirthDate.Value.ToString("yyyyMMdd"),
                ReturnMviMessagesInResponse = _state.ReturnMviMessagesInResponse,
                PatientVeteran = _state.PatientVeteran,
                PatientServiceConnected = _state.PatientServiceConnected,
                PatientType = _state.PatientType
            };

            return proxyAddToVistaRequest;
        }
    }
}
