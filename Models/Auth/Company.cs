namespace PortfolioPlatform.Api.Models.Auth;

/// <summary>
/// Strongly typed company/app settings used by services that need brand or support details.
/// </summary>
/// <remarks>
/// Keeping the app name and support email in configuration makes it easier to rename or reuse the API later.
/// </remarks>
public class Company
{
    /// <summary>
    /// Public name of the application or company shown in emails and user-facing messages.
    /// </summary>
    public string Name { get; set; } = "Portfolio Platform";

    /// <summary>
    /// Support or contact email address used by application services.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
