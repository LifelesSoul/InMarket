namespace ProductService.Domain.Entities
{
    public class Category
    {
        public required int Id { get; set; }
        public required string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}