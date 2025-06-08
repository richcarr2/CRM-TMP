using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceUpdatePostStageAsync : IPlugin
    {
        public void Execute(IServiceProvider ServiceProvider)
        {
            var runner = new McsResourceUpdatePostStageAsyncRunner(ServiceProvider);
            runner.RunPlugin(ServiceProvider);
        }
    }
}
