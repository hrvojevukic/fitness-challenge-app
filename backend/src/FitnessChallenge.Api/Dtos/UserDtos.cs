namespace FitnessChallenge.Api.Dtos;

public class RegisterUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class RegisterUserResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
