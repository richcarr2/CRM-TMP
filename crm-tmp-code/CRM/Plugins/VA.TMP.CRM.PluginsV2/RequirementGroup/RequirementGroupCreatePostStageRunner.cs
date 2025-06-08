using MCSShared;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Activities.Expressions;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    public class RequirementGroupCreatePostStageRunner : PluginRunner
    {
        public RequirementGroupCreatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider){}
        /// <summary>
        /// Entry Point for Resource Package Pre Create Plugin Runner
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public new void RunPlugin(IServiceProvider serviceProvider) { base.RunPlugin(serviceProvider); }

        /// <summary>
        /// Ensure the name is correct
        /// </summary>
        public override void Execute()
        {
            Logger.WriteDebugMessage("About to retrieve the isTemplate.");
            TracingService?.Trace("About to retrieve the isTemplate.");   
            var isTemplate = (bool)PrimaryEntity["msdyn_istemplate"];
            if (isTemplate) return;
            TracingService?.Trace("PrimaryEntity.Id." + PrimaryEntity.Id);

            var fetchData = new
            {
                msdyn_requirementgroupid = PrimaryEntity.Id,
                msdyn_name = "Veteran"
            };
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""50"">
                      <entity name=""msdyn_requirementrelationship"">
                        <filter>
                          <condition attribute=""msdyn_requirementgroupid"" operator=""eq"" value=""{fetchData.msdyn_requirementgroupid/*52074255-141a-f011-998a-001dd80b121d*/}"" />
                          <condition attribute=""msdyn_name"" operator=""eq"" value=""{fetchData.msdyn_name/*Veteran*/}"" />
                        </filter>
                      </entity>
                    </fetch>";

            
            
            var relRecords = OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

            if (relRecords.Entities.Count > 0)
            {
                TracingService?.Trace("Found the Veteran Requirement");

                var newRecordId = Guid.Empty;
                var newBR = new Guid("73c3e002-0a1a-f011-998a-001dd80b121d");
                var requirement = CreateRequirement(new EntityReference("msdyn_requirementgroup", PrimaryEntity.Id), relRecords.Entities[0].ToEntityReference(), "Veteran Name", new Guid("f1f20cae-4a76-44eb-be6d-db346ba57013"), 1);
                var preference = CreatePreference(new EntityReference("bookableresource", newBR), requirement, "Veteran Name preference");

            }
            else
            {
                TracingService?.Trace("Did not find the Veteran Requirement");
            }
        }
        private EntityReference CreatePreference(EntityReference bookableResource, EntityReference resourceRequirement, string resourceRequirementName)
        {
            var rrPrefs = new Entity();
            rrPrefs.LogicalName = "msdyn_requirementresourcepreference";
            rrPrefs["msdyn_name"] = $"{resourceRequirementName} preference";
            rrPrefs["msdyn_preferencetype"] = new OptionSetValue(690970002);
            rrPrefs["msdyn_resourcerequirement"] = resourceRequirement;
            rrPrefs["msdyn_bookableresource"] = bookableResource;

            var resPref = OrganizationService.Create(rrPrefs);
            TracingService?.Trace("Created the Requirement Preference");

            return new EntityReference("msdyn_requirementresourcepreference", resPref);
        }
        private EntityReference CreateRequirement(EntityReference requirementGroup, EntityReference relationEr, string recordName, Guid activeStatus, int count)
        {
            var rr = new Entity();
            rr.LogicalName = "msdyn_resourcerequirement";
            rr["msdyn_requirementgroupid"] = requirementGroup;
            rr["msdyn_requirementrelationshipid"] = relationEr;
            rr["msdyn_name"]= recordName;
            rr["msdyn_effort"] = Convert.ToDecimal(1);
            rr["msdyn_status"] = new EntityReference("msdyn_requirementstatus", activeStatus);
            rr["msdyn_requirementgroupcontrolviewid"] = count.ToString();


            var rrId = OrganizationService.Create(rr);
            TracingService?.Trace("Created the Requirement");




            return new EntityReference("msdyn_resourcerequirement", rrId);
        }


        #region Additional Interface Methods/Properties
        /// <summary>
        /// Used for Debugging - turns on or off creation of log records for this particular entity
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_ppereview"; }
        }
        #endregion
    }
}