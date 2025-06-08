using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentUpdateVVSPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new AppointmentUpdateVVSPostStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
    }
}
