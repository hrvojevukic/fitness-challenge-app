using FitnessChallenge.Application.Dtos;

namespace FitnessChallenge.Application.Services;

public interface IActivityValidator
{
    ActivityValidationResult Validate(IngestActivityRequest request);
}
