using System;
using System.Collections.Generic;
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

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancel
{
    /// <summary>
    /// Create and Save CRM entities.
    /// </summary>
    public class CreateAndSaveEntitiesStep : IFilter<MakeCancelStateObject>
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
        public void Execute(MakeCancelStateObject state)
        {
            var appointment = state.Appointment;
            var orgContext = new OrganizationServiceContext(state.OrganizationServiceProxy);

            _logger.Info($" Cancel code {appointment.cvt_CancelCode}");

            switch (appointment.cvt_CancelCode)
            {
                case "NCD":
                    // Add Reserve Resource for the Non-Available Clinic
                    _logger.Info($"NCD Request: Creating Reserve Resource");
                    Guid IntegrationResultId = CreateIntegrationResult(appointment.ActivityId, state, orgContext);
                    CreateReserveResource(orgContext, appointment, IntegrationResultId, state);
                    _logger.Info($"NCD Request: Completed Creating Reserve Resource");
                    break;
                case "RCD":
                    // Restore/Delete Bloked Reserve Resource for the Clinic
                    _logger.Info($"RCD Request: Deleting Reserve Resource");
                    IntegrationResultId = CreateIntegrationResult(appointment.ActivityId, state, orgContext);
                    DeleteReserveResource(appointment, state, IntegrationResultId);
                    _logger.Info($"RCD Request: Completed Deleting Reserve Resource");
                    break;
                default:
                    // Appointment update code 
                    if (appointment == null)
                    {
                        var err = "Appointment is null. Cannot process Make/Cancel Appointment at step VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancel.CreateandSaveEntities.cs";
                        _logger.Error(err);
                    }
                    else
                    {
                        try
                        {
                            if (!appointment.ScheduledStart.HasValue)
                            {
                                if (DateTime.TryParse(state.RequestMessage.StartTime, out var startTime))
                                {
                                    _logger.Debug($"Setting missing Scheduled Start Time to: {startTime}");
                                    appointment.ScheduledStart = startTime;
                                }
                            }

                            if (!appointment.ScheduledDurationMinutes.HasValue) { appointment.ScheduledDurationMinutes = state.RequestMessage.Duration; }

                            if (appointment.ScheduledDurationMinutes != null && appointment.ScheduledStart != null)
                                appointment.ScheduledEnd = appointment.ScheduledStart.Value.AddMinutes((double)appointment.ScheduledDurationMinutes);

                            appointment.Subject = $"VISTA - {appointment.cvt_ClinicName ?? appointment.cvt_ConsultName} @ {appointment.cvt_FacilityText}";

                            if (appointment.RequiredAttendees == null && !appointment.cvt_VisitStatus.Equals("NO-SHOW"))
                            {
                                var err = "Required Attendees is null. Reserve Resource will not appear on calendar without Required Attendees. At step VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancel.CreateandSaveEntities.cs";
                                _logger.Error(err);

                                CreateIntegrationResult(appointment.ActivityId, state, orgContext);
                            }
                            else
                            {
                                var status = state.RequestMessage?.VisitStatus;
                                if (!string.IsNullOrWhiteSpace(status) && status.ToUpperInvariant().Equals("NO-SHOW"))
                                {
                                    // We have to search for the *existing appointment to cancel 
                                    using (var service = new Xrm(state.OrganizationServiceProxy))
                                    {
                                        var existingAppointment = appointment.ScheduledStart.HasValue
                                            ? service.AppointmentSet.FirstOrDefault(
                                                i => i.cvt_ClinicIEN == appointment.cvt_ClinicIEN &&
                                                    i.cvt_PatientICN == appointment.cvt_PatientICN &&
                                                    i.ScheduledStart == ((DateTime)appointment.ScheduledStart).ToUniversalTime() &&
                                                    i.StateCode != AppointmentState.Canceled
                                                )
                                            : null;

                                        // We found the appointment, now we can update it to NO-SHOW.
                                        if (existingAppointment != null)
                                        {
                                            var req = new SetStateRequest
                                            {
                                                EntityMoniker = new EntityReference("appointment", existingAppointment.Id),
                                                State = new OptionSetValue((int)AppointmentState.Scheduled),
                                                Status = new OptionSetValue(178970001)
                                            };
                                            service.Execute(req);

                                            _logger.Info($"Update a Reserve Resource with Clinic IEN: {appointment.cvt_ClinicIEN}, PatientICN: {appointment.cvt_PatientICN}, ScheduledStart: {appointment.ScheduledStart} to a Status Reason of No-Show");

                                            CreateIntegrationResult(appointment.ActivityId, state, orgContext);
                                        }
                                        else
                                        {
                                            var patient = appointment.OptionalAttendees.First();

                                            var existingSAs = service.ServiceAppointmentSet.Where(sa =>
                                                (sa.mcs_relatedsite.Id.Equals(appointment.cvt_Site.Id) || sa.mcs_relatedprovidersite.Id.Equals(appointment.cvt_Site.Id)) &&
                                                sa.ScheduledStart.Equals(appointment.ScheduledStart)).ToList();

                                            if (existingSAs != null && !existingSAs.Count.Equals(0))
                                            {
                                                ServiceAppointment existingSA = null;

                                                if (existingSAs.Count > 1)
                                                {
                                                    existingSAs.ToList().ForEach(sa =>
                                                    {
                                                            //Find the Appointment for the NO-SHOW Patient
                                                            sa.Customers.ToList().ForEach(ap =>
                                                        {
                                                            if (ap.PartyId.Id.Equals(patient.PartyId.Id))
                                                            {
                                                                if (existingSA == null) existingSA = sa;
                                                            }
                                                        });
                                                    });
                                                }
                                                else
                                                {
                                                    existingSA = existingSAs.First();
                                                }

                                                var req = new SetStateRequest
                                                {
                                                    EntityMoniker = new EntityReference("serviceappointment", existingSA.Id),
                                                    State = new OptionSetValue((int)ServiceAppointmentState.Scheduled),
                                                    Status = new OptionSetValue(917290010)
                                                };
                                                service.Execute(req);

                                                CreateIntegrationResult(existingSA, state, orgContext);
                                            }
                                            else
                                            {
                                                _logger.Info($"ERROR: Could not find an appointment with Clinic IEN: {appointment.cvt_ClinicIEN}, PatientICN: {appointment.cvt_PatientICN}, ScheduledStart: {appointment.ScheduledStart}");
                                                //throw new MissingAppointmentException($"ERROR: Could not find an appointment with Clinic IEN: {state.RequestMessage?.ClinicIen}, PatientICN: {state.RequestMessage?.PatientIcn}, ScheduledStart: {state.RequestMessage?.StartTime}");
                                            }
                                        }
                                    }
                                }
                                else if (!string.IsNullOrWhiteSpace(status) && status.ToUpperInvariant() != "SCHEDULED")
                                {
                                    // We have to search for the *existing appointment to cancel 
                                    using (var service = new Xrm(state.OrganizationServiceProxy))
                                    {
                                        var existingAppointment = appointment.ScheduledStart.HasValue
                                            ? service.AppointmentSet.FirstOrDefault(
                                                i => i.cvt_ClinicIEN == appointment.cvt_ClinicIEN &&
                                                    i.cvt_PatientICN == appointment.cvt_PatientICN &&
                                                    i.ScheduledStart == ((DateTime)appointment.ScheduledStart).ToUniversalTime() &&
                                                    i.StateCode != AppointmentState.Canceled
                                                )
                                            : null;

                                        // We found the appointment, now we can cancel it.
                                        if (existingAppointment != null)
                                        {
                                            service.DeleteObject(existingAppointment);
                                            service.SaveChanges();
                                            _logger.Info($"Deleted a Reserve Resource with Clinic IEN: {appointment.cvt_ClinicIEN}, PatientICN: {appointment.cvt_PatientICN}, ScheduledStart: {appointment.ScheduledStart}");

                                            CreateIntegrationResult(appointment.ActivityId, state, orgContext);
                                        }
                                        else
                                        {
                                            _logger.Info($"ERROR: Could not find an appointment with Clinic IEN: {appointment.cvt_ClinicIEN}, PatientICN: {appointment.cvt_PatientICN}, ScheduledStart: {appointment.ScheduledStart}");
                                            //throw new MissingAppointmentException($"ERROR: Could not find an appointment with Clinic IEN: {state.RequestMessage?.ClinicIen}, PatientICN: {state.RequestMessage?.PatientIcn}, ScheduledStart: {state.RequestMessage?.StartTime}");
                                        }
                                    }
                                }
                                else //we can schedule one
                                {
                                    orgContext.AddObject(appointment);
                                    orgContext.SaveChanges();


                                    using (var service = new Xrm(state.OrganizationServiceProxy))
                                    {
                                        var cAppt = service.AppointmentSet.FirstOrDefault(i => i.ActivityId == appointment.ActivityId);

                                        if (cAppt != null)
                                        {
                                            var req = new SetStateRequest
                                            {
                                                EntityMoniker = new EntityReference("appointment", cAppt.Id),
                                                State = new OptionSetValue((int)AppointmentState.Scheduled),
                                                Status = new OptionSetValue((int)appointment_statuscode.Busy)
                                            };
                                            service.Execute(req);
                                        }

                                        service.UpdateObject(cAppt);

                                    }
                                    orgContext.Attach(appointment);

                                    CreateIntegrationResult(appointment.ActivityId, state, orgContext);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"ERROR: The following error occurred while trying to update the Appointment:\n{e}");
                            throw;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgContext"></param>
        /// <param name="appointment"></param>
        /// <param name="logger"></param>
        /// <param name="integrationResultId"></param>
        /// <param name="state"></param>
        private void DeleteReserveResource(Appointment appointment, MakeCancelStateObject state, Guid integrationResultId)
        {
            if (!appointment.ScheduledStart.HasValue)
            {
                if (DateTime.TryParse(state.RequestMessage.StartTime, out var startTime))
                {
                    _logger.Debug($"Setting missing Scheduled Start Time to: {startTime}");
                    appointment.ScheduledStart = startTime;
                }
            }

            if (!appointment.ScheduledStart.HasValue)
            {
                _logger.Error("ERROR: Could not delete Reserve Resource due to missing ScheduledStart");
                return;
            }

            try
            {
                _logger.Debug("Inside DeleteReserveResource method - Start DateTime: " + appointment.ScheduledStart);
                var status = state.RequestMessage?.VisitStatus;
                if (!string.IsNullOrWhiteSpace(status) && status.ToUpperInvariant() != "SCHEDULED")
                {
                    // Search for an existing Reserve Resource to Restore the block
                    _logger.Debug("Search for an existing Reserve Resource");
                    using (var service = new Xrm(state.OrganizationServiceProxy))
                    {
                        var existingAppointment = service.AppointmentSet.FirstOrDefault(
                               i => i.cvt_ClinicIEN == appointment.cvt_ClinicIEN &&
                                    i.ScheduledStart == ((DateTime)appointment.ScheduledStart).ToUniversalTime() &&
                                    i.ScheduledDurationMinutes == appointment.ScheduledDurationMinutes &&
                                    i.StateCode != AppointmentState.Canceled &&
                                    i.cvt_Facility == appointment.cvt_Facility &&
                                    i.cvt_ClinicName == appointment.cvt_ClinicName &&
                                    i.cvt_stationnumber == appointment.cvt_stationnumber
                            );
                        _logger.Debug("Check if Reserve Resource found");
                        // Find existing Reserve Resource and Delete it
                        if (existingAppointment != null)
                        {
                            _logger.Debug("Delete Reserve Resource " + existingAppointment.Subject);
                            service.DeleteObject(existingAppointment, false);
                            service.SaveChanges();
                            _logger.Debug("Deleted Reserve Resource ");

                            var integrationResult = new mcs_integrationresult
                            {
                                Id = integrationResultId,
                                mcs_status = new OptionSetValue(803750002)//Completed
                            };

                            service.Attach(integrationResult);
                            service.UpdateObject(integrationResult);
                            service.SaveChanges();
                        }
                        else
                        {
                            var integrationResult = new mcs_integrationresult
                            {
                                Id = integrationResultId,
                                mcs_error = $"ERROR: Could not find an Reserve Resource to Restore - Appointment: { appointment.Subject}, Clinic: { appointment.cvt_ClinicName}, Clinic IEN: { appointment.cvt_ClinicIEN}, ScheduledStart: { appointment.ScheduledStart }",
                                mcs_status = new OptionSetValue(803750001)//Investigation Required
                            };

                            service.Attach(integrationResult);
                            service.UpdateObject(integrationResult);
                            service.SaveChanges();

                            _logger.Info($"ERROR: Could not find an Reserve Resource to Restore - Appointment: {appointment.Subject}, Clinic: {appointment.cvt_ClinicName}, Clinic IEN: {appointment.cvt_ClinicIEN}, ScheduledStart: {appointment.ScheduledStart}");
                        }
                    }
                }
                else
                {
                    using (var service = new Xrm(state.OrganizationServiceProxy))
                    {
                        var integrationResult = new mcs_integrationresult
                        {
                            Id = integrationResultId,
                            mcs_error = $"ERROR: Could not find an VisitStatus to Restore - Appointment: { appointment.Subject}, Clinic: { appointment.cvt_ClinicName}, Clinic IEN: { appointment.cvt_ClinicIEN}, ScheduledStart: { appointment.ScheduledStart }",
                            mcs_status = new OptionSetValue(803750001)//Investigation Required
                        };

                        service.Attach(integrationResult);
                        service.UpdateObject(integrationResult);
                        service.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                using (var service = new Xrm(state.OrganizationServiceProxy))
                {
                    var integrationResult = new mcs_integrationresult
                    {
                        Id = integrationResultId,
                        mcs_error = $"ERROR: Could not find an Reserve Resource to Restore - Appointment: { appointment.Subject}, Clinic: { appointment.cvt_ClinicName}, Clinic IEN: { appointment.cvt_ClinicIEN}, ScheduledStart: { appointment.ScheduledStart }",
                        mcs_status = new OptionSetValue(803750001)//Investigation Required
                    };

                    service.Attach(integrationResult);
                    service.UpdateObject(integrationResult);
                    service.SaveChanges();
                }
                _logger.Error($"ERROR: The following error occurred while attempting to delete the Reserve Resource:\n{e}");
            }
        }

        /// <summary>
        /// Create Reserved Resource
        /// </summary>
        /// <param name="orgContext">Organization Service Context</param>
        /// <param name="appointment">Reserve Resource</param>
        private void CreateReserveResource(OrganizationServiceContext orgContext, Appointment appointment, Guid IntegrationResultId, MakeCancelStateObject state)
        {
            if (!appointment.ScheduledStart.HasValue)
            {
                if (DateTime.TryParse(state.RequestMessage.StartTime, out var startTime))
                {
                    _logger.Debug($"Setting missing Scheduled Start Time to: {startTime}");
                    appointment.ScheduledStart = startTime;
                }
            }

            if (!appointment.ScheduledStart.HasValue)
            {
                _logger.Error("ERROR: Could not create Reserve Resource due to missing ScheduledStart");
            }

            if (appointment.cvt_Site == null)
            {
                _logger.Error("ERROR: Could not create Reserve Resource due to missing Site");
            }

            try
            {
                appointment.Subject = appointment.cvt_CancelReason;

                _logger.Debug("inside CreateReserveResource - Start DateTime: " + appointment.ScheduledStart);
                appointment.ScheduledEnd = appointment.ScheduledStart.Value.AddMinutes((double)appointment.ScheduledDurationMinutes);


                var integrationResult = new mcs_integrationresult
                {
                    mcs_integrationresultId = IntegrationResultId
                };

                if (appointment == null)
                {
                    integrationResult.mcs_error = $"ERROR: Could not create Reserve Resource due to missing Appointment: { appointment.Subject }, Clinic: { appointment.cvt_ClinicName }, Clinic IEN: { appointment.cvt_ClinicIEN }, ScheduledStart: { appointment.ScheduledStart }";
                    integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                }
                else
                {
                    if (appointment.cvt_Facility == null)
                    {
                        integrationResult.mcs_error = $"ERROR: Could not create Reserve Resource due to missing Facility - Station Number: { state.RequestMessage.Facility }, Site: {state.RequestMessage.StationNumber }, Clinic: { appointment.cvt_ClinicName }, Clinic IEN: { appointment.cvt_ClinicIEN } ";
                        integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                    }
                    else if (appointment.cvt_Site == null)
                    {
                        integrationResult.mcs_error = $"ERROR: Could not create Reserve Resource due to missing Site - Station Number: { state.RequestMessage.StationNumber }, Facility: {state.RequestMessage.Facility }, Clinic: { appointment.cvt_ClinicName }, Clinic IEN: { appointment.cvt_ClinicIEN } ";
                        integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                    }
                    else if (!appointment.ScheduledStart.HasValue)
                    {
                        integrationResult.mcs_error = $"ERROR: Could not create Reserve Resource due to missing ScheduledStart - Appointment: { appointment.Subject }, Clinic: { appointment.cvt_ClinicName }, Clinic IEN: { appointment.cvt_ClinicIEN }, ScheduledStart: { appointment.ScheduledStart }";
                        integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                    }
                }

                using (var service = new Xrm(state.OrganizationServiceProxy))
                {
                    var clinic = appointment.cvt_Facility != null
                        ? service.mcs_resourceSet.Where(
                            c => c.cvt_ien == appointment.cvt_ClinicIEN
                                && c.mcs_UserNameInput == appointment.cvt_ClinicName
                                && c.mcs_RelatedSiteId.Id == appointment.cvt_Site.Id
                                && c.mcs_Type.Value == (int)mcs_resourcetype.VistaClinic
                                && c.statecode == 0).OrderBy(c => c.CreatedOn)
                        : null;

                    if (clinic != null)
                    {
                        if (clinic.ToList().Count == 0)
                        {
                            _logger.Info($"Cannot find clinic with {appointment.cvt_ClinicIEN}");

                            integrationResult.mcs_error = $"ERROR: Could not create Reserve Resource due to missing Clinic: { appointment.cvt_ClinicName }, Clinic IEN: { appointment.cvt_ClinicIEN}, Site: { state.RequestMessage.StationNumber }, Facility: { state.RequestMessage.Facility }";
                            integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                        }
                        else
                        {
                            var firstClinic = clinic.First<mcs_resource>();
                            _logger.Info($"firstClinic is {firstClinic.Id}");
                            if (firstClinic.mcs_relatedResourceId != null)
                            {
                                if (firstClinic.mcs_relatedResourceId.Id != null)
                                {
                                    var reqParty = new ActivityParty
                                    {
                                        ["partyid"] = new EntityReference(Equipment.EntityLogicalName, firstClinic.mcs_relatedResourceId.Id)
                                    };
                                    appointment.RequiredAttendees = new List<ActivityParty> { reqParty };
                                }
                                else
                                {
                                    // Log it when there's no required attendees, otherwise reserve resource will not appear on service calendar
                                    _logger.Info($"Clinic : {firstClinic.Id} does not have required attendees in the reserve resource populated");
                                    integrationResult.mcs_error = $"ERROR: Could not create Reserve Resource due to Reserve Resource missing required attendees. Clinic : { appointment.cvt_ClinicName }, Clinic IEN: { appointment.cvt_ClinicIEN}, Site: { state.RequestMessage.StationNumber }, Facility: { state.RequestMessage.Facility }";
                                    integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                                }
                            }
                            else
                            {
                                // Log it when we can't find a provider
                                _logger.Info($"Clinic : {firstClinic.Id} does not have a mcs_relatedResourceId populated");
                                integrationResult.mcs_error = $"ERROR: Could not create Reserve Resource due to Clinic missing mcs_relatedResourceId. Clinic : { appointment.cvt_ClinicName }, Clinic IEN: { appointment.cvt_ClinicIEN}, Site: { state.RequestMessage.StationNumber }, Facility: { state.RequestMessage.Facility }";
                                integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                            }

                            orgContext.AddObject(appointment);
                            orgContext.SaveChanges();

                            _logger.Info($"Reserveresource Value {appointment.Id} ");
                        }
                    }

                    if (appointment != null && !appointment.Id.Equals(Guid.Empty))
                    {
                        var req = new SetStateRequest
                        {
                            EntityMoniker = new EntityReference("appointment", appointment.Id),
                            State = new OptionSetValue((int)AppointmentState.Scheduled),
                            Status = new OptionSetValue((int)appointment_statuscode.Busy)//osvStatus
                        };
                        service.Execute(req);

                        integrationResult.mcs_appointmentid = new EntityReference(appointment.LogicalName, appointment.Id);
                        integrationResult.mcs_status = new OptionSetValue(803750002);//Completed

                    }

                    service.Attach(integrationResult);
                    service.UpdateObject(integrationResult);
                    service.SaveChanges();

                    if (!appointment.Id.Equals(Guid.Empty)) _logger.Info($"IntegrationResult {IntegrationResultId} updated with ReserveResource {appointment.Id} ");
                }
            }
            catch (Exception e)
            {
                using (var service = new Xrm(state.OrganizationServiceProxy))
                {
                    var integrationResult = new mcs_integrationresult
                    {
                        Id = IntegrationResultId,
                        mcs_error = $"ERROR: Failed to create.- Appointment: { appointment.Subject}, Clinic: { appointment.cvt_ClinicName}, Clinic IEN: { appointment.cvt_ClinicIEN}, ScheduledStart: { appointment.ScheduledStart }.Error Message: {e.Message} ",
                        mcs_status = new OptionSetValue(803750001)//Investigation Required
                    };

                    service.Attach(integrationResult);
                    service.UpdateObject(integrationResult);
                    service.SaveChanges();
                }
                _logger.Error($"ERROR: The following error occurred while attempting to create the Reserve Resource:\n{e}");
            }
        }

        /// <summary>
        /// Create the Integration Result Record
        /// </summary>
        /// <param name="apptId">Appointment Record Id</param>
        /// <param name="state">MakeCancelStateObject</param>
        /// <param name="orgContext">Org Context.</param>
        /// <returns>the Integration Result Object created </returns>
        private Guid CreateIntegrationResult(Guid? apptId, MakeCancelStateObject state, OrganizationServiceContext orgContext)
        {
            var integrationResult = new mcs_integrationresult
            {
                mcs_name = $"HealthShare MakeCancel Appointment - {state.Appointment?.cvt_ConsultName}",
                mcs_integrationrequest = state.SerializedRequestMessage,
                mcs_VimtRequestMessageType = typeof(TmpHealthShareMakeAndCancelAppointmentRequestMessage).FullName,
                mcs_VimtResponseMessageType = typeof(TmpHealthShareMakeAndCancelAppointmentResponseMessage).FullName,
                mcs_VimtMessageRegistryName = "TmpHealthShareMakeAndCancelAppointmentRequestMessage"
            };

            if (state.Appointment.cvt_CancelCode != "NCD" && state.Appointment.cvt_CancelCode != "RCD")
            {
                if (state.Appointment.OptionalAttendees == null || state.Appointment.OptionalAttendees.Count().Equals(0))
                {
                    integrationResult.mcs_error = $"ERROR: Failed to find Patient for ICN {state.Appointment.cvt_PatientICN}";
                    integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                }
                else if (state.Appointment.RequiredAttendees == null)
                {
                    integrationResult.mcs_error = $"ERROR: Failed to find Required Attendees for the Reserve Resource. ClinicIEN: {state.Appointment.cvt_ClinicIEN}. Facility: {state.Appointment.cvt_FacilityText}. Station Number: {state.Appointment.cvt_stationnumber}.";
                    integrationResult.mcs_status = new OptionSetValue(803750001);//Investigation Required
                }
                else
                {
                    integrationResult.mcs_status = new OptionSetValue(803750002);//Completed
                }
            }

            if (apptId != null && apptId != Guid.Empty)
            {
                integrationResult.mcs_appointmentid = new EntityReference(Appointment.EntityLogicalName, apptId.Value);
            }

            orgContext.AddObject(integrationResult);
            orgContext.SaveChanges();

            var vistaIntegrationResult = CreateVistaIntegrationResult(apptId, integrationResult, state);

            orgContext.AddObject(vistaIntegrationResult);
            orgContext.SaveChanges();

            return integrationResult.Id;
        }

        /// <summary>
        /// Create the Integration Result Record
        /// </summary>
        /// <param name="apptId">Appointment Record Id</param>
        /// <param name="state">MakeCancelStateObject</param>
        /// <param name="orgContext">Org Context.</param>
        /// <returns>the Integration Result Object created </returns>
        private Guid CreateIntegrationResult(Entity serviceAppt, MakeCancelStateObject state, OrganizationServiceContext orgContext)
        {
            var integrationResult = new mcs_integrationresult
            {
                mcs_name = $"HealthShare MakeCancel Appointment - {state.Appointment?.cvt_ConsultName}",
                mcs_integrationrequest = state.SerializedRequestMessage,
                mcs_serviceappointmentid = serviceAppt.ToEntityReference(),
                mcs_VimtRequestMessageType = typeof(TmpHealthShareMakeAndCancelAppointmentRequestMessage).FullName,
                mcs_VimtResponseMessageType = typeof(TmpHealthShareMakeAndCancelAppointmentResponseMessage).FullName,
                mcs_VimtMessageRegistryName = "TmpHealthShareMakeAndCancelAppointmentRequestMessage"
            };

            integrationResult.mcs_status = new OptionSetValue(803750002);//Completed

            orgContext.AddObject(integrationResult);
            orgContext.SaveChanges();

            return integrationResult.Id;
        }

        /// <summary>
        /// Create the Vista Integration Result Record
        /// </summary>
        /// <param name="apptId">Appointment Record Id</param>
        /// /// <param name="ir">Associated Integration Record</param>
        /// <param name="state">MakeCancelStateObject</param>
        /// <returns>the Integration Result Object created </returns>
        private cvt_vistaintegrationresult CreateVistaIntegrationResult(Guid? apptId, mcs_integrationresult ir, MakeCancelStateObject state)
        {
            using (var service = new Xrm(state.OrganizationServiceProxy))
            {
                var patient = (service.mcs_personidentifiersSet.FirstOrDefault(i => i.mcs_assigningauthority == "USVHA" && i.mcs_identifier == state.Appointment.cvt_PatientICN));

                var vistaIntegrationResult = new cvt_vistaintegrationresult();
                var appt = state.Appointment;

                vistaIntegrationResult.cvt_ClinicIEN = appt.cvt_ClinicIEN;
                vistaIntegrationResult.cvt_ClinicName = appt.cvt_ClinicName;
                vistaIntegrationResult.cvt_DateTime = string.Format("{0:yyyymmddhh}", DateTime.Now);
                if (appt.cvt_Facility != null) vistaIntegrationResult.cvt_FacilityCode = appt.cvt_Facility.Name;
                vistaIntegrationResult.cvt_FacilityName = appt.cvt_ClinicName;
                vistaIntegrationResult.cvt_PatientName = appt.cvt_BookedPatients;
                vistaIntegrationResult.cvt_PersonId = appt.cvt_PatientICN;
                vistaIntegrationResult.cvt_Reason = appt.cvt_CancelReason;
                vistaIntegrationResult.cvt_VistAStatus = appt.cvt_VisitStatus;
                vistaIntegrationResult.cvt_ParentResult = new EntityReference { Id = ir.Id, LogicalName = mcs_integrationresult.EntityLogicalName };
                vistaIntegrationResult.cvt_name = $"{appt.cvt_ConsultName} - at {appt.cvt_ClinicName} ({appt.cvt_FacilityText})";
                vistaIntegrationResult.cvt_VistaReasonCode = appt.cvt_CancelReason == "UNKNOWN" ? "" : appt.cvt_CancelReason;
                vistaIntegrationResult.cvt_VistaStatusCode = appt.cvt_CancelCode == "UNKNOWN" ? "" : appt.cvt_CancelCode;
                if (patient != null && patient.mcs_patient != null) vistaIntegrationResult.cvt_Veteran = patient.mcs_patient;
                if (apptId != null) vistaIntegrationResult.cvt_Appointment = new EntityReference("appointment", apptId.Value);

                return vistaIntegrationResult;
            }
        }
    }
}
