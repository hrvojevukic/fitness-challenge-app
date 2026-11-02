using System.Security.Claims;
using FitnessChallenge.Application.Exceptions;

namespace FitnessChallenge.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new AuthenticationFailedException("The bearer token does not identify a user.");
        }

        return userId;
    }
}
