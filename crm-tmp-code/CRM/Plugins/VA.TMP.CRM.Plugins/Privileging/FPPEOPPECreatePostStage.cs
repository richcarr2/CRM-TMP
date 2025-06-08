using Microsoft.Xrm.Sdk;
using System;

namespace VA.TMP.CRM
{
    public class FPPEOPPECreatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new FPPEOPPECreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}