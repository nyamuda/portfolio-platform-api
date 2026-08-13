using PortfolioPlatform.Api.Enums.Themes;

namespace PortfolioPlatform.Api.Dtos.Themes;

/// <summary>
/// The three presentation choices needed to render a public portfolio.
/// </summary>
public class ThemeSelectionDto
{
    /// <summary>Complete portfolio design selected by the profile owner.</summary>
    public PortfolioTheme Theme { get; set; } = PortfolioTheme.Classic;

    /// <summary>Hex colour used for links, buttons, and small theme highlights.</summary>
    public string AccentColor { get; set; } = "#1640D6";

    /// <summary>Typography treatment used by the selected theme.</summary>
    public PortfolioTypography Typography { get; set; } = PortfolioTypography.Sans;
}
