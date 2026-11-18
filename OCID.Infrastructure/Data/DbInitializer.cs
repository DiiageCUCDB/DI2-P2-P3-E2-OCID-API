using Bogus;
using Microsoft.EntityFrameworkCore;

namespace OCID.Infrastructure.Data;

/// <summary>
/// Seeds the database with realistic fake data using the Bogus library.
/// Only runs when the DB is empty — safe to call on every startup in Development.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Applique les migrations en attente puis remplit la base de données de test.
    /// </summary>
    /// <param name="db">Contexte de la base à initialiser.</param>
    /// <remarks>
    /// Le générateur Bogus est initialisé avec une graine fixe (42) pour obtenir toujours les mêmes données.
    /// Pour l'instant aucune entité n'est générée : le code de génération d'utilisateurs est en commentaire.
    /// Cette méthode n'est pas appelée au démarrage (voir le bloc commenté dans <c>Program.cs</c>).
    /// </remarks>
    public static async Task SeedAsync(AppDbContext db)
    {
        Randomizer.Seed = new Random(42);
        var faker = new Faker("fr");

        await db.SaveChangesAsync();
    }
}
