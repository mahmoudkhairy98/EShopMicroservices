using MediatR;

namespace BuildingBlocks.CQRS
{
    //Regular ICommand doesn't return response, Unit is like void but in MediatR
    public interface ICommand : ICommand<Unit>
    {

    }
    //This ICommand returns response because it implements IRequest<out TResponse> that has to return response
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {

    }
}
