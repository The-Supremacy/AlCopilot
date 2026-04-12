namespace AlCopilot.Shared.Errors;

public abstract class ApiException : Exception
{
    protected ApiException(int statusCode, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public int StatusCode { get; }
    public string Title { get; }
}

public sealed class NotFoundException(string message)
    : ApiException(404, "Not Found", message);

public sealed class ConflictException(string message)
    : ApiException(409, "Conflict", message);

public sealed class ValidationException(string message)
    : ApiException(400, "Invalid Request", message);

public sealed class InvalidStateException(string message)
    : ApiException(400, "Invalid State", message);
