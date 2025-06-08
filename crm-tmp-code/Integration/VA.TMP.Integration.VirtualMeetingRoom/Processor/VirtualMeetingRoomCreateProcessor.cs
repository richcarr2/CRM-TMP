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
using VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.Processor
{
    /// <summary>
    /// Virtual Meeting Room Processor.
    /// </summary>
    public class VirtualMeetingRoomCreateProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VirtualMeetingRoomCreateProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">VirtualMeetingRoomCreateRequestMessage.</param>
        /// <returns>VirtualMeetingRoomCreateResponseMessage.</returns>
        public VirtualMeetingRoomCreateResponseMessage Execute(VirtualMeetingRoomCreateRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, GetServiceAppointmentStep>("GetServiceAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, SetMeetingRoomNameStep>("SetMeetingRoomNameStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, GeneratePinsStep>("GeneratePinsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, SetMiscDataStep>("SetMiscDataStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, SerializeVmrStep>("SerializeVmrStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, ValidateXmlStep>("ValidateXmlStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, CreateMeetingRoomeRequestStep>("CreateMeetingRoomeRequestStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, CreateUrlsStep>("CreateUrlsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, CompareUrlsStep>("CompareUrlsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomCreateStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new VirtualMeetingRoomCreateStateObject(request))
            {
                new Pipeline<VirtualMeetingRoomCreateStateObject>()
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("GetServiceAppointmentStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("SetMeetingRoomNameStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("GeneratePinsStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("SetMiscDataStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("SerializeVmrStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("ValidateXmlStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("CreateMeetingRoomeRequestStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("CreateUrlsStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("CompareUrlsStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomCreateStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.VirtualMeetingRoomCreateResponseMessage;
            }
        }
    }
}