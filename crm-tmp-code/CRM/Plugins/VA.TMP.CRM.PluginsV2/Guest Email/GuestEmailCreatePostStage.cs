using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM.Guest_Email
{
    public class GuestEmailCreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new GuestEmailCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }

}
