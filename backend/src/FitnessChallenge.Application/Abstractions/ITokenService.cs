using FitnessChallenge.Domain.Entities;

namespace FitnessChallenge.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(User user);
}
