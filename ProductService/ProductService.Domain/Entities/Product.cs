using ProductService.Domain.Enums;
using UserService.Domain.Entities;


namespace ProductService.Domain.Entities
{
    public class Product
    {
        public required Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public required ProductStatus Status { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public int Priority { get; set; } = 0;


        public required Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public required Guid SellerId { get; set; }
        public virtual User? Seller { get; set; }


        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}