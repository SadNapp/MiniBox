using Microsoft.AspNetCore.Mvc;
using MediatR;
using MiniBox.Application.Features.Simulation;

namespace MiniBox.API.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("online-users")]
    public async Task<IActionResult> GetOnlineUsers(CancellationToken cancellationToken)
    {
        var count = await mediator.Send(new GetActiveUsersOnlineQuery(), cancellationToken);
        return Ok(new { onlineUsers = count });
    }

}
