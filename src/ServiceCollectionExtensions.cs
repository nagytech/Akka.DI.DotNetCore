using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Akka.DI.DotnetCore
{
    public static class ServiceCollectionExtensions
    {
        public static AkkaServiceProvider BuildAkkaServiceProvider(this IServiceCollection services) 
        {
            var typeLookup = services.Select(x => x.ImplementationType).GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x);
            var provider = services.BuildServiceProvider();
            return new AkkaServiceProvider(provider, typeLookup);
        }
    }
}
