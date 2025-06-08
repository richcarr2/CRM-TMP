using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Fakes;
using VA.TMP.CRM.Fakes;
using VA.TMP.DataModel;
using VA.TMP.DataModel.Fakes;
using VA.TMP.Integration.Plugins.Helpers.Fakes;
using VA.TMP.Integration.Plugins.Patient_flag.Fakes;
using VA.TMP.Shared.UnitTests;

namespace VA.TMP.Integration.Plugins.UnitTests
{
    [TestClass]
    public class PersonFlagUnitTests : BaseUnitTest
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void HappyPath()
        {
            var actualPatientId = Guid.Empty;
            var expectedPatientId = Guid.NewGuid();

            var actualPatientIdentifier = new mcs_personidentifiers();
            var expectedPatientIdentifier = new mcs_personidentifiers(){
                Id = Guid.NewGuid(),
                mcs_assigningauthority = "USVHA",
                mcs_identifier = "1012901147",
                mcs_identifiertype = new OptionSetValue(125150000),
                mcs_patient = new EntityReference(Contact.EntityLogicalName, actualPatientId)
            };

            ShimPatientGetPatientFlagPreStageRunner _mockPatientGetPatientFlagPreStageRunner;

            //Arrange
            using (ShimsContext.Create())
            {
                ShimPluginRunner.AllInstances.LoggerGet = (instance) => _mockLogger;
                ShimPluginRunner.AllInstances.McsHelperGet = (instance) => _mockHelper;
                ShimPluginRunner.AllInstances.McsSettingsGet = (instance) => _mockSettings;
                ShimPluginRunner.AllInstances.OrganizationServiceGet = (instance) => _mockOrgService;
                ShimPluginRunner.AllInstances.OrganizationServiceFactoryGet = (instance) => _mockServiceFactory;
                ShimPluginRunner.AllInstances.PrimaryEntityGet = (instance) =>
                {
                    if (actualPatientId.Equals(Guid.Empty)) actualPatientId = expectedPatientId;
                    return new Contact()
                    {
                        Id = expectedPatientId
                    };
                };
                ShimPluginRunner.AllInstances.PluginExecutionContextGet = (instance) => _mockPluginExecutionContext;
                ShimPluginRunner.AllInstances.TracingServiceGet = (instance) => _mockTimestampedTracingService;

                ShimXrm.AllInstances.mcs_personidentifiersSetGet = (ctx) =>
                {
                    actualPatientIdentifier = expectedPatientIdentifier;
                    var personalIdentifiers = new List<mcs_personidentifiers>()
                    {
                        expectedPatientIdentifier
                    };
                    return personalIdentifiers.AsQueryable();
                };

                ShimHttpClient.AllInstances.GetAsyncString = (client, uri) =>
                {
                    return System.Threading.Tasks.Task.FromResult(new System.Net.Http.HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK });
                };

                ShimHttpResponseMessage.AllInstances.ContentGet = (msg) =>
                {
                    return new StubHttpContent();
                };

                ShimHttpContent.AllInstances.ReadAsStringAsync = (content) =>
                {
                    return System.Threading.Tasks.Task.FromResult("Test");
                };

                _mockPatientGetPatientFlagPreStageRunner = new ShimPatientGetPatientFlagPreStageRunner(new Patient_flag.PatientGetPatientFlagPreStageRunner(_mockServiceProvider))
                {
                    ApiIntegrationSettingsGet = () =>
                    {
                        return new StubApiIntegrationSettings()
                        {
                            BaseUrl = "http://test.io"
                        };
                    },
                    GetIntegrationSettings = () => { }
                };

                //Act
                _mockPatientGetPatientFlagPreStageRunner.Instance.Execute();
            }

            //Assert
            Assert.AreEqual(expectedPatientId, actualPatientId);
            Assert.AreEqual(actualPatientIdentifier.Id, expectedPatientIdentifier.Id);
        }

        [TestMethod]
        public void FailWhenRequestIsNotSuccessful()
        {
            var actualPatientId = Guid.Empty;
            var expectedPatientId = Guid.NewGuid();
            var patientFlagRequestSuccessful = true;

            var actualPatientIdentifier = new mcs_personidentifiers();
            var expectedPatientIdentifier = new mcs_personidentifiers()
            {
                Id = Guid.NewGuid(),
                mcs_assigningauthority = "USVHA",
                mcs_identifier = "1012901147",
                mcs_identifiertype = new OptionSetValue(125150000),
                mcs_patient = new EntityReference(Contact.EntityLogicalName, expectedPatientId)
            };

            ShimPatientGetPatientFlagPreStageRunner _mockPatientGetPatientFlagPreStageRunner;

            //Arrange
            using (ShimsContext.Create())
            {
                ShimPluginRunner.AllInstances.LoggerGet = (instance) => _mockLogger;
                ShimPluginRunner.AllInstances.McsHelperGet = (instance) => _mockHelper;
                ShimPluginRunner.AllInstances.McsSettingsGet = (instance) => _mockSettings;
                ShimPluginRunner.AllInstances.OrganizationServiceGet = (instance) => _mockOrgService;
                ShimPluginRunner.AllInstances.OrganizationServiceFactoryGet = (instance) => _mockServiceFactory;
                ShimPluginRunner.AllInstances.PrimaryEntityGet = (instance) =>
                {
                    if (actualPatientId.Equals(Guid.Empty)) actualPatientId = expectedPatientId;
                    return new Contact()
                    {
                        Id = expectedPatientId
                    };
                };
                ShimPluginRunner.AllInstances.PluginExecutionContextGet = (instance) => _mockPluginExecutionContext;
                ShimPluginRunner.AllInstances.TracingServiceGet = (instance) => _mockTimestampedTracingService;

                ShimXrm.AllInstances.mcs_personidentifiersSetGet = (ctx) =>
                {
                    actualPatientIdentifier = expectedPatientIdentifier;
                    var personalIdentifiers = new List<mcs_personidentifiers>()
                    {
                        expectedPatientIdentifier
                    };
                    return personalIdentifiers.AsQueryable();
                };

                ShimHttpClient.AllInstances.GetAsyncString = (client, uri) =>
                {
                    patientFlagRequestSuccessful = false;
                    return System.Threading.Tasks.Task.FromResult(new System.Net.Http.HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.BadRequest });
                };

                ShimHttpResponseMessage.AllInstances.ContentGet = (msg) =>
                {
                    return new StubHttpContent();
                };

                ShimHttpContent.AllInstances.ReadAsStringAsync = (content) =>
                {
                    patientFlagRequestSuccessful = true;
                    return System.Threading.Tasks.Task.FromResult("Test");
                };

                _mockPatientGetPatientFlagPreStageRunner = new ShimPatientGetPatientFlagPreStageRunner(new Patient_flag.PatientGetPatientFlagPreStageRunner(_mockServiceProvider))
                {
                    ApiIntegrationSettingsGet = () =>
                    {
                        return new StubApiIntegrationSettings()
                        {
                            BaseUrl = "http://test.io"
                        };
                    },
                    GetIntegrationSettings = () => { }
                };

                //Act
                _mockPatientGetPatientFlagPreStageRunner.Instance.Execute();
            }

            //Assert
            Assert.AreEqual(actualPatientIdentifier.Id, expectedPatientIdentifier.Id);
            Assert.IsFalse(patientFlagRequestSuccessful);
        }
    }
}
