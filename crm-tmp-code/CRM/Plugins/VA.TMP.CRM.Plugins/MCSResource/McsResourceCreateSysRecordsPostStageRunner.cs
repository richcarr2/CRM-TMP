using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsResourceCreateSysRecordsPostStageRunner : PluginRunner
    {
        #region Constructor
        public McsResourceCreateSysRecordsPostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods/Properties
        /// <summary>
        /// Create the system service records that will be required for scheduling. 
        /// </summary>
        public override void Execute()
        {
            //This plugin runs after the mcs_resource has been created.
            //
            // It accomplishes the following:
            //1. Create the system service records that will be required for scheduling. 

            var resourcespecguid = McsHelper.getStringValue("mcs_resourcespecguid");                    
            if (resourcespecguid == null)
            {
                //Create the system service records
                Logger.WriteGranularTimingMessage("Starting CreateSysResource");
                CreateSystemRecords(PluginExecutionContext.PrimaryEntityId);
                Logger.WriteGranularTimingMessage("Ending CreateSysResource");
            }
            var relatedEquip = McsHelper.getEntRefID("mcs_relatedresourceid");
            var updateCalendar = McsHelper.getBoolValue("cvt_updatecalendar");
            if ((relatedEquip != null) && (updateCalendar == true))
            {
                Logger.WriteGranularTimingMessage("Starting Update System Record Calendar");
                CvtHelper.ChangeNewlyCreatedCalendar(relatedEquip, OrganizationService, Logger, McsSettings);
                mcs_resource updateResource = new mcs_resource()
                {
                    cvt_updatecalendar = false,
                    Id = PluginExecutionContext.PrimaryEntityId
                };
                OrganizationService.Update(updateResource);

                Logger.WriteGranularTimingMessage("Ending Update System Record Calendar");
            }
        }


        internal void CreateSystemRecords(Guid thisId)
        {
            //Starting the creation of a System Service Records which will be associated with target MCS Resource. 
            Logger.WriteDebugMessage("starting CreateSystemRecords");

            var whereami = "top";

            try
            {
                using (var srv = new Xrm(OrganizationService))
                {
                    Logger.setMethod = "CreateSystemRecords";

                    //build the top half of the constraints xml
                    var builder = new System.Text.StringBuilder("<Constraints>");
                    builder.Append("<Constraint>");
                    builder.Append("<Expression>");
                    builder.Append("<Body>resource[\"Id\"] == ");
                    
                    var resource = srv.mcs_resourceSet.FirstOrDefault(i => i.Id == thisId);
                    if (resource == null) return;
                    Logger.WriteDebugMessage("got resource");
                    builder.Append(resource.mcs_relatedResourceId.Id.ToString("B"));
                        
                    builder.Append("</Body>");
                    builder.Append("<Parameters>");
                    builder.Append("<Parameter name=\"resource\" />");
                    builder.Append("</Parameters>");
                    builder.Append("</Expression>");
                    builder.Append("</Constraint>");
                    builder.Append("</Constraints>");
                    // Define an anonymous type to define the possible constraint based group type code values.
                    var constraintBasedGroupTypeCode = new
                    {
                        Static = 0,
                        Dynamic = 1,
                        Implicit = 2
                    };
                    //we need the user for the business unit
                    Logger.WriteDebugMessage("About to get User");
                    var systemUser = srv.SystemUserSet.FirstOrDefault(i => i.Id == PluginExecutionContext.InitiatingUserId);

                    if (systemUser == null) return;
                    Logger.WriteDebugMessage("got user");

                    var group = new ConstraintBasedGroup
                    {
                        BusinessUnitId = systemUser.BusinessUnitId,
                        Constraints = builder.ToString(),
                        Name = "Selection Rule:" + McsHelper.getStringValue("mcs_name"),
                        GroupTypeCode = new OptionSetValue(constraintBasedGroupTypeCode.Implicit),

                    };

                    var newSysResource = OrganizationService.Create(group);
                    whereami = "created";

                    //now create the resource spec record
                    var spec = new ResourceSpec
                    {
                        BusinessUnitId = systemUser.BusinessUnitId,
                        ObjectiveExpression = @"<Expression><Body>udf ""Random""(factory,resource,appointment,request,leftoffset,rightoffset)</Body><Parameters><Parameter name=""factory"" /><Parameter name=""resource"" /><Parameter name=""appointment"" /><Parameter name=""request"" /><Parameter name=""leftoffset"" /><Parameter name=""rightoffset"" /></Parameters><Properties EvaluationInterval=""P0D"" evaluationcost=""0"" /></Expression>",
                        RequiredCount = 1,
                        Name = "Selection Rule:" + McsHelper.getStringValue("mcs_name"),
                        GroupObjectId = newSysResource,
                        SameSite = true
                    };
                    var _specId = OrganizationService.Create(spec);

                    //update the pat group resource so we can use these values when we create the service.
                    Logger.setMethod = "Update Resource";
                    Logger.WriteDebugMessage("About to update Resource");
                    var updateResource = new Entity("mcs_resource") { Id = thisId };

                    //can't create relationships to these two, so we have to just store the guids in text fields.
                    updateResource["mcs_resourcespecguid"] = _specId.ToString();
                    updateResource["mcs_constraintgroupguid"] = newSysResource.ToString();
                    OrganizationService.Update(updateResource);
                    Logger.WriteDebugMessage("Resource Updated");

                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(whereami + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.WriteToFile(whereami + ex.Message);
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
                           
        public override string McsSettingsDebugField
        {
            get { return "mcs_resourceplugin"; }
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
