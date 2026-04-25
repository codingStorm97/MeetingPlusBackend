using System.Security.Claims;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AiTodoApp.Api.Services;
using AiTodoApp.Messaging.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiTodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationStreamService _notificationStreamService;

    public NotificationsController(NotificationStreamService notificationStreamService)
    {
        _notificationStreamService = notificationStreamService;
    }

    [HttpGet("ws")]
    public async Task WebSocketStream(CancellationToken cancellationToken)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("WebSocket connection expected.", cancellationToken);
            return;
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var subscription = _notificationStreamService.Subscribe(userId, cancellationToken);

        await foreach (var message in subscription.Reader.ReadAllAsync(cancellationToken))
        {
            switch (message)
            {
                case FileUploadedNotification uploaded:
                    await WriteEventAsync(socket, "fileUploaded", JsonSerializer.Serialize(uploaded), cancellationToken);
                    break;
                case JobStatusChangedMessage changed:
                    await WriteEventAsync(socket, "jobStatusChanged", JsonSerializer.Serialize(changed), cancellationToken);
                    break;
            }
        }
    }

    private static async Task WriteEventAsync(
        WebSocket socket,
        string eventName,
        string data,
        CancellationToken cancellationToken)
    {
        if (socket.State != WebSocketState.Open)
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            eventType = eventName,
            data = JsonSerializer.Deserialize<JsonElement>(data)
        });

        var bytes = Encoding.UTF8.GetBytes(payload);
        await socket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken);
    }
}
