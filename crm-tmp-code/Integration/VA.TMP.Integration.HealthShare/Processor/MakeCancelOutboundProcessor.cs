using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.HealthShare.Processor
{
    /// <summary>
    /// HealthShare Make and Cancel Outbound Processor.
    /// </summary>
    public class MakeCancelOutboundProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MakeCancelOutboundProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">TmpHealthShareMakeCancelOutboundRequestMessage.</param>
        /// <returns>TmpHealthShareMakeCancelOutboundResponseMessage.</returns>
        public TmpHealthShareMakeCancelOutboundResponseMessage Execute(TmpHealthShareMakeCancelOutboundRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, GetIntegrationSettingsStep>("GetIntegrationSettingsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, SerializeRequestStep>("SerializeRequestStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, GetServiceAppointmentStep>("GetServiceAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, GetAppointmentTypeStep>("GetAppointmentTypeStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, GetAppointmentStep>("GetAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, GetFacilitiesStep>("GetFacilitiesStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, GetClinicsStep>("GetClinicsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, GetVistaIntegrationResultStep>("GetVistaIntegrationResultStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, MapAppointmentStep>("MapAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, SendAppointmentToEcStep>("SendAppointmentToEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelOutboundStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new MakeCancelOutboundStateObject(request))
            {
                new Pipeline<MakeCancelOutboundStateObject>()
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("GetIntegrationSettingsStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("SerializeRequestStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("GetServiceAppointmentStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("GetAppointmentTypeStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("GetAppointmentStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("GetFacilitiesStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("GetClinicsStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("GetVistaIntegrationResultStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("MapAppointmentStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("SendAppointmentToEcStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<MakeCancelOutboundStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.ResponseMessage;
            }
        }
    }
}