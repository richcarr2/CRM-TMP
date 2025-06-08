using System;
using System.Linq;
using Ec.HealthShare.Messages;
using log4net;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.Mappers
{
    /// <summary>
    /// Mapper class for LOB to EC Request.
    /// </summary>
    internal class GetConsultsLobEcMapper
    {
        /// <summary>
        /// Map LOB to EC Request.
        /// </summary>
        /// <returns>EcHealthShareGetConsultsRequest.</returns>
        internal EcHealthShareGetConsultsRequest Map(TmpHealthShareGetConsultsRequest request, string uniqueId, bool isPatientStation, IOrganizationService service, ILog logger)
        {
            var ecRequest = new EcHealthShareGetConsultsRequest
            {
                UniqueId = uniqueId,
                StationNumber = (isPatientStation) ? request.PatientLoginStationNumber : request.ProviderLoginStationNumber,
                Side = (isPatientStation) ? "Patient" : "Provider"
            };

            if (request.PatientIds != null && request.PatientIds.Any())
            {
                var patientId = request.PatientIds.FirstOrDefault();
                if (patientId != Guid.Empty)
                {
                    using (var context = new Xrm(service))
                    {
                        ecRequest.PatientDfn = GetPatientDfn(patientId, ecRequest.StationNumber, context, logger);
                        ecRequest.PatientIcn = GetPatientIcn(patientId, context, logger);
                    }
                }
            }

            // Substitute PatientIcn with PatientDfn in case PatientIcn is null/empty
            if (string.IsNullOrWhiteSpace(ecRequest.PatientIcn))
            {
                ecRequest.PatientIcn = ecRequest.PatientDfn;
                logger.Info($"Patient ICN for the {ecRequest.Side} station '{ecRequest.StationNumber}' is not set. Hence substituting it with Patient DFN");
            }

            if (ecRequest.StationNumber == 0)
            {
                logger.Info($"Patient DFN '{ecRequest.PatientDfn}' or Station Number '{ecRequest.StationNumber}' for {ecRequest.Side} is not set. " + $"Hence not invoking the HealthShare GetConsultsForPatient for {ecRequest.Side} station");
                ecRequest = null;
            }

            return ecRequest;
        }

        /// <summary>
        /// Retrieve the Patient's DFN
        /// </summary>
        /// <param name="contactId">Patient Contact Guid</param>
        /// <param name="stationNumber">The Station Number</param>
        /// <param name="context">The Xrm Context</param>
        /// <returns>The Patient DFN from the Patient Identifier based on the station number</returns>
        private static string GetPatientDfn(Guid contactId, int stationNumber, Xrm context, ILog logger)
        {
            string patientDfn = null;

            var personIdentifier = (from i in context.mcs_personidentifiersSet
                                   where
                                   i.mcs_patient.Id == contactId && 
                                   i.mcs_assigningauthority == "USVHA" &&
                                   i.mcs_assigningfacility == stationNumber.ToString() &&
                                   i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.ParticipantIdentifier_PI
                                   select i)
                                   .Where(pi => pi.tmp_identifierstatus.Equals("A") || 
                                        pi.tmp_identifierstatus.Equals("C") || 
                                        (pi.mcs_identifiertype.Value.Equals((int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI) && pi.tmp_identifierstatus.Equals("P")) ||
                                        pi.tmp_identifierstatus == null)
                                   .FirstOrDefault();

            if (personIdentifier != null)
            {
                patientDfn = personIdentifier.mcs_identifier;
            }

            return patientDfn;
        }

        /// <summary>
        /// Retrieve the Patient's ICN
        /// </summary>
        /// <param name="contactId">Patient Contact Guid</param>
        /// <param name="context">Xrm Context</param>
        /// <returns>The Patient ICN from the Patient Identifier for the Patient Contact</returns>
        private static string GetPatientIcn(Guid contactId, Xrm context, ILog logger)
        {
            string patientIcn = null;

            var personIdentifier = (from i in context.mcs_personidentifiersSet
                                    where
                                    i.mcs_patient.Id == contactId &&
                                    i.mcs_assigningauthority == "USVHA" &&
                                    i.mcs_identifiertype.Value == (int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI
                                    select i)
                                   .Where(pi => pi.tmp_identifierstatus.Equals("A") ||
                                        pi.tmp_identifierstatus.Equals("C") ||
                                        (pi.mcs_identifiertype.Value.Equals((int)mcs_personidentifiersmcs_identifiertype.NationalIdentifier_NI) && pi.tmp_identifierstatus.Equals("P")) ||
                                        pi.tmp_identifierstatus == null)
                                   .FirstOrDefault();

            if (personIdentifier == null)
            {
                var errorMessage = $"ERROR: The patient ICN is not available for the Patient with Id: {contactId}";
                logger.Error(errorMessage);
                throw new PatientIcnException(errorMessage);
            }
            else { 
                patientIcn = personIdentifier.mcs_identifier;
            }

            return patientIcn;
        }
    }
}
