using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    /// <summary>
    /// CRM Plugin class to handle creating a ServiceAppointment.
    /// </summary>
    public class ServiceAppointmentUpdateRetryPostStage : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentUpdateRetryPostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}