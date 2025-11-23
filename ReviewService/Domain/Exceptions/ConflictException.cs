namespace ReviewService.Domain.Exceptions;

/// <summary>
///бізнес-конфлікт (duplicate)
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }
}