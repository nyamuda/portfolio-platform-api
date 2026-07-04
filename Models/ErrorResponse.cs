namespace PortfolioPlatform.Api.Models;

/// <summary>
/// Represents a standard error body returned by API endpoints.
/// </summary>
/// <remarks>
/// Controllers use this type so validation, authorization, not-found, conflict, and unexpected errors
/// are returned with a consistent JSON shape.
/// </remarks>
public class ErrorResponse
{
    /// <summary>
    /// Short client-facing message that explains what went wrong.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Optional technical or diagnostic detail. This should be used carefully in production responses.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Creates an error response for a known failure.
    /// </summary>
    public static ErrorResponse Create(string message, string? details = null) =>
        new() { Message = message, Details = details };

    /// <summary>
    /// Creates a generic server-error response for unexpected failures.
    /// </summary>
    public static ErrorResponse Unexpected(string? details = null) =>
        new()
        {
            Message = "The server encountered an unexpected issue. Please try again later.",
            Details = details
        };

    /// <summary>
    /// Creates a standard forbidden response.
    /// </summary>
    public static ErrorResponse Forbidden(string? details = null) =>
        new() { Message = "You do not have permission to access this resource.", Details = details };
}
