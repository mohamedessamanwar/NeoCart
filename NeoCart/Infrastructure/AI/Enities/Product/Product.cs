namespace NeoCart.Infrastructure.AI.Enities.Product
{
    using Pgvector;

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class ProductDescriptionChunk
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string Text { get; set; } = "";
        public Vector Embedding { get; set; } // Pgvector.Vector (not System.Numerics)
    }

}
