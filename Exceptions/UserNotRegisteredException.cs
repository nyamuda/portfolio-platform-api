namespace PortfolioPlatform.Api.Exceptions;

/// <summary>
/// Thrown when an OAuth sign-in is attempted for an email that has not registered yet.
/// </summary>
public class UserNotRegisteredException(string message) : Exception(message);
