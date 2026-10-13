using MediatR;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Domain.Events;

namespace MiniBox.API.Services;

public sealed class OrderCreatedDomainEventHandler(ISignalRNotificationService notifications)
    : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<OrderCreatedDomainEvent> notification, CancellationToken cancellationToken) =>
        notifications.NotifyOrderCreatedAsync(notification.DomainEvent.OrderId,
            notification.DomainEvent.ProductNames, cancellationToken);
}
