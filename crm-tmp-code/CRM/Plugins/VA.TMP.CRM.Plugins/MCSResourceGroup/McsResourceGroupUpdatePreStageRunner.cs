using MCSShared;
using System;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsResourceGroupUpdatePreStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceGroupUpdatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        public override void Execute()
        {
            //Check Pre Name against format and update if needed
            var derivedName = CvtHelper.ReturnRecordNameIfChanged(PrimaryEntity.ToEntity<mcs_resourcegroup>(), false, Logger, OrganizationService);
            Logger.WriteDebugMessage("The derivedName came back, it is: " + derivedName.ToString());

            if (!String.IsNullOrEmpty(derivedName))
            {
                Logger.WriteDebugMessage("The TSS Resource Group name should be different, updating it in the UpdatePreStage: " + derivedName + ".");
                PrimaryEntity.Attributes["mcs_name"] = (string)derivedName;
                Logger.WriteDebugMessage("New name as read from the PrimaryEntity: " + PrimaryEntity.Attributes["mcs_name"].ToString());
            }
            else
            {
                Logger.WriteDebugMessage("The TSS Resource Group name should be same as PreStage, so make sure the name is not updated.");
                if (PrimaryEntity.Attributes.Contains("mcs_name"))
                    PrimaryEntity.Attributes.Remove("mcs_name");
            }
            
            Logger.WriteDebugMessage("End of PreStageUpdate Execute method.");
        }
        #endregion
       
        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourcegroupplugin"; }
        }
        #endregion
    }
}