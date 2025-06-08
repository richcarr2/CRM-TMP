using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.CrmePerson
{
    /// <summary>
    /// CRM Plugin class to handle Person Search and Get Person Identifiers.
    /// </summary>
    public class CrmePersonRetrieveMultiplePostStage : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CrmePersonRetrieveMultiplePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
