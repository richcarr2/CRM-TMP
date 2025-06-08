using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Save Entities Step.
    /// </summary>
    public class SaveEntitiesStep : IFilter<MakeCancelInboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SaveEntitiesStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelInboundStateObject state)
        {
            using (var orgContext = new OrganizationServiceContext(state.OrganizationServiceProxy))
            {
                UpdateIntegrationResult(state, orgContext);
                CreateOrUpdateVistaIntegrationResult(state, orgContext);

                orgContext.SaveChanges();

                var setStateResult = SetServiceAppointmentStatus(state);
                if (setStateResult != null) orgContext.Execute(setStateResult);
            }
        }

        /// <summary>
        /// Update the Integration Result.
        /// </summary>
        /// <param name="state">State object.</param>
        /// <param name="svc">Organization Service Proxy.</param>
        private static void UpdateIntegrationResult(MakeCancelInboundStateObject state, OrganizationServiceContext svc)
        {
            var integrationResult = new mcs_integrationresult
            {
                Id = state.IntegrationResult.Id
            };

            if (state.ErrorMakeCancelToVista)
            {
                integrationResult.mcs_error = $"Error HealthShare Inbound: {state.RequestMessage.Status}:{state.RequestMessage.FailureReason}. " + state.IntegrationResult.mcs_error;
                integrationResult.mcs_status = new OptionSetValue((int)mcs_integrationresultmcs_status.Error);
            }
            else
            {
                integrationResult.mcs_status = new OptionSetValue((int)mcs_integrationresultmcs_status.Complete);
            }

            svc.Attach(integrationResult);
            svc.UpdateObject(integrationResult);
        }

        /// <summary>
        /// Create Vista Integration Result.
        /// </summary>
        /// <param name="state">State object.</param>
        /// <param name="svc">Organization Service Proxy.</param>
        private static void CreateOrUpdateVistaIntegrationResult(MakeCancelInboundStateObject state, OrganizationServiceContext svc)
        {
            if (state.OutboundEcRequestMessage.VisitStatus == VistaStatus.SCHEDULED.ToString())
            {
                switch (state.AppointmentType)
                {
                    case AppointmentType.HOME_MOBILE:
                        svc.AddObject(CreateProviderSideVistaIntegrationResult(state));
                        break;

                    case AppointmentType.STORE_FORWARD:
                        svc.AddObject(CreatePatientSideVistaIntegrationResult(state));
                        break;

                    default:
                        svc.AddObject(CreatePatientSideVistaIntegrationResult(state));
                        svc.AddObject(CreateProviderSideVistaIntegrationResult(state));
                        break;
                }
            }
            else
            {
                switch (state.AppointmentType)
                {
                    case AppointmentType.HOME_MOBILE:
                    case AppointmentType.STORE_FORWARD:
                        var vistaIntegrationResult = GetVistaIntegrationResultForCancel(state);
                        svc.Attach(vistaIntegrationResult);
                        svc.UpdateObject(vistaIntegrationResult);
                        break;

                    default:
                        var vistaIntegrationResults = GetVistaIntegrationResultsForCancel(state).ToList();
                        foreach (var vir in vistaIntegrationResults)
                        {
                            svc.Attach(vir);
                            svc.UpdateObject(vir);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Create Patient Side Vista Integration Result.
        /// </summary>
        /// <param name="state">State Object.</param>
        /// <returns>Vista Integration Result.</returns>
        private static cvt_vistaintegrationresult CreatePatientSideVistaIntegrationResult(MakeCancelInboundStateObject state)
        {
            var clinic = state.OutboundEcRequestMessage.Patient.FirstOrDefault() != null ? state.OutboundEcRequestMessage.Patient.First().ClinicIen : string.Empty;
            var facility = state.OutboundEcRequestMessage.Patient.FirstOrDefault() != null ? state.OutboundEcRequestMessage.Patient.First().Facility : string.Empty;

            var vistaIntegrationResult = GetVistaIntegrationResult(state);
            vistaIntegrationResult.cvt_name = $"{state.OutboundEcRequestMessage.PatientName} @ {clinic} ({facility})";
            vistaIntegrationResult.cvt_ClinicIEN = clinic;
            vistaIntegrationResult.cvt_FacilityCode = facility;

            return vistaIntegrationResult;
        }

        /// <summary>
        /// Create Provider Side Vista Integration Result.
        /// </summary>
        /// <param name="state">State Object.</param>
        /// <returns>Vista Integration Result.</returns>
        private static cvt_vistaintegrationresult CreateProviderSideVistaIntegrationResult(MakeCancelInboundStateObject state)
        {
            var clinic = state.OutboundEcRequestMessage.Provider.FirstOrDefault() != null ? state.OutboundEcRequestMessage.Provider.First().ClinicIen : string.Empty;
            var facility = state.OutboundEcRequestMessage.Provider.FirstOrDefault() != null ? state.OutboundEcRequestMessage.Provider.First().Facility : string.Empty;

            var vistaIntegrationResult = GetVistaIntegrationResult(state);
            vistaIntegrationResult.cvt_name = $"{state.OutboundEcRequestMessage.PatientName} @ {clinic} ({facility})";
            vistaIntegrationResult.cvt_ClinicIEN = clinic;
            vistaIntegrationResult.cvt_FacilityCode = facility;

            return vistaIntegrationResult;
        }

        /// <summary>
        /// Create base level Vista Integration Result.
        /// </summary>
        /// <param name="state">State Object.</param>
        /// <returns>Vista Integration Result.</returns>
        private static cvt_vistaintegrationresult GetVistaIntegrationResult(MakeCancelInboundStateObject state)
        {
            var startTime = state.ServiceAppointment.ScheduledStart ?? DateTime.UtcNow;

            var vistaIntegrationResult = new cvt_vistaintegrationresult
            {
                // TODO - use UTC to local conversion - See VIA MakeAppointmentMapper for an example
                cvt_DateTime = startTime.ToString("yyyyMMddHHmm"),
                cvt_PatientName = state.OutboundEcRequestMessage.PatientName,
                cvt_PersonId = state.OutboundEcRequestMessage.PatientIcn,
                cvt_ServiceActivity = new EntityReference { Id = state.ServiceAppointment.Id, LogicalName = ServiceAppointment.EntityLogicalName },
                cvt_ParentResult = new EntityReference { Id = state.IntegrationResult.Id, LogicalName = mcs_integrationresult.EntityLogicalName },
                cvt_Veteran = new EntityReference { Id = state.PatientId, LogicalName = Contact.EntityLogicalName },
                cvt_VistAStatus = state.ErrorMakeCancelToVista ? VistaStatus.FAILED_TO_SCHEDULE.ToString() : VistaStatus.SCHEDULED.ToString(),
                cvt_Reason = state.ErrorMakeCancelToVista ? $"{state.RequestMessage.Status}: {state.RequestMessage.FailureReason}".TrimEnd() : state.OutboundEcRequestMessage.CancelReason
            };

            if (state.IsGroupAppointment) vistaIntegrationResult.cvt_Appointment = new EntityReference(Appointment.EntityLogicalName, state.Appointment.Id);

            return vistaIntegrationResult;
        }

        /// <summary>
        /// Get existing Vista Integration Result and Update for Cancel.
        /// </summary>
        /// <param name="state">State Object.</param>
        /// <returns>Vista Integration Result.</returns>
        private static cvt_vistaintegrationresult GetVistaIntegrationResultForCancel(MakeCancelInboundStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var result = srv.cvt_vistaintegrationresultSet.FirstOrDefault(x =>
                    x.cvt_ServiceActivity.Id == state.ServiceAppointment.Id &&
                    x.cvt_Veteran.Id == state.PatientId &&
                    x.cvt_PersonId == state.OutboundEcRequestMessage.PatientIcn);

                if (result == null) throw new MissingVistaIntegrationResultException($"Unable to find a Vista Integration Result for IR: {state.IntegrationResult.Id}");

                return GetVistaIntegrationResult(state, result.Id);
            }
        }

        /// <summary>
        /// Get existing Vista Integration results for Group and Clinic based.
        /// </summary>
        /// <param name="state">State Object.</param>
        /// <returns>Collection of Vista Integration Result.</returns>
        private static IEnumerable<cvt_vistaintegrationresult> GetVistaIntegrationResultsForCancel(MakeCancelInboundStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                var results = srv.cvt_vistaintegrationresultSet.Where(x =>
                    x.cvt_ServiceActivity.Id == state.ServiceAppointment.Id &&
                    x.cvt_Veteran.Id == state.PatientId &&
                    x.cvt_PersonId == state.OutboundEcRequestMessage.PatientIcn).Select(x => x.Id).ToList();

                if (!results.Any()) throw new MissingVistaIntegrationResultException($"Unable to find a Vista Integration Results for IR: {state.IntegrationResult.Id}");

                foreach (var result in results)
                {
                    yield return GetVistaIntegrationResult(state, result);
                }
            }
        }

        /// <summary>
        /// Get Vista Integration Result for Cancel.
        /// </summary>
        /// <param name="state">State Object.</param>
        /// <param name="vistaIntegrationResultId">Vista Integration Result Id.</param>
        /// <returns>cvt_vistaintegrationresult</returns>
        private static cvt_vistaintegrationresult GetVistaIntegrationResult(MakeCancelInboundStateObject state, Guid vistaIntegrationResultId)
        {
            return new cvt_vistaintegrationresult
            {
                Id = vistaIntegrationResultId,
                cvt_VistaReasonCode = state.OutboundEcRequestMessage.CancelReason,
                cvt_VistaStatusCode = state.OutboundEcRequestMessage.CancelCode,
                cvt_VistAStatus = state.ErrorMakeCancelToVista ? VistaStatus.FAILED_TO_CANCEL.ToString() : VistaStatus.CANCELED.ToString(),
                cvt_Reason = state.ErrorMakeCancelToVista ? $"{state.RequestMessage.Status}: {state.RequestMessage.FailureReason}".TrimEnd() : state.OutboundEcRequestMessage.CancelReason
            };
        }

        /// <summary>
        /// Update the Service Appointment Status.
        /// </summary>
        /// <param name="state">State object.</param>
        /// <returns>Set State Request.</returns>
        private static SetStateRequest SetServiceAppointmentStatus(MakeCancelInboundStateObject state)
        {
            if (state.OutboundEcRequestMessage.VisitStatus.ToUpper() == VistaStatus.SCHEDULED.ToString())
            {
                return GetSetStateRequest(new EntityReference(ServiceAppointment.EntityLogicalName, state.ServiceAppointment.Id),
                    new OptionSetValue((int)ServiceAppointmentState.Scheduled),
                    !state.ErrorMakeCancelToVista
                        ? new OptionSetValue((int)serviceappointment_statuscode.ReservedScheduled)
                        : new OptionSetValue((int)serviceappointment_statuscode.InterfaceVIMTFailure));
            }

            return state.ErrorMakeCancelToVista
                ? GetSetStateRequest(new EntityReference(ServiceAppointment.EntityLogicalName, state.ServiceAppointment.Id),
                    new OptionSetValue((int)ServiceAppointmentState.Canceled),
                    new OptionSetValue((int)serviceappointment_statuscode.CancelFailure))
                : null;
        }

        /// <summary>
        /// Gets Set State Request
        /// </summary>
        /// <param name="entityMoniker">Entity Reference.</param>
        /// <param name="state">State.</param>
        /// <param name="status">Status.</param>
        /// <returns>SetStateRequest.</returns>
        private static SetStateRequest GetSetStateRequest(EntityReference entityMoniker, OptionSetValue state, OptionSetValue status)
        {
            return new SetStateRequest { EntityMoniker = entityMoniker, State = state, Status = status };
        }
    }
}
