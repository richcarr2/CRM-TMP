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
using VA.TMP.Integration.VideoVisit.PipelineSteps.GetLoanedDevices;
using VA.TMP.Integration.VideoVisit.StateObject;

namespace VA.TMP.Integration.VideoVisit.Processor
{
    public class VideoVisitGetLoanedDevicesProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VideoVisitGetLoanedDevicesProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">VideoVisitCreateRequestMessage.</param>
        /// <returns>VideoVisitCreateResponseMessage.</returns>
        public VideoVisitGetLoanedDevicesResponseMessage Execute(VideoVisitGetLoanedDevicesRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitGetLoanedDevicesStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitGetLoanedDevicesStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitGetLoanedDevicesStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitGetLoanedDevicesStateObject>, GetSamlTokenStep>("GetSamlTokenStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitGetLoanedDevicesStateObject>, PostToVideoVisitServiceStep>("PostToVideoVisitServiceStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitGetLoanedDevicesStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VideoVisitGetLoanedDevicesStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new VideoVisitGetLoanedDevicesStateObject(request))
            {
                new Pipeline<VideoVisitGetLoanedDevicesStateObject>()
                    .Register(container.Resolve<IFilter<VideoVisitGetLoanedDevicesStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<VideoVisitGetLoanedDevicesStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<VideoVisitGetLoanedDevicesStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<VideoVisitGetLoanedDevicesStateObject>>("GetSamlTokenStep"))
                    .Register(container.Resolve<IFilter<VideoVisitGetLoanedDevicesStateObject>>("PostToVideoVisitServiceStep"))
                    .Register(container.Resolve<IFilter<VideoVisitGetLoanedDevicesStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<VideoVisitGetLoanedDevicesStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.VideoVisitGetLoanedDevicesResponseMessage;
            }
        }
    }
}
