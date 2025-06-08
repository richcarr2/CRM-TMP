using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentIntegrationOrchestratorPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentIntegrationOrchestratorPostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
