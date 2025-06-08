using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentVistaCancelPostStage : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentVistaHealthShareCancelPostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}