using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class McsResourceCreatePostStageAsync : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsResourceCreatePostStageAsyncRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
