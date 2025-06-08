using System;
using System.Configuration;
using System.Linq;
using Ec.JsonWebToken.Services.Rest;
using log4net;
using Microsoft.ApplicationInsights;
using Unity;
using Unity.Injection;
using VA.TMP.Integration.Certificate;
using VA.TMP.Integration.Certificate.Interface;
using VA.TMP.Integration.Core;

namespace Ec.Jwt.Api
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            log4net.Config.XmlConfigurator.Configure();
            var log = LogManager.GetLogger("TMP JWT EC");

            var settings = new Settings(ConfigurationManager.AppSettings);

            var certificateUrl = settings.Items.First(x => x.Key == "CertificateUrl").Value;
            var appId = settings.Items.First(x => x.Key == "AppId").Value;
            var secret = settings.Items.First(x => x.Key == "Secret").Value;

            var telemetryClient = new TelemetryClient();            

            container.RegisterInstance(log);
            container.RegisterInstance(settings);
            container.RegisterInstance(telemetryClient);
            container.RegisterType<IKeyVaultCert, KeyVaultCert>(new InjectionConstructor(new[] { certificateUrl, appId, secret }));
            container.RegisterType<IServiceFactory, ServiceFactory>();
        }
    }
}
