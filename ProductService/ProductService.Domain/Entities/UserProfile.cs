using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities;
public class UserProfile
{
    [Key]
    [ForeignKey("User")]
    public required Guid UserId { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(1000)]
    public string? Biography { get; set; }

    [Column(TypeName = "float")]
    public double RatingScore { get; set; } = default;

    public required virtual User User { get; set; }
}