using MediatR;

namespace NeoCart.Commen.BaseController
{
    public class ControllerParam
    {
        public readonly IMediator _mediator;


        public ControllerParam(IMediator mediator)
        {
            _mediator = mediator;

        }

    }
}
