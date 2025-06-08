using MCSShared.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.CRM.Fakes;
using VA.TMP.CRM.ServiceActivity;
using VA.TMP.CRM.ServiceActivity.Fakes;
using VA.TMP.DataModel;
using VA.TMP.DataModel.Fakes;
using VA.TMP.OptionSets;
using VA.TMP.Shared.UnitTests;

namespace VA.TMP.CRM.Plugin.UnitTests
{
    [TestClass]
    public class ServiceAppointmentUnitTests : BaseUnitTest
    {
        private ShimServiceAppointmentUpdatePostStageRunner _mockServiceAppointmentUpdatePostStageRunner;
        private ShimXrm _mockXrm;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void HappyPath()
        {
            var customerId = Guid.NewGuid();
            var oldCustomerId = Guid.NewGuid();
            var saId = Guid.NewGuid();

            bool? actualIsPatient = null;
            bool? expectedIsPatient = false;

            var actualAddedPatients = new List<Guid>();
            var expectedAddedPatients = new List<Guid>() { customerId };

            var actualNewPatients = new List<Guid>();
            var expectedNewPatients = new List<Guid>() { customerId };

            var actualOldPatients = new List<Guid>();
            var expectedOldPatients = new List<Guid>() { oldCustomerId };

            var actualRemovedPatients = new List<Guid>();
            var expectedRemovedPatients = new List<Guid>() { oldCustomerId };

            //Arrange
            var actualServiceAppt = new ServiceAppointment();
            var expectedServiceAppt = new ServiceAppointment()
            {
                ActivityId = saId,
                Id = saId,
                Customers = new List<ActivityParty>() {
                    new ActivityParty {
                        Id = Guid.NewGuid(), PartyId = new EntityReference(Contact.EntityLogicalName, oldCustomerId)
                    }
                },
                Resources = new List<ActivityParty>() { new ActivityParty { PartyId = new EntityReference(SystemUser.EntityLogicalName) } },
                ScheduledStart = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} 10:15:00AM"),
                cvt_relatedschedulingpackage = new EntityReference(cvt_resourcepackage.EntityLogicalName, Guid.NewGuid()),
                cvt_Type = false
            };

            var relatedActivityParty = new ActivityParty
            {
                Id = Guid.NewGuid(),
                PartyId = new EntityReference(Contact.EntityLogicalName, customerId),
                ParticipationTypeMask = new OptionSetValue((int)ActivityPartyParticipationTypeMask.Customer),
                serviceappointment_activity_parties = expectedServiceAppt
            };
            expectedServiceAppt.serviceappointment_activity_parties = new List<ActivityParty>() { relatedActivityParty };

            var kvp = new KeyValuePair<string, Entity>("pre", expectedServiceAppt);
            _mockPluginExecutionContext.PreEntityImagesGet = () => { return new EntityImageCollection() { kvp }; };

            using (ShimsContext.Create())
            {
                ShimPluginRunner.AllInstances.LoggerGet = (instance) => _mockLogger;
                ShimPluginRunner.AllInstances.McsHelperGet = (instance) => _mockHelper;
                ShimPluginRunner.AllInstances.McsSettingsGet = (instance) => _mockSettings;
                ShimPluginRunner.AllInstances.OrganizationServiceGet = (instance) => _mockOrgService;
                ShimPluginRunner.AllInstances.OrganizationServiceFactoryGet = (instance) => _mockServiceFactory;
                ShimPluginRunner.AllInstances.PrimaryEntityGet = (e) => { return new Entity(ServiceAppointment.EntityLogicalName, expectedServiceAppt.Id); };
                ShimPluginRunner.AllInstances.PluginExecutionContextGet = (instance) => _mockPluginExecutionContext;
                ShimPluginRunner.AllInstances.TracingServiceGet = (instance) => _mockTimestampedTracingService;

                ShimOrganizationServiceContext.AllInstances.LoadPropertyEntityRelationship = (context, entity, r) =>
                {
                    entity = expectedServiceAppt;
                };

                ShimXrm.AllInstances.ActivityPartySetGet = (query) =>
                {
                    return new List<ActivityParty>() { new ActivityParty() }.AsQueryable();
                };
                ShimXrm.AllInstances.ServiceAppointmentSetGet = (query) =>
                {
                    return new List<ServiceAppointment>() { expectedServiceAppt }.AsQueryable();
                };

                ShimCvtHelper.ShouldGenerateVeteranEmailGuidIOrganizationServiceMCSLogger = (g, os, l) =>
                {
                    return false;
                };

                ShimServiceAppointmentEmail.AllInstances.getPRGsStringIOrganizationService = (s, os, rgs) =>
                {
                    return new List<cvt_schedulingresource>();
                };
                ShimServiceAppointmentEmail.AllInstances.getPatientVirtualMeetingSpaceNullableOfBooleanOut = (ServiceAppointmentEmail e, out bool? isPatient) =>
                {
                    isPatient = false;
                    actualIsPatient = isPatient;
                    return string.Empty;
                };
                ShimServiceAppointmentEmail.AllInstances.GetRecipientsListOfActivityPartyListOfcvt_schedulingresource = (e, users, prgs) =>
                {
                    return new List<SystemUser>();
                };

                var svc = new StubXrm(_mockOrgService);
                svc.AddObject(expectedServiceAppt);

                _mockXrm = new ShimXrm(svc);

                var pluginRunner = new ServiceAppointmentUpdatePostStageRunner(_mockServiceProvider);

                _mockServiceAppointmentUpdatePostStageRunner = new ShimServiceAppointmentUpdatePostStageRunner(pluginRunner)
                {
                    Execute = () =>
                    {
                        actualNewPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetCurrentPatients(out actualServiceAppt);
                        ShimsContext.ExecuteWithoutShims(() =>
                        {
                            actualOldPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetOldPatients();
                            actualRemovedPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetRemovedPatients(expectedNewPatients, expectedOldPatients);
                            actualAddedPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetAddedPatients(expectedNewPatients, expectedOldPatients);
                        });
                        _mockServiceAppointmentUpdatePostStageRunner.Instance.GenerateProviderEmail(expectedRemovedPatients, expectedAddedPatients, expectedServiceAppt);
                        _mockServiceAppointmentUpdatePostStageRunner.Instance.GeneratePatientAttendanceChangeEmails(expectedRemovedPatients, expectedAddedPatients, expectedServiceAppt);
                    }
                };

                //Act
                pluginRunner.Execute();

                //Assert
                Assert.AreEqual(actualIsPatient.Value, expectedIsPatient.Value);
                Assert.AreEqual(expectedOldPatients.Count, actualOldPatients.Count);
                Assert.AreEqual(expectedRemovedPatients.Count, actualRemovedPatients.Count);
                Assert.AreEqual(expectedAddedPatients.Count, actualAddedPatients.Count);
                Assert.AreEqual(expectedNewPatients.Count, actualNewPatients.Count);
                Assert.AreEqual(expectedServiceAppt.Id, actualServiceAppt.Id);
            }
        }

        [TestMethod]
        public void NoEmailsSent()
        {
            var customerId = Guid.NewGuid();
            var oldCustomerId = Guid.NewGuid();
            var saId = Guid.NewGuid();

            bool? actualIsPatient = null;

            var actualAddedPatients = new List<Guid>();
            var expectedAddedPatients = new List<Guid>();

            var actualNewPatients = new List<Guid>();
            var expectedNewPatients = new List<Guid>();

            var actualOldPatients = new List<Guid>();
            var expectedOldPatients = new List<Guid>();

            var actualRemovedPatients = new List<Guid>();

            var noEmailsSent = false;

            //Arrange
            var actualServiceAppt = new ServiceAppointment();
            var expectedServiceAppt = new ServiceAppointment()
            {
                ActivityId = saId,
                Id = saId,
                Customers = new List<ActivityParty>(),
                Resources = new List<ActivityParty>() { new ActivityParty { PartyId = new EntityReference(SystemUser.EntityLogicalName) } },
                ScheduledStart = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} 10:15:00AM"),
                cvt_relatedschedulingpackage = new EntityReference(cvt_resourcepackage.EntityLogicalName, Guid.NewGuid()),
                cvt_Type = false
            };

            var relatedActivityParty = new ActivityParty
            {
                Id = Guid.NewGuid(),
                PartyId = new EntityReference(Contact.EntityLogicalName, customerId),
                ParticipationTypeMask = new OptionSetValue((int)ActivityPartyParticipationTypeMask.Organizer),
                serviceappointment_activity_parties = expectedServiceAppt
            };
            expectedServiceAppt.serviceappointment_activity_parties = new List<ActivityParty>() { relatedActivityParty };

            var kvp = new KeyValuePair<string, Entity>("pre", expectedServiceAppt);
            _mockPluginExecutionContext.PreEntityImagesGet = () => { return new EntityImageCollection() { kvp }; };

            using (ShimsContext.Create())
            {
                ShimPluginRunner.AllInstances.LoggerGet = (instance) => _mockLogger;
                ShimPluginRunner.AllInstances.McsHelperGet = (instance) => _mockHelper;
                ShimPluginRunner.AllInstances.McsSettingsGet = (instance) => _mockSettings;
                ShimPluginRunner.AllInstances.OrganizationServiceGet = (instance) => _mockOrgService;
                ShimPluginRunner.AllInstances.OrganizationServiceFactoryGet = (instance) => _mockServiceFactory;
                ShimPluginRunner.AllInstances.PrimaryEntityGet = (e) => { return new Entity(ServiceAppointment.EntityLogicalName, expectedServiceAppt.Id); };
                ShimPluginRunner.AllInstances.PluginExecutionContextGet = (instance) => _mockPluginExecutionContext;
                ShimPluginRunner.AllInstances.TracingServiceGet = (instance) => _mockTimestampedTracingService;

                ShimOrganizationServiceContext.AllInstances.LoadPropertyEntityRelationship = (context, entity, r) =>
                {
                    entity = expectedServiceAppt;
                };

                ShimXrm.AllInstances.ActivityPartySetGet = (query) =>
                {
                    return new List<ActivityParty>() { new ActivityParty() }.AsQueryable();
                };
                ShimXrm.AllInstances.ServiceAppointmentSetGet = (query) =>
                {
                    return new List<ServiceAppointment>() { expectedServiceAppt }.AsQueryable();
                };

                ShimCvtHelper.ShouldGenerateVeteranEmailGuidIOrganizationServiceMCSLogger = (g, os, l) =>
                {
                    return false;
                };

                ShimServiceAppointmentEmail.AllInstances.getPRGsStringIOrganizationService = (s, os, rgs) =>
                {
                    return new List<cvt_schedulingresource>();
                };
                ShimServiceAppointmentEmail.AllInstances.getPatientVirtualMeetingSpaceNullableOfBooleanOut = (ServiceAppointmentEmail e, out bool? isPatient) =>
                {
                    isPatient = false;
                    actualIsPatient = isPatient;
                    return string.Empty;
                };
                ShimServiceAppointmentEmail.AllInstances.GetRecipientsListOfActivityPartyListOfcvt_schedulingresource = (e, users, prgs) =>
                {
                    return new List<SystemUser>();
                };

                var svc = new StubXrm(_mockOrgService);
                svc.AddObject(expectedServiceAppt);

                _mockXrm = new ShimXrm(svc);

                var pluginRunner = new ServiceAppointmentUpdatePostStageRunner(_mockServiceProvider);

                _mockServiceAppointmentUpdatePostStageRunner = new ShimServiceAppointmentUpdatePostStageRunner(pluginRunner)
                {
                    Execute = () =>
                    {
                        actualNewPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetCurrentPatients(out actualServiceAppt);
                        ShimsContext.ExecuteWithoutShims(() =>
                        {
                            actualOldPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetOldPatients();
                            actualRemovedPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetRemovedPatients(actualNewPatients, actualOldPatients);
                            actualAddedPatients = _mockServiceAppointmentUpdatePostStageRunner.Instance.GetAddedPatients(actualNewPatients, actualOldPatients);

                            noEmailsSent = actualAddedPatients.Count.Equals(0) && actualOldPatients.Count.Equals(0);
                        });
                        _mockServiceAppointmentUpdatePostStageRunner.Instance.GenerateProviderEmail(actualRemovedPatients, actualAddedPatients, actualServiceAppt);
                        _mockServiceAppointmentUpdatePostStageRunner.Instance.GeneratePatientAttendanceChangeEmails(actualRemovedPatients, actualAddedPatients, actualServiceAppt);
                    }
                };

                //Act
                pluginRunner.Execute();

                //Assert
                Assert.IsNotNull(actualIsPatient);
                Assert.IsFalse(actualIsPatient.Value);
                Assert.IsTrue(actualAddedPatients.Count.Equals(0));
                Assert.IsTrue(expectedAddedPatients.Count.Equals(0));
                Assert.IsTrue(actualOldPatients.Count.Equals(0));
                Assert.IsTrue(expectedOldPatients.Count.Equals(0));
                Assert.IsTrue(actualRemovedPatients.Count.Equals(0));
                Assert.AreEqual(expectedOldPatients.Count, actualOldPatients.Count);
                Assert.AreEqual(expectedAddedPatients.Count, actualAddedPatients.Count);
                Assert.IsTrue(noEmailsSent);
            }
        }
    }
}
