using NeoCart.Infrastructure.Persistence.Enities;

namespace NeoCart.Infrastructure.Persistence.Entities
{
    public class ProductImage : IBaseEntity
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; } = false;
        public Product Product { get; set; } = null!;
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

    }
}

