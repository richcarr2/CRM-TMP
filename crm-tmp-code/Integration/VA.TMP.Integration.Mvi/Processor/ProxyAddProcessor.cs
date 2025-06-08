using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.PipelineSteps.ProxyAdd;
using VA.TMP.Integration.Mvi.Services;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.Mvi.Processor
{
    /// <summary>
    /// Proxy Add Processor.
    /// </summary>
    public class ProxyAddProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ProxyAddProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">ProxyAddRequestMessage.</param>
        /// <returns>ProxyAddResponseMessage.</returns>
        public ProxyAddResponseMessage Execute(ProxyAddRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IProxyAddService, ProxyAddService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, GetServiceAppointmentStep>("GetServiceAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, GetVeteranStep>("GetVeteranStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, GetVeteranIdentifiersStep>("GetVeteranIdentifiersStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, CreatePatientVeteranIdentifierStep>("CreatePatientVeteranIdentifierStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, MapPatientVeteranIdentifierToProxyAddRequestEcStep>("MapPatientVeteranIdentifierToProxyAddRequestEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, SendPatientVeteranIdentifierToEcStep>("SendPatientVeteranIdentifierToEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, CreateProviderVeteranIdentifierStep>("CreateProviderVeteranIdentifierStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, MapProviderVeteranIdentifierToProxyAddRequestEcStep>("MapProviderVeteranIdentifierToProxyAddRequestEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, SendProviderVeteranIdentifierToEcStep>("SendProviderVeteranIdentifierToEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ProxyAddStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new ProxyAddStateObject(request))
            {
                new Pipeline<ProxyAddStateObject>()
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("GetServiceAppointmentStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("GetVeteranStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("GetVeteranIdentifiersStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("CreatePatientVeteranIdentifierStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("MapPatientVeteranIdentifierToProxyAddRequestEcStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("SendPatientVeteranIdentifierToEcStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("CreateProviderVeteranIdentifierStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("MapProviderVeteranIdentifierToProxyAddRequestEcStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("SendProviderVeteranIdentifierToEcStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<ProxyAddStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.ProxyAddResponseMessage;
            }
        }
    }
}