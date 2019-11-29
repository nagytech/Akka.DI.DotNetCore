using System;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Akka.DI.Core;
using Akka.DI.DotnetCore;
using Akka.Routing;
using Microsoft.Extensions.DependencyInjection;
using static example.Program;

namespace example
{
    public class TypedWorker : ReceiveActor
    {
        public TypedWorker()
        {
            Receive<TypedActorMessage>(msg => {
                Context.System.Log.Info(msg.ToString());
            });
        }
    }

    class Program
    {
        public class TypedActorMessage
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        static void Main(string[] args)
        {
            IServiceCollection collection = new ServiceCollection();
            collection.AddTransient<TypedWorker>();

            var config = ConfigurationFactory.ParseString(
@"akka.actor.deployment {
  /workers {
    router = consistent-hashing-group
    routees.paths = [""/user/Worker1"", ""/user/Worker2""]
  }
}");
        
            // Create the ActorSystem
            using (var system = ActorSystem.Create("MySystem"))
            {
                var provider = collection.BuildAkkaServiceProvider();
                IDependencyResolver resolver = new DotNetCoreDependencyResolver(provider, system);

                // Register the actors with the system
                system.ActorOf(system.DI().Props<TypedWorker>(), "Worker1");
                system.ActorOf(system.DI().Props<TypedWorker>(), "Worker2");

                // Create the router
                var workers = new[] { "/user/Worker1", "/user/Worker2" };
                IActorRef router = system.ActorOf(Props.Empty.WithRouter(new RoundRobinGroup(workers)));

                // Create the message to send
                TypedActorMessage message = new TypedActorMessage
                {
                    Id = 1,
                    Name = Guid.NewGuid().ToString()
                };

                // Send the message to the router
                router.Tell(new Broadcast(message));

                system.WhenTerminated.Wait();
            }
        }
    }
}
