using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using MCSShared;
using System.Data;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace VA.TMP.CRM
{
    public class ResourceRequirementUpdatePostStageRunner : PluginRunner
    {
        public ResourceRequirementUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public override void Execute()
        {
            if (PrimaryEntity.Contains("msdyn_serviceappointment"))
            {
                TracingService?.Trace("Appt is on the update");
                var preImage = GetSecondaryEntity();
                if (preImage == null)
                {
                    TracingService?.Trace("Pre Image is null");
                    return;
                }
                if (preImage.Contains("msdyn_name"))
                {
                    TracingService?.Trace("Pre Image has msdyn_name");
                    var nameofrequirement = preImage["msdyn_name"] as string;

                    if (nameofrequirement == "Veteran Name")
                    {
                        TracingService?.Trace("Found the Veteran Name, id:"+ PrimaryEntity.Id);
                    }
                    else
                    {
                        TracingService?.Trace("Did not find the Veteran Name");
                        return;
                    }
                }
                else
                {
                    TracingService?.Trace("Pre Image does not have msdyn_name");
                    return;
                }
                
                var serviceAppt = PrimaryEntity["msdyn_serviceappointment"] as EntityReference;

                var fetchData = new
                {
                    activityid = serviceAppt.Id,
                    participationtypemask = "11"
                };
                var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""50"">
                      <entity name=""serviceappointment"">
                        <filter>
                          <condition attribute=""activityid"" operator=""eq"" value=""{fetchData.activityid/*8a8c8b23-e11e-f011-998a-001dd803d745*/}"" />
                        </filter>
                        <link-entity name=""activityparty"" from=""activityid"" to=""activityid"" alias=""contact"" >
                          <attribute name=""partyid"" />
                          <filter>
                            <condition attribute=""participationtypemask"" operator=""eq"" value=""{fetchData.participationtypemask/*11*/}"" />
                          </filter>
                        </link-entity>
                      </entity>
                        </fetch>";

                var relRecords = OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

                if (relRecords.Entities.Count > 0)
                {
                    TracingService?.Trace("Found the Veteran");
                    var outValue = new AliasedValue();
                    relRecords.Entities[0].TryGetAttributeValue<AliasedValue>("contact.partyid", out outValue);
                    if (outValue != null)
                    {
                        var contactId = (EntityReference)relRecords.Entities[0].GetAttributeValue<AliasedValue>("contact.partyid").Value;
                        var fetchData2 = new
                        {
                            msdyn_resourcerequirement = PrimaryEntity.Id,
                            msdyn_bookableresource = "dd8416c6-67eb-e911-a987-001dd80081ad"
                        };
                        fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
                    <fetch top=""50"">
                      <entity name=""msdyn_requirementresourcepreference"">
                        <attribute name=""msdyn_bookableresource"" />
                        <attribute name=""msdyn_name"" />
                        <filter>
                          <condition attribute=""msdyn_resourcerequirement"" operator=""eq"" value=""{fetchData2.msdyn_resourcerequirement/*611d9229-e11e-f011-998a-001dd803d745*/}"" />
                          <condition attribute=""msdyn_bookableresource"" operator=""eq"" value=""{fetchData2.msdyn_bookableresource/*dd8416c6-67eb-e911-a987-001dd80081ad*/}"" />                        
                        </filter>
                      </entity>
                    </fetch>";

                        var relPrefRecords = OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));

                        if (relPrefRecords.Entities.Count > 0)
                        {
                            TracingService?.Trace("Found the Preference");


                            var newBR = relRecords.Entities[0].Id;

                            relPrefRecords.Entities[0].Attributes["msdyn_bookableresource"] = new EntityReference("bookableresource", contactId.Id);
                            OrganizationService.Update(relPrefRecords.Entities[0]);
                            TracingService?.Trace("updated the Preference");
                        }
                        else
                        {
                            TracingService?.Trace("Did not find the Preference");
                        }   
                    }
                }
                else
                {
                    TracingService?.Trace("no Veteran on this resourcerequirement");
                }
            }
        }
        /// <summary>
        /// Import Inventory Managment Records
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "mcs_serviceplugin"; }
        }
        public override Entity GetSecondaryEntity()
        {
            TracingService?.Trace("trying to get pre-imate");
            var images = PluginExecutionContext.PreEntityImages.Count.ToString();
            TracingService?.Trace("there are this many images:" + images);
            if (PluginExecutionContext.PreEntityImages.Contains("pre"))
            {
                TracingService?.Trace("found the pre-image");
            }
            else
            {
                TracingService?.Trace("did not find the pre-image");
                return null;
            }
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
    }
}
