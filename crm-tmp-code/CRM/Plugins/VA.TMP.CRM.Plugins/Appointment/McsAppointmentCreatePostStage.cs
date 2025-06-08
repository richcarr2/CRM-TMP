using Microsoft.Xrm.Sdk;
using System;


namespace VA.TMP.CRM
{
    public class McsAppointmentCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsAppointmentCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
