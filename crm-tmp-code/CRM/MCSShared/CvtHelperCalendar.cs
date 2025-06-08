using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;

namespace MCSShared
{
    public static partial class CvtHelper
    {

        #region Calendar Methods
        //FROM CREATE
        /// <summary>
        /// This method is used to update the calendar for the equipment record passed in.  It uses a combination of the fields passed in and the values from the default calendar.  
        /// </summary>
        /// <param name="sysResourceId">The ID of the equipment record corresponding to the calendar to be updated</param>
        /// <param name="OrganizationService">The OrganizationService from the plugin execution context</param>
        /// <param name="Logger">The Logger from the plugin execution context</param>
        /// <param name="McsSettings">The McsSettings from the plugin execution context</param>
        internal static void ChangeNewlyCreatedCalendar(Guid sysResourceId, IOrganizationService OrganizationService, MCSLogger Logger, MCSSettings McsSettings)
        {
            Logger.setMethod = "ChangeNewlyCreatedCalendar";
            Logger.WriteDebugMessage("starting ChangeNewlyCreatedCalendar");
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    var newTimeCode = 0;
                    var i = 0;
                    var thisEquipment = srv.EquipmentSet.FirstOrDefault(e => e.Id == sysResourceId);

                    if (thisEquipment == null)
                        return;
                    Logger.WriteDebugMessage("Equipment name is " + thisEquipment.Name);
                    var defaultEquipmentfromSettings = McsSettings.GetSingleSetting("mcs_defaultcalendar", "entityreference");
                    Logger.WriteDebugMessage("Got EquipmentId");
                    var defaultEquipmentId = new Guid(defaultEquipmentfromSettings);

                    var defaultEquipment = srv.EquipmentSet.FirstOrDefault(u => u.EquipmentId == defaultEquipmentId);
                    if (defaultEquipment == null)
                        return;

                    Logger.WriteDebugMessage("Got Default Equipment");
                    Entity defaultCalendarEntity = OrganizationService.Retrieve("calendar", defaultEquipment.CalendarId.Id, new ColumnSet(true));
                    Logger.WriteDebugMessage("Got default Calendar");
                    Entity thisCalendarEntity = OrganizationService.Retrieve("calendar", thisEquipment.CalendarId.Id, new ColumnSet(true));
                    Logger.WriteDebugMessage("Got This Calendar");

                    // Retrieve the calendar rules defined in the calendar
                    EntityCollection thiscalendarRules = (EntityCollection)thisCalendarEntity.Attributes["calendarrules"];
                    EntityCollection defaultCalendarRules = (EntityCollection)defaultCalendarEntity.Attributes["calendarrules"];

                    foreach (CalendarRule defaultCalRule in defaultCalendarRules.Entities)
                    {
                        Logger.WriteDebugMessage("Starting new Loop: " + i);
                        Entity defaultInnerCalendarEntity = OrganizationService.Retrieve("calendar", defaultCalRule.InnerCalendarId.Id, new ColumnSet(true));
                        EntityCollection defaultInnerCalendarRules = (EntityCollection)defaultInnerCalendarEntity.Attributes["calendarrules"];

                        if (thiscalendarRules.Entities.Count >= (i + 1))
                        {
                            Logger.WriteDebugMessage("Creating new calendar:" + i);
                            CalendarRule newRule = (thiscalendarRules.Entities[i]).ToEntity<CalendarRule>();
                            newRule.Duration = defaultCalRule.Duration;
                            newRule.EffectiveIntervalEnd = defaultCalRule.EffectiveIntervalEnd;
                            newRule.EffectiveIntervalStart = defaultCalRule.EffectiveIntervalStart;
                            newRule.StartTime = defaultCalRule.StartTime;
                            newRule.Pattern = defaultCalRule.Pattern;

                            ///Adding Effort
                            if (thisEquipment.cvt_type.Value == 251920000 && thisEquipment.cvt_capacity != null)
                                newRule.Effort = thisEquipment.cvt_capacity.Value;

                            newTimeCode = newRule.TimeZoneCode.Value;

                            if (i == 0)
                            {
                                Logger.WriteDebugMessage("First Loop");
                                if (defaultCalRule.InnerCalendarId != null)
                                {
                                    CalendarRule defaultInnerCalRule = new CalendarRule();
                                    if ((defaultInnerCalendarRules.Entities != null) && (defaultInnerCalendarRules.Entities.Count > 0))
                                    {
                                        defaultInnerCalRule = (defaultInnerCalendarRules.Entities[0]).ToEntity<CalendarRule>();
                                    }
                                    else
                                    {
                                        Logger.WriteDebugMessage("Could not find the Default Calendar's Inner Calendar Rule. Ref: CvtHelper.ChangeNewlyCreatedCalendar");
                                        break;
                                    }
                                    Entity thisInnerCalendarEntity = OrganizationService.Retrieve("calendar", newRule.InnerCalendarId.Id, new ColumnSet(true));
                                    EntityCollection thisInnerCalendarRules = (EntityCollection)thisInnerCalendarEntity.Attributes["calendarrules"];
                                    CalendarRule thisInnerCalRule = new CalendarRule();

                                    //there is always 1 and only 1 inner calendar, but there may be many more than 1 outer calendar. 
                                    //Update: There are instances where there isn't 1 inner calendar. Need to check
                                    if ((thisInnerCalendarRules.Entities != null) && (thisInnerCalendarRules.Entities.Count > 0))
                                        thisInnerCalRule = (thisInnerCalendarRules.Entities[0]).ToEntity<CalendarRule>();

                                    else //Figure out what the other properties of thisInnterCalendarRules' CalendarRule, and set them.
                                    {
                                        thisInnerCalRule = defaultInnerCalRule;
                                        thisInnerCalRule.CalendarId.Id = thisInnerCalendarEntity.Id;
                                    }

                                    if (thisEquipment.cvt_type.Value == 251920000 && thisEquipment.cvt_capacity != null)
                                        thisInnerCalRule.Effort = thisEquipment.cvt_capacity.Value;
                                    else
                                        thisInnerCalRule.Effort = defaultInnerCalRule.Effort;

                                    thisInnerCalRule.Offset = defaultInnerCalRule.Offset;
                                    thisInnerCalRule.Duration = defaultInnerCalRule.Duration;

                                    if (thisInnerCalRule.TimeZoneCode == null)
                                        thisInnerCalRule.TimeZoneCode = newTimeCode;
                                    if (thisInnerCalRule.Rank == null)
                                        thisInnerCalRule.Rank = defaultInnerCalRule.Rank == null ? 0 : defaultInnerCalRule.Rank;

                                    if (thisInnerCalRule.InnerCalendarId == null)
                                        thisInnerCalRule.IsSimple = true;
                                    else
                                        thisInnerCalRule.IsSimple = null;

                                    Logger.WriteDebugMessage(String.Format("Duration: {0}, Rank: {1}, Time Zone: {2}", thisInnerCalRule.Duration, thisInnerCalRule.Rank, thisInnerCalRule.TimeZoneCode));
                                    OrganizationService.Update(thisInnerCalendarEntity);
                                    Logger.WriteDebugMessage("Updating Calendar");
                                }
                            }
                        }
                        else
                        {
                            //this means that the default calendar has more than 1 calendar entry.
                            if (defaultCalRule.Name == "Holiday Closure Link" || defaultCalRule.Description == "Holiday Rule")
                            {
                                Logger.WriteDebugMessage("This is the Business Closure, skip creating a Calendar Rule for this as it is a shared Calendar - isShared == 1");
                                break;
                            }
                            else
                            {
                                Logger.WriteDebugMessage("New Calendar, not Holiday Closure Link: " + defaultCalRule.Name);
                                // Create a new  calendar
                                Entity newCalendar = new Entity("calendar");
                                newCalendar.Attributes["businessunitid"] = new EntityReference("businessunit", ((Microsoft.Xrm.Sdk.EntityReference)(thisCalendarEntity["businessunitid"])).Id);
                                newCalendar.Attributes["description"] = defaultCalendarEntity["description"];
                                Guid innerCalendarId = OrganizationService.Create(newCalendar);

                                // Create a new calendar rule and assign the inner calendar id to it
                                CalendarRule calendarRule = new CalendarRule();
                                calendarRule.ExtentCode = defaultCalRule.ExtentCode;
                                calendarRule.StartTime = defaultCalRule.StartTime;
                                calendarRule.TimeZoneCode = newTimeCode;
                                calendarRule.Pattern = defaultCalRule.Pattern;
                                calendarRule.Duration = defaultCalRule.Duration;
                                calendarRule.Rank = defaultCalRule.Rank;
                                calendarRule.InnerCalendarId = new EntityReference("calendar", innerCalendarId);

                                //Adding Effort
                                if (thisEquipment.cvt_type.Value == 251920000 && thisEquipment.cvt_capacity != null)
                                    calendarRule.Effort = thisEquipment.cvt_capacity.Value;

                                if (calendarRule.InnerCalendarId == null)
                                    calendarRule.IsSimple = true;
                                else
                                    calendarRule.IsSimple = null;

                                thiscalendarRules.Entities.Add(calendarRule);
                                Logger.WriteDebugMessage("Adding new innerCalendar");

                                if (defaultCalRule.InnerCalendarId != null)
                                {
                                    CalendarRule defaultInnerCalRule = new CalendarRule();
                                    //there is always 1 and only 1 inner calendar, but there may be many more than 1 outer calendar.
                                    if ((defaultInnerCalendarRules.Entities != null) && (defaultInnerCalendarRules.Entities.Count > 0))
                                    {
                                        defaultInnerCalRule = (defaultInnerCalendarRules.Entities[0]).ToEntity<CalendarRule>();
                                    }
                                    else
                                    {
                                        Logger.WriteDebugMessage("Could not find the Default Calendar's Inner Calendar Rule. Ref: CvtHelper.ChangeNewlyCreatedCalendar");
                                        break;
                                    }

                                    CalendarRule newRule = new CalendarRule();
                                    newRule.Description = "Default";
                                    newRule.Rank = defaultInnerCalRule.Rank;
                                    newRule.StartTime = defaultInnerCalRule.StartTime;
                                    newRule.Name = defaultInnerCalRule.Name;
                                    newRule.TimeZoneCode = newTimeCode;
                                    newRule.Offset = defaultInnerCalRule.Offset;
                                    newRule.ExtentCode = defaultInnerCalRule.ExtentCode;
                                    newRule.EffectiveIntervalEnd = defaultInnerCalRule.EffectiveIntervalEnd;
                                    newRule.IsSimple = defaultInnerCalRule.IsSimple;
                                    newRule.GroupDesignator = defaultInnerCalRule.GroupDesignator;
                                    newRule.Duration = defaultInnerCalRule.Duration;
                                    newRule.TimeCode = defaultInnerCalRule.TimeCode;
                                    newRule.SubCode = defaultInnerCalRule.SubCode;

                                    //Adding Effort
                                    if (thisEquipment.cvt_type.Value == 251920000 && thisEquipment.cvt_capacity != null)
                                        newRule.Effort = thisEquipment.cvt_capacity.Value;

                                    EntityCollection innerCalendarRules = new EntityCollection();
                                    innerCalendarRules.EntityName = "calendarrule";
                                    innerCalendarRules.Entities.Add(newRule);

                                    newCalendar.Attributes["calendarrules"] = innerCalendarRules;
                                    newCalendar.Attributes["calendarid"] = innerCalendarId;

                                    Logger.WriteDebugMessage("Updating Inner Service Calendar " + i);
                                    OrganizationService.Update(newCalendar);
                                    Logger.WriteDebugMessage("Service Update Inner");
                                }
                            }
                        }
                        i += 1;
                    }
                    //((EntityCollection)(thisCalendarEntity.Attributes["calendarrules"])).Entities.Count));
                    Logger.WriteDebugMessage(String.Format("Attempting to update calendar entity with {0} rules", ((EntityCollection)(thisCalendarEntity.Attributes["calendarrules"])).Entities.Count));
                    OrganizationService.Update(thisCalendarEntity);
                    Logger.WriteDebugMessage("Service Update Outer");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.WriteToFile(ex.Message);
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
            Logger.WriteDebugMessage("Ending ChangeNewlyCreatedCalendar");
        }

        #endregion
       
    }
}