using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.PipelineSteps.GetConsults;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.HealthShare.Processor
{
    /// <summary>
    /// HealthShare Get Consults Processor.
    /// </summary>
    public class GetConsultsProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetConsultsProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">TmpHealthShareGetConsultsRequest.</param>
        /// <returns>TmpHealthShareGetConsultsResponse.</returns>
        public TmpHealthShareGetConsultsResponse Execute(TmpHealthShareGetConsultsRequest request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, SerializeConsultsStep>("SerializeConsultsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, GenerateUniqueIdStep>("GenerateUniqueIdStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, MapConsultsRequestStep>("MapConsultsRequestStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, SendToEcStep>("SendToEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, MapConsultsResponseStep>("MapConsultsResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetConsultsStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new GetConsultsStateObject(request))
            {
                new Pipeline<GetConsultsStateObject>()
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("SerializeConsultsStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("GenerateUniqueIdStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("MapConsultsRequestStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("SendToEcStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("MapConsultsResponseStep"))
                    .Register(container.Resolve<IFilter<GetConsultsStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.ResponseMessage;
            }
        }
    }
}