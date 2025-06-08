using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Collections.Generic;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    /// <summary>
    /// </summary>
    public class MissingResourceActionPostStageRunner : PluginRunner
    {
        #region Constructor
        public MissingResourceActionPostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion
        #region Internal Methods/Properties
        public override void Execute()
        {

            using (var srv = new Xrm(OrganizationService))
            {
                Entity ThisRecord = CvtHelper.ValidateReturnRecord(PrimaryEntity, "mcs_missingresource", Logger, OrganizationService);

                //Set ActivityParty details based off the Missing Resource type
                int resourceType = ThisRecord.GetAttributeValue<OptionSetValue>("mcs_type").Value;
                Guid partyId = new Guid();
                String partyType = "";
                String participationType = "";
                switch (resourceType)
                {
                    //Missing Resource type is TMP Resource
                    case 803750000:
                        Guid resourceId = ThisRecord.GetAttributeValue<EntityReference>("mcs_tmpresource").Id;
                        Entity tmpResource = OrganizationService.Retrieve("mcs_resource", resourceId, new ColumnSet(true));
                        partyId = tmpResource.GetAttributeValue<EntityReference>("mcs_relatedresourceid").Id;
                        partyType = tmpResource.GetAttributeValue<EntityReference>("mcs_relatedresourceid").LogicalName;
                        participationType = "requiredattendees";
                        break;
                    //Missing Resource type is User (AKA Provider)
                    case 803750001:
                        partyId = ThisRecord.GetAttributeValue<EntityReference>("mcs_user").Id;
                        partyType = ThisRecord.GetAttributeValue<EntityReference>("mcs_user").LogicalName;
                        participationType = "requiredattendees";
                        break;
                    //Missing Resource type is Patient
                    case 803750002:
                        partyId = ThisRecord.GetAttributeValue<EntityReference>("mcs_patient").Id;
                        partyType = ThisRecord.GetAttributeValue<EntityReference>("mcs_patient").LogicalName;
                        participationType = "optionalattendees";
                        break;
                    default:
                        break;
                }

                //Retrieve all active Reserve Resource Missing Resources related to the Missing Resource
                QueryExpression query = new QueryExpression("mcs_reserveresourcemissingresource") { ColumnSet = new ColumnSet(true) };
                FilterExpression filter = new FilterExpression(LogicalOperator.And);
                filter.Conditions.Add(new ConditionExpression ("mcs_missingresource", ConditionOperator.Equal, PrimaryEntity.Id));
                filter.Conditions.Add(new ConditionExpression ("statuscode", ConditionOperator.Equal, 1));
                filter.Conditions.Add(new ConditionExpression ("statecode", ConditionOperator.Equal, 0));
                query.Criteria.AddFilter(filter);
                List<Entity> ReserveResourceMissingResources = OrganizationService.RetrieveMultiple(query).Entities.ToList();

                //For each Reserve Resource Missing Resource, update the related Reserve Resource with the appropriate missing resource
                foreach (Entity ReserveResourceMissingResource in ReserveResourceMissingResources)
                {
                    //Che
                    if (ReserveResourceMissingResource.GetAttributeValue<EntityReference>("mcs_reserveresource").Id != null)
                    {
                        var reserveResource = OrganizationService.Retrieve("appointment", ReserveResourceMissingResource.GetAttributeValue<EntityReference>("mcs_reserveresource").Id, new ColumnSet(true));
                        
                        var activityPartyList = reserveResource.GetAttributeValue<EntityCollection>(participationType);
                        //if (requiredAttendees.Entities.Where(attendee => attendee.GetAttributeValue<EntityReference>("partyid").Id == equipmentId).FirstOrDefault() == null)
                        Dictionary<Guid, Entity> requiredAttendeesDictionary = activityPartyList.Entities.ToDictionary(entity => entity.GetAttributeValue<EntityReference>("partyid").Id);
                        if (!requiredAttendeesDictionary.ContainsKey(partyId))
                        {
                            Entity party = new Entity("activityparty");
                            party["partyid"] = new EntityReference(partyType, partyId);
                            activityPartyList.Entities.Add(party);
                            reserveResource[participationType] = activityPartyList;
                            OrganizationService.Update(reserveResource);
                            //Deactivate the Reserve Resource Missing Resource
                            Utilities.DeactivateRecord(ReserveResourceMissingResource, 1, 2);
                        }
                    }  
                }
            }
        }
        #endregion
        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "mcs_missingresourceplugin"; }
        }
        #endregion
    }
}