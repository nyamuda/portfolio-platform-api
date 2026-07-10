namespace PortfolioPlatform.Api.Enums.Offerings;

/// <summary>
/// Sort options supported by offering list endpoints.
/// </summary>
public enum OfferingSortOption
{
    /// <summary>
    /// Recently updated offerings appear first.
    /// </summary>
    Recent,

    /// <summary>
    /// Older offerings appear first.
    /// </summary>
    Oldest,

    /// <summary>
    /// Offerings are sorted alphabetically by title.
    /// </summary>
    Title,

    /// <summary>
    /// Offerings follow the owner's manual sort order.
    /// </summary>
    Manual
}
