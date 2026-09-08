using Atm.Application.Accounts;
using Atm.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Atm.Api.ErrorHandling;

/// <summary>
/// Translates domain and application exceptions into RFC 7807 <c>ProblemDetails</c>
/// responses. Anything it does not recognise is left for the framework's default 500.
/// </summary>
public sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int Status, string Title)? mapped = exception switch
        {
            AccountNotFoundException => (StatusCodes.Status404NotFound, "Account not found"),
            InsufficientFundsException => (StatusCodes.Status409Conflict, "Insufficient funds"),
            InvalidAmountException => (StatusCodes.Status422UnprocessableEntity, "Invalid amount"),
            InvalidTransferException => (StatusCodes.Status422UnprocessableEntity, "Invalid transfer"),
            DomainException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => null,
        };

        if (mapped is null)
        {
            return false;
        }

        (int status, string title) = mapped.Value;
        httpContext.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
            },
        });
    }
}
