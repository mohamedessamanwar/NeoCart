using Microsoft.EntityFrameworkCore;
using NeoCart.Infrastructure.AI.Enities.Product;

namespace NeoCart.Infrastructure.Persistence.Contexts
{
    public class AppDbPostgresContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductDescriptionChunk> ProductDescriptionChunks => Set<ProductDescriptionChunk>();

        public AppDbPostgresContext(DbContextOptions<AppDbPostgresContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Ensure the extension is present in the model
            b.HasPostgresExtension("vector");

            b.Entity<Product>(e =>
            {
                e.ToTable("product");
                e.HasKey(x => x.Id);
            });

            b.Entity<ProductDescriptionChunk>(e =>
            {
                e.ToTable("product_description_chunk");
                e.HasKey(x => x.Id);

                // Native pgvector column with dimensions
                e.Property(x => x.Embedding)
                    .HasColumnType("vector(1536)");

                e.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // Do NOT add custom converters for Pgvector.Vector.
        // ConfigureConventions override is not needed anymore.
    }
}
