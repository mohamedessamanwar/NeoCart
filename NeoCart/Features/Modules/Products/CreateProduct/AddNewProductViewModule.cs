namespace NeoCart.Features.Modules.Products.CreateProduct
{
    public class AddNewProductViewModule
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public static CreateProductDto ToDto(AddNewProductViewModule module)
        {
            return new CreateProductDto
            {
                Name = module.Name,
                Description = module.Description,
                Price = module.Price,
                Stock = module.Stock
            };
        }
    }
}
