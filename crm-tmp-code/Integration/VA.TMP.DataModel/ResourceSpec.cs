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
    /// Selection rule that allows the scheduling engine to select a number of resources from a pool of resources. The rules can be associated with a service.
    /// </summary>
    [System.Runtime.Serialization.DataContractAttribute()]
    [Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("resourcespec")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("CrmSvcUtil", "9.1.0.71")]
    public partial class ResourceSpec : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ResourceSpec(): base(EntityLogicalName)
        {
        }

        public const string EntityLogicalName = "resourcespec";
        public const string EntityLogicalCollectionName = "resourcespecs";
        public const string EntitySetName = "resourcespecs";
        public const int EntityTypeCode = 4006;
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
        /// business_unit_resource_specs
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("businessunitid")]
        public Microsoft.Xrm.Sdk.EntityReference BusinessUnitId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("businessunitid");
            }

            set
            {
                this.OnPropertyChanging("BusinessUnitId");
                this.SetAttributeValue("businessunitid", value);
                this.OnPropertyChanged("BusinessUnitId");
            }
        }

        /// <summary>
        /// Additional constraints, specified as expressions, which are used to filter a set of valid resources.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("constraints")]
        public string Constraints
        {
            get
            {
                return this.GetAttributeValue<string>("constraints");
            }

            set
            {
                this.OnPropertyChanging("Constraints");
                this.SetAttributeValue("constraints", value);
                this.OnPropertyChanged("Constraints");
            }
        }

        /// <summary>
        /// lk_resourcespec_createdby
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
        /// Date and time when the resource specification was created.
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
        /// lk_resourcespec_createdonbehalfby
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
        /// Selection rule that allows the scheduling engine to select a number of resources from a pool of resources. The rules can be associated with a service.
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
        /// Number that specifies the minimal effort required from resources.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("effortrequired")]
        public System.Nullable<double> EffortRequired
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<double>>("effortrequired");
            }

            set
            {
                this.OnPropertyChanging("EffortRequired");
                this.SetAttributeValue("effortrequired", value);
                this.OnPropertyChanged("EffortRequired");
            }
        }

        /// <summary>
        /// team_resource_specs
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("groupobjectid")]
        public System.Nullable<System.Guid> GroupObjectId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("groupobjectid");
            }

            set
            {
                this.OnPropertyChanging("GroupObjectId");
                this.SetAttributeValue("groupobjectid", value);
                this.OnPropertyChanged("GroupObjectId");
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
        /// lk_resourcespec_modifiedby
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
        /// Date and time when the resource specification was last modified.
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
        /// lk_resourcespec_modifiedonbehalfby
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
        /// Name of the resource specification.
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
        /// Search strategy to use for the resource specification.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("objectiveexpression")]
        public string ObjectiveExpression
        {
            get
            {
                return this.GetAttributeValue<string>("objectiveexpression");
            }

            set
            {
                this.OnPropertyChanging("ObjectiveExpression");
                this.SetAttributeValue("objectiveexpression", value);
                this.OnPropertyChanged("ObjectiveExpression");
            }
        }

        /// <summary>
        /// Type of entity with which the resource specification is associated.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("objecttypecode")]
        public string ObjectTypeCode
        {
            get
            {
                return this.GetAttributeValue<string>("objecttypecode");
            }

            set
            {
                this.OnPropertyChanging("ObjectTypeCode");
                this.SetAttributeValue("objecttypecode", value);
                this.OnPropertyChanged("ObjectTypeCode");
            }
        }

        /// <summary>
        /// organization_resource_specs
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("organizationid")]
        public Microsoft.Xrm.Sdk.EntityReference OrganizationId
        {
            get
            {
                return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("organizationid");
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
        /// Required number of resources that must be available. Use -1 to indicate all resources.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("requiredcount")]
        public System.Nullable<int> RequiredCount
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<int>>("requiredcount");
            }

            set
            {
                this.OnPropertyChanging("RequiredCount");
                this.SetAttributeValue("requiredcount", value);
                this.OnPropertyChanged("RequiredCount");
            }
        }

        /// <summary>
        /// Unique identifier of the resource specification.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("resourcespecid")]
        public System.Nullable<System.Guid> ResourceSpecId
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<System.Guid>>("resourcespecid");
            }

            set
            {
                this.OnPropertyChanging("ResourceSpecId");
                this.SetAttributeValue("resourcespecid", value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = System.Guid.Empty;
                }

                this.OnPropertyChanged("ResourceSpecId");
            }
        }

        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("resourcespecid")]
        public override System.Guid Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                this.ResourceSpecId = value;
            }
        }

        /// <summary>
        /// Value that specifies that all valid and available resources must be in the same site.
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("samesite")]
        public System.Nullable<bool> SameSite
        {
            get
            {
                return this.GetAttributeValue<System.Nullable<bool>>("samesite");
            }

            set
            {
                this.OnPropertyChanging("SameSite");
                this.SetAttributeValue("samesite", value);
                this.OnPropertyChanged("SameSite");
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
        /// 1:N ActivityPartyResourceSpec
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("ActivityPartyResourceSpec")]
        public System.Collections.Generic.IEnumerable<ActivityParty> ActivityPartyResourceSpec
        {
            get
            {
                return this.GetRelatedEntities<ActivityParty>("ActivityPartyResourceSpec", null);
            }

            set
            {
                this.OnPropertyChanging("ActivityPartyResourceSpec");
                this.SetRelatedEntities<ActivityParty>("ActivityPartyResourceSpec", null, value);
                this.OnPropertyChanged("ActivityPartyResourceSpec");
            }
        }

        /// <summary>
        /// 1:N resource_spec_services
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("resource_spec_services")]
        public System.Collections.Generic.IEnumerable<Service> resource_spec_services
        {
            get
            {
                return this.GetRelatedEntities<Service>("resource_spec_services", null);
            }

            set
            {
                this.OnPropertyChanging("resource_spec_services");
                this.SetRelatedEntities<Service>("resource_spec_services", null, value);
                this.OnPropertyChanged("resource_spec_services");
            }
        }

        /// <summary>
        /// 1:N ResourceSpec_Annotation
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("ResourceSpec_Annotation")]
        public System.Collections.Generic.IEnumerable<Annotation> ResourceSpec_Annotation
        {
            get
            {
                return this.GetRelatedEntities<Annotation>("ResourceSpec_Annotation", null);
            }

            set
            {
                this.OnPropertyChanging("ResourceSpec_Annotation");
                this.SetRelatedEntities<Annotation>("ResourceSpec_Annotation", null, value);
                this.OnPropertyChanged("ResourceSpec_Annotation");
            }
        }

        /// <summary>
        /// 1:N ResourceSpec_AsyncOperations
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("ResourceSpec_AsyncOperations")]
        public System.Collections.Generic.IEnumerable<SystemJob> SystemJobs_Regarding
        {
            get
            {
                return this.GetRelatedEntities<SystemJob>("ResourceSpec_AsyncOperations", null);
            }

            set
            {
                this.OnPropertyChanging("SystemJobs_Regarding");
                this.SetRelatedEntities<SystemJob>("ResourceSpec_AsyncOperations", null, value);
                this.OnPropertyChanged("SystemJobs_Regarding");
            }
        }

        /// <summary>
        /// 1:N ResourceSpec_BulkDeleteFailures
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("ResourceSpec_BulkDeleteFailures")]
        public System.Collections.Generic.IEnumerable<BulkDeleteFailure> BulkDeleteFailures_Name
        {
            get
            {
                return this.GetRelatedEntities<BulkDeleteFailure>("ResourceSpec_BulkDeleteFailures", null);
            }

            set
            {
                this.OnPropertyChanging("BulkDeleteFailures_Name");
                this.SetRelatedEntities<BulkDeleteFailure>("ResourceSpec_BulkDeleteFailures", null, value);
                this.OnPropertyChanged("BulkDeleteFailures_Name");
            }
        }

        /// <summary>
        /// 1:N resourcespec_MailboxTrackingFolders
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("resourcespec_MailboxTrackingFolders")]
        public System.Collections.Generic.IEnumerable<MailboxAutoTrackingFolder> MailboxAutoTrackingFolders_RegardingObjectId
        {
            get
            {
                return this.GetRelatedEntities<MailboxAutoTrackingFolder>("resourcespec_MailboxTrackingFolders", null);
            }

            set
            {
                this.OnPropertyChanging("MailboxAutoTrackingFolders_RegardingObjectId");
                this.SetRelatedEntities<MailboxAutoTrackingFolder>("resourcespec_MailboxTrackingFolders", null, value);
                this.OnPropertyChanged("MailboxAutoTrackingFolders_RegardingObjectId");
            }
        }

        /// <summary>
        /// 1:N resourcespec_PrincipalObjectAttributeAccesses
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("resourcespec_PrincipalObjectAttributeAccesses")]
        public System.Collections.Generic.IEnumerable<FieldSharing> FieldSharing_EntityInstance
        {
            get
            {
                return this.GetRelatedEntities<FieldSharing>("resourcespec_PrincipalObjectAttributeAccesses", null);
            }

            set
            {
                this.OnPropertyChanging("FieldSharing_EntityInstance");
                this.SetRelatedEntities<FieldSharing>("resourcespec_PrincipalObjectAttributeAccesses", null, value);
                this.OnPropertyChanged("FieldSharing_EntityInstance");
            }
        }

        /// <summary>
        /// 1:N resourcespec_resources
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("resourcespec_resources")]
        public System.Collections.Generic.IEnumerable<Resource> resourcespec_resources
        {
            get
            {
                return this.GetRelatedEntities<Resource>("resourcespec_resources", null);
            }

            set
            {
                this.OnPropertyChanging("resourcespec_resources");
                this.SetRelatedEntities<Resource>("resourcespec_resources", null, value);
                this.OnPropertyChanged("resourcespec_resources");
            }
        }

        /// <summary>
        /// 1:N resourcespec_SyncErrors
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("resourcespec_SyncErrors")]
        public System.Collections.Generic.IEnumerable<SyncError> SyncErrors_Record
        {
            get
            {
                return this.GetRelatedEntities<SyncError>("resourcespec_SyncErrors", null);
            }

            set
            {
                this.OnPropertyChanging("SyncErrors_Record");
                this.SetRelatedEntities<SyncError>("resourcespec_SyncErrors", null, value);
                this.OnPropertyChanged("SyncErrors_Record");
            }
        }

        /// <summary>
        /// 1:N userentityinstancedata_resourcespec
        /// </summary>
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("userentityinstancedata_resourcespec")]
        public System.Collections.Generic.IEnumerable<UserEntityInstanceData> UserEntityInstanceData_ObjectId
        {
            get
            {
                return this.GetRelatedEntities<UserEntityInstanceData>("userentityinstancedata_resourcespec", null);
            }

            set
            {
                this.OnPropertyChanging("UserEntityInstanceData_ObjectId");
                this.SetRelatedEntities<UserEntityInstanceData>("userentityinstancedata_resourcespec", null, value);
                this.OnPropertyChanged("UserEntityInstanceData_ObjectId");
            }
        }

        /// <summary>
        /// N:1 business_unit_resource_specs
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("businessunitid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("business_unit_resource_specs")]
        public BusinessUnit business_unit_resource_specs
        {
            get
            {
                return this.GetRelatedEntity<BusinessUnit>("business_unit_resource_specs", null);
            }

            set
            {
                this.OnPropertyChanging("business_unit_resource_specs");
                this.SetRelatedEntity<BusinessUnit>("business_unit_resource_specs", null, value);
                this.OnPropertyChanged("business_unit_resource_specs");
            }
        }

        /// <summary>
        /// N:1 constraint_based_group_resource_specs
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("groupobjectid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("constraint_based_group_resource_specs")]
        public ConstraintBasedGroup constraint_based_group_resource_specs
        {
            get
            {
                return this.GetRelatedEntity<ConstraintBasedGroup>("constraint_based_group_resource_specs", null);
            }

            set
            {
                this.OnPropertyChanging("constraint_based_group_resource_specs");
                this.SetRelatedEntity<ConstraintBasedGroup>("constraint_based_group_resource_specs", null, value);
                this.OnPropertyChanged("constraint_based_group_resource_specs");
            }
        }

        /// <summary>
        /// N:1 lk_resourcespec_createdby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_resourcespec_createdby")]
        public SystemUser lk_resourcespec_createdby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_resourcespec_createdby", null);
            }
        }

        /// <summary>
        /// N:1 lk_resourcespec_createdonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_resourcespec_createdonbehalfby")]
        public SystemUser lk_resourcespec_createdonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_resourcespec_createdonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 lk_resourcespec_modifiedby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_resourcespec_modifiedby")]
        public SystemUser lk_resourcespec_modifiedby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_resourcespec_modifiedby", null);
            }
        }

        /// <summary>
        /// N:1 lk_resourcespec_modifiedonbehalfby
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("lk_resourcespec_modifiedonbehalfby")]
        public SystemUser lk_resourcespec_modifiedonbehalfby
        {
            get
            {
                return this.GetRelatedEntity<SystemUser>("lk_resourcespec_modifiedonbehalfby", null);
            }
        }

        /// <summary>
        /// N:1 team_resource_specs
        /// </summary>
        [Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("groupobjectid")]
        [Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("team_resource_specs")]
        public Team team_resource_specs
        {
            get
            {
                return this.GetRelatedEntity<Team>("team_resource_specs", null);
            }

            set
            {
                this.OnPropertyChanging("team_resource_specs");
                this.SetRelatedEntity<Team>("team_resource_specs", null, value);
                this.OnPropertyChanged("team_resource_specs");
            }
        }
    }
}