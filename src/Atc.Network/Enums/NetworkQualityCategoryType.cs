// ReSharper disable once CheckNamespace
namespace Atc.Network;

/// <summary>
/// Enumeration: NetworkQualityCategoryType.
/// </summary>
public enum NetworkQualityCategoryType
{
    None,

    /// <summary>
    /// The very poor
    /// </summary>
    [LocalizedDescription(nameof(NetworkQualityCategoryType) + nameof(VeryPoor), typeof(EnumResources))]
    VeryPoor,

    /// <summary>
    /// The poor
    /// </summary>
    [LocalizedDescription(nameof(NetworkQualityCategoryType) + nameof(Poor), typeof(EnumResources))]
    Poor,

    /// <summary>
    /// The fair
    /// </summary>
    [LocalizedDescription(nameof(NetworkQualityCategoryType) + nameof(Fair), typeof(EnumResources))]
    Fair,

    /// <summary>
    /// The good
    /// </summary>
    [LocalizedDescription(nameof(NetworkQualityCategoryType) + nameof(Good), typeof(EnumResources))]
    Good,

    /// <summary>
    /// The very good
    /// </summary>
    [LocalizedDescription(nameof(NetworkQualityCategoryType) + nameof(Good), typeof(EnumResources))]
    VeryGood,

    /// <summary>
    /// The excellent
    /// </summary>
    [LocalizedDescription(nameof(NetworkQualityCategoryType) + nameof(Excellent), typeof(EnumResources))]
    Excellent,

    /// <summary>
    /// The perfect
    /// </summary>
    [LocalizedDescription(nameof(NetworkQualityCategoryType) + nameof(Perfect), typeof(EnumResources))]
    Perfect,
}