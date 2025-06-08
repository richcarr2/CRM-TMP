using Ec.VirtualMeetingRoom.Messages;
using Ec.VirtualMeetingRoom.Processors;
using Ec.VirtualMeetingRoom.Services;
using log4net;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Core;

namespace Ec.VirtualMeetingRoom.MessageHandler
{
    public class DeleteMeetingMessageHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;
        private readonly IServiceFactory _serviceFactory;

        public DeleteMeetingMessageHandler(ILog logger, Settings settings, IKeyVaultCert keyVaultCert, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
            _serviceFactory = serviceFactory;
        }

        public EcVirtualDeleteMeetingResponse HandleRequestResponse(EcVirtualDeleteMeetingRequest message)
        {
            var processor = new DeleteMeetingProcessor(_logger, _settings, _keyVaultCert, _serviceFactory);
            return processor.Execute(message);
        }
    }
}