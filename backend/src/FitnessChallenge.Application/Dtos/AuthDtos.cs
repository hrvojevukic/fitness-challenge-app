namespace FitnessChallenge.Application.Dtos;

public class LoginRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserResponse User { get; set; } = new();
}
