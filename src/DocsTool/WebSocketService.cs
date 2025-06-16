using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.DocsTool;

public class WebSocketService
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();

    public async Task Handle(WebSocket webSocket)
    {
        var socketId = Guid.NewGuid();
        _sockets.TryAdd(socketId, webSocket);

        try
        {
            // Keep the connection open until the client closes it
            var buffer = new Memory<byte>(new byte[1024 * 4]);
            ValueWebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            }
            while (result.MessageType != WebSocketMessageType.Close);

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
        finally
        {
            _sockets.TryRemove(socketId, out _);
        }
    }

    public async Task SendReload()
    {
        var message = Encoding.UTF8.GetBytes("reload");
        foreach (var socket in _sockets.Values)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ReadOnlyMemory<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
} 