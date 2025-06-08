using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentVmrCancelPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentVmrCancelPostStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
    }
}