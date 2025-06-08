using System;
using System.Runtime.Serialization;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Messages.VideoVisit
{
    [DataContract()]
    [Serializable]
    public class VideoVisitGetLoanedDevicesResponseMessage : TmpBaseResponseMessage
    {
        [DataMember(EmitDefaultValue = false)]
        public VideoVisitLoanedDevice[] Devices { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public VideoVisitLoanedDeviceLink Links { get; set; }
    }

    [DataContract()]
    [Serializable]
    public class VideoVisitLoanedDevice
    {
        [DataMember(EmitDefaultValue = false)]
        public VideoVisitLoanedDeviceType Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public VideoVisitLoanedDeviceAttributes Attributes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public VideoVisitLoanedDeviceLink Links { get; set; }
    }

    [DataContract()]
    [Serializable]
    public class VideoVisitLoanedDeviceAttributes
    {
        [DataMember(EmitDefaultValue = false)]
        public string HCPCS { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DeviceName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Vendor { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Manufacturer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DeviceType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SerialNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FacilityId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string OrderedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string OrderedDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string OrderingFacilityId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RequestedDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string BillDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ICN { get; set; }
    }

    [DataContract()]
    [Serializable]
    public class VideoVisitLoanedDeviceType
    {
        [DataMember(EmitDefaultValue = false)]
        public string Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "enum")]
        public VideoVisitDeviceType DeviceType { get; set; }
    }

    [DataContract()]
    [Serializable]
    public class VideoVisitLoanedDeviceLink
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "self")]
        public string Url { get; set; }
    }

    public enum VideoVisitDeviceType
    {
        DEVICE = 0
    }
}