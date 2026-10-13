using MiniBox.Domain.Common;

namespace MiniBox.Domain.Events;

public record StockLowDomainEvent(
    Guid ProductId,
    int RemainingQuantity,
    DateTime OccurredOnUtc) : IDomainEvent;