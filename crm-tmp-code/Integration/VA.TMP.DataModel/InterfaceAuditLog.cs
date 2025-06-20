//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using VA.TMP.DataModel.OptionSets;

namespace VA.TMP.DataModel
{
    /// <summary>
    /// 
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("mcs_messageaudit")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.71")]
    public partial class InterfaceAuditLog : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public InterfaceAuditLog(): base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "mcs_messageaudit";
        public const string EntityLogicalCollectionName = "mcs_messageaudits";
        public const string EntitySetName = "mcs_messageaudits";
        public const int EntityTypeCode = 10025;
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
        /// Unique identifier of the user who created the record.
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
        /// Date and time when the record was created.
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
        /// Unique identifier of the delegate user who created the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference CreatedBy_Delegate
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
            }
        }

        /// <summary>
        /// Sequence number of the import that created this record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("importsequencenumber")]
        public System.Nullable<int> ImportSequenceNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("importsequencenumber");
            }

            set
            {
                this.OnPropertyChanging("ImportSequenceNumber");
                this.SetAttributeValue("importsequencenumber", value);
                this.OnPropertyChanged("ImportSequenceNumber");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_eventdatetime")]
        public string EventDatetime
        {
            get
            {
                return this.GetAttributeValue<string>("mcs_eventdatetime");
            }

            set
            {
                this.OnPropertyChanging("EventDatetime");
                this.SetAttributeValue("mcs_eventdatetime", value);
                this.OnPropertyChanged("EventDatetime");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_eventoutcome")]
        public System.Nullable<InterfaceAuditLog_EventOutcome> EventOutcome
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("mcs_eventoutcome");
                if ((optionSet != null))
                {
                    return ((InterfaceAuditLog_EventOutcome)(System.Enum.ToObject(typeof(InterfaceAuditLog_EventOutcome), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("EventOutcome");
                if ((value == null))
                {
                    this.SetAttributeValue("mcs_eventoutcome", null);
                }
                else
                {
                    this.SetAttributeValue("mcs_eventoutcome", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("EventOutcome");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_eventtype")]
        public System.Nullable<MessageEvent> EventType
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("mcs_eventtype");
                if ((optionSet != null))
                {
                    return ((MessageEvent)(System.Enum.ToObject(typeof(MessageEvent), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("EventType");
                if ((value == null))
                {
                    this.SetAttributeValue("mcs_eventtype", null);
                }
                else
                {
                    this.SetAttributeValue("mcs_eventtype", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("EventType");
            }
        }

        /// <summary>
        /// Unique identifier for entity instances
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_messageauditid")]
        public System.Nullable<System.Guid> MessageAuditId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("mcs_messageauditid");
            }

            set
            {
                this.OnPropertyChanging("MessageAuditId");
                this.SetAttributeValue("mcs_messageauditid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }

                this.OnPropertyChanged("MessageAuditId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_messageauditid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                this.MessageAuditId = value;
            }
        }

        /// <summary>
        /// Unique identifier for Message associated with Message Audit.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_messageid")]
        public Microsoft.Xrm.Sdk.EntityReference MessageID
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("mcs_messageid");
            }

            set
            {
                this.OnPropertyChanging("MessageID");
                this.SetAttributeValue("mcs_messageid", value);
                this.OnPropertyChanged("MessageID");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_messagestate")]
        public System.Nullable<MessageState> MessageState
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("mcs_messagestate");
                if ((optionSet != null))
                {
                    return ((MessageState)(System.Enum.ToObject(typeof(MessageState), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("MessageState");
                if ((value == null))
                {
                    this.SetAttributeValue("mcs_messagestate", null);
                }
                else
                {
                    this.SetAttributeValue("mcs_messagestate", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("MessageState");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_messagestatus")]
        public System.Nullable<MessageStatus> MessageStatus
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("mcs_messagestatus");
                if ((optionSet != null))
                {
                    return ((MessageStatus)(System.Enum.ToObject(typeof(MessageStatus), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("MessageStatus");
                if ((value == null))
                {
                    this.SetAttributeValue("mcs_messagestatus", null);
                }
                else
                {
                    this.SetAttributeValue("mcs_messagestatus", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("MessageStatus");
            }
        }

        /// <summary>
        /// The name of the custom entity.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_name")]
        public string Name
        {
            get
            {
                return this.GetAttributeValue<string>("mcs_name");
            }

            set
            {
                this.OnPropertyChanging("Name");
                this.SetAttributeValue("mcs_name", value);
                this.OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Unique identifier of the user who modified the record.
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
        /// Date and time when the record was modified.
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
        /// Unique identifier of the delegate user who modified the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        public Microsoft.Xrm.Sdk.EntityReference ModifiedBy_Delegate
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedonbehalfby");
            }
        }

        /// <summary>
        /// Date and time that the record was migrated.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("overriddencreatedon")]
        public System.Nullable<System.DateTime> RecordCreatedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("overriddencreatedon");
            }

            set
            {
                this.OnPropertyChanging("RecordCreatedOn");
                this.SetAttributeValue("overriddencreatedon", value);
                this.OnPropertyChanged("RecordCreatedOn");
            }
        }

        /// <summary>
        /// Owner Id
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ownerid")]
        public Microsoft.Xrm.Sdk.EntityReference Owner
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("ownerid");
            }

            set
            {
                this.OnPropertyChanging("Owner");
                this.SetAttributeValue("ownerid", value);
                this.OnPropertyChanged("Owner");
            }
        }

        /// <summary>
        /// Unique identifier for the business unit that owns the record
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
        public Microsoft.Xrm.Sdk.EntityReference OwningBusinessUnit
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningbusinessunit");
            }
        }

        /// <summary>
        /// Unique identifier for the team that owns the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
        public Microsoft.Xrm.Sdk.EntityReference OwningTeam
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owningteam");
            }
        }

        /// <summary>
        /// Unique identifier for the user that owns the record.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
        public Microsoft.Xrm.Sdk.EntityReference OwningUser
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("owninguser");
            }
        }

        /// <summary>
        /// Status of the Message Audit
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statecode")]
        public System.Nullable<InterfaceAuditLog_Status> Status
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode");
                if ((optionSet != null))
                {
                    return ((InterfaceAuditLog_Status)(System.Enum.ToObject(typeof(InterfaceAuditLog_Status), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("Status");
                if ((value == null))
                {
                    this.SetAttributeValue("statecode", null);
                }
                else
                {
                    this.SetAttributeValue("statecode", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Reason for the status of the Message Audit
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statuscode")]
        public System.Nullable<InterfaceAuditLog_StatusReason> StatusReason
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode");
                if ((optionSet != null))
                {
                    return ((InterfaceAuditLog_StatusReason)(System.Enum.ToObject(typeof(InterfaceAuditLog_StatusReason), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("StatusReason");
                if ((value == null))
                {
                    this.SetAttributeValue("statuscode", null);
                }
                else
                {
                    this.SetAttributeValue("statuscode", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("StatusReason");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("timezoneruleversionnumber")]
        public System.Nullable<int> TimeZoneRuleVersionNumber
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("timezoneruleversionnumber");
            }

            set
            {
                this.OnPropertyChanging("TimeZoneRuleVersionNumber");
                this.SetAttributeValue("timezoneruleversionnumber", value);
                this.OnPropertyChanged("TimeZoneRuleVersionNumber");
            }
        }

        /// <summary>
        /// Time zone code that was in use when the record was created.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("utcconversiontimezonecode")]
        public System.Nullable<int> UTCConversionTimeZoneCode
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("utcconversiontimezonecode");
            }

            set
            {
                this.OnPropertyChanging("UTCConversionTimeZoneCode");
                this.SetAttributeValue("utcconversiontimezonecode", value);
                this.OnPropertyChanged("UTCConversionTimeZoneCode");
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
        /// 1:N mcs_messageaudit_ActivityPointers
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_ActivityPointers")]
        public System.Collections.Generic.IEnumerable<ActivityPointer> Activities_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ActivityPointer>("mcs_messageaudit_ActivityPointers", null);
            }

            set
            {
                this.OnPropertyChanging("Activities_Regarding");
                this.SetRelatedEntities<ActivityPointer>("mcs_messageaudit_ActivityPointers", null, value);
                this.OnPropertyChanged("Activities_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_Annotations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_Annotations")]
        public System.Collections.Generic.IEnumerable<Annotation> Notes_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Annotation>("mcs_messageaudit_Annotations", null);
            }

            set
            {
                this.OnPropertyChanging("Notes_Regarding");
                this.SetRelatedEntities<Annotation>("mcs_messageaudit_Annotations", null, value);
                this.OnPropertyChanged("Notes_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_Appointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_Appointments")]
        public System.Collections.Generic.IEnumerable<Appointment> ReserveResources_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Appointment>("mcs_messageaudit_Appointments", null);
            }

            set
            {
                this.OnPropertyChanging("ReserveResources_Regarding");
                this.SetRelatedEntities<Appointment>("mcs_messageaudit_Appointments", null, value);
                this.OnPropertyChanged("ReserveResources_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_AsyncOperations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_AsyncOperations")]
        public System.Collections.Generic.IEnumerable<SystemJob> SystemJobs_Regarding
        {
            get
            {
                return this.GetRelatedEntities<SystemJob>("mcs_messageaudit_AsyncOperations", null);
            }

            set
            {
                this.OnPropertyChanging("SystemJobs_Regarding");
                this.SetRelatedEntities<SystemJob>("mcs_messageaudit_AsyncOperations", null, value);
                this.OnPropertyChanged("SystemJobs_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_BulkDeleteFailures
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_BulkDeleteFailures")]
        public System.Collections.Generic.IEnumerable<BulkDeleteFailure> BulkDeleteFailures_Name
        {
            get
            {
                return this.GetRelatedEntities<BulkDeleteFailure>("mcs_messageaudit_BulkDeleteFailures", null);
            }

            set
            {
                this.OnPropertyChanging("BulkDeleteFailures_Name");
                this.SetRelatedEntities<BulkDeleteFailure>("mcs_messageaudit_BulkDeleteFailures", null, value);
                this.OnPropertyChanged("BulkDeleteFailures_Name");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_connections1
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_connections1")]
        public System.Collections.Generic.IEnumerable<Connection> Connections_ConnectedFrom
        {
            get
            {
                return this.GetRelatedEntities<Connection>("mcs_messageaudit_connections1", null);
            }

            set
            {
                this.OnPropertyChanging("Connections_ConnectedFrom");
                this.SetRelatedEntities<Connection>("mcs_messageaudit_connections1", null, value);
                this.OnPropertyChanged("Connections_ConnectedFrom");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_connections2
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_connections2")]
        public System.Collections.Generic.IEnumerable<Connection> Connections_ConnectedTo
        {
            get
            {
                return this.GetRelatedEntities<Connection>("mcs_messageaudit_connections2", null);
            }

            set
            {
                this.OnPropertyChanging("Connections_ConnectedTo");
                this.SetRelatedEntities<Connection>("mcs_messageaudit_connections2", null, value);
                this.OnPropertyChanged("Connections_ConnectedTo");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_Emails
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_Emails")]
        public System.Collections.Generic.IEnumerable<Email> EmailMessages_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Email>("mcs_messageaudit_Emails", null);
            }

            set
            {
                this.OnPropertyChanging("EmailMessages_Regarding");
                this.SetRelatedEntities<Email>("mcs_messageaudit_Emails", null, value);
                this.OnPropertyChanged("EmailMessages_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_MailboxTrackingFolders
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_MailboxTrackingFolders")]
        public System.Collections.Generic.IEnumerable<MailboxAutoTrackingFolder> MailboxAutoTrackingFolders_RegardingObjectId
        {
            get
            {
                return this.GetRelatedEntities<MailboxAutoTrackingFolder>("mcs_messageaudit_MailboxTrackingFolders", null);
            }

            set
            {
                this.OnPropertyChanging("MailboxAutoTrackingFolders_RegardingObjectId");
                this.SetRelatedEntities<MailboxAutoTrackingFolder>("mcs_messageaudit_MailboxTrackingFolders", null, value);
                this.OnPropertyChanged("MailboxAutoTrackingFolders_RegardingObjectId");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_PrincipalObjectAttributeAccesses
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_PrincipalObjectAttributeAccesses")]
        public System.Collections.Generic.IEnumerable<FieldSharing> FieldSharing_EntityInstance
        {
            get
            {
                return this.GetRelatedEntities<FieldSharing>("mcs_messageaudit_PrincipalObjectAttributeAccesses", null);
            }

            set
            {
                this.OnPropertyChanging("FieldSharing_EntityInstance");
                this.SetRelatedEntities<FieldSharing>("mcs_messageaudit_PrincipalObjectAttributeAccesses", null, value);
                this.OnPropertyChanged("FieldSharing_EntityInstance");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_ProcessSession
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_ProcessSession")]
        public System.Collections.Generic.IEnumerable<ProcessSession> ProcessSessions_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ProcessSession>("mcs_messageaudit_ProcessSession", null);
            }

            set
            {
                this.OnPropertyChanging("ProcessSessions_Regarding");
                this.SetRelatedEntities<ProcessSession>("mcs_messageaudit_ProcessSession", null, value);
                this.OnPropertyChanged("ProcessSessions_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_RecurringAppointmentMasters
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_RecurringAppointmentMasters")]
        public System.Collections.Generic.IEnumerable<RecurringAppointmentMaster> RecurringReserveResources_Regarding
        {
            get
            {
                return this.GetRelatedEntities<RecurringAppointmentMaster>("mcs_messageaudit_RecurringAppointmentMasters", null);
            }

            set
            {
                this.OnPropertyChanging("RecurringReserveResources_Regarding");
                this.SetRelatedEntities<RecurringAppointmentMaster>("mcs_messageaudit_RecurringAppointmentMasters", null, value);
                this.OnPropertyChanged("RecurringReserveResources_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_ServiceAppointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_ServiceAppointments")]
        public System.Collections.Generic.IEnumerable<ServiceAppointment> Appointments_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ServiceAppointment>("mcs_messageaudit_ServiceAppointments", null);
            }

            set
            {
                this.OnPropertyChanging("Appointments_Regarding");
                this.SetRelatedEntities<ServiceAppointment>("mcs_messageaudit_ServiceAppointments", null, value);
                this.OnPropertyChanged("Appointments_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_Tasks
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_Tasks")]
        public System.Collections.Generic.IEnumerable<Task> Tasks_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Task>("mcs_messageaudit_Tasks", null);
            }

            set
            {
                this.OnPropertyChanging("Tasks_Regarding");
                this.SetRelatedEntities<Task>("mcs_messageaudit_Tasks", null, value);
                this.OnPropertyChanged("Tasks_Regarding");
            }
        }

        /// <summary>
        /// 1:N mcs_messageaudit_UserEntityInstanceDatas
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_messageaudit_UserEntityInstanceDatas")]
        public System.Collections.Generic.IEnumerable<UserEntityInstanceData> UserEntityInstanceData_ObjectId
        {
            get
            {
                return this.GetRelatedEntities<UserEntityInstanceData>("mcs_messageaudit_UserEntityInstanceDatas", null);
            }

            set
            {
                this.OnPropertyChanging("UserEntityInstanceData_ObjectId");
                this.SetRelatedEntities<UserEntityInstanceData>("mcs_messageaudit_UserEntityInstanceDatas", null, value);
                this.OnPropertyChanged("UserEntityInstanceData_ObjectId");
            }
        }

        /// <summary>
        /// N:1 business_unit_mcs_messageaudit
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("business_unit_mcs_messageaudit")]
        public BusinessUnit InterfaceAuditLog_OwningBusinessUnit
        {
            get
            {
                return this.GetRelatedEntity<BusinessUnit>("business_unit_mcs_messageaudit", null);
            }
        }

        /// <summary>
        /// N:1 lk_mcs_messageaudit_createdby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_mcs_messageaudit_createdby")]
        public SystemUser InterfaceAuditLog_CreatedBy
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_mcs_messageaudit_createdby", null);
            }
        }

        /// <summary>
        /// N:1 lk_mcs_messageaudit_createdonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_mcs_messageaudit_createdonbehalfby")]
        public SystemUser InterfaceAuditLog_CreatedBy_Delegate
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_mcs_messageaudit_createdonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 lk_mcs_messageaudit_modifiedby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_mcs_messageaudit_modifiedby")]
        public SystemUser InterfaceAuditLog_ModifiedBy
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_mcs_messageaudit_modifiedby", null);
            }
        }

        /// <summary>
        /// N:1 lk_mcs_messageaudit_modifiedonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_mcs_messageaudit_modifiedonbehalfby")]
        public SystemUser InterfaceAuditLog_ModifiedBy_Delegate
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_mcs_messageaudit_modifiedonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 mcs_mcs_message_mcs_messageaudit
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("mcs_messageid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("mcs_mcs_message_mcs_messageaudit")]
        public Message InterfaceAuditLog_MessageID
        {
            get
            {
                return this.GetRelatedEntity<Message>("mcs_mcs_message_mcs_messageaudit", null);
            }

            set
            {
                this.OnPropertyChanging("InterfaceAuditLog_MessageID");
                this.SetRelatedEntity<Message>("mcs_mcs_message_mcs_messageaudit", null, value);
                this.OnPropertyChanged("InterfaceAuditLog_MessageID");
            }
        }

        /// <summary>
        /// N:1 team_mcs_messageaudit
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("team_mcs_messageaudit")]
        public Team InterfaceAuditLog_OwningTeam
        {
            get
            {
                return this.GetRelatedEntity<Team>("team_mcs_messageaudit", null);
            }
        }

        /// <summary>
        /// N:1 user_mcs_messageaudit
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("user_mcs_messageaudit")]
        public SystemUser InterfaceAuditLog_OwningUser
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("user_mcs_messageaudit", null);
            }
        }
    }
}