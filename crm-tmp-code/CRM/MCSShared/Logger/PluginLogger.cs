using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

using Microsoft.Crm.Sdk.Messages;

using Microsoft.Xrm.Sdk.PluginTelemetry;
using MCSUtilities2011;

namespace MCS.ApplicationInsights
{
    public class PluginLogger
    {
        TimestampedTracingService TracingService;
        MCSLogger Logger;
        
        public StringBuilder AITraceLog { get; set; }
        public PluginLogger(TimestampedTracingService tracingService)
        {
            TracingService = tracingService;
            AITraceLog = new StringBuilder();
            //useAppInsights = useApplicationInsights;
           // useTracing = userPluginTracing;
        }

        public PluginLogger(TimestampedTracingService tracingService, MCSLogger logger)
        {
            TracingService = tracingService;
            AITraceLog = new StringBuilder();
            Logger = logger;
            //useAppInsights = useApplicationInsights;
           // useTracing = userPluginTracing;
        }

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
            //if (Timer != null)
            //    message = $"{Timer.ElapsedMilliseconds}ms: {message}";
            //else
            //    message = $"{message}; Warning: No Timer Found";
            message = $"{level.ToString()}: {message}";

            if (Logger != null) Logger.WriteDebugMessage(message);
            else TracingService.Trace(message);
            if (writeTraceToAI)
                AITraceLog.AppendLine(message);
        }


    }
}
