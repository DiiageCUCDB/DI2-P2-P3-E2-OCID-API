using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OCID.Infrastructure.Entities;

[Table("Users")]
[Index(nameof(Email), IsUnique = true)]
public class Users
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [StringLength(256)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(512)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    // Comma-separated or space-separated list, e.g. "admin user"
    [Required]
    [StringLength(256)]
    [Column("roles")]
    public string Roles { get; set; } = "user";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Tokens> Tokens { get; set; } = new List<Tokens>();
    public ICollection<Logs> Logs { get; set; } = new List<Logs>();
}