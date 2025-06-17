namespace ECommerce.Application.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message ?? "Forbidden")
    {
    }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 