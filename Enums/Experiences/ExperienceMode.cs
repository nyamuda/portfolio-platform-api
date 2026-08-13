namespace PortfolioPlatform.Api.Enums.Experiences;

/// <summary>
/// Describes where or how an experience took place.
/// </summary>
/// <remarks>
/// Mode is about delivery or setting, not the type of experience. For example, a Work experience
/// can be Remote, InPerson, Hybrid, or OnSite.
/// </remarks>
public enum ExperienceMode
{
    /// <summary>
    /// The mode has not been specified.
    /// </summary>
    NotSpecified,

    /// <summary>
    /// The experience happened fully remotely.
    /// </summary>
    Remote,

    /// <summary>
    /// The experience happened physically in person.
    /// </summary>
    InPerson,

    /// <summary>
    /// The experience mixed remote and in-person work.
    /// </summary>
    Hybrid,

    /// <summary>
    /// The experience was delivered online, such as online tutoring, webinars, or virtual training.
    /// </summary>
    Online,

    /// <summary>
    /// The experience happened at a workplace, school, client site, venue, or other specific place.
    /// </summary>
    OnSite
}
