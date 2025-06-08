using System.Linq;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VEIS.Messages.VIAScheduling;

namespace VA.TMP.Integration.Vista.Mappers
{
    public static class VistaMapperHelper
    {
        internal static VEISVIAScheLIloginVIAResponse VistaLoginFakeResponse(string fakeResponseType)
        {
            if (fakeResponseType == "0")
                return new VEISVIAScheLIloginVIAResponse
                {
                    VEISVIAScheLIuserTOInfo = new VEISVIAScheLIuserTO
                    {
                        mcs_DUZ = "FakeUserDuz",
                        mcs_greeting = "Good Afternoon, Test Name",
                        mcs_name = "Test Name",
                        mcs_siteId = "516"
                    },
                    ExceptionOccurred = false,
                    //MessageId = new Guid().ToString()
                };
            else
                return new VEISVIAScheLIloginVIAResponse
                {
                    VEISVIAScheLIuserTOInfo = new VEISVIAScheLIuserTO
                    {
                        VEISVIAScheLIfault2Info = new VEISVIAScheLIfault2
                        {
                            mcs_message = "Unable to Sign in using Identity and Access Management STS Token.  Try using Access/Verify Codes instead"
                        }
                    },
                    ExceptionOccurred = false,
                    //MessageId = new Guid().ToString()
                };
        }

        public static QueryBeanSecuritySettings GetSecuritySettings(IOrganizationService orgService)
        {
            QueryBeanSecuritySettings settings = new QueryBeanSecuritySettings();
            using (var srv = new Xrm(orgService))
            {
                var errorString = string.Empty;
                var reqAppString = "VIA Requesting App";
                var consAppString = "VIA Consuming App Token";
                var consPassString = "VIA Consuming App Password";
                var ViaSettings = srv.mcs_integrationsettingSet.Where(s => s.mcs_name.Contains("VIA")).ToList();
                var RequestingAppRecord = ViaSettings.FirstOrDefault(s => s.mcs_name == reqAppString);
                var ConsumingAppRecord = ViaSettings.FirstOrDefault(s => s.mcs_name == consAppString);
                var ConsumingAppPasswordRecord = ViaSettings.FirstOrDefault(s => s.mcs_name == consPassString);

                if (RequestingAppRecord == null || string.IsNullOrEmpty(RequestingAppRecord.mcs_value))
                    errorString += reqAppString;
                if (ConsumingAppRecord == null || string.IsNullOrEmpty(ConsumingAppRecord.mcs_value))
                    errorString += string.IsNullOrEmpty(errorString) ? consAppString : ", " + consAppString;
                if (ConsumingAppPasswordRecord == null || string.IsNullOrEmpty(ConsumingAppPasswordRecord.mcs_value))
                    errorString += string.IsNullOrEmpty(errorString) ? consPassString : ", " + consPassString;
                if (!string.IsNullOrEmpty(errorString))
                    throw new MissingViaCredentialsException(string.Format("No {0} Setting was found, unable to process Vista Request", errorString));

                settings.RequestingApp = RequestingAppRecord.mcs_value;
                settings.ConsumingAppToken = ConsumingAppRecord.mcs_value;
                settings.ConsumingAppPassword = ConsumingAppPasswordRecord.mcs_value;
            }
            return settings;
        }
    }
}
