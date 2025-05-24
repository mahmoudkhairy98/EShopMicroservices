using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace BuildingBlocks.Exceptions.Handler
{
    public class CustomExceptionHandler (ILogger<CustomExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError($"Error Message: {exception.Message}, Time of occurence: {DateTime.UtcNow}");

            (string Detail,string Title, int StatusCode) details = exception switch
            {
                NotFoundException => (exception.Message, exception.GetType().Name, httpContext.Response.StatusCode = StatusCodes.Status404NotFound),
                InternalServerException => (exception.Message, exception.GetType().Name, httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError),
                BadRequestException => (exception.Message, exception.GetType().Name, httpContext.Response.StatusCode = StatusCodes.Status400BadRequest),
                ValidationException => (exception.Message, exception.GetType().Name, httpContext.Response.StatusCode = StatusCodes.Status400BadRequest),
                _ => (exception.Message, exception.GetType().Name, httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError)
            };

            var problemDetails = new ProblemDetails
            {
                Detail = details.Detail,
                Title = details.Title,
                Status = details.StatusCode,
                Instance = httpContext.Request.Path
            };

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
            return true;
        }
    }
}
