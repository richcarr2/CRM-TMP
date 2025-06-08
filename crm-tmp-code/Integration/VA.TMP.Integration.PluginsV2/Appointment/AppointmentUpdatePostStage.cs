using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.Appointment
{
    /// <summary>
    /// CRM Plugin class to handle creating an Appointment.
    /// </summary>
    public class AppointmentUpdatePostStage : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new AppointmentUpdatePostStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
    }
}
