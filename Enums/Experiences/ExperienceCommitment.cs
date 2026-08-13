namespace PortfolioPlatform.Api.Enums.Experiences;

/// <summary>
/// Describes the time or workload commitment behind an experience.
/// </summary>
/// <remarks>
/// Commitment is intentionally narrower than experience type. It captures values such as full-time
/// or part-time without replacing categories like Work, Education, Volunteering, or Certification.
/// </remarks>
public enum ExperienceCommitment
{
    /// <summary>
    /// The commitment level has not been specified.
    /// </summary>
    NotSpecified,

    /// <summary>
    /// A full-time role, study load, or commitment.
    /// </summary>
    FullTime,

    /// <summary>
    /// A part-time role, study load, or commitment.
    /// </summary>
    PartTime,

    /// <summary>
    /// A fixed-term arrangement or contract-style commitment.
    /// </summary>
    Contract,

    /// <summary>
    /// A temporary arrangement with a limited time window.
    /// </summary>
    Temporary,

    /// <summary>
    /// A seasonal arrangement tied to a specific period or cycle.
    /// </summary>
    Seasonal,

    /// <summary>
    /// A casual or occasional commitment.
    /// </summary>
    Casual,

    /// <summary>
    /// A flexible commitment without a fixed schedule or workload.
    /// </summary>
    Flexible
}
