﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using VA.TMP.OptionSets;

namespace VA.TMP.DataModel
{
    /// <summary>
    /// 
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("cvt_ppefeedback")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.71")]
    public partial class cvt_ppefeedback : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public cvt_ppefeedback(): base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "cvt_ppefeedback";
        public const string EntityLogicalCollectionName = "cvt_ppefeedbacks";
        public const string EntitySetName = "cvt_ppefeedbacks";
        public const int EntityTypeCode = 10069;
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
        public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_anythingtoreport")]
        public Microsoft.Xrm.Sdk.OptionSetValue cvt_anythingtoreport
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("cvt_anythingtoreport");
                if ((optionSet != null))
                {
                    return optionSet;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("cvt_anythingtoreport");
                if ((value == null))
                {
                    this.SetAttributeValue("cvt_anythingtoreport", null);
                }
                else
                {
                    this.SetAttributeValue("cvt_anythingtoreport", value);
                }

                this.OnPropertyChanged("cvt_anythingtoreport");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_facility")]
        public Microsoft.Xrm.Sdk.EntityReference cvt_facility
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("cvt_facility");
            }

            set
            {
                this.OnPropertyChanging("cvt_facility");
                this.SetAttributeValue("cvt_facility", value);
                this.OnPropertyChanged("cvt_facility");
            }
        }

        /// <summary>
        /// The name of the custom entity.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_name")]
        public string cvt_name
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_name");
            }

            set
            {
                this.OnPropertyChanging("cvt_name");
                this.SetAttributeValue("cvt_name", value);
                this.OnPropertyChanged("cvt_name");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_nextemail")]
        public System.Nullable<System.DateTime> cvt_nextemail
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("cvt_nextemail");
            }

            set
            {
                this.OnPropertyChanging("cvt_nextemail");
                this.SetAttributeValue("cvt_nextemail", value);
                this.OnPropertyChanged("cvt_nextemail");
            }
        }

        /// <summary>
        /// Unique identifier for entity instances
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_ppefeedbackid")]
        public System.Nullable<System.Guid> cvt_ppefeedbackId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("cvt_ppefeedbackid");
            }

            set
            {
                this.OnPropertyChanging("cvt_ppefeedbackId");
                this.SetAttributeValue("cvt_ppefeedbackid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }

                this.OnPropertyChanged("cvt_ppefeedbackId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_ppefeedbackid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                this.cvt_ppefeedbackId = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_ppereview")]
        public Microsoft.Xrm.Sdk.EntityReference cvt_ppereview
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("cvt_ppereview");
            }

            set
            {
                this.OnPropertyChanging("cvt_ppereview");
                this.SetAttributeValue("cvt_ppereview", value);
                this.OnPropertyChanged("cvt_ppereview");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_proxyprivileging")]
        public Microsoft.Xrm.Sdk.EntityReference cvt_proxyprivileging
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("cvt_proxyprivileging");
            }

            set
            {
                this.OnPropertyChanging("cvt_proxyprivileging");
                this.SetAttributeValue("cvt_proxyprivileging", value);
                this.OnPropertyChanged("cvt_proxyprivileging");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_responseescalated")]
        public Microsoft.Xrm.Sdk.EntityReference cvt_responseescalated
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("cvt_responseescalated");
            }

            set
            {
                this.OnPropertyChanging("cvt_responseescalated");
                this.SetAttributeValue("cvt_responseescalated", value);
                this.OnPropertyChanged("cvt_responseescalated");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_responserequested")]
        public Microsoft.Xrm.Sdk.EntityReference cvt_responserequested
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("cvt_responserequested");
            }

            set
            {
                this.OnPropertyChanging("cvt_responserequested");
                this.SetAttributeValue("cvt_responserequested", value);
                this.OnPropertyChanged("cvt_responserequested");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_submitteddate")]
        public System.Nullable<System.DateTime> cvt_submitteddate
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("cvt_submitteddate");
            }

            set
            {
                this.OnPropertyChanging("cvt_submitteddate");
                this.SetAttributeValue("cvt_submitteddate", value);
                this.OnPropertyChanged("cvt_submitteddate");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_submitter")]
        public Microsoft.Xrm.Sdk.EntityReference cvt_submitter
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("cvt_submitter");
            }

            set
            {
                this.OnPropertyChanging("cvt_submitter");
                this.SetAttributeValue("cvt_submitter", value);
                this.OnPropertyChanged("cvt_submitter");
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
        public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
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
        public System.Nullable<System.DateTime> OverriddenCreatedOn
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("overriddencreatedon");
            }

            set
            {
                this.OnPropertyChanging("OverriddenCreatedOn");
                this.SetAttributeValue("overriddencreatedon", value);
                this.OnPropertyChanged("OverriddenCreatedOn");
            }
        }

        /// <summary>
        /// Owner Id
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ownerid")]
        public Microsoft.Xrm.Sdk.EntityReference OwnerId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("ownerid");
            }

            set
            {
                this.OnPropertyChanging("OwnerId");
                this.SetAttributeValue("ownerid", value);
                this.OnPropertyChanged("OwnerId");
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
        /// Status of the PPE Feedback
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statecode")]
        public System.Nullable<cvt_ppefeedback_statecode> statecode
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode");
                if ((optionSet != null))
                {
                    return ((cvt_ppefeedback_statecode)(System.Enum.ToObject(typeof(cvt_ppefeedback_statecode), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("statecode");
                if ((value == null))
                {
                    this.SetAttributeValue("statecode", null);
                }
                else
                {
                    this.SetAttributeValue("statecode", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("statecode");
            }
        }

        /// <summary>
        /// Reason for the status of the PPE Feedback
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statuscode")]
        public System.Nullable<cvt_ppefeedback_statuscode> statuscode
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode");
                if ((optionSet != null))
                {
                    return ((cvt_ppefeedback_statuscode)(System.Enum.ToObject(typeof(cvt_ppefeedback_statuscode), optionSet.Value)));
                }
                else
                {
                    return null;
                }
            }

            set
            {
                this.OnPropertyChanging("statuscode");
                if ((value == null))
                {
                    this.SetAttributeValue("statuscode", null);
                }
                else
                {
                    this.SetAttributeValue("statuscode", new Microsoft.Xrm.Sdk.OptionSetValue(((int)(value))));
                }

                this.OnPropertyChanged("statuscode");
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
        /// 1:N cvt_ppefeedback_ActivityPointers
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_ActivityPointers")]
        public System.Collections.Generic.IEnumerable<ActivityPointer> cvt_ppefeedback_ActivityPointers
        {
            get
            {
                return this.GetRelatedEntities<ActivityPointer>("cvt_ppefeedback_ActivityPointers", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_ppefeedback_ActivityPointers");
                this.SetRelatedEntities<ActivityPointer>("cvt_ppefeedback_ActivityPointers", null, value);
                this.OnPropertyChanged("cvt_ppefeedback_ActivityPointers");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_Annotations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_Annotations")]
        public System.Collections.Generic.IEnumerable<Annotation> cvt_ppefeedback_Annotations
        {
            get
            {
                return this.GetRelatedEntities<Annotation>("cvt_ppefeedback_Annotations", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_ppefeedback_Annotations");
                this.SetRelatedEntities<Annotation>("cvt_ppefeedback_Annotations", null, value);
                this.OnPropertyChanged("cvt_ppefeedback_Annotations");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_Appointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_Appointments")]
        public System.Collections.Generic.IEnumerable<Appointment> cvt_ppefeedback_Appointments
        {
            get
            {
                return this.GetRelatedEntities<Appointment>("cvt_ppefeedback_Appointments", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_ppefeedback_Appointments");
                this.SetRelatedEntities<Appointment>("cvt_ppefeedback_Appointments", null, value);
                this.OnPropertyChanged("cvt_ppefeedback_Appointments");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_AsyncOperations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_AsyncOperations")]
        public System.Collections.Generic.IEnumerable<SystemJob> SystemJobs_Regarding
        {
            get
            {
                return this.GetRelatedEntities<SystemJob>("cvt_ppefeedback_AsyncOperations", null);
            }

            set
            {
                this.OnPropertyChanging("SystemJobs_Regarding");
                this.SetRelatedEntities<SystemJob>("cvt_ppefeedback_AsyncOperations", null, value);
                this.OnPropertyChanged("SystemJobs_Regarding");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_BulkDeleteFailures
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_BulkDeleteFailures")]
        public System.Collections.Generic.IEnumerable<BulkDeleteFailure> BulkDeleteFailures_Name
        {
            get
            {
                return this.GetRelatedEntities<BulkDeleteFailure>("cvt_ppefeedback_BulkDeleteFailures", null);
            }

            set
            {
                this.OnPropertyChanging("BulkDeleteFailures_Name");
                this.SetRelatedEntities<BulkDeleteFailure>("cvt_ppefeedback_BulkDeleteFailures", null, value);
                this.OnPropertyChanged("BulkDeleteFailures_Name");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_connections1
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_connections1")]
        public System.Collections.Generic.IEnumerable<Connection> Connections_ConnectedFrom
        {
            get
            {
                return this.GetRelatedEntities<Connection>("cvt_ppefeedback_connections1", null);
            }

            set
            {
                this.OnPropertyChanging("Connections_ConnectedFrom");
                this.SetRelatedEntities<Connection>("cvt_ppefeedback_connections1", null, value);
                this.OnPropertyChanged("Connections_ConnectedFrom");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_connections2
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_connections2")]
        public System.Collections.Generic.IEnumerable<Connection> Connections_ConnectedTo
        {
            get
            {
                return this.GetRelatedEntities<Connection>("cvt_ppefeedback_connections2", null);
            }

            set
            {
                this.OnPropertyChanging("Connections_ConnectedTo");
                this.SetRelatedEntities<Connection>("cvt_ppefeedback_connections2", null, value);
                this.OnPropertyChanged("Connections_ConnectedTo");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_Emails
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_Emails")]
        public System.Collections.Generic.IEnumerable<Email> cvt_ppefeedback_Emails
        {
            get
            {
                return this.GetRelatedEntities<Email>("cvt_ppefeedback_Emails", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_ppefeedback_Emails");
                this.SetRelatedEntities<Email>("cvt_ppefeedback_Emails", null, value);
                this.OnPropertyChanged("cvt_ppefeedback_Emails");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_MailboxTrackingFolders
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_MailboxTrackingFolders")]
        public System.Collections.Generic.IEnumerable<MailboxAutoTrackingFolder> MailboxAutoTrackingFolders_RegardingObjectId
        {
            get
            {
                return this.GetRelatedEntities<MailboxAutoTrackingFolder>("cvt_ppefeedback_MailboxTrackingFolders", null);
            }

            set
            {
                this.OnPropertyChanging("MailboxAutoTrackingFolders_RegardingObjectId");
                this.SetRelatedEntities<MailboxAutoTrackingFolder>("cvt_ppefeedback_MailboxTrackingFolders", null, value);
                this.OnPropertyChanged("MailboxAutoTrackingFolders_RegardingObjectId");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_PrincipalObjectAttributeAccesses
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_PrincipalObjectAttributeAccesses")]
        public System.Collections.Generic.IEnumerable<FieldSharing> FieldSharing_EntityInstance
        {
            get
            {
                return this.GetRelatedEntities<FieldSharing>("cvt_ppefeedback_PrincipalObjectAttributeAccesses", null);
            }

            set
            {
                this.OnPropertyChanging("FieldSharing_EntityInstance");
                this.SetRelatedEntities<FieldSharing>("cvt_ppefeedback_PrincipalObjectAttributeAccesses", null, value);
                this.OnPropertyChanged("FieldSharing_EntityInstance");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_ProcessSession
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_ProcessSession")]
        public System.Collections.Generic.IEnumerable<ProcessSession> ProcessSessions_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ProcessSession>("cvt_ppefeedback_ProcessSession", null);
            }

            set
            {
                this.OnPropertyChanging("ProcessSessions_Regarding");
                this.SetRelatedEntities<ProcessSession>("cvt_ppefeedback_ProcessSession", null, value);
                this.OnPropertyChanged("ProcessSessions_Regarding");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_RecurringAppointmentMasters
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_RecurringAppointmentMasters")]
        public System.Collections.Generic.IEnumerable<RecurringAppointmentMaster> cvt_ppefeedback_RecurringAppointmentMasters
        {
            get
            {
                return this.GetRelatedEntities<RecurringAppointmentMaster>("cvt_ppefeedback_RecurringAppointmentMasters", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_ppefeedback_RecurringAppointmentMasters");
                this.SetRelatedEntities<RecurringAppointmentMaster>("cvt_ppefeedback_RecurringAppointmentMasters", null, value);
                this.OnPropertyChanged("cvt_ppefeedback_RecurringAppointmentMasters");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_ServiceAppointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_ServiceAppointments")]
        public System.Collections.Generic.IEnumerable<ServiceAppointment> cvt_ppefeedback_ServiceAppointments
        {
            get
            {
                return this.GetRelatedEntities<ServiceAppointment>("cvt_ppefeedback_ServiceAppointments", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_ppefeedback_ServiceAppointments");
                this.SetRelatedEntities<ServiceAppointment>("cvt_ppefeedback_ServiceAppointments", null, value);
                this.OnPropertyChanged("cvt_ppefeedback_ServiceAppointments");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_SyncErrors
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_SyncErrors")]
        public System.Collections.Generic.IEnumerable<SyncError> SyncErrors_Record
        {
            get
            {
                return this.GetRelatedEntities<SyncError>("cvt_ppefeedback_SyncErrors", null);
            }

            set
            {
                this.OnPropertyChanging("SyncErrors_Record");
                this.SetRelatedEntities<SyncError>("cvt_ppefeedback_SyncErrors", null, value);
                this.OnPropertyChanged("SyncErrors_Record");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_Tasks
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_Tasks")]
        public System.Collections.Generic.IEnumerable<Task> Tasks_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Task>("cvt_ppefeedback_Tasks", null);
            }

            set
            {
                this.OnPropertyChanging("Tasks_Regarding");
                this.SetRelatedEntities<Task>("cvt_ppefeedback_Tasks", null, value);
                this.OnPropertyChanged("Tasks_Regarding");
            }
        }

        /// <summary>
        /// 1:N cvt_ppefeedback_UserEntityInstanceDatas
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_ppefeedback_UserEntityInstanceDatas")]
        public System.Collections.Generic.IEnumerable<UserEntityInstanceData> UserEntityInstanceData_ObjectId
        {
            get
            {
                return this.GetRelatedEntities<UserEntityInstanceData>("cvt_ppefeedback_UserEntityInstanceDatas", null);
            }

            set
            {
                this.OnPropertyChanging("UserEntityInstanceData_ObjectId");
                this.SetRelatedEntities<UserEntityInstanceData>("cvt_ppefeedback_UserEntityInstanceDatas", null, value);
                this.OnPropertyChanged("UserEntityInstanceData_ObjectId");
            }
        }

        /// <summary>
        /// N:1 business_unit_cvt_ppefeedback
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("business_unit_cvt_ppefeedback")]
        public BusinessUnit business_unit_cvt_ppefeedback
        {
            get
            {
                return this.GetRelatedEntity<BusinessUnit>("business_unit_cvt_ppefeedback", null);
            }
        }

        /// <summary>
        /// N:1 cvt_cvt_ppereview_cvt_ppefeedback_ppereview
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_ppereview")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_cvt_ppereview_cvt_ppefeedback_ppereview")]
        public cvt_ppereview cvt_cvt_ppereview_cvt_ppefeedback_ppereview
        {
            get
            {
                return this.GetRelatedEntity<cvt_ppereview>("cvt_cvt_ppereview_cvt_ppefeedback_ppereview", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_cvt_ppereview_cvt_ppefeedback_ppereview");
                this.SetRelatedEntity<cvt_ppereview>("cvt_cvt_ppereview_cvt_ppefeedback_ppereview", null, value);
                this.OnPropertyChanged("cvt_cvt_ppereview_cvt_ppefeedback_ppereview");
            }
        }

        /// <summary>
        /// N:1 cvt_cvt_tssprivileging_cvt_ppefeedback_proxyprivileging
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_proxyprivileging")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_cvt_tssprivileging_cvt_ppefeedback_proxyprivileging")]
        public cvt_tssprivileging cvt_cvt_tssprivileging_cvt_ppefeedback_proxyprivileging
        {
            get
            {
                return this.GetRelatedEntity<cvt_tssprivileging>("cvt_cvt_tssprivileging_cvt_ppefeedback_proxyprivileging", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_cvt_tssprivileging_cvt_ppefeedback_proxyprivileging");
                this.SetRelatedEntity<cvt_tssprivileging>("cvt_cvt_tssprivileging_cvt_ppefeedback_proxyprivileging", null, value);
                this.OnPropertyChanged("cvt_cvt_tssprivileging_cvt_ppefeedback_proxyprivileging");
            }
        }

        /// <summary>
        /// N:1 cvt_mcs_facility_cvt_ppefeedback_facility
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_facility")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_mcs_facility_cvt_ppefeedback_facility")]
        public mcs_facility cvt_mcs_facility_cvt_ppefeedback_facility
        {
            get
            {
                return this.GetRelatedEntity<mcs_facility>("cvt_mcs_facility_cvt_ppefeedback_facility", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_mcs_facility_cvt_ppefeedback_facility");
                this.SetRelatedEntity<mcs_facility>("cvt_mcs_facility_cvt_ppefeedback_facility", null, value);
                this.OnPropertyChanged("cvt_mcs_facility_cvt_ppefeedback_facility");
            }
        }

        /// <summary>
        /// N:1 cvt_systemuser_cvt_ppefeedback_submitter
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_submitter")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_systemuser_cvt_ppefeedback_submitter")]
        public SystemUser cvt_systemuser_cvt_ppefeedback_submitter
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("cvt_systemuser_cvt_ppefeedback_submitter", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_systemuser_cvt_ppefeedback_submitter");
                this.SetRelatedEntity<SystemUser>("cvt_systemuser_cvt_ppefeedback_submitter", null, value);
                this.OnPropertyChanged("cvt_systemuser_cvt_ppefeedback_submitter");
            }
        }

        /// <summary>
        /// N:1 cvt_team_cvt_ppefeedback_responseescalated
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_responseescalated")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_team_cvt_ppefeedback_responseescalated")]
        public Team cvt_team_cvt_ppefeedback_responseescalated
        {
            get
            {
                return this.GetRelatedEntity<Team>("cvt_team_cvt_ppefeedback_responseescalated", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_team_cvt_ppefeedback_responseescalated");
                this.SetRelatedEntity<Team>("cvt_team_cvt_ppefeedback_responseescalated", null, value);
                this.OnPropertyChanged("cvt_team_cvt_ppefeedback_responseescalated");
            }
        }

        /// <summary>
        /// N:1 cvt_team_cvt_ppefeedback_responserequested
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_responserequested")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_team_cvt_ppefeedback_responserequested")]
        public Team cvt_team_cvt_ppefeedback_responserequested
        {
            get
            {
                return this.GetRelatedEntity<Team>("cvt_team_cvt_ppefeedback_responserequested", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_team_cvt_ppefeedback_responserequested");
                this.SetRelatedEntity<Team>("cvt_team_cvt_ppefeedback_responserequested", null, value);
                this.OnPropertyChanged("cvt_team_cvt_ppefeedback_responserequested");
            }
        }

        /// <summary>
        /// N:1 lk_cvt_ppefeedback_createdby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_ppefeedback_createdby")]
        public SystemUser lk_cvt_ppefeedback_createdby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_ppefeedback_createdby", null);
            }
        }

        /// <summary>
        /// N:1 lk_cvt_ppefeedback_createdonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_ppefeedback_createdonbehalfby")]
        public SystemUser lk_cvt_ppefeedback_createdonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_ppefeedback_createdonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 lk_cvt_ppefeedback_modifiedby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_ppefeedback_modifiedby")]
        public SystemUser lk_cvt_ppefeedback_modifiedby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_ppefeedback_modifiedby", null);
            }
        }

        /// <summary>
        /// N:1 lk_cvt_ppefeedback_modifiedonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_ppefeedback_modifiedonbehalfby")]
        public SystemUser lk_cvt_ppefeedback_modifiedonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_ppefeedback_modifiedonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 team_cvt_ppefeedback
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("team_cvt_ppefeedback")]
        public Team team_cvt_ppefeedback
        {
            get
            {
                return this.GetRelatedEntity<Team>("team_cvt_ppefeedback", null);
            }
        }

        /// <summary>
        /// N:1 user_cvt_ppefeedback
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("user_cvt_ppefeedback")]
        public SystemUser user_cvt_ppefeedback
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("user_cvt_ppefeedback", null);
            }
        }
    }
}