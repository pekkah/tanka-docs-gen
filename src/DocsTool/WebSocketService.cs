using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.DocsTool;

public class WebSocketService : IDisposable
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();
    private volatile bool _disposed;

    public async Task Handle(WebSocket webSocket, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;
        
        var socketId = Guid.NewGuid();
        _sockets.TryAdd(socketId, webSocket);

        try
        {
            // Keep the connection open until the client closes it or cancellation is requested
            var buffer = new Memory<byte>(new byte[1024 * 4]);
            ValueWebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            }
            while (result.MessageType != WebSocketMessageType.Close && !cancellationToken.IsCancellationRequested);

            if (!cancellationToken.IsCancellationRequested)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected during shutdown
        }
        catch (WebSocketException)
        {
            // Client disconnected unexpectedly, ignore
        }
        finally
        {
            _sockets.TryRemove(socketId, out _);
        }
    }

    public async Task SendReload(CancellationToken cancellationToken = default)
    {
        if (_disposed || cancellationToken.IsCancellationRequested) return;
        
        var message = Encoding.UTF8.GetBytes("reload");
        var socketsToRemove = new List<Guid>();
        
        foreach (var kvp in _sockets)
        {
            var socketId = kvp.Key;
            var socket = kvp.Value;
            
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ReadOnlyMemory<byte>(message), WebSocketMessageType.Text, true, cancellationToken);
                }
                else
                {
                    socketsToRemove.Add(socketId);
                }
            }
            catch (WebSocketException)
            {
                // Socket is no longer valid, mark for removal
                socketsToRemove.Add(socketId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
        
        // Clean up invalid sockets
        foreach (var socketId in socketsToRemove)
        {
            _sockets.TryRemove(socketId, out _);
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        // Close all remaining WebSocket connections
        foreach (var socket in _sockets.Values)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None)
                        .GetAwaiter().GetResult();
                }
                socket.Dispose();
            }
            catch
            {
                // Ignore exceptions during cleanup
            }
        }
        
        _sockets.Clear();
    }
} 