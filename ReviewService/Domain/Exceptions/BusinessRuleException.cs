namespace ReviewService.Domain.Exceptions;

/// <summary>
///порушення бізнес-правил
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}