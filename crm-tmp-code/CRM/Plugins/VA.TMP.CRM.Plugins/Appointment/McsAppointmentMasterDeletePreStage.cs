using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsAppointmentMasterDeletePreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsAppointmentMasterDeletePreStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
