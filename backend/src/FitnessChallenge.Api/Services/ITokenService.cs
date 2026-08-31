using FitnessChallenge.Api.Models;

namespace FitnessChallenge.Api.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
