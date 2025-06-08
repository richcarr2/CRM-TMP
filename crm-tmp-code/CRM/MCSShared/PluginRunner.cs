using MCSHelperClass;
using MCSShared;
using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace VA.TMP.CRM
{
    public abstract class PluginRunner
    {
        #region Private Fields
        public IServiceProvider _ServiceProvider;
        private IPluginExecutionContext _PluginExecutionContext;
        private IOrganizationServiceFactory _OrganizationServiceFactory;
        private IOrganizationService _OrganizationService;
        //private ITracingService _TracingService;
        private TimestampedTracingService _TracingService;
        private MCSLogger _Logger;
        private UtilityFunctions _UtilityFunctions;
        private MCSSettings _McsSettings;
        private MCSHelper _McsHelper;
        private Entity _PrimaryEntity;
        #endregion

        #region Constructor
        /// <summary>
        /// This is the constructor that takes the serviceProvider and initializes the logger and serviceProvider class variables
        /// </summary>
        /// <param name="serviceProvider">Object Received from the Execute method of the plugin</param>
        protected PluginRunner(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new InvalidPluginExecutionException("Invalid Plug Registration");
            _ServiceProvider = serviceProvider;
            Initialize();
        }
        #endregion

        #region Public Methods/Properties
        /// <summary>
        /// ServiceProvider getter - must already be set from the Execute parameter
        /// </summary>
        public IServiceProvider ServiceProvider => _ServiceProvider;

        /// <summary>
        /// Plugin Execution Context getter - instantiated through Service Provider if it doesn't already exist
        /// </summary>
        public IPluginExecutionContext PluginExecutionContext => _PluginExecutionContext ??
                       (_PluginExecutionContext =
                        (IPluginExecutionContext)ServiceProvider.GetService(typeof(IPluginExecutionContext)));

        /// <summary>
        /// Organization Service Factory getter - instantiated through Service Provider if it doesn't already exist
        /// </summary>
        public IOrganizationServiceFactory OrganizationServiceFactory => _OrganizationServiceFactory ??
                       (_OrganizationServiceFactory =
                        (IOrganizationServiceFactory)ServiceProvider.GetService(typeof(IOrganizationServiceFactory)));

        /// <summary>
        /// Organization Service getter - instantiated through OrganizationServiceFactory if it doesn't already exist
        /// </summary>
        public IOrganizationService OrganizationService => _OrganizationService ??
                       (_OrganizationService =
                        OrganizationServiceFactory.CreateOrganizationService(PluginExecutionContext.InitiatingUserId));

        /// <summary>
        /// Tracing Service getter - instantiated through ServiceProvider if it doesn't already exist
        /// </summary>
        //public ITracingService TracingService => _TracingService ??
        //               (_TracingService =
        //                (ITracingService)ServiceProvider.GetService(typeof(ITracingService)));
        public TimestampedTracingService TracingService => _TracingService ??
               (_TracingService =
                new TimestampedTracingService(ServiceProvider));

        /// <summary>
        /// Logger getter - instantiates through Org Service and Tracing Service if it doesn't already exist
        /// </summary>
        public MCSLogger Logger => _Logger ??
                       (_Logger = new MCSLogger
                       {
                           setService = OrganizationService,
                           setTracingService = TracingService,
                           setModule = string.Format("{0}:", GetType())
                       });

        /// <summary>
        /// Utilities getter - instantiated through Org Service and Logger if it doesn't already exist
        /// </summary>
        public UtilityFunctions Utilities => _UtilityFunctions ??
                    (_UtilityFunctions = new UtilityFunctions
                    {
                        setService = OrganizationService,
                        setlogger = Logger
                    });

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
        /// McsHelper getter - instantiated through Primary and Secondary Entity if it doesnt already exist
        /// </summary>
        public MCSHelper McsHelper => _McsHelper ?? (_McsHelper = new MCSHelper(PrimaryEntity, GetSecondaryEntity()));

        /// <summary>
        /// Primary Entity getter - instantiated through virtual or overridden GetPrimaryEntity()
        /// </summary>
        public Entity PrimaryEntity => _PrimaryEntity ?? (_PrimaryEntity = GetPrimaryEntity());

        /// <summary>
        /// Sets up the logging and Service Provider for all plugins to use
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Initialize()
        {
            TracingService.Trace("PluginRunner Initialized");
            Logger.setDebug = McsSettings.getDebug;
            Logger.setTxnTiming = McsSettings.getTxnTiming;
            Logger.setGranularTiming = McsSettings.getGranular;
            Logger.setEntityName = getEntityName();
            Logger.setEntityId = PrimaryEntity.Id;
        }

        /// <summary>
        /// Default "name" field for Logger
        /// </summary>
        /// <returns>Name of the record</returns>
        /// <remarks>if primary "name" field on entity is not mcs_name, cvt_name, or subject, it defaults to the Entity Type instead</remarks>
        internal string getEntityName()
        {
            var name = PrimaryEntity == null ? "" : PrimaryEntity.Attributes.Contains("mcs_name") ? PrimaryEntity.Attributes["mcs_name"].ToString() :
                       PrimaryEntity.Attributes.Contains("cvt_name") ? PrimaryEntity.Attributes["cvt_name"].ToString() :
                       PrimaryEntity.Attributes.Contains("subject") ? PrimaryEntity.Attributes["subject"].ToString() :
                       PrimaryEntity.Attributes.Contains("name") ? PrimaryEntity.Attributes["name"].ToString() : PrimaryEntity.LogicalName;
            return name;
        }

        /// <summary>
        /// Baseline Execute method that is immediately called from all plugins - this wraps all business logic with standardized try catch, logging, timing, and validation
        /// </summary>
        /// <param name="serviceProvider">CRM Service Provider used to Retrieve Org Service, PluginContext, Tracing Service, etc.</param>
        public void RunPlugin(IServiceProvider serviceProvider)
        {
            try
            {
                Logger.setMethod = "Execute";
                Logger.WriteDebugMessage("Begin " + this.GetType());
                if (PluginExecutionContext.InputParameters.Contains("Target") &&
                    (PluginExecutionContext.InputParameters["Target"] is Entity ||
                    PluginExecutionContext.InputParameters["Target"] is EntityReference))
                {
                    //Start the timing of the Plugin
                    Logger.WriteTxnTimingMessage(string.Format("Starting : {0}", this.GetType()));

                    //Run the Business Logic of the plugin
                    Execute();
                    Logger.setMethod = "Execute";
                    //End the timing for the plugin
                    Logger.WriteTxnTimingMessage(string.Format("Ending : {0}", this.GetType()));
                    Logger.WriteDebugMessage("Ending " + this.GetType());
                }
                else if (PluginExecutionContext.InputParameters.Contains("EntityMoniker")
                    && PluginExecutionContext.InputParameters["EntityMoniker"] is EntityReference)
                {
                    //Start the timing of the Plugin
                    Logger.WriteTxnTimingMessage(string.Format("Starting SetStateRequest : {0}", GetType()));

                    //Run the Business Logic of the plugin
                    Execute();
                    Logger.setMethod = "Execute";
                    //End the timing for the plugin
                    Logger.WriteTxnTimingMessage(string.Format("Ending : {0}", GetType()));
                    Logger.WriteDebugMessage("Ending " + GetType());
                }
                else if (PluginExecutionContext.InputParameters.Contains("Query")
                    && (PluginExecutionContext.InputParameters["Query"] is QueryExpression || PluginExecutionContext.InputParameters["Query"] is FetchExpression))
                {
                    //Start the timing of the Plugin
                    Logger.WriteTxnTimingMessage(string.Format("Starting RetrieveMultiple : {0}", GetType()));

                    //Run the Business Logic of the plugin
                    Execute();
                    Logger.setMethod = "Execute";
                    //End the timing for the plugin
                    Logger.WriteTxnTimingMessage(string.Format("Ending : {0}", GetType()));
                    Logger.WriteDebugMessage("Ending " + GetType());

                }
                else
                {
                    Logger.WriteToFile("Invalid Plugin Registration");
                    throw new InvalidPluginExecutionException("Invalid Plugin Registration");
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
        public abstract void Execute();

        /// <summary>
        /// optional attribute per class to use as primary entity for mcsHelper - to be overridden if the "Target" is not the intended primary entity
        /// </summary>
        /// <returns></returns>
        public virtual Entity GetPrimaryEntity()
        {
            if (PluginExecutionContext.InputParameters.Contains("Target"))
            {
                if (PluginExecutionContext.InputParameters["Target"] is EntityReference)
                {
                    var entityReference = PluginExecutionContext.InputParameters["Target"].GetType().GetProperty("Id").GetValue(PluginExecutionContext.InputParameters["Target"], null);
                    return OrganizationService.Retrieve("mcs_missingresource", new Guid(entityReference.ToString()), new ColumnSet(true));
                }
                return (Entity)PluginExecutionContext.InputParameters["Target"];
            }
            else if (PluginExecutionContext.InputParameters.Contains("EntityMoniker"))
            {
                var PrimaryReference = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];
                return new Entity(PrimaryReference.LogicalName) { Id = PrimaryReference.Id };
            }
            else
                return string.IsNullOrEmpty(PluginExecutionContext.PrimaryEntityName) ? null : new Entity(PluginExecutionContext.PrimaryEntityName);
        }

        /// <summary>
        /// optional attribute used by McsHelper - to be overridden if the preimage is not the intended secondary entity
        /// </summary>
        /// <returns></returns>
        public virtual Entity GetSecondaryEntity()
        {
            return (PluginExecutionContext.PreEntityImages.ContainsKey("pre") ? PluginExecutionContext.PreEntityImages["pre"] : null);
        }

        /// <summary>
        /// abstract string that is used to determine if log.debug statements get outputted into logs.  
        /// </summary>
        public abstract string McsSettingsDebugField { get; }
        #endregion
    }
}
