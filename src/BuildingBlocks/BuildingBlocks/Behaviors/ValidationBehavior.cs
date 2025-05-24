
using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;
using System.Windows.Input;

namespace BuildingBlocks.Behaviors
{
    //validators here in the primary constructor will contain all the validations that have been created by the command request if implements the AbstractValidator which implements the interface IValidator
    public class ValidationBehavior<TRequest, TResponse> (IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(validators.Select(x => x.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.Where(x => x.Errors.Any()).SelectMany(x => x.Errors).ToList();
            if(failures.Any())
            {
                throw new ValidationException(failures);
            }
            return await next();
        }
    }
}
