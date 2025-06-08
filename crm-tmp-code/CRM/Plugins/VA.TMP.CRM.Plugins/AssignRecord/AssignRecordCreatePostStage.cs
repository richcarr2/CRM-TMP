using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Plugin entry for Assign Record Create Post Stage
    /// </summary>
    public class AssignRecordCreatePostStage : IPlugin
    {
        /// <summary>
        /// Calls the Assign Record Create Post Stage Runner
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new AssignRecordCreatePostStageRunner(serviceProvider);          
            runner.RunPlugin(serviceProvider);
        }
    }
}