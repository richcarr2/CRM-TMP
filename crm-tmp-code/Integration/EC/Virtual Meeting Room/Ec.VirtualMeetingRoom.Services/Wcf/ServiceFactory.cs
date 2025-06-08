using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using Ec.VirtualMeetingRoom.Services.Vyopta.UcManager;
using log4net;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Core;

namespace Ec.VirtualMeetingRoom.Services
{
    public class ServiceFactory : IServiceFactory
    {
        private const string CacheKey = "_VMR";

        private readonly ILog _logger;
        private readonly Settings _settings;
        private readonly IKeyVaultCert _keyVaultCert;

        private string VmrUrl => _settings.Items.First(x => x.Key == "VmrUrl").Value;

        private string RequiresClientCertificate => _settings.Items.First(x => x.Key == "RequiresClientCertificate").Value;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="logger"></param>
        public ServiceFactory(ILog logger, Settings settings, IKeyVaultCert keyVaultCert)
        {
            _logger = logger;
            _settings = settings;
            _keyVaultCert = keyVaultCert;
        }

        public IUcManagerService GetVirtualMeetingRoomWebServiceReference()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 214783647
            };
            var endpoint = new EndpointAddress(VmrUrl);
            var client = new UcManagerServiceClient(binding, endpoint);

            var useCertificate = Convert.ToBoolean(RequiresClientCertificate);

            if (useCertificate)
            {
                var certificate = _keyVaultCert.GetKeyVaultCertificate(CacheKey).ConfigureAwait(false).GetAwaiter().GetResult();
                client.ClientCredentials.ClientCertificate.Certificate = certificate;
            }

            return client;
        }
    }
}
