using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentVistaUpdatePostStage : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentVistaHealthShareUpdatePostStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
    }
}
