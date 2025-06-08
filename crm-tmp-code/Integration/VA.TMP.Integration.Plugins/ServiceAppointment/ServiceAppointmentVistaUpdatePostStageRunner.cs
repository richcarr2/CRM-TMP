using System;
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

namespace VA.TMP.Integration.Plugins.ServiceAppointment
{
    public class ServiceAppointmentVistaUpdatePostStageRunner : PluginRunner
    {
        public ServiceAppointmentVistaUpdatePostStageRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }

        public bool Success { get; set; }

        private string VimtUrl { get; set; }

        private int VimtTimeout { get; set; }

        public List<Guid> VeteranList { get; set; }

        public override void Execute()
        {
            var saRecord = OrganizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, PrimaryEntity.Id, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();

            if (saRecord.mcs_groupappointment == null || !saRecord.mcs_groupappointment.Value || saRecord.cvt_Type.Value)
            {
                using (var context = new Xrm(OrganizationService))
                {
                    var runVista = VistaPluginHelpers.RunVistaIntegration(saRecord, context, Logger);
                    if (runVista)
                    {
                        if (saRecord.StateCode.Value == ServiceAppointmentState.Scheduled || saRecord.StateCode.Value == ServiceAppointmentState.Open)
                        {
                            var orgName = PluginExecutionContext.OrganizationName;
                            var user = OrganizationService.Retrieve(SystemUser.EntityLogicalName, PluginExecutionContext.UserId, new ColumnSet(true)).ToEntity<SystemUser>();

                            Logger.WriteDebugMessage("Starting GetAndSetIntegrationSettings");
                            GetAndSetIntegrationSettings(context);
                            Logger.WriteDebugMessage("Finished GetAndSetIntegrationSettings");

                            var anyFailures = RunVistaBooking(saRecord, orgName, user);

                            if (!anyFailures)
                                TriggerNextIntegration(saRecord);

                        }
                        else //TODO Cancel also sets Success = true here, consider doing that 
                            Logger.WriteDebugMessage("Service Activity is not in a proper status to run Vista Integration");
                    }
                    else
                    {
                        TriggerNextIntegration(saRecord);
                        Logger.WriteDebugMessage("Vista Integration Switched Off, moving on to next integration.");
                    }
                }
            }
            else //TODO Cancel also sets Success = true here, consider doing that 
                Logger.WriteDebugMessage("Clinic Based Groups do not run Vista Integration");
        }

        private void GetAndSetIntegrationSettings(Xrm context)
        {
            VimtTimeout = IntegrationPluginHelpers.GetVimtTimeout(context, Logger, GetType().Name);

            var vimtUrlSetting = context.mcs_integrationsettingSet.FirstOrDefault(x => x.mcs_name == "VIMT URL");
            if (vimtUrlSetting != null) VimtUrl = vimtUrlSetting.mcs_value;
            else throw new InvalidPluginExecutionException("No VIMT Url listed in Integration Settings.  Please contact the Help Desk to add VIMT URL.  Proxy Add Canceled.");
        }

        private bool RunVistaBooking(DataModel.ServiceAppointment sa, string orgName, SystemUser user)
        {
            var anyfailures = false;
            var isBooking = false;
            var veterans = VistaPluginHelpers.GetChangedPatients(sa, OrganizationService, Logger, out isBooking);
            var status = -1;

            //Checks to ensure new patients were added, and only sends those new patients
            if (!isBooking)
            {
                Logger.WriteDebugMessage("No new patients detected, exiting Vista Booking");
                return anyfailures;
            }
            foreach (var veteran in veterans)
            {
                if (veteran != Guid.Empty)
                {
                    string proUserDuz = string.Empty;
                    Logger.WriteDebugMessage("VeteranParty: " + veteran.ToString());
                    var request = new MakeAppointmentRequestMessage
                    {
                        AddedPatients = new List<Guid> { veteran },
                        AppointmentId = PrimaryEntity.Id,
                        LogRequest = true,
                        OrganizationName = orgName,
                        UserId = user.Id,
                        SAMLToken = user.cvt_SAMLToken,
                        PatUserDuz = VistaPluginHelpers.GetUserDuz(sa, OrganizationService, PluginExecutionContext.UserId, Logger, out proUserDuz), 
                        ProUserDuz = proUserDuz
                    };

                    Logger.WriteDebugMessage("Set up MakeAppointmentRequestMessage request object.");

                    MakeAppointmentResponseMessage response = null;
                    var vimtRequest = IntegrationPluginHelpers.SerializeInstance(request);
                    try
                    {
                        Logger.WriteDebugMessage(string.Format("Sending Vista Make Appointment Request Message to VIMT: {0}.", vimtRequest));
                        response = Utility.SendReceive<MakeAppointmentResponseMessage>(new Uri(VimtUrl), MessageRegistry.MakeAppointmentRequestMessage, request, null, VimtTimeout, out int lag);
                        response.VimtLagMs = lag;
                        Logger.WriteDebugMessage(string.Format("Finished Sending Vista Make Appointment Request Message to VIMT for {0}", veteran));
                        var results = ProcessVistaResponse(response, new EntityReference(Contact.EntityLogicalName, veteran));
                        if (results != (int)serviceappointment_statuscode.ReservedScheduled)
                        {
                            anyfailures = true;
                            status = results;
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format(IntegrationPluginHelpers.VimtServerDown, ex);
                        IntegrationPluginHelpers.CreateIntegrationResultOnVimtFailure("Make Vista Appointment", errorMessage, vimtRequest, typeof(ProxyAddRequestMessage).FullName,
                            typeof(MakeAppointmentResponseMessage).FullName, MessageRegistry.ProxyAddRequestMessage, PrimaryEntity.Id, OrganizationService, response?.VimtRequest, response?.VimtResponse, response?.VimtLagMs, response?.EcProcessingMs, response?.VimtProcessingMs);
                        Logger.WriteToFile(errorMessage);
                        anyfailures = true;
                    }
                }
            }
            if (status != -1)
                IntegrationPluginHelpers.UpdateServiceAppointmentStatus(OrganizationService, PrimaryEntity.Id, (serviceappointment_statuscode)status);
            return anyfailures;
        }

        private int ProcessVistaResponse(MakeAppointmentResponseMessage response, EntityReference patient)
        {
            if (response == null) return (int)serviceappointment_statuscode.VistaFailure ;

            var errorMessage = response.ExceptionOccured ? response.ExceptionMessage : string.Empty;
            var integrationResultId = IntegrationPluginHelpers.CreateIntegrationResult("Make Vista Appointment", response.ExceptionOccured, errorMessage,
                response.VimtRequest, response.SerializedInstance, response.VimtResponse,
                typeof(MakeAppointmentRequestMessage).FullName, typeof(MakeAppointmentResponseMessage).FullName, MessageRegistry.MakeAppointmentRequestMessage,
                PrimaryEntity.Id, OrganizationService, response.VimtLagMs, response.EcProcessingMs, response.VimtProcessingMs);


            serviceappointment_statuscode status = serviceappointment_statuscode.ReservedScheduled;
            if (response.ExceptionOccured)
                status = serviceappointment_statuscode.VistaFailure;
            else
            {
                var vistaAppointments = new List<VistaAppointment>();
                if (response.PatVistaAppointment != null)
                    vistaAppointments.Add(response.PatVistaAppointment);
                if (response.ProVistaAppointment != null)
                    vistaAppointments.Add(response.ProVistaAppointment);
                status = (serviceappointment_statuscode)IntegrationPluginHelpers.WriteVistaResults(vistaAppointments, integrationResultId, "book", OrganizationService, Logger, patient);
            }
            return (int)status;
        }

        private void TriggerNextIntegration(DataModel.ServiceAppointment sa)
        {
            Success = true;
            //if (VeteranList == null)
            //{
            //    Logger.WriteDebugMessage("Updating Make Vista Appointment flag");

            //    var updateServiceAppointment = new DataModel.ServiceAppointment
            //    {
            //        Id = sa.Id,
            //        cvt_VistaAddCompleted = true
            //    };
            //    try
            //    {
            //        OrganizationService.Update(updateServiceAppointment);
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidPluginExecutionException(string.Format("Failed to complete Updating Make Vista Appointment flag: {0}", IntegrationPluginHelpers.BuildErrorMessage(ex)), ex);
            //    }
            //}
            //else
            //{
            //    var veteranList = VistaPluginHelpers.GetChangedPatients(sa, OrganizationService, Logger);
            //    var runVvs = new ServiceAppointmentVvsUpdatePostStage();
            //    runVvs.Execute(ServiceProvider, veteranList);
            //}
        }
    }
}
