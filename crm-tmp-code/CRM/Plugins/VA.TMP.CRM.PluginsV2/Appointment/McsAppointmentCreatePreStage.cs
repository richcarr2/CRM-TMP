using Microsoft.Xrm.Sdk;
using System;


namespace VA.TMP.CRM
{
    public class McsAppointmentCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsAppointmentCreatePreStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
    }
}
