using System;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.Provider_Availability
{
    public class UserGetProviderAvailability : IPlugin
    {
        /// <summary>
        /// Entry point to plugin execution.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new UserGetProviderAvailabilityRunner(serviceProvider);
            runner.Execute();
        }
    }
}
