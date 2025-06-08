using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsGroupResourceUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public McsGroupResourceUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }       
        #endregion
             
        public override void Execute()
        {
            UpdateResourceGroup(PrimaryEntity.Id);

            Logger.WriteDebugMessage("GR Update: About to Check if Provider was updated to a TSA.");
            McsGroupResourceCreatePostStageRunner.sendEmailIfProvider(PrimaryEntity.Id, Guid.Empty, OrganizationService, Logger);
        }
        #region Logic
        /// <summary>
        /// Update the TSS Resource Group with the list of new resources string and record Name if needed
        /// Updated the OOB Resource Group (Constraint Based Group) with the new XML Constraints
        /// </summary>
        /// <param name="thisId"></param>
        internal void UpdateResourceGroup(Guid thisId)
        {
            Logger.setMethod = "UpdateResourceGroup";
            Logger.WriteDebugMessage("starting UpdateResourceGroup");
            using (var srv = new Xrm(OrganizationService))
            {
                try
                {
                    var thisGroupResource = srv.mcs_groupresourceSet.FirstOrDefault(g => g.Id == thisId);
                    if (thisGroupResource.mcs_relatedResourceGroupId == null)
                        return;

                    var RelatedResourceGroupId = thisGroupResource.mcs_relatedResourceGroupId.Id;
                    var resourceGroup = srv.mcs_resourcegroupSet.FirstOrDefault(i => i.Id == RelatedResourceGroupId);

                    if (resourceGroup == null) 
                        return;

                    Logger.WriteDebugMessage("mcs_resourcegroup was found");

                    var resourceNames = "";
                    var count = 0;
                    var builder = CvtHelper.GetResources(RelatedResourceGroupId, srv, out resourceNames, out count);
                    builder = CvtHelper.BuildConstraintsXML(builder);
                    Logger.WriteDebugMessage("Built XML");
                                      
                    if (resourceNames != resourceGroup.cvt_resources)
                    {
                        var resGroupUpdate = new mcs_resourcegroup
                        {
                            Id = resourceGroup.Id,
                            cvt_resources = resourceNames
                        };

                        //systematic TSS Resource Group naming
                        var derivedName = CvtHelper.ReturnRecordNameIfChanged(resourceGroup, false, Logger, OrganizationService);
                        if (!String.IsNullOrEmpty(derivedName))
                            resGroupUpdate.mcs_name = derivedName;

                        OrganizationService.Update(resGroupUpdate);
                        Logger.WriteDebugMessage("Updated Resource Group with new string: " + resourceNames);

                        if (resourceGroup.mcs_RelatedResourceGroupId != null)
                        {
                            //Could check if builder = constraints to see if update even needs to happen
                            var group = new ConstraintBasedGroup
                            {
                                Id = resourceGroup.mcs_RelatedResourceGroupId.Id,
                                Constraints = builder.ToString()
                            };
                            OrganizationService.Update(group);
                            Logger.WriteDebugMessage("System Resource Updated with " + count + " resources");
                        }
                    }
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
        }
 
    #endregion

        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_serviceplugin"; }
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