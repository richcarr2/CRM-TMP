using System.Diagnostics;
using System.Linq;
using Ec.VirtualMeetingRoom.Messages;
using Ec.VirtualMeetingRoom.Services;
using Ec.VirtualMeetingRoom.Services.Vyopta.UcManager;
using log4net;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;

namespace Ec.VirtualMeetingRoom.Processors
{
    public class ScheduleMeetingProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;
        private readonly IServiceFactory _serviceFactory;

        private string VmrUrl => _settings.Items.First(x => x.Key == "VmrUrl").Value;

        public ScheduleMeetingProcessor(ILog logger, Settings settings, IKeyVaultCert keyVaultCert, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
            _serviceFactory = serviceFactory;
        }

        public EcVyoptaSMScheduleMeetingResponse Execute(EcVyoptaSMScheduleMeetingRequest request)
        {
            _logger.Info("Starting VMR Create");

            if (request == null)
            {
                _logger.Error("Error calling VRM Schedule Meeting. The request cannot be null");
                return new EcVyoptaSMScheduleMeetingResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VMR Schedule Meeting. The request cannot be null" };
            }

            Stopwatch thisTimer = Stopwatch.StartNew();

            var scheduleMeetingRequestData = new ScheduleMeetingRequest();

            if (!string.IsNullOrEmpty(request.mcs_EncounterId)) scheduleMeetingRequestData.EncounterId = request.mcs_EncounterId;
            scheduleMeetingRequestData.EndTime = request.mcs_EndTime;
            if (!string.IsNullOrEmpty(request.mcs_GuestName)) scheduleMeetingRequestData.GuestName = request.mcs_GuestName;
            if (!string.IsNullOrEmpty(request.mcs_GuestPin)) scheduleMeetingRequestData.GuestPin = request.mcs_GuestPin;
            if (!string.IsNullOrEmpty(request.mcs_HostName)) scheduleMeetingRequestData.HostName = request.mcs_HostName;
            if (!string.IsNullOrEmpty(request.mcs_HostPin)) scheduleMeetingRequestData.HostPin = request.mcs_HostPin;
            if (!string.IsNullOrEmpty(request.mcs_MeetingRoomName)) scheduleMeetingRequestData.MeetingRoomName = request.mcs_MeetingRoomName;
            if (!string.IsNullOrEmpty(request.mcs_MiscData)) scheduleMeetingRequestData.MiscData = request.mcs_MiscData;
            scheduleMeetingRequestData.StartTime = request.mcs_StartTime;

            _logger.Info($"Posting to {VmrUrl}");
            _logger.Info($"Payload: {Serialization.XmlSerializeInstance(scheduleMeetingRequestData)}");

            var service = _serviceFactory.GetVirtualMeetingRoomWebServiceReference();
            var scheduleMeetingResponse = service.ScheduleMeeting(scheduleMeetingRequestData);

            thisTimer.Stop();
            _logger.Info($"Calling Vyopta Schedule took {thisTimer.ElapsedMilliseconds} ms");

            if (scheduleMeetingResponse == null)
            {
                _logger.Error("Error calling VRM Schedule Meeting. The response returned was Vyopta was null");
                return new EcVyoptaSMScheduleMeetingResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VMR Schedule Meeting. The response returned was Vyopta was null" };
            }

            _logger.Info("Ending VMR Create");

            return new EcVyoptaSMScheduleMeetingResponse
            {
                mcs_DialingAlias = scheduleMeetingResponse.DialingAlias,
                mcs_EncounterId = scheduleMeetingResponse.EncounterId,
                mcs_MiscData = scheduleMeetingResponse.MiscData,
                ExceptionOccured = false
            };
        }
    }
}
