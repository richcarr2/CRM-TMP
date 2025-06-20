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
    /// Reservation entity representing the summary of the associated resource bookings.
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("bookableresourcebookingheader")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.71")]
    public partial class BookableResourceBookingHeader : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public BookableResourceBookingHeader(): base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "bookableresourcebookingheader";
        public const string EntityLogicalCollectionName = "bookableresourcebookingheaders";
        public const string EntitySetName = "bookableresourcebookingheaders";
        public const int EntityTypeCode = 1146;
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
        /// Unique identifier of the resource booking header.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("bookableresourcebookingheaderid")]
        public System.Nullable<System.Guid> BookableResourceBookingHeaderId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("bookableresourcebookingheaderid");
            }

            set
            {
                this.OnPropertyChanging("BookableResourceBookingHeaderId");
                this.SetAttributeValue("bookableresourcebookingheaderid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }

                this.OnPropertyChanged("BookableResourceBookingHeaderId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("bookableresourcebookingheaderid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                this.BookableResourceBookingHeaderId = value;
            }
        }

        /// <summary>
        /// lk_bookableresourcebookingheader_createdby
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
        /// lk_bookableresourcebookingheader_createdonbehalfby
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
        /// Shows the aggregate duration of the linked bookings.
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
        /// Shows the end date and time of the booking summary.
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
        /// Exchange rate for the currency associated with the bookableresourcebookingheader with respect to the base currency.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("exchangerate")]
        public System.Nullable<decimal> ExchangeRate
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<decimal>>("exchangerate");
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
        /// lk_bookableresourcebookingheader_modifiedby
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
        /// lk_bookableresourcebookingheader_modifiedonbehalfby
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
        /// The name of the booking summary.
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
        /// owner_bookableresourcebookingheader
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
        /// business_unit_bookableresourcebookingheader
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
        /// team_bookableresourcebookingheader
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
        /// user_bookableresourcebookingheader
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
        /// Shows the ID of the process.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("processid")]
        public System.Nullable<System.Guid> Process
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("processid");
            }

            set
            {
                this.OnPropertyChanging("Process");
                this.SetAttributeValue("processid", value);
                this.OnPropertyChanged("Process");
            }
        }

        /// <summary>
        /// Shows the ID of the stage.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("stageid")]
        public System.Nullable<System.Guid> ProcessStage
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("stageid");
            }

            set
            {
                this.OnPropertyChanging("ProcessStage");
                this.SetAttributeValue("stageid", value);
                this.OnPropertyChanged("ProcessStage");
            }
        }

        /// <summary>
        /// Shows the start date and time of the booking summary.
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
        /// Status of the Bookable Resource Booking Header
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statecode")]
        public System.Nullable<BookableResourceBookingHeader_Status> Status
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode");
                if ((optionSet != null))
                {
                    return ((BookableResourceBookingHeader_Status)(System.Enum.ToObject(typeof(BookableResourceBookingHeader_Status), optionSet.Value)));
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
        /// Reason for the status of the Bookable Resource Booking Header
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statuscode")]
        public System.Nullable<BookableResourceBookingHeader_StatusReason> StatusReason
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode");
                if ((optionSet != null))
                {
                    return ((BookableResourceBookingHeader_StatusReason)(System.Enum.ToObject(typeof(BookableResourceBookingHeader_StatusReason), optionSet.Value)));
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
        /// TransactionCurrency_bookableresourcebookingheader
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("transactioncurrencyid")]
        public Microsoft.Xrm.Sdk.EntityReference Currency
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("transactioncurrencyid");
            }

            set
            {
                this.OnPropertyChanging("Currency");
                this.SetAttributeValue("transactioncurrencyid", value);
                this.OnPropertyChanged("Currency");
            }
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("traversedpath")]
        public string TraversedPath
        {
            get
            {
                return this.GetAttributeValue<string>("traversedpath");
            }

            set
            {
                this.OnPropertyChanging("TraversedPath");
                this.SetAttributeValue("traversedpath", value);
                this.OnPropertyChanged("TraversedPath");
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
        /// 1:N bookableresourcebookingheader_ActivityPointers
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_ActivityPointers")]
        public System.Collections.Generic.IEnumerable<ActivityPointer> Activities_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ActivityPointer>("bookableresourcebookingheader_ActivityPointers", null);
            }

            set
            {
                this.OnPropertyChanging("Activities_Regarding");
                this.SetRelatedEntities<ActivityPointer>("bookableresourcebookingheader_ActivityPointers", null, value);
                this.OnPropertyChanged("Activities_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_Annotations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_Annotations")]
        public System.Collections.Generic.IEnumerable<Annotation> Notes_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Annotation>("bookableresourcebookingheader_Annotations", null);
            }

            set
            {
                this.OnPropertyChanging("Notes_Regarding");
                this.SetRelatedEntities<Annotation>("bookableresourcebookingheader_Annotations", null, value);
                this.OnPropertyChanged("Notes_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_Appointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_Appointments")]
        public System.Collections.Generic.IEnumerable<Appointment> ReserveResources_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Appointment>("bookableresourcebookingheader_Appointments", null);
            }

            set
            {
                this.OnPropertyChanging("ReserveResources_Regarding");
                this.SetRelatedEntities<Appointment>("bookableresourcebookingheader_Appointments", null, value);
                this.OnPropertyChanged("ReserveResources_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_AsyncOperations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_AsyncOperations")]
        public System.Collections.Generic.IEnumerable<SystemJob> SystemJobs_Regarding
        {
            get
            {
                return this.GetRelatedEntities<SystemJob>("bookableresourcebookingheader_AsyncOperations", null);
            }

            set
            {
                this.OnPropertyChanging("SystemJobs_Regarding");
                this.SetRelatedEntities<SystemJob>("bookableresourcebookingheader_AsyncOperations", null, value);
                this.OnPropertyChanged("SystemJobs_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_bookableresourcebooking_Header
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_bookableresourcebooking_Header")]
        public System.Collections.Generic.IEnumerable<BookableResourceBooking> BookableResourceBookings_Header
        {
            get
            {
                return this.GetRelatedEntities<BookableResourceBooking>("bookableresourcebookingheader_bookableresourcebooking_Header", null);
            }

            set
            {
                this.OnPropertyChanging("BookableResourceBookings_Header");
                this.SetRelatedEntities<BookableResourceBooking>("bookableresourcebookingheader_bookableresourcebooking_Header", null, value);
                this.OnPropertyChanged("BookableResourceBookings_Header");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_BulkDeleteFailures
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_BulkDeleteFailures")]
        public System.Collections.Generic.IEnumerable<BulkDeleteFailure> BulkDeleteFailures_Name
        {
            get
            {
                return this.GetRelatedEntities<BulkDeleteFailure>("bookableresourcebookingheader_BulkDeleteFailures", null);
            }

            set
            {
                this.OnPropertyChanging("BulkDeleteFailures_Name");
                this.SetRelatedEntities<BulkDeleteFailure>("bookableresourcebookingheader_BulkDeleteFailures", null, value);
                this.OnPropertyChanged("BulkDeleteFailures_Name");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_BulkOperations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_BulkOperations")]
        public System.Collections.Generic.IEnumerable<QuickCampaign> QuickCampaigns_ImportFileName
        {
            get
            {
                return this.GetRelatedEntities<QuickCampaign>("bookableresourcebookingheader_BulkOperations", null);
            }

            set
            {
                this.OnPropertyChanging("QuickCampaigns_ImportFileName");
                this.SetRelatedEntities<QuickCampaign>("bookableresourcebookingheader_BulkOperations", null, value);
                this.OnPropertyChanged("QuickCampaigns_ImportFileName");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_CampaignActivities
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_CampaignActivities")]
        public System.Collections.Generic.IEnumerable<CampaignActivity> CampaignActivities_ParentCampaign
        {
            get
            {
                return this.GetRelatedEntities<CampaignActivity>("bookableresourcebookingheader_CampaignActivities", null);
            }

            set
            {
                this.OnPropertyChanging("CampaignActivities_ParentCampaign");
                this.SetRelatedEntities<CampaignActivity>("bookableresourcebookingheader_CampaignActivities", null, value);
                this.OnPropertyChanged("CampaignActivities_ParentCampaign");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_CampaignResponses
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_CampaignResponses")]
        public System.Collections.Generic.IEnumerable<CampaignResponse> CampaignResponses_ParentCampaign
        {
            get
            {
                return this.GetRelatedEntities<CampaignResponse>("bookableresourcebookingheader_CampaignResponses", null);
            }

            set
            {
                this.OnPropertyChanging("CampaignResponses_ParentCampaign");
                this.SetRelatedEntities<CampaignResponse>("bookableresourcebookingheader_CampaignResponses", null, value);
                this.OnPropertyChanged("CampaignResponses_ParentCampaign");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_Emails
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_Emails")]
        public System.Collections.Generic.IEnumerable<Email> EmailMessages_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Email>("bookableresourcebookingheader_Emails", null);
            }

            set
            {
                this.OnPropertyChanging("EmailMessages_Regarding");
                this.SetRelatedEntities<Email>("bookableresourcebookingheader_Emails", null, value);
                this.OnPropertyChanged("EmailMessages_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_MailboxTrackingFolders
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_MailboxTrackingFolders")]
        public System.Collections.Generic.IEnumerable<MailboxAutoTrackingFolder> MailboxAutoTrackingFolders_RegardingObjectId
        {
            get
            {
                return this.GetRelatedEntities<MailboxAutoTrackingFolder>("bookableresourcebookingheader_MailboxTrackingFolders", null);
            }

            set
            {
                this.OnPropertyChanging("MailboxAutoTrackingFolders_RegardingObjectId");
                this.SetRelatedEntities<MailboxAutoTrackingFolder>("bookableresourcebookingheader_MailboxTrackingFolders", null, value);
                this.OnPropertyChanged("MailboxAutoTrackingFolders_RegardingObjectId");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_PrincipalObjectAttributeAccess
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_PrincipalObjectAttributeAccess")]
        public System.Collections.Generic.IEnumerable<FieldSharing> FieldSharing_EntityInstance
        {
            get
            {
                return this.GetRelatedEntities<FieldSharing>("bookableresourcebookingheader_PrincipalObjectAttributeAccess", null);
            }

            set
            {
                this.OnPropertyChanging("FieldSharing_EntityInstance");
                this.SetRelatedEntities<FieldSharing>("bookableresourcebookingheader_PrincipalObjectAttributeAccess", null, value);
                this.OnPropertyChanged("FieldSharing_EntityInstance");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_ProcessSession
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_ProcessSession")]
        public System.Collections.Generic.IEnumerable<ProcessSession> ProcessSessions_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ProcessSession>("bookableresourcebookingheader_ProcessSession", null);
            }

            set
            {
                this.OnPropertyChanging("ProcessSessions_Regarding");
                this.SetRelatedEntities<ProcessSession>("bookableresourcebookingheader_ProcessSession", null, value);
                this.OnPropertyChanged("ProcessSessions_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_RecurringAppointmentMasters
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_RecurringAppointmentMasters")]
        public System.Collections.Generic.IEnumerable<RecurringAppointmentMaster> RecurringReserveResources_Regarding
        {
            get
            {
                return this.GetRelatedEntities<RecurringAppointmentMaster>("bookableresourcebookingheader_RecurringAppointmentMasters", null);
            }

            set
            {
                this.OnPropertyChanging("RecurringReserveResources_Regarding");
                this.SetRelatedEntities<RecurringAppointmentMaster>("bookableresourcebookingheader_RecurringAppointmentMasters", null, value);
                this.OnPropertyChanged("RecurringReserveResources_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_ServiceAppointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_ServiceAppointments")]
        public System.Collections.Generic.IEnumerable<ServiceAppointment> Appointments_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ServiceAppointment>("bookableresourcebookingheader_ServiceAppointments", null);
            }

            set
            {
                this.OnPropertyChanging("Appointments_Regarding");
                this.SetRelatedEntities<ServiceAppointment>("bookableresourcebookingheader_ServiceAppointments", null, value);
                this.OnPropertyChanged("Appointments_Regarding");
            }
        }

        /// <summary>
        /// 1:N BookableResourceBookingHeader_SyncErrors
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("BookableResourceBookingHeader_SyncErrors")]
        public System.Collections.Generic.IEnumerable<SyncError> SyncErrors_Record
        {
            get
            {
                return this.GetRelatedEntities<SyncError>("BookableResourceBookingHeader_SyncErrors", null);
            }

            set
            {
                this.OnPropertyChanging("SyncErrors_Record");
                this.SetRelatedEntities<SyncError>("BookableResourceBookingHeader_SyncErrors", null, value);
                this.OnPropertyChanged("SyncErrors_Record");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_Tasks
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_Tasks")]
        public System.Collections.Generic.IEnumerable<Task> Tasks_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Task>("bookableresourcebookingheader_Tasks", null);
            }

            set
            {
                this.OnPropertyChanging("Tasks_Regarding");
                this.SetRelatedEntities<Task>("bookableresourcebookingheader_Tasks", null, value);
                this.OnPropertyChanged("Tasks_Regarding");
            }
        }

        /// <summary>
        /// 1:N bookableresourcebookingheader_UserEntityInstanceDatas
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("bookableresourcebookingheader_UserEntityInstanceDatas")]
        public System.Collections.Generic.IEnumerable<UserEntityInstanceData> UserEntityInstanceData_ObjectId
        {
            get
            {
                return this.GetRelatedEntities<UserEntityInstanceData>("bookableresourcebookingheader_UserEntityInstanceDatas", null);
            }

            set
            {
                this.OnPropertyChanging("UserEntityInstanceData_ObjectId");
                this.SetRelatedEntities<UserEntityInstanceData>("bookableresourcebookingheader_UserEntityInstanceDatas", null, value);
                this.OnPropertyChanged("UserEntityInstanceData_ObjectId");
            }
        }

        /// <summary>
        /// N:1 business_unit_bookableresourcebookingheader
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("business_unit_bookableresourcebookingheader")]
        public BusinessUnit BookableResourceBookingHeaders_OwningBusinessUnit
        {
            get
            {
                return this.GetRelatedEntity<BusinessUnit>("business_unit_bookableresourcebookingheader", null);
            }
        }

        /// <summary>
        /// N:1 lk_bookableresourcebookingheader_createdby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_bookableresourcebookingheader_createdby")]
        public SystemUser BookableResourceBookingHeaders_CreatedBy
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_bookableresourcebookingheader_createdby", null);
            }
        }

        /// <summary>
        /// N:1 lk_bookableresourcebookingheader_createdonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_bookableresourcebookingheader_createdonbehalfby")]
        public SystemUser BookableResourceBookingHeaders_CreatedBy_Delegate
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_bookableresourcebookingheader_createdonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 lk_bookableresourcebookingheader_modifiedby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_bookableresourcebookingheader_modifiedby")]
        public SystemUser BookableResourceBookingHeaders_ModifiedBy
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_bookableresourcebookingheader_modifiedby", null);
            }
        }

        /// <summary>
        /// N:1 lk_bookableresourcebookingheader_modifiedonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_bookableresourcebookingheader_modifiedonbehalfby")]
        public SystemUser BookableResourceBookingHeaders_ModifiedBy_Delegate
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_bookableresourcebookingheader_modifiedonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 team_bookableresourcebookingheader
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("team_bookableresourcebookingheader")]
        public Team BookableResourceBookingHeaders_OwningTeam
        {
            get
            {
                return this.GetRelatedEntity<Team>("team_bookableresourcebookingheader", null);
            }
        }

        /// <summary>
        /// N:1 TransactionCurrency_bookableresourcebookingheader
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("transactioncurrencyid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("TransactionCurrency_bookableresourcebookingheader")]
        public Currency BookableResourceBookingHeaders_Currency
        {
            get
            {
                return this.GetRelatedEntity<Currency>("TransactionCurrency_bookableresourcebookingheader", null);
            }

            set
            {
                this.OnPropertyChanging("BookableResourceBookingHeaders_Currency");
                this.SetRelatedEntity<Currency>("TransactionCurrency_bookableresourcebookingheader", null, value);
                this.OnPropertyChanged("BookableResourceBookingHeaders_Currency");
            }
        }

        /// <summary>
        /// N:1 user_bookableresourcebookingheader
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("user_bookableresourcebookingheader")]
        public SystemUser BookableResourceBookingHeaders_OwningUser
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("user_bookableresourcebookingheader", null);
            }
        }
    }
}