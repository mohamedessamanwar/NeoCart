using MediatR;
using NeoCart.Commen.BaseUseCase;
using NeoCart.Commen.Helper;
using NeoCart.Commen.UseCaseResponseViewModel;
using NeoCart.Infrastructure.Persistence.Repositories;

namespace NeoCart.Features.Modules.Products.CreateProduct.Commands
{
    public class CreateProductCommand : IRequest<ResponseGenericResponse<int>>
    {
        public CreateProductDto Product { get; set; } = new CreateProductDto();
    }
    public class CreateProductHandler : BaseRequest<CreateProductCommand, ResponseGenericResponse<int>>
    {
        private readonly IGenericRepository<Infrastructure.Persistence.Entities.Product> _productRepository;
        public CreateProductHandler(UseCaseParam useCaseParam, IGenericRepository<Infrastructure.Persistence.Entities.Product> productRepository) : base(useCaseParam)
        {
            _productRepository = productRepository;
        }

        public override async Task<ResponseGenericResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Infrastructure.Persistence.Entities.Product
            {
                Name = request.Product.Name,
                Description = request.Product.Description,
                Price = request.Product.Price,
            };
            await _productRepository.AddAsync(product);
            return ResponseGenericResponse<int>.Success(product.Id, "Product created successfully", StatusCode.Created);
        }
    }
}
