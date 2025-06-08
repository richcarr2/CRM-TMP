using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentSetStatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentSetStatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

