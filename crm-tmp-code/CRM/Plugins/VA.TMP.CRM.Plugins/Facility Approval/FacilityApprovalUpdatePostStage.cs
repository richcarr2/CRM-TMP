using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.CRM.Facility_Approval
{
    public class FacilityApprovalUpdatePostStage : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var runner = new FacilityApprovalUpdatePostStageRunner(serviceProvider);
            runner.RunPlugin(serviceProvider);
        }
    }
}