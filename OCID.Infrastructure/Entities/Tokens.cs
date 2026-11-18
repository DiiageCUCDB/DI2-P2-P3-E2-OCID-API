using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OCID.Infrastructure.Entities;

[Table("Tokens")]
[Index(nameof(UserId))]
[Index(nameof(AppId))]
[Index(nameof(ExpiresAt))]
public class Tokens
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("app_id")]
    public Guid AppId { get; set; }

    [Column("issued_at")]
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public Users User { get; set; } = null!;

    [ForeignKey(nameof(AppId))]
    public Applications Application { get; set; } = null!;
}