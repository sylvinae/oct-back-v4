using System.Reflection;

namespace API.Utils;

public static class ServiceRegistrator
{
    public static IServiceCollection AddServicesByConvention(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var types = assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Select(type => new
            {
                Implementation = type,
                Interface = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}"),
            })
            .Where(t => t.Interface != null);

        foreach (var typePair in types)
        {
            if (typePair.Interface != null)
                services.AddScoped(typePair.Interface, typePair.Implementation);
        }

        return services;
    }
}
