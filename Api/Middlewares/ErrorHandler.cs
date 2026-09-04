using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Api.Middlewares;

public class ErrorHandler(ILogger<ErrorHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        var error = MapException(exception);
        var logLevel = error.StatusCode >= StatusCodes.Status500InternalServerError
            ? LogLevel.Error
            : LogLevel.Warning;

        logger.Log(logLevel, exception, "Request failed with status code {StatusCode}.", error.StatusCode);

        httpContext.Response.StatusCode = error.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                status = error.StatusCode,
                title = error.Title,
                detail = error.Detail,
                traceId = httpContext.TraceIdentifier
            },
            cancellationToken);

        return true;
    }

    private static ErrorResponse MapException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => new ErrorResponse(StatusCodes.Status404NotFound, "Resource not found.",
                "The requested resource was not found."),
            ArgumentException argumentException => new ErrorResponse(StatusCodes.Status400BadRequest,
                "Invalid request.",
                argumentException.Message),
            ValidationException validationException => new ErrorResponse(StatusCodes.Status400BadRequest,
                "Validation failed.",
                validationException.Message),
            JsonException => new ErrorResponse(StatusCodes.Status400BadRequest, "Invalid JSON.",
                "The request body contains invalid JSON."),
            UnauthorizedAccessException => new ErrorResponse(StatusCodes.Status401Unauthorized, "Unauthorized.",
                "Authentication is required to access this resource."),
            TimeoutException => new ErrorResponse(StatusCodes.Status503ServiceUnavailable, "Service unavailable.",
                "The operation took too long to complete."),
            NpgsqlException => new ErrorResponse(StatusCodes.Status503ServiceUnavailable, "Database unavailable.",
                "The database is temporarily unavailable."),
            HttpRequestException => new ErrorResponse(StatusCodes.Status503ServiceUnavailable, "Service unavailable.",
                "A required service is temporarily unavailable."),
            DbUpdateConcurrencyException => new ErrorResponse(StatusCodes.Status409Conflict, "Concurrency conflict.",
                "The resource was changed by another request."),
            DbUpdateException databaseException => MapDatabaseException(databaseException),
            _ => new ErrorResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred.",
                "The server could not complete the request.")
        };
    }

    private static ErrorResponse MapDatabaseException(DbUpdateException exception)
    {
        if (exception.InnerException is PostgresException postgresException)
        {
            return postgresException.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation => new ErrorResponse(StatusCodes.Status409Conflict, "Conflict.",
                    "A resource with the same value already exists."),
                PostgresErrorCodes.ForeignKeyViolation => new ErrorResponse(StatusCodes.Status409Conflict, "Conflict.",
                    "The operation conflicts with a related resource."),
                PostgresErrorCodes.NotNullViolation => new ErrorResponse(StatusCodes.Status400BadRequest,
                    "Invalid request.",
                    "A required value is missing."),
                PostgresErrorCodes.CheckViolation => new ErrorResponse(StatusCodes.Status400BadRequest,
                    "Invalid request.",
                    "The data violates a database rule."),
                _ => new ErrorResponse(StatusCodes.Status500InternalServerError, "Database error.",
                    "The server could not persist the data.")
            };
        }

        return new ErrorResponse(StatusCodes.Status500InternalServerError, "Database error.",
            "The server could not persist the data.");
    }

    private sealed record ErrorResponse(int StatusCode, string Title, string Detail);
}