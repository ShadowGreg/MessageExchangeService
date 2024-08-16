using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MessageExchangeService.Domain;
using MessageExchangeService.infrastructure.Service;
using Microsoft.Extensions.Logging;

namespace MessageExchangeService.infrastructure.Services;

public class WebSocketService(ILogger<WebSocketService> logger) : IWebSocketService
{
    private readonly List<WebSocket> _sockets = new List<WebSocket>();

    public async Task BroadcastMessageAsync(Message message)
    {
        logger.LogInformation("Broadcasting message with SequenceNumber: {SequenceNumber}", message.SequenceNumber);

        var messageJson = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(messageJson);

        foreach (var socket in _sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public void AddSocket(WebSocket socket)
    {
        logger.LogInformation("Adding a new WebSocket connection.");

        _sockets.Add(socket);
    }
}