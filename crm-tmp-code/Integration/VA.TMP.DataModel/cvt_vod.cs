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
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("cvt_vod")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.71")]
    public partial class cvt_vod : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public cvt_vod(): base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "cvt_vod";
        public const string EntityLogicalCollectionName = "cvt_vods";
        public const string EntitySetName = "cvt_vods";
        public const int EntityTypeCode = 10072;
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
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_dialingalias")]
        public string cvt_dialingalias
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_dialingalias");
            }

            set
            {
                this.OnPropertyChanging("cvt_dialingalias");
                this.SetAttributeValue("cvt_dialingalias", value);
                this.OnPropertyChanged("cvt_dialingalias");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_endtime")]
        public System.Nullable<System.DateTime> cvt_endtime
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("cvt_endtime");
            }

            set
            {
                this.OnPropertyChanging("cvt_endtime");
                this.SetAttributeValue("cvt_endtime", value);
                this.OnPropertyChanged("cvt_endtime");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_guestpin")]
        public string cvt_guestpin
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_guestpin");
            }

            set
            {
                this.OnPropertyChanging("cvt_guestpin");
                this.SetAttributeValue("cvt_guestpin", value);
                this.OnPropertyChanged("cvt_guestpin");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_meetingroomname")]
        public string cvt_meetingroomname
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_meetingroomname");
            }

            set
            {
                this.OnPropertyChanging("cvt_meetingroomname");
                this.SetAttributeValue("cvt_meetingroomname", value);
                this.OnPropertyChanged("cvt_meetingroomname");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_miscdata")]
        public string cvt_miscdata
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_miscdata");
            }

            set
            {
                this.OnPropertyChanging("cvt_miscdata");
                this.SetAttributeValue("cvt_miscdata", value);
                this.OnPropertyChanged("cvt_miscdata");
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
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_patientemail")]
        public string cvt_patientemail
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_patientemail");
            }

            set
            {
                this.OnPropertyChanging("cvt_patientemail");
                this.SetAttributeValue("cvt_patientemail", value);
                this.OnPropertyChanged("cvt_patientemail");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_patientpin")]
        public string cvt_patientpin
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_patientpin");
            }

            set
            {
                this.OnPropertyChanging("cvt_patientpin");
                this.SetAttributeValue("cvt_patientpin", value);
                this.OnPropertyChanged("cvt_patientpin");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_patienturl")]
        public string cvt_patienturl
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_patienturl");
            }

            set
            {
                this.OnPropertyChanging("cvt_patienturl");
                this.SetAttributeValue("cvt_patienturl", value);
                this.OnPropertyChanged("cvt_patienturl");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_provider")]
        public Microsoft.Xrm.Sdk.EntityReference cvt_provider
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("cvt_provider");
            }

            set
            {
                this.OnPropertyChanging("cvt_provider");
                this.SetAttributeValue("cvt_provider", value);
                this.OnPropertyChanged("cvt_provider");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_providerpin")]
        public string cvt_providerpin
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_providerpin");
            }

            set
            {
                this.OnPropertyChanging("cvt_providerpin");
                this.SetAttributeValue("cvt_providerpin", value);
                this.OnPropertyChanged("cvt_providerpin");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_providerurl")]
        public string cvt_providerurl
        {
            get
            {
                return this.GetAttributeValue<string>("cvt_providerurl");
            }

            set
            {
                this.OnPropertyChanging("cvt_providerurl");
                this.SetAttributeValue("cvt_providerurl", value);
                this.OnPropertyChanged("cvt_providerurl");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_starttime")]
        public System.Nullable<System.DateTime> cvt_starttime
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.DateTime>>("cvt_starttime");
            }

            set
            {
                this.OnPropertyChanging("cvt_starttime");
                this.SetAttributeValue("cvt_starttime", value);
                this.OnPropertyChanged("cvt_starttime");
            }
        }

        /// <summary>
        /// Unique identifier for entity instances
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_vodid")]
        public System.Nullable<System.Guid> cvt_vodId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("cvt_vodid");
            }

            set
            {
                this.OnPropertyChanging("cvt_vodId");
                this.SetAttributeValue("cvt_vodid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }

                this.OnPropertyChanged("cvt_vodId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_vodid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                this.cvt_vodId = value;
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
        /// Status of the Video On Demand
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statecode")]
        public System.Nullable<cvt_vod_statecode> statecode
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode");
                if ((optionSet != null))
                {
                    return ((cvt_vod_statecode)(System.Enum.ToObject(typeof(cvt_vod_statecode), optionSet.Value)));
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
        /// Reason for the status of the Video On Demand
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("statuscode")]
        public Microsoft.Xrm.Sdk.OptionSetValue statuscode
        {
            get
            {
                Microsoft.Xrm.Sdk.OptionSetValue optionSet = this.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode");
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
                this.OnPropertyChanging("statuscode");
                if ((value == null))
                {
                    this.SetAttributeValue("statuscode", null);
                }
                else
                {
                    this.SetAttributeValue("statuscode", value);
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
        /// 1:N cvt_cvt_vod_mcs_integrationresult_vod
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_cvt_vod_mcs_integrationresult_vod")]
        public System.Collections.Generic.IEnumerable<mcs_integrationresult> cvt_cvt_vod_mcs_integrationresult_vod
        {
            get
            {
                return this.GetRelatedEntities<mcs_integrationresult>("cvt_cvt_vod_mcs_integrationresult_vod", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_cvt_vod_mcs_integrationresult_vod");
                this.SetRelatedEntities<mcs_integrationresult>("cvt_cvt_vod_mcs_integrationresult_vod", null, value);
                this.OnPropertyChanged("cvt_cvt_vod_mcs_integrationresult_vod");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_ActivityParties
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_ActivityParties")]
        public System.Collections.Generic.IEnumerable<ActivityParty> cvt_vod_ActivityParties
        {
            get
            {
                return this.GetRelatedEntities<ActivityParty>("cvt_vod_ActivityParties", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_vod_ActivityParties");
                this.SetRelatedEntities<ActivityParty>("cvt_vod_ActivityParties", null, value);
                this.OnPropertyChanged("cvt_vod_ActivityParties");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_ActivityPointers
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_ActivityPointers")]
        public System.Collections.Generic.IEnumerable<ActivityPointer> cvt_vod_ActivityPointers
        {
            get
            {
                return this.GetRelatedEntities<ActivityPointer>("cvt_vod_ActivityPointers", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_vod_ActivityPointers");
                this.SetRelatedEntities<ActivityPointer>("cvt_vod_ActivityPointers", null, value);
                this.OnPropertyChanged("cvt_vod_ActivityPointers");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_Appointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_Appointments")]
        public System.Collections.Generic.IEnumerable<Appointment> cvt_vod_Appointments
        {
            get
            {
                return this.GetRelatedEntities<Appointment>("cvt_vod_Appointments", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_vod_Appointments");
                this.SetRelatedEntities<Appointment>("cvt_vod_Appointments", null, value);
                this.OnPropertyChanged("cvt_vod_Appointments");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_AsyncOperations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_AsyncOperations")]
        public System.Collections.Generic.IEnumerable<SystemJob> SystemJobs_Regarding
        {
            get
            {
                return this.GetRelatedEntities<SystemJob>("cvt_vod_AsyncOperations", null);
            }

            set
            {
                this.OnPropertyChanging("SystemJobs_Regarding");
                this.SetRelatedEntities<SystemJob>("cvt_vod_AsyncOperations", null, value);
                this.OnPropertyChanged("SystemJobs_Regarding");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_BulkDeleteFailures
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_BulkDeleteFailures")]
        public System.Collections.Generic.IEnumerable<BulkDeleteFailure> BulkDeleteFailures_Name
        {
            get
            {
                return this.GetRelatedEntities<BulkDeleteFailure>("cvt_vod_BulkDeleteFailures", null);
            }

            set
            {
                this.OnPropertyChanging("BulkDeleteFailures_Name");
                this.SetRelatedEntities<BulkDeleteFailure>("cvt_vod_BulkDeleteFailures", null, value);
                this.OnPropertyChanged("BulkDeleteFailures_Name");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_connections1
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_connections1")]
        public System.Collections.Generic.IEnumerable<Connection> Connections_ConnectedFrom
        {
            get
            {
                return this.GetRelatedEntities<Connection>("cvt_vod_connections1", null);
            }

            set
            {
                this.OnPropertyChanging("Connections_ConnectedFrom");
                this.SetRelatedEntities<Connection>("cvt_vod_connections1", null, value);
                this.OnPropertyChanged("Connections_ConnectedFrom");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_connections2
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_connections2")]
        public System.Collections.Generic.IEnumerable<Connection> Connections_ConnectedTo
        {
            get
            {
                return this.GetRelatedEntities<Connection>("cvt_vod_connections2", null);
            }

            set
            {
                this.OnPropertyChanging("Connections_ConnectedTo");
                this.SetRelatedEntities<Connection>("cvt_vod_connections2", null, value);
                this.OnPropertyChanged("Connections_ConnectedTo");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_Emails
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_Emails")]
        public System.Collections.Generic.IEnumerable<Email> cvt_vod_Emails
        {
            get
            {
                return this.GetRelatedEntities<Email>("cvt_vod_Emails", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_vod_Emails");
                this.SetRelatedEntities<Email>("cvt_vod_Emails", null, value);
                this.OnPropertyChanged("cvt_vod_Emails");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_MailboxTrackingFolders
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_MailboxTrackingFolders")]
        public System.Collections.Generic.IEnumerable<MailboxAutoTrackingFolder> MailboxAutoTrackingFolders_RegardingObjectId
        {
            get
            {
                return this.GetRelatedEntities<MailboxAutoTrackingFolder>("cvt_vod_MailboxTrackingFolders", null);
            }

            set
            {
                this.OnPropertyChanging("MailboxAutoTrackingFolders_RegardingObjectId");
                this.SetRelatedEntities<MailboxAutoTrackingFolder>("cvt_vod_MailboxTrackingFolders", null, value);
                this.OnPropertyChanged("MailboxAutoTrackingFolders_RegardingObjectId");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_PrincipalObjectAttributeAccesses
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_PrincipalObjectAttributeAccesses")]
        public System.Collections.Generic.IEnumerable<FieldSharing> FieldSharing_EntityInstance
        {
            get
            {
                return this.GetRelatedEntities<FieldSharing>("cvt_vod_PrincipalObjectAttributeAccesses", null);
            }

            set
            {
                this.OnPropertyChanging("FieldSharing_EntityInstance");
                this.SetRelatedEntities<FieldSharing>("cvt_vod_PrincipalObjectAttributeAccesses", null, value);
                this.OnPropertyChanged("FieldSharing_EntityInstance");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_ProcessSession
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_ProcessSession")]
        public System.Collections.Generic.IEnumerable<ProcessSession> ProcessSessions_Regarding
        {
            get
            {
                return this.GetRelatedEntities<ProcessSession>("cvt_vod_ProcessSession", null);
            }

            set
            {
                this.OnPropertyChanging("ProcessSessions_Regarding");
                this.SetRelatedEntities<ProcessSession>("cvt_vod_ProcessSession", null, value);
                this.OnPropertyChanged("ProcessSessions_Regarding");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_RecurringAppointmentMasters
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_RecurringAppointmentMasters")]
        public System.Collections.Generic.IEnumerable<RecurringAppointmentMaster> cvt_vod_RecurringAppointmentMasters
        {
            get
            {
                return this.GetRelatedEntities<RecurringAppointmentMaster>("cvt_vod_RecurringAppointmentMasters", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_vod_RecurringAppointmentMasters");
                this.SetRelatedEntities<RecurringAppointmentMaster>("cvt_vod_RecurringAppointmentMasters", null, value);
                this.OnPropertyChanged("cvt_vod_RecurringAppointmentMasters");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_ServiceAppointments
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_ServiceAppointments")]
        public System.Collections.Generic.IEnumerable<ServiceAppointment> cvt_vod_ServiceAppointments
        {
            get
            {
                return this.GetRelatedEntities<ServiceAppointment>("cvt_vod_ServiceAppointments", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_vod_ServiceAppointments");
                this.SetRelatedEntities<ServiceAppointment>("cvt_vod_ServiceAppointments", null, value);
                this.OnPropertyChanged("cvt_vod_ServiceAppointments");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_SyncErrors
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_SyncErrors")]
        public System.Collections.Generic.IEnumerable<SyncError> SyncErrors_Record
        {
            get
            {
                return this.GetRelatedEntities<SyncError>("cvt_vod_SyncErrors", null);
            }

            set
            {
                this.OnPropertyChanging("SyncErrors_Record");
                this.SetRelatedEntities<SyncError>("cvt_vod_SyncErrors", null, value);
                this.OnPropertyChanged("SyncErrors_Record");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_Tasks
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_Tasks")]
        public System.Collections.Generic.IEnumerable<Task> Tasks_Regarding
        {
            get
            {
                return this.GetRelatedEntities<Task>("cvt_vod_Tasks", null);
            }

            set
            {
                this.OnPropertyChanging("Tasks_Regarding");
                this.SetRelatedEntities<Task>("cvt_vod_Tasks", null, value);
                this.OnPropertyChanged("Tasks_Regarding");
            }
        }

        /// <summary>
        /// 1:N cvt_vod_UserEntityInstanceDatas
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_vod_UserEntityInstanceDatas")]
        public System.Collections.Generic.IEnumerable<UserEntityInstanceData> UserEntityInstanceData_ObjectId
        {
            get
            {
                return this.GetRelatedEntities<UserEntityInstanceData>("cvt_vod_UserEntityInstanceDatas", null);
            }

            set
            {
                this.OnPropertyChanging("UserEntityInstanceData_ObjectId");
                this.SetRelatedEntities<UserEntityInstanceData>("cvt_vod_UserEntityInstanceDatas", null, value);
                this.OnPropertyChanged("UserEntityInstanceData_ObjectId");
            }
        }

        /// <summary>
        /// N:1 business_unit_cvt_vod
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningbusinessunit")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("business_unit_cvt_vod")]
        public BusinessUnit business_unit_cvt_vod
        {
            get
            {
                return this.GetRelatedEntity<BusinessUnit>("business_unit_cvt_vod", null);
            }
        }

        /// <summary>
        /// N:1 cvt_systemuser_cvt_vod_provider
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("cvt_provider")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("cvt_systemuser_cvt_vod_provider")]
        public SystemUser cvt_systemuser_cvt_vod_provider
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("cvt_systemuser_cvt_vod_provider", null);
            }

            set
            {
                this.OnPropertyChanging("cvt_systemuser_cvt_vod_provider");
                this.SetRelatedEntity<SystemUser>("cvt_systemuser_cvt_vod_provider", null, value);
                this.OnPropertyChanged("cvt_systemuser_cvt_vod_provider");
            }
        }

        /// <summary>
        /// N:1 lk_cvt_vod_createdby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_vod_createdby")]
        public SystemUser lk_cvt_vod_createdby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_vod_createdby", null);
            }
        }

        /// <summary>
        /// N:1 lk_cvt_vod_createdonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_vod_createdonbehalfby")]
        public SystemUser lk_cvt_vod_createdonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_vod_createdonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 lk_cvt_vod_modifiedby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_vod_modifiedby")]
        public SystemUser lk_cvt_vod_modifiedby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_vod_modifiedby", null);
            }
        }

        /// <summary>
        /// N:1 lk_cvt_vod_modifiedonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_cvt_vod_modifiedonbehalfby")]
        public SystemUser lk_cvt_vod_modifiedonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_cvt_vod_modifiedonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 team_cvt_vod
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owningteam")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("team_cvt_vod")]
        public Team team_cvt_vod
        {
            get
            {
                return this.GetRelatedEntity<Team>("team_cvt_vod", null);
            }
        }

        /// <summary>
        /// N:1 user_cvt_vod
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("owninguser")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("user_cvt_vod")]
        public SystemUser user_cvt_vod
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("user_cvt_vod", null);
            }
        }
    }
}