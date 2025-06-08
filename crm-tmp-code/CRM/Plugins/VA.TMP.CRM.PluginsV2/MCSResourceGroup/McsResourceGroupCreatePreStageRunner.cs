using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;
using MCSUtilities2011;

namespace VA.TMP.CRM
{
    public class McsResourceGroupCreatePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceGroupCreatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        
        public override void Execute()
        {
            Logger.WriteDebugMessage("About to retrieve the derived name.");
            //Check Pre Name against format and update if needed
            var derivedName = CvtHelper.ReturnRecordNameIfChanged(PrimaryEntity.ToEntity<mcs_resourcegroup>(), true, Logger, OrganizationService);
            Logger.WriteDebugMessage("The derivedName came back, it is: " + derivedName.ToString());

            if (!String.IsNullOrEmpty(derivedName))
            {
                Logger.WriteDebugMessage(String.Format("The TSS Resource Group name should be different than {0}, updating it in the CreatePreStage to: {1}.", PrimaryEntity.Attributes["mcs_name"].ToString(), derivedName));
                PrimaryEntity.Attributes["mcs_name"] = (string)derivedName;
                Logger.WriteDebugMessage("New name as read from the PrimaryEntity: " + PrimaryEntity.Attributes["mcs_name"].ToString());
            }
            else
            {
                Logger.WriteDebugMessage("No change made to the name.  The TSS Resource Group name is already correct.");
            }
            
            Logger.WriteDebugMessage("End of PreStageCreate Execute method.");

        }

        #region Additional Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourcegroupplugin"; }
        }
        #endregion
    }
}