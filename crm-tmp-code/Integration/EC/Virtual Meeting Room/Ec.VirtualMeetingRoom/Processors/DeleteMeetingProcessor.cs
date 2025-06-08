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
    public class DeleteMeetingProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;
        private readonly IServiceFactory _serviceFactory;

        private string VmrUrl => _settings.Items.First(x => x.Key == "VmrUrl").Value;

        public DeleteMeetingProcessor(ILog logger, Settings settings, IKeyVaultCert keyVaultCert, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
            _serviceFactory = serviceFactory;
        }

        public EcVirtualDeleteMeetingResponse Execute(EcVirtualDeleteMeetingRequest request)
        {
            _logger.Info("Starting VMR Delete");

            if (request == null)
            {
                _logger.Error("Error calling VRM Delete Meeting. The request cannot be null");
                return new EcVirtualDeleteMeetingResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VMR Delete Meeting. The request cannot be null" };
            }

            var thisTimer = Stopwatch.StartNew();

            var deleteMeetingRequestData = new DeleteMeetingRequest();

            if (!string.IsNullOrEmpty(request.mcs_EncounterId)) deleteMeetingRequestData.EncounterId = request.mcs_EncounterId;
            if (!string.IsNullOrEmpty(request.mcs_MiscData)) deleteMeetingRequestData.MiscData = request.mcs_MiscData;

            _logger.Info($"Posting to {VmrUrl}");
            _logger.Info($"Payload: {Serialization.XmlSerializeInstance(deleteMeetingRequestData)}");

            var service = _serviceFactory.GetVirtualMeetingRoomWebServiceReference();
            var deleteMeetingResponse = service.DeleteMeeting(deleteMeetingRequestData);

            thisTimer.Stop();
            _logger.Info($"Calling Vyopta Delete took {thisTimer.ElapsedMilliseconds} ms");

            if (deleteMeetingResponse == null)
            {
                _logger.Error("Error calling VRM Delete Meeting. The response returned was Vyopta was null");
                return new EcVirtualDeleteMeetingResponse { ExceptionOccured = true, ExceptionMessage = "Error calling VMR Delete Meeting. The response returned was Vyopta was null" };
            }

            _logger.Info("Ending VMR Create");

            return new EcVirtualDeleteMeetingResponse
            {
                mcs_MiscData = deleteMeetingResponse.MiscData,
                ExceptionOccured = false
            };
        }
    }
}
