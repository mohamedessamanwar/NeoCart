using MediatR;

namespace NeoCart.Commen.BaseUseCase
{
    public class UseCaseParam
    {
        public readonly IMediator _mediator;
        public UseCaseParam(IMediator mediator)
        {
            _mediator = mediator;
        }

    }
}
