namespace FitnessChallenge.Application.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string title, string message) : base(message)
    {
        Title = title;
    }

    public string Title { get; }
}

public class InvalidRequestException : AppException
{
    public InvalidRequestException(string message) : base("Invalid request", message)
    {
    }
}

public class AuthenticationFailedException : AppException
{
    public AuthenticationFailedException(string message) : base("Login failed", message)
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base("Forbidden", message)
    {
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base("Not found", message)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base("Conflict", message)
    {
    }
}
