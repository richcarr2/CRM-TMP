using log4net;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using VA.TMP.Integration.Context;
using VA.TMP.Integration.Context.Interface;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.PipelineSteps.PersonSearch;
using VA.TMP.Integration.Mvi.StateObject;
using VA.TMP.Integration.Rest;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Token;
using VA.TMP.Integration.Token.Interface;

namespace VA.TMP.Integration.Mvi.Processor
{
    /// <summary>
    /// Person Search Processor.
    /// </summary>
    public class PersonSearchProcessor
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public PersonSearchProcessor(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        ///  Execute the pipeline steps.
        /// </summary>
        /// <param name="request">PersonSearchRequestMessage.</param>
        /// <returns>PersonSearchRequestMessage.</returns>
        public PersonSearchResponseMessage Execute(PersonSearchRequestMessage request)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container
                .RegisterInstance(_logger)
                .RegisterInstance(_settings)
                .RegisterType<ITokenService, TokenService>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<ITmpContext, TmpContext>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IServicePost, ServicePost>(new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<PersonSearchStateObject>, StartProcessingStep>("StartProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<PersonSearchStateObject>, ConnectToCrmStep>("ConnectToCrmStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<PersonSearchStateObject>, MapPersonSearchToAttendedSearchStep>("MapPersonSearchToAttendedSearchStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<PersonSearchStateObject>, SendPersonSearchToEcStep>("SendPersonSearchToEcStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<PersonSearchStateObject>, CreateResponseStep>("CreateResponseStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>())
                .RegisterType<IFilter<PersonSearchStateObject>, StopProcessingStep>("StopProcessingStep", new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<TmpInterceptor>());

            using (var state = new PersonSearchStateObject(request))
            {
                new Pipeline<PersonSearchStateObject>()
                    .Register(container.Resolve<IFilter<PersonSearchStateObject>>("StartProcessingStep"))
                    .Register(container.Resolve<IFilter<PersonSearchStateObject>>("ConnectToCrmStep"))
                    .Register(container.Resolve<IFilter<PersonSearchStateObject>>("MapPersonSearchToAttendedSearchStep"))
                    .Register(container.Resolve<IFilter<PersonSearchStateObject>>("SendPersonSearchToEcStep"))
                    .Register(container.Resolve<IFilter<PersonSearchStateObject>>("CreateResponseStep"))
                    .Register(container.Resolve<IFilter<PersonSearchStateObject>>("StopProcessingStep"))
                    .Execute(state);

                return state.PersonSearchResponseMessage;
            }
        }
    }
}