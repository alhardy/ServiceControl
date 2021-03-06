namespace ServiceControl.AcceptanceTesting
{
    using System;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.Logging;

    public class StaticLoggerFactory : ILoggerFactory
    {
        public StaticLoggerFactory(ScenarioContext currentContext)
        {
            CurrentContext = currentContext;
        }

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            return new StaticContextAppender();
        }

        public static ScenarioContext CurrentContext;
    }
}