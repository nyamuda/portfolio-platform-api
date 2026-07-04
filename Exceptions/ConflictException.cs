namespace PortfolioPlatform.Api.Exceptions;

/// <summary>
/// Represents a request that conflicts with the current state of a resource.
/// </summary>
/// <remarks>
/// Typical examples include duplicate usernames, duplicate emails, or attempting to create
/// a profile slug that is already used by another account.
/// </remarks>
public class ConflictException(string message) : Exception(message) { }
