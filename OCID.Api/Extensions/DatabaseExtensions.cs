using OCID.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OCID.Api.Extensions;

/// <summary>
/// Configure l'accès à la base de données SQL Server via Entity Framework Core.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Enregistre <c>AppDbContext</c> avec la chaîne de connexion <c>DefaultConnection</c>.
    /// </summary>
    /// <param name="services">Collection de services à compléter.</param>
    /// <param name="config">Configuration contenant la section <c>ConnectionStrings</c>.</param>
    /// <param name="env">Environnement d'hébergement (non utilisé pour l'instant).</param>
    /// <returns>La même collection, pour chaîner les appels.</returns>
    /// <exception cref="InvalidOperationException">
    /// La chaîne de connexion est absente ou vide ; elle est lue au moment où le contexte est résolu, pas au démarrage.
    /// Elle se définit par la variable d'environnement <c>ConnectionStrings__DefaultConnection</c>.
    /// </exception>
    /// <remarks>
    /// L'avertissement <c>PendingModelChangesWarning</c> est ignoré : EF Core ne lève plus d'exception quand le modèle diffère de la dernière migration.
    /// </remarks>
    public static IServiceCollection AddDatabase(this IServiceCollection services,
        IConfiguration config, IWebHostEnvironment env)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Chaîne de connexion absente : définir la variable d'environnement " +
                    "ConnectionStrings__DefaultConnection.");
            }

            options.UseSqlServer(connectionString);

            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        return services;
    }
}