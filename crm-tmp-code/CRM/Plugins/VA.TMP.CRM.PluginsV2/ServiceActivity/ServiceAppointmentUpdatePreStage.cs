using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentUpdatePreStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentUpdatePreStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
