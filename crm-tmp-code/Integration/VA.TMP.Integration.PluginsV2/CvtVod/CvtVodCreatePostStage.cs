using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.CvtVod
{
    /// <summary>
    /// CRM Plugin class to handle updating a VOD.
    /// </summary>
    public class CvtVodCreatePostStage : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider">Service Provider.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new CvtVodCreatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
