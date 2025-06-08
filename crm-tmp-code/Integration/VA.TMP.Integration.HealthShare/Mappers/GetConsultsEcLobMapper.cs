using Ec.HealthShare.Messages;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.Mappers
{
    /// <summary>
    /// Mapper class for EC to LOB Response.
    /// </summary>
    internal class GetConsultsEcLobMapper
    {
        /// <summary>
        /// Map EC to LOB Response.
        /// </summary>
        /// <returns>TmpHealthShareGetConsultsResponse.</returns>
        internal TmpHealthShareGetConsultsResponse Map(EcHealthShareGetConsultsResponse response, bool isPatientStation, TmpHealthShareGetConsultsResponse tmpResponse = null)
        {
            if (tmpResponse == null)
            {
                tmpResponse = new TmpHealthShareGetConsultsResponse
                {
                    ControlId = response.ControlId,
                    PatientDfn = response.PatientDfn,
                    PatientIcn = response.PatientIcn,
                    QueryName = response.QueryName,
                    Institution = response.Institution
                };
            }

            tmpResponse.ExceptionOccured = response.ExceptionOccured;
            tmpResponse.ExceptionMessage = response.ExceptionMessage;

            foreach (var resConsult in response.Consults)
            {
                var consult = new TmpConsult
                {
                    ConsultId = resConsult.ConsultId,
                    UniqueRequestId = response.ControlId,
                    ConsultRequestDateTime = resConsult.ConsultRequestDateTime,
                    ToConsultService = resConsult.ToConsultService,
                    ConsultTitle = resConsult.ConsultTitle,
                    ConsultStatus = resConsult.ConsultStatus,
                    ClinicallyIndicatedDate = resConsult.ClinicallyIndicatedDate,
                    StopCodes = resConsult.StopCodes,
                    Provider = resConsult.Provider,
                    ReceivingSiteConsultId = resConsult.ReceivingSiteConsultId
                };

                if (isPatientStation) tmpResponse.PatientConsults.Add(consult);
                else tmpResponse.ProviderConsults.Add(consult);
            }

            foreach (var responseRtc in response.ReturnToClinic)
            {
                var rtc = new TmpReturnToClinicOrder
                {
                    RtcId = responseRtc.RtcId,
                    RtcRequestDateTime = responseRtc.RtcRequestDateTime,
                    ToClinicIen = responseRtc.ToClinicIen,
                    ClinicName = responseRtc.ClinicName,
                    ClinicallyIndicatedDate = responseRtc.ClinicallyIndicatedDate,
                    StopCodes = responseRtc.StopCodes,
                    Provider = responseRtc.Provider,
                    Comments = responseRtc.Comments,
                    MultiRtc = responseRtc.MultiRtc
                };

                if (isPatientStation) tmpResponse.PatientReturnToClinicOrders.Add(rtc);
                else tmpResponse.ProviderReturnToClinicOrders.Add(rtc);
            }

            return tmpResponse;
        }
    }
}