using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.PipelineSteps.GetPersonIdentifiers;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.Mvi.Processor
{
    /// <summary>
    /// Get Person Identifiers Processor.
    /// </summary>
    public class GetPersonIdentifiersProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetPersonIdentifiersProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">GetPersonIdentifiersRequestMessage.</param>
        /// <returns>GetPersonIdentifiersResponseMessage.</returns>
        public GetPersonIdentifiersResponseMessage Execute(GetPersonIdentifiersRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, IsSearchNeededStep>("IsSearchNeededStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, MapGetPersonIdentifiersToGetIdsStep>("MapGetPersonIdentifiersToGetIdsStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, SendSelectedPersonRequestToEcStep>("SendSelectedPersonRequestToEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, SaveContactAndIdsIntoCrmStep>("SaveContactAndIdsIntoCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, MapCorrespondingIdsResponseToGetPersonIdentifiersResponseStep>("MapCorrespondingIdsResponseToGetPersonIdentifiersResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<GetPersonIdentifiersStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new GetPersonIdentifiersStateObject(request))
            {
                new Pipeline<GetPersonIdentifiersStateObject>()
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("IsSearchNeededStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("MapGetPersonIdentifiersToGetIdsStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("SendSelectedPersonRequestToEcStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("SaveContactAndIdsIntoCrmStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("MapCorrespondingIdsResponseToGetPersonIdentifiersResponseStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<GetPersonIdentifiersStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.GetPersonIdentifiersResponseMessage;
            }
        }
    }
}