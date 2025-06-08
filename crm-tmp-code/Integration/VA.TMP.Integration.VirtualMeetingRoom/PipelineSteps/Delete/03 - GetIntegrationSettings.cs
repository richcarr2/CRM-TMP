using System;
using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Delete
{
    /// <summary>
    /// Get Integration Settings step.
    /// </summary>
    public class GetIntegrationSettingsStep : IFilter<VirtualMeetingRoomDeleteStateObject>
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
        public void Execute(VirtualMeetingRoomDeleteStateObject state)
        {
            using (var context = new Xrm(state.OrganizationServiceProxy))
            {
                state.SchemaPath = context.mcs_integrationsettingSet.First(x => x.mcs_name == "Schema Path").mcs_value;

                var useFakeResponse = context.mcs_integrationsettingSet.First(x => x.mcs_name == "Use Fake Response").mcs_value;
                state.UseFakeResponse = Convert.ToBoolean(useFakeResponse);

                var logDebugEc = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "Log Debug Ec");
                state.LogDebugEc = logDebugEc == null ? true : Convert.ToBoolean(logDebugEc.mcs_name);

                var logSoapEc = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "Log Soap Ec");
                state.LogSoapEc = logSoapEc == null ? true : Convert.ToBoolean(logSoapEc.mcs_name);

                var logTimingEc = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "Log Timing Ec");
                state.LogTimingEc = logTimingEc == null ? true : Convert.ToBoolean(logTimingEc.mcs_name);

            }
        }
    }
}
