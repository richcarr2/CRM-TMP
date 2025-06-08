using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsAppointmentDeletePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsAppointmentDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
