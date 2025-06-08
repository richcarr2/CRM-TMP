using System;
using System.ServiceModel;
using System.Linq;
using Microsoft.Xrm.Sdk;
using VA.TMP.CRM;
using VA.TMP.Integration.Plugins.Shared;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using Microsoft.Xrm.Sdk.Query;
using VRMRest;
using VA.TMP.Integration.Plugins.Messages;
using System.Collections.Generic;
using MCSShared;
using Microsoft.Crm.Sdk.Messages;

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    class ServiceAppointmentVistaCancelPostStageRunner:PluginRunner
    {

        public ServiceAppointmentVistaCancelPostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Gets the MCS Debug field.
        /// </summary>
        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }

        /// <summary>
        /// Gets the primary entity.
        /// </summary>
        /// <returns>Returns the primary entity.</returns>
        public override Entity GetPrimaryEntity()
        {
            var primaryReference = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];

            return new Entity(primaryReference.LogicalName) { Id = primaryReference.Id };
        }

        /// <summary>
        /// Gets or sets the VIMT URL.
        /// </summary>
        private string VimtUrl { get; set; }

        private int VimtTimeout { get; set; }

        public bool Success { get; set; }
        
        public override void Execute()
        {
            var saRecord = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();
            bool filter = false;
            using (var context = new Xrm(OrganizationService)) {
                filter = VistaPluginHelpers.RunVistaIntegration(saRecord, context, Logger);
            }
            if (saRecord.mcs_groupappointment == null || !saRecord.mcs_groupappointment.Value || saRecord.cvt_Type.Value)
            {
                if (saRecord.StateCode.Value == DataModel.ServiceAppointmentState.Canceled)
                {
                    if (filter)
                    {
                        var orgName = PluginExecutionContext.OrganizationName;
                        var user = OrganizationService.Retrieve(SystemUser.EntityLogicalName, PluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();

                        using (var context = new Xrm(OrganizationService))
                        {
                            Logger.WriteDebugMessage("Starting GetAndSetIntegrationSettings");
                            GetAndSetIntegrationSettings(context);
                            Logger.WriteDebugMessage("Finished GetAndSetIntegrationSettings");

                            var anyFailures = RunCancelVistaBooking(saRecord, orgName, user);

                            if (!anyFailures)
                            {
                                Success = true;
                                return;
                            }
                            else //something went wrong
                            {
                                Success = false;
                                IntegrationPluginHelpers.ChangeEntityStatus(OrganizationService, saRecord, (int)serviceappointment_statuscode.CancelFailure);
                            }
                        }
                    }
                    else // VIA integrations are turned off
                    {
                        Logger.WriteDebugMessage("Vista Integration (through VIA) is turned off system wide, skipping Vista Cancel integration");
                        Success = true;
                    }
                }
                else
                {
                    Logger.WriteDebugMessage("Vista Cancel Integration skipped - Appointment not in 'Canceled' state");
                    Success = true;
                }
            }
            else
            {
                Logger.WriteDebugMessage("Cancel Request invalid for Clinic Based Group Appointments. ");
                Success = true;
            }
        }

        private void GetAndSetIntegrationSettings(Xrm context)
        {
            VimtTimeout = IntegrationPluginHelpers.GetVimtTimeout(context, Logger, this.GetType().Name);

            var vimtUrlSetting = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "VIMT URL");
            if (vimtUrlSetting != null) VimtUrl = vimtUrlSetting.mcs_value;
            else throw new InvalidPluginExecutionException("No VIMT Url listed in Integration Settings.  Please contact the Help Desk to add VIMT URL.  Proxy Add Canceled.");
        }

        private bool RunCancelVistaBooking(DataModel.ServiceAppointment sa, string orgName, SystemUser user)
        {
            var anyfailures = false;
            var veterans = GetPatients(sa);

            foreach (var veteran in veterans)
            {
                if (veteran != Guid.Empty)
                {
                    Logger.WriteDebugMessage("VeteranParty: " + veteran.ToString());
                    string proUserDuz = string.Empty;
                    var request = new CancelAppointmentRequestMessage
                    {
                        CanceledPatients = new List<Guid> { veteran },
                        AppointmentId = PrimaryEntity.Id,
                        LogRequest = true,
                        OrganizationName = orgName,
                        UserId = user.Id,
                        PatUserDuz = VistaPluginHelpers.GetUserDuz(sa, OrganizationService, PluginExecutionContext.UserId, Logger, out proUserDuz),
                        ProUserDuz = proUserDuz,
                        SAMLToken = user.cvt_SAMLToken,
                        WholeAppointmentCanceled = true
                    };

                    var vimtRequest = IntegrationPluginHelpers.SerializeInstance(request);
                    CancelAppointmentResponseMessage response = null;
                    try
                    {
                        Logger.WriteDebugMessage(string.Format("Sending Vista Cancel Appointment Request Message to VIMT: {0}.", vimtRequest));
                        response = Utility.SendReceive<CancelAppointmentResponseMessage>(new Uri(VimtUrl), MessageRegistry.CancelAppointmentRequestMessage, request, null, VimtTimeout, out int lag);
                        response.VimtLagMs = lag;
                        Logger.WriteDebugMessage(string.Format("Finished Sending Vista Cancel Appointment Request Message to VIMT for {0}", veteran));
                        var status = ProcessVistaResponse(response, sa, new EntityReference(Contact.EntityLogicalName, veteran));
                        if (status != sa.StatusCode.Value)
                            anyfailures = true;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                        IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Vista Cancel Appointment:", errorMessage, vimtRequest, typeof(CancelAppointmentRequestMessage).FullName,
                            typeof(CancelAppointmentResponseMessage).FullName, MessageRegistry.CancelAppointmentRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                        Logger.WriteToFile(errorMessage);
                        anyfailures = true;
                    }
                }
            }
            return anyfailures;
        }

        private List<Guid> GetPatients(DataModel.ServiceAppointment sa)
        {
            var patients = new List<Guid>();
            using (var srv = new Xrm(OrganizationService))
            {
                var vistaBookings = srv.cvt_vistaintegrationresultSet.Where(vir => vir.cvt_ServiceActivity.Id == sa.Id && vir.cvt_VistAStatus != VistaStatus.CANCELLED.ToString() && vir.cvt_VistAStatus != VistaStatus.FAILED_TO_RECEIVE.ToString() && vir.cvt_Veteran != null).ToList();
                patients = vistaBookings.Select(vir => vir.cvt_Veteran.Id).Distinct().ToList();
            }
            return patients;
                //return sa.Customers.Select(c => c.PartyId.Id).ToList(); - old way, doesn't catched failed to cancels
        }

        private int ProcessVistaResponse(CancelAppointmentResponseMessage response, DataModel.ServiceAppointment sa, EntityReference patient)
        {
            if (response == null) return (int)serviceappointment_statuscode.CancelFailure;

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            var integrationResultId = IntegrationPluginHelpers.CreateIntegrationResult("Cancel Vista Appointment", response.ExceptionOccured, errorMessage,
                response.VimtRequest, response.SerializedInstance, response.VimtResponse,
                typeof(CancelAppointmentRequestMessage).FullName, typeof(CancelAppointmentResponseMessage).FullName, MessageRegistry.CancelAppointmentRequestMessage,
                PrimaryEntity.Id, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs);

            serviceappointment_statuscode status = (serviceappointment_statuscode)sa.StatusCode.Value;
            if (response.ExceptionOccured)
                status = serviceappointment_statuscode.CancelFailure;
            else
            {
                var vistaAppointments = new List<VistaAppointment>();
                if (response.PatVistaAppointment != null)
                    vistaAppointments.Add(response.PatVistaAppointment);
                if (response.ProVistaAppointment != null)
                    vistaAppointments.Add(response.ProVistaAppointment);
                status = (serviceappointment_statuscode)IntegrationPluginHelpers.WriteVistaResults(vistaAppointments, integrationResultId, "cancel", OrganizationService, Logger, patient, (int)status);
            }
            return (int)status;
        }
    }
}
