using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.CRM
{
    public class McsPreRetrieveMultipleSearch : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new McsPreRetrieveMultipleSearchRunner(serviceProvider);

            runner.RunPlugin(serviceProvider);
        }
    }
}

