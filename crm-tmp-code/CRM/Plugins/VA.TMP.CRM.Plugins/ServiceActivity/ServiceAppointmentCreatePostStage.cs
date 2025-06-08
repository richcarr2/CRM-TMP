using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentCreatePostStage : IPlugin
    {
        #region IPlugin Implementation
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new ServiceAppointmentCreatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
        #endregion
    }
}