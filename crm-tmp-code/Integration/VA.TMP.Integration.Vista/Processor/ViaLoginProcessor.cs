using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Vista;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;
using VA.TMP.Integration.Vista.PipelineSteps.ViaLogin;
using VA.TMP.Integration.Vista.StateObject;

namespace VA.TMP.Integration.Vista.Processor
{
    public class ViaLoginProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ViaLoginProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public ViaLoginResponseMessage Execute(ViaLoginRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, MapToLoginEcRequestStep>("MapToLoginEcRequestStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, SerializeInstanceStep>("SerializeInstanceStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, SendLoginToEcStep>("SendLoginToEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<ViaLoginStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new ViaLoginStateObject(request))
            {
                new Pipeline<ViaLoginStateObject>()
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("MapToLoginEcRequestStep"))
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("SerializeInstanceStep"))
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("SendLoginToEcStep"))
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<ViaLoginStateObject>>("StopProcessingStep"))
                    .Execute(state);
                return state.LoginResponse;
            }
        }
    }
}