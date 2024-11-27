using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace API.Utils;

public static class ServiceRegistrator
{
    public static void AddCustomServices(this IServiceCollection services, Assembly assembly)
    {
        var serviceTypes = assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(i => i.Name.EndsWith("Service")));

        foreach (var serviceType in serviceTypes)
        {
            var interfaceType = serviceType
                .GetInterfaces()
                .First(i => i.Name == $"I{serviceType.Name}");

            services.AddScoped(interfaceType, serviceType);
        }
    }
}
