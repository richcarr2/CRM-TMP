using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancel;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.HealthShare.Processor
{
    /// <summary>
    /// HealthShare Make and Cancel Processor.
    /// </summary>
    public class MakeCancelProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MakeCancelProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">TmpHealthShareMakeAndCancelAppointmentRequestMessage.</param>
        /// <returns>TmpHealthShareMakeAndCancelAppointmentResponseMessage.</returns>
        public TmpHealthShareMakeAndCancelAppointmentResponseMessage Execute(TmpHealthShareMakeAndCancelAppointmentRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelStateObject>, SerializeAppointmentStep>("SerializeAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelStateObject>, MapAppointmentStep>("MapAppointmentStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelStateObject>, CreateAndSaveEntitiesStep>("CreateAndSaveEntitiesStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<MakeCancelStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new MakeCancelStateObject(request))
            {
                new Pipeline<MakeCancelStateObject>()
                    .Register(container.Resolve<IFilter<MakeCancelStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<MakeCancelStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<MakeCancelStateObject>>("SerializeAppointmentStep"))
                    .Register(container.Resolve<IFilter<MakeCancelStateObject>>("MapAppointmentStep"))
                    .Register(container.Resolve<IFilter<MakeCancelStateObject>>("CreateAndSaveEntitiesStep"))
                    .Register(container.Resolve<IFilter<MakeCancelStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<MakeCancelStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.ResponseMessage;
            }
        }
    }
}