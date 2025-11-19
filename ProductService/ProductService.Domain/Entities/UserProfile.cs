using ProductService.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities;

public class UserProfile
{
    [Key]
    [ForeignKey("User")]
    public required Guid UserId { get; set; }

    public string? AvatarUrl { get; set; }

    [MaxLength(DbConstants.BiographyTextLength)]
    public string? Biography { get; set; }

    [Column(DbConstants.FloatType)]
    public double RatingScore { get; set; } = default;

    public required virtual User User { get; set; }
}
