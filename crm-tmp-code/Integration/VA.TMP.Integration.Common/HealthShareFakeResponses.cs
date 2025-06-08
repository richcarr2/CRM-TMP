using System.Collections.Generic;
using Ec.HealthShare.Messages;

namespace VA.TMP.Integration.Common
{
    public static class HealthShareFakeResponses
    {
        /// <summary>
        /// Return the fake response for the Get Consults for patients HealthShare request
        /// </summary>
        /// <returns>The Get Consults successful Fake response</returns>
        public static EcHealthShareGetConsultsResponse FakeGetConsultsForPatientSuccess()
        {
            return new EcHealthShareGetConsultsResponse
            {
                Institution = 500,
                ControlId = "143",
                QueryName = "Pending Consults",
                Consults = new List<EcConsult>
                {
                    new EcConsult
                    {
                        ConsultId = "1288",
                        ConsultRequestDateTime = "2018-06-15T19:22:00.000Z",
                        ToConsultService = "DIABETIC EYE EXAM CONSULT",
                        ConsultTitle = "DIABETIC EYE EXAM CONSULT Cons",
                        ClinicallyIndicatedDate = "2018-06-29T05:00:00.000Z",
                        StopCodes = "146,172",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "p",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "1289",
                        ConsultRequestDateTime = "2018-06-15T19:22:00.000Z",
                        ToConsultService = "DIABETES NUTRITION",
                        ConsultTitle = "DIABETES NUTRITION Cons",
                        ClinicallyIndicatedDate = "2018-06-29T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "dc",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "1290",
                        ConsultRequestDateTime = "2018-06-15T19:22:00.000Z",
                        ToConsultService = "DIABETES PODIATRY CONSULT",
                        ConsultTitle = "DIABETES PODIATRY CONSULT Cons",
                        ClinicallyIndicatedDate = "2018-06-29T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "c",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "1287",
                        ConsultRequestDateTime = "2018-06-14T18:13:00.000Z",
                        ToConsultService = "CARDIOLOGY",
                        ConsultTitle = "CARDIOLOGY Cons",
                        ClinicallyIndicatedDate = "2018-06-15T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "h",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "624",
                        ConsultRequestDateTime = "2014-05-06T17:53:00.000Z",
                        ToConsultService = "CARDIOLOGY",
                        ConsultTitle = "CARDIOLOGY Cons",
                        ClinicallyIndicatedDate = "2014-05-06T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "?",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "625",
                        ConsultRequestDateTime = "2014-05-06T17:53:00.000Z",
                        ToConsultService = "RADIOLOGY LOW\\/VASC STUDY",
                        ConsultTitle = "RADIOLOGY LOW\\/VASC STUDY Cons",
                        ClinicallyIndicatedDate = "2014-03-22T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "a",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "623",
                        ConsultRequestDateTime = "2014-05-06T17:53:00.000Z",
                        ToConsultService = "ANTICOAG CLINIC DR. WHITE",
                        ConsultTitle = "ANTICOAG CLINIC DR. WHITE Cons",
                        ClinicallyIndicatedDate = "2014-05-06T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "e",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "621",
                        ConsultRequestDateTime = "2013-11-05T18:14:00.000Z",
                        ToConsultService = "CARDIOLOGY",
                        ConsultTitle = "CARDIOLOGY Cons",
                        ClinicallyIndicatedDate = "2013-11-05T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "Provider,FIFTEEN",
                        ConsultStatus = "s",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "620",
                        ConsultRequestDateTime = "2013-10-25T14:02:00.000Z",
                        ToConsultService = "CARDIOLOGY",
                        ConsultTitle = "CARDIOLOGY Cons",
                        ClinicallyIndicatedDate = "2013-10-25T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "P",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "619",
                        ConsultRequestDateTime = "2013-10-24T16:10:00.000Z",
                        ToConsultService = "CARDIOLOGY",
                        ConsultTitle = "CARDIOLOGY Cons",
                        ClinicallyIndicatedDate = "2013-10-24T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "pr",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "618",
                        ConsultRequestDateTime = "2013-09-10T14:28:00.000Z",
                        ToConsultService = "RADIOLOGY LOW\\/VASC STUDY",
                        ConsultTitle = "RADIOLOGY LOW\\/VASC STUDY Cons",
                        ClinicallyIndicatedDate = "2013-09-10T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "dly",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "617",
                        ConsultRequestDateTime = "2013-08-26T15:50:00.000Z",
                        ToConsultService = "CARDIOLOGY",
                        ConsultTitle = "CARDIOLOGY Cons",
                        ClinicallyIndicatedDate = "2013-08-26T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PROGRAMMER,ONE",
                        ConsultStatus = "u",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "385",
                        ConsultRequestDateTime = "2004-04-02T04:03:00.000Z",
                        ToConsultService = "HEMATOLOGY CONSULT",
                        ConsultTitle = "HEMATOLOGY CONSULT Cons",
                        ClinicallyIndicatedDate = "2013-08-26T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PATHOLOGY,ONE",
                        ConsultStatus = "dce",
                        ReceivingSiteConsultId = "0"
                    },
                    new EcConsult
                    {
                        ConsultId = "386",
                        ConsultRequestDateTime = "2004-04-02T04:03:00.000Z",
                        ToConsultService = "AUDIOLOGY OUTPATIENT",
                        ConsultTitle = "AUDIOLOGY OUTPATIENT Cons",
                        ClinicallyIndicatedDate = "2013-08-26T05:00:00.000Z",
                        StopCodes = "",
                        Provider = "PATHOLOGY,ONE",
                        ConsultStatus = "",
                        ReceivingSiteConsultId = "0"
                    }
                },
                ReturnToClinic = new List<EcReturnToClinicOrder>
                {
                    new EcReturnToClinicOrder
                    {
                        RtcId = "1306",
                        RtcRequestDateTime = "2018-06-15T05:00:00.000Z",
                        ToClinicIen = "23",
                        ClinicName = "GENERAL MEDICINE",
                        ClinicallyIndicatedDate = "2018-07-20T05:00:00.000Z",
                        StopCodes = "141,114",
                        Provider = "ZZZRETFIVEFIFTYONE,PATIENT",
                        Comments = "need to see patient again by Jul 20"
                    },
                    new EcReturnToClinicOrder
                    {
                        RtcId = "1370",
                        RtcRequestDateTime = "2018-10-16T05:00:00.000Z",
                        ToClinicIen = "195",
                        ClinicName = "CARDIOLOGY",
                        ClinicallyIndicatedDate = "2018-10-23T05:00:00.000Z",
                        StopCodes = "143,114",
                        Provider = "ZZZRETFIVEFIFTYONE,PATIENT",
                        Comments = "TEST RTC"
                    },
                    new EcReturnToClinicOrder
                    {
                        RtcId = "1371",
                        RtcRequestDateTime = "2018-10-16T05:00:00.000Z",
                        ToClinicIen = "195",
                        ClinicName = "CARDIOLOGY",
                        ClinicallyIndicatedDate = "2018-10-31T05:00:00.000Z",
                        StopCodes = "143,114",
                        Provider = "ZZZRETFIVEFIFTYONE,PATIENT",
                        Comments = string.Empty
                    },
                    new EcReturnToClinicOrder
                    {
                        RtcId = "1396",
                        RtcRequestDateTime = "2018-11-02T05:00:00.000Z",
                        ToClinicIen = "285",
                        ClinicName = "DIABETIC",
                        ClinicallyIndicatedDate = "2018-11-15T05:00:00.000Z",
                        StopCodes = "146,",
                        Provider = "ZZZRETFIVEFIFTYONE,PATIENT",
                        Comments = "test rtc for multiple appointments"
                    }
                }
            };
        }
    }
}
