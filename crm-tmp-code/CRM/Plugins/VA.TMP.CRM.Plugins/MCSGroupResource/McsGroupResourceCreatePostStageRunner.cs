using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;
using System.Text;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM
{
    /// <summary>
    /// Update the Group Resource, the Constraint Based Group and the TSS Resource Group
    /// </summary>
    public class McsGroupResourceCreatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsGroupResourceCreatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        public override void Execute()
        {
            var thisGroupResource = CvtHelper.ValidateReturnRecord(PrimaryEntity, mcs_groupresource.EntityLogicalName, Logger, OrganizationService);

            mcs_groupresource thisGR = (mcs_groupresource)thisGroupResource;
            //Validate only one VC if Paired Resource Group.
            Guid relatedRG = thisGR.mcs_relatedResourceGroupId != null ? thisGR.mcs_relatedResourceGroupId.Id : Guid.Empty;
            Guid relatedR = thisGR.mcs_RelatedResourceId != null ? thisGR.mcs_RelatedResourceId.Id : Guid.Empty;

            CvtHelper.CheckPairedResourceGroup(relatedRG, relatedR, Logger, OrganizationService);

            UpdateGroupResource(thisGR);
            UpdateTSSResourceGroup(thisGR);

            Logger.WriteDebugMessage("GR Create: About to Check if Provider was added to a TSA.");
            sendEmailIfProvider(thisGroupResource.Id, Guid.Empty, OrganizationService, Logger);
        }

        #region Logic
       
        /// <summary>
        /// Update the Group Resource
        /// </summary>
        /// <param name="thisGroupResource"></param>
        /// TODO: Does the Vista Clinic capacity need to be set as another check?
        internal void UpdateGroupResource(mcs_groupresource thisGroupResource)
        {
            Logger.setMethod = "UpdateGroupResource";
            Logger.WriteDebugMessage("starting UpdateGroupResource");

            try
            {
                if (thisGroupResource == null || thisGroupResource.mcs_Type == null)
                    return;

                if (thisGroupResource.mcs_Type.Value == (int)mcs_resourcetype.Room)
                {
                    var relatedResource = OrganizationService.Retrieve(mcs_resource.EntityLogicalName, thisGroupResource.mcs_RelatedResourceId.Id, new ColumnSet(true)).ToEntity<mcs_resource>();
                    var updateGroupResource = new mcs_groupresource()
                    {
                        Id = thisGroupResource.Id,
                        cvt_capacity = relatedResource.cvt_capacity
                    };
                    Logger.WriteDebugMessage("Updating Group Resource.");
                    OrganizationService.Update(updateGroupResource);
                }
                Logger.WriteDebugMessage("ending UpdateGroupResource");
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

        /// <summary>
        /// Update the TSS Resource Group
        /// </summary>
        /// <param name="thisId"></param>
        internal void UpdateTSSResourceGroup(mcs_groupresource thisGroupResource)
        {
            Logger.setMethod = "UpdateTSSResourceGroup";
            Logger.WriteDebugMessage("starting UpdateTSSResourceGroup");

            try
            {
                if (thisGroupResource.mcs_relatedResourceGroupId == null)
                    return;

                var thisTSSResourceGroupId = thisGroupResource.mcs_relatedResourceGroupId.Id;
                var thisTSSResourceGroup = OrganizationService.Retrieve(mcs_resourcegroup.EntityLogicalName, thisTSSResourceGroupId, new ColumnSet(true)).ToEntity<mcs_resourcegroup>();

                if (thisTSSResourceGroup == null || thisTSSResourceGroup.mcs_Type == null)
                    return;

                var builderCBG = new StringBuilder("<Constraints><Constraint><Expression>");
                string resourceString = null;
                var count = 0;
                using (var srv = new Xrm(OrganizationService))
                {
                    var getEquipmentResources = from resGroups in srv.mcs_groupresourceSet
                                                join mcsResourcs in srv.mcs_resourceSet on resGroups.mcs_RelatedResourceId.Id equals mcsResourcs.mcs_resourceId.Value
                                                where resGroups.mcs_relatedResourceGroupId.Id == thisTSSResourceGroupId && resGroups.statecode == 0
                                                select new
                                                {
                                                    mcsResourcs.mcs_name,
                                                    mcsResourcs.mcs_relatedResourceId
                                                };

                    var getUserResources = from resGroups in srv.mcs_groupresourceSet
                                           join Users in srv.SystemUserSet on resGroups.mcs_RelatedUserId.Id equals Users.Id
                                           where resGroups.mcs_relatedResourceGroupId.Id == thisTSSResourceGroupId && resGroups.statecode == 0
                                           select new
                                           {
                                               resGroups.mcs_RelatedUserId,
                                               resGroups.mcs_name
                                           };

                    switch (thisTSSResourceGroup.mcs_Type.Value)
                    {
                        case (int)mcs_resourcetype.VistaClinic:
                        case (int)mcs_resourcetype.Room:
                        case (int)mcs_resourcetype.Technology:
                            Logger.WriteDebugMessage("TSS Resource Group is either VistA Clinic, Room, or Technology.");
                            foreach (var mcsResource in getEquipmentResources)
                            {
                                builderCBG = buildFunction(mcsResource.mcs_relatedResourceId, resourceString, builderCBG, count, out count, out resourceString);
                            }
                            break;
                        case (int)mcs_resourcetype.TelepresenterImager:
                        case (int)mcs_resourcetype.Provider:
                            Logger.WriteDebugMessage("TSS Resource Group is either Telepresenter or Provider.");
                            foreach (var systemUser in getUserResources)
                            {
                                builderCBG = buildFunction(systemUser.mcs_RelatedUserId, resourceString, builderCBG, count, out count, out resourceString);
                            }
                            break;
                        case (int)mcs_resourcetype.PairedResourceGroup:
                            Logger.WriteDebugMessage("TSS Resource Group is Paired.");
                            foreach (var mcsResource in getEquipmentResources)
                            {
                                builderCBG = buildFunction(mcsResource.mcs_relatedResourceId, resourceString, builderCBG, count, out count, out resourceString);
                            }
                            foreach (var systemUser in getUserResources)
                            {
                                builderCBG = buildFunction(systemUser.mcs_RelatedUserId, resourceString, builderCBG, count, out count, out resourceString);
                            }
                            break;
                    }
                }
                if (count == 0)
                    builderCBG.Append("<Body>false");
                builderCBG.Append("</Body><Parameters><Parameter name=\"resource\" /></Parameters></Expression></Constraint></Constraints>");

                Logger.WriteDebugMessage("Constraints XML Constructed");
                var updateCBG = new ConstraintBasedGroup
                {
                    Id = thisTSSResourceGroup.mcs_RelatedResourceGroupId.Id,
                    Constraints = builderCBG.ToString()
                };
           
                var updateRG = new mcs_resourcegroup
                {
                    Id = thisTSSResourceGroupId,
                    cvt_resources = resourceString
                };

                //systematic TSS Resource Group naming
                var derivedName = CvtHelper.ReturnRecordNameIfChanged(thisTSSResourceGroup, false, Logger, OrganizationService);
                if (!String.IsNullOrEmpty(derivedName))
                    updateRG.mcs_name = derivedName;

                Logger.WriteDebugMessage("Updating CBG");
                OrganizationService.Update(updateCBG);
                Logger.WriteDebugMessage("Updated ConstraintBasedGroup.");
                OrganizationService.Update(updateRG);
                Logger.WriteDebugMessage("Updated Resource Group.");
                Logger.WriteDebugMessage("System Resource Updated with " + count + " resources. Ending UpdateTSSResourceGroup");
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

        internal static StringBuilder buildFunction(EntityReference field, string resourceString, StringBuilder builderCBG, int count, out int outCount, out string outResourceString)
        {
            //Add the resource name to the text field (for the view)
            resourceString += field.Name.ToString() + " ; ";

            //Add the relatedresourceId to the cbg
            builderCBG.Append(count > 0 ? " || resource[\"Id\"] == " : "<Body>resource[\"Id\"] == ");
            builderCBG.Append(field.Id.ToString("B"));
            count++;
            outCount = count;
            outResourceString = resourceString;
            return builderCBG;
        }


        /// Create Email if Provider was added/changed to the TMP Resource Group of an InterFacility TSA
        internal static void sendEmailIfProvider(Guid grResId, Guid provResGroupId, IOrganizationService OrganizationService, MCSLogger Logger)
        {
            Logger.setMethod = "sendEmailIfProvider";
            Logger.WriteDebugMessage("Starting");
            var body = "";
            var count = 0;
            var provs = "";
            var pluralProvs = " is";
            var pluralService = "";
            var regardingObject = new EntityReference();
            using (var srv = new Xrm(OrganizationService))
            {
                if (grResId != Guid.Empty)
                {
                    var grRes = srv.mcs_groupresourceSet.FirstOrDefault(gr => gr.Id == grResId);
                    regardingObject = new EntityReference(mcs_groupresource.EntityLogicalName, grResId);

                    if (grRes != null)
                    {
                        Logger.WriteDebugMessage("Assessing Addition/Change in Group Resource.");
                        //Change or Addition of Group Resource means that the TSS Resource Group has changed.  If this TSS Resource Group is part of any Interfacility TSAs, then find all TSAs.
                        if (grRes.mcs_RelatedUserId != null)
                        {
                            if (grRes.mcs_relatedResourceGroupId != null)
                            {
                                Logger.WriteDebugMessage("Retrieve the parent TMP Resource Group.");
                                var relatedPRGs = srv.cvt_providerresourcegroupSet.Where(prg => prg.cvt_RelatedResourceGroupid.Id == grRes.mcs_relatedResourceGroupId.Id && prg.cvt_RelatedTSAid != null);

                                foreach (cvt_providerresourcegroup item in relatedPRGs)
                                {
                                        body = checkIFCTSA(item.cvt_RelatedTSAid.Id, body, count, OrganizationService, Logger, out count);
                                }
                            }
                            provs = grRes.mcs_RelatedUserId.Name;
                            Logger.WriteDebugMessage("Provs: " + provs);
                        }
                        else
                            Logger.WriteDebugMessage("Group Resource has to be User to continue.");
                    }
                }
                else if (provResGroupId != Guid.Empty)
                {
                    var provResGroup = srv.cvt_providerresourcegroupSet.FirstOrDefault(prg => prg.Id == provResGroupId && prg.cvt_RelatedTSAid != null);
                    regardingObject = new EntityReference(cvt_providerresourcegroup.EntityLogicalName, provResGroupId);

                    if (provResGroup != null)
                    {
                        Logger.WriteDebugMessage("Assessing Addition/Change in Provider Resource Group.");
                        //Two Scenarios
                        //1. Provider
                        //2. TSS Resource Group
                        var isTrue = false;

                        if (provResGroup.cvt_RelatedUserId != null)
                        {
                            Logger.WriteDebugMessage("Single User through PRG");
                            isTrue = true;
                            provs = provResGroup.cvt_RelatedUserId.Name;
                        }
                        if (provResGroup.cvt_RelatedResourceGroupid != null)
                        {
                            Logger.WriteDebugMessage("TMP Resource Group through PRG");
                            var resGroupChildren = srv.mcs_groupresourceSet.Where(gr => gr.mcs_relatedResourceGroupId.Id == provResGroup.cvt_RelatedResourceGroupid.Id);
                            foreach (mcs_groupresource child in resGroupChildren)
                            {
                                Logger.WriteDebugMessage("Within Foreach loop");
                                if (child.mcs_RelatedUserId != null)
                                {
                                    if (!String.IsNullOrEmpty(provs))
                                    {
                                        provs += "; ";
                                        pluralProvs = "s are";
                                    }

                                    provs += child.mcs_RelatedUserId.Name;
                                    isTrue = true;
                                }
                            }
                        }
                        Logger.WriteDebugMessage("Provs: " + provs);

                        if (isTrue && provResGroup.cvt_RelatedTSAid != null)
                                body = checkIFCTSA(provResGroup.cvt_RelatedTSAid.Id, body, count, OrganizationService,Logger, out count);
                    }
                }
                Logger.WriteDebugMessage("Body string: " + body);
                //Creating the email
                if (!String.IsNullOrEmpty(body))
                {
                    Logger.WriteDebugMessage("Body string not empty");
                    if (count > 1)
                        pluralService = "s";

                    Email email = new Email()
                    {
                        Subject = "Changing provider(s) for telemedicine service",
                        Description = String.Format("A provider has changed for the following service{0}.<br/>The new provider{1}: {2}.<br/><br/>{3}<br/><br/>This is an automated notification from the Telehealth Management Platform.", pluralService, pluralProvs, provs, body),
                        RegardingObjectId = regardingObject
                    };
                    OrganizationService.Create(email);
                    Logger.WriteDebugMessage("Created New Provider email from Group Resource.");
                }
                else
                    Logger.WriteDebugMessage("Not creating email.");

            }
            Logger.WriteDebugMessage("Ending");
        }

        /// <summary>
        /// /Check if TSA is interfacility, if yes, then return name
        /// </summary>
        internal static string checkIFCTSA(Guid tsaId, string body, int count, IOrganizationService OrganizationService, MCSLogger Logger, out int newCount)
        {
            Logger.WriteDebugMessage("Starting checkIFCTSA");
            using (var srv = new Xrm(OrganizationService))
            {
                newCount = count;
                var tsa = OrganizationService.Retrieve(cvt_facilityapproval.EntityLogicalName, tsaId, new ColumnSet("cvt_name", "cvt_patientfacility","cvt_providerfacility"));
                cvt_facilityapproval tsaCast = (cvt_facilityapproval)tsa;

                //if (tsa != null && tsaCast.cvt_ServiceScope != null && tsaCast.cvt_ServiceScope.Value == (int)mcs_servicescvt_ServiceScope.InterFacility)
                if (tsa != null && tsaCast.cvt_patientfacility != null && tsaCast.cvt_providerfacility != null && (tsaCast.cvt_providerfacility != tsaCast.cvt_patientfacility))
                {
                    body += "Name of TSA: " + tsaCast.cvt_name + "<br/>";
                    newCount++;
                    Logger.WriteDebugMessage("Found IFC TSA, name: " + tsaCast.cvt_name);
                }
                return body;
            }
        }
        #endregion
        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_groupresourceplugin"; }
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