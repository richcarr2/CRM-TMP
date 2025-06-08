using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace MCS.ApplicationInsights
{
    [DataContract]
    public class AppInsightsLogData
    {
        public AppInsightsLogData()
        {
            CustomDimensions = new Dictionary<string, string>();
            CustomMetrics = new Dictionary<string, double>();
        }

        [DataMember]
        public string InstrumentationKey { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string OperationName { get; set; }

        [DataMember]
        public string OperationSyntheticSource { get; set; }

        [DataMember]
        public string CorrelationId { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public int SeverityLevel { get; set; }

        [DataMember]
        public Dictionary<string, string> CustomDimensions { get; set; }

        [DataMember]
        public Dictionary<string, double> CustomMetrics { get; set; }

        [DataMember]
        public AIException Exception { get; set; }

        [DataMember]
        public AiLogType CustomLogType { get; set; }

        [DataMember]
        public string HttpStatusCode { get; set; }
    }

    [DataContract]
    public enum AiLogType
    {
        Unknown = 0,
        Trace = 1,
        Event = 2,
        Request = 3,
        Exception = 4,
        Availability = 5
    }
}
