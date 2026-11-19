using OCID.Infrastructure.Interfaces;
using OCID.Infrastructure.Repositories;

namespace OCID.Api.Extensions;

public static class InfraServicesExtensions
{
    /// <summary>
    /// Ajoute tous les services de l'infraa.
    /// </summary>
    /// <param name="services">Collection de services à compléter.</param>
    /// <returns>La même collection, pour chaîner les appels.</returns>
    public static IServiceCollection AddInfraServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddScoped<ILogRepository, LogRepository>();
        return services;
    }
}