using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class UserUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new UserUpdatePostStageRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

