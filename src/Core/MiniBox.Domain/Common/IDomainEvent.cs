namespace MiniBox.Domain.Common;

public interface IDomainEvent 
{
    DateTime OccurredOnUtc { get; }
}