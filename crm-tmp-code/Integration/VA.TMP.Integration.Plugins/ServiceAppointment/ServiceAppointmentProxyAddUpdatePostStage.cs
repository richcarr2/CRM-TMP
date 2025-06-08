using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentProxyAddUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
