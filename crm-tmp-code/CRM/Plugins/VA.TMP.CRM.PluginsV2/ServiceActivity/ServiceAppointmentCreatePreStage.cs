using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentCreatePreStage:IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentCreatePreStageRunner(serviceProvider);
            runner.Execute();
            //runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}
