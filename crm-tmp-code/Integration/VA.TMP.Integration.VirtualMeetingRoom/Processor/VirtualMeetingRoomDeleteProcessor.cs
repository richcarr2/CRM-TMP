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
using VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Delete;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.Processor
{
    /// <summary>
    /// Virtual Meeting Room Processor.
    /// </summary>
    public class VirtualMeetingRoomDeleteProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public VirtualMeetingRoomDeleteProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">VirtualMeetingRoomDeleteRequestMessage.</param>
        /// <returns>VirtualMeetingRoomDeleteResponseMessage.</returns>
        public VirtualMeetingRoomDeleteResponseMessage Execute(VirtualMeetingRoomDeleteRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, SerializeDeleteVmrStep>("SerializeDeleteVmrStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, ValidateDeleteXmlStep>("ValidateDeleteXmlStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, DeleteMeetingRoomRequestStep>("DeleteMeetingRoomRequestStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<VirtualMeetingRoomDeleteStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new VirtualMeetingRoomDeleteStateObject(request))
            {
                new Pipeline<VirtualMeetingRoomDeleteStateObject>()
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("SerializeDeleteVmrStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("ValidateDeleteXmlStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("DeleteMeetingRoomRequestStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<VirtualMeetingRoomDeleteStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.VirtualMeetingRoomDeleteResponseMessage;
            }
        }
    }
}