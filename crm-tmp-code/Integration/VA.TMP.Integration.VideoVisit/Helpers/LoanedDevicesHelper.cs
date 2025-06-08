using System.Collections.Generic;
using System.Linq;
using Ec.VideoVisit.Messages;
using VA.TMP.Integration.Messages.VideoVisit;

namespace VA.TMP.Integration.VideoVisit.Helpers
{
    public class LoanedDevicesHelper
    {
        public static VideoVisitGetLoanedDevicesResponseMessage ConvertLoanedDevices(EcTmpGetLoanedDevicesResponse from, log4net.ILog _logger)
        {
            var respMessage = new VideoVisitGetLoanedDevicesResponseMessage();
            var devices = new List<VideoVisitLoanedDevice>();

            _logger.Debug($"Device Count: {from.Devices.Length}");

            from.Devices.ToList().ForEach(d =>
            {
                var device = new VideoVisitLoanedDevice();

                if(d.Attributes != null)
                {
                    device.Attributes = new VideoVisitLoanedDeviceAttributes
                    {
                        BillDateTime = d.Attributes.BillDateTime,
                        DeviceName = d.Attributes.DeviceName,
                        DeviceType = d.Attributes.DeviceType,
                        FacilityId = d.Attributes.FacilityId,
                        HCPCS = d.Attributes.HCPCS,
                        ICN = d.Attributes.ICN,
                        Manufacturer = d.Attributes.Manufacturer,
                        OrderedBy = d.Attributes.OrderedBy,
                        OrderedDateTime = d.Attributes.OrderedDateTime,
                        OrderingFacilityId = d.Attributes.OrderingFacilityId,
                        RequestedDateTime = d.Attributes.RequestedDateTime,
                        SerialNumber = d.Attributes.SerialNumber,
                        Vendor = d.Attributes.Vendor
                    };
                }

                if (!string.IsNullOrEmpty(d.Id)) device.Id = d.Id;

                if(d.Links != null)
                {
                    device.Links = new VideoVisitLoanedDeviceLink
                    {
                        Url = d.Links.Url
                    };
                }

                if (d.Type != null)
                {
                    device.Type = new VideoVisitLoanedDeviceType
                    {
                        Description = d.Type.Description,
                        DeviceType = (VideoVisitDeviceType)(int)d.Type.DeviceType,
                        Type = d.Type.Type
                    };
                }

                devices.Add(device);
            });

            respMessage.Devices = devices.ToArray();
            respMessage.Links = new VideoVisitLoanedDeviceLink
            {
                Url = from.Links.Url
            };

            return respMessage;
        }
    }
}
