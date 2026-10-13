using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Domain.Entities.Orders;
using MiniBox.Application.Features.Orders.Queries.GetUserOrders;
using CreateOrderUseCase = MiniBox.Application.Features.Orders.Commands.CreateOrder.CreateOrderRequest;

namespace MiniBox.API.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var userId))
            return Unauthorized();
        return Ok(await mediator.Send(new GetUserOrdersQuery(userId), cancellationToken));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest? request, CancellationToken cancellationToken)
    {
        if (request is null || request.Items is null || string.IsNullOrWhiteSpace(request.CustomerName)
            || request.Items.Count == 0 || request.Items.Any(item => item.Quantity < 1))
            return BadRequest(new { message = "Перевірте ім'я, email і склад замовлення." });
        try
        {
            var result = await mediator.Send(new CreateOrderUseCase(request.CustomerName,
                request.Items.Select(item => new CreateOrderLine(item.ProductId, item.Quantity)).ToArray(),
                User.FindFirstValue(ClaimTypes.Email) ?? string.Empty, request.DeliveryStreet, request.DeliveryCity,
                request.DeliveryPostalCode, request.DeliveryCountry ?? "Україна", request.DeliveryNotes), cancellationToken);
            return Created($"/api/orders/{result.OrderId}", new { orderId = result.OrderId, result.OrderNumber, result.TotalPrice, status = OrderStatus.Pending.ToString() });
        }
        catch (KeyNotFoundException exception) { return BadRequest(new { message = exception.Message }); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
    }
}

public sealed record OrderLineRequest(Guid ProductId, int Quantity);
public sealed record CreateOrderRequest(string CustomerName, string Email, IReadOnlyList<OrderLineRequest> Items,
    string DeliveryStreet, string DeliveryCity, string DeliveryPostalCode, string? DeliveryCountry = "Україна", string? DeliveryNotes = null);
