namespace ERP_Government.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found. Mapped to 404 by ProblemDetailsExceptionHandler.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }

    public NotFoundException(string name, object key, Exception innerException)
        : base($"Entity \"{name}\" ({key}) was not found.", innerException)
    {
    }
}
