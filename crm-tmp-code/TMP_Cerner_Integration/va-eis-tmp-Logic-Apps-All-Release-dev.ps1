az deployment group create --resource-group $(ResourceGroup) --name Rollout_$(Release.ReleaseId) --template-file _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-integration-acct.json --parameters _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-integration-acct-dev.parameters.json

az deployment group create --resource-group $(ResourceGroup) --name Rollout_$(Release.ReleaseId) --template-file _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-OAuthAuthenticator.json --parameters _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-OAuthAuthenticator-dev.parameters.json

az deployment group create --resource-group $(ResourceGroup) --name Rollout_$(Release.ReleaseId) --template-file _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-DataLakeAPIWrapper.json --parameters _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-DataLakeAPIWrapper-dev.parameters.json

az deployment group create --resource-group $(ResourceGroup) --name Rollout_$(Release.ReleaseId) --template-file _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-AzureManagementAPIWrapper.json --parameters _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-AzureManagementAPIWrapper-dev.parameters.json

az deployment group create --resource-group $(ResourceGroup) --name Rollout_$(Release.ReleaseId) --template-file _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-orchestrator.json --parameters _TMP_Cerner_Integration\ARMTemplates\va-eis-tmp-orchestrator-dev.parameters.json

az deployment group create --resource-group $(ResourceGroup) --name Rollout_$(Release.ReleaseId) --template-file _TMP_Cerner_Integration\ARMTemplates\veis-eis-tmp-inbound-worker.json --parameters _TMP_Cerner_Integration\ARMTemplates\veis-eis-tmp-inbound-worker-dev.parameters.json