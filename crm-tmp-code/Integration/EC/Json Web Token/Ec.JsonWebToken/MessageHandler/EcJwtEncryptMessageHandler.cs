using Ec.JsonWebToken.Messages;
using Ec.JsonWebToken.Processors;
using Ec.JsonWebToken.Services.Rest;
using log4net;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Core;

namespace Ec.JsonWebToken.MessageHandler
{
    public class EcJwtEncryptMessageHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public EcJwtEncryptMessageHandler(ILog logger, Settings settings, IKeyVaultCert keyVaultCert, IServiceFactory serviceFactory)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
            _serviceFactory = serviceFactory;
        }

        public EcJwtEncryptTokenResponse HandleRequestResponse(EcJwtEncryptTokenRequest message)
        {
            var processor = new EcJwtEncryptProcessor(_logger, _settings, _keyVaultCert, _serviceFactory);
            return processor.Execute(message);
        }
    }
}