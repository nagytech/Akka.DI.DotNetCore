using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.DI.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Akka.DI.DotnetCore
{
    public class DotNetCoreDependencyResolver : IDependencyResolver
    {
        AkkaServiceProvider Container { get; }
        ActorSystem System { get; }
        ConcurrentDictionary<string, Type> TypeCache { get; }
        ConditionalWeakTable<ActorBase, IServiceScope> References { get; }

        public DotNetCoreDependencyResolver(AkkaServiceProvider container, ActorSystem system) 
        {
            if (container == null) throw new ArgumentNullException("container");
            if (system == null) throw new ArgumentNullException("system");

            Container = container;
            System = system;

            TypeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            References = new ConditionalWeakTable<ActorBase, IServiceScope>();

            System.AddDependencyResolver(this);
        }

        public Props Create<TActor>() where TActor : ActorBase
        {
            return Create(typeof(TActor));
        }

        public Props Create(Type actorType)
        {
            return System.GetExtension<DIExt>().Props(actorType);
        }

        public Func<ActorBase> CreateActorFactory(Type actorType)
        {
            return () => {
                var scope = Container.CreateScope();
                var actor = (ActorBase)scope.ServiceProvider.GetService(actorType);
                References.Add(actor, scope);
                return actor;
            };
        }

        public Type GetType(string actorName)
        {
            TypeCache.TryAdd(actorName, actorName.GetTypeValue() ?? Container.GetType(actorName) );
            return TypeCache[actorName];
        }

        public void Release(ActorBase actor)
        {
            IServiceScope scope;

            if (References.TryGetValue(actor, out scope))
            {
                scope.Dispose();
                References.Remove(actor);
            }
        }
    }
}
