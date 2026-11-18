using Microsoft.EntityFrameworkCore;
using OCID.Infrastructure.Entities;

namespace OCID.Infrastructure.Data;

/// <summary>
/// Contexte Entity Framework Core de la base SQL Server de l'API.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Initialise le contexte avec les options configurées par <c>AddDatabase</c>.</summary>
    /// <param name="options">Options du contexte (fournisseur SQL Server, chaîne de connexion).</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Users> Users => Set<Users>();
    public DbSet<Applications> Applications => Set<Applications>();
    public DbSet<Tokens> Tokens => Set<Tokens>();
    public DbSet<Logs> Logs => Set<Logs>();

    /// <summary>
    /// Décrit le modèle de données : clés, colonnes obligatoires, conversion des énumérations en texte et relation incident → notes.
    /// </summary>
    /// <param name="mb">Constructeur de modèle fourni par EF Core.</param>
    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
    }
}