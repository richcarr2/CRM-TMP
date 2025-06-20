//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VA.TMP.DataModel
{
    /// <summary>
    /// Defines free/busy times for a service and for resources or resource groups, such as working, non-working, vacation, and blocked.
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("calendarrule")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.71")]
    public partial class CalendarRule : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public CalendarRule(): base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "calendarrule";
        public const string EntityLogicalCollectionName = "calendarrules";
        public const string EntitySetName = "calendarrules";
        public const int EntityTypeCode = 4004;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
        private void OnPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Unique identifier of the business unit with which the calendar rule is associated.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("businessunitid")]
        public System.Nullable<System.Guid> BusinessUnitId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("businessunitid");
            }
        }

        /// <summary>
        /// Unique identifier of the calendar with which the calendar rule is associated.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("calendarid")]
        public Microsoft.Xrm.Sdk.EntityReference CalendarId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("calendarid");
            }

            set
            {
                this.OnPropertyChanging("CalendarId");
                this.SetAttributeValue("calendarid", value);
                this.OnPropertyChanged("CalendarId");
            }
        }

        /// <summary>
        /// Unique identifier of the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("calendarruleid")]
        public System.Nullable<System.Guid> CalendarRuleId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("calendarruleid");
            }

            set
            {
                this.OnPropertyChanging("CalendarRuleId");
                this.SetAttributeValue("calendarruleid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }

                this.OnPropertyChanged("CalendarRuleId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("calendarruleid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                this.CalendarRuleId = value;
            }
        }

        /// <summary>
        /// Unique identifier of the user who created the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdby");
            }
        }

        /// <summary>
        /// Date and time when the calendar rule was created.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdon")]
        public System.Nullable<System.DateTime> CreatedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("createdon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who created the calendarrule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
            }
        }

        /// <summary>
        /// Defines free/busy times for a service and for resources or resource groups, such as working, non-working, vacation, and blocked.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("description")]
        public string Description
        {
            get
            {
                return this.GetAttributeValue<string>("description");
            }

            set
            {
                this.OnPropertyChanging("Description");
                this.SetAttributeValue("description", value);
                this.OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Duration of the calendar rule in minutes.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("duration")]
        public System.Nullable<int> Duration
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("duration");
            }

            set
            {
                this.OnPropertyChanging("Duration");
                this.SetAttributeValue("duration", value);
                this.OnPropertyChanged("Duration");
            }
        }

        /// <summary>
        /// Effective interval end of the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("effectiveintervalend")]
        public System.Nullable<System.DateTime> EffectiveIntervalEnd
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("effectiveintervalend");
            }

            set
            {
                this.OnPropertyChanging("EffectiveIntervalEnd");
                this.SetAttributeValue("effectiveintervalend", value);
                this.OnPropertyChanged("EffectiveIntervalEnd");
            }
        }

        /// <summary>
        /// Effective interval start of the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("effectiveintervalstart")]
        public System.Nullable<System.DateTime> EffectiveIntervalStart
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("effectiveintervalstart");
            }

            set
            {
                this.OnPropertyChanging("EffectiveIntervalStart");
                this.SetAttributeValue("effectiveintervalstart", value);
                this.OnPropertyChanged("EffectiveIntervalStart");
            }
        }

        /// <summary>
        /// Effort available for a resource during the time described by the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("effort")]
        public System.Nullable<double> Effort
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<double>>("effort");
            }

            set
            {
                this.OnPropertyChanging("Effort");
                this.SetAttributeValue("effort", value);
                this.OnPropertyChanged("Effort");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("endtime")]
        public System.Nullable<System.DateTime> EndTime
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("endtime");
            }

            set
            {
                this.OnPropertyChanging("EndTime");
                this.SetAttributeValue("endtime", value);
                this.OnPropertyChanged("EndTime");
            }
        }

        /// <summary>
        /// Extent of the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("extentcode")]
        public System.Nullable<int> ExtentCode
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("extentcode");
            }

            set
            {
                this.OnPropertyChanging("ExtentCode");
                this.SetAttributeValue("extentcode", value);
                this.OnPropertyChanged("ExtentCode");
            }
        }

        /// <summary>
        /// Unique identifier of the group.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("groupdesignator")]
        public string GroupDesignator
        {
            get
            {
                return this.GetAttributeValue<string>("groupdesignator");
            }

            set
            {
                this.OnPropertyChanging("GroupDesignator");
                this.SetAttributeValue("groupdesignator", value);
                this.OnPropertyChanged("GroupDesignator");
            }
        }

        /// <summary>
        /// Unique identifier of the inner calendar for non-leaf calendar rules.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("innercalendarid")]
        public Microsoft.Xrm.Sdk.EntityReference InnerCalendarId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("innercalendarid");
            }

            set
            {
                this.OnPropertyChanging("InnerCalendarId");
                this.SetAttributeValue("innercalendarid", value);
                this.OnPropertyChanged("InnerCalendarId");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ismodified")]
        public System.Nullable<bool> IsModified
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>("ismodified");
            }

            set
            {
                this.OnPropertyChanging("IsModified");
                this.SetAttributeValue("ismodified", value);
                this.OnPropertyChanged("IsModified");
            }
        }

        /// <summary>
        /// Flag used in vary-by-day calendar rules.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("isselected")]
        public System.Nullable<bool> IsSelected
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>("isselected");
            }

            set
            {
                this.OnPropertyChanging("IsSelected");
                this.SetAttributeValue("isselected", value);
                this.OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// Flag used in vary-by-day calendar rules.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("issimple")]
        public System.Nullable<bool> IsSimple
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>("issimple");
            }

            set
            {
                this.OnPropertyChanging("IsSimple");
                this.SetAttributeValue("issimple", value);
                this.OnPropertyChanged("IsSimple");
            }
        }

        /// <summary>
        /// Flag used in leaf nonrecurring rules.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("isvaried")]
        public System.Nullable<bool> IsVaried
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>("isvaried");
            }

            set
            {
                this.OnPropertyChanging("IsVaried");
                this.SetAttributeValue("isvaried", value);
                this.OnPropertyChanged("IsVaried");
            }
        }

        /// <summary>
        /// Unique identifier of the user who last modified the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedby");
            }
        }

        /// <summary>
        /// Date and time when the calendar rule was last modified.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedon")]
        public System.Nullable<System.DateTime> ModifiedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("modifiedon");
            }
        }

        /// <summary>
        /// Unique identifier of the delegate user who last modified the calendarrule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedonbehalfby");
            }
        }

        /// <summary>
        /// Name of the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("name")]
        public string Name
        {
            get
            {
                return this.GetAttributeValue<string>("name");
            }

            set
            {
                this.OnPropertyChanging("Name");
                this.SetAttributeValue("name", value);
                this.OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Start offset for leaf nonrecurring rules.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("offset")]
        public System.Nullable<int> Offset
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("offset");
            }

            set
            {
                this.OnPropertyChanging("Offset");
                this.SetAttributeValue("offset", value);
                this.OnPropertyChanged("Offset");
            }
        }

        /// <summary>
        /// Unique identifier of the organization with which the calendar rule is associated.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("organizationid")]
        public System.Nullable<System.Guid> OrganizationId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("organizationid");
            }
        }

        /// <summary>
        /// Pattern of the rule recurrence.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("pattern")]
        public string Pattern
        {
            get
            {
                return this.GetAttributeValue<string>("pattern");
            }

            set
            {
                this.OnPropertyChanging("Pattern");
                this.SetAttributeValue("pattern", value);
                this.OnPropertyChanged("Pattern");
            }
        }

        /// <summary>
        /// Rank of the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("rank")]
        public System.Nullable<int> Rank
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("rank");
            }

            set
            {
                this.OnPropertyChanging("Rank");
                this.SetAttributeValue("rank", value);
                this.OnPropertyChanged("Rank");
            }
        }

        /// <summary>
        /// service_calendar_rules
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("serviceid")]
        public Microsoft.Xrm.Sdk.EntityReference ServiceId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("serviceid");
            }

            set
            {
                this.OnPropertyChanging("ServiceId");
                this.SetAttributeValue("serviceid", value);
                this.OnPropertyChanged("ServiceId");
            }
        }

        /// <summary>
        /// Start time for the rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("starttime")]
        public System.Nullable<System.DateTime> StartTime
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("starttime");
            }

            set
            {
                this.OnPropertyChanging("StartTime");
                this.SetAttributeValue("starttime", value);
                this.OnPropertyChanged("StartTime");
            }
        }

        /// <summary>
        /// Sub-type of calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("subcode")]
        public System.Nullable<int> SubCode
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("subcode");
            }

            set
            {
                this.OnPropertyChanging("SubCode");
                this.SetAttributeValue("subcode", value);
                this.OnPropertyChanged("SubCode");
            }
        }

        /// <summary>
        /// Type of calendar rule such as working hours, break, holiday, or time off.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("timecode")]
        public System.Nullable<int> TimeCode
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("timecode");
            }

            set
            {
                this.OnPropertyChanging("TimeCode");
                this.SetAttributeValue("timecode", value);
                this.OnPropertyChanged("TimeCode");
            }
        }

        /// <summary>
        /// Local time zone for the calendar rule.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("timezonecode")]
        public System.Nullable<int> TimeZoneCode
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("timezonecode");
            }

            set
            {
                this.OnPropertyChanging("TimeZoneCode");
                this.SetAttributeValue("timezonecode", value);
                this.OnPropertyChanged("TimeZoneCode");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("versionnumber")]
        public System.Nullable<long> VersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
            }
        }

        /// <summary>
        /// 1:N userentityinstancedata_calendarrule
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("userentityinstancedata_calendarrule")]
        public System.Collections.Generic.IEnumerable<UserEntityInstanceData> UserEntityInstanceData_ObjectId
        {
            get
            {
                return this.GetRelatedEntities<UserEntityInstanceData>("userentityinstancedata_calendarrule", null);
            }

            set
            {
                this.OnPropertyChanging("UserEntityInstanceData_ObjectId");
                this.SetRelatedEntities<UserEntityInstanceData>("userentityinstancedata_calendarrule", null, value);
                this.OnPropertyChanged("UserEntityInstanceData_ObjectId");
            }
        }

        /// <summary>
        /// N:1 calendar_calendar_rules
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("calendarid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("calendar_calendar_rules")]
        public Calendar calendar_calendar_rules
        {
            get
            {
                return this.GetRelatedEntity<Calendar>("calendar_calendar_rules", null);
            }

            set
            {
                this.OnPropertyChanging("calendar_calendar_rules");
                this.SetRelatedEntity<Calendar>("calendar_calendar_rules", null, value);
                this.OnPropertyChanged("calendar_calendar_rules");
            }
        }

        /// <summary>
        /// N:1 inner_calendar_calendar_rules
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("innercalendarid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("inner_calendar_calendar_rules")]
        public Calendar inner_calendar_calendar_rules
        {
            get
            {
                return this.GetRelatedEntity<Calendar>("inner_calendar_calendar_rules", null);
            }

            set
            {
                this.OnPropertyChanging("inner_calendar_calendar_rules");
                this.SetRelatedEntity<Calendar>("inner_calendar_calendar_rules", null, value);
                this.OnPropertyChanged("inner_calendar_calendar_rules");
            }
        }

        /// <summary>
        /// N:1 lk_calendarrule_createdby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_calendarrule_createdby")]
        public SystemUser lk_calendarrule_createdby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_calendarrule_createdby", null);
            }
        }

        /// <summary>
        /// N:1 lk_calendarrule_createdonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_calendarrule_createdonbehalfby")]
        public SystemUser lk_calendarrule_createdonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_calendarrule_createdonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 lk_calendarrule_modifiedby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_calendarrule_modifiedby")]
        public SystemUser lk_calendarrule_modifiedby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_calendarrule_modifiedby", null);
            }
        }

        /// <summary>
        /// N:1 lk_calendarrule_modifiedonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_calendarrule_modifiedonbehalfby")]
        public SystemUser lk_calendarrule_modifiedonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_calendarrule_modifiedonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 service_calendar_rules
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("serviceid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("service_calendar_rules")]
        public Service service_calendar_rules
        {
            get
            {
                return this.GetRelatedEntity<Service>("service_calendar_rules", null);
            }

            set
            {
                this.OnPropertyChanging("service_calendar_rules");
                this.SetRelatedEntity<Service>("service_calendar_rules", null, value);
                this.OnPropertyChanged("service_calendar_rules");
            }
        }
    }
}