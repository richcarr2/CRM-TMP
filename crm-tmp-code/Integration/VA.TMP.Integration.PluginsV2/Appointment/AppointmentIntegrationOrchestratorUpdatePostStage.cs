using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.Appointment
{
    public class AppointmentIntegrationOrchestratorUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new AppointmentIntegrationOrchestratorUpdatePostStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
    }
}
