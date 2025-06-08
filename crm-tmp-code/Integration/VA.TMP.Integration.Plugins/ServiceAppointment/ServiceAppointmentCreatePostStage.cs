using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    /// <summary>
    /// CRM Plugin class to handle creating a ServiceAppointment.
    /// </summary>
    public class ServiceAppointmentCreatePostStage : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
