using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VideoVisit;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;
using VA.TMP.Integration.VideoVisit.PipelineSteps.Update;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.Processor
{
    /// <summary>
    /// Video Visit Processor.
    /// </summary>
    public class VideoVisitUpdateProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VideoVisitUpdateProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">VideoVisitUpdateRequestMessage.</param>
        /// <returns>VideoVisitUpdateResponseMessage.</returns>
        public VideoVisitUpdateResponseMessage Execute(VideoVisitUpdateRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, GetServiceAppointmentStep>("GetServiceAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, GetProvidersStep>("GetProvidersStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, MapAppointmentStep>("MapAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, SerializeAppointmentStep>("SerializeAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, ValidateBusinessRulesStep>("ValidateBusinessRulesStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, GetSamlTokenStep>("GetSamlTokenStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, PutToVideoVisitServiceStep>("PutToVideoVisitServiceStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitUpdateStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new VideoVisitUpdateStateObject(request))
            {
                new Pipeline<VideoVisitUpdateStateObject>()
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("GetServiceAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("GetProvidersStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("MapAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("SerializeAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("ValidateBusinessRulesStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("GetSamlTokenStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("PutToVideoVisitServiceStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<VideoVisitUpdateStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.VideoVisitUpdateResponseMessage;
            }
        }
    }
}