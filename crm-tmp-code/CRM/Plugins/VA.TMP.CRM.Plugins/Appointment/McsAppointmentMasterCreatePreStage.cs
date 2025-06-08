using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsAppointmentMasterCreatePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsAppointmentMasterCreatePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
