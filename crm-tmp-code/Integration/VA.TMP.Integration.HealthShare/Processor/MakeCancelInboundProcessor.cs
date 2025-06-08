using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.HealthShare.Processor
{
    /// <summary>
    /// HealthShare Make and Cancel Inbound Processor.
    /// </summary>
    public class MakeCancelInboundProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MakeCancelInboundProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">TmpHealthShareMakeCancelInboundRequestMessage.</param>
        /// <returns>TmpHealthShareMakeCancelInboundResponseMessage.</returns>
        public TmpHealthShareMakeCancelInboundResponseMessage Execute(TmpHealthShareMakeCancelInboundRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, SerializeInboundRequestStep>("SerializeInboundRequestStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, GetIntegrationResultStep>("GetIntegrationResultStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, GetOutboundRequestMessageStep>("GetOutboundRequestMessageStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, GetOutboundEcRequestMessageStep>("GetOutboundEcRequestMessageStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, GetPatientIdStep>("GetPatientIdStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, GetServiceAppointmentStep>("GetServiceAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, GetAppointmentTypeStep>("GetAppointmentTypeStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, GetAppointmentStep>("GetAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, SaveEntitiesStep>("SaveEntitiesStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelInboundStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new MakeCancelInboundStateObject(request))
            {
                new Pipeline<MakeCancelInboundStateObject>()
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("SerializeInboundRequestStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("GetIntegrationResultStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("GetOutboundRequestMessageStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("GetOutboundEcRequestMessageStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("GetPatientIdStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("GetServiceAppointmentStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("GetAppointmentTypeStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("GetAppointmentStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("SaveEntitiesStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<MakeCancelInboundStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.ResponseMessage;
            }
        }
    }
}