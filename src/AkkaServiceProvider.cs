using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Akka.DI.DotnetCore
{
    public class AkkaServiceProvider : IServiceProvider
    {
        Dictionary<string, IGrouping<string, Type>> TypeLookup { get; }
        IServiceProvider Provider { get; }

        public AkkaServiceProvider(ServiceProvider provider, Dictionary<string, IGrouping<string, Type>> typeLookup)
        {
            Provider = provider;
            TypeLookup = typeLookup;
        }

        public Type GetType(string actorName) 
        {
            return TypeLookup[actorName].FirstOrDefault();
        }

        public object GetService(Type serviceType)
        {
            return Provider.GetService(serviceType);
        }
    }
}
