using MediatR;

namespace NeoCart.Commen.BaseUseCase
{
    public abstract class BaseRequest<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        protected readonly IMediator _mediator;

        protected BaseRequest(UseCaseParam useCaseParam)
        {
            _mediator = useCaseParam._mediator;
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
