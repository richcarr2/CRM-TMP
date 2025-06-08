using MCS.ApplicationInsights;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    internal class McsAppointmentCreatePreStageRunner : AILogicBase
    {
        private IServiceProvider serviceProvider;

        public McsAppointmentCreatePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override string McsSettingsDebugField
        {
            get { return "mcs_appointmentplugin"; }
        }

        public override void ExecuteLogic()
        {
            //Logger.WriteDebugMessage("Converting Entity to Appointment");
            //var appointment = PrimaryEntity.ToEntity<Appointment>();

            using (var srv = new Xrm(OrganizationService))
            {
                //Logger.WriteDebugMessage($"Appointment contains Optional Attendees: {PrimaryEntity.Contains("optionalattendees")}");
                Trace($"Appointment contains Optional Attendees: {PrimaryEntity.Contains("optionalattendees")}", LogLevel.Debug);
                if (PrimaryEntity.Contains("optionalattendees"))
                {
                    var optionalAttendeeCnt = 0;
                    try
                    {
                        optionalAttendeeCnt = PrimaryEntity.GetAttributeValue<EntityCollection>("optionalattendees").Entities.Count();

                    }
                    catch (Exception)
                    {
                        //Logger.WriteDebugMessage($"Issue trying to get a count");
                        Trace("Issue trying to get a count", LogLevel.Error);
                        return;
                    }
                  
                    if (optionalAttendeeCnt > 0)
                    {

                        //Logger.WriteDebugMessage($"Optional Attendees: {PrimaryEntity.GetAttributeValue<EntityCollection>("optionalattendees").Entities.Count}");
                        Trace($"Optional Attendees: {PrimaryEntity.GetAttributeValue<EntityCollection>("optionalattendees").Entities.Count}", LogLevel.Debug);
                        var optionalAttendee = PrimaryEntity.GetAttributeValue<EntityCollection>("optionalattendees").Entities.FirstOrDefault()?.ToEntity<ActivityParty>();

                        var patient = optionalAttendee != null && optionalAttendee.PartyId != null && !optionalAttendee.PartyId.Id.Equals(Guid.Empty)
                            ? srv.ContactSet.FirstOrDefault(c => c.Id == optionalAttendee.PartyId.Id)
                            : null;

                        //Logger.WriteDebugMessage("About to retrieve Search Text from Patient");
                        Trace("About to retrieve Search Text from Patient", LogLevel.Debug);
                        var searchText = patient != null && patient.Attributes.Contains("tmp_searchtext") ? patient["tmp_searchtext"] : "";

                        //Logger.WriteDebugMessage($"Search Text: {searchText}");
                        Trace($"Search Text: {searchText}", LogLevel.Debug);

                        PrimaryEntity["tmp_searchtext"] = searchText;
                    }
                }
            }
        }
    }
}