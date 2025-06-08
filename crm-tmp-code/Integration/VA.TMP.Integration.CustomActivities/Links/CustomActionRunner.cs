using System;
using System.Activities;
using System.ServiceModel;
using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace VA.TMP.Integration.CustomActivities
{
    public abstract class CustomActionRunner
    {

        #region Private Fields
        public CodeActivityContext _ExecutionContext;
        private IOrganizationServiceFactory _OrganizationServiceFactory;
        private IOrganizationService _OrganizationService;
        private ITracingService _TracingService;
        private MCSLogger _Logger;
        private MCSSettings _McsSettings;
        private IWorkflowContext _workflowExecutionContext;
        #endregion

        public abstract string McsSettingsDebugField { get; }

        public CustomActionRunner(CodeActivityContext executionContext)
        {
            _ExecutionContext = executionContext;
        }

        public CodeActivityContext ExecutionContext => _ExecutionContext;
        /// <summary>
        /// Organization Service Factory getter - instantiated through Service Provider if it doesn't already exist
        /// </summary>
        public IOrganizationServiceFactory OrganizationServiceFactory
        {
            get
            {
                return _OrganizationServiceFactory ??
                       (_OrganizationServiceFactory = ExecutionContext.GetExtension<IOrganizationServiceFactory>());
            }
        }

        public IWorkflowContext WorkflowExecutionContext
        {
            get
            {
                return _workflowExecutionContext ?? (_workflowExecutionContext = ExecutionContext.GetExtension<IWorkflowContext>());
            }
        }
        /// <summary>
        /// Organization Service getter - instantiated through OrganizationServiceFactory if it doesn't already exist
        /// </summary>
        public IOrganizationService OrganizationService
        {
            get
            {
                return _OrganizationService ??
                       (_OrganizationService = OrganizationServiceFactory.CreateOrganizationService(WorkflowExecutionContext.UserId));
            }
        }

        /// <summary>
        /// Tracing Service getter - instantiated through ServiceProvider if it doesn't already exist
        /// </summary>
        public ITracingService TracingService
        {
            get
            {
                return _TracingService ??
                       (_TracingService =
                        ExecutionContext.GetExtension<ITracingService>());
            }
        }

        /// <summary>
        /// Logger getter - instantiates through Org Service and Tracing Service if it doesn't already exist
        /// </summary>
        public MCSLogger Logger
        {
            get
            {
                return _Logger ??
                       (_Logger = new MCSLogger
                       {
                           setService = OrganizationService,
                           setTracingService = TracingService,
                           setModule = string.Format("{0}:", GetType())
                       });
            }
        }

        /// <summary>
        /// McsSettings getter - instantiated through Org Service, Logger, and debug field (passed in per class)
        /// </summary>
        public MCSSettings McsSettings
        {
            get
            {
                if (_McsSettings == null)
                {
                    _McsSettings = new MCSSettings
                    {
                        setService = OrganizationService,
                        setDebugField = McsSettingsDebugField,
                        systemSetting = "Active Settings",
                        setLogger = Logger
                    };

                    _McsSettings.GetStartupSettings();
                }

                return _McsSettings;
            }
        }

        /// <summary>
        /// Sets up the logging and Service Provider for all plugins to use
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Initialize()
        {
            TracingService.Trace("Custom Action Runner Initialized");
            Logger.setDebug = McsSettings.getDebug;
            Logger.setTxnTiming = McsSettings.getTxnTiming;
            Logger.setGranularTiming = McsSettings.getGranular;
        }

        /// <summary>
        /// Baseline Execute method that is immediately called from all Custom Workflow Actions - this wraps all business logic with standardized try catch, logging, timing, and validation
        /// </summary>
        public void RunCustomAction(AttributeCollection inputs)
        {
            try
            {
                Logger.setMethod = "Run";
                Logger.WriteDebugMessage("Begin " + GetType());
                if (ExecutionContext != null)
                {
                    //Start the timing of the CWA
                    Logger.WriteTxnTimingMessage(string.Format("Starting : {0}", GetType()));

                    //Run the Business Logic of the CWA
                    Execute(inputs);
                    Logger.setMethod = "Run";
                    //End the timing for the CWA
                    Logger.WriteTxnTimingMessage(string.Format("Ending : {0}", GetType()));
                    Logger.WriteDebugMessage("Ending " + GetType());
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Logger.WriteToFile(CvtHelper.BuildExceptionMessage(ex));
                throw new InvalidPluginExecutionException(McsSettings.getUnexpectedErrorMessage);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("custom"))
                {
                    Logger.WriteToFile(CvtHelper.BuildExceptionMessage(ex));
                    throw new InvalidPluginExecutionException(ex.Message.Substring(6));
                }
                else
                {
                    Logger.setMethod = "Execute";
                    Logger.WriteToFile(CvtHelper.BuildExceptionMessage(ex));
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }

        /// <summary>
        /// This is the primary method that each class must implement and contains the business logic
        /// </summary>
        public abstract void Execute(AttributeCollection inputs);
    }
}
