using MCSUtilities2011;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;

namespace MCS.ApplicationInsights
{
    public abstract class AILogicBase 
    {
        /// <summary>
        /// The Microsoft Dynamics CRM organization service for SYSTEM user (Admin Privs).
        /// </summary>
        public IOrganizationService OrganizationService;

        /// <summary>
        /// The Microsoft Dynamics CRM organization service with elevation.
        /// </summary>
        public IOrganizationService ElevatedOrganizationService;

        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Provides logging run-time trace information for plug-ins. 
        /// </summary>
        public TimestampedTracingService TracingService;

        /// <summary>
        /// IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
        /// </summary>
        public IPluginExecutionContext PluginExecutionContext;

        /// <summary>
        /// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/> 
        /// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
        /// </summary>
        public IServiceEndpointNotificationService NotificationService;

        /// <summary>
        /// This is the Stopwatch to write out the processing time (since plugin initiation) that each trace statement reaches
        /// Provides granular timing per Trace statement
        /// </summary>
        public Stopwatch Timer;

        /// <summary>
        /// The Initiating entity for the plugin for Create or Update Messages; 
        /// or the Entity Moniker re-cast as an entity for other non-query Messages
        /// will be null for Query Messages
        /// </summary>
        public Entity PrimaryEntity;

        /// <summary>
        /// Logger getter - instantiates through Org Service and Tracing Service if it doesn't already exist
        /// </summary>
        public MCSLogger Logger => _Logger ??
                       (_Logger = new MCSLogger
                       {
                           setService = OrganizationService,
                           setTracingService = (TimestampedTracingService)TracingService,
                           setModule = string.Format("{0}:", GetType())
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

        public string Secure { get; set; }

        public string UnSecure { get; set; }

        public Guid PluginExecutionInstanceId { get; set; }

        internal AppInsightsLogData LogData { get; set; }

        //public StringBuilder TraceLog { get; set; }

        public Entity SettingsRecord { get; set; }

        internal bool VerboseTracing { get; set; }

        internal PluginLogger pluginLogger { get; set; }

        internal Functionality functionality { get; set; }

        internal MCSLogger _Logger;

        internal MCSSettings _McsSettings;

        /// <summary>
        /// Standard Constructor for all PluginLogic Classes
        /// </summary>
        /// <param name="serviceProvider"></param>
        public AILogicBase(IServiceProvider serviceProvider)
        {
            Timer = Stopwatch.StartNew();
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            ServiceProvider = serviceProvider;

            // Obtain the execution context service from the service provider.
            PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the tracing service from the service provider.
            //TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            TracingService = new TimestampedTracingService(serviceProvider);

            //TraceLog = new StringBuilder();

            pluginLogger = new PluginLogger(TracingService);

            // Get the notification service from the service provider.
            //NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));

            // Obtain the organization factory service from the service provider.
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Use the factory to generate the organization service.
            OrganizationService = factory.CreateOrganizationService(null);

            // Use the factory to generate the elevated organization service.
            ElevatedOrganizationService = factory.CreateOrganizationService(null);

            // Use for tracking a unique ID for downstream calls
            PluginExecutionInstanceId = Guid.NewGuid();

            LogData = new AppInsightsLogData();
            

            

            // Log Completion of Constructor with timer to monitor performance
            Trace($"{PluginExecutionInstanceId}: Plugin Execution Instance, Base Constructed, Correlation Id: {PluginExecutionContext.CorrelationId}, Initiating User: {PluginExecutionContext.InitiatingUserId}");
        }

        /// <summary>
        /// overload for Primary Trace method.  Writes to the TracingService.Trace
        /// </summary>
        /// <param name="message">the message to be written</param>
        /// <param name="level">the log level to write with</param>
        public void Trace(string message, int level = 4, bool writeTraceToAI = true)
        {
            if (level <= 4 && level >= 1)
                Trace(message, (LogLevel)level, writeTraceToAI);
            else
                Trace(message, LogLevel.Debug, writeTraceToAI);
        }

        /// <summary>
        /// Writes a trace message to the CRM trace log.
        /// </summary>
        /// <param name="message">Message name to trace.</param>
        public void Trace(string message, LogLevel level, bool writeTraceToAI = true)
        {
            if (string.IsNullOrWhiteSpace(message) || TracingService == null)
            {
                return;
            }
            if (Timer != null)
                message = $"{Timer.ElapsedMilliseconds}ms: {message}";
            else
                message = $"{message}; Warning: No Timer Found";
            message = $"{level.ToString()}: {message}";

            //TracingService.Trace(message);
            Logger.WriteDebugMessage(message);
            if (writeTraceToAI)
                pluginLogger.Trace(message);
        }

        /// <summary>
        /// Determines the appropriate Input Parameter to become the Primary Entity and sets it to Primary Entity
        /// </summary>
        /// <remarks>For Retrieve Multiple and certain obscure messages that have no primary entity concept, this sets Primary Entity to null</remarks>
        /// <remarks>For Custom Actions, override this method to set the Primary Entity if making use of Primary Entity as the Action Message name won't already exist in this list</remarks>
        internal virtual void SetPrimaryEntity()
        {
            Trace($"Message Name: {PluginExecutionContext.MessageName}.");
            switch (PluginExecutionContext.MessageName)
            {
                case "Create":
                case "Update":
                case "Upsert":
                    PrimaryEntity = (Entity)PluginExecutionContext.InputParameters["Target"];
                    break;
                case "Assign":
                case "Associate":
                case "Delete":
                case "Disassociate":
                case "GrantAccess":
                case "ModifyAccess":
                case "RevokeAccess":
                case "Retrieve":
                    var target = (EntityReference)PluginExecutionContext.InputParameters["Target"];
                    PrimaryEntity = new Entity { Id = target.Id, LogicalName = target.LogicalName };
                    break;
                case "SetState":
                case "SetStateDynamicEntity":
                    var moniker = (EntityReference)PluginExecutionContext.InputParameters["EntityMoniker"];
                    PrimaryEntity = new Entity { Id = moniker.Id, LogicalName = moniker.LogicalName };
                    break;
                case "RetrieveMultiple":
                    PrimaryEntity = null;
                    break;
                case "mcs_ApplicationInsightsLogger":
                    PrimaryEntity = null;
                    break;
                default:
                    //Trace($"Unknown Message Type: {PluginExecutionContext.MessageName}.  Please check documentation and add handler to list: {PluginExecutionContext.MessageName}", LogLevel.Warning);
                    PrimaryEntity = null;
                    break;
            }
        }

        /// <summary>
        /// This should always be called from the (Plugin) class that implements IPlugin - it manages exception handling and setting of Primary Entity and logging to App Insights
        /// </summary>
        public void Execute()
        {
            try
            {
                if (PluginExecutionContext == null)
                    throw new ArgumentNullException("PluginExecutionContext");
                if (TracingService == null)
                    throw new ArgumentNullException("TracingService");
                if (OrganizationService == null)
                    throw new ArgumentNullException("OrganizationService");
                //if (ElevatedOrganizationService == null)
                //    throw new ArgumentNullException("ElevatedOrganizationService");

                Trace("Setting Primary Entity");
                // In plugins for common operations (other than Retrieve Multiple), 
                //   either a Primary Entity or EntityReference will be defined.  
                // SetPrimaryEntity sets Primary Entity to this value (derived from the PluginExecutionContext)
                SetPrimaryEntity();

                SettingsRecord = GetSettings(ElevatedOrganizationService);

                SetupLogger();

                Trace($"Entering ExecuteLogic()", LogLevel.Information);
                // Invoke the custom implementation 
                ExecuteLogic();
                // now exit - if the derived plug-in has incorrectly registered overlapping event registrations,
                // guard against multiple executions.
                Trace($"Completed ExecuteLogic()");
                return;
            }
            catch (ArgumentNullException ex)
            {
                Trace($"Unsuccessfully initialized input: {ex.Message}.  Re-throwing...", LogLevel.Error);
                throw;
            }
            catch(TimeoutException ex)
            {
                Trace($"A Timeout Exception has occurred: {ex.Message}. Re-throwing...", LogLevel.Error);
                throw;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                Trace($"Fault Exception thrown by org service: {ex.Message}. Detailed Message: {ex.Detail.Message}. Re-throwing...", LogLevel.Error);
                throw;
            }
            catch (Exception ex)
            {
                LogData.Exception = new AIException(ex, ExceptionSeverity.Critical);
                Trace($"Exception thrown in plugin: {ex.Message}. Inner Trace: {WriteOutExceptions(ex)}", LogLevel.Error);
                if (ex.InnerException != null)
                    throw new InvalidPluginExecutionException(ex.Message, ex.InnerException);
                else
                    throw new InvalidPluginExecutionException(ex.Message);
            }
            finally
            {
                Trace($"Exiting LogicBase.Execute()", LogLevel.Information);
                try
                {
                    if (GetType().Name != "LogToAppInsightsLogic" && LogToAppInsights())
                    {
                        var pluginTime = ((double)Timer.ElapsedMilliseconds);
                        AddCustomDimension("TraceLog", pluginLogger.AITraceLog.ToString().Trim());
                        AddCustomDimension("CorrelationId", PluginExecutionContext.CorrelationId.ToString());
                        AddCustomDimension("PluginInstance", PluginExecutionInstanceId.ToString());
                        AddCustomDimension("MessageName", PluginExecutionContext.MessageName);
                        AddCustomDimension("Functionality", functionality.ToString());
                        AddCustomMetric("PluginTime", pluginTime);
                        AddCustomMetric("PluginDepth", PluginExecutionContext.Depth);
                        if (PluginExecutionContext.Mode == 1 || LogData.Exception == null) //Async or Sync with No exception to block action call
                            LogToAi();
                        else
                            LogException();
                    }
                    else
                        Trace($"Not Logging to App Insights. Type: {GetType().Name}|LogToAI: {LogToAppInsights()}. Field: {LogToAppInsightsField}");
                }
                catch(Exception ex)
                {
                    Trace($"Failed to log plugin data to app insights: {ex.ToString()}");
                }
                Trace("Plugin Completed");
                Timer.Stop();
            }
        }

        /// <summary>
        /// Gets the full System Settings record with all attributes
        /// </summary>
        /// <param name="OrganizationService"></param>
        /// <returns>System Settings Record</returns>
        public static Entity GetSettings(IOrganizationService orgService)
        {
            var settingsQuery = new QueryExpression()
            {
                EntityName = "mcs_setting", // table names needs to be mcs_setting
                ColumnSet = new ColumnSet(true),
                TopCount = 1
            };
            var retrievedSettings = orgService.RetrieveMultiple(settingsQuery);
            var settings = retrievedSettings.Entities.FirstOrDefault();
            return settings;
        }

        /// <summary>
        /// optional attribute used by McsHelper - to be overridden if the preimage is not the intended secondary entity
        /// </summary>
        /// <returns></returns>
        public virtual Entity GetSecondaryEntity()
        {
            return (PluginExecutionContext.PreEntityImages.ContainsKey("pre") ? PluginExecutionContext.PreEntityImages["pre"] : null);
        }

        private void LogException()
        {
            LogData.CustomLogType = AiLogType.Exception;
            //LogData.CustomDimensions = 
            var logger = new AppInsightsLogger(LogData.InstrumentationKey, LogData.Url, LogData.OperationName, LogData.UserId, LogData.CorrelationId, LogData.OperationSyntheticSource);
            var json = logger.GetExceptionJsonString(LogData.Exception, LogData.CustomDimensions, LogData.CustomMetrics);
            var errors = logger.SendToAi(json);
            foreach (var e in errors)
                Trace("Failures writing to AI: " + e.Message);
            if (errors != null && errors.Count > 0)
                Trace($"Failed to write to app insights following string: {json}");
            Trace(logger.GetExceptionJsonString(LogData.Exception, LogData.CustomDimensions, LogData.CustomMetrics));
            Trace("Write Exception to AI directly (via REST) due to OrganizationService failure blocking cascading action");
        }

        public void LogToAi()
        {
            var req = new OrganizationRequest("mcs_AppInsightLogger");
            var json = SerializationHelper.Serialize(LogData);
            req.Parameters["SerializedLogData"] = json;
            try
            {
                OrganizationService.Execute(req);
                Trace("Wrote to App Insights: " + json);
            }
            catch (Exception ex)
            {
                Logger.WriteDebugMessage($"Save to AI Failed. Exception: {ex}.");
                Logger.WriteDebugMessage($"Content attempted to be sent to AI: {json}.");
            }
        }

        /// <summary>
        /// Exception method helper that recursively loops through inner exceptions printing out the messages
        /// </summary>
        /// <param name="ex">Exception being parsed</param>
        /// <returns>string representation of Exception and its Children Exceptions</returns>
        internal string WriteOutExceptions(Exception ex)
        {
            var message = ex.Message;
            if (ex.InnerException != null)
                return message + "\n--------Inner-------- " + WriteOutExceptions(ex.InnerException);
            else
                return message;
        }

        internal void SetupLogger()
        {
            VerboseTracing = SettingsRecord.GetAttributeValue<bool>("tmp_verbosetracing");
            //var aiKey = VemsCrm.ValuePairService.getKeyValuePair(OrganizationService, "AIInstrumentationKey");// SettingsRecord.GetAttributeValue<string>("mcs_appinsightskey");
            //var aiUrl = VemsCrm.ValuePairService.getKeyValuePair(OrganizationService, "AILoggingEndPoint");//  SettingsRecord.GetAttributeValue<string>("mcs_appinsightsurl") ?? "https://dc.services.visualstudio.com/v2/track";
            var aiKey = SettingsRecord.GetAttributeValue<string>("tmp_applicationinsightsinstrumentationkey");
            var aiUrl = SettingsRecord.GetAttributeValue<string>("tmp_applicationinsightsurl");

            this.LogData = new AppInsightsLogData
            {
                InstrumentationKey = aiKey,
                Url = aiUrl,
                OperationName = "Spark-" + this.GetType().Name,
                OperationSyntheticSource = "SparkPlugin",
                CustomDimensions = new System.Collections.Generic.Dictionary<string, string>(),
                CustomMetrics = new System.Collections.Generic.Dictionary<string, double>(),
                CustomLogType = AiLogType.Request
            };
            LogData.CorrelationId = PluginExecutionContext.CorrelationId.ToString();
            LogData.UserId = PluginExecutionContext.UserId.ToString();
            LogData.Message = $"{LogData.OperationName} Plugin";
            functionality = Functionality.NotSet;
        }

        public void AddCustomMetric(string metricName, decimal metricValue)
        {
            var convertedNumber = Convert.ToDouble(metricValue);
            AddCustomMetric(metricName, convertedNumber);
        }

        public void AddCustomMetric(string metricName, int metricValue)
        {
            var convertedNumber = Convert.ToDouble(metricValue);
            AddCustomMetric(metricName, convertedNumber);
        }

        public void AddCustomMetric(string metricName, double metricValue, int count = 1)
        {
            if (LogData.CustomMetrics == null)
                SetupLogger();
            var newName = metricName + (count++).ToString();
            if (LogData.CustomMetrics.ContainsKey(metricName))
            {
                if (LogData.CustomMetrics.ContainsKey(newName))
                    AddCustomMetric(metricName, metricValue, count);
                else
                    LogData.CustomMetrics.Add(newName, metricValue);
            }
            else
                LogData.CustomMetrics.Add(metricName, metricValue);
        }

        public void AddCustomDimension(string name, string value, int count = 1)
        {
            if (LogData.CustomDimensions == null)
                SetupLogger();
            var newName = name + (count++).ToString();
            if (LogData.CustomDimensions.ContainsKey(name))
            {
                if (LogData.CustomDimensions.ContainsKey(newName))
                    AddCustomDimension(name, value, count);
                else
                    LogData.CustomDimensions.Add(newName, value);
            }
            else
                LogData.CustomDimensions.Add(name, value);
        }

        /// <summary>
        /// This always must be overridden to provide the custom business logic for which a plugin was written
        /// </summary>
        public abstract void ExecuteLogic();

        public virtual string LogToAppInsightsField
        {
            get
            {
                //return McsSettingsDebugField;
                return "mcs_" + this.GetType().Name.ToLower();
            }
        }

        public virtual bool LogToAppInsights()
        {
            //return true;
            return SettingsRecord.GetAttributeValue<bool?>(LogToAppInsightsField.ToLower()) ?? true;
        }

        public string GetSecureConfigurationValue(string keyName)
        {
            return GetConfigurationValue(keyName, Secure);
        }

        public string GetUnSecureConfigurationValue(string keyName)
        {
            return GetConfigurationValue(keyName, UnSecure);
        }

        public string GetConfigurationValue(string keyName, string configuration)
        {
            var result = string.Empty;

            if (string.IsNullOrEmpty(configuration))
                return result;
            try
            {
                //Parse the Xml to get the settings                                   

                XmlReaderSettings settings = new XmlReaderSettings();
                System.Xml.Schema.XmlSchema schema = new System.Xml.Schema.XmlSchema();
                settings.Schemas.Add(schema);

                settings.ValidationType = ValidationType.Schema;
                StringReader sr = new StringReader(configuration);
                XmlReader reader = XmlReader.Create(sr, settings);


                var xmlDocument = new XmlDocument();

                xmlDocument.Load(reader);

                if (xmlDocument.GetElementsByTagName(keyName).Count <= 0)
                    return result;

                var element = xmlDocument.GetElementsByTagName(keyName);

                result = element[0].InnerText;
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("Error getting plugin configuration value for " + configuration + ": " + e.Message);
            }

            return result;

        }

        /// <summary>
        /// abstract string that is used to determine if log.debug statements get outputted into logs.  
        /// </summary>
        public abstract string McsSettingsDebugField { get; }
    }

    public class ExceptionHelper
    {
        public static List<ParsedStack> GetParsedStacked(Exception e)
        {
            if (string.IsNullOrEmpty(e.StackTrace))
                return null;

            List<ParsedStack> parsedStacks = new List<ParsedStack>();

            Exception currentException = e;
            while (currentException != null)
            {
                ParsedStack parsedStack = ParseStackTrace(e);
                parsedStacks.Add(parsedStack);

                currentException = currentException.InnerException;
            }

            return parsedStacks;
        }

        private static ParsedStack ParseStackTrace(Exception e)
        {
            StackTrace stackTrace = new StackTrace(e);
            StackFrame stackFrame = stackTrace.GetFrame(0);
            ParsedStack aiParsedStack = new ParsedStack
            {
                Method = stackFrame.GetMethod().Name,
                FileName = stackFrame.GetFileName(),
                Line = stackFrame.GetFileLineNumber()
            };

            return aiParsedStack;
        }
    }

    public enum LogLevel
    {
        Unknown = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
        Debug = 4
    }
}
