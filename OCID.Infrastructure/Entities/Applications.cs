using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OCID.Infrastructure.Entities;

[Table("Applications")]
[Index(nameof(ClientId), IsUnique = true)]
public class Applications
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(150)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("client_id")]
    public string ClientId { get; set; } = string.Empty;

    // Store a hash of the secret
    [Required]
    [StringLength(512)]
    [Column("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    // Space-separated, style: "dossier.read"
    [Required]
    [StringLength(1000)]
    [Column("scopes")]
    public string Scopes { get; set; } = string.Empty;

    public ICollection<Tokens> Tokens { get; set; } = new List<Tokens>();
}