using NeoCart.Infrastructure.Persistence.Enities;

namespace NeoCart.Infrastructure.Persistence.Entities
{
    public class Product : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public byte[] RowVersion { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new HashSet<ProductImage>();


    }
}
