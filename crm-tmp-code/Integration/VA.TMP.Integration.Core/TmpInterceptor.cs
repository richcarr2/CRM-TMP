using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.PolicyInjection.Pipeline;

namespace VA.TMP.Integration.Core
{
    public class TmpInterceptor : IInterceptionBehavior
    {
        private readonly ILog _logger;

        public TmpInterceptor(ILog logger)
        {
            _logger = logger;
        }

        public bool WillExecute => true;

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var methodName = input.Target.ToString();

            var stopWatch = Stopwatch.StartNew();

            _logger.Info($"Calling {methodName}.");

            var message = getNext()(input, getNext);

            _logger.Info($"Exiting {methodName}. Took {stopWatch.ElapsedMilliseconds} ms");

            stopWatch.Stop();

            return message;
        }
    }
}
