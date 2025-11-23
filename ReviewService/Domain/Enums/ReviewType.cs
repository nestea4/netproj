namespace ReviewService.Domain.Enums;

/// <summary>
///тип відгуку
/// </summary>
public enum ReviewType
{
    Standard,
    
    Detailed,

    /// <summary>
    ///швидкий відгук (короткий)
    /// </summary>
    Quick
}