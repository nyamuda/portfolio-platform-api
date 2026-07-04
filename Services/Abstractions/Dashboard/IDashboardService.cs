using PortfolioPlatform.Api.Dtos.Dashboard;

namespace PortfolioPlatform.Api.Services.Abstractions.Dashboard;

/// <summary>
/// Builds dashboard data for authenticated users.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets a compact dashboard summary for the authenticated user.
    /// </summary>
    /// <param name="userId">Authenticated user id.</param>
    /// <returns>Dashboard summary for the user.</returns>
    Task<DashboardSummaryDto> GetSummaryAsync(int userId);
}
