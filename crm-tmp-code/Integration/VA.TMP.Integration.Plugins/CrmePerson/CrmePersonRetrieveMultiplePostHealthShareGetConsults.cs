using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.CrmePerson
{
    public class CrmePersonRetrieveMultiplePostHealthShareGetConsults : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CrmePersonRetrieveMultiplePostHealthShareGetConsultsRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
