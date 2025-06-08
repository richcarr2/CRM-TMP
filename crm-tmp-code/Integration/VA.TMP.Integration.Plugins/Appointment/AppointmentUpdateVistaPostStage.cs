using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentUpdateVistaPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new AppointmentUpdateVistaHealthSharePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
