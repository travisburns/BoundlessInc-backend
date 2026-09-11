using BoundlessEnterprises.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ValidationException = BoundlessEnterprises.Application.Common.Exceptions.ValidationException;

namespace BoundlessEnterprises.Api.Middleware;

/// <summary>
/// Translates application exceptions into RFC 7807 ProblemDetails responses so
/// endpoints and handlers can throw meaningfully without knowing about HTTP.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, title, problem) = exception switch
        {
            ValidationException v => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                (ProblemDetails)new ValidationProblemDetails(v.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                }),
            NotFoundException nf => (
                StatusCodes.Status404NotFound,
                nf.Message,
                new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = nf.Message }),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Unauthorized." }),
            ForbiddenAccessException fa => (
                StatusCodes.Status403Forbidden,
                fa.Message,
                new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = fa.Message }),
            ConflictException c => (
                StatusCodes.Status409Conflict,
                c.Message,
                new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = c.Message }),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                }),
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Path}", context.Request.Path);

        problem.Status = status;
        problem.Title = title;
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
