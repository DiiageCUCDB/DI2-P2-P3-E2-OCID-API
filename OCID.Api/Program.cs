using Microsoft.EntityFrameworkCore;
using OCID.Api.Extensions;
using Microsoft.OpenApi;
using OCID.Infrastructure.Data;
using Scalar.AspNetCore;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.SnakeCaseUpper));
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "OCID API",
            Version = "v1",
        });
        options.CustomSchemaIds(type => type.FullName);

        // Commentaires XML (///) des projets Api, Application et Domain affichés dans Swagger
        foreach (var assembly in new[] { "OCID.Api", "OCID.Application", "OCID.Domain" })
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly}.xml");
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        }
    });

    // Extensions
    builder.Services.AddDatabase(builder.Configuration, builder.Environment);
    builder.Services.AddRedisClient(builder.Configuration);
    builder.Services.AddInfraServices(builder.Configuration);
    builder.Services.AddAppServices(builder.Configuration);
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create/update the schema first, in every environment
        await context.Database.MigrateAsync();

        if (app.Environment.IsDevelopment() || args.Contains("--seed"))
            await DbInitializer.SeedAsync(context);
    }

    // Docs (non-prod only)
    var apiDocs = builder.Configuration["ApiDocs"] ?? "Swagger";
    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();

        app.MapScalarApiReference();

        app.MapGet("/", () => Results.Redirect(
            apiDocs.Equals("Swagger", StringComparison.OrdinalIgnoreCase) ? "/swagger" : "/scalar/v1"
        ));
    }

    app.MapHealthChecks("/health");

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex}");
}

/// <summary>
/// Point d'entrée de l'API OCID (hébergement minimal ASP.NET Core).
/// </summary>
/// <remarks>
/// La déclaration <c>partial</c> rend la classe <c>Program</c> visible des projets de test d'intégration (<c>WebApplicationFactory&lt;Program&gt;</c>).
/// <para>Configuration du pipeline, dans l'ordre :</para>
/// <list type="number">
/// <item><description>Contrôleurs avec sérialisation JSON des énumérations en majuscules snake_case (<c>HOST</c>, <c>CRITICAL</c>...).</description></item>
/// <item><description>OpenAPI, Swagger (<c>/swagger</c>) et Scalar (<c>/scalar/v1</c>), exposés hors production uniquement ; la racine <c>/</c> redirige vers celui que désigne le réglage <c>ApiDocs</c> (<c>Swagger</c> par défaut).</description></item>
/// <item><description>Base de données (<see cref="OCID.Api.Extensions.DatabaseExtensions"/>) et services applicatifs (<see cref="OCID.Api.Extensions.AppServicesExtensions"/>).</description></item>
/// <item><description>Sonde de santé sur <c>/health</c>, redirection HTTPS, authentification et autorisation, puis routage des contrôleurs.</description></item>
/// </list>
/// Toute exception levée au démarrage est écrite sur la console au lieu de faire échouer le processus avec une trace.
/// Les migrations EF Core ne sont pas appliquées automatiquement : le bloc correspondant est en commentaire.
/// </remarks>
public partial class Program;
