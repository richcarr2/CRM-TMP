using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsAppointmentCloseCancelPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsAppointmentCloseCancelPostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
