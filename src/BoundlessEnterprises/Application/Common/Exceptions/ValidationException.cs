namespace BoundlessEnterprises.Application.Common.Exceptions;

/// <summary>
/// Thrown when a command or query fails FluentValidation checks in the pipeline.
/// Carries a field-keyed dictionary of error messages for the API to surface.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}
