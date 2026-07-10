namespace PortfolioPlatform.Api.Enums.Offerings;

/// <summary>
/// Featured-state filters supported by owner offering list endpoints.
/// </summary>
public enum OfferingFeaturedFilter
{
    /// <summary>
    /// Return featured and regular offerings.
    /// </summary>
    All,

    /// <summary>
    /// Return only offerings marked as featured.
    /// </summary>
    Featured,

    /// <summary>
    /// Return offerings that are not currently featured.
    /// </summary>
    Regular
}
