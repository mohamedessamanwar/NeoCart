using Microsoft.AspNetCore.Mvc;
using NeoCart.Commen.BaseController;
using NeoCart.Commen.ResponseViewModel;
using NeoCart.Features.Modules.Products.CreateProduct.Commands;

namespace NeoCart.Features.Modules.Products.CreateProduct
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddProductController : BaseController
    {
        public AddProductController(ControllerParam controllerParam) : base(controllerParam)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] AddNewProductViewModule model)
        {
            var command = new CreateProductCommand
            {
                Product = AddNewProductViewModule.ToDto(model)
            };

            var response = await _mediator.Send(command);

            if (response.Succeeded)
            {
                return Ok(ApiResponse<int>.Success(response.Data, response.StatusCode, response.Message));
            }

            return Ok(ApiResponse<int>.Fail(response.Message, response.StatusCode, response.Errors));
        }
    }
}
