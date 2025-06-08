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
using VA.TMP.Integration.VideoVisit.PipelineSteps.Create;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.Processor
{
    /// <summary>
    /// Video Visit Processor.
    /// </summary>
    public class VideoVisitCreateProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VideoVisitCreateProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">VideoVisitCreateRequestMessage.</param>
        /// <returns>VideoVisitCreateResponseMessage.</returns>
        public VideoVisitCreateResponseMessage Execute(VideoVisitCreateRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, GetServiceAppointmentStep>("GetServiceAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, GetProvidersStep>("GetProvidersStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, MapAppointmentStep>("MapAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, SerializeAppointmentStep>("SerializeAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, ValidateBusinessRulesStep>("ValidateBusinessRulesStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, GetSamlTokenStep>("GetSamlTokenStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, PostToVideoVisitServiceStep>("PostToVideoVisitServiceStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitCreateStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new VideoVisitCreateStateObject(request))
            {
                new Pipeline<VideoVisitCreateStateObject>()
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("GetServiceAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("GetProvidersStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("MapAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("SerializeAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("ValidateBusinessRulesStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("GetSamlTokenStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("PostToVideoVisitServiceStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<VideoVisitCreateStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.VideoVisitCreateResponseMessage;
            }
        }
    }
}