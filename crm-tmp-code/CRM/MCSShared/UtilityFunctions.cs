using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace MCSUtilities2011
{
    public class UtilityFunctions
    {
        private IOrganizationService _service;
        public IOrganizationService setService
        {
            set => _service = value;
        }
        private MCSLogger _logger;
        public MCSLogger setlogger
        {
            set => _logger = value;
        }
        public int getOptionSetValue(string optionSetString, string entityName, string attributeName)
        {
            try
            {
                int returnInt = 0;
                RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityName,
                    LogicalName = attributeName,
                    // Retrieve only the currently published changes, ignoring the changes that have
                    // not been published.
                    RetrieveAsIfPublished = false
                };

                RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)_service.Execute(attributeRequest);

                // Access the retrieved attribute.
                PicklistAttributeMetadata retrievedAttributeMetadata = (PicklistAttributeMetadata)attributeResponse.AttributeMetadata;
                for (int i = 0; i < retrievedAttributeMetadata.OptionSet.Options.Count; i++)
                {
                    if (retrievedAttributeMetadata.OptionSet.Options[i].Label.LocalizedLabels[0].Label == optionSetString)
                    {
                        returnInt = retrievedAttributeMetadata.OptionSet.Options[i].Value.Value;
                        break;
                    }

                }
                return returnInt;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                _logger.setService = _service;
                _logger.setModule = "getOptionSetValue";
                _logger.WriteToFile(ex.Detail.Message);
                _logger.setModule = "execute";
                return 0;
            }
            catch (Exception ex)
            {
                _logger.setService = _service;
                _logger.setModule = "getOptionSetValue";
                _logger.WriteToFile(ex.Message);
                _logger.setModule = "execute";
                return 0;
            }
        }

        public void DeactivateRecord(Entity entity, int stateCode, int statusCode)
        {
            SetStateRequest setStateRequest = new SetStateRequest()
            {
                EntityMoniker = new EntityReference
                {
                    Id = entity.Id,
                    LogicalName = entity.LogicalName,
                },
                State = new OptionSetValue(stateCode),
                Status = new OptionSetValue(statusCode)
            };
            _service.Execute(setStateRequest);
        }

        public void DeactivateRecords(List<Entity> entities, int stateCode, int statusCode)
        {
            foreach (Entity entity in entities)
            {
                DeactivateRecord(entity, stateCode, statusCode);
            }

        }

        public string getOptionSetString(int optionSetValue, string entityName, string attributeName)
        {
            try
            {

                string optionSetString = string.Empty;

                RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityName,
                    LogicalName = attributeName,
                    // Retrieve only the currently published changes, ignoring the changes that have
                    // not been published.
                    RetrieveAsIfPublished = true
                };

                RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)_service.Execute(attributeRequest);

                // Access the retrieved attribute.
                PicklistAttributeMetadata retrievedAttributeMetadata = (PicklistAttributeMetadata)attributeResponse.AttributeMetadata;
                for (int i = 0; i < retrievedAttributeMetadata.OptionSet.Options.Count; i++)
                {
                    if (retrievedAttributeMetadata.OptionSet.Options[i].Value == optionSetValue)
                    {
                        optionSetString = retrievedAttributeMetadata.OptionSet.Options[i].Label.LocalizedLabels[0].Label;
                        break;
                    }

                }
                return optionSetString;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                _logger.setService = _service;
                _logger.setModule = "getOptionSetString";
                _logger.WriteToFile(ex.Detail.Message);
                _logger.setModule = "execute";
                return null;
            }
            catch (Exception ex)
            {
                _logger.setService = _service;
                _logger.setModule = "getOptionSetString";
                _logger.WriteToFile(ex.Message);
                _logger.setModule = "execute";
                return null;
            }
        }
    }
}
