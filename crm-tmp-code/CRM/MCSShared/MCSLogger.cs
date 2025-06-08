using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Runtime.CompilerServices;
using System.ServiceModel;

namespace MCSUtilities2011
{
    [Serializable]
    public class MCSLogger
    {
        private int _sequence = 1;
        private TimestampedTracingService _tracingService;
        public TimestampedTracingService setTracingService
        {
            set => _tracingService = value;
        }
        //private ITracingService _tracingService;
        //public ITracingService setTracingService
        //{
        //    set => _tracingService = value;
        //}
        private IOrganizationService _service;
        public IOrganizationService setService
        {
            set => _service = value;
        }
        private string _method;
        public string setMethod
        {
            get => _method;
            set => _method = value;
        }

        private string _relatedEntityName;
        public string setEntityName
        {
            get => _relatedEntityName;
            set => _relatedEntityName = Truncate(value, 100);
        }

        private string Truncate(string source, int length)
        {
            if (!string.IsNullOrEmpty(source) && source.Length > length)
            {
                source = source.Substring(0, length - 4) + "...";
            }
            return source;
        }

        private Guid _relatedEntityId;
        public Guid setEntityId
        {
            get => _relatedEntityId;
            set => _relatedEntityId = value;
        }
        private string _module;
        public string setModule
        {
            get => _module;
            set => _module = value;
        }

        private bool _debug = true;
        public bool setDebug
        {
            set => _debug = value;
        }
        private bool _txnTiming = true;
        public bool setTxnTiming
        {
            set => _txnTiming = value;
        }
        private bool _granularTiming = true;
        public bool setGranularTiming
        {
            set => _granularTiming = value;
        }
        private void writeOutMessage(string message, bool debugMessage, bool granularTiming, bool TxnTiming)
        {
            try
            {
                DateTime myNow = DateTime.Now;
                message = myNow.ToString("HH:mm:ss.fff") + " - " + message;
                Entity logCreate = new Entity("mcs_log");
                logCreate["mcs_name"] = _module.Length > 100 ? _module.Substring(0, 100) : _module;
                //logCreate["new_timestamp"] = myNow.ToString("yyyy/MM/dd HH:mm:ss.fff");              
                logCreate["mcs_errormessage"] = message.Length > 20000 ? message.Substring(0, 19999) + "*" : message;
                logCreate["mcs_debugmessage"] = debugMessage;
                logCreate["mcs_grantiming"] = granularTiming;
                logCreate["mcs_txntiming"] = TxnTiming;
                logCreate["mcs_entityid"] = (_relatedEntityId != Guid.Empty) ? _relatedEntityId.ToString() : string.Empty;
                logCreate["mcs_entityname"] = _relatedEntityName;
                logCreate["mcs_method"] = _method;
                logCreate["mcs_sequence"] = _sequence;
                _sequence += 1;
                _tracingService?.Trace(message);
                _service.Create(logCreate);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("Log failed to write: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Log failed to write" + ex.Message);
            }
        }
        public void WriteToFile(string message)
        {
            writeOutMessage(message, false, false, false);
        }
        public void WriteTxnTimingMessage(string message)
        {
            if (_txnTiming)
            {
                writeOutMessage(message, false, false, true);
            }
        }

        internal void WriteDebugMessage(OptionSetValue appointmentmodality)
        {
            throw new NotImplementedException();
        }

        public void WriteGranularTimingMessage(string message)
        {
            if (_granularTiming)
            {
                writeOutMessage(message, false, true, false);
            }
        }
        public void WriteDebugMessage(string message, [CallerMemberName] string caller = null)
        {
            setMethod = caller;
            if (_debug)
            {
                writeOutMessage(message, true, false, false);
            }
        }
    }

    /// <summary>
    /// An implementation of ITracingService that prefixes all traced messages with a timestamp and time deltas for diagnoising plugin performance issues.
    /// Out-of-box tracing service usage:
    ///    ITracingService trc = (ITracingService) serviceProvider.GetService(typeof(ITracingService));
    /// TimestampedTracingService usage:
    ///    ITracingService trc = new TimestampedTracingService(serviceProvider);
    /// </summary>
    public class TimestampedTracingService : ITracingService
    {
        /// <summary>
        /// Create a new TimestampedTracingService relying on Xrm services from the IServiceProvider.
        /// </summary>
        /// <param name="serviceProvider">The IServiceProvider passed into a plugin's Execute() method.</param>
        public TimestampedTracingService(IServiceProvider serviceProvider)
        {
            var utcNow = DateTime.UtcNow;

            // Get the initial timestamp from the IExecutionContext
            var context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));
            var initialTimestamp = context.OperationCreatedOn;

            // Ensure the inititalTimestamp is not in the future (since servers may not be exactly in sync)
            if (initialTimestamp > utcNow)
            {
                initialTimestamp = utcNow;
            }

            // Set private members
            _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            _firstTraceTime = _previousTraceTime = initialTimestamp;

            // Trace a starting message
            Trace("TimestampedTracingService initialized.");
        }

        public TimestampedTracingService(CodeActivityContext executionContext, IWorkflowContext context)
        {
            var utcNow = DateTime.UtcNow;

            // Get the initial timestamp from the IExecutionContext
            var initialTimestamp = context.OperationCreatedOn;

            // Ensure the inititalTimestamp is not in the future (since servers may not be exactly in sync)
            if (initialTimestamp > utcNow)
            {
                initialTimestamp = utcNow;
            }

            // Set private members
            _tracingService = executionContext.GetExtension<ITracingService>();
            _firstTraceTime = _previousTraceTime = initialTimestamp;

            // Trace a starting message
            Trace("TimestampedTracingService initialized.");
        }

        #region ITracingService support
        /// <summary>
        /// Trace a formatted message prefixed with UTC timestamp, overall duration and delta since last trace
        /// </summary>
        public void Trace(string format, params object[] args)
        {
            var utcNow = DateTime.UtcNow;

            _tracingService.Trace(
                "[{0:O} - @{1:N0}ms (+{2:N0}ms)] - {3}",
                utcNow,
                (utcNow - _firstTraceTime).TotalMilliseconds,
                (utcNow - _previousTraceTime).TotalMilliseconds,
                string.Format(format, args)
            );

            _previousTraceTime = utcNow;
        }
        /// <summary>
        /// Trace a formatted message prefixed with UTC timestamp, overall duration and delta since last trace
        /// </summary>
        public void Trace(string message)
        {
            var utcNow = DateTime.UtcNow;

            _tracingService.Trace(
                "[{0:O} - @{1:N0}ms (+{2:N0}ms)] - {3}",
                utcNow,
                (utcNow - _firstTraceTime).TotalMilliseconds,
                (utcNow - _previousTraceTime).TotalMilliseconds,
                message
            );

            _previousTraceTime = utcNow;
        }
        #endregion

        // Base ITracingService
        private ITracingService _tracingService;

        // DateTime fields used in calculating deltas
        private DateTime _firstTraceTime;
        private DateTime _previousTraceTime;
    }
}
