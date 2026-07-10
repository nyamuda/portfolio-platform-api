namespace PortfolioPlatform.Api.Enums.Offerings;

/// <summary>
/// Publication state filters supported by owner offering list endpoints.
/// </summary>
public enum OfferingStatus
{
    /// <summary>
    /// Return published and draft offerings.
    /// </summary>
    All,

    /// <summary>
    /// Return only offerings visible on the public profile.
    /// </summary>
    Published,

    /// <summary>
    /// Return only offerings still being edited privately.
    /// </summary>
    Draft
}
