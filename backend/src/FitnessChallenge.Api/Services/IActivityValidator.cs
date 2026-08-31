using FitnessChallenge.Api.Dtos;

namespace FitnessChallenge.Api.Services;

public interface IActivityValidator
{
    ActivityValidationResult Validate(IngestActivityRequest request);
}
