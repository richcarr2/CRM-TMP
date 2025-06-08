using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using MCS.ApplicationInsights;
using MCSShared;
using MCSUtilities2011;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Plugins.Messages;
using VA.TMP.OptionSets;

namespace VA.TMP.Integration.Plugins.Helpers
{
    /// <summary>
    /// Static class of reusable methods to facilitate integration plugin development.
    /// </summary>
    public static class IntegrationPluginHelpers
    {
        public const string VimtServerDown = "Error - VIMT is likely unavailable. Details of the error are as follows: {0}";

        /// <summary>
        ///  Update Service Appointment's Status.
        /// </summary>
        /// <param name="organizationService">Organization Service.</param>
        /// <param name="serviceAppointmentId">Service Appointment Id.</param>
        /// <param name="statusCode">Service Appointment Status.</param>
        public static void UpdateServiceAppointmentStatus(IOrganizationService organizationService, Guid serviceAppointmentId, serviceappointment_statuscode statusCode)
        {
            var sa = organizationService.Retrieve(DataModel.ServiceAppointment.EntityLogicalName, serviceAppointmentId, new ColumnSet(true)).ToEntity<DataModel.ServiceAppointment>();
            ChangeEntityStatus(organizationService, sa, (int)statusCode, true);
        }

        internal static QueryExpression ConvertFetchExpressionToQueryExpression(FetchExpression query, IOrganizationService organizationService, MCSLogger logger)
        {
            QueryExpression qe = null;
            try
            {

                if (query.GetType() == typeof(FetchExpression))
                {

                    FetchExpression fe = query;

                    // Convert the FetchXML into a query expression.
                    var conversionRequest = new FetchXmlToQueryExpressionRequest
                    {
                        FetchXml = fe.Query
                    };

                    FetchXmlToQueryExpressionResponse fxmltoqe = (FetchXmlToQueryExpressionResponse)organizationService.Execute(conversionRequest);
                    qe = fxmltoqe.Query;
                }
            }
            catch (Exception ex)
            {
                logger.WriteToFile(ex.Message);
            }
            return qe;
        }

        /// <summary>
        ///  Update Service Appointment's Status.
        /// </summary>
        /// <param name="organizationService">Organization Service.</param>
        /// <param name="vodId">Service Appointment Id.</param>
        /// <param name="statusCode">Service Appointment Status.</param>
        public static void UpdateVodStatus(IOrganizationService organizationService, Guid vodId, cvt_vod_statuscode statusCode)
        {
            var vod = organizationService.Retrieve(cvt_vod.EntityLogicalName, vodId, new ColumnSet(true)).ToEntity<cvt_vod>();
            ChangeEntityStatus(organizationService, vod, (int)statusCode, true);
        }

        public static void UpdateAppointment(IOrganizationService organizationService, Guid appointmentId, Appointmentcvt_IntegrationBookingStatus status)
        {
            var appt = organizationService.Retrieve(DataModel.Appointment.EntityLogicalName, appointmentId, new ColumnSet(true)).ToEntity<DataModel.Appointment>();
            if (appt.StatusCode.Value == (int)status)
                return;
            var appointmentToUpdate = new DataModel.Appointment
            {
                Id = appointmentId,
                cvt_IntegrationBookingStatus = new OptionSetValue((int)status)
            };
            organizationService.Update(appointmentToUpdate);
        }

        /// <summary>
        /// Create Integration Result on successful completion of VIMT call.
        /// </summary>
        /// <param name="integrationResultName">>Name of the Integration Result.</param>
        /// <param name="exceptionOccured">Whether an exception occured.</param>
        /// <param name="errorMessage">Error Message.</param>
        /// <param name="vimtRequest">VIMT Request.</param>
        /// <param name="integrationRequest">Integration Request.</param>
        /// <param name="vimtResponse">VIMT Response.</param>
        /// <param name="vimtRequestMessageType">VIMT Message Request Type.</param>
        /// <param name="vimtResponseMessageType">VIMT Message Response Type.</param>
        /// <param name="vimtMessageRegistryName">VIMT Message Registry Name.</param>
        /// <param name="serviceAppointId">Service Appointment Id.</param>
        /// <param name="organizationService">CRM Organization Service.</param>
        public static Guid CreateIntegrationResult(
            string integrationResultName,
            bool exceptionOccured,
            string errorMessage,
            string vimtRequest,
            string integrationRequest,
            string vimtResponse,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            Guid serviceAppointId,
            IOrganizationService organizationService,
            int vimtLagTime,
            int EcProcessingTime,
            int vimtProcessingTime, PatientIntegrationResultInformation patientIntegrationResultInformation = null, bool updateAppointmentStatus = true, string controlId = null)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, EcProcessingTime, vimtProcessingTime, null, patientIntegrationResultInformation, controlId);
            integrationResult.mcs_serviceappointmentid = new EntityReference(DataModel.ServiceAppointment.EntityLogicalName, serviceAppointId);

            var id = organizationService.Create(integrationResult);

            if (exceptionOccured && updateAppointmentStatus)
            {
                UpdateServiceAppointmentStatus(organizationService, serviceAppointId, serviceappointment_statuscode.InterfaceVIMTFailure);
            }

            return id;
        }

        public static Guid CreateCernerIntegrationResult(
             string integrationResultName,
             bool exceptionOccured,
             string errorMessage,
             string vimtRequest,
             string integrationRequest,
             string vimtResponse,
             string vimtRequestMessageType,
             string vimtResponseMessageType,
             string vimtMessageRegistryName,
             Guid serviceAppointId,
             IOrganizationService organizationService,
             int vimtLagTime,
             int EcProcessingTime,
             int vimtProcessingTime, bool updateAppointmentStatus = true, string controlId = null)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, EcProcessingTime, vimtProcessingTime, null, controlId: controlId);
            integrationResult.mcs_serviceappointmentid = new EntityReference(DataModel.ServiceAppointment.EntityLogicalName, serviceAppointId);

            var id = organizationService.Create(integrationResult);

            if (exceptionOccured && updateAppointmentStatus)
            {
                UpdateServiceAppointmentStatus(organizationService, serviceAppointId, serviceappointment_statuscode.InterfaceVIMTFailure);
            }

            return id;
        }

        /// <summary>
        /// Create Integration Result on successful completion of VIMT call.
        /// </summary>
        /// <param name="integrationResultName">>Name of the Integration Result.</param>
        /// <param name="exceptionOccured">Whether an exception occured.</param>
        /// <param name="errorMessage">Error Message.</param>
        /// <param name="vimtRequest">VIMT Request.</param>
        /// <param name="integrationRequest">Integration Request.</param>
        /// <param name="vimtResponse">VIMT Response.</param>
        /// <param name="vimtRequestMessageType">VIMT Message Request Type.</param>
        /// <param name="vimtResponseMessageType">VIMT Message Response Type.</param>
        /// <param name="vimtMessageRegistryName">VIMT Message Registry Name.</param>
        /// <param name="vodId">Service Appointment Id.</param>
        /// <param name="organizationService">CRM Organization Service.</param>
        public static Guid CreateVodIntegrationResult(
            string integrationResultName,
            bool exceptionOccured,
            string errorMessage,
            string vimtRequest,
            string integrationRequest,
            string vimtResponse,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            Guid vodId,
            IOrganizationService organizationService,
            int vimtLagTime,
            int eCProcessingTime,
            int vimtProcessingTime,
            MCSLogger Logger)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime);

            integrationResult.cvt_vod = new EntityReference(cvt_vod.EntityLogicalName, vodId);

            var id = organizationService.Create(integrationResult);

            if (exceptionOccured)
            {
                Logger.WriteToFile($"Exception Occurred, updating VOD Status to failed. Exception : {errorMessage}");
                UpdateVodStatus(organizationService, vodId, cvt_vod_statuscode.Failure);
            }

            return id;
        }


        /// <summary>
        /// Creates Integration Result.
        /// </summary>
        /// <param name="integrationResultName">Integration Result Name.</param>
        /// <param name="exceptionOccured">Whether an exception occurred.</param>
        /// <param name="errorMessage">Error Message.</param>
        /// <param name="vimtRequest">VIMT Request.</param>
        /// <param name="integrationRequest">Integration Request.</param>
        /// <param name="vimtResponse">VIMT Response.</param>
        /// <param name="vimtRequestMessageType">VIMT Request Message Type.</param>
        /// <param name="vimtResponseMessageType">VIMT Response Message Type.</param>
        /// <param name="vimtMessageRegistryName">VIMT Message Registry Name.</param>
        /// <param name="appointmentId">Appointment Id</param>
        /// <param name="organizationService">Organization Service Proxy.</param>
        /// <param name="updateAppointment">If true, updates appointment status (success or failure), if false, only updates status on failure.</param>
        /// <param name="patientIntegrationResultInformation"></param>
        public static Guid CreateAppointmentIntegrationResult(
            string integrationResultName,
            bool exceptionOccured,
            string errorMessage,
            string vimtRequest,
            string integrationRequest,
            string vimtResponse,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            Guid appointmentId,
            IOrganizationService organizationService,
            int vimtLagTime,
            int eCProcessingTime,
            int vimtProcessingTime,
            bool updateAppointment = true,
            PatientIntegrationResultInformation patientIntegrationResultInformation = null)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse,
                vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime, null, patientIntegrationResultInformation);

            integrationResult.mcs_appointmentid = new EntityReference(DataModel.ServiceAppointment.EntityLogicalName, appointmentId);

            var id = organizationService.Create(integrationResult);

            if (updateAppointment)
            {
                UpdateAppointment(organizationService, appointmentId, exceptionOccured
                    ? Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure
                    : Appointmentcvt_IntegrationBookingStatus.ReservedScheduled);
            }
            else
            {
                if (exceptionOccured)
                    UpdateAppointment(organizationService, appointmentId, Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure);
            }
            return id;
        }

        public static Guid CreateCernerAppointmentIntegrationResult(
         string integrationResultName,
         bool exceptionOccured,
         string errorMessage,
         string vimtRequest,
         string integrationRequest,
         string vimtResponse,
         string vimtRequestMessageType,
         string vimtResponseMessageType,
         string vimtMessageRegistryName,
         Guid appointmentId,
         IOrganizationService organizationService,
         int vimtLagTime,
         int eCProcessingTime,
         int vimtProcessingTime,
         bool updateAppointment = true,
         string controlId = null)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse,
                vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime, null, null, controlId: controlId);

            integrationResult.mcs_appointmentid = new EntityReference(DataModel.ServiceAppointment.EntityLogicalName, appointmentId);

            var id = organizationService.Create(integrationResult);

            if (updateAppointment)
            {
                UpdateAppointment(organizationService, appointmentId, exceptionOccured
                    ? Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure
                    : Appointmentcvt_IntegrationBookingStatus.ReservedScheduled);
            }
            else
            {
                if (exceptionOccured)
                    UpdateAppointment(organizationService, appointmentId, Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure);
            }
            return id;
        }

        public static Guid CreateServiceAppointmentIntegrationResult(
            string integrationResultName,
            bool exceptionOccured,
            string errorMessage,
            string vimtRequest,
            string integrationRequest,
            string vimtResponse,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            Guid serviceAppointmentId,
            Guid patientId,
            IOrganizationService organizationService,
            int vimtLagTime,
            int eCProcessingTime,
            int vimtProcessingTime,
            MCSLogger logger, 
            bool updateCustomers,
            PatientIntegrationResultInformation patientIntegrationResultInformation = null)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime, null, patientIntegrationResultInformation);
            integrationResult.mcs_serviceappointmentid = new EntityReference(DataModel.ServiceAppointment.EntityLogicalName, serviceAppointmentId);

            var id = organizationService.Create(integrationResult);

            if (updateCustomers)
            {
                using(var svc = new Xrm(organizationService))
                {
                    var sa = svc.ServiceAppointmentSet.FirstOrDefault(s => s.Id == serviceAppointmentId);
                    if(sa != null)
                    {
                        var customers = sa.Customers.ToList();
                        var updatedCustomers = new List<ActivityParty>();

                        logger.WriteDebugMessage($"Patient Id: {patientId}");
                        customers.ForEach(customer =>
                        {
                            logger.WriteDebugMessage($"Customer Id: {customer.PartyId.Id}");
                            if (!customer.PartyId.Id.Equals(patientId))
                            {
                                updatedCustomers.Add(customer);
                            }
                        });

                        if (updatedCustomers.Count.Equals(customers.Count)) logger.WriteDebugMessage("Failed to remove customer");
                        else
                        {
                            logger.WriteDebugMessage("Customer Removed");
                            sa.Customers = updatedCustomers;
                            svc.UpdateObject(sa);
                            svc.SaveChanges();
                        }

                        //if (customers.Remove(customers.Find(c => c.PartyId.Id == patientId)))
                        //{
                        //    logger.WriteDebugMessage("Customer Removed");
                        //    sa.Customers = customers;
                        //    svc.UpdateObject(sa);
                        //    svc.SaveChanges();
                        //}
                        //else logger.WriteDebugMessage("Failed to remove customer");
                    }
                }
            }
           
            return id;
        }


        /// <summary>
        /// Creates an Error Integration Result when calls to VIMT fail.
        /// </summary>
        /// <param name="integrationResultName">Name of the Integration Result.</param>
        /// <param name="errorMessage">Error Message.</param>
        /// <param name="vimtRequest">VIMT Request.</param>
        /// <param name="vimtRequestMessageType">VIMT Message Request Type.</param>
        /// <param name="vimtResponseMessageType">VIMT Message Response Type.</param>
        /// <param name="vimtMessageRegistryName">VIMT Message Registry Name.</param>
        /// <param name="serviceAppointId">Service Appointment Id.</param>
        /// <param name="organizationService">CRM Organization Service.</param>
        /// <param name="isCancelRequest">Whether call is for a Cancel Request.</param>
        /// <param name="updateAppointmentStatus">Whether the Service Appointment status need to be updated to InterfaceVIMTFailure.</param>
        public static void CreateIntegrationResultOnVimtFailure(
            string integrationResultName,
            string errorMessage,
            string integrationRequest,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            Guid serviceAppointId,
            IOrganizationService organizationService,
            string vimtRequest,
            string vimtResponse,
            int? vimtLagTime,
            int? eCProcessingTime,
            int? vimtProcessingTime,
            bool isCancelRequest = false, bool updateAppointmentStatus = true)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, true, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime);

            integrationResult.mcs_serviceappointmentid = new EntityReference(DataModel.ServiceAppointment.EntityLogicalName, serviceAppointId);
            organizationService.Create(integrationResult);

            if (isCancelRequest) return;

            if (updateAppointmentStatus)
            {
                UpdateServiceAppointmentStatus(organizationService, serviceAppointId, serviceappointment_statuscode.InterfaceVIMTFailure);
            }
        }


        public static void CreateIntegrationResultOnCernerFailure(
             string integrationResultName,
             string errorMessage,
             string integrationRequest,
             string vimtRequestMessageType,
             string vimtResponseMessageType,
             string vimtMessageRegistryName,
             Guid serviceAppointId,
             IOrganizationService organizationService,
             string vimtRequest,
             string vimtResponse,
             int? vimtLagTime,
             int? eCProcessingTime,
             int? vimtProcessingTime,
             bool isCancelRequest = false, bool updateAppointmentStatus = true, string controlId = "")
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, true, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime, controlId: controlId);

            integrationResult.mcs_serviceappointmentid = new EntityReference(DataModel.ServiceAppointment.EntityLogicalName, serviceAppointId);
            organizationService.Create(integrationResult);

            if (isCancelRequest) return;

            if (updateAppointmentStatus)
            {
                UpdateServiceAppointmentStatus(organizationService, serviceAppointId, serviceappointment_statuscode.InterfaceVIMTFailure);
            }
        }

        /// <summary>
        /// Creates an Error Integration Result when calls to VIMT fail.
        /// </summary>
        /// <param name="integrationResultName">Name of the Integration Result.</param>
        /// <param name="errorMessage">Error Message.</param>
        /// <param name="vimtRequest">VIMT Request.</param>
        /// <param name="vimtRequestMessageType">VIMT Message Request Type.</param>
        /// <param name="vimtResponseMessageType">VIMT Message Response Type.</param>
        /// <param name="vimtMessageRegistryName">VIMT Message Registry Name.</param>
        /// <param name="vodId">Service Appointment Id.</param>
        /// <param name="organizationService">CRM Organization Service.</param>
        /// <param name="isCancelRequest">Whether call is for a Cancel Request.</param>
        public static void CreateVodIntegrationResultOnVimtFailure(
            string integrationResultName,
            string errorMessage,
            string integrationRequest,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            Guid vodId,
            IOrganizationService organizationService,
            string vimtRequest,
            string vimtResponse,
            int? vimtLagTime,
            int? eCProcessingTime,
            int? vimtProcessingTime)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, true, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime);
            integrationResult.cvt_vod = new EntityReference(DataModel.cvt_vod.EntityLogicalName, vodId);
            organizationService.Create(integrationResult);

            UpdateVodStatus(organizationService, vodId, cvt_vod_statuscode.Failure);
        }

        public static void CreateIntegrationResultOnPersonSearch(
            string integrationResultName,
            bool exceptionOccured,
            string errorMessage,
            string vimtRequest,
            string integrationRequest,
            string vimtResponse,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            IOrganizationService organizationService,
            int vimtLagTime,
            int eCProcessingTime,
            int vimtProcessingTime, MCSLogger logger)
        {
            logger.WriteDebugMessage($"PrepareGenericIntegrationResultRecord Initiated\nParameter values: \nintegrationResultName - {integrationResultName}, exceptionOccured - {exceptionOccured}, errorMessage - {errorMessage}, vimtRequest - {vimtRequest}, integrationRequest - {integrationRequest}, vimtResponse - {vimtResponse}, vimtRequestMessageType - {vimtRequestMessageType}, vimtResponseMessageType - {vimtResponseMessageType},vimtMessageRegistryName - {vimtMessageRegistryName},organizationService - {organizationService}, vimtLagTime - {vimtLagTime},eCProcessingTime - { eCProcessingTime},vimtProcessingTime - { vimtProcessingTime}");

            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime);
            logger.WriteDebugMessage("PrepareGenericIntegrationResultRecord Call Complete");

            try
            {
                organizationService.Create(integrationResult);
            }
            catch (Exception ex)
            {
                logger.WriteToFile($"CreateIntegrationResultOnPersonSearch {ex.Message}, integrationResultName - {integrationResultName}, exceptionOccured - {exceptionOccured}, errorMessage - {errorMessage}, vimtRequest - {vimtRequest}, integrationRequest - {integrationRequest}, vimtResponse - {vimtResponse}, vimtRequestMessageType - {vimtRequestMessageType}, vimtResponseMessageType - {vimtResponseMessageType},vimtMessageRegistryName - {vimtMessageRegistryName},organizationService - {organizationService}, vimtLagTime - {vimtLagTime},eCProcessingTime - { eCProcessingTime},vimtProcessingTime - { vimtProcessingTime}");
                throw;
            }
        }
        public static void CreateIntegrationResultOnGetPersonIdentifiers(
            string integrationResultName,
            bool exceptionOccured,
            string errorMessage,
            string vimtRequest,
            string integrationRequest,
            string vimtResponse,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            IOrganizationService organizationService,
            int vimtLagTime,
            int eCProcessingTime,
            int vimtProcessingTime, MCSLogger logger)
        {
            logger.WriteDebugMessage($"PrepareGenericIntegrationResultRecord Initiated\nParameter values: \nintegrationResultName - {integrationResultName}, exceptionOccured - {exceptionOccured}, errorMessage - {errorMessage}, vimtRequest - {vimtRequest}, integrationRequest - {integrationRequest}, vimtResponse - {vimtResponse}, vimtRequestMessageType - {vimtRequestMessageType}, vimtResponseMessageType - {vimtResponseMessageType},vimtMessageRegistryName - {vimtMessageRegistryName},organizationService - {organizationService}, vimtLagTime - {vimtLagTime},eCProcessingTime - { eCProcessingTime},vimtProcessingTime - { vimtProcessingTime}");

            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, exceptionOccured, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime);
            logger.WriteDebugMessage("PrepareGenericIntegrationResultRecord Call Complete");

            try
            {
                organizationService.Create(integrationResult);
            }
            catch (Exception ex)
            {
                logger.WriteToFile($"CreateIntegrationResultOnGetPersonIdentifiers {ex.Message}, integrationResultName - {integrationResultName}, exceptionOccured - {exceptionOccured}, errorMessage - {errorMessage}, vimtRequest - {vimtRequest}, integrationRequest - {integrationRequest}, vimtResponse - {vimtResponse}, vimtRequestMessageType - {vimtRequestMessageType}, vimtResponseMessageType - {vimtResponseMessageType},vimtMessageRegistryName - {vimtMessageRegistryName},organizationService - {organizationService}, vimtLagTime - {vimtLagTime},eCProcessingTime - { eCProcessingTime},vimtProcessingTime - { vimtProcessingTime}");
                throw;
            }
        }

        /// <summary>
        /// Creates an Error Integration Result when calls to VIMT fail.
        /// </summary>
        /// <param name="integrationResultName">Integration Result Name.</param>
        /// <param name="errorMessage">Error Message.</param>
        /// <param name="vimtRequest">VIMT Request.</param>
        /// <param name="vimtRequestMessageType">VIMT Request Message Type.</param>
        /// <param name="vimtResponseMessageType">VIMT Response Message Type.</param>
        /// <param name="vimtMessageRegistryName">VIMT Message Registry Name.</param>
        /// <param name="appointmentId">Appointment Id.</param>
        /// <param name="organizationService">Organization Service Proxy.</param>
        public static void CreateAppointmentIntegrationResultOnVimtFailure(
            string integrationResultName,
            string errorMessage,
            string integrationRequest,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            Guid appointmentId,
            IOrganizationService organizationService,
            string vimtRequest,
            string vimtResponse,
            int? vimtLagTime,
            int? eCProcessingTime,
            int? vimtProcessingTime)
        {
            var integrationResult = PrepareGenericIntegrationResultRecord(integrationResultName, errorMessage, true, vimtRequest, integrationRequest, vimtResponse, vimtRequestMessageType, vimtResponseMessageType, vimtMessageRegistryName, organizationService, vimtLagTime, eCProcessingTime, vimtProcessingTime);
            integrationResult.mcs_appointmentid = new EntityReference(DataModel.Appointment.EntityLogicalName, appointmentId);
            organizationService.Create(integrationResult);

            UpdateAppointment(organizationService, appointmentId, Appointmentcvt_IntegrationBookingStatus.InterfaceVIMTFailure);
        }

        private static mcs_integrationresult PrepareGenericIntegrationResultRecord(string integrationResultName,
            string errorMessage,
            bool exceptionOccured,
            string vimtRequest,
            string integrationRequest,
            string vimtResponse,
            string vimtRequestMessageType,
            string vimtResponseMessageType,
            string vimtMessageRegistryName,
            IOrganizationService organizationService,
            int? vimtLagTime,
            int? eCProcessingTime,
            int? vimtProcessingTime,
            MCSLogger logger = null, PatientIntegrationResultInformation patientIntegrationResultInformation = null, string controlId = null)
        {
            var lag = vimtLagTime == null ? decimal.Zero : ConvertSecondsToMilliseconds(vimtLagTime.Value);
            var vimtProcessing = vimtProcessingTime == null ? decimal.Zero : ConvertSecondsToMilliseconds(vimtProcessingTime.Value);
            var ecProcessing = eCProcessingTime == null ? decimal.Zero : ConvertSecondsToMilliseconds(eCProcessingTime.Value);
            logger?.WriteDebugMessage("Timers calculated. Creating the result object");

            //Added ternaries in case the timers' precision on the 2 different machines aren't exact and it leads to a negative result.  Set to 0 in this case, otherwise use true value
            var integrationResult = new mcs_integrationresult
            {
                mcs_name = integrationResultName,
                mcs_error = BoundString(errorMessage, 25000),
                mcs_vimtrequest = BoundString(vimtRequest, 5000),
                mcs_integrationrequest = BoundString(integrationRequest, 5000),
                mcs_vimtresponse = BoundString(vimtResponse, 50000),
                mcs_VimtRequestMessageType = vimtRequestMessageType,
                mcs_VimtResponseMessageType = vimtResponseMessageType,
                mcs_VimtMessageRegistryName = vimtMessageRegistryName,
                cvt_ECProcessing = ecProcessing,
                cvt_VIMTProcessing = ecProcessing > vimtProcessing ? 0 : vimtProcessing - ecProcessing,
                cvt_VIMTLag = vimtProcessing > lag ? 0 : lag - vimtProcessing,
                cvt_controlid = (patientIntegrationResultInformation?.ControlId ?? controlId)
            };

            if (exceptionOccured)
                integrationResult.mcs_status = new OptionSetValue((int)mcs_integrationresultmcs_status.Error);
            else
                integrationResult.mcs_status = (patientIntegrationResultInformation == null) ? new OptionSetValue((int)mcs_integrationresultmcs_status.Complete) : new OptionSetValue((int)mcs_integrationresultmcs_status.WaitingforResponse);

            return integrationResult;
        }

        internal static Guid GetPatIdFromIcn(string icn, IOrganizationService organizationService)
        {
            Guid id;
            using (var srv = new Xrm(organizationService))
            {
                var personIdentifier = srv.mcs_personidentifiersSet.FirstOrDefault(i => i.mcs_identifier == icn);
                if (personIdentifier != null && personIdentifier.mcs_patient != null)
                {
                    var pat = srv.ContactSet.FirstOrDefault(c => c.Id == personIdentifier.mcs_patient.Id);

                    if (pat == null)
                        throw new InvalidPluginExecutionException(string.Format("Person Identifier {0} is not linked to a patient.  ", icn));
                    id = pat.Id;
                }
                else
                {
                    throw new InvalidPluginExecutionException("No patient could be found with ICN = " + icn);
                }
            }
            return id;
        }

        public static List<Guid> GetPatientsFromActivityPartyList(List<ActivityParty> currentPatientsApList)
        {
            var patients = new List<Guid>();
            foreach (var ap in currentPatientsApList)
            {
                patients.Add(ap.PartyId.Id);
            }
            return patients;
        }

        /// <summary>
        /// Builds an error message from an exception recursively.
        /// </summary>
        /// <param name="ex">Exception.</param>
        /// <returns>Exception message.</returns>
        public static string BuildErrorMessage(Exception ex)
        {
            var errorMessage = ex.Message;

            if (ex.InnerException == null) return errorMessage;

            //errorMessage += string.Format("\n\n{0}\n", ex.InnerException.Message);
            errorMessage += "\n" + BuildErrorMessage(ex.InnerException);

            return errorMessage;
        }

        public static ApiIntegrationSettings GetApiSettings(Xrm srv, string uri)
        {
            var settings = srv.mcs_integrationsettingSet.Select(x => new ApiIntegrationSettingsNameValuePair { Name = x.mcs_name, Value = x.mcs_value }).ToList();

            var integrationSettings = new ApiIntegrationSettings
            {
                Resource = settings.FirstOrDefault(x => x.Name == "Resource").Value,
                AppId = settings.FirstOrDefault(x => x.Name == "AppId").Value,
                Secret = settings.FirstOrDefault(x => x.Name == "Secret").Value,
                Authority = settings.FirstOrDefault(x => x.Name == "Authority").Value,
                TenantId = settings.FirstOrDefault(x => x.Name == "TenantId").Value,
                SubscriptionId = settings.FirstOrDefault(x => x.Name == "SubscriptionId").Value,
                BaseUrl = settings.FirstOrDefault(x => x.Name == "BaseUrl").Value,
                Uri = settings.FirstOrDefault(x => x.Name == uri).Value,
                IsProdApi = Convert.ToBoolean(settings.FirstOrDefault(x => x.Name == "IsProdApi").Value),
                SubscriptionIdEast = settings.FirstOrDefault(x => x.Name == "SubscriptionIdEast").Value,
                SubscriptionIdSouth = settings.FirstOrDefault(x => x.Name == "SubscriptionIdSouth").Value
            };

            return integrationSettings;
        }

        public static void ChangeEntityStatus(IOrganizationService service, Entity entity, int statusValue, bool useUpdateNotSsr = false)
        {
            var attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entity.LogicalName,
                LogicalName = "statuscode",
                RetrieveAsIfPublished = true
            };
            var attributeResponse = (RetrieveAttributeResponse)service.Execute(attributeRequest);
            var statusMetadata = (StatusAttributeMetadata)attributeResponse.AttributeMetadata;
            var status = (StatusOptionMetadata)statusMetadata.OptionSet.Options.FirstOrDefault(option => option.Value == statusValue);
            if (status == null)
                throw new InvalidPluginExecutionException(string.Format("{0} is an invalid status for Changing Entity State of {1} with id {2}", statusValue, entity.LogicalName, entity.Id));
            if (status.State == ((OptionSetValue)(entity.Attributes["statecode"])).Value && useUpdateNotSsr)
            {
                var updateEntity = new Entity { Id = entity.Id, LogicalName = entity.LogicalName };
                if (status.Value == null)
                    throw new InvalidPluginExecutionException(string.Format("Invalid Status Reason List: unable to Change Status for record: {0}; id={1}", entity.LogicalName, entity.Id));
                updateEntity.Attributes.Add(new KeyValuePair<string, object>("statuscode", new OptionSetValue(status.Value.Value)));
                try
                {
                    service.Update(updateEntity);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("Failed to Update status of {0} with id {1}.  Error: {2}", entity.LogicalName, entity.Id, ex.Message));
                }
            }
            else
            {
                var stateRequest = new SetStateRequest
                {
                    State = new OptionSetValue((int)status.State),
                    Status = new OptionSetValue((int)status.Value),
                    EntityMoniker = new EntityReference
                    {
                        LogicalName = entity.LogicalName,
                        Id = entity.Id
                    }
                };
                try
                {
                    service.Execute(stateRequest);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(string.Format("Set State Request failed for {0} with id {1}.  Error: {2}", entity.LogicalName, entity.Id, ex.Message));
                }
            }
        }

        internal static string BoundString(string input, int length)
        {
            return string.IsNullOrEmpty(input) ? "" : input.Length > length ? input.Substring(0, length) : input;
        }

        /// <summary>
        /// Gets the Veteran Id for all new or removed veterans
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<Guid, bool> GetListOfNewPatients(IOrganizationService OrganizationService, Entity PrimaryEntity, MCSLogger Logger, IPluginExecutionContext PluginExecutionContext)
        {
            List<Guid> currentCustomers;
            string key = string.Empty;
            if (PrimaryEntity.LogicalName == DataModel.ServiceAppointment.EntityLogicalName)
            {
                currentCustomers = PrimaryEntity.ToEntity<DataModel.ServiceAppointment>().Customers?.Select(ap => ap.PartyId.Id).ToList() ?? new List<Guid>();
                key = "customers";
                Logger.WriteToFile("Customer Count :" + currentCustomers.Count);
            }
            else if (PrimaryEntity.LogicalName == DataModel.Appointment.EntityLogicalName)
            {
                currentCustomers = PrimaryEntity.ToEntity<DataModel.Appointment>().OptionalAttendees?.Select(ap => ap.PartyId.Id).ToList() ?? new List<Guid>();
                key = "optionalattendees";
                Logger.WriteToFile("Customer Count :" + currentCustomers.Count);
            }
            else
                throw new InvalidPluginExecutionException("Failed to Retrieve current List of Veterans for Proxy Add");

            var previousCustomers = new List<Guid>();

            if (PluginExecutionContext.MessageName != "Create")
            {
                ////previously using images to determine who was added.  Doesn't work for retry, Int Results handles retry or direct additions of new patients
                //var pre = PluginExecutionContext.PreEntityImages["pre"];
                //var patientsAtt = pre.Attributes.FirstOrDefault(k => k.Key == key);
                //if (patientsAtt.Value != null)
                //{
                //    EntityCollection ec = (EntityCollection)patientsAtt.Value;
                //    foreach (var entity in ec.Entities)
                //    {
                //        previousCustomers.Add(entity.ToEntity<ActivityParty>().PartyId.Id);
                //    }
                //}
                using (var srv = new Xrm(OrganizationService))
                {
                    //Name of the XML Node where the veteran ID is stored in VIMT Request, which is ALWAYS tracked in any integration result
                    var vetIdString = "//VeteranPartyId";
                    var pastProxyAdds = srv.mcs_integrationresultSet.Where(ir => ir.mcs_VimtMessageRegistryName == MessageRegistry.ProxyAddRequestMessage && (ir.mcs_serviceappointmentid.Id == PrimaryEntity.Id || ir.mcs_appointmentid.Id == PrimaryEntity.Id) && ir.mcs_status.Value != (int)mcs_integrationresultmcs_status.Error)?.ToList() ?? new List<mcs_integrationresult>();
                    foreach (var ir in pastProxyAdds)
                    {
                        //Get the VeteranId
                        try
                        {
                            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(ir.mcs_vimtrequest), new System.Xml.XmlDictionaryReaderQuotas());
                            var root = XElement.Load(jsonReader);

                            var vetId = root.XPathSelectElement(vetIdString).Value;
                            if (!string.IsNullOrEmpty(vetId))
                                previousCustomers.Add(new Guid(vetId));
                            else
                                throw new Exception("No Veteran Id Found");
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToFile(string.Format("Unable to Find Veteran Id in integration result: {0}.  Error: {1}", ir.Id, CvtHelper.BuildExceptionMessage(ex)));
                        }
                    }
                }
            }
            var veteransAdded = new List<Guid>();
            var veteransRemoved = new List<Guid>();

            if (previousCustomers.Count != 0 && currentCustomers.Count != 0)
            {
                veteransAdded = currentCustomers.Except(previousCustomers).Distinct().ToList();
                veteransRemoved = previousCustomers.Except(currentCustomers).Distinct().ToList();
            }
            else
                veteransAdded = currentCustomers.Distinct().ToList();

            var veterans = new Dictionary<Guid, bool>();
            veteransAdded.ForEach(a => veterans.Add(a, true));
            Logger.WriteToFile("veteransAdded Count :" + veteransAdded.Count);
            veteransRemoved.ForEach(r => veterans.Add(r, false));
            Logger.WriteToFile("veteransRemoved Count :" + veteransRemoved.Count);
            return veterans;
        }

        /// <summary>
        /// Gets the Veteran Id for all new or removed veterans
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<Guid, bool> GetListOfNewPatients(IOrganizationService OrganizationService, Entity PrimaryEntity, PluginLogger Logger, IPluginExecutionContext PluginExecutionContext)
        {
            List<Guid> currentCustomers;
            string key = string.Empty;
            if (PrimaryEntity.LogicalName == DataModel.ServiceAppointment.EntityLogicalName)
            {
                currentCustomers = PrimaryEntity.ToEntity<DataModel.ServiceAppointment>().Customers?.Select(ap => ap.PartyId.Id).ToList() ?? new List<Guid>();
                key = "customers";
                Logger.Trace("Customer Count :" + currentCustomers.Count);
            }
            else if (PrimaryEntity.LogicalName == DataModel.Appointment.EntityLogicalName)
            {
                currentCustomers = PrimaryEntity.ToEntity<DataModel.Appointment>().OptionalAttendees?.Select(ap => ap.PartyId.Id).ToList() ?? new List<Guid>();
                key = "optionalattendees";
                Logger.Trace("Customer Count :" + currentCustomers.Count);
            }
            else
                throw new InvalidPluginExecutionException("Failed to Retrieve current List of Veterans for Proxy Add");

            var previousCustomers = new List<Guid>();

            if (PluginExecutionContext.MessageName != "Create")
            {
                ////previously using images to determine who was added.  Doesn't work for retry, Int Results handles retry or direct additions of new patients
                //var pre = PluginExecutionContext.PreEntityImages["pre"];
                //var patientsAtt = pre.Attributes.FirstOrDefault(k => k.Key == key);
                //if (patientsAtt.Value != null)
                //{
                //    EntityCollection ec = (EntityCollection)patientsAtt.Value;
                //    foreach (var entity in ec.Entities)
                //    {
                //        previousCustomers.Add(entity.ToEntity<ActivityParty>().PartyId.Id);
                //    }
                //}
                using (var srv = new Xrm(OrganizationService))
                {
                    //Name of the XML Node where the veteran ID is stored in VIMT Request, which is ALWAYS tracked in any integration result
                    var vetIdString = "//VeteranPartyId";
                    var pastProxyAdds = srv.mcs_integrationresultSet.Where(ir => ir.mcs_VimtMessageRegistryName == MessageRegistry.ProxyAddRequestMessage && (ir.mcs_serviceappointmentid.Id == PrimaryEntity.Id || ir.mcs_appointmentid.Id == PrimaryEntity.Id) && ir.mcs_status.Value != (int)mcs_integrationresultmcs_status.Error)?.ToList() ?? new List<mcs_integrationresult>();
                    foreach (var ir in pastProxyAdds)
                    {
                        //Get the VeteranId
                        try
                        {
                            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(ir.mcs_vimtrequest), new System.Xml.XmlDictionaryReaderQuotas());
                            var root = XElement.Load(jsonReader);

                            var vetId = root.XPathSelectElement(vetIdString).Value;
                            if (!string.IsNullOrEmpty(vetId))
                                previousCustomers.Add(new Guid(vetId));
                            else
                                throw new Exception("No Veteran Id Found");
                        }
                        catch (Exception ex)
                        {
                            Logger.Trace(string.Format("Unable to Find Veteran Id in integration result: {0}.  Error: {1}", ir.Id, CvtHelper.BuildExceptionMessage(ex)));
                        }
                    }
                }
            }
            var veteransAdded = new List<Guid>();
            var veteransRemoved = new List<Guid>();

            if (previousCustomers.Count != 0 && currentCustomers.Count != 0)
            {
                veteransAdded = currentCustomers.Except(previousCustomers).Distinct().ToList();
                veteransRemoved = previousCustomers.Except(currentCustomers).Distinct().ToList();
            }
            else
                veteransAdded = currentCustomers.Distinct().ToList();

            var veterans = new Dictionary<Guid, bool>();
            veteransAdded.ForEach(a => veterans.Add(a, true));
            Logger.Trace("veteransAdded Count :" + veteransAdded.Count);
            veteransRemoved.ForEach(r => veterans.Add(r, false));
            Logger.Trace("veteransRemoved Count :" + veteransRemoved.Count);
            return veterans;
        }

        private static decimal ConvertSecondsToMilliseconds(int time)
        {
            return (decimal)time / 1000;
        }

        /// <summary>
        /// This method ranks the list of integration results based on their sequence in the process and returns the 1st one in the sequence
        /// </summary>
        /// <param name="irs"></param>
        /// <returns></returns>
        /// <remarks>arbitrarily picks if there is both cancel and book for same integration endpoint (aka vista book and cancel)</remarks>
        internal static mcs_integrationresult FindEarliestFailure(List<mcs_integrationresult> irs, MCSLogger logger)
        {
            var dictionary = new Dictionary<mcs_integrationresult, int>();
            foreach (var failure in irs)
            {
                switch (failure.mcs_VimtMessageRegistryName)
                {
                    case MessageRegistry.ProxyAddRequestMessage:
                        dictionary.Add(failure, 1);
                        break;
                    case MessageRegistry.HealthShareMakeCancelOutboundRequestMessage:
                        dictionary.Add(failure, 3);
                        break;
                    case MessageRegistry.VirtualMeetingRoomCreateRequestMessage:
                        dictionary.Add(failure, 2);
                        break;
                    case MessageRegistry.VideoVisitCreateRequestMessage:
                        dictionary.Add(failure, 4);
                        break;
                    case MessageRegistry.VirtualMeetingRoomDeleteRequestMessage:
                        dictionary.Add(failure, 2);
                        break;
                    case MessageRegistry.VideoVisitDeleteRequestMessage:
                        dictionary.Add(failure, 4);
                        break;
                    default:
                        logger.WriteDebugMessage($"Unknown Message Type: {failure.mcs_VimtMessageRegistryName}, skipping retry");
                        break;
                }
            }

            // Order the dictionary by the "values" (aka sort by the rankings 1-3 listed above), then get the IR (key) from the first item in the resulting dictionary
            var orderedList = dictionary.OrderBy(x => x.Value);

            // From entry in dictionary order by entry.Value ascending select entry;
            return orderedList.FirstOrDefault().Key;
        }

        /// <summary>
        /// This method ranks the list of integration results based on their sequence in the process and returns the 1st one in the sequence
        /// </summary>
        /// <param name="irs"></param>
        /// <returns></returns>
        /// <remarks>arbitrarily picks if there is both cancel and book for same integration endpoint (aka vista book and cancel)</remarks>
        internal static mcs_integrationresult FindEarliestFailure(List<mcs_integrationresult> irs, PluginLogger logger)
        {
            var dictionary = new Dictionary<mcs_integrationresult, int>();
            foreach (var failure in irs)
            {
                switch (failure.mcs_VimtMessageRegistryName)
                {
                    case MessageRegistry.ProxyAddRequestMessage:
                        dictionary.Add(failure, 1);
                        break;
                    case MessageRegistry.HealthShareMakeCancelOutboundRequestMessage:
                        dictionary.Add(failure, 3);
                        break;
                    case MessageRegistry.VirtualMeetingRoomCreateRequestMessage:
                        dictionary.Add(failure, 2);
                        break;
                    case MessageRegistry.VideoVisitCreateRequestMessage:
                        dictionary.Add(failure, 4);
                        break;
                    case MessageRegistry.VirtualMeetingRoomDeleteRequestMessage:
                        dictionary.Add(failure, 2);
                        break;
                    case MessageRegistry.VideoVisitDeleteRequestMessage:
                        dictionary.Add(failure, 4);
                        break;
                    default:
                        logger.Trace($"Unknown Message Type: {failure.mcs_VimtMessageRegistryName}, skipping retry");
                        break;
                }
            }

            // Order the dictionary by the "values" (aka sort by the rankings 1-3 listed above), then get the IR (key) from the first item in the resulting dictionary
            var orderedList = dictionary.OrderBy(x => x.Value);

            // From entry in dictionary order by entry.Value ascending select entry;
            return orderedList.FirstOrDefault().Key;
        }

        internal static List<cvt_vistaintegrationresult> GetVistaIntegrationResultsForSA(IOrganizationService organizationService, Guid serviceApptId, MCSLogger logger)
        {
            using (var svc = new Xrm(organizationService))
            {
                var virs = svc.cvt_vistaintegrationresultSet.Where(v => v.cvt_ServiceActivity.Id == serviceApptId).ToList();
                logger.WriteDebugMessage($"Vista Integration Results found: {virs?.Count}");

                return virs;
            }
        }
    }
}
