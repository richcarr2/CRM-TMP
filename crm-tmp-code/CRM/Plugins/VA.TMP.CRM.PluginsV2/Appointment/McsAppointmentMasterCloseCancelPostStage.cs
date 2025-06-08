using System;
using Microsoft.Xrm.Sdk;


namespace VA.TMP.CRM
{
    public class McsAppointmentMasterCloseCancelPostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsAppointmentMasterCloseCancelPostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}
