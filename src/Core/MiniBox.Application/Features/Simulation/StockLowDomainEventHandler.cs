using MediatR;
using Microsoft.Extensions.Logging;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Domain.Events;

namespace MiniBox.Application.Features.Simulation;

public sealed class StockLowDomainEventHandler(ILogger<StockLowDomainEventHandler> logger)
    : INotificationHandler<DomainEventNotification<StockLowDomainEvent>>
{
    public Task Handle(DomainEventNotification<StockLowDomainEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogWarning("Stock is low for product {ProductId}: {RemainingQuantity} units remain.",
            notification.DomainEvent.ProductId, notification.DomainEvent.RemainingQuantity);
        return Task.CompletedTask;
    }
}
