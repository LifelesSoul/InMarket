using ProductService.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace UserService.Domain.Entities;

[ExcludeFromCodeCoverage]
public class UserProfile
{
    [Key]
    [ForeignKey("User")]
    public required Guid UserId { get; set; }

    public string? AvatarUrl { get; set; }

    [MaxLength(DbConstants.BiographyTextLength)]
    public string? Biography { get; set; }

    [Column(TypeName = DbConstants.FloatType)]
    public double RatingScore { get; set; } = default;

    public virtual required User User { get; set; }
}
