using MCSHelperClass.Fakes;
using MCSUtilities2011.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Fakes;
using System.Fakes;

namespace VA.TMP.Shared.UnitTests
{
    public class BaseUnitTest
    {
        internal StubIExecutionContext _mockExecutionContext;
        internal StubIOrganizationService _mockOrgService;
        internal StubIOrganizationServiceFactory _mockServiceFactory;
        internal StubIPluginExecutionContext _mockPluginExecutionContext;
        internal StubIServiceProvider _mockServiceProvider;
        internal StubITracingService _mockTracingService;
        internal StubTimestampedTracingService _mockTimestampedTracingService;
        internal StubMCSHelper _mockHelper;
        internal StubMCSLogger _mockLogger;
        internal StubMCSSettings _mockSettings;
        internal StubUtilityFunctions _mockUtilityFunctions;

        [TestInitialize]
        public virtual void Initialize()
        {
            _mockExecutionContext = new StubIExecutionContext();
            _mockPluginExecutionContext = new StubIPluginExecutionContext() { 
                InputParametersGet = () => { return new ParameterCollection(); },
                OutputParametersGet = () => { return new ParameterCollection(); }
            };
            _mockHelper = new StubMCSHelper();
            _mockLogger = new StubMCSLogger();
            _mockOrgService = new StubIOrganizationService()
            {
                RetrieveStringGuidColumnSet = (s, g, cs) => { return new Entity(s, g); }
            };
            _mockSettings = new StubMCSSettings();
            _mockServiceFactory = new StubIOrganizationServiceFactory()
            {
                CreateOrganizationServiceNullableOfGuid = (userId) => { return _mockOrgService; }
            };
            _mockServiceProvider = new StubIServiceProvider()
            {
                GetServiceType = (serviceType) =>
                {
                    if (serviceType.Equals(typeof(IExecutionContext))) return _mockExecutionContext;
                    if (serviceType.Equals(typeof(IOrganizationService))) return _mockOrgService;
                    if (serviceType.Equals(typeof(IOrganizationServiceFactory))) return _mockServiceFactory;
                    if (serviceType.Equals(typeof(IPluginExecutionContext))) return _mockPluginExecutionContext;
                    if (serviceType.Equals(typeof(ITracingService))) return _mockTracingService;
                    return null;
                }
            };
            _mockTracingService = new StubITracingService();
            _mockTimestampedTracingService = new StubTimestampedTracingService(_mockServiceProvider);
            _mockUtilityFunctions = new StubUtilityFunctions();
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
        }
    }
}
