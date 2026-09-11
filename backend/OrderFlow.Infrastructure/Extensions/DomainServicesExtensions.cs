using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Domain.Common;

namespace OrderFlow.Infrastructure.Extensions;

public static class DomainServicesExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, Func<Type, bool>? filter = null)
    {
        var domainServiceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.FullName?.Contains("Domain", StringComparison.OrdinalIgnoreCase) ?? false)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttributes(typeof(DomainServiceAttribute), false).Length != 0)
            .Where(filter ?? (_ => true));

        foreach (var domainServiceType in domainServiceTypes)
        {
            services.AddTransient(domainServiceType);
        }

        return services;
    }
}
