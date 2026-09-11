using BoundlessEnterprises.Api.Endpoints;

namespace BoundlessEnterprises.Api.Extensions;

/// <summary>
/// Discovers and maps every <see cref="IEndpointModule"/> in the Api assembly so
/// new capability modules are wired up simply by implementing the interface.
/// </summary>
public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder app)
    {
        var moduleType = typeof(IEndpointModule);
        var modules = typeof(EndpointExtensions).Assembly
            .GetTypes()
            .Where(t => moduleType.IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .Select(Activator.CreateInstance)
            .Cast<IEndpointModule>();

        foreach (var module in modules)
            module.MapEndpoints(app);

        return app;
    }
}
