using System.ComponentModel.DataAnnotations;
using PortfolioPlatform.Api.Enums.Themes;

namespace PortfolioPlatform.Api.Dtos.Themes;

/// <summary>
/// Values a profile owner may change in the portfolio design screen.
/// </summary>
public class UpdateThemeSelectionDto
{
    /// <summary>Complete portfolio design to use.</summary>
    public PortfolioTheme Theme { get; set; }

    /// <summary>Six-digit hexadecimal accent colour, including the leading hash.</summary>
    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Accent colour must use a six-digit hex value such as #1640D6.")]
    public required string AccentColor { get; set; }

    /// <summary>Typography treatment to use inside the selected theme.</summary>
    public PortfolioTypography Typography { get; set; }
}
