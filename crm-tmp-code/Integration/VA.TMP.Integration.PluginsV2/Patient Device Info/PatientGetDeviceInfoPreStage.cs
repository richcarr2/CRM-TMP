using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.Patient_DeviceInfo
{
    public class PatientGetDeviceInfoPreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new PatientGetDeviceInfoPreStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
