using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.VirtualMeetingRoom;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;
using VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.OnDemand;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.Processor
{
    /// <summary>
    /// VMR On Demand Processor.
    /// </summary>
    public class VmrOnDemandCreateProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VmrOnDemandCreateProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">VmrOnDemandCreateRequestMessage.</param>
        /// <returns>VmrOnDemandCreateResponseMessage.</returns>
        public VmrOnDemandCreateResponseMessage Execute(VmrOnDemandCreateRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, GetVodStep>("GetVodStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, SetMeetingRoomNameStep>("SetMeetingRoomNameStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, GeneratePinsStep>("GeneratePinsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, SetMiscDataStep>("SetMiscDataStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, SerializeVmrStep>("SerializeVmrStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, ValidateXmlStep>("ValidateXmlStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, CreateMeetingRoomeRequestStep>("CreateMeetingRoomeRequestStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, CreateUrlsStep>("CreateUrlsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VmrOnDemandCreateStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new VmrOnDemandCreateStateObject(request))
            {
                new Pipeline<VmrOnDemandCreateStateObject>()
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("GetVodStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("SetMeetingRoomNameStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("GeneratePinsStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("SetMiscDataStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("SerializeVmrStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("ValidateXmlStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("CreateMeetingRoomeRequestStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("CreateUrlsStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<VmrOnDemandCreateStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.VmrOnDemandCreateResponseMessage;
            }
        }
    }
}