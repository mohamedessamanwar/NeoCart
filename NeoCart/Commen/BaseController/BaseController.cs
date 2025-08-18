using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace NeoCart.Commen.BaseController
{
    public class BaseController : ControllerBase
    {
        protected readonly IMediator _mediator;
        //  public UserState UserState { get; set; }

        public BaseController(ControllerParam controllerParam)
        {
            _mediator = controllerParam._mediator;
            //UserState = new UserState
            //{
            //    UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? string.Empty,
            //    UserName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty,
            //    Email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
            //    Roles = User.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList()
            //};
        }

    }
}
