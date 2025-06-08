using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.PipelineSteps.UpdateClinic;
using VA.TMP.Integration.HealthShare.StateObject;
using VA.TMP.Integration.Messages.HealthShare;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.HealthShare.Processor
{
    /// <summary>
    /// HealthShare Update Clinic Processor.
    /// </summary>
    public class UpdateClinicProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public UpdateClinicProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">TmpHealthShareUpdateClinicRequestMessage.</param>
        /// <returns>TmpHealthShareUpdateClinicResponseMessage.</returns>
        public TmpHealthShareUpdateClinicResponseMessage Execute(TmpHealthShareUpdateClinicRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<UpdateClinicStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<UpdateClinicStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<UpdateClinicStateObject>, SerializeClinicStep>("SerializeClinicStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<UpdateClinicStateObject>, MapClinicStep>("MapClinicStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<UpdateClinicStateObject>, CreateAndSaveEntitiesStep>("CreateAndSaveEntitiesStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<UpdateClinicStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<UpdateClinicStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new UpdateClinicStateObject(request))
            {
                new Pipeline<UpdateClinicStateObject>()
                    .Register(container.Resolve<IFilter<UpdateClinicStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<UpdateClinicStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<UpdateClinicStateObject>>("SerializeClinicStep"))
                    .Register(container.Resolve<IFilter<UpdateClinicStateObject>>("MapClinicStep"))
                    .Register(container.Resolve<IFilter<UpdateClinicStateObject>>("CreateAndSaveEntitiesStep"))
                    .Register(container.Resolve<IFilter<UpdateClinicStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<UpdateClinicStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.ResponseMessage;
            }
        }
    }
}