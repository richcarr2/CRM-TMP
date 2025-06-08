using System;
using System.Linq;
using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.UpdateClinic
{
    /// <summary>
    /// Create and Save CRM entities.
    /// </summary>
    public class CreateAndSaveEntitiesStep : IFilter<UpdateClinicStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CreateAndSaveEntitiesStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(UpdateClinicStateObject state)
        {
            mcs_integrationresult interationResult = null;
            OrganizationServiceContext orgContext = null;
            var debugMessage = string.Empty;
            var integrationStatus = (int)mcs_integrationresultmcs_status.Error;

            if (state.Clinic == null)
            {
                _logger.Info("Clinic in the state object is null. Skipping Clinic Update in VA.TMP.Integration.HealthShare.PipelineSteps.UpdateClinic.CreateAndSaveEntitiesStep.Execute");
            }
            else
            {
                var clinicResource = state.Clinic;
                orgContext = new OrganizationServiceContext(state.OrganizationServiceProxy);
                interationResult = CreateIntegrationResult(clinicResource.mcs_resourceId, state, orgContext, _logger);

                if (clinicResource.mcs_RelatedSiteId == null)
                {
                    debugMessage += $"Error: The site with station number {clinicResource.cvt_StationNumber} does not exists in TMP.\n";
                }

                // Create a clinic if TMP doesn't have a clinic available with matching IEN
                if (clinicResource.mcs_resourceId == null)
                {
                    debugMessage += "Clinic with IEN supplied in the HealthShare message does not match with any clinic record's IEN available in TMP.\n";
                    if (clinicResource.mcs_RelatedSiteId == null)
                    {
                        debugMessage += "Skipping the clinic record creation as site with station number does not exist";
                        integrationStatus = (int)mcs_integrationresultmcs_status.Error;
                        UpdateIntegrationResult(interationResult, orgContext, integrationStatus, _logger, debugMessage);
                        throw new MissingStationNumberException(debugMessage);
                    }
                    else
                    {
                        debugMessage += "Hence creating a new Clinic in TMP";
                        _logger.Info(debugMessage);
                        orgContext.AddObject(clinicResource);
                        var response = orgContext.SaveChanges();
                        if (response != null && response.Count > 0)
                        {
                            foreach (var saveChangesResult in response)
                            {
                                if (saveChangesResult.Response != null)
                                {
                                    var resourceId = ((Microsoft.Xrm.Sdk.Messages.CreateResponse)saveChangesResult.Response).id;
                                    interationResult.cvt_resourceid = new EntityReference(mcs_resource.EntityLogicalName, resourceId);
                                }
                            }
                        }
                        integrationStatus = (int)mcs_integrationresultmcs_status.Complete;
                    }
                }
                else
                {
                    debugMessage += $"Clinic with Id: {clinicResource.mcs_resourceId.Value} and IEN: {clinicResource.cvt_ien} available in TMP. Updating the Clinic in TMP";
                    ActivateClinicStatus(clinicResource.mcs_resourceId.Value, state.OrganizationServiceProxy, _logger);
                    orgContext.Attach(clinicResource);
                    orgContext.UpdateObject(clinicResource);
                    orgContext.SaveChanges();
                    integrationStatus = (int)mcs_integrationresultmcs_status.Complete;
                }

                if (!string.IsNullOrWhiteSpace(state.RequestMessage.ClinicStatus) &&
                    state.RequestMessage.ClinicStatus.ToLowerInvariant() == "i")
                {
                    UpdateClinicStatus(interationResult.cvt_resourceid.Id, state.OrganizationServiceProxy, false, _logger);
                }
            }
            UpdateIntegrationResult(interationResult, orgContext, integrationStatus, _logger, debugMessage);
        }

        /// <summary>
        /// Activate the clinic Status in case if it's Inactive for updation
        /// </summary>
        /// <param name="clinicId">The Clinic Guid</param>
        /// <param name="svc">The Organization Service object</param>
        private static void ActivateClinicStatus(Guid clinicId, IOrganizationService svc, ILog logger)
        {
            using (var context = new Xrm(svc))
            {
                var clinic = context.mcs_resourceSet.FirstOrDefault(s => s.Id == clinicId);
                if (clinic != null && clinic.statecode == mcs_resourceState.Inactive)
                    UpdateClinicStatus(clinicId, svc, true, logger);
            }
        }

        /// <summary>
        /// Update the clinic Status based on the request
        /// </summary>
        /// <param name="clinicId">The Clinic Guid</param>
        /// <param name="svc">The Organization Service object</param>
        /// <param name="shouldActivate">Should the clinic record be activated</param>
        private static void UpdateClinicStatus(Guid clinicId, IOrganizationService svc, bool shouldActivate, ILog logger)
        {
            var state = new SetStateRequest
            {
                State = new OptionSetValue(shouldActivate ? (int)mcs_resourceState.Active : (int)mcs_resourceState.Inactive),
                Status = new OptionSetValue(shouldActivate ? (int)mcs_resource_statuscode.Active : (int)mcs_resource_statuscode.Inactive),
                EntityMoniker = new EntityReference(mcs_resource.EntityLogicalName, clinicId)
            };

            svc.Execute(state);
        }

        /// <summary>
        /// Update the integration status record with the provided status and error message (if any)
        /// </summary>
        /// <param name="interationResult">Integration result record object</param>
        /// <param name="orgContext">Organization Service Context</param>
        /// <param name="status">The Integration Result status value to be set</param>
        /// <param name="errorMessage">Error message details (Optional)</param>
        private static void UpdateIntegrationResult(mcs_integrationresult interationResult, OrganizationServiceContext orgContext, int status, ILog logger, string errorMessage = null)
        {
            if (interationResult != null && orgContext != null)
            {
                interationResult.mcs_error = errorMessage;
                interationResult.mcs_status = new OptionSetValue(status);
                orgContext.Attach(interationResult);
                orgContext.UpdateObject(interationResult);
                orgContext.SaveChanges();
            }
        }

        /// <summary>
        /// Create the Integration Result Record
        /// </summary>
        /// <param name="resourceId">Associated Resource (Clinic) Record Id</param>
        /// <param name="state">The UpdateClinicStateObject</param>
        /// <param name="orgContext">Organization Context.</param>
        /// <returns>the Integration Result Object created </returns>
        private static mcs_integrationresult CreateIntegrationResult(Guid? resourceId, UpdateClinicStateObject state, OrganizationServiceContext orgContext, ILog logger)
        {
            var newIntegrationResult = new mcs_integrationresult
            {
                mcs_name = "Update Clinic from HealthShare",
                mcs_integrationrequest = state.SerializedRequestMessage,
                mcs_VimtRequestMessageType = typeof(TmpHealthShareUpdateClinicRequestMessage).FullName,
                mcs_VimtResponseMessageType = typeof(TmpHealthShareUpdateClinicResponseMessage).FullName,
                mcs_VimtMessageRegistryName = "TmpHealthShareUpdateClinicRequestMessage"
            };

            if (resourceId != null && resourceId != Guid.Empty) newIntegrationResult.cvt_resourceid = new EntityReference(mcs_resource.EntityLogicalName, resourceId.Value);

            orgContext.AddObject(newIntegrationResult);
            orgContext.SaveChanges();

            return newIntegrationResult;
        }
    }
}
