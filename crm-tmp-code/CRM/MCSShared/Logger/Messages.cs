using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;

namespace MCS.ApplicationInsights
{
    public enum TraceSeverity
    {
        Verbose,
        Information,
        Warning,
        Error,
        Critical
    }

    public enum ExceptionSeverity
    {
        Verbose,
        Information,
        Warning,
        Error,
        Critical
    }

    public enum Functionality
    {
        NotSet,
        Sign0877,
        DocumentUpload,
        SSOe,
        SSOeFedmine,
        Reverification,
        InititalDunsSearch,
        SimplifiedRenewal,
        BusinessTypeChange,
        GetIntegratedData,
        SubmitVerification,
        RefreshSamData,
        Reapply
    }



    [DataContract]
    [KnownType(typeof(Trace))]
    [KnownType(typeof(Event))]
    [KnownType(typeof(AIException))]
    [KnownType(typeof(Request))]
    public class BaseData
    {
        [DataMember(Name = "ver")]
        public int Version { get; set; } = 2;

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "properties")]
        public string CustomDimensionsReplacementString { get;
            set; }

        /// <summary>
        /// Intentionally leaving off DataMember tags so as not to be serialized.  This is where the data input goes that will then replace CustomDimensionsReplacementString
        /// </summary>
        public Dictionary<string,string> CustomDimensions { get; set; }

        [DataMember(Name = "exceptions")]
        public List<AIException> Exceptions { get; set; }

        [DataMember(Name = "measurements")]
        public string CustomMeasuresReplacementString { get;
            set; }

        /// <summary>
        /// Intentionally leaving off DataMember tags so as not to be serialized.  This is where the data input goes that will then replace CustomMeasuresReplacementString
        /// </summary>
        public Dictionary<string, double> CustomMeasurements { get; set; }

        public BaseData()
        {
            CustomDimensionsReplacementString = "****THIS IS A CUSTOMDIMENSION STRING TO BE REPLACED****";
            CustomMeasuresReplacementString = "****THIS IS A CUSTOMMEASUREMENTS STRING TO BE REPLACED****";
        }

        public BaseData(string name)
        {
            CustomDimensionsReplacementString = "****THIS IS A CUSTOMDIMENSION STRING TO BE REPLACED****";
            CustomMeasuresReplacementString = "****THIS IS A CUSTOMMEASUREMENTS STRING TO BE REPLACED****";
            Name = name;
        }
        public BaseData(string name, Dictionary<string, string> customDimensions)
        {
            CustomDimensionsReplacementString = "****THIS IS A CUSTOMDIMENSION STRING TO BE REPLACED****";
            CustomMeasuresReplacementString = "****THIS IS A CUSTOMMEASUREMENTS STRING TO BE REPLACED****";
            Name = name;
            CustomDimensions = customDimensions;
        }
    }

    [DataContract]
    public class Data
    {
        [DataMember(Name = "baseType")]
        public string BaseType { get; set; }

        [DataMember(Name = "baseData")]
        public BaseData BaseData { get; set; }
    }

    [DataContract]
    public class Trace : BaseData
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "severityLevel")]
        public TraceSeverity SeverityLevel { get; set; }

        public Trace(string message, TraceSeverity aiTraceSeverity, Dictionary<string, string> dictionary) : base()
        {
            Message = message.Length > 32768 ? message.Substring(0, 32767) : message;
            SeverityLevel = aiTraceSeverity;
            CustomDimensions = dictionary;
        }
    }

    [DataContract]
    public class Request : BaseData
    {
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string duration { get; set; }
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string responseCode { get; set; }

        public Request(string id, double durationInMs, string name, string source, bool success, Dictionary<string, string> customDimensions, Dictionary<string, double> customMeasurements, string responseCode = "") : base(name)
        {
            this.source = source;
            this.success = success;
            var durationSpan = TimeSpan.FromMilliseconds(durationInMs);
            this.duration = durationSpan.ToString(@"dd\.hh\:mm\:ss\.fffffff"); 
            this.id = id;
            if (string.IsNullOrEmpty(responseCode))
                this.responseCode = success ? HttpStatusCode.OK.ToString() : HttpStatusCode.InternalServerError.ToString();
            else
                this.responseCode = responseCode;

            CustomDimensions = customDimensions;
            CustomMeasurements = customMeasurements;
        }
    }

    [DataContract]
    public class Event : BaseData
    {
        public Event(string message, Dictionary<string, string> customDimensions, Dictionary<string, double> measurements) : base(message, customDimensions)
        {
            Name = message;
            CustomMeasurements = measurements;
        }
    }


    [DataContract]
    public class Tags
    {
        [DataMember(Name = "ai.operation.name")]
        public string OperationName { get; set; }

        [DataMember(Name = "ai.cloud.roleInstance")]
        public string RoleInstance { get; set; }

        [DataMember(Name = "ai.operation.id")]
        public string OperationId { get; set; }

        [DataMember(Name = "ai.user.authUserId")]
        public string AuthenticatedUserId { get; set; }

        [DataMember(Name = "ai.operation.parentid")]
        public string OperationParentId { get; set; }

        [DataMember(Name = "application_Version")]
        public string ApplicationVersion { get; set; }
    }

    [DataContract]
    public class Properties
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }

        [DataMember(Name = "methodName")]
        public string MethodName { get; set; }
    }

    //[DataContract]
    //public class CustomDimensions
    //{
    //    [DataMember(Name)]
    //}

    [DataContract]
    public class ParsedStack
    {
        [DataMember(Name = "level")]
        public int Level { get; set; }

        [DataMember(Name = "method")]
        public string Method { get; set; }

        [DataMember(Name = "assembly")]
        public string Assembly { get; set; }

        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "line")]
        public int Line { get; set; }
    }

    [DataContract]
    public class Metric
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "value")]
        public int Value { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "min")]
        public int Min { get; set; }

        [DataMember(Name = "max")]
        public int Max { get; set; }

        [DataMember(Name = "stdDev")]
        public int StdDev { get; set; }

        [DataMember(Name = "kind")]
        public int Kind { get; set; }
    }

    //[DataContract]
    //public class Measurementss
    //{
    //    [DataMember]
    //    public Dictionary<string, double> Measurements { get; set; }
    //}

    //[DataContract]
    //public class Dimensionss
    //{
    //    [DataMember]
    //    public Dictionary<string, string> Dimensions { get; set; }
    //}

    [DataContract]
    public class LogRequest
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "time")]
        public string Time { get; set; }

        [DataMember(Name = "iKey")]
        public string InstrumentationKey { get; set; }

        [DataMember(Name = "tags")]
        public Tags Tags { get; set; }

        [DataMember(Name = "data")]
        public Data Data { get; set; }
    }

    [DataContract]
    public class AIException
    {
        [DataMember(Name = "typeName")]
        public string TypeName { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "hasFullStack")]
        public bool HasFullStack { get; set; }

        [DataMember(Name = "stack")]
        public string Stack { get; set; }

        [DataMember(Name = "parsedStack")]
        public List<ParsedStack> ParsedStacks { get; set; }

        [DataMember(Name = "severityLevel")]
        public ExceptionSeverity SeverityLevel { get; set; }

        public AIException(Exception exception, ExceptionSeverity severity)
        {
            TypeName = exception.GetType().Name.Length > 1024
                ? exception.GetType().Name.Substring(0, 1023)
                : exception.GetType().Name;
            Message = exception.Message;
            HasFullStack = !string.IsNullOrEmpty(exception.StackTrace);
            Stack = HasFullStack ? exception.StackTrace : null;
            ParsedStacks = ExceptionHelper.GetParsedStacked(exception);
            SeverityLevel = severity;
        }
    }

    [DataContract]
    public class Response
    {
        [DataMember(Name = "itemsReceived")]
        public int ItemsReceived { get; set; }

        [DataMember(Name = "itemsAccepted")]
        public int ItemsAccepted { get; set; }

        [DataMember(Name = "errors")]
        public List<Error> Errors { get; set; }
    }

    [DataContract]
    public class Error
    {
        [DataMember(Name = "index")]
        public int Index { get; set; }

        [DataMember(Name = "statusCode")]
        public int StatusCode { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}