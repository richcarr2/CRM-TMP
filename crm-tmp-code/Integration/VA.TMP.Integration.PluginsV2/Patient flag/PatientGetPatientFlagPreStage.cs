using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace VA.TMP.Integration.Plugins.Patient_flag
{
    public class PatientGetPatientFlagPreStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new PatientGetPatientFlagPreStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}
