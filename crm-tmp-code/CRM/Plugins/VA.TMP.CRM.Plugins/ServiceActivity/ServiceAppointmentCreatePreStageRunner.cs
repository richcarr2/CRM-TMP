using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using System.Text.RegularExpressions;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentCreatePreStageRunner:PluginRunner
    {
        #region Constructor
        public ServiceAppointmentCreatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        { }

        #endregion

        #region Execute
        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != ServiceAppointment.EntityLogicalName)
                throw new Exception("Target entity is not of type ServiceAppointment");
            CheckVistaIENs(PrimaryEntity);
        }

        public void CheckVistaIENs(Entity PrimaryEntity)
        {
            Logger.setMethod = "CheckVistaIENs";
            Logger.WriteDebugMessage("Starting 'CheckVistaIENs' method.");
            if (PrimaryEntity.LogicalName.ToLower() != ServiceAppointment.EntityLogicalName.ToLower())
            {
                Logger.WriteToFile($"ERROR: Plugin is currently registered with entity {PrimaryEntity.LogicalName} i.e., different from Service Appointment");
                return;
            }

            //Review all the associated clinic resources for the appointment. Verify if any clinic do not have an IEN or have a alphanumeric IEN and throw the error message.
            if (PrimaryEntity.Attributes != null && PrimaryEntity.Attributes.Contains("resources"))
            {
                var resources = (EntityCollection)PrimaryEntity.Attributes["resources"];
                using (var srv = new Xrm(OrganizationService))
                {
                    foreach (Entity party in resources.Entities)
                    {
                        foreach (var resource in party.Attributes)
                        {
                            if (resource.Value.GetType() == typeof(EntityReference))
                            {
                                EntityReference reference = (EntityReference)resource.Value;
                                if (reference.LogicalName == Equipment.EntityLogicalName)
                                {
                                    var clinic = (from e in srv.EquipmentSet
                                                  join r in srv.mcs_resourceSet on e.mcs_relatedresource.Id equals r.mcs_resourceId.Value
                                                  where e.cvt_type.Value == 251920000 && e.EquipmentId.Value == reference.Id
                                                  select r).FirstOrDefault();
                                    if (clinic != null)
                                    {
                                        if (string.IsNullOrEmpty(clinic.cvt_ien) || !int.TryParse(clinic.cvt_ien, out var n))
                                        {
                                            throw new InvalidPluginExecutionException($"customThe appointment could not be saved as the Clinic associated to the appointment {clinic.mcs_name} does not have a valid IEN");
                                        }
                                    }
                                    else
                                    {
                                        Logger.WriteDebugMessage($"Skipping as the equipment {reference.Name}, Id: {reference.Id} is either not a clinic or the resource was not found.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Logger.WriteDebugMessage("Finished 'CheckVistaIENs' method.");
        }
        #endregion

        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }
        #endregion
    }
}
