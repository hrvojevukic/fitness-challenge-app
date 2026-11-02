using FitnessChallenge.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FitnessChallenge.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException appException)
        {
            return false;
        }

        var status = GetStatusCode(appException);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = appException.Title,
                Status = status,
                Detail = appException.Message,
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }

    private static int GetStatusCode(AppException exception)
    {
        return exception switch
        {
            InvalidRequestException => StatusCodes.Status400BadRequest,
            AuthenticationFailedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
