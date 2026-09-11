namespace BoundlessEnterprises.Application.Common.Exceptions;

/// <summary>
/// Thrown when an authenticated caller lacks access to the requested company or
/// resource. Maps to HTTP 403.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "You do not have access to this resource.")
        : base(message) { }
}
