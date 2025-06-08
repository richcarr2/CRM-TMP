using MCSShared.Fakes;
using MCSUtilities2011;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.QualityTools.Testing.Fakes.Shims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.CRM.Fakes;
using VA.TMP.DataModel.Fakes;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Messages.Mvi.Fakes;
using VA.TMP.Integration.Messages.VirtualMeetingRoom.Fakes;
using VA.TMP.Integration.Plugins.Helpers.Fakes;
using VA.TMP.Integration.Plugins.ServiceAppointment;
using VA.TMP.Integration.Plugins.ServiceAppointment.Fakes;
using VA.TMP.OptionSets;
using VA.TMP.Shared.UnitTests;
using DM = VA.TMP.DataModel;

namespace VA.TMP.Integration.Plugins.UnitTests
{
    [TestClass]
    public class ServiceAppointmentUnitTests : BaseUnitTest
    {
        private ShimServiceAppointmentCreatePostStageRunner _mockServiceAppointmentCreatePostStageRunner;
        private ShimServiceAppointmentIntegrationOrchestratorPostStageRunner _mockServiceAppointmentIntegrationOrchestratorPostStageRunner;
        private ShimServiceAppointmentVistaHealthShareUpdatePostStageRunner _mockServiceAppointmentVistaHealthShareUpdatePostStageRunner;
        private ShimServiceAppointmentVmrUpdatePostStageRunner _mockServiceAppointmentVmrUpdatePostStageRunner;
        private ShimXrm _mockXrm;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void HappyPath()
        {
            var customerId = Guid.Empty;
            var expectedCustomerId = Guid.NewGuid();
            var oldCustomerId = Guid.Empty;
            var expectedOldCustomerId = Guid.NewGuid();
            var saId = Guid.NewGuid();

            //Arrange
            var actualServiceAppt = new DM.ServiceAppointment();
            var expectedServiceAppt = new DM.ServiceAppointment()
            {
                ActivityId = saId,
                Id = saId,
                Customers = new List<DM.ActivityParty>() {
                    new DM.ActivityParty {
                        Id = Guid.NewGuid(), PartyId = new EntityReference(DM.Contact.EntityLogicalName, expectedOldCustomerId),
                    }
                },
                Resources = new List<DM.ActivityParty>() { new DM.ActivityParty { PartyId = new EntityReference(DM.SystemUser.EntityLogicalName) } },
                ScheduledStart = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} 10:15:00AM"),
                StatusCode = new OptionSetValue((int)serviceappointment_statuscode.Pending),
                StateCode = DM.ServiceAppointmentState.Scheduled,
                cvt_relatedschedulingpackage = new EntityReference(DM.cvt_resourcepackage.EntityLogicalName, Guid.NewGuid()),
                cvt_Type = true
            };

            var integrationSettings_mvi = new DM.mcs_integrationsetting
            {
                mcs_name = "MVI ProxyAdd Fake Response Type",
                mcs_value = "Test"
            };
            var integrationSettings_pc = new DM.mcs_integrationsetting
            {
                mcs_name = "ProcessingCode",
                mcs_value = "1"
            };
            var integrationSettings_rm = new DM.mcs_integrationsetting
            {
                mcs_name = "ReturnMviMessagesInResponse",
                mcs_value = "false"
            };
            var integrationSettings_pv = new DM.mcs_integrationsetting
            {
                mcs_name = "PatientVeteran",
                mcs_value = "false"
            };
            var integrationSettings_ps = new DM.mcs_integrationsetting
            {
                mcs_name = "PatientServiceConnected",
                mcs_value = "false"
            };
            var integrationSettings_pt = new DM.mcs_integrationsetting
            {
                mcs_name = "PatientType",
                mcs_value = "1"
            };

            var kvp = new KeyValuePair<string, Entity>("pre", expectedServiceAppt);
            _mockPluginExecutionContext.PreEntityImagesGet = () => { return new EntityImageCollection() { kvp }; };
            _mockPluginExecutionContext.MessageNameGet = () => { return "Create"; };

            using (ShimsContext.Create())
            {
                ShimPluginRunner.AllInstances.LoggerGet = (instance) => _mockLogger;
                ShimPluginRunner.AllInstances.McsHelperGet = (instance) => _mockHelper;
                ShimPluginRunner.AllInstances.McsSettingsGet = (instance) => _mockSettings;
                ShimPluginRunner.AllInstances.OrganizationServiceGet = (instance) => _mockOrgService;
                ShimPluginRunner.AllInstances.OrganizationServiceFactoryGet = (instance) => _mockServiceFactory;
                ShimPluginRunner.AllInstances.PrimaryEntityGet = (e) => { return expectedServiceAppt; };
                ShimPluginRunner.AllInstances.PluginExecutionContextGet = (instance) => _mockPluginExecutionContext;
                ShimPluginRunner.AllInstances.TracingServiceGet = (instance) => _mockTimestampedTracingService;

                ShimOrganizationServiceContext.AllInstances.LoadPropertyEntityRelationship = (context, entity, r) =>
                {
                    entity = expectedServiceAppt;
                };

                ShimCvtHelper.ShouldGenerateVeteranEmailGuidIOrganizationServiceMCSLogger = (g, os, l) =>
                {
                    return false;
                };

                ShimCernerHelper.CheckIfRelatedCernerFacilityServiceAppointmentXrmMCSLoggerAppointment = (svcAppt, ctx, logger, appt) => { return true; };

                _mockXrm = new ShimXrm(new StubXrm(_mockOrgService));

                ShimXrm.AllInstances.ActivityPartySetGet = (query) =>
                {
                    return new List<DM.ActivityParty>() { new DM.ActivityParty() }.AsQueryable();
                };

                ShimXrm.AllInstances.ServiceAppointmentSetGet = (query) =>
                {
                    return new List<DM.ServiceAppointment>() { expectedServiceAppt }.AsQueryable();
                };

                ShimXrm.AllInstances.mcs_integrationsettingSetGet = (ctx) =>
                {
                    var integrationSettings = new List<DM.mcs_integrationsetting>
                    {
                        integrationSettings_pt,
                        integrationSettings_ps,
                        integrationSettings_pv,
                        integrationSettings_rm,
                        integrationSettings_pc,
                        integrationSettings_mvi
                    };
                    return integrationSettings.AsQueryable();
                };

                ShimXrm.AllInstances.mcs_settingSetGet = (ctx) =>
                {
                    return new List<DM.mcs_setting>
                    {
                        new DM.mcs_setting
                        {
                            cvt_accenturevyopta = true,
                            mcs_name = "Active Settings"
                        }
                    }.AsQueryable();
                };

                _mockPluginExecutionContext.InputParametersGet = () =>
                {
                    return new ParameterCollection
                    {
                        new KeyValuePair<string, object>("Target", expectedServiceAppt)
                    };
                };

                _mockOrgService.RetrieveStringGuidColumnSet = (s, g, cs) =>
                {
                    if (s == DM.SystemUser.EntityLogicalName) return new Entity(DM.SystemUser.EntityLogicalName, Guid.NewGuid());
                    if (s == DM.Contact.EntityLogicalName) return new DM.Contact
                    {
                        cvt_TabletType = new OptionSetValue((int)Contactcvt_TabletType.VAIssuediOSDevice),
                        cvt_staticvmrlink = "http://test",
                        DoNotEMail = true
                    };
                    actualServiceAppt = expectedServiceAppt;
                    return actualServiceAppt;
                };

                var pluginRunner = new ServiceAppointmentIntegrationOrchestratorPostStageRunner(_mockServiceProvider);

                ShimPluginRunner.AllInstances.RunPluginIServiceProvider = (pr, sp) =>
                {
                    pr.Execute();
                };

                ShimIntegrationPluginHelpers.GetApiSettingsXrmString = (_mockXrm, s) => { return new StubApiIntegrationSettings(); };
                ShimIntegrationPluginHelpers.GetListOfNewPatientsIOrganizationServiceEntityMCSLoggerIPluginExecutionContext = (orgSvc, appt, l, pec) =>
                {
                    customerId = expectedCustomerId;
                    oldCustomerId = expectedOldCustomerId;
                    var resp = new Dictionary<Guid, bool>
                    {
                        { expectedCustomerId, true },
                        { expectedOldCustomerId, false }
                    };
                    return resp;
                };

                _mockServiceAppointmentIntegrationOrchestratorPostStageRunner = new ShimServiceAppointmentIntegrationOrchestratorPostStageRunner(pluginRunner)
                {
                    RunProxyAdd = () =>
                    {
                        ShimCvtHelper.SetServiceAppointmentPermissionsIOrganizationServiceEntityMCSLogger = (ctx, sa, logger) => { };
                        ShimRestPoster.PostOf2StringStringStringM0StringStringStringStringStringStringBooleanStringStringInt32OutMCSLogger<ProxyAddRequestMessage, ProxyAddResponseMessage>(
                        (string s1, string s2, string s3, ProxyAddRequestMessage payload, string s4, string s5, string s6, string s7, string s8, string s9, bool b, string s10, string s11, out int lag, MCSLogger l)
                        =>
                        {
                            lag = 0;
                            return new StubProxyAddResponseMessage();
                        });
                        ShimRestPoster.Behavior = ShimBehaviors.DefaultValue;
                        _mockServiceAppointmentIntegrationOrchestratorPostStageRunner.ExecuteBookServiceAppointmentServiceAppointment = (sa, psa) =>
                        {
                            _mockServiceAppointmentIntegrationOrchestratorPostStageRunner.RunProxyAdd = () =>
                            {
                                return true;
                            };
                        };
                        _mockServiceAppointmentCreatePostStageRunner = new ShimServiceAppointmentCreatePostStageRunner(new ServiceAppointmentCreatePostStageRunner(_mockServiceProvider))
                        {
                            ProcessProxyAddToVistaResponseProxyAddResponseMessage = (resp) =>
                            {
                            }
                        };

                        _mockServiceAppointmentCreatePostStageRunner.Instance.RunPlugin(_mockServiceProvider);
                        return true;
                    },
                    RunVistaBoolean = (b) =>
                    {
                        ShimVistaPluginHelpers.GetChangedPatientsEntityIOrganizationServiceMCSLoggerBooleanOut = (Entity sa, IOrganizationService ctx, MCSLogger logger, out bool isBooking) =>
                        {
                            isBooking = true;
                            return new List<Guid>();
                        };
                        ShimVistaPluginHelpers.RunVistaIntegrationServiceAppointmentXrmMCSLogger = (sa, ctx, logger) => { return true; };
                        ShimRestPoster.PostOf2StringStringStringM0StringStringStringStringStringStringBooleanStringStringInt32OutMCSLogger<TmpHealthShareMakeCancelOutboundRequestMessage, TmpHealthShareMakeCancelOutboundResponseMessage>(
                        (string s1, string s2, string s3, TmpHealthShareMakeCancelOutboundRequestMessage payload, string s4, string s5, string s6, string s7, string s8, string s9, bool b2, string s10, string s11, out int lag, MCSLogger l)
                        =>
                        {
                            lag = 0;
                            return new TmpHealthShareMakeCancelOutboundResponseMessage();
                        });
                        _mockServiceAppointmentVistaHealthShareUpdatePostStageRunner = new ShimServiceAppointmentVistaHealthShareUpdatePostStageRunner(new ServiceAppointmentVistaHealthShareUpdatePostStageRunner(_mockServiceProvider))
                        {
                            ProcessVistaResponseTmpHealthShareMakeCancelOutboundResponseMessage = (resp) => { }
                        };
                        _mockServiceAppointmentVistaHealthShareUpdatePostStageRunner.Instance.RunPlugin(_mockServiceProvider);
                        return true;
                    },
                    RunVmr = () =>
                    {
                        ShimCvtHelper.isGfeServiceActivityServiceAppointmentIOrganizationServiceMCSLogger = (sa, ctx, logger) => { return false; };
                        _mockServiceAppointmentVmrUpdatePostStageRunner = new ShimServiceAppointmentVmrUpdatePostStageRunner(new ServiceAppointmentVmrUpdatePostStageRunner(_mockServiceProvider))
                        {
                            CreateAndSendVirtualMeetingRoomXrmServiceAppointmentGuidString = (ctx, sa, uid, orgName) => { return new StubVirtualMeetingRoomCreateResponseMessage(); },
                            ProcessVirtualMeetingRoomCreateResponseMessageVirtualMeetingRoomCreateResponseMessage = (resp) => { }
                        };
                        _mockServiceAppointmentVmrUpdatePostStageRunner.Instance.RunPlugin(_mockServiceProvider);
                        return true;
                    },
                };

                //Act
                pluginRunner.RunPlugin(_mockServiceProvider);

                //Assert
                Assert.AreNotEqual(customerId, Guid.Empty);
                Assert.AreNotEqual(oldCustomerId, Guid.Empty);
                Assert.AreEqual(customerId, expectedCustomerId);
                Assert.AreEqual(oldCustomerId, expectedOldCustomerId);
                Assert.AreNotEqual(Guid.Empty, actualServiceAppt.Id);
                Assert.AreEqual(actualServiceAppt.Id, expectedServiceAppt.Id);
            }
        }
    }
}