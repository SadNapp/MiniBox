using MiniBox.Domain.Common;

namespace MiniBox.Domain.Events;

public record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string ProductNames,
    DateTime OccurredOnUtc) : IDomainEvent;
