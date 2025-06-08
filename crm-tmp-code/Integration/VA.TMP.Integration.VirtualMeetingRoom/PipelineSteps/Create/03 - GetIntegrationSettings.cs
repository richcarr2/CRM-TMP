using System;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create
{
    /// <summary>
    /// Get Integration Settings step.
    /// </summary>
    public class GetIntegrationSettingsStep : IFilter<VirtualMeetingRoomCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetIntegrationSettingsStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomCreateStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                var integrationSettings = context.mcs_integrationsettingSet.Select(x => new IntegrationSetting { Name = x.mcs_name, Value = x.mcs_value }).ToList();

                var virtualMeetingRoomDigitLengthString = integrationSettings.First(x => x.Name == "Virtual Meeting Room Digit Length").Value;
                state.VirtualMeetingRoomDigitLength = Convert.ToInt32(virtualMeetingRoomDigitLengthString);

                state.VirtualMeetingRoomPrefix = integrationSettings.First(x => x.Name == "Virtual Meeting Room Prefix").Value;
                state.VirtualMeetingRoomSuffix = integrationSettings.First(x => x.Name == "Virtual Meeting Room Suffix").Value;

                var patientPinLengthString = integrationSettings.First(x => x.Name == "Patient Pin Length").Value;
                state.PatientPinLength = Convert.ToInt32(patientPinLengthString);

                var providerPinLengthString = integrationSettings.First(x => x.Name == "Provider Pin Length").Value;
                state.ProviderPinLength = Convert.ToInt32(providerPinLengthString);

                state.SchemaPath = integrationSettings.First(x => x.Name == "Schema Path").Value;

                state.ProviderVmrFormatUrl = integrationSettings.First(x => x.Name == "Provider VMR Format URL").Value;
                state.PatientVmrFormatUrl = integrationSettings.First(x => x.Name == "Patient VMR Format URL").Value;

                state.VmrBaseUrl = integrationSettings.First(x => x.Name == "VMR Base URL").Value;
                state.VmrBaseUrlExtension = integrationSettings.First(x => x.Name == "VMR Base URL Extension").Value;

                var useFakeResponse = integrationSettings.First(x => x.Name == "Use Fake Response").Value;
                state.UseFakeResponse = Convert.ToBoolean(useFakeResponse);

                state.VyoptaGuestUrlPrefix = integrationSettings.First(x => x.Name == "Vyopta Guest Url Prefix").Value;
                state.VyoptaHostUrlPrefix = integrationSettings.First(x => x.Name == "Vyopta Host Url Prefix").Value;

                var logDebugEc = integrationSettings.FirstOrDefault(x => x.Name == "Log Debug Ec");
                state.LogDebugEc = logDebugEc == null || Convert.ToBoolean(logDebugEc.Value);

                var logSoapEc = integrationSettings.FirstOrDefault(x => x.Name == "Log Soap Ec");
                state.LogSoapEc = logSoapEc == null || Convert.ToBoolean(logSoapEc.Value);

                var logTimingEc = integrationSettings.FirstOrDefault(x => x.Name == "Log Timing Ec");
                state.LogTimingEc = logTimingEc == null || Convert.ToBoolean(logTimingEc.Value);
            }
        }
    }
}
