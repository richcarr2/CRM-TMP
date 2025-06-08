using Microsoft.Xrm.Sdk;
using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    /// <summary>
    ///
    /// </summary>
    public class ImportFieldValidationUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public ImportFieldValidationUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        public override void Execute()
        {
            cvt_fieldmismatch importValidation = null;
            Logger.setMethod = "ImportFieldValidationUpdatePostStageRunner";
            Logger.WriteGranularTimingMessage("Starting ImportFieldValidationUpdatePostStageRunner");
            try
            {
                if (PluginExecutionContext.InputParameters.Contains("Target") &&
                    PluginExecutionContext.InputParameters["Target"] is Entity)
                {
                    var thisImportValidation = PrimaryEntity.ToEntity<cvt_fieldmismatch>();
                    if (PluginExecutionContext.Depth > 2 || thisImportValidation.cvt_valuechosen == null)
                    {
                        return;
                    }
                    importValidation = GetSecondaryEntity().ToEntity<cvt_fieldmismatch>();

                    if (thisImportValidation.cvt_valuechosen.Value == (int) cvt_fieldmismatchcvt_valuechosen.TMP) 
                    {
                        UpdateValueInTmp(importValidation);
                    }

                    var fieldMismatch = new cvt_fieldmismatch
                    {
                        Id = thisImportValidation.Id,
                        cvt_valuechosenon = DateTime.Now,
                        cvt_valuechosenby =
                            new EntityReference(SystemUser.EntityLogicalName, PluginExecutionContext.InitiatingUserId)
                    };
                    OrganizationService.Update(fieldMismatch);

                    DeactivateRecord(thisImportValidation);
                }
            }
            catch (Exception ex)
            {
                var message =
                    $"Import Field Validation Failed. Error occurredin the plugin for the record with Id: {importValidation?.Id}\nError: {ex.Message}\n{ex.StackTrace}";
                Logger.WriteToFile(message);
                throw new InvalidPluginExecutionException(message);
            }
            Logger.WriteGranularTimingMessage("Ending ImportFieldValidationUpdatePostStageRunner");
        }

        private void DeactivateRecord(cvt_fieldmismatch thisImportValidation)
        {
            try
            {
                Logger.setMethod = "DeactivateRecord";
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = thisImportValidation.Id,
                        LogicalName = cvt_fieldmismatch.EntityLogicalName
                    },
                    State = new OptionSetValue((int) cvt_fieldmismatchState.Inactive),
                    Status = new OptionSetValue((int) cvt_fieldmismatch_statuscode.Inactive)
                };
                OrganizationService.Execute(setStateRequest);
            }
            catch (Exception ex)
            {
                var message = $"Import Field Validation Failed. Error occurred while deactivating the Import field validation record with Id: {thisImportValidation.Id}\n{ex.Message}\n{ex.StackTrace}";
                Logger.WriteToFile(message);
                throw new InvalidPluginExecutionException(message);
            }
        }

        private void UpdateValueInTmp(cvt_fieldmismatch importValidation)
        {
            Entity updateEntity = null;

            Logger.setMethod = "UpdateValueInTmp";

            if (importValidation.cvt_entity == null)
            {
                var message =
                    $"Import Field Validation Failed. The Entity value does not exist for the field mismatch record : {importValidation.Id}";
                Logger.WriteToFile(message);
                throw new InvalidPluginExecutionException(message);
            }

            if (importValidation.cvt_entity.Value == (int) cvt_fieldmismatchcvt_entity.Resource &&
                (importValidation.cvt_resourceid == null || importValidation.cvt_stagingresource == null))
            {
                var message =
                    $"Import Field Validation Failed. The Resource/Staging Resource Lookup value does not exist for the field mismatch record : {importValidation.Id}";
                Logger.WriteToFile(message);
                throw new InvalidPluginExecutionException(message);
            }
            if (importValidation.cvt_entity.Value == (int) cvt_fieldmismatchcvt_entity.Component &&
                (importValidation.cvt_componentid == null || importValidation.cvt_stagingcomponentId == null))
            {
                var message =
                    $"Import Field Validation Failed. The Component/Staging Component Lookup value does not exist for the field mismatch record : {importValidation.Id}";
                Logger.WriteToFile(message);
                throw new InvalidPluginExecutionException(message);
            }

            try
            {
                var entity = (importValidation.cvt_entity?.Value == (int)cvt_fieldmismatchcvt_entity.Component)
                    ? importValidation.cvt_stagingcomponentId
                    : importValidation.cvt_stagingresource;
                var refEntity = (importValidation.cvt_entity?.Value == (int)cvt_fieldmismatchcvt_entity.Component)
                    ? importValidation.cvt_componentid
                    : importValidation.cvt_resourceid;

                if (entity == null)
                {
                    var message =
                        $"Import Field Validation Failed. Lookup value for the {importValidation.FormattedValues["cvt_entity"]} is not set for the record with Id: {importValidation?.Id}";
                    Logger.WriteToFile(message);
                    throw new InvalidPluginExecutionException(message);
                }

                updateEntity = new Entity
                {
                    LogicalName = entity.LogicalName,
                    Id = entity.Id,
                };

                updateEntity.Attributes.Add(importValidation.cvt_fieldschemaname,
                    GetAttributeValue(importValidation, refEntity?.LogicalName, refEntity.Id));

                OrganizationService.Update(updateEntity);
            }
            catch (Exception ex)
            {
                var message =
                    $"Import Field Validation Failed. Error occurred while updating the {importValidation.cvt_fieldschemaname} field for the entity {importValidation.FormattedValues["cvt_entity"]} record with Id: {importValidation?.Id}\nError: {ex.Message}\n{ex.StackTrace}";
                Logger.WriteToFile(message);
                throw new InvalidPluginExecutionException(message);
            }
        }

        private object GetAttributeValue(cvt_fieldmismatch importValidation, string entityLogicalName, Guid entityId)
        {
            Logger.setMethod = "GetAttributeValue";
            object attributeValue;

            if (importValidation.cvt_fieldschemaname != null)
            {
                var record = OrganizationService.Retrieve(entityLogicalName,
                    entityId, new ColumnSet(new[] {importValidation.cvt_fieldschemaname}));
                if (record == null)
                {
                    var message = $"Import Field Validation Failed. Associated {entityLogicalName} record with Id: {entityId} does not exist for the field mismatch record : {importValidation.Id}";
                    Logger.WriteToFile(message);
                    throw new InvalidPluginExecutionException(message);
                }

                if (record.Attributes.Contains(importValidation.cvt_fieldschemaname))
                {
                    attributeValue = record.Attributes[importValidation.cvt_fieldschemaname];
                }
                else
                {
                    var message = $"Import Field Validation Failed. Associated {entityLogicalName} record with Id: {entityId} does not exist for the field mismatch record : {importValidation.Id}";
                    Logger.WriteToFile(message);
                    throw new InvalidPluginExecutionException(message);
                }
            }
            else
            {
                var message = $"Import Field Validation Failed. Field Schema Name is not set for the field mismatch record : {importValidation.Id}";
                Logger.WriteToFile(message);
                throw new InvalidPluginExecutionException(message);
            }

            return attributeValue;
        }

        #region Additional Interface methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_fieldmismatchplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.InputParameters["Target"];
        }

        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PostEntityImages["post"];
        }
        #endregion
    }
}