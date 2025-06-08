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
using VA.TMP.Integration.VideoVisit.PipelineSteps.Delete;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.Processor
{
    /// <summary>
    /// Video Visit Processor.
    /// </summary>
    public class VideoVisitDeleteProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VideoVisitDeleteProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">VideoVisitDeleteRequestMessage.</param>
        /// <returns>VideoVisitDeleteResponseMessage.</returns>
        public VideoVisitDeleteResponseMessage Execute(VideoVisitDeleteRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, GetServiceAppointmentStep>("GetServiceAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, MapAppointmentStep>("MapAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, SerializeAppointmentStep>("SerializeAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, GetSamlTokenStep>("GetSamlTokenStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, PutToVideoVisitServiceStep>("PutToVideoVisitServiceStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitDeleteStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new VideoVisitDeleteStateObject(request))
            {
                new Pipeline<VideoVisitDeleteStateObject>()
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("GetServiceAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("MapAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("SerializeAppointmentStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("GetSamlTokenStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("PutToVideoVisitServiceStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<VideoVisitDeleteStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.VideoVisitDeleteResponseMessage;
            }
        }
    }
}