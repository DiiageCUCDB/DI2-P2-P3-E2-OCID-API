using OCID.Application.Interfaces;
using OCID.Application.Services;

namespace OCID.Api.Extensions;

public static class AppServicesExtensions
{
    /// <summary>
    /// Ajoute tous les services de l'application.
    /// </summary>
    /// <param name="services">Collection de services à compléter.</param>
    /// <returns>La même collection, pour chaîner les appels.</returns>
    public static IServiceCollection AddAppServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddScoped<ILogService, LogService>();
        return services;
    }
}