using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OCID.Infrastructure.Entities;

[Table("Logs")]
[Index(nameof(UserId))]
[Index(nameof(Timestamp))]
public class Logs
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    [Column("event")]
    public string Event { get; set; } = string.Empty;

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public Users? User { get; set; }
}