using System.Runtime.Serialization;

namespace Ec.VideoVisit.Messages
{
    [DataContract()]
    public class EcTmpGetLoanedDevicesResponse
    {
        [DataMember(EmitDefaultValue = false, Name = "data")]
        public EcTmpLoandedDevice[] Devices { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "links")]
        public EcTmpLoanedDeviceLink Links { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HttpStatusCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ExceptionOccured { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ExceptionMessage { get; set; }
    }

    [DataContract()]
    public class EcTmpLoandedDevice
    {
        [DataMember(EmitDefaultValue = true, Name = "type")]
        public EcTmpLoanedDeviceType Type { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "id")]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "attributes")]
        public EcTmpLoanedDeviceAttributes Attributes { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "links")]
        public EcTmpLoanedDeviceLink Links { get; set; }
    }

    [DataContract()]
    public class EcTmpLoanedDeviceAttributes
    {
        [DataMember(EmitDefaultValue = false, Name = "hcpcs")]
        public string HCPCS { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "deviceName")]
        public string DeviceName { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "vendor")]
        public string Vendor { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "deviceType")]
        public string DeviceType { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "serialNumber")]
        public string SerialNumber { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "facilityId")]
        public string FacilityId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "orderedBy")]
        public string OrderedBy { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "orderedDateTime")]
        public string OrderedDateTime { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "requestedDateTime")]
        public string RequestedDateTime { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "billDateTime")]
        public string BillDateTime { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "orderingFacilityId")]
        public string OrderingFacilityId { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "icn")]
        public string ICN { get; set; }
    }

    [DataContract()]
    public class EcTmpLoanedDeviceType {
        [DataMember(EmitDefaultValue = false, Name = "type")]
        public string Type { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "description")]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "enum")]
        public EcTmpDeviceType DeviceType { get; set; }
    }

    [DataContract()]
    public class EcTmpLoanedDeviceLink
    {
        [DataMember(EmitDefaultValue = false, IsRequired = true, Name = "self")]
        public string Url { get; set; }
    }

    public enum EcTmpDeviceType
    {
        DEVICE = 0
    }
}
